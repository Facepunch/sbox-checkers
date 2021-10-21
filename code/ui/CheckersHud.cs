using Sandbox;
using Sandbox.UI;

namespace Facepunch.Checkers
{
	public partial class CheckersHudEntity : Sandbox.HudEntity<RootPanel>
	{

		public CheckersHudEntity()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/ui/CheckersHud.html" );
				RootPanel.Style.PointerEvents = "visible";
			}
		}

	}
}
