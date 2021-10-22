using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersPlayer : Sandbox.Player
	{

		[Net]
		public CheckersTeam Team { get; set; } = CheckersTeam.Spectator;

		private CheckersCell _hoveredCell;

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Camera = new CheckersCamera();
			Animator = new StandardPlayerAnimator();
			Controller = new CheckersController();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			// todo: map spawn points
			Position = Vector3.Up * 400;
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( !IsClient )
			{
				return;
			}

			var tr = Trace.Ray( Input.Cursor, 3000 )
				.WorldOnly()
				.Run();

			if ( tr.Hit )
			{
				var board = Entity.All.FirstOrDefault( x => x is CheckersBoard ) as CheckersBoard;
				SetHoveredCell( board.GetCellAt( tr.EndPos ) );
			}
		}

		private void SetHoveredCell( CheckersCell cell )
		{
			if ( _hoveredCell != null )
			{
				_hoveredCell.Hovered = false;
			}

			_hoveredCell = cell;

			if ( cell != null )
			{
				cell.Hovered = true;
			}
		}

	}
}
