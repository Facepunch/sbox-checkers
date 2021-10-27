using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	public partial class DevHud : RootPanel
	{

		public DevHud()
		{
			SetTemplate( "/ui/devhud.html" );
		}

		public void RestartGame()
		{
			CheckersGame.NetworkRestart();
		}

	}
}
