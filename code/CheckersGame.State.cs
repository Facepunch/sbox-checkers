﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersGame
	{

		[Net, Change( nameof( ClientGameStateChanged ) )]
		public GameState CurrentState { get; set; }
		[Net]
		public CheckersTeam ActiveTeam { get; set; } // the team that needs to make a move
		[Net]
		public float TurnTimer { get; set; }

		public const float PlayerTurnTime = 30;

		[Event.Tick]
		private void OnTick()
		{
			if ( IsClient )
			{
				return;
			}

			switch ( CurrentState )
			{
				case GameState.WaitingToStart:
					TickWaitingToStart();
					break;
				case GameState.Live:
					TickLive();
					break;
			}
		}

		private void TickLive()
		{
			TurnTimer -= Time.Delta;
			if ( TurnTimer <= 0 )
			{
				EndTurn();
			}
		}

		private void TickWaitingToStart()
		{
			var player1 = Player.All.FirstOrDefault( x => x is CheckersPlayer pl
				&& pl.Team == CheckersTeam.Red
				&& pl.IsValid );
			var player2 = Player.All.FirstOrDefault( x => x is CheckersPlayer pl
				&& pl.Team == CheckersTeam.Black
				&& pl.IsValid );

			if ( player1 == null || player2 == null )
			{
				return;
			}

			TurnTimer = PlayerTurnTime;
			ActiveTeam = CheckersTeam.Black;

			SetGameState( GameState.Live );
		}

		public void SetGameState( GameState newState )
		{
			CurrentState = newState;

			Event.Run( CheckersEvents.GameStateChanged, newState );
		}

		public void AttemptMove( CheckersPlayer player, CheckersPiece piece, Vector2 target )
		{
			if ( IsClient )
			{
				// client shouldn't be doing this
				return;
			}

			if ( player.Team != ActiveTeam
				|| player.Team != piece.Team )
			{
				return;
			}

			var move = piece.GetLegalMoves().FirstOrDefault( x => x.Cell.BoardPosition == target );
			if ( move == null )
			{
				// invalid move, notify the client
				return;
			}

			// todo: mandatory jumps

			if ( move.Jump.IsValid() )
			{
				// eliminated a piece, notify the client
				move.Jump.Delete();
			}

			piece.BoardPosition = target;

			if ( (target.y == 7 && piece.Team == CheckersTeam.Black)
				|| (target.y == 0 && piece.Team == CheckersTeam.Red) )
			{
				piece.IsKing = true;
			}

			EndTurn();
		}

		public void EndTurn()
		{
			ActiveTeam = ActiveTeam == CheckersTeam.Black
				? CheckersTeam.Red
				: CheckersTeam.Black;
			TurnTimer = PlayerTurnTime;
		}

		private bool TeamHasLegalJump( CheckersTeam team )
		{
			var pieces = Entity.All.Where( x => x is CheckersPiece p && p.IsValid() && p.Team == team );
			foreach ( var ent in pieces )
			{
				var piece = ent as CheckersPiece;
				var moves = piece.GetLegalMoves();
				if ( moves.FirstOrDefault( x => x.Jump != null ) != null )
				{
					return true;
				}
			}
			return false;
		}

		private void ClientGameStateChanged()
		{
			Event.Run( CheckersEvents.GameStateChanged, CurrentState );
		}

	}
}
