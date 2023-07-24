
namespace Facepunch.Checkers;

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

		_moveDelay = new Random().Float( 0.65f, 2f );

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

		foreach ( var piece in CheckersBoard.Current.Pieces )
		{
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
		var move = boardState.Predict( me.Team );

		if ( move == null )
		{
			return false;
		}

		piecePosition = move.Me.Position;
		targetPosition = move.TargetPosition;

		return true;
	}

}
