using Sandbox;
using System.Linq;

namespace Facepunch.Checkers
{
	[Library( "checkers", Title = "Checkers" )]
	partial class Game : Sandbox.Game
	{

		public Game()
		{
			if ( IsServer )
			{
				new CheckersHudEntity();
				new GameStateEntity();
			}
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new CheckersPlayer();
			player.Team = CheckersTeam.Spectator;
			cl.Pawn = player;
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
