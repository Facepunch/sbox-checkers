using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.Checkers
{
	[Library( "checkers", Title = "Checkers" )]
	partial class CheckersGame : Sandbox.Game
	{

		public static CheckersGame Instance;

		public CheckersGame()
		{
			Instance = this;
			
			if ( IsServer )
			{
				new CheckersHudEntity();

				SetGameState( GameState.WaitingToStart );
			}
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new CheckersPlayer();
			cl.Pawn = player;

			player.Team = CheckersTeam.Spectator;
			player.Respawn();
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			// Make a ray of where the local pawn is looking
			var ray = Input.Cursor;

			// Emulate the mouse buttons with any inputs we want
			var leftMouseDown = Input.Down( InputButton.Attack1 );
			var rightMouseDown = Input.Down( InputButton.Attack2 );
			var scroll = new Vector2 { x = Input.MouseWheel }; // ( only x is used )

			WorldInput.Update( ray, leftMouseDown, rightMouseDown, scroll );
		}

		[Event.Entity.PostSpawn]
		private void CreateGrid()
		{
			if ( IsClient )
			{
				return;
			}

			var gridTrigger = Entity.All.FirstOrDefault( x => x is BoardTrigger );
			if ( gridTrigger == null )
			{
				// map is missing grid trigger, do something
				return;
			}

			var gridEnt = Create<CheckersBoard>();
			gridEnt.Mins = gridTrigger.WorldSpaceBounds.Mins;
			gridEnt.Maxs = gridTrigger.WorldSpaceBounds.Maxs.WithZ( gridEnt.Mins.z );
			gridEnt.SpawnCells();
		}

	}
}
