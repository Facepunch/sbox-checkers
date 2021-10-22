using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
    partial class CheckersGame
	{

		[Event( CheckersEvents.GameStateChanged )]
		private void OnGameStateChanged( GameState oldState, GameState newState )
		{
			Log.Info( "CLIENT: " + IsClient );
			Log.Info( "CHANGED: " + oldState + " to " + newState );
		}

	}
}
