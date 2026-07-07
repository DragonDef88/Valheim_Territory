using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal sealed class WardMenuWardActions : IWardMenuWardActions
    {
        public void ToggleProtection(WardId wardId)
        {
            ModLog.Debug("[WardMenuActions] ToggleProtection requested: " + wardId);
        }

        public void SetRadius(WardId wardId, float radius)
        {
            ModLog.Debug("[WardMenuActions] SetRadius requested: " + wardId + ", radius: " + radius);
        }

        public void RemovePermittedPlayer(WardId wardId, long playerId)
        {
            ModLog.Debug("[WardMenuActions] RemovePermittedPlayer requested: " + wardId + ", playerId: " + playerId);
        }
    }
}