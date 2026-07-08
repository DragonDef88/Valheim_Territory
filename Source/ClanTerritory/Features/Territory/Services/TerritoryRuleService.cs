using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryRuleService
    {
        private const string SetDoorLockRpc = "CT_SetTerritoryDoorLock";
        private const string SetStructureDamageProtectionRpc = "CT_SetTerritoryStructureDamageProtection";

        private static readonly FieldInfo AllAreasField =
            AccessTools.Field(typeof(PrivateArea), "m_allAreas");

        public void RegisterRpc(PrivateArea privateArea)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRules] RPC registration ignored. PrivateArea is null.");
                return;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || zNetView.GetZDO() == null)
            {
                ModLog.Debug("[TerritoryRules] RPC registration ignored. ZNetView or ZDO is null.");
                return;
            }

            zNetView.Register<long, bool>(
                SetDoorLockRpc,
                delegate (long sender, long playerId, bool enabled)
                {
                    RPC_SetDoorLock(
                        privateArea,
                        zNetView,
                        sender,
                        playerId,
                        enabled);
                });

            zNetView.Register<long, bool>(
                SetStructureDamageProtectionRpc,
                delegate (long sender, long playerId, bool enabled)
                {
                    RPC_SetStructureDamageProtection(
                        privateArea,
                        zNetView,
                        sender,
                        playerId,
                        enabled);
                });

            ModLog.Debug("[TerritoryRules] RPCs registered for ward.");
        }

        public bool GetDoorLockEnabled(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            return zdo != null &&
                   zdo.GetBool(
                       TerritoryZdoKeys.DoorLockEnabled,
                       false);
        }

        public bool GetStructureDamageProtectionEnabled(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            return zdo != null &&
                   zdo.GetBool(
                       TerritoryZdoKeys.StructureDamageProtectionEnabled,
                       false);
        }

        public bool RequestSetDoorLock(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool enabled)
        {
            if (!ValidateCreatorAction(
                    "SetDoorLock",
                    wardId,
                    privateArea,
                    player))
            {
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryRules] SetDoorLock ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                SetDoorLockRpc,
                player.GetPlayerID(),
                enabled);

            ModLog.Info("[TerritoryRules] Door lock change requested: " + wardId + ", enabled: " + enabled);
            return true;
        }

        public bool RequestSetStructureDamageProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool enabled)
        {
            if (!ValidateCreatorAction(
                    "SetStructureDamageProtection",
                    wardId,
                    privateArea,
                    player))
            {
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryRules] SetStructureDamageProtection ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                SetStructureDamageProtectionRpc,
                player.GetPlayerID(),
                enabled);

            ModLog.Info("[TerritoryRules] Structure damage protection change requested: " + wardId + ", enabled: " + enabled);
            return true;
        }

        public bool IsDoorLockedForPlayer(
            Vector3 position,
            Player player)
        {
            List<PrivateArea> areas = GetPrivateAreas();

            for (int i = 0; i < areas.Count; i++)
            {
                PrivateArea privateArea = areas[i];

                if (!IsInside(privateArea, position))
                    continue;

                if (!GetDoorLockEnabled(privateArea))
                    continue;

                if (!HasAccess(
                        privateArea,
                        player))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsDoorLockedForLocalPlayer(Vector3 position)
        {
            return IsDoorLockedForPlayer(
                position,
                Player.m_localPlayer);
        }

        public bool IsStructureDamageProtected(Vector3 position)
        {
            List<PrivateArea> areas = GetPrivateAreas();

            for (int i = 0; i < areas.Count; i++)
            {
                PrivateArea privateArea = areas[i];

                if (!IsInside(privateArea, position))
                    continue;

                if (GetStructureDamageProtectionEnabled(privateArea))
                    return true;
            }

            return false;
        }

        private void RPC_SetDoorLock(
            PrivateArea privateArea,
            ZNetView zNetView,
            long sender,
            long playerId,
            bool enabled)
        {
            SetRuleOnOwner(
                "DoorLock",
                TerritoryZdoKeys.DoorLockEnabled,
                privateArea,
                zNetView,
                playerId,
                enabled);
        }

        private void RPC_SetStructureDamageProtection(
            PrivateArea privateArea,
            ZNetView zNetView,
            long sender,
            long playerId,
            bool enabled)
        {
            SetRuleOnOwner(
                "StructureDamageProtection",
                TerritoryZdoKeys.StructureDamageProtectionEnabled,
                privateArea,
                zNetView,
                playerId,
                enabled);
        }

        private static void SetRuleOnOwner(
            string ruleName,
            string zdoKey,
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool enabled)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. PrivateArea is null: " + ruleName);
                return;
            }

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. ZNetView is invalid: " + ruleName);
                return;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. ZNetView is not owner: " + ruleName);
                return;
            }

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. ZDO is null: " + ruleName);
                return;
            }

            if (zdo.GetLong(ZDOVars.s_creator, 0L) != playerId)
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. Player is not ward creator: " + ruleName + ", playerId: " + playerId);
                return;
            }

            zdo.Set(
                zdoKey,
                enabled);

            ModLog.Info("[TerritoryRules] Rule saved: " + ruleName + ", enabled: " + enabled);
        }

        private static bool ValidateCreatorAction(
            string actionName,
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRules] " + actionName + " ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[TerritoryRules] " + actionName + " ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[TerritoryRules] " + actionName + " ignored. Piece is null: " + wardId);
                return false;
            }

            if (piece.GetCreator() != player.GetPlayerID())
            {
                ModLog.Debug("[TerritoryRules] " + actionName + " ignored. Player is not ward creator: " + wardId);
                return false;
            }

            return true;
        }

        private static bool HasAccess(
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null || player == null)
                return false;

            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return false;

            long playerId = player.GetPlayerID();

            if (zdo.GetLong(ZDOVars.s_creator, 0L) == playerId)
                return true;

            int permittedCount = zdo.GetInt(ZDOVars.s_permitted);

            for (int i = 0; i < permittedCount; i++)
            {
                long permittedPlayerId = zdo.GetLong("pu_id" + i, 0L);

                if (permittedPlayerId == playerId)
                    return true;
            }

            return false;
        }

        private static bool IsInside(
            PrivateArea privateArea,
            Vector3 position)
        {
            if (privateArea == null)
                return false;

            return global::Utils.DistanceXZ(
                       privateArea.transform.position,
                       position) < privateArea.m_radius;
        }

        private static List<PrivateArea> GetPrivateAreas()
        {
            if (AllAreasField == null)
                return new List<PrivateArea>();

            List<PrivateArea> areas =
                AllAreasField.GetValue(null) as List<PrivateArea>;

            if (areas == null)
                return new List<PrivateArea>();

            return areas;
        }

        private static ZDO GetZdo(PrivateArea privateArea)
        {
            if (privateArea == null)
                return null;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return null;

            return zNetView.GetZDO();
        }
    }
}
