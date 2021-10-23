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

		public float TurnTimer { get; set; }

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
			TurnTimer = CheckersGame.Instance.TurnTimer;
		}

	}
}
