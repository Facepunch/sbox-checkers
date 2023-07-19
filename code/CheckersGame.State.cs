using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Diagnostics;

namespace Facepunch.Checkers
{
	partial class CheckersGame
	{

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

		private void TickWaitingToStart()
		{
			var player1 = Entity.All.FirstOrDefault( x => x is CheckersPlayer pl
				&& pl.Team == CheckersTeam.Red
				&& pl.IsValid );
			var player2 = Entity.All.FirstOrDefault( x => x is CheckersPlayer pl
				&& pl.Team == CheckersTeam.Black
				&& pl.IsValid );

			if ( player1 == null || player2 == null )
			{
				return;
			}

			TurnTimer = PlayerTurnTime;
			ActiveTeam = CheckersTeam.Black;

			SetGameState( GameState.Live );
		}

		private void SetGameState( GameState newState )
		{
			CurrentState = newState;

			Event.Run( CheckersEvents.GameStateChanged, newState );
		}

		public void AttemptMove( CheckersPlayer player, CheckersPiece piece, Vector2 target )
		{
			Assert.True( Game.IsServer );

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

			if ( attemptedMove.Jump != null
				&& piece.GetLegalMoves().Any( x => x.Jump.IsValid() ) )
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
			Log.Error("Restarting");

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
}
