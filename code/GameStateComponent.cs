using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class GameStateComponent : EntityComponent<CheckersGame>
	{

		[Net, Change( nameof( ClientGameStateChanged ) )]
		public GameState CurrentState { get; set; }
		[Net]
		public CheckersPlayer RedPlayer { get; set; }
		[Net]
		public CheckersPlayer BlackPlayer { get; set; }
		[Net]
		public CheckersTeam ActivePvvvlayer { get; set; } // the team that needs to make a move
		[Net]
		public float StateTimer { get; set; }

		private GameState _prevClientGameState;

		[Event.Tick]
		private void OnTick()
		{
			if ( Entity.IsClient )
			{
				return;
			}

			switch ( CurrentState )
			{
				case GameState.WaitingToStart:
					TickWaitingToStart();
					break;
			}
		}

		private void TickWaitingToStart()
		{
			if ( RedPlayer == null
				|| BlackPlayer == null
				|| !RedPlayer.IsValid
				|| !BlackPlayer.IsValid
				|| !RedPlayer.ReadyToStart
				|| !BlackPlayer.ReadyToStart )
			{
				StateTimer = 5f;
				return;
			}

			StateTimer -= Time.Delta;

			if ( StateTimer <= 0 )
			{
				SetCurrentState( GameState.Live );
			}
		}

		public void SetCurrentState( GameState newState )
		{
			var oldState = CurrentState;
			CurrentState = newState;

			Event.Run( CheckersEvents.GameStateChanged, oldState, newState );
		}

		private void ClientGameStateChanged()
		{
			Event.Run( CheckersEvents.GameStateChanged, _prevClientGameState, CurrentState );
			_prevClientGameState = CurrentState;
		}

	}
}
