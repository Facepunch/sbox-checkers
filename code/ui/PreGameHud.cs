using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class PreGameHud : Panel
	{

		public string Player1Name { get; set; }
		public string Player2Name { get; set; }

		public PreGameHud()
		{
			SetTemplate( "/ui/PreGameHud.html" );
		}

		public void ChooseTeam1()
		{
			CheckersGame.SetClientTeam( CheckersTeam.Red );
		}

		public void ChooseTeam2()
		{
			CheckersGame.SetClientTeam( CheckersTeam.Black );
		}

		public void ChooseSpectator()
		{
			CheckersGame.SetClientTeam( CheckersTeam.Spectator );
		}

		public void PlayAgainstAi()
		{
			CheckersGame.PlayAgainstAi();
		}

		[Event.Frame]
		private void OnFrame()
		{
			if ( CheckersGame.Instance.CurrentState != GameState.WaitingToStart )
			{
				Style.Display = DisplayMode.None;
				return;
			}

			Style.Display = DisplayMode.Flex;

			Player1Name = CheckersGame.Instance.Team1 != null
				? CheckersGame.Instance.Team1.Client.Name
				: "Nobody";

			Player2Name = CheckersGame.Instance.Team2 != null
				? CheckersGame.Instance.Team2.Client.Name
				: "Nobody";
		}

	}
}
