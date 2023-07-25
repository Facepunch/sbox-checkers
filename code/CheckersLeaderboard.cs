
namespace Facepunch.Checkers;

public static partial class CheckersLeaderboard
{

	const double defaultElo = 1200.0;

	public static async Task<double> GetScore( long playerid )
	{
		var ldb = Sandbox.Services.Leaderboards.Get( "Elo" );
		ldb.TargetSteamId = playerid;
		await ldb.Refresh();

		var player = ldb.Entries.FirstOrDefault( x => x.SteamId == playerid );
		if ( player.SteamId != playerid ) return defaultElo;

		return ldb.Entries[0].Value;
	}

	public static async Task UpdateScores( IClient winner, IClient loser )
	{
		var winnerScore = await GetScore( winner.SteamId );
		var loserScore = await GetScore( loser.SteamId );

		var (winnerNewScore, loserNewScore) = CalculateElo( winnerScore, loserScore );

		UpdateMyScore( To.Single( winner), winnerNewScore );
		UpdateMyScore( To.Single( loser ), loserNewScore );
	}

	public static (double winnerNewScore, double loserNewScore) CalculateElo( double winnerScore, double loserScore )
	{
		const int K = 32;

		double winnerExpected = 1 / (1 + Math.Pow( 10, (loserScore - winnerScore) / 400.0 ));
		double loserExpected = 1 / (1 + Math.Pow( 10, (winnerScore - loserScore) / 400.0 ));

		double winnerNewScore = winnerScore + (int)(K * (1 - winnerExpected));
		double loserNewScore = loserScore + (int)(K * (0 - loserExpected));

		return (winnerNewScore, loserNewScore);
	}

	[ClientRpc]
	public static void UpdateMyScore( double newScore )
	{
		Game.AssertClient();

		Sandbox.Services.Stats.SetValue( "elo", newScore );
	}

}
