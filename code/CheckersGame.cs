
namespace Facepunch.Checkers;

[Library( "checkers", Title = "Checkers" )]
partial class CheckersGame : GameManager
{

	public static CheckersGame Instance;

	public CheckersGame()
	{
		Instance = this;

		if ( Game.IsServer )
		{
			SetGameState( GameState.WaitingToStart );

			// is this suppose to be done internally?
			try
			{
				var dict = Json.Deserialize<Dictionary<string, string>>( ConsoleSystem.GetValue( "gamesettings" ) );
				foreach ( var kvp in dict )
				{
					ConsoleSystem.SetValue( kvp.Key, kvp.Value );
				}
			}
			catch { Log.Error( "Failed to load game settings" ); }
		}

		if ( Game.IsClient )
		{
			new CheckersHud();
		}
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		var player = new CheckersPlayer();
		cl.Pawn = player;

		player.Team = CheckersTeam.Spectator;
		player.Dress( cl );
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
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

	[GameEvent.Entity.PostSpawn]
	private void CreateGrid()
	{
		if ( Game.IsClient )
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

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( Game.LocalPawn is not CheckersPlayer player )
			return;

		var targetCam = player.Team != CheckersTeam.Red ? "camera_black" : "camera_red";

		// todo: maybe calculate the ideal camera position using the board's bounds
		var cam = Entity.FindByName( targetCam );
		if ( cam == null ) return; // map isn't set up for this game

		Camera.Position = cam.Position;
		Camera.Rotation = cam.Rotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 55 );
	}

	[ConCmd.Server("mequit")]
	public static void RequestQuit()
	{
		if ( !ConsoleSystem.Caller.IsValid() ) 
			return;

		ConsoleSystem.Caller.Kick();
	}

}
