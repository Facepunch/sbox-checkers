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
