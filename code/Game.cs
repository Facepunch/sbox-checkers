using Sandbox;

namespace Facepunch.Checkers
{
	[Library( "checkers", Title = "Checkers" )]
	partial class Game : Sandbox.Game
	{

		public Game()
		{
			if ( IsServer )
			{
				new HudEntity();
				new GameStateEntity();
			}
		}

	}
}
