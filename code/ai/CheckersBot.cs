﻿using Facepunch.Checkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Checkers
{
	class CheckersBot : Bot
	{

		private float _moveDelay = 3;

		public override void Tick()
		{
			base.Tick();

			var me = Client.Pawn as CheckersPlayer;
			if ( !me.IsValid()
				|| CheckersGame.Instance.CurrentState != GameState.Live
				|| me.Team != CheckersGame.Instance.ActiveTeam )
			{
				return;
			}

			if ( _moveDelay > 0 )
			{
				_moveDelay -= Time.Delta;
				return;
			}

			_moveDelay = new Random().Float( 1, 5 );

			if ( !FindBestMove( out Vector2 piecePos, out Vector2 targetPos ) )
			{
				// todo: end game if a player has no moves
				// (also make sure the ai isn't shit and failed to find a move)
				return;
			}

			var piece = CheckersBoard.Current.GetPieceAt( piecePos );

			CheckersGame.Instance.AttemptMove( me, piece, targetPos );
		}

		private bool FindBestMove( out Vector2 piecePosition, out Vector2 targetPosition )
		{
			piecePosition = Vector2.Zero;
			targetPosition = Vector2.Zero;

			var me = Client.Pawn as CheckersPlayer;
			var boardPositions = new List<AiBoardState.PieceData>();

			// todo: integrate ai state so we don't have to set it up always

			foreach ( var ent in CheckersBoard.Current.Pieces )
			{
				var piece = ent as CheckersPiece;
				boardPositions.Add( new AiBoardState.PieceData()
				{
					IsAlive = true,
					IsKing = piece.IsKing,
					Position = piece.BoardPosition,
					Team = piece.Team
				} );
			}

			// todo : recursively calculate a few moves and board states to predict the best move

			var boardState = new AiBoardState( null, boardPositions );
			AiBoardState.AiCheckersMove bestMove = null;

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

		private AiBoardState PredictBestMove( AiBoardState init, int depth = 8 )
		{
			AiBoardState.AiCheckersMove bestMove = null;

			return null;
		}

	}
}
