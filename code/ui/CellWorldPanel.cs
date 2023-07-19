
using Sandbox;
using Sandbox.UI;
using System.Linq;

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

		[GameEvent.Client.Frame]
		private void OnFrame()
		{
			if ( Game.LocalPawn is not CheckersPlayer pl )
			{
				return;
			}

			var hasMove = pl.LegalMoveCache.Any( x => x.Cell == _cell );
			var hasJump = pl.LegalMoveCache.Any( x => x.Cell == _cell && x.Jump.IsValid() );

			SetClass( "hovered", _cell.Hovered );
			SetClass( "legalmove", hasMove );
			SetClass( "jump", hasJump );
		}

	}
}
