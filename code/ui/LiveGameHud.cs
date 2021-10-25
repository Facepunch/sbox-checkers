using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	class LiveGameHud : Panel
	{

		public float TurnTimer => (int)CheckersGame.Instance.TurnTimer;
		public CheckersTeam ActiveTeam => CheckersGame.Instance.ActiveTeam;

		public LiveGameHud()
		{
			SetTemplate( "/ui/livegamehud.html" );
		}

		[Event.Frame]
		private void OnFrame()
		{
			if ( CheckersGame.Instance.CurrentState != GameState.Live )
			{
				Style.Display = DisplayMode.None;
				return;
			}
			Style.Display = DisplayMode.Flex;
		}

	}
}
