namespace ClanTerritory.Integration.Groups
{
    internal interface IGroupsService
    {
        bool IsAvailable { get; }

        bool TryGetPlayerGroupId(long playerId, out string groupId);

        bool ArePlayersInSameGroup(long firstPlayerId, long secondPlayerId);

        bool IsPlayerGroupLeader(long playerId);
    }
}