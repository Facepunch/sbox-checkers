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

		private float _startTimer = 2f;

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
				&& pl.Team == CheckersTeam.One
				&& pl.IsValid );
			var player2 = Player.All.FirstOrDefault( x => x is CheckersPlayer pl
				&& pl.Team == CheckersTeam.Two
				&& pl.IsValid );

			if ( player1 == null || player2 == null )
			{
				_startTimer = 2f;
				return;
			}

			_startTimer -= Time.Delta;

			if ( _startTimer <= 0 )
			{
				SetGameState( GameState.Live );
			}
		}

		public void SetGameState( GameState newState )
		{
			CurrentState = newState;

			Event.Run( CheckersEvents.GameStateChanged, newState );
		}

		private void ClientGameStateChanged()
		{
			Event.Run( CheckersEvents.GameStateChanged, CurrentState );
		}

	}
}
