using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	class CheckersGameServices : EntityComponent<CheckersGame>
	{

		[Event( CheckersEvents.GameStateChanged )]
		private void OnGameStateChanged( GameState newState )
		{
			if ( Entity.IsClient )
			{
				return;
			}

			switch ( newState )
			{
				case GameState.Live:
					GameServices.StartGame();
					break;
				case GameState.Abandoned:
					GameServices.AbandonGame(); // todo: pass player who abandoned the game 
					break;
			}
		}

		[Event( CheckersEvents.ServerPieceEliminated )]
		private void OnPieceEliminated( CheckersPiece piece )
		{
			var victim = piece.Team == CheckersTeam.Red 
				? Entity.RedPlayer 
				: Entity.BlackPlayer;

			var attacker = victim == Entity.RedPlayer 
				? Entity.BlackPlayer 
				: Entity.RedPlayer;

			GameServices.RecordEvent( victim.Client, $"Lost {piece.Team} Chip" );
			GameServices.RecordEvent( attacker.Client, $"Eliminated {piece.Team} Chip" );
		}

		[Event( CheckersEvents.ServerMatchCompleted )]
		private void OnMatchCompleted( CheckersPlayer winner, CheckersPlayer loser )
		{
			winner.Client.SetGameResult( GameplayResult.Win, 0 );
			loser.Client.SetGameResult( GameplayResult.Lose, 0 );

			GameServices.EndGame();
		}

	}
}
