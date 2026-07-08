using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal interface IWardMenuTerritoryActions
    {
        void RenameTerritory(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string name);

        bool ToggleDoorLock(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool SetDoorAutoCloseSeconds(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int seconds);

        bool ToggleStructureDamageProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        void ToggleGuildAccess(WardId wardId);

        void ToggleGroupAccess(WardId wardId);
    }
}
