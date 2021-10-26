using Facepunch.Checkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Checkers
{
	class CheckersBot : Bot
	{

		public override void Tick()
		{
			base.Tick();

			var me = Client.Pawn as CheckersPlayer;
			if ( !me.IsValid() )
			{
				return;
			}

			if ( CheckersGame.Instance.CurrentState != GameState.Live )
			{
				return;
			}

			if ( me.Team != CheckersGame.Instance.ActiveTeam )
			{
				return;
			}

			if ( !FindBestMove( out Vector2 piecePos, out Vector2 targetPos ) )
			{
				// todo: end game if a player has no moves
				// (also make sure the ai isn't shit and failed to find a move)
				return;
			}

			var board = Entity.All.FirstOrDefault( x => x is CheckersBoard ) as CheckersBoard;
			if ( board == null )
			{
				// ???
				return;
			}

			var piece = board.GetPieceAt( piecePos );

			if ( piece == null )
			{
				// ???
				return;
			}

			// todo: pass Move object instead?
			CheckersGame.Instance.AttemptMove( me, piece, targetPos );
		}

		private bool FindBestMove( out Vector2 piecePosition, out Vector2 targetPosition )
		{
			piecePosition = Vector2.Zero;
			targetPosition = Vector2.Zero;

			var me = Client.Pawn as CheckersPlayer;
			var pieces = Entity.All.Where( x => x is CheckersPiece p
				&& p.IsValid() );
			var boardPositions = new List<ScoredBoardState.BoardPosition>();

			foreach ( var ent in pieces )
			{
				var piece = ent as CheckersPiece;
				boardPositions.Add( new ScoredBoardState.BoardPosition()
				{
					IsAlive = true,
					IsKing = piece.IsKing,
					Position = piece.BoardPosition,
					Team = piece.Team
				} );
			}

			// todo : recursively calculate a few moves and board states to predict the best move

			var boardState = new ScoredBoardState( null, boardPositions );
			ScoredBoardState.AiCheckersMove bestMove = null;

			switch ( me.Team )
			{
				case CheckersTeam.Red:
					bestMove = boardState.BestRedMove;
					break;
				case CheckersTeam.Black:
					bestMove = boardState.BestBlackMove;
					break;
			}

			if ( bestMove == null )
			{
				return false;
			}

			piecePosition = bestMove.Me.Position;
			targetPosition = bestMove.TargetPosition;

			return true;
		}

	}
}
