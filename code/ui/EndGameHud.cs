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

		public EndGameHud()
		{
			SetTemplate( "/ui/endgamehud.html" );
		}

		[Event.Frame]
		private void OnFrame()
		{
			if ( CheckersGame.Instance.CurrentState != GameState.Completed
				&& CheckersGame.Instance.CurrentState != GameState.Abandoned )
			{
				Style.Display = DisplayMode.None;
				return;
			}

			Style.Display = DisplayMode.Flex;
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

			return "Winner: " + game.Winner.Client.Name;
		}

	}
}
