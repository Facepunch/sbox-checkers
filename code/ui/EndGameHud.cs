using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	class EndGameHud : Panel
	{

		public string GameResult => GetGameResultString();
		public string Timer => GetTimerString();

		public EndGameHud()
		{
			SetTemplate( "/ui/endgamehud.html" );
		}

		[Event.Frame]
		private void OnFrame()
		{
			if ( CheckersGame.Instance.CurrentState != GameState.Completed
				&& CheckersGame.Instance.CurrentState != GameState.Abandoned)
			{
				Style.Opacity = 0;
				return;
			}

			Style.Opacity = 1;
		}

		private string GetTimerString()
		{
			return "Restarting in " + (int)CheckersGame.Instance.EndGameTimer + " seconds";
		}

		private string GetGameResultString()
		{
			var game = CheckersGame.Instance;
			if ( game.CurrentState == GameState.Abandoned )
			{
				return "Game abandoned, scores will not count";
			}

			if ( game.CurrentState != GameState.Completed || !game.Winner.IsValid() )
			{
				return "unknown";
			}

			return game.Winner.Client.Name + " has won the game!";
		}

	}
}
