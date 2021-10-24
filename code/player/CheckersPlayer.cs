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
		[Net]
		public bool ReadyToStart { get; set; }

		private CheckersCell _hoveredCell;
		private CheckersPiece _selectedPiece;
		private Clothing.Container _clothing = new();

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

			if ( Client != null )
			{
				_clothing.LoadFromClient( Client );
				_clothing.DressEntity( this );
			}

			base.Respawn();
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

			SimulatePieceControl();
		}

		private void SimulatePieceControl()
		{
			if ( _hoveredCell == null )
			{
				return;
			}

			if ( !Input.Pressed( InputButton.Attack1 ) )
			{
				return;
			}

			if ( _selectedPiece == null )
			{
				var piece = Entity.All.FirstOrDefault( x => x is CheckersPiece p && p.BoardPosition == _hoveredCell.BoardPosition ) as CheckersPiece;

				if ( piece == null )
				{
					return;
				}
				_selectedPiece = piece;
			}
			else
			{
				CheckersPiece.NetworkMove( _selectedPiece.NetworkIdent, _hoveredCell.BoardPosition );
				_selectedPiece = null;
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
