using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal sealed class WardMenuTerritoryActions : IWardMenuTerritoryActions
    {
        private readonly ITerritoryNamingService _territoryNamingService;

        public WardMenuTerritoryActions(
            ITerritoryNamingService territoryNamingService)
        {
            _territoryNamingService = territoryNamingService;
        }

        public void RenameTerritory(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string name)
        {
            if (_territoryNamingService == null)
            {
                ModLog.Debug("[WardMenuActions] RenameTerritory ignored. TerritoryNamingService is null: " + wardId);
                return;
            }

            _territoryNamingService.RequestRename(
                privateArea,
                player,
                name);

            ModLog.Info("[WardMenuActions] RenameTerritory requested: " + wardId + ", name: " + name);
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