using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal interface IWardMenuWardActions
    {
        bool ToggleProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        void SetRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            float radius);

        bool RemovePermittedPlayer(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            long playerId);

        bool ToggleSelfPermission(
            WardId wardId,
            PrivateArea privateArea,
            Player player);
    }
}
