using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	class CellWorldPanel : WorldPanel
	{

		private CheckersCell _cell;

		public CellWorldPanel( CheckersCell cell )
		{
			_cell = cell;
			UpdateSizeAndPosition();
			Add.Panel( "border" );
			StyleSheet.Load( "/ui/cellworldpanel.scss" );
		}

		private void UpdateSizeAndPosition()
		{
			var sz = (_cell.Maxs.x - _cell.Mins.x) * 18;
			MaxInteractionDistance = 5000f;
			PanelBounds = new Rect( -sz * .5f, -sz * .5f, sz, sz );
			Position = _cell.Center + Vector3.Up * 5;
			Rotation = Rotation.LookAt( Vector3.Up );
		}

		[Event.Frame]
		private void OnFrame()
		{
			if ( Local.Pawn is not CheckersPlayer pl )
			{
				return;
			}

			SetClass( "legalmove", pl.LegalMoveCache.FirstOrDefault( x => x.Cell == _cell ) != null );
		}

	}
}
