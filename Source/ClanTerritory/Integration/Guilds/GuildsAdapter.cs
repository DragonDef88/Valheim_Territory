using ClanTerritory.Utils;

namespace ClanTerritory.Integration.Guilds
{
    internal sealed class GuildsAdapter : IGuildService
    {
        public bool IsAvailable
        {
            get { return false; }
        }

        public bool TryGetPlayerGuildId(long playerId, out string guildId)
        {
            guildId = null;

            ModLog.Debug("[Guilds] Guild lookup requested, but adapter is not implemented yet. PlayerId: " + playerId);

            return false;
        }

        public bool TryGetPlayerGuildName(long playerId, out string guildName)
        {
            guildName = null;

            ModLog.Debug("[Guilds] Guild name lookup requested, but adapter is not implemented yet. PlayerId: " + playerId);

            return false;
        }

        public bool ArePlayersInSameGuild(long firstPlayerId, long secondPlayerId)
        {
            ModLog.Debug(
                "[Guilds] Same guild check requested, but adapter is not implemented yet. First: " +
                firstPlayerId +
                ", Second: " +
                secondPlayerId);

            return false;
        }

        public bool IsPlayerGuildLeader(long playerId)
        {
            ModLog.Debug("[Guilds] Guild leader check requested, but adapter is not implemented yet. PlayerId: " + playerId);

            return false;
        }
    }
}