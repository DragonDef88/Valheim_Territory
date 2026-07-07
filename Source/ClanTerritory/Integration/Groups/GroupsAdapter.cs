using ClanTerritory.Utils;

namespace ClanTerritory.Integration.Groups
{
    internal sealed class GroupsAdapter : IGroupsService
    {
        public bool IsAvailable
        {
            get { return false; }
        }

        public bool TryGetPlayerGroupId(long playerId, out string groupId)
        {
            groupId = null;

            ModLog.Debug("[Groups] Group lookup requested, but adapter is not implemented yet. PlayerId: " + playerId);

            return false;
        }

        public bool ArePlayersInSameGroup(long firstPlayerId, long secondPlayerId)
        {
            ModLog.Debug(
                "[Groups] Same group check requested, but adapter is not implemented yet. First: " +
                firstPlayerId +
                ", Second: " +
                secondPlayerId);

            return false;
        }

        public bool IsPlayerGroupLeader(long playerId)
        {
            ModLog.Debug("[Groups] Group leader check requested, but adapter is not implemented yet. PlayerId: " + playerId);

            return false;
        }
    }
}