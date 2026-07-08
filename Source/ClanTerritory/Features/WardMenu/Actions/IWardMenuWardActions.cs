using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal interface IWardMenuWardActions
    {
        void ToggleProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        void SetRadius(
            WardId wardId,
            PrivateArea privateArea,
            float radius);

        void RemovePermittedPlayer(WardId wardId, long playerId);
    }
}