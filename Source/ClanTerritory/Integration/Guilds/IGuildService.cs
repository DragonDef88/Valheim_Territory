namespace ClanTerritory.Integration.Guilds
{
    internal interface IGuildService
    {
        bool IsAvailable { get; }

        bool TryGetPlayerGuildId(long playerId, out string guildId);

        bool TryGetPlayerGuildName(long playerId, out string guildName);

        bool ArePlayersInSameGuild(long firstPlayerId, long secondPlayerId);

        bool IsPlayerGuildLeader(long playerId);
    }
}