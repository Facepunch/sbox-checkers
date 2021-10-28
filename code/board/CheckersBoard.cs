using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersBoard : Entity
	{

		public static CheckersBoard Current;

		[Net]
		public Vector3 Mins { get; set; }
		[Net]
		public Vector3 Maxs { get; set; }

		public IEnumerable<CheckersPiece> Pieces => Children.Where( x => x.IsValid() && x is CheckersPiece ).Cast<CheckersPiece>();
		public IEnumerable<CheckersPiece> RedPieces => Pieces.Where( x => x.Team == CheckersTeam.Red );
		public IEnumerable<CheckersPiece> BlackPieces => Pieces.Where( x => x.Team == CheckersTeam.Black );

		public float CellSize => (Maxs.x - Mins.x) / 8;

		public CheckersBoard()
		{
			Current = this;

			Transmit = TransmitType.Always;
		}

		public void SpawnCells()
		{
			if ( IsClient )
			{
				return;
			}

			for ( int i = Children.Count - 1; i >= 0; i-- )
			{
				if ( Children[i] is not CheckersCell
					&& Children[i] is not CheckersPiece )
				{
					continue;
				}
				Children[i].Delete();
			}

			var pieceIdx = 0;

			for ( int y = 0; y < 8; y++ )
			{
				for ( int x = 0; x < 8; x++ )
				{
					var cell = Entity.Create<CheckersCell>();
					cell.SetParent( this );
					cell.Mins = new Vector3( CellSize * x, CellSize * y ) + Mins;
					cell.Maxs = (CellSize * Vector3.One).WithZ( 0 ) + cell.Mins;
					cell.BoardPosition = new Vector2( x, y );

					if ( (x + y) % 2 == 0 )
					{
						if ( pieceIdx >= 12 && pieceIdx < 20 )
						{
							pieceIdx++;
							continue;
						}
						pieceIdx++;
						var piece = Entity.Create<CheckersPiece>();
						piece.SetParent( this );
						piece.Team = pieceIdx <= 12 ? CheckersTeam.Black : CheckersTeam.Red;
						piece.MoveToPosition( cell.BoardPosition );
					}
				}
			}
		}

		public CheckersPiece GetPieceAt( Vector2 boardPosition )
		{
			return Children.FirstOrDefault( x => x is CheckersPiece p && p.BoardPosition == boardPosition ) as CheckersPiece;
		}

		public CheckersCell GetCellAt( Vector2 boardPosition )
		{
			foreach ( var child in Children )
			{
				if ( child is not CheckersCell cell
					|| cell.BoardPosition != boardPosition )
				{
					continue;
				}
				return cell;
			}
			return null;
		}

		public CheckersCell GetCellAt( Vector3 worldOrigin )
		{
			foreach ( var child in Children )
			{
				if ( child is not CheckersCell cell || !cell.Contains( worldOrigin ) )
				{
					continue;
				}
				return cell;
			}
			return null;
		}

		public bool TeamCanJump( CheckersTeam team )
		{
			var pieces = team == CheckersTeam.Red ? RedPieces : BlackPieces;
			List<CheckersMove> moves = new List<CheckersMove>();
			foreach ( var p in pieces )
			{
				moves.AddRange( p.GetLegalMoves() );
			}
			return moves.Any( x => x.Jump.IsValid() );
		}

	}
}
