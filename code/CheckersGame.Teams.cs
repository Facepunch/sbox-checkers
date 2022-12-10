using Sandbox;
using Sandbox.Checkers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersGame
	{

		// todo: implement teams via lobby and avoid all this

		public CheckersPlayer RedPlayer => Player.All.FirstOrDefault( x => x is CheckersPlayer pl
			 && pl.IsValid
			 && pl.Team == CheckersTeam.Red ) as CheckersPlayer;

		public CheckersPlayer BlackPlayer => Player.All.FirstOrDefault( x => x is CheckersPlayer pl
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

			var teamTaken = Player.All.FirstOrDefault( x => x is CheckersPlayer pl
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
			foreach ( var pl in Player.All )
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
			botPlayer.Respawn();
			botPlayer.Team = CheckersTeam.Black;
			bot.Client.Pawn = botPlayer;
		}

	}
}
