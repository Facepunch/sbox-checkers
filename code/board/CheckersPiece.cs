using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersPiece : ModelEntity
	{

		[Net]
		public Vector2 BoardPosition { get; set; }
		[Net]
		public bool IsKing { get; set; }
		[Net, Change( nameof( SetTeamColor ) )]
		public CheckersTeam Team { get; set; }

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

			if ( checkLegality && !GetLegalCells().Contains( cell ) )
			{
				return false;
			}

			BoardPosition = cell.BoardPosition;
			Position = cell.Center;

			return true;
		}

		[Event.Frame]
		private void OnFrame()
		{
			var cell = (Parent as CheckersBoard).GetCellAt( BoardPosition );
			if ( !cell.Hovered )
			{
				return;
			}

			foreach ( var c in GetLegalCells() )
			{
				DebugOverlay.Line( c.Center, c.Center + Vector3.Up * 100 );
			}
		}

		public List<CheckersCell> GetLegalCells()
		{
			var board = Parent as CheckersBoard;
			var my = Team == CheckersTeam.Red ? -1 : 1;

			var legalMoves = new List<Vector2>()
			{
				 new Vector2( 1, my ),
				 new Vector2( -1, my )
			};

			if ( IsKing )
			{
				legalMoves.Add( new Vector2( 1, -my ) );
				legalMoves.Add( new Vector2( -1, -my ) );
			}

			var result = new List<CheckersCell>();

			foreach ( var dir in legalMoves )
			{
				var legalPos = BoardPosition + dir;
				var cell = board.GetCellAt( legalPos );

				if ( cell == null )
				{
					continue;
				}

				if(board.GetPieceAt(legalPos) != null )
				{
					continue;
				}

				result.Add( cell );
			}

			return result;
		}

		private void SetTeamColor()
		{
			RenderColor = Team == CheckersTeam.Red
				? Color.Red
				: Team == CheckersTeam.Black
					? Color.Black
					: Color.White;
		}

		[ServerCmd]
		public static void NetworkMove( int pieceId, Vector2 boardPosition )
		{
			if ( ConsoleSystem.Caller.Pawn is not CheckersPlayer player )
			{
				return;
			}

			if ( Entity.FindByIndex( pieceId ) is not CheckersPiece piece )
			{
				return;
			}

			if ( piece.Team != player.Team )
			{
				return;
			}

			if(!piece.MoveToPosition( boardPosition, true ) )
			{
				// notify invalid move
				return;
			}

			// notify valid move
		}

	}
}
