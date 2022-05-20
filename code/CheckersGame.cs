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
				SetGameState( GameState.WaitingToStart );

				Components.Add( new CheckersGameServices() );

				new CheckersHud();
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

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );

			if ( CurrentState != GameState.Live
				|| cl.Pawn is not CheckersPlayer pl )
			{
				return;
			}

			if ( pl == RedPlayer || pl == BlackPlayer )
			{
				AbandonGame();
			}
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

			var gridEnt = new CheckersBoard();
			gridEnt.Mins = gridTrigger.WorldSpaceBounds.Mins;
			gridEnt.Maxs = gridTrigger.WorldSpaceBounds.Maxs.WithZ( gridEnt.Mins.z );
			gridEnt.SpawnCells();
		}

	}
}
