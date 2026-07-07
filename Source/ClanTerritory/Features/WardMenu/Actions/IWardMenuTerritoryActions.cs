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

        void ToggleGuildAccess(WardId wardId);

        void ToggleGroupAccess(WardId wardId);
    }
}