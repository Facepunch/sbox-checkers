
namespace Sandbox
{
    public static class LobbyHelper
    {

        public static PlayerInfo? GetPlayerInfo(this ILobby lobby, long steamId)
        {
            if (!lobby.Data.TryGetValue(steamId.ToString(), out var json))
                return null;

            return Json.Deserialize<PlayerInfo>(json);
        }

        public static PlayerInfo? GetPlayerInfo(this ILobby lobby, CheckersTeam team)
        {
            var playerId = GetPlayer(lobby, team);
            if (playerId != null)
            {
                return GetPlayerInfo(lobby, playerId.Value);
            }

            return null;
        }

        public static void SetPlayerInfo(this ILobby lobby, long steamId, PlayerInfo info)
        {
            lobby.SetData(steamId.ToString(), Json.Serialize(info));
        }

        public static bool SetPlayerTeam(this ILobby lobby, long steamId, CheckersTeam team)
        {
            var playerInfo = GetPlayerInfo(lobby, steamId);
            if (playerInfo == null || !HasMember(lobby, steamId)) return false;

            var pi = playerInfo.Value;

            switch (team)
            {
                case CheckersTeam.Red:
                case CheckersTeam.Black:
                    if (GetPlayer(lobby, team) != null) return false;
                    pi.Team = team;
                    SetPlayerInfo(lobby, steamId, pi);
                    return true;
                case CheckersTeam.Spectator:
                    pi.Team = team;
                    SetPlayerInfo(lobby, steamId, pi);
                    return true;
                default:
                    return false;
            }
        }

        public static long? GetPlayer(this ILobby lobby, CheckersTeam team)
        {
            foreach (var member in lobby.Members)
            {
                var playerInfo = GetPlayerInfo(lobby, member.Id);
                if (playerInfo != null && playerInfo.Value.Team == team)
                {
                    return member.Id;
                }
            }

            return null;
        }

        public static bool HasMember( this ILobby lobby, long steamId )
        {
            return lobby.Members.Any(x => x.Id == steamId);
        }

    }

    public struct PlayerInfo
    {
        public string Name { get; set; }
        public int EloScore { get; set; }
        public CheckersTeam Team { get; set; }
    }
}
