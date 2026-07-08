using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Config;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryRuleService
    {
        private const string SetDoorLockRpc = "CT_SetTerritoryDoorLock";
        private const string SetDoorAutoCloseSecondsRpc = "CT_SetTerritoryDoorAutoCloseSeconds";
        private const string SetStructureDamageProtectionRpc = "CT_SetTerritoryStructureDamageProtection";

        private static readonly FieldInfo AllAreasField =
            AccessTools.Field(typeof(PrivateArea), "m_allAreas");

        private static readonly FieldInfo DoorZNetViewField =
            AccessTools.Field(typeof(Door), "m_nview");

        private static readonly MethodInfo DoorUpdateStateMethod =
            AccessTools.Method(typeof(Door), "UpdateState");

        private readonly Dictionary<ZDOID, ScheduledDoorClose> _scheduledDoorClosures =
            new Dictionary<ZDOID, ScheduledDoorClose>();

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

            zNetView.Register<long, int>(
                SetDoorAutoCloseSecondsRpc,
                delegate (long sender, long playerId, int seconds)
                {
                    RPC_SetDoorAutoCloseSeconds(
                        privateArea,
                        zNetView,
                        sender,
                        playerId,
                        seconds);
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

        public void Update()
        {
            if (_scheduledDoorClosures.Count <= 0)
                return;

            float time = Time.time;
            List<ZDOID> dueDoorIds = new List<ZDOID>();

            foreach (KeyValuePair<ZDOID, ScheduledDoorClose> scheduledDoor in _scheduledDoorClosures)
            {
                if (time >= scheduledDoor.Value.DueTime)
                    dueDoorIds.Add(scheduledDoor.Key);
            }

            for (int i = 0; i < dueDoorIds.Count; i++)
            {
                ZDOID doorId = dueDoorIds[i];
                ScheduledDoorClose scheduledDoor;

                if (!_scheduledDoorClosures.TryGetValue(doorId, out scheduledDoor))
                    continue;

                CloseScheduledDoor(scheduledDoor);
                _scheduledDoorClosures.Remove(doorId);
            }
        }

        public bool GetDoorLockEnabled(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            return zdo != null &&
                   zdo.GetBool(
                       TerritoryZdoKeys.DoorLockEnabled,
                       false);
        }

        public int GetDoorAutoCloseSeconds(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return ConfigValues.DoorAutoCloseSeconds;

            return NormalizeDoorAutoCloseSeconds(
                zdo.GetInt(
                    TerritoryZdoKeys.DoorAutoCloseSeconds,
                    ConfigValues.DoorAutoCloseSeconds));
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

        public bool RequestSetDoorAutoCloseSeconds(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int seconds)
        {
            if (!ValidateCreatorAction(
                    "SetDoorAutoCloseSeconds",
                    wardId,
                    privateArea,
                    player))
            {
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryRules] SetDoorAutoCloseSeconds ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            int normalizedSeconds = NormalizeDoorAutoCloseSeconds(seconds);

            zNetView.InvokeRPC(
                SetDoorAutoCloseSecondsRpc,
                player.GetPlayerID(),
                normalizedSeconds);

            ModLog.Info("[TerritoryRules] Door auto-close seconds change requested: " + wardId + ", seconds: " + normalizedSeconds);
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

        public bool IsDoorLockEnabledAt(Vector3 position)
        {
            return FindDoorLockArea(position) != null;
        }

        public bool IsStructureDamageProtected(Vector3 position)
        {
            return FindStructureDamageProtectionArea(position) != null;
        }

        public bool TryBlockStructureDamage(Vector3 position)
        {
            PrivateArea privateArea = FindStructureDamageProtectionArea(position);

            if (privateArea == null)
                return false;

            FlashStructureProtectionShield(
                privateArea,
                position);

            return true;
        }

        public void ScheduleDoorAutoClose(Door door)
        {
            if (door == null)
                return;

            ZNetView zNetView = GetDoorZNetView(door);

            if (zNetView == null || !zNetView.IsValid() || !zNetView.IsOwner())
                return;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return;

            PrivateArea privateArea = FindDoorLockArea(door.transform.position);

            if (privateArea == null)
            {
                _scheduledDoorClosures.Remove(zdo.m_uid);
                return;
            }

            if (zdo.GetInt(ZDOVars.s_state) == 0)
            {
                _scheduledDoorClosures.Remove(zdo.m_uid);
                return;
            }

            int seconds = GetDoorAutoCloseSeconds(privateArea);
            float dueTime = Time.time + seconds;

            _scheduledDoorClosures[zdo.m_uid] =
                new ScheduledDoorClose(
                    door,
                    dueTime);

            ModLog.Debug(
                "[TerritoryRules] Door auto-close scheduled: " +
                zdo.m_uid +
                ", seconds: " +
                seconds);
        }

        private void RPC_SetDoorLock(
            PrivateArea privateArea,
            ZNetView zNetView,
            long sender,
            long playerId,
            bool enabled)
        {
            if (SetBoolRuleOnOwner(
                    "DoorLock",
                    TerritoryZdoKeys.DoorLockEnabled,
                    privateArea,
                    zNetView,
                    playerId,
                    enabled) &&
                enabled)
            {
                ScheduleOpenDoorsInsideTerritory(privateArea);
            }
        }

        private void RPC_SetDoorAutoCloseSeconds(
            PrivateArea privateArea,
            ZNetView zNetView,
            long sender,
            long playerId,
            int seconds)
        {
            if (SetIntRuleOnOwner(
                    "DoorAutoCloseSeconds",
                    TerritoryZdoKeys.DoorAutoCloseSeconds,
                    privateArea,
                    zNetView,
                    playerId,
                    NormalizeDoorAutoCloseSeconds(seconds)) &&
                GetDoorLockEnabled(privateArea))
            {
                ScheduleOpenDoorsInsideTerritory(privateArea);
            }
        }

        private void RPC_SetStructureDamageProtection(
            PrivateArea privateArea,
            ZNetView zNetView,
            long sender,
            long playerId,
            bool enabled)
        {
            SetBoolRuleOnOwner(
                "StructureDamageProtection",
                TerritoryZdoKeys.StructureDamageProtectionEnabled,
                privateArea,
                zNetView,
                playerId,
                enabled);
        }

        private void ScheduleOpenDoorsInsideTerritory(PrivateArea privateArea)
        {
            if (privateArea == null)
                return;

            Door[] doors = UnityEngine.Object.FindObjectsByType<Door>(
                FindObjectsSortMode.None);

            for (int i = 0; i < doors.Length; i++)
            {
                Door door = doors[i];

                if (door == null)
                    continue;

                if (!IsInside(privateArea, door.transform.position))
                    continue;

                ScheduleDoorAutoClose(door);
            }
        }

        private void CloseScheduledDoor(ScheduledDoorClose scheduledDoor)
        {
            if (scheduledDoor == null || scheduledDoor.Door == null)
                return;

            Door door = scheduledDoor.Door;
            ZNetView zNetView = GetDoorZNetView(door);

            if (zNetView == null || !zNetView.IsValid() || !zNetView.IsOwner())
                return;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return;

            if (zdo.GetInt(ZDOVars.s_state) == 0)
                return;

            if (!IsDoorLockEnabledAt(door.transform.position))
                return;

            zdo.Set(
                ZDOVars.s_state,
                0);

            if (DoorUpdateStateMethod != null)
                DoorUpdateStateMethod.Invoke(door, null);

            ModLog.Debug("[TerritoryRules] Door auto-closed: " + zdo.m_uid);
        }

        private static void FlashStructureProtectionShield(
            PrivateArea privateArea,
            Vector3 damagePosition)
        {
            const float customBubbleRadius = 5f;
            const float vanillaBubbleRadius = 50f;
            const float customBubbleScale = customBubbleRadius / vanillaBubbleRadius;

            if (privateArea == null)
                return;

            if (privateArea.m_flashEffect == null)
            {
                ModLog.Info("[TerritoryRules] Structure damage blocked. Shield feedback skipped because flash effect is null.");
                return;
            }

            GameObject[] effects = privateArea.m_flashEffect.Create(
                damagePosition,
                Quaternion.identity);

            for (int i = 0; i < effects.Length; i++)
            {
                GameObject effect = effects[i];

                if (effect == null)
                    continue;

                effect.transform.localScale =
                    effect.transform.localScale * customBubbleScale;
            }

            ModLog.Info("[TerritoryRules] Structure damage blocked. Local 5m shield bubble shown at protected piece.");
        }

        private static bool SetBoolRuleOnOwner(
            string ruleName,
            string zdoKey,
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool enabled)
        {
            ZDO zdo;

            if (!TryGetOwnerRuleZdo(
                    ruleName,
                    privateArea,
                    zNetView,
                    playerId,
                    out zdo))
            {
                return false;
            }

            zdo.Set(
                zdoKey,
                enabled);

            ModLog.Info("[TerritoryRules] Rule saved: " + ruleName + ", enabled: " + enabled);
            return true;
        }

        private static bool SetIntRuleOnOwner(
            string ruleName,
            string zdoKey,
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            int value)
        {
            ZDO zdo;

            if (!TryGetOwnerRuleZdo(
                    ruleName,
                    privateArea,
                    zNetView,
                    playerId,
                    out zdo))
            {
                return false;
            }

            zdo.Set(
                zdoKey,
                value);

            ModLog.Info("[TerritoryRules] Rule saved: " + ruleName + ", value: " + value);
            return true;
        }

        private static bool TryGetOwnerRuleZdo(
            string ruleName,
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            out ZDO zdo)
        {
            zdo = null;

            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. PrivateArea is null: " + ruleName);
                return false;
            }

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. ZNetView is invalid: " + ruleName);
                return false;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. ZNetView is not owner: " + ruleName);
                return false;
            }

            zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. ZDO is null: " + ruleName);
                return false;
            }

            if (zdo.GetLong(ZDOVars.s_creator, 0L) != playerId)
            {
                ModLog.Debug("[TerritoryRules] RPC ignored. Player is not ward creator: " + ruleName + ", playerId: " + playerId);
                return false;
            }

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

        private static PrivateArea FindDoorLockArea(Vector3 position)
        {
            List<PrivateArea> areas = GetPrivateAreas();

            for (int i = 0; i < areas.Count; i++)
            {
                PrivateArea privateArea = areas[i];

                if (!IsInside(privateArea, position))
                    continue;

                if (GetStaticDoorLockEnabled(privateArea))
                    return privateArea;
            }

            return null;
        }

        private static PrivateArea FindStructureDamageProtectionArea(Vector3 position)
        {
            List<PrivateArea> areas = GetPrivateAreas();

            for (int i = 0; i < areas.Count; i++)
            {
                PrivateArea privateArea = areas[i];

                if (!IsInside(privateArea, position))
                    continue;

                if (GetStaticStructureDamageProtectionEnabled(privateArea))
                    return privateArea;
            }

            return null;
        }

        private static bool GetStaticDoorLockEnabled(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            return zdo != null &&
                   zdo.GetBool(
                       TerritoryZdoKeys.DoorLockEnabled,
                       false);
        }

        private static bool GetStaticStructureDamageProtectionEnabled(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            return zdo != null &&
                   zdo.GetBool(
                       TerritoryZdoKeys.StructureDamageProtectionEnabled,
                       false);
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

        private static ZNetView GetDoorZNetView(Door door)
        {
            if (door == null || DoorZNetViewField == null)
                return null;

            return DoorZNetViewField.GetValue(door) as ZNetView;
        }

        private static int NormalizeDoorAutoCloseSeconds(int seconds)
        {
            if (seconds < 3)
                return 3;

            if (seconds > 10)
                return 10;

            return seconds;
        }

        private sealed class ScheduledDoorClose
        {
            public Door Door { get; private set; }

            public float DueTime { get; private set; }

            public ScheduledDoorClose(
                Door door,
                float dueTime)
            {
                Door = door;
                DueTime = dueTime;
            }
        }
    }
}
