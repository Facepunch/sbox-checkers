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
		[Net, Predicted]
		private CheckersPiece SelectedPiece { get; set; }
		[Net, Predicted]
		private CheckersCell HoveredCell { get; set; }

		private Clothing.Container _clothing = new();

		public List<CheckersMove> LegalMoveCache = new List<CheckersMove>();

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

			var tr = Trace.Ray( Input.Cursor, 3000 )
				.WorldOnly()
				.Run();

			if ( tr.Hit )
			{
				var board = Entity.All.FirstOrDefault( x => x is CheckersBoard ) as CheckersBoard;
				SetHoveredCell( board.GetCellAt( tr.EndPos ) );
			}

			if ( !HoveredCell.IsValid() )
			{
				return;
			}

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				var piece = Entity.All.FirstOrDefault( x => x is CheckersPiece p 
					&& p.BoardPosition == HoveredCell.BoardPosition
					&& p.Team == Team ) as CheckersPiece;

				SetSelectedPiece( piece );
			}

			if ( !SelectedPiece.IsValid() )
			{
				return;
			}

			SelectedPiece.DragPosition = tr.Hit ? tr.EndPos : Vector3.Zero;

			if ( Input.Released( InputButton.Attack1 ) )
			{
				if ( !IsClient )
				{
					CheckersGame.Instance.AttemptMove( this, SelectedPiece, HoveredCell.BoardPosition );
				}

				SetSelectedPiece( null );
			}
		}

		private void SetSelectedPiece( CheckersPiece piece )
		{
			if ( SelectedPiece.IsValid() )
			{
				SelectedPiece.Floating = false;
				SelectedPiece.DragPosition = Vector3.Zero;
			}

			SelectedPiece = piece;

			if ( piece.IsValid() )
			{
				piece.Floating = true;
			}
		}

		private void SetHoveredCell( CheckersCell cell )
		{
			LegalMoveCache.Clear();

			if ( HoveredCell != null )
			{
				HoveredCell.Hovered = false;
			}

			HoveredCell = cell;

			if ( cell != null )
			{
				cell.Hovered = true;

				var piece = (cell.Parent as CheckersBoard).GetPieceAt( cell.BoardPosition );
				if ( piece.IsValid() && piece.Team == Team )
				{
					LegalMoveCache.AddRange( piece.GetLegalMoves() );
				}
			}
		}

	}
}
