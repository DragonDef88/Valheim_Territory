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

            if (!IsWardCreator(
                    wardId,
                    privateArea,
                    player))
            {
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

        private static bool IsWardCreator(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] RenameTerritory ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenuActions] RenameTerritory ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[WardMenuActions] RenameTerritory ignored. Piece is null: " + wardId);
                return false;
            }

            if (piece.GetCreator() != player.GetPlayerID())
            {
                ModLog.Debug("[WardMenuActions] RenameTerritory ignored. Player is not ward creator: " + wardId);
                return false;
            }

            return true;
        }
    }
}
