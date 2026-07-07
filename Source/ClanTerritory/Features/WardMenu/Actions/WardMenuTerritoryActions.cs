using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal sealed class WardMenuTerritoryActions : IWardMenuTerritoryActions
    {
        public void RenameTerritory(WardId wardId, string name)
        {
            ModLog.Debug("[WardMenuActions] RenameTerritory requested: " + wardId + ", name: " + name);
        }

        public void ToggleGuildAccess(WardId wardId)
        {
            ModLog.Debug("[WardMenuActions] ToggleGuildAccess requested: " + wardId);
        }

        public void ToggleGroupAccess(WardId wardId)
        {
            ModLog.Debug("[WardMenuActions] ToggleGroupAccess requested: " + wardId);
        }
    }
}