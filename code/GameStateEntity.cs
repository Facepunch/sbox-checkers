using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
    partial class GameStateEntity : Entity
	{

		[Net]
		public GameState State { get; set; }
		[Net]
		public Player ActivePlayer { get; set; } // the player who needs to make a move
		[Net]
		public float Timer { get; set; }

		private const float TurnTime = 30;
		private const float StartDelay = 5;

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( IsClient )
			{
				return;
			}

			switch ( State )
			{
				case GameState.WaitingToStart:
					CheckCanStart();
					break;
				case GameState.Starting:
					Timer -= Time.Delta;
					if(Timer <= 0 )
					{
						StartGame();
					}
					break;
			}
		}

		private void CheckCanStart()
		{

		}

		private void StartGame()
		{

		}

	}
}
