using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Territory;
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

        private static readonly FieldInfo PermittedPlayersField =
            AccessTools.Field(
                typeof(PrivateArea),
                "m_permittedPlayers");

        private static Type _offlineCompanionSetupType;

        private static int _offlineCompanionOwnerHash =
            GetStableHashCode("HC_Owner");

        private static bool _offlineCompanionDetectedLogged;

        private ZNetScene _observedScene;
        private float _nextScanTime;

        public void Handle(
            WardDestroyedEvent eventData)
        {
            if (eventData == null)
                return;

            string wardId =
                eventData.WardId.ToString();

            if (string.IsNullOrEmpty(wardId))
                return;

            ItemDrop[] drops =
                UnityEngine.Object
                    .FindObjectsByType<ItemDrop>(
                        UnityEngine.FindObjectsSortMode.None);

            int released = 0;

            for (int i = 0; i < drops.Length; i++)
            {
                ItemDrop drop =
                    drops[i];

                if (!IsOwnedByWard(
                        drop,
                        wardId))
                {
                    continue;
                }

                if (!ClaimNetworkOwnership(drop))
                    continue;

                ZDO itemZdo =
                    GetItemZdo(drop);

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
            ZNetScene currentScene =
                ZNetScene.instance;

            if (!ReferenceEquals(
                    currentScene,
                    _observedScene))
            {
                _observedScene =
                    currentScene;

                _nextScanTime = 0f;
            }

            if (currentScene == null ||
                ZDOMan.instance == null)
            {
                return;
            }

            if (Time.time < _nextScanTime)
                return;

            _nextScanTime =
                Time.time + ScanInterval;

            List<PrivateArea> privateAreas =
                GetPrivateAreas();

            if (privateAreas.Count == 0)
                return;

            ItemDrop[] drops =
                UnityEngine.Object
                    .FindObjectsByType<ItemDrop>(
                        UnityEngine.FindObjectsSortMode.None);

            int assigned = 0;

            for (int i = 0;
                 i < drops.Length &&
                 assigned < MaxAssignmentsPerScan;
                 i++)
            {
                ItemDrop drop =
                    drops[i];

                if (!CanAssign(drop))
                    continue;

                if (!string.IsNullOrEmpty(
                        GetTerritoryWardId(drop)))
                {
                    continue;
                }

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

                ZDO wardZdo =
                    wardView.GetZDO();

                if (wardZdo == null)
                    continue;

                if (!ClaimNetworkOwnership(drop))
                    continue;

                ZDO itemZdo =
                    GetItemZdo(drop);

                if (itemZdo == null)
                    continue;

                string wardId =
                    wardZdo.m_uid.ToString();

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

        public static bool CanHumanoidPickup(
            ItemDrop drop,
            Humanoid humanoid)
        {
            if (drop == null ||
                humanoid == null)
            {
                return true;
            }

            string wardId =
                GetTerritoryWardId(drop);

            if (string.IsNullOrEmpty(wardId))
                return true;

            PrivateArea privateArea =
                FindPrivateAreaByWardId(
                    wardId);

            if (privateArea == null)
                return false;

            long playerId;

            Player player =
                humanoid as Player;

            if (player != null)
            {
                playerId =
                    player.GetPlayerID();
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

            return EnsurePickupOwnership(drop);
        }

        public static bool IsOwnedByPrivateArea(
            ItemDrop drop,
            PrivateArea privateArea)
        {
            if (drop == null ||
                privateArea == null)
            {
                return false;
            }

            ZDO wardZdo =
                GetWardZdo(privateArea);

            if (wardZdo == null)
                return false;

            return IsOwnedByWard(
                drop,
                wardZdo.m_uid.ToString());
        }

        public static bool IsOwnedByWard(
            ItemDrop drop,
            string wardId)
        {
            if (drop == null ||
                string.IsNullOrEmpty(wardId))
            {
                return false;
            }

            return string.Equals(
                GetTerritoryWardId(drop),
                wardId,
                StringComparison.Ordinal);
        }

        public static string GetTerritoryWardId(
            ItemDrop drop)
        {
            ZDO itemZdo =
                GetItemZdo(drop);

            if (itemZdo == null)
                return "";

            return itemZdo.GetString(
                TerritoryZdoKeys.ItemTerritoryWardId,
                "");
        }

        private static bool CanAssign(
            ItemDrop drop)
        {
            if (drop == null ||
                drop.gameObject == null ||
                !drop.gameObject.activeInHierarchy ||
                drop.m_itemData == null ||
                drop.m_itemData.m_stack <= 0)
            {
                return false;
            }

            return !drop.IsPiece();
        }

        private static bool ClaimNetworkOwnership(
            ItemDrop drop)
        {
            if (drop == null)
                return false;

            ZNetView zNetView =
                drop.GetComponent<ZNetView>();

            if (zNetView == null ||
                !zNetView.IsValid())
            {
                return false;
            }

            if (!zNetView.IsOwner())
                zNetView.ClaimOwnership();

            return zNetView.IsOwner();
        }

        private static bool EnsurePickupOwnership(
            ItemDrop drop)
        {
            return ClaimNetworkOwnership(drop) &&
                   drop.CanPickup(false);
        }

        private static ZDO GetItemZdo(
            ItemDrop drop)
        {
            if (drop == null)
                return null;

            ZNetView zNetView =
                drop.GetComponent<ZNetView>();

            if (zNetView == null ||
                !zNetView.IsValid())
            {
                return null;
            }

            return zNetView.GetZDO();
        }

        private static ZDO GetWardZdo(
            PrivateArea privateArea)
        {
            if (privateArea == null)
                return null;

            ZNetView zNetView =
                privateArea.GetComponent<ZNetView>();

            if (zNetView == null ||
                !zNetView.IsValid())
            {
                return null;
            }

            return zNetView.GetZDO();
        }

        private static bool HasTerritoryAccess(
            PrivateArea privateArea,
            long playerId)
        {
            if (privateArea == null ||
                playerId == 0L)
            {
                return false;
            }

            ZDO wardZdo =
                GetWardZdo(privateArea);

            if (wardZdo == null)
                return false;

            long creatorId =
                wardZdo.GetLong(
                    ZDOVars.s_creator,
                    0L);

            if (creatorId == playerId)
                return true;

            if (IsPermittedPlayer(
                    privateArea,
                    playerId))
            {
                return true;
            }

            string wardGuildId =
                wardZdo.GetString(
                    TerritoryZdoKeys.WardGuildId,
                    "");

            if (string.IsNullOrEmpty(
                    wardGuildId))
            {
                return false;
            }

            IGuildService guildService;

            if (!ServiceContainer.TryGet<
                    IGuildService>(
                    out guildService) ||
                guildService == null ||
                !guildService.IsAvailable)
            {
                return false;
            }

            string playerGuildId;

            return guildService.TryGetPlayerGuildId(
                       playerId,
                       out playerGuildId) &&
                   !string.IsNullOrEmpty(
                       playerGuildId) &&
                   string.Equals(
                       wardGuildId,
                       playerGuildId,
                       StringComparison.Ordinal);
        }

        private static bool IsPermittedPlayer(
            PrivateArea privateArea,
            long playerId)
        {
            if (privateArea == null ||
                playerId == 0L ||
                PermittedPlayersField == null)
            {
                return false;
            }

            object rawValue;

            try
            {
                rawValue =
                    PermittedPlayersField.GetValue(
                        privateArea);
            }
            catch (Exception)
            {
                return false;
            }

            if (rawValue == null)
                return false;

            IDictionary<long, string> genericDictionary =
                rawValue as IDictionary<long, string>;

            if (genericDictionary != null)
            {
                return genericDictionary.ContainsKey(
                    playerId);
            }

            IEnumerable<KeyValuePair<long, string>>
                genericPairs =
                    rawValue as IEnumerable<
                        KeyValuePair<long, string>>;

            if (genericPairs != null)
            {
                foreach (KeyValuePair<long, string> pair
                         in genericPairs)
                {
                    if (pair.Key == playerId)
                        return true;
                }

                return false;
            }

            IDictionary dictionary =
                rawValue as IDictionary;

            if (dictionary != null)
            {
                return dictionary.Contains(
                    playerId);
            }

            IEnumerable enumerable =
                rawValue as IEnumerable;

            if (enumerable == null)
                return false;

            foreach (object entry in enumerable)
            {
                if (entry == null)
                    continue;

                Type entryType =
                    entry.GetType();

                PropertyInfo keyProperty =
                    entryType.GetProperty(
                        "Key",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                if (keyProperty == null)
                    continue;

                object keyValue;

                try
                {
                    keyValue =
                        keyProperty.GetValue(
                            entry,
                            null);
                }
                catch (Exception)
                {
                    continue;
                }

                if (keyValue is long &&
                    (long)keyValue == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetOfflineCompanionOwnerId(
            Humanoid humanoid,
            out long ownerId)
        {
            ownerId = 0L;

            if (humanoid == null)
                return false;

            Type setupType =
                ResolveOfflineCompanionSetupType();

            if (setupType == null)
                return false;

            Component setup =
                humanoid.GetComponent(
                    setupType);

            if (setup == null)
                return false;

            ZNetView zNetView =
                setup.GetComponent<ZNetView>();

            if (zNetView == null ||
                !zNetView.IsValid())
            {
                return false;
            }

            ZDO zdo =
                zNetView.GetZDO();

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

            for (int i = 0;
                 i < assemblies.Length;
                 i++)
            {
                Type type =
                    assemblies[i].GetType(
                        OfflineCompanionSetupTypeName,
                        false);

                if (type == null)
                    continue;

                _offlineCompanionSetupType =
                    type;

                _offlineCompanionOwnerHash =
                    ResolveOfflineCompanionOwnerHash(
                        type);

                if (!_offlineCompanionDetectedLogged)
                {
                    _offlineCompanionDetectedLogged = true;

                    ModLog.Info(
                        "[TerritoryResources] Offline Companions integration detected.");
                }

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

            return GetStableHashCode(
                "HC_Owner");
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

        private static PrivateArea FindPrivateAreaByWardId(
            string wardId)
        {
            if (string.IsNullOrEmpty(wardId))
                return null;

            List<PrivateArea> privateAreas =
                GetPrivateAreas();

            for (int i = 0;
                 i < privateAreas.Count;
                 i++)
            {
                PrivateArea privateArea =
                    privateAreas[i];

                ZDO wardZdo =
                    GetWardZdo(privateArea);

                if (wardZdo == null)
                    continue;

                if (string.Equals(
                        wardZdo.m_uid.ToString(),
                        wardId,
                        StringComparison.Ordinal))
                {
                    return privateArea;
                }
            }

            return null;
        }

        private static PrivateArea FindTerritoryAt(
            List<PrivateArea> privateAreas,
            Vector3 position)
        {
            PrivateArea nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0;
                 i < privateAreas.Count;
                 i++)
            {
                PrivateArea privateArea =
                    privateAreas[i];

                if (privateArea == null)
                    continue;

                float distance =
                    global::Utils.DistanceXZ(
                        privateArea.transform.position,
                        position);

                if (distance >
                    privateArea.m_radius)
                {
                    continue;
                }

                if (distance >=
                    nearestDistance)
                {
                    continue;
                }

                nearest =
                    privateArea;

                nearestDistance =
                    distance;
            }

            return nearest;
        }

        private static int GetStableHashCode(
            string value)
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

                    if (i ==
                            value.Length - 1 ||
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

    internal static class TerritoryResourceOwnershipRuntime
    {
        private static TerritoryResourceOwnershipService _service;
        private static GameObject _runnerObject;
        private static bool _eventBusSubscribed;

        public static TerritoryResourceOwnershipService Service
        {
            get
            {
                EnsureStarted();
                return _service;
            }
        }

        public static void EnsureStarted()
        {
            if (_service == null)
            {
                _service =
                    new TerritoryResourceOwnershipService();

                ServiceContainer.Register<
                    TerritoryResourceOwnershipService>(
                        _service);
            }

            if (!_eventBusSubscribed)
            {
                EventBus eventBus;

                if (ServiceContainer.TryGet<EventBus>(
                        out eventBus) &&
                    eventBus != null)
                {
                    eventBus.Subscribe<
                        WardDestroyedEvent>(
                            _service);

                    _eventBusSubscribed = true;
                }
            }

            if (_runnerObject != null)
                return;

            _runnerObject =
                new GameObject(
                    "ClanTerritory_TerritoryResourceOwnershipRunner");

            UnityEngine.Object.DontDestroyOnLoad(
                _runnerObject);

            _runnerObject.AddComponent<
                TerritoryResourceOwnershipRunner>();

            ModLog.Info(
                "[TerritoryResources] Territory-owned item runtime started.");
        }

        public static void Update()
        {
            if (_service != null)
                _service.Update();
        }
    }

    internal sealed class TerritoryResourceOwnershipRunner :
        MonoBehaviour
    {
        private void Update()
        {
            TerritoryResourceOwnershipRuntime.Update();
        }
    }
}

namespace ClanTerritory.Integration.Valheim.Harmony
{
    using ClanTerritory.Features.Territory.Services;

    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class TerritoryResourceOwnershipRuntimeStartHook
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            TerritoryResourceOwnershipRuntime
                .EnsureStarted();
        }
    }

    [HarmonyPatch(
        typeof(ItemDrop),
        "Pickup",
        new Type[] { typeof(Humanoid) })]
    internal static class TerritoryItemDropPickupHook
    {
        [HarmonyPrefix]
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

            Player player =
                __0 as Player;

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
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo[] methods =
                typeof(Humanoid).GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            for (int i = 0;
                 i < methods.Length;
                 i++)
            {
                MethodInfo method =
                    methods[i];

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

        [HarmonyPrefix]
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

    [HarmonyPatch(
        typeof(TerritoryTerraformingService),
        "IsAbsorbableGroundItem")]
    internal static class TerritoryResourceAbsorptionOwnershipHook
    {
        [HarmonyPostfix]
        private static void Postfix(
            ItemDrop drop,
            PrivateArea privateArea,
            ref bool __result)
        {
            if (!__result)
                return;

            __result =
                TerritoryResourceOwnershipService
                    .IsOwnedByPrivateArea(
                        drop,
                        privateArea);
        }
    }
}
