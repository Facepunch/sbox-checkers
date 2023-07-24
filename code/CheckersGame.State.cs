
using Sandbox;

namespace Facepunch.Checkers;

partial class CheckersGame
{

	[ConVar.Replicated( name: "player1red" )]
	public static string Player1Red { get; set; }
	[ConVar.Replicated( name: "player2black" )]
	public static string Player2Black { get; set; }

	[Net, Change( nameof( ClientGameStateChanged ) )]
	public GameState CurrentState { get; set; }
	[Net]
	public CheckersTeam ActiveTeam { get; set; } // the team that needs to make a move
	[Net]
	public float TurnTimer { get; set; }
	[Net]
	public float EndGameTimer { get; set; }
	[Net]
	public CheckersPlayer Winner { get; set; }
	[Net]
	public CheckersPlayer Loser { get; set; }

	public const float PlayerTurnTime = 30;
	public const float EndGameTime = 8;

	[GameEvent.Tick]
	private void OnTick()
	{
		if ( Game.IsClient )
		{
			return;
		}

		switch ( CurrentState )
		{
			case GameState.WaitingToStart:
				TickWaitingToStart();
				break;
			case GameState.Live:
				TickLive();
				break;
			case GameState.Completed:
			case GameState.Abandoned:
				TickGameEnd();
				break;
		}
	}

	private void TickGameEnd()
	{
		EndGameTimer -= Time.Delta;

		if ( EndGameTimer <= 0 )
		{
			RestartGame();
		}
	}

	private void TickLive()
	{
		TurnTimer -= Time.Delta;
		if ( TurnTimer <= 0 )
		{
			EndTurn();
		}
	}

	CheckersPlayer EnsureAI ( CheckersTeam team )
	{
		var botClient = Game.Clients.FirstOrDefault( x => x.IsBot && x.Pawn is CheckersPlayer pl && pl.Team == team );
		if ( botClient != null ) return botClient.Pawn as CheckersPlayer;

		var bot = new CheckersBot();
		bot.Client.Pawn?.Delete();
		var botPlayer = new CheckersPlayer();
		botPlayer.Team = team;
		bot.Client.Pawn = botPlayer;

		return botPlayer;
	}

	public bool IsWaitingFor( long steamid )
	{
		if ( Player1Red != steamid.ToString() && Player2Black != steamid.ToString() ) return false;

		var player = Entity.All.OfType<CheckersPlayer>().FirstOrDefault( x => x.Client?.SteamId.ToString() == Player1Red );

		return !player.IsValid();
	}

	private void TickWaitingToStart()
	{
		var players = Entity.All.OfType<CheckersPlayer>();

		var player1 = players.FirstOrDefault( x => x.Client?.SteamId.ToString() == Player1Red );
		var player2 = players.FirstOrDefault( x => x.Client?.SteamId.ToString() == Player2Black );

		if ( Player1Red == "ai" )
		{
			player1 = EnsureAI( CheckersTeam.Red );
		}

		if ( Player2Black == "ai" )
		{
			player2 = EnsureAI( CheckersTeam.Black );
		}

		if ( player1.IsValid() )
		{
			player1.Team = CheckersTeam.Red;
		}

		if ( player2.IsValid() )
		{
			player2.Team = CheckersTeam.Black;
		}

		if ( player1.IsValid() && player2.IsValid() )
		{
			TurnTimer = PlayerTurnTime;
			ActiveTeam = CheckersTeam.Black;
			SetGameState( GameState.Live );
		}
	}

	private void SetGameState( GameState newState )
	{
		CurrentState = newState;

		Event.Run( CheckersEvents.GameStateChanged, newState );
	}

	public void AttemptMove( CheckersPlayer player, CheckersPiece piece, Vector2 target )
	{
		Game.AssertServer();

		if ( CurrentState != GameState.Live )
		{
			return;
		}

		if ( player.Team != ActiveTeam || player.Team != piece.Team )
		{
			return;
		}

		var moves = piece.GetLegalMoves();
		var attemptedMove = moves.FirstOrDefault( x => x.Cell.BoardPosition == target );
		if ( attemptedMove == null )
		{
			// move is invalid! pour some juice
			return;
		}

		if ( attemptedMove.Jump == null )
		{
			if ( CheckersBoard.Current.TeamCanJump( player.Team ) )
			{
				// this player has a mandatory jump
				return;
			}
		}
		else
		{
			EliminatePiece( attemptedMove.Jump );
		}

		piece.MoveToPosition( target );

		if ( attemptedMove.Jump != null && piece.GetLegalMoves().Any( x => x.Jump.IsValid() ) )
		{
			// this piece can jump again
			return;
		}

		EndTurn();
	}

	private void EliminatePiece( CheckersPiece piece )
	{
		Game.AssertServer();

		piece.Delete();

		Event.Run( CheckersEvents.ServerPieceEliminated, piece );
	}

	private void EndTurn()
	{
		ActiveTeam = ActiveTeam == CheckersTeam.Black
			? CheckersTeam.Red
			: CheckersTeam.Black;
		TurnTimer = PlayerTurnTime;

		CheckWinConditions();
	}

	private void CheckWinConditions()
	{
		if ( !HasMoves( CheckersTeam.Red ) )
		{
			DeclareWinner( CheckersTeam.Black );
		}

		if ( !HasMoves( CheckersTeam.Black ) )
		{
			DeclareWinner( CheckersTeam.Red );
		}
	}

	private bool HasMoves( CheckersTeam t )
	{
		var pieces = t == CheckersTeam.Red
			? CheckersBoard.Current.RedPieces
			: CheckersBoard.Current.BlackPieces;

		foreach ( var ent in pieces )
		{
			var piece = ent as CheckersPiece;
			if ( piece.GetLegalMoves().Count > 0 )
			{
				return true;
			}
		}

		return false;
	}

	private void DeclareWinner( CheckersTeam team )
	{
		if ( team == CheckersTeam.Red )
		{
			Winner = RedPlayer;
			Loser = BlackPlayer;
		}
		else
		{
			Winner = BlackPlayer;
			Loser = RedPlayer;
		}

		Event.Run( CheckersEvents.ServerMatchCompleted );

		// todo: glorious celebration
		SetGameState( GameState.Completed );

		EndGameTimer = EndGameTime;
	}

	private void AbandonGame()
	{
		EndGameTimer = EndGameTime;

		SetGameState( GameState.Abandoned );
	}

	private void ClientGameStateChanged()
	{
		Sound.FromScreen( "game-restart" );

		Event.Run( CheckersEvents.GameStateChanged, CurrentState );
	}

	private void RestartGame()
	{
		Winner = null;
		Loser = null;

		for ( int i = Bot.All.Count - 1; i >= 0; i-- )
		{
			Bot.All[i].Client.Kick();
		}

		foreach ( var player in Entity.All )
		{
			if ( player is CheckersPlayer pl && pl.IsValid() )
			{
				pl.Team = CheckersTeam.Spectator;
			}
		}

		CheckersBoard.Current.SpawnCells();
		SetGameState( GameState.WaitingToStart );
	}

	[ConCmd.Server]
	public static void NetworkRestart()
	{
		CheckersGame.Instance.RestartGame();
	}

}
