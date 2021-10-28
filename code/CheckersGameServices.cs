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

		[Event( CheckersEvents.ServerPieceEliminated )]
		private void OnPieceEliminated( CheckersPiece piece )
		{
			Log.Info( "ELIM: " + Entity.IsServer + ":" + piece.Team );
		}

		[Event( CheckersEvents.ServerVictory )]
		private void OnVictory( CheckersPlayer winner, CheckersPlayer loser )
		{
			Log.Info( "WIN: " + winner.Client.Name );
		}

	}
}
