
namespace Facepunch.Checkers;

class AiBoardState
{

	public class PieceData
	{
		public CheckersTeam Team;
		public Vector2 Position;
		public bool IsKing;
		public bool IsAlive;
		public int Score;
		public AiCheckersMove BestMove;
	}

	public int RedScore { get; private set; }
	public int BlackScore { get; private set; }
	public AiBoardState Parent { get; private set; }
	public List<PieceData> Positions { get; private set; }
	public AiCheckersMove BestRedMove { get; private set; }
	public AiCheckersMove BestBlackMove { get; private set; }

	public AiBoardState( AiBoardState parent, List<PieceData> positions )
	{
		Parent = parent;
		Positions = positions;

		CalculateScores();
	}

	public AiCheckersMove Predict( CheckersTeam team )
	{
		var piecesWithMoves = Positions.Where( x => x.BestMove != null && x.Team == team );
		if ( !piecesWithMoves.Any() )
		{
			return null;
		}

		var myMaxScore = int.MinValue;
		var enemyMinScore = int.MaxValue;
		AiCheckersMove bestOffensiveMove = null;
		AiCheckersMove bestDefensiveMove = null;

		foreach ( var piece in piecesWithMoves )
		{
			var newState = GetPredictedBoardState( piece.BestMove );
			var enemyScore = team == CheckersTeam.Red ? newState.BlackScore : newState.RedScore;
			var myScore = team == CheckersTeam.Red ? newState.RedScore : newState.BlackScore;

			if ( enemyScore < enemyMinScore )
			{
				bestOffensiveMove = piece.BestMove;
				enemyMinScore = enemyScore;
			}

			if ( myScore > myMaxScore )
			{
				bestDefensiveMove = piece.BestMove;
				myMaxScore = myScore;
			}
		}

		var myCurrentScore = team == CheckersTeam.Red ? RedScore : BlackScore;
		if ( myMaxScore > myCurrentScore )
			return bestDefensiveMove;

		return bestOffensiveMove;
	}

	private AiBoardState GetPredictedBoardState( AiCheckersMove predictedMove )
	{
		var predictedPositions = new List<PieceData>();

		foreach ( var pos in Positions )
		{
			var pieceData = new PieceData()
			{
				Position = pos.Position,
				Team = pos.Team,
				IsKing = pos.IsKing,
				IsAlive = pos.IsAlive
			};
			predictedPositions.Add( pieceData );
		}

		var movedPiece = predictedPositions.First( x => x.Position == predictedMove.Me.Position );
		movedPiece.Position = predictedMove.TargetPosition;

		if ( !movedPiece.IsKing )
		{
			movedPiece.IsKing = WillBecomeKing( movedPiece, predictedMove );
		}

		if ( predictedMove.Jump != null )
		{
			var jumpedPiece = predictedPositions.First( x => x.Position == predictedMove.Jump.Position );
			jumpedPiece.IsAlive = false;
		}

		return new AiBoardState( this, predictedPositions );
	}

	private void CalculateScores()
	{
		foreach ( var pos in Positions )
		{
			CalculateScore( pos );
		}

		var redPositions = Positions.Where( x => x.Team == CheckersTeam.Red );
		var blackPositions = Positions.Where( x => x.Team == CheckersTeam.Black );
		RedScore = redPositions.Sum( x => x.Score );
		BlackScore = blackPositions.Sum( x => x.Score );

		var redMoves = redPositions.Where( x => x.BestMove != null );
		var blackMoves = blackPositions.Where( x => x.BestMove != null );
		BestRedMove = FindMaxMove( redMoves );
		BestBlackMove = FindMaxMove( blackMoves );
	}

	private AiCheckersMove FindMaxMove( IEnumerable<PieceData> positions )
	{
		var maxScore = int.MinValue;
		AiCheckersMove result = null;

		foreach ( var p in positions )
		{
			if ( p.BestMove == null )
			{
				continue;
			}
			if ( p.BestMove.Score >= maxScore )
			{
				result = p.BestMove;
				maxScore = p.BestMove.Score;
			}
		}

		return result;
	}

	private void CalculateScore( PieceData position )
	{
		position.Score = 1;

		if ( !position.IsAlive )
		{
			position.Score = 0;
			return;
		}

		var legalMoves = GetLegalMoves( position );

		if ( legalMoves.Count == 0 )
		{
			// can't do shit, score for alive and exit
			position.Score = 1;
			return;
		}

		foreach ( var move in legalMoves )
		{
			if ( move.Vulnerable )
				move.Score -= 3;

			if ( !position.IsKing && WillBecomeKing( position, move ) )
				move.Score += 2;

			if ( move.Jump != null )
				move.Score += 3;

			if ( position.BestMove == null || move.Score >= position.BestMove.Score )
				position.BestMove = move;
		}
	}

	private bool WillBecomeKing( PieceData p, AiCheckersMove move )
	{
		if ( (move.TargetPosition.y == 0 && p.Team == CheckersTeam.Black)
			|| (move.TargetPosition.y == 7 && p.Team == CheckersTeam.Red) )
		{
			return true;
		}
		return false;
	}

	private bool PositionIsVulnerable( CheckersTeam team, Vector2 boardPosition )
	{
		foreach ( var piece in Positions.Where( x => x.Team != team ) )
		{
			var dir = boardPosition - piece.Position;
			var jumpPos = piece.Position + dir * 2;

			if ( !MoveDirs.Contains( dir ) || !InBounds( jumpPos ) )
				continue;

			if ( Positions.Any( x => x.Position == jumpPos ) )
				continue;

			if ( !piece.IsKing )
				if ( (piece.Team == CheckersTeam.Red && dir.y > 0) || (piece.Team == CheckersTeam.Black && dir.y < 0) )
					continue;

			return true;
		}

		return false;
	}

	// todo: this is quickly ported from CheckersPiece.cs
	// it should be unified somewhere

	private List<AiCheckersMove> GetLegalMoves( PieceData p )
	{
		var result = new List<AiCheckersMove>();

		foreach ( var dir in MoveDirs )
		{
			var targetPosition = p.Position + dir;
			if ( !InBounds( targetPosition ) ) continue;
			var move = new AiCheckersMove();
			move.Me = p;
			move.TargetPosition = targetPosition;
			move.Vulnerable = PositionIsVulnerable( p.Team, move.TargetPosition );

			switch ( GetMoveState( p, targetPosition ) )
			{
				case MoveState.Yes:
					result.Add( move );
					break;
				case MoveState.YesIfKing:
					if ( p.IsKing )
						result.Add( move );
					break;
				case MoveState.OccupiedByEnemy:
					var jumpPosition = p.Position + dir * 2;
					if ( !InBounds( jumpPosition ) ) break;
					move.Jump = Positions.First( x => x.Position == targetPosition );
					move.TargetPosition = jumpPosition;
					var moveState = GetMoveState( p, jumpPosition );
					if ( moveState == MoveState.Yes || (moveState == MoveState.YesIfKing && p.IsKing) )
					{
						result.Add( move );
					}
					break;
			}
		}

		if ( result.Any( x => x.Jump != null ) )
			result.RemoveAll( x => x.Jump == null );

		return result;
	}

	static Vector2[] MoveDirs => new Vector2[]
	{
		new Vector2(-1, 1),
		new Vector2(1, 1),
		new Vector2(1, -1),
		new Vector2(-1, -1)
	};

	public class AiCheckersMove
	{
		public PieceData Me;
		public Vector2 TargetPosition;
		public PieceData Jump;
		public int Score;
		public bool Vulnerable;
	}

	private enum MoveState
	{
		No,
		Yes,
		YesIfKing,
		OccupiedByEnemy
	}

	private MoveState GetMoveState( PieceData me, Vector2 targetPosition )
	{
		if ( !InBounds( targetPosition ) )
		{
			return MoveState.No;
		}

		// there's a piece occupying this space
		var piece = Positions.FirstOrDefault( x => x.Position == targetPosition );
		if ( piece != null )
		{
			// but maybe we can jump it
			if ( piece.Team != me.Team )
			{
				return MoveState.OccupiedByEnemy;
			}
			return MoveState.No;
		}

		// only kings can move backwards
		var yDir = targetPosition.y - me.Position.y;
		if ( (yDir > 0 && me.Team == CheckersTeam.Red)
			|| (yDir < 0 && me.Team == CheckersTeam.Black) )
		{
			return MoveState.YesIfKing;
		}

		// the move is ok
		return MoveState.Yes;
	}

	private bool InBounds( Vector2 position )
	{
		if ( position.x < 0 || position.x > 7
			|| position.y < 0 || position.y > 7 )
		{
			return false;
		}
		return true;
	}

}
