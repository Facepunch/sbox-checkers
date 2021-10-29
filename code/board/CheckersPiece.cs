using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	class CheckersMove
	{
		public CheckersPiece Piece;
		public CheckersCell Cell;
		public CheckersPiece Jump;
	}

	partial class CheckersPiece : ModelEntity
	{

		[Net]
		public Vector2 BoardPosition { get; set; }
		[Net, Change(nameof(SetIsKing))]
		public bool IsKing { get; set; }
		[Net, Change( nameof( SetTeamColor ) )]
		public CheckersTeam Team { get; set; }
		[Net]
		public bool Floating { get; set; }
		[Net]
		public Vector3 DragPosition { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;

			SetModel( "models/checkers_piece.vmdl" );
			SetTeamColor();
		}

		public bool MoveToPosition( Vector2 position, bool checkLegality = false )
		{
			var board = Parent as CheckersBoard;
			var cell = board.GetCellAt( position );

			if ( cell == null )
			{
				return false;
			}

			if ( checkLegality )
			{
				var legalMoves = GetLegalMoves();
				var hasLegalMove = legalMoves.FirstOrDefault( x => x.Cell.IsValid() && x.Cell.BoardPosition == position ) != null;

				if ( !hasLegalMove )
				{
					return false;
				}
			}

			BoardPosition = cell.BoardPosition;
			Position = cell.Center;

			if ( (position.y == 7 && Team == CheckersTeam.Black)
				|| (position.y == 0 && Team == CheckersTeam.Red) )
			{
				IsKing = true;
			}

			return true;
		}

		[Event.Tick]
		private void OnTick()
		{
			if ( IsClient )
			{
				return;
			}

			var cell = CheckersBoard.Current.GetCellAt( BoardPosition );
			var staticPos = cell.Center;

			if ( Floating )
			{
				staticPos = DragPosition;
				Rotation = Rotation.RotateAroundAxis( Vector3.Up, Time.Delta * 30 );
				staticPos.z += 20;
				staticPos.z += (float)Math.Sin( Time.Now * Math.PI * .75f ) * 20;
			}

			Position = staticPos;
		}

		public List<CheckersMove> GetLegalMoves()
		{
			var result = new List<CheckersMove>();
			var board = Parent as CheckersBoard;

			foreach ( var dir in MoveDirs )
			{
				var targetPosition = BoardPosition + dir;
				var move = new CheckersMove();
				move.Piece = this;
				move.Cell = board.GetCellAt( targetPosition );
				switch ( GetMoveState( targetPosition ) )
				{
					case MoveState.Yes:
						result.Add( move );
						break;
					case MoveState.YesIfKing:
						if ( IsKing )
							result.Add( move );
						break;
					case MoveState.OccupiedByEnemy:
						var jumpPosition = BoardPosition + dir * 2;
						move.Jump = board.GetPieceAt( targetPosition );
						move.Cell = board.GetCellAt( BoardPosition + dir * 2 );
						var moveState = GetMoveState( jumpPosition );
						if( moveState == MoveState.Yes || (moveState == MoveState.YesIfKing && IsKing) )
						{
							result.Add( move );
						}
						break;
				}
			}

			if( result.Any(x => x.Jump.IsValid() ) )
			{
				result.RemoveAll( x => !x.Jump.IsValid() );
			}

			return result;
		}

		static Vector2[] MoveDirs => new Vector2[]
		{
			new Vector2(-1, 1),
			new Vector2(1, 1),
			new Vector2(1, -1),
			new Vector2(-1, -1)
		};

		private enum MoveState
		{
			No,
			Yes,
			YesIfKing,
			OccupiedByEnemy
		}

		private MoveState GetMoveState( Vector2 boardPosition )
		{
			var board = Parent as CheckersBoard;
			var cell = board.GetCellAt( boardPosition );

			// cell doesn't exist, probably outside the board bounds
			if ( !cell.IsValid() )
			{
				return MoveState.No;
			}

			// there's a piece occupying this space
			var piece = board.GetPieceAt( boardPosition );
			if ( piece.IsValid() )
			{
				// we can't jump our own piece
				if( piece.Team == Team )
				{
					return MoveState.No;
				}
				return MoveState.OccupiedByEnemy;
			}

			// only kings can move backwards
			var yDir = boardPosition.y - BoardPosition.y;
			if ( (yDir > 0 && Team == CheckersTeam.Red)
				|| (yDir < 0 && Team == CheckersTeam.Black) )
			{
				return MoveState.YesIfKing;
			}

			// the move is ok
			return MoveState.Yes;
		}

		private void SetIsKing()
		{
			if ( IsKing )
			{
				SetModel( "models/checkers_king.vmdl" );
			}
			else
			{
				SetModel( "models/checkers_piece.vmdl" );
			}
		}

		private void SetTeamColor()
		{
			RenderColor = Team == CheckersTeam.Red
				? Color.Red
				: Team == CheckersTeam.Black
					? Color.Black
					: Color.White;
		}

	}
}
