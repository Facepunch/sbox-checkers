using Sandbox;
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
					break;
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

			var move = piece.GetLegalMoves().FirstOrDefault( x => x.Cell.BoardPosition == target );
			if ( move == null )
			{
				// invalid move, notify the client
				return;
			}

			Log.Info( "JUMPING: " + (move.Jump != null) );

			if ( move.Jump.IsValid() )
			{
				// eliminated a piece, notify the client
				move.Jump.Delete();
			}

			piece.BoardPosition = target;
		}

		private void ClientGameStateChanged()
		{
			Event.Run( CheckersEvents.GameStateChanged, CurrentState );
		}

	}
}
