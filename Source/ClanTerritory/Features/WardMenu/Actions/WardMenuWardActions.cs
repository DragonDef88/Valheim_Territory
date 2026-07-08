using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal sealed class WardMenuWardActions : IWardMenuWardActions
    {
        private const string ToggleEnabledRpc = "ToggleEnabled";

        public void ToggleProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. PrivateArea is null: " + wardId);
                return;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. Player is null: " + wardId);
                return;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. Piece is null: " + wardId);
                return;
            }

            if (!piece.IsCreator())
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. Player is not ward creator: " + wardId);
                return;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. ZNetView is invalid: " + wardId);
                return;
            }

            long playerId = player.GetPlayerID();

            zNetView.InvokeRPC(ToggleEnabledRpc, playerId);

            ModLog.Info("[WardMenuActions] ToggleProtection invoked through Valheim RPC: " + wardId);
        }

        public void SetRadius(
            WardId wardId,
            PrivateArea privateArea,
            float radius)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] SetRadius ignored. PrivateArea is null: " + wardId);
                return;
            }

            TerritoryWardRadiusService radiusService;

            if (!ServiceContainer.TryGet<TerritoryWardRadiusService>(out radiusService))
            {
                ModLog.Debug("[WardMenuActions] SetRadius ignored. TerritoryWardRadiusService is missing: " + wardId);
                return;
            }

            radiusService.ApplyRadius(
                privateArea,
                radius);

            ModLog.Info("[WardMenuActions] SetRadius applied: " + wardId + ", radius: " + radius);
        }

        public void RemovePermittedPlayer(WardId wardId, long playerId)
        {
            ModLog.Debug("[WardMenuActions] RemovePermittedPlayer requested: " + wardId + ", playerId: " + playerId);
        }
    }
}