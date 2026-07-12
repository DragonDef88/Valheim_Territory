using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ClanTerritory.Events;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryResourceOwnershipService :
        IEventHandler<WardDestroyedEvent>
    {
        private const float ScanInterval = 0.5f;
        private const int MaxAssignmentsPerScan = 64;
        private const string OfflineCompanionSetupTypeName =
            "Companions.CompanionSetup";

        private static readonly FieldInfo AllPrivateAreasField =
            AccessTools.Field(
                typeof(PrivateArea),
                "m_allAreas");

        private static Type _offlineCompanionSetupType;
        private static int _offlineCompanionOwnerHash =
            GetStableHashCode("HC_Owner");

        private float _nextScanTime;

        public void Handle(WardDestroyedEvent eventData)
        {
            if (eventData == null)
                return;

            string wardId = eventData.WardId.ToString();

            if (string.IsNullOrEmpty(wardId))
                return;

            ItemDrop[] drops =
                UnityEngine.Object.FindObjectsByType<ItemDrop>(
                    UnityEngine.FindObjectsSortMode.None);

            int released = 0;

            for (int i = 0; i < drops.Length; i++)
            {
                ItemDrop drop = drops[i];

                if (!IsOwnedByWard(drop, wardId))
                    continue;

                if (!EnsureNetworkOwnership(drop))
                    continue;

                ZDO itemZdo = GetItemZdo(drop);

                if (itemZdo == null)
                    continue;

                itemZdo.Set(
                    TerritoryZdoKeys.ItemTerritoryWardId,
                    "");

                released++;
            }

            if (released > 0)
            {
                ModLog.Info(
                    "[TerritoryResources] Released ground items after ward destruction: " +
                    released +
                    ", ward: " +
                    wardId);
            }
        }

        public void Update()
        {
            if (Time.time < _nextScanTime)
                return;

            _nextScanTime = Time.time + ScanInterval;

            List<PrivateArea> privateAreas = GetPrivateAreas();

            if (privateAreas.Count == 0)
                return;

            ItemDrop[] drops =
                UnityEngine.Object.FindObjectsByType<ItemDrop>(
                    UnityEngine.FindObjectsSortMode.None);

            int assigned = 0;

            for (int i = 0;
                 i < drops.Length && assigned < MaxAssignmentsPerScan;
                 i++)
            {
                ItemDrop drop = drops[i];

                if (!CanAssign(drop))
                    continue;

                if (!string.IsNullOrEmpty(GetTerritoryWardId(drop)))
                    continue;

                PrivateArea privateArea =
                    FindTerritoryAt(
                        privateAreas,
                        drop.transform.position);

                if (privateArea == null)
                    continue;

                ZNetView wardView =
                    privateArea.GetComponent<ZNetView>();

                if (wardView == null ||
                    !wardView.IsValid() ||
                    !wardView.IsOwner())
                {
                    continue;
                }

                ZDO wardZdo = wardView.GetZDO();

                if (wardZdo == null)
                    continue;

                if (!EnsureNetworkOwnership(drop))
                    continue;

                ZDO itemZdo = GetItemZdo(drop);

                if (itemZdo == null)
                    continue;

                string wardId = wardZdo.m_uid.ToString();

                itemZdo.Set(
                    TerritoryZdoKeys.ItemTerritoryWardId,
                    wardId);

                assigned++;

                ModLog.Debug(
                    "[TerritoryResources] Ground item assigned to ward: " +
                    wardId +
                    ", item: " +
                    drop.gameObject.name);
            }
        }

        public static bool IsOwnedByWard(
            ItemDrop drop,
            string wardId)
        {
            if (drop == null || string.IsNullOrEmpty(wardId))
                return false;

            return string.Equals(
                GetTerritoryWardId(drop),
                wardId,
                StringComparison.Ordinal);
        }

        public static bool EnsureNetworkOwnership(ItemDrop drop)
        {
            if (drop == null)
                return false;

            ZNetView zNetView =
                drop.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return drop.CanPickup(false);

            if (!zNetView.IsOwner())
                zNetView.ClaimOwnership();

            return zNetView.IsOwner() &&
                   drop.CanPickup(false);
        }

        public static bool CanHumanoidPickup(
            ItemDrop drop,
            Humanoid humanoid)
        {
            if (drop == null || humanoid == null)
                return true;

            string wardId = GetTerritoryWardId(drop);

            if (string.IsNullOrEmpty(wardId))
                return true;

            PrivateArea privateArea =
                TerritoryGuildAccess.FindPrivateAreaByWardId(
                    wardId);

            if (privateArea == null)
                return false;

            long playerId;

            Player player = humanoid as Player;

            if (player != null)
            {
                playerId = player.GetPlayerID();
            }
            else if (!TryGetOfflineCompanionOwnerId(
                         humanoid,
                         out playerId))
            {
                return false;
            }

            if (!HasTerritoryAccess(
                    privateArea,
                    playerId))
            {
                return false;
            }

            return EnsureNetworkOwnership(drop);
        }

        public static string GetTerritoryWardId(ItemDrop drop)
        {
            ZDO itemZdo = GetItemZdo(drop);

            if (itemZdo == null)
                return "";

            return itemZdo.GetString(
                TerritoryZdoKeys.ItemTerritoryWardId,
                "");
        }

        private static bool CanAssign(ItemDrop drop)
        {
            if (drop == null ||
                drop.m_itemData == null ||
                drop.m_itemData.m_stack <= 0)
            {
                return false;
            }

            return !drop.IsPiece();
        }

        private static ZDO GetItemZdo(ItemDrop drop)
        {
            if (drop == null)
                return null;

            ZNetView zNetView =
                drop.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return null;

            return zNetView.GetZDO();
        }

        private static bool HasTerritoryAccess(
            PrivateArea privateArea,
            long playerId)
        {
            if (privateArea == null || playerId == 0L)
                return false;

            ZNetView zNetView =
                privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return false;

            ZDO wardZdo = zNetView.GetZDO();

            if (wardZdo == null)
                return false;

            long creatorId =
                wardZdo.GetLong(
                    ZDOVars.s_creator,
                    0L);

            if (creatorId == playerId)
                return true;

            List<KeyValuePair<long, string>> permittedPlayers =
                privateArea.GetPermittedPlayers();

            for (int i = 0; i < permittedPlayers.Count; i++)
            {
                if (permittedPlayers[i].Key == playerId)
                    return true;
            }

            return TerritoryGuildAccess.HasGuildAccess(
                wardZdo,
                playerId);
        }

        private static bool TryGetOfflineCompanionOwnerId(
            Humanoid humanoid,
            out long ownerId)
        {
            ownerId = 0L;

            Type setupType =
                ResolveOfflineCompanionSetupType();

            if (setupType == null)
                return false;

            Component setup =
                humanoid.GetComponent(setupType);

            if (setup == null)
                return false;

            ZNetView zNetView =
                setup.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return false;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return false;

            string rawOwnerId =
                zdo.GetString(
                    _offlineCompanionOwnerHash,
                    "");

            return long.TryParse(
                rawOwnerId,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out ownerId) &&
                ownerId != 0L;
        }

        private static Type ResolveOfflineCompanionSetupType()
        {
            if (_offlineCompanionSetupType != null)
                return _offlineCompanionSetupType;

            Assembly[] assemblies =
                AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type =
                    assemblies[i].GetType(
                        OfflineCompanionSetupTypeName,
                        false);

                if (type == null)
                    continue;

                _offlineCompanionSetupType = type;
                _offlineCompanionOwnerHash =
                    ResolveOfflineCompanionOwnerHash(type);

                ModLog.Info(
                    "[TerritoryResources] Offline Companions 1.3.0 integration detected.");

                return _offlineCompanionSetupType;
            }

            return null;
        }

        private static int ResolveOfflineCompanionOwnerHash(
            Type setupType)
        {
            BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo field =
                setupType.GetField(
                    "OwnerHash",
                    flags);

            if (field != null &&
                field.FieldType == typeof(int))
            {
                object value =
                    field.GetValue(null);

                if (value is int)
                    return (int)value;
            }

            PropertyInfo property =
                setupType.GetProperty(
                    "OwnerHash",
                    flags);

            if (property != null &&
                property.PropertyType == typeof(int))
            {
                object value =
                    property.GetValue(
                        null,
                        null);

                if (value is int)
                    return (int)value;
            }

            return GetStableHashCode("HC_Owner");
        }

        private static List<PrivateArea> GetPrivateAreas()
        {
            if (AllPrivateAreasField == null)
                return new List<PrivateArea>();

            List<PrivateArea> privateAreas =
                AllPrivateAreasField.GetValue(null)
                as List<PrivateArea>;

            if (privateAreas == null)
                return new List<PrivateArea>();

            return privateAreas;
        }

        private static PrivateArea FindTerritoryAt(
            List<PrivateArea> privateAreas,
            Vector3 position)
        {
            PrivateArea nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < privateAreas.Count; i++)
            {
                PrivateArea privateArea =
                    privateAreas[i];

                if (privateArea == null)
                    continue;

                float distance =
                    global::Utils.DistanceXZ(
                        privateArea.transform.position,
                        position);

                if (distance > privateArea.m_radius)
                    continue;

                if (distance >= nearestDistance)
                    continue;

                nearest = privateArea;
                nearestDistance = distance;
            }

            return nearest;
        }

        private static int GetStableHashCode(string value)
        {
            if (value == null)
                return 0;

            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0;
                     i < value.Length &&
                     value[i] != '\0';
                     i += 2)
                {
                    hash1 =
                        ((hash1 << 5) + hash1) ^
                        value[i];

                    if (i == value.Length - 1 ||
                        value[i + 1] == '\0')
                    {
                        break;
                    }

                    hash2 =
                        ((hash2 << 5) + hash2) ^
                        value[i + 1];
                }

                return hash1 +
                       hash2 * 1566083941;
            }
        }
    }
}

namespace ClanTerritory.Integration.Valheim.Harmony
{
    using ClanTerritory.Features.Territory.Services;

    [HarmonyPatch(
        typeof(ItemDrop),
        "Pickup",
        new[] { typeof(Humanoid) })]
    internal static class TerritoryItemDropPickupHook
    {
        private static bool Prefix(
            ItemDrop __instance,
            Humanoid __0)
        {
            if (TerritoryResourceOwnershipService
                .CanHumanoidPickup(
                    __instance,
                    __0))
            {
                return true;
            }

            Player player = __0 as Player;

            if (player != null)
            {
                player.Message(
                    MessageHud.MessageType.Center,
                    "$msg_privatezone");
            }

            return false;
        }
    }

    [HarmonyPatch]
    internal static class TerritoryHumanoidPickupHook
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo[] methods =
                typeof(Humanoid).GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                if (!string.Equals(
                        method.Name,
                        "Pickup",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters =
                    method.GetParameters();

                if (parameters.Length == 0 ||
                    parameters[0].ParameterType !=
                    typeof(GameObject))
                {
                    continue;
                }

                yield return method;
            }
        }

        private static bool Prefix(
            Humanoid __instance,
            object[] __args)
        {
            if (__args == null ||
                __args.Length == 0)
            {
                return true;
            }

            GameObject gameObject =
                __args[0] as GameObject;

            if (gameObject == null)
                return true;

            ItemDrop drop =
                gameObject.GetComponent<ItemDrop>();

            if (drop == null)
            {
                drop =
                    gameObject.GetComponentInParent<ItemDrop>();
            }

            if (drop == null)
                return true;

            return TerritoryResourceOwnershipService
                .CanHumanoidPickup(
                    drop,
                    __instance);
        }
    }
}
