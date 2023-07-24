
namespace Facepunch.Checkers;

internal partial class CheckersGame
{

	[Event( CheckersEvents.GameStateChanged )]
	private void OnGameStateChanged( GameState newState )
	{
		if ( Game.IsClient )
		{
			return;
		}

		switch ( newState )
		{
			case GameState.Live:
				//Global.Service.StartGame();
				break;
			case GameState.Abandoned:
				// Global.Service.AbandonGame(); // todo: pass player who abandoned the game 
				break;
		}
	}

	[Event( CheckersEvents.ServerPieceEliminated )]
	private void OnPieceEliminated( CheckersPiece piece )
	{
		if ( CurrentState != GameState.Live )
		{
			return;
		}

		var victim = piece.Team == CheckersTeam.Red
			? RedPlayer
			: BlackPlayer;

		var attacker = victim == RedPlayer
			? BlackPlayer
			: RedPlayer;

		// Global.Service.RecordEvent(victim.Client, $"Lost {piece.Team} Chip");
		// Global.Service.RecordEvent(attacker.Client, $"Eliminated {piece.Team} Chip");
	}

	[Event( CheckersEvents.ServerMatchCompleted )]
	private void OnMatchCompleted()
	{
		//if (Winner.Client.IsBot || Loser.Client.IsBot)
		//	return;

		//Winner.Client.SetGameResult(GameplayResult.Win, 0);
		//Loser.Client.SetGameResult(GameplayResult.Lose, 0);

		//  Global.Service.EndGame();
	}

}
