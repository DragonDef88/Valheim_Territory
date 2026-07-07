using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal interface IWardMenuWardActions
    {
        void ToggleProtection(WardId wardId);

        void SetRadius(WardId wardId, float radius);

        void RemovePermittedPlayer(WardId wardId, long playerId);
    }
}