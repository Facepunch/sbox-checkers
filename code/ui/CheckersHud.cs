using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.Checkers
{
	public partial class CheckersHud : HudEntity<RootPanel>
	{

		public CheckersHud()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/ui/checkershud.html" );
				RootPanel.Style.PointerEvents = "all";
			}
		}

	}
}
