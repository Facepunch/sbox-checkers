using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.Checkers
{
	[Library( "checkers", Title = "Checkers" )]
	partial class CheckersGame : GameManager
	{

		public static CheckersGame Instance;

		public CheckersGame()
		{
			Instance = this;

			if ( IsServer )
			{
				SetGameState( GameState.WaitingToStart );
			}

            if (IsClient)
            {
				new CheckersHud();
            }
		}

		public override void Simulate(IClient cl)
		{
			base.Simulate(cl);

			if (Input.Pressed(InputButton.Jump))
			{
				DeclareWinner(CheckersTeam.Red);
			}
		}

		public override void ClientJoined(IClient cl )
		{
			base.ClientJoined( cl );

			var player = new CheckersPlayer();
			cl.Pawn = player;

			player.Team = CheckersTeam.Spectator;
			player.Respawn();
		}

		public override void ClientDisconnect(IClient cl, NetworkDisconnectionReason reason )
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

        public override void FrameSimulate(IClient cl)
        {
            base.FrameSimulate(cl);

            if ( Game.LocalPawn is not CheckersPlayer player )
                return;

            var targetCam = player.Team != CheckersTeam.Red ? "camera_black" : "camera_red";

            // todo: maybe calculate the ideal camera position using the board's bounds
            var cam = Entity.FindByName(targetCam);
            Camera.Position = cam.Position;
            Camera.Rotation = cam.Rotation;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 55 );
        }

    }
}
