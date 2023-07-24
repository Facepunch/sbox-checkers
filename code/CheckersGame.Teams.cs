
namespace Facepunch.Checkers;

partial class CheckersGame
{

	// todo: implement teams via lobby and avoid all this

	public CheckersPlayer RedPlayer => Entity.All.FirstOrDefault( x => x is CheckersPlayer pl
		 && pl.IsValid
		 && pl.Team == CheckersTeam.Red ) as CheckersPlayer;

	public CheckersPlayer BlackPlayer => Entity.All.FirstOrDefault( x => x is CheckersPlayer pl
		 && pl.IsValid
		 && pl.Team == CheckersTeam.Black ) as CheckersPlayer;

	[ConCmd.Server]
	public static void SetClientTeam( CheckersTeam team )
	{
		var player = ConsoleSystem.Caller.Pawn as CheckersPlayer;

		if ( player.Team == team )
		{
			return;
		}

		var teamTaken = Entity.All.FirstOrDefault( x => x is CheckersPlayer pl
			 && pl.IsValid
			 && pl.Team == team ) != null;

		if ( teamTaken )
		{
			return;
		}

		player.Team = team;
	}

	[ConCmd.Server]
	public static void PlayAgainstAi()
	{
		foreach ( var pl in Entity.All )
		{
			if ( pl is not CheckersPlayer cpl )
			{
				continue;
			}
			cpl.Team = CheckersTeam.Spectator;
		}

		var player = ConsoleSystem.Caller.Pawn as CheckersPlayer;
		player.Team = CheckersTeam.Red;

		foreach ( var cl in Game.Clients )
		{
			if ( cl.IsBot )
			{
				cl.Kick();
			}
		}

		var bot = new CheckersBot();
		bot.Client.Pawn.Delete();
		var botPlayer = new CheckersPlayer();
		botPlayer.Team = CheckersTeam.Black;
		bot.Client.Pawn = botPlayer;
	}

}
