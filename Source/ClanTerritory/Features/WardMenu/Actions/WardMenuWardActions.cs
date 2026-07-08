using System.Collections.Generic;
using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal sealed class WardMenuWardActions : IWardMenuWardActions
    {
        private const string ToggleEnabledRpc = "ToggleEnabled";
        private const string TogglePermittedRpc = "TogglePermitted";

        public bool ToggleProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (!ValidateCreatorAction(
                    "ToggleProtection",
                    wardId,
                    privateArea,
                    player))
            {
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            long playerId = player.GetPlayerID();

            zNetView.InvokeRPC(ToggleEnabledRpc, playerId);

            ModLog.Info("[WardMenuActions] ToggleProtection invoked through Valheim RPC: " + wardId);
            return true;
        }

        public void SetRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            float radius)
        {
            if (!ValidateCreatorAction(
                    "SetRadius",
                    wardId,
                    privateArea,
                    player))
            {
                return;
            }

            TerritoryWardRadiusService radiusService;

            if (!ServiceContainer.TryGet<TerritoryWardRadiusService>(out radiusService))
            {
                ModLog.Debug("[WardMenuActions] SetRadius ignored. TerritoryWardRadiusService is missing: " + wardId);
                return;
            }

            radiusService.RequestSetRadius(
                  privateArea,
                  player,
                  radius);

            ModLog.Info("[WardMenuActions] SetRadius applied: " + wardId + ", radius: " + radius);
        }

        public bool RemovePermittedPlayer(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            long playerId)
        {
            if (!ValidateCreatorAction(
                    "RemovePermittedPlayer",
                    wardId,
                    privateArea,
                    player))
            {
                return false;
            }

            if (playerId == 0L)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. PlayerId is empty: " + wardId);
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. ZNetView is not owned locally: " + wardId);
                return false;
            }

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. ZDO is null: " + wardId);
                return false;
            }

            List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers(zdo);

            int removedCount = permittedPlayers.RemoveAll(
                delegate (KeyValuePair<long, string> permittedPlayer)
                {
                    return permittedPlayer.Key == playerId;
                });

            if (removedCount <= 0)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. Player is not permitted: " + wardId + ", playerId: " + playerId);
                return false;
            }

            SetPermittedPlayers(
                zdo,
                permittedPlayers);

            ModLog.Info("[WardMenuActions] RemovePermittedPlayer applied: " + wardId + ", playerId: " + playerId);
            return true;
        }

        public bool ToggleSelfPermission(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleSelfPermission ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleSelfPermission ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece != null && piece.GetCreator() == player.GetPlayerID())
            {
                ModLog.Debug("[WardMenuActions] ToggleSelfPermission ignored. Player is ward creator: " + wardId);
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[WardMenuActions] ToggleSelfPermission ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            if (zNetView.GetZDO().GetBool(ZDOVars.s_enabled))
            {
                ModLog.Debug("[WardMenuActions] ToggleSelfPermission ignored. Ward is enabled: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                TogglePermittedRpc,
                player.GetPlayerID(),
                player.GetPlayerName());

            ModLog.Info("[WardMenuActions] ToggleSelfPermission invoked through Valheim RPC: " + wardId);
            return true;
        }

        private static bool ValidateCreatorAction(
            string actionName,
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. Piece is null: " + wardId);
                return false;
            }

            if (piece.GetCreator() != player.GetPlayerID())
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. Player is not ward creator: " + wardId);
                return false;
            }

            return true;
        }

        private static List<KeyValuePair<long, string>> GetPermittedPlayers(ZDO zdo)
        {
            List<KeyValuePair<long, string>> players =
                new List<KeyValuePair<long, string>>();

            if (zdo == null)
                return players;

            int permittedCount = zdo.GetInt(ZDOVars.s_permitted);

            for (int i = 0; i < permittedCount; i++)
            {
                long playerId = zdo.GetLong("pu_id" + i, 0L);
                string playerName = zdo.GetString("pu_name" + i, "Unknown");

                if (playerId == 0L)
                    continue;

                players.Add(
                    new KeyValuePair<long, string>(
                        playerId,
                        playerName));
            }

            return players;
        }

        private static void SetPermittedPlayers(
            ZDO zdo,
            List<KeyValuePair<long, string>> players)
        {
            if (zdo == null)
                return;

            if (players == null)
                players = new List<KeyValuePair<long, string>>();

            zdo.Set(
                ZDOVars.s_permitted,
                players.Count);

            for (int i = 0; i < players.Count; i++)
            {
                KeyValuePair<long, string> player = players[i];

                zdo.Set(
                    "pu_id" + i,
                    player.Key);

                zdo.Set(
                    "pu_name" + i,
                    player.Value);
            }
        }
    }
}
