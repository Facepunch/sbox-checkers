using Sandbox;
using Sandbox.UI;

namespace Facepunch.Checkers
{
	partial class HudEntity : HudEntity<RootPanel>
	{

		public HudEntity()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "ui/hud.html" );
			}
		}

	}
}
