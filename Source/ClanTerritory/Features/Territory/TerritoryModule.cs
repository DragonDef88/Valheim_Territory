using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Abstractions;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Map.Services;
using ClanTerritory.Features.Territory.Events;
using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Placement;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.Territory.Zdo;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory
{
    internal sealed class TerritoryModule :
        IInitializable,
        IDisposableModule
    {
        private TerritoryRegistry _registry;
        private TerritoryFactory _factory;
        private TerritoryService _service;
        private TerritoryZdoService _zdoService;
        private WardMapIconService _mapIconService;
        private TerritoryWardRadiusService _wardRadiusService;
        private TerritoryRuleService _ruleService;
        private TerritoryTerraformingService _terraformingService;
        private TerritoryPresenceService _presenceService;
        private GameObject _runnerObject;
        private TerritoryRunner _runner;

        public void Initialize()
        {
            _registry = new TerritoryRegistry();
            _factory = new TerritoryFactory();
            _zdoService = new TerritoryZdoService();

            EventBus eventBus = ServiceContainer.Get<EventBus>();

            _wardRadiusService = new TerritoryWardRadiusService(eventBus);
            _ruleService = new TerritoryRuleService();
            _terraformingService = new TerritoryTerraformingService();
            _presenceService = new TerritoryPresenceService();

            _mapIconService = new WardMapIconService(
                _zdoService,
                _registry);

            _mapIconService.Initialize();

            _service = new TerritoryService(
                _registry,
                _factory,
                _mapIconService);

            ServiceContainer.Register<TerritoryRegistry>(_registry);
            ServiceContainer.Register<TerritoryZdoService>(_zdoService);
            ServiceContainer.Register<TerritoryWardRadiusService>(_wardRadiusService);
            ServiceContainer.Register<TerritoryRuleService>(_ruleService);
            ServiceContainer.Register<TerritoryTerraformingService>(_terraformingService);
            ServiceContainer.Register<WardMapIconService>(_mapIconService);
            ServiceContainer.Register<ITerritoryService>(_service);

            IWardPlacementPolicy placementPolicy = WardPlacementPolicyFactory.Create(_zdoService);
            ServiceContainer.Register<IWardPlacementPolicy>(placementPolicy);

            if (ServiceContainer.TryGet<EventBus>(out eventBus))
            {
                eventBus.Subscribe<WardRegisteredEvent>(_service);
                eventBus.Subscribe<WardDestroyedEvent>(_service);
                eventBus.Subscribe<TerritoryRadiusChangedEvent>(_service);
            }

            _runnerObject = new GameObject("ClanTerritory_TerritoryRunner");
            Object.DontDestroyOnLoad(_runnerObject);

            _runner = _runnerObject.AddComponent<TerritoryRunner>();
            _runner.Initialize(
                _ruleService,
                _terraformingService,
                _presenceService);

            ModLog.Info("Territory module initialized.");
        }

        public void Shutdown()
        {
            if (_mapIconService != null)
                _mapIconService.RemoveAll();

            if (_registry != null)
                _registry.Clear();

            if (_runnerObject != null)
                Object.Destroy(_runnerObject);

            _runner = null;
            _runnerObject = null;
            _presenceService = null;
            _terraformingService = null;
            _ruleService = null;

            ModLog.Info("Territory module shutdown.");
        }

        private sealed class TerritoryRunner : MonoBehaviour
        {
            private TerritoryRuleService _ruleService;
            private TerritoryTerraformingService _terraformingService;
            private TerritoryPresenceService _presenceService;

            public void Initialize(
                TerritoryRuleService ruleService,
                TerritoryTerraformingService terraformingService,
                TerritoryPresenceService presenceService)
            {
                _ruleService = ruleService;
                _terraformingService = terraformingService;
                _presenceService = presenceService;
            }

            private void Update()
            {
                if (_ruleService != null)
                    _ruleService.Update();

                if (_terraformingService != null)
                    _terraformingService.Update();

                if (_presenceService != null)
                    _presenceService.Update();
            }
        }

        private sealed class TerritoryPresenceService
        {
            private const float CheckInterval = 0.5f;

            private static readonly FieldInfo AllAreasField =
                AccessTools.Field(typeof(PrivateArea), "m_allAreas");

            private string _currentWardId = "";
            private string _currentTerritoryName = "";
            private float _nextCheckTime;

            public void Update()
            {
                if (Time.time < _nextCheckTime)
                    return;

                _nextCheckTime = Time.time + CheckInterval;

                Player player = Player.m_localPlayer;

                if (player == null)
                {
                    ClearCurrentTerritory();
                    return;
                }

                PrivateArea currentArea = FindCurrentTerritoryArea(player.transform.position);

                if (currentArea == null)
                {
                    if (!string.IsNullOrEmpty(_currentWardId))
                    {
                        ShowMessage(player, "Left territory: " + _currentTerritoryName);
                        ModLog.Info("[TerritoryPresence] Left territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
                    }

                    ClearCurrentTerritory();
                    return;
                }

                string wardId = GetWardId(currentArea);

                if (string.IsNullOrEmpty(wardId))
                    return;

                if (wardId == _currentWardId)
                    return;

                if (!string.IsNullOrEmpty(_currentWardId))
                {
                    ShowMessage(player, "Left territory: " + _currentTerritoryName);
                    ModLog.Info("[TerritoryPresence] Left territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
                }

                _currentWardId = wardId;
                _currentTerritoryName = GetTerritoryName(currentArea);

                ShowMessage(player, "Entered territory: " + _currentTerritoryName);
                ModLog.Info("[TerritoryPresence] Entered territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
            }

            private static PrivateArea FindCurrentTerritoryArea(Vector3 position)
            {
                List<PrivateArea> areas = GetPrivateAreas();
                PrivateArea nearestArea = null;
                float nearestDistance = float.MaxValue;

                for (int i = 0; i < areas.Count; i++)
                {
                    PrivateArea privateArea = areas[i];

                    if (privateArea == null)
                        continue;

                    if (!IsInside(privateArea, position))
                        continue;

                    float distance = global::Utils.DistanceXZ(privateArea.transform.position, position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestArea = privateArea;
                    }
                }

                return nearestArea;
            }

            private static bool IsInside(PrivateArea privateArea, Vector3 position)
            {
                if (privateArea == null)
                    return false;

                return global::Utils.DistanceXZ(privateArea.transform.position, position) < privateArea.m_radius;
            }

            private static string GetWardId(PrivateArea privateArea)
            {
                if (privateArea == null)
                    return "";

                ZNetView zNetView = privateArea.GetComponent<ZNetView>();

                if (zNetView == null || !zNetView.IsValid())
                    return "";

                ZDO zdo = zNetView.GetZDO();

                if (zdo == null)
                    return "";

                return zdo.m_uid.ToString();
            }

            private static string GetTerritoryName(PrivateArea privateArea)
            {
                ITerritoryNamingService namingService;

                if (ServiceContainer.TryGet<ITerritoryNamingService>(out namingService))
                {
                    string name = namingService.GetTerritoryName(privateArea);

                    if (!string.IsNullOrWhiteSpace(name))
                        return name;
                }

                return "Unnamed Territory";
            }

            private static void ShowMessage(Player player, string message)
            {
                if (player == null)
                    return;

                player.Message(MessageHud.MessageType.Center, message);
            }

            private static List<PrivateArea> GetPrivateAreas()
            {
                if (AllAreasField == null)
                    return new List<PrivateArea>();

                List<PrivateArea> areas = AllAreasField.GetValue(null) as List<PrivateArea>;

                if (areas == null)
                    return new List<PrivateArea>();

                return areas;
            }

            private void ClearCurrentTerritory()
            {
                _currentWardId = "";
                _currentTerritoryName = "";
            }
        }
    }
}

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryTerraformingService
    {
        private const string SetEnabledRpc = "CT_SetTerraformingEnabled";
        private const string SetRunningRpc = "CT_SetTerraformingRunning";
        private const string SetRadiusRpc = "CT_SetTerraformingRadius";
        private const string SetHoeStoredRpc = "CT_SetTerraformingHoeStored";
        private const string SetPickaxeStoredRpc = "CT_SetTerraformingPickaxeStored";
        private const string AddFuelSlotRpc = "CT_AddTerraformingFuelSlot";
        private const string AddStoneSlotRpc = "CT_AddTerraformingStoneSlot";

        private const float DefaultRadius = 12f;
        private const float MinimumRadius = 2f;
        private const float MaximumRadius = 80f;
        private const float RadiusStep = 2f;
        private const int SlotCapacity = 500;
        private const int FuelSlotCount = 5;
        private const int StoneSlotCount = 5;

        private static readonly string[] FuelPrefabNames =
        {
            "Wood",
            "Coal",
            "Resin"
        };

        private static readonly string[] PickaxePrefabNames =
        {
            "PickaxeAntler",
            "PickaxeBronze",
            "PickaxeIron",
            "PickaxeBlackMetal"
        };

        public void RegisterRpc(PrivateArea privateArea)
        {
            if (privateArea == null)
                return;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || zNetView.GetZDO() == null)
            {
                ModLog.Debug("[TerritoryTerraforming] RPC registration ignored. ZNetView or ZDO is null.");
                return;
            }

            zNetView.Register<long, bool>(
                SetEnabledRpc,
                delegate(long sender, long playerId, bool enabled)
                {
                    RPC_SetEnabled(privateArea, zNetView, playerId, enabled);
                });

            zNetView.Register<long, bool>(
                SetRunningRpc,
                delegate(long sender, long playerId, bool running)
                {
                    RPC_SetRunning(privateArea, zNetView, playerId, running);
                });

            zNetView.Register<long, float>(
                SetRadiusRpc,
                delegate(long sender, long playerId, float radius)
                {
                    RPC_SetRadius(privateArea, zNetView, playerId, radius);
                });

            zNetView.Register<long, bool>(
                SetHoeStoredRpc,
                delegate(long sender, long playerId, bool stored)
                {
                    RPC_SetHoeStored(privateArea, zNetView, playerId, stored);
                });

            zNetView.Register<long, bool>(
                SetPickaxeStoredRpc,
                delegate(long sender, long playerId, bool stored)
                {
                    RPC_SetPickaxeStored(privateArea, zNetView, playerId, stored);
                });

            zNetView.Register<long, int, int>(
                AddFuelSlotRpc,
                delegate(long sender, long playerId, int slotIndex, int amount)
                {
                    RPC_AddFuelSlot(privateArea, zNetView, playerId, slotIndex, amount);
                });

            zNetView.Register<long, int, int>(
                AddStoneSlotRpc,
                delegate(long sender, long playerId, int slotIndex, int amount)
                {
                    RPC_AddStoneSlot(privateArea, zNetView, playerId, slotIndex, amount);
                });

            EnsureDefaults(privateArea);

            ModLog.Debug("[TerritoryTerraforming] RPCs registered for ward.");
        }

        public void Update()
        {
            // The terrain worker will use the ward height as the fixed target height.
            // This package establishes the preparation chest and storage rules first.
        }

        public TerraformingState GetState(PrivateArea privateArea)
        {
            EnsureDefaults(privateArea);

            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return TerraformingState.Disabled();

            int[] fuelSlots = ReadSlots(
                zdo,
                TerritoryZdoKeys.TerraformingFuelSlotPrefix,
                FuelSlotCount);

            int[] stoneSlots = ReadSlots(
                zdo,
                TerritoryZdoKeys.TerraformingStoneSlotPrefix,
                StoneSlotCount);

            return new TerraformingState(
                zdo.GetBool(TerritoryZdoKeys.TerraformingEnabled, false),
                zdo.GetBool(TerritoryZdoKeys.TerraformingRunning, false),
                NormalizeRadius(zdo.GetFloat(TerritoryZdoKeys.TerraformingRadius, DefaultRadius)),
                privateArea != null ? privateArea.transform.position.y : 0f,
                fuelSlots,
                stoneSlots,
                zdo.GetBool(TerritoryZdoKeys.TerraformingHoeStored, false),
                zdo.GetBool(TerritoryZdoKeys.TerraformingPickaxeStored, false),
                Mathf.Max(0f, zdo.GetFloat(TerritoryZdoKeys.TerraformingScanProgress, 0f)),
                Mathf.Max(0, zdo.GetInt(TerritoryZdoKeys.TerraformingScanIndex, 0)));
        }

        public bool RequestToggleEnabled(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetEnabled(
                wardId,
                privateArea,
                player,
                !state.Enabled);
        }

        public bool RequestToggleRunning(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetRunning(
                wardId,
                privateArea,
                player,
                !state.Running);
        }

        public bool RequestDecreaseRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetRadius(
                wardId,
                privateArea,
                player,
                state.Radius - RadiusStep);
        }

        public bool RequestIncreaseRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetRadius(
                wardId,
                privateArea,
                player,
                state.Radius + RadiusStep);
        }

        public bool RequestStoreHoe(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (!TryConsumeTool(player, "Hoe"))
                return false;

            return RequestSetHoeStored(
                wardId,
                privateArea,
                player,
                true);
        }

        public bool RequestStorePickaxe(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (!TryConsumeAnyTool(player, PickaxePrefabNames))
                return false;

            return RequestSetPickaxeStored(
                wardId,
                privateArea,
                player,
                true);
        }

        public bool RequestAddFuelSlot(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int slotIndex)
        {
            if (!ValidateCreatorAction("AddTerraformingFuelSlot", wardId, privateArea, player))
                return false;

            if (!IsValidFuelSlot(slotIndex))
                return false;

            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return false;

            int stored = GetSlot(
                zdo,
                TerritoryZdoKeys.TerraformingFuelSlotPrefix,
                slotIndex);

            int capacity = SlotCapacity - stored;

            if (capacity <= 0)
            {
                player.Message(MessageHud.MessageType.Center, "Fuel slot is full");
                return false;
            }

            int moved = TakeItemsFromPlayer(
                player,
                FuelPrefabNames,
                capacity);

            if (moved <= 0)
            {
                player.Message(MessageHud.MessageType.Center, "No fuel in inventory");
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return false;

            zNetView.InvokeRPC(
                AddFuelSlotRpc,
                player.GetPlayerID(),
                slotIndex,
                moved);

            player.Message(MessageHud.MessageType.Center, "Fuel stored: " + moved);
            ModLog.Info("[TerritoryTerraforming] Add fuel slot requested: " + wardId + ", slot: " + slotIndex + ", amount: " + moved);
            return true;
        }

        public bool RequestAddStoneSlot(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int slotIndex)
        {
            if (!ValidateCreatorAction("AddTerraformingStoneSlot", wardId, privateArea, player))
                return false;

            if (!IsValidStoneSlot(slotIndex))
                return false;

            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return false;

            int stored = GetSlot(
                zdo,
                TerritoryZdoKeys.TerraformingStoneSlotPrefix,
                slotIndex);

            int capacity = SlotCapacity - stored;

            if (capacity <= 0)
            {
                player.Message(MessageHud.MessageType.Center, "Stone slot is full");
                return false;
            }

            int moved = TakeItemsFromPlayer(
                player,
                new[] { "Stone" },
                capacity);

            if (moved <= 0)
            {
                player.Message(MessageHud.MessageType.Center, "No stone in inventory");
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return false;

            zNetView.InvokeRPC(
                AddStoneSlotRpc,
                player.GetPlayerID(),
                slotIndex,
                moved);

            player.Message(MessageHud.MessageType.Center, "Stone stored: " + moved);
            ModLog.Info("[TerritoryTerraforming] Add stone slot requested: " + wardId + ", slot: " + slotIndex + ", amount: " + moved);
            return true;
        }

        private bool RequestSetEnabled(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool enabled)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingEnabled",
                SetEnabledRpc,
                enabled);
        }

        private bool RequestSetRunning(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool running)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingRunning",
                SetRunningRpc,
                running);
        }

        private bool RequestSetRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            float radius)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingRadius",
                SetRadiusRpc,
                NormalizeRadius(radius));
        }

        private bool RequestSetHoeStored(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool stored)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingHoeStored",
                SetHoeStoredRpc,
                stored);
        }

        private bool RequestSetPickaxeStored(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool stored)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingPickaxeStored",
                SetPickaxeStoredRpc,
                stored);
        }

        private bool InvokeOwnerRpc(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string actionName,
            string rpcName,
            bool value)
        {
            if (!ValidateCreatorAction(actionName, wardId, privateArea, player))
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                rpcName,
                player.GetPlayerID(),
                value);

            ModLog.Info("[TerritoryTerraforming] " + actionName + " requested: " + wardId + ", value: " + value);
            return true;
        }

        private bool InvokeOwnerRpc(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string actionName,
            string rpcName,
            int value)
        {
            if (!ValidateCreatorAction(actionName, wardId, privateArea, player))
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                rpcName,
                player.GetPlayerID(),
                value);

            ModLog.Info("[TerritoryTerraforming] " + actionName + " requested: " + wardId + ", value: " + value);
            return true;
        }

        private bool InvokeOwnerRpc(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string actionName,
            string rpcName,
            float value)
        {
            if (!ValidateCreatorAction(actionName, wardId, privateArea, player))
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                rpcName,
                player.GetPlayerID(),
                value);

            ModLog.Info("[TerritoryTerraforming] " + actionName + " requested: " + wardId + ", value: " + value);
            return true;
        }

        private void RPC_SetEnabled(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool enabled)
        {
            if (!TryGetOwnerZdo("SetTerraformingEnabled", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingEnabled, enabled);

            if (!enabled)
                zdo.Set(TerritoryZdoKeys.TerraformingRunning, false);

            ModLog.Info("[TerritoryTerraforming] Enabled saved: " + enabled);
        }

        private void RPC_SetRunning(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool running)
        {
            if (!TryGetOwnerZdo("SetTerraformingRunning", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            if (running && !zdo.GetBool(TerritoryZdoKeys.TerraformingEnabled, false))
            {
                ModLog.Debug("[TerritoryTerraforming] Running ignored. Terraforming is disabled.");
                return;
            }

            zdo.Set(TerritoryZdoKeys.TerraformingRunning, running);
            ModLog.Info("[TerritoryTerraforming] Running saved: " + running);
        }

        private void RPC_SetRadius(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            float radius)
        {
            if (!TryGetOwnerZdo("SetTerraformingRadius", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingRadius, NormalizeRadius(radius));
            zdo.Set(TerritoryZdoKeys.TerraformingScanProgress, 0f);
            zdo.Set(TerritoryZdoKeys.TerraformingScanIndex, 0);
            ModLog.Info("[TerritoryTerraforming] Radius saved: " + NormalizeRadius(radius));
        }

        private void RPC_SetHoeStored(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool stored)
        {
            if (!TryGetOwnerZdo("SetTerraformingHoeStored", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingHoeStored, stored);
            ModLog.Info("[TerritoryTerraforming] Hoe slot saved: " + stored);
        }

        private void RPC_SetPickaxeStored(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool stored)
        {
            if (!TryGetOwnerZdo("SetTerraformingPickaxeStored", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingPickaxeStored, stored);
            ModLog.Info("[TerritoryTerraforming] Pickaxe slot saved: " + stored);
        }

        private void RPC_AddFuelSlot(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            int slotIndex,
            int amount)
        {
            if (!TryGetOwnerZdo("AddTerraformingFuelSlot", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            if (!IsValidFuelSlot(slotIndex))
                return;

            int stored = GetSlot(zdo, TerritoryZdoKeys.TerraformingFuelSlotPrefix, slotIndex);
            int moved = Mathf.Clamp(amount, 0, SlotCapacity - stored);

            if (moved <= 0)
                return;

            SetSlot(
                zdo,
                TerritoryZdoKeys.TerraformingFuelSlotPrefix,
                slotIndex,
                stored + moved);

            SyncLegacyTotals(zdo);
            ModLog.Info("[TerritoryTerraforming] Fuel slot " + slotIndex + " stored: " + (stored + moved));
        }

        private void RPC_AddStoneSlot(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            int slotIndex,
            int amount)
        {
            if (!TryGetOwnerZdo("AddTerraformingStoneSlot", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            if (!IsValidStoneSlot(slotIndex))
                return;

            int stored = GetSlot(zdo, TerritoryZdoKeys.TerraformingStoneSlotPrefix, slotIndex);
            int moved = Mathf.Clamp(amount, 0, SlotCapacity - stored);

            if (moved <= 0)
                return;

            SetSlot(
                zdo,
                TerritoryZdoKeys.TerraformingStoneSlotPrefix,
                slotIndex,
                stored + moved);

            SyncLegacyTotals(zdo);
            ModLog.Info("[TerritoryTerraforming] Stone slot " + slotIndex + " stored: " + (stored + moved));
        }

        private static void EnsureDefaults(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return;

            if (zdo.GetFloat(TerritoryZdoKeys.TerraformingRadius, 0f) <= 0f)
                zdo.Set(TerritoryZdoKeys.TerraformingRadius, DefaultRadius);

            SyncLegacyTotals(zdo);
        }

        private static int[] ReadSlots(
            ZDO zdo,
            string prefix,
            int count)
        {
            int[] slots = new int[count];

            for (int i = 0; i < count; i++)
                slots[i] = Mathf.Clamp(zdo.GetInt(prefix + i, 0), 0, SlotCapacity);

            return slots;
        }

        private static int GetSlot(
            ZDO zdo,
            string prefix,
            int index)
        {
            return Mathf.Clamp(zdo.GetInt(prefix + index, 0), 0, SlotCapacity);
        }

        private static void SetSlot(
            ZDO zdo,
            string prefix,
            int index,
            int value)
        {
            zdo.Set(
                prefix + index,
                Mathf.Clamp(value, 0, SlotCapacity));
        }

        private static void SyncLegacyTotals(ZDO zdo)
        {
            if (zdo == null)
                return;

            int fuelTotal = 0;
            int stoneTotal = 0;

            for (int i = 0; i < FuelSlotCount; i++)
                fuelTotal += GetSlot(zdo, TerritoryZdoKeys.TerraformingFuelSlotPrefix, i);

            for (int i = 0; i < StoneSlotCount; i++)
                stoneTotal += GetSlot(zdo, TerritoryZdoKeys.TerraformingStoneSlotPrefix, i);

            zdo.Set(TerritoryZdoKeys.TerraformingFuelStored, (float)fuelTotal);
            zdo.Set(TerritoryZdoKeys.TerraformingStoneStored, (float)stoneTotal);
        }

        private static bool ValidateCreatorAction(
            string actionName,
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. Piece is null: " + wardId);
                return false;
            }

            if (piece.GetCreator() != player.GetPlayerID())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. Player is not ward creator: " + wardId);
                return false;
            }

            return true;
        }

        private static bool TryGetOwnerZdo(
            string actionName,
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            out ZDO zdo)
        {
            zdo = null;

            if (privateArea == null || zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. Invalid ward: " + actionName);
                return false;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. ZNetView is not owner: " + actionName);
                return false;
            }

            zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. ZDO is null: " + actionName);
                return false;
            }

            if (zdo.GetLong(ZDOVars.s_creator, 0L) != playerId)
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. Player is not ward creator: " + actionName + ", playerId: " + playerId);
                return false;
            }

            return true;
        }

        private static bool TryConsumeTool(Player player, string prefabName)
        {
            if (player == null)
                return false;

            Inventory inventory = player.GetInventory();

            if (inventory == null)
                return false;

            ItemDrop.ItemData item = FindItemByPrefabName(
                inventory,
                prefabName);

            if (item == null)
            {
                player.Message(MessageHud.MessageType.Center, "Required tool missing: " + prefabName);
                return false;
            }

            player.UnequipItem(item, true);
            inventory.RemoveOneItem(item);
            player.Message(MessageHud.MessageType.Center, "Stored tool: " + prefabName);
            return true;
        }

        private static bool TryConsumeAnyTool(Player player, string[] prefabNames)
        {
            if (player == null || prefabNames == null)
                return false;

            Inventory inventory = player.GetInventory();

            if (inventory == null)
                return false;

            for (int i = 0; i < prefabNames.Length; i++)
            {
                ItemDrop.ItemData item = FindItemByPrefabName(
                    inventory,
                    prefabNames[i]);

                if (item == null)
                    continue;

                player.UnequipItem(item, true);
                inventory.RemoveOneItem(item);
                player.Message(MessageHud.MessageType.Center, "Stored tool: " + prefabNames[i]);
                return true;
            }

            player.Message(MessageHud.MessageType.Center, "Required pickaxe missing");
            return false;
        }

        private static int TakeItemsFromPlayer(
            Player player,
            string[] prefabNames,
            int maxAmount)
        {
            if (player == null || prefabNames == null || maxAmount <= 0)
                return 0;

            Inventory inventory = player.GetInventory();

            if (inventory == null)
                return 0;

            int remaining = maxAmount;
            int moved = 0;
            System.Collections.Generic.List<ItemDrop.ItemData> items =
                new System.Collections.Generic.List<ItemDrop.ItemData>(inventory.GetAllItems());

            for (int i = 0; i < items.Count && remaining > 0; i++)
            {
                ItemDrop.ItemData item = items[i];

                if (item == null)
                    continue;

                if (player.IsItemEquiped(item))
                    continue;

                if (!MatchesAnyPrefabName(item, prefabNames))
                    continue;

                int amount = Mathf.Min(item.m_stack, remaining);

                if (amount <= 0)
                    continue;

                inventory.RemoveItem(item, amount);
                remaining -= amount;
                moved += amount;
            }

            return moved;
        }

        private static ItemDrop.ItemData FindItemByPrefabName(
            Inventory inventory,
            string prefabName)
        {
            if (inventory == null || string.IsNullOrEmpty(prefabName))
                return null;

            System.Collections.Generic.List<ItemDrop.ItemData> items =
                inventory.GetAllItems();

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];

                if (MatchesPrefabName(item, prefabName))
                    return item;
            }

            return null;
        }

        private static bool MatchesAnyPrefabName(
            ItemDrop.ItemData item,
            string[] prefabNames)
        {
            if (prefabNames == null)
                return false;

            for (int i = 0; i < prefabNames.Length; i++)
            {
                if (MatchesPrefabName(item, prefabNames[i]))
                    return true;
            }

            return false;
        }

        private static bool MatchesPrefabName(
            ItemDrop.ItemData item,
            string prefabName)
        {
            if (item == null || string.IsNullOrEmpty(prefabName))
                return false;

            if (item.m_dropPrefab != null && item.m_dropPrefab.name == prefabName)
                return true;

            return item.m_shared != null &&
                   (item.m_shared.m_name == prefabName ||
                    item.m_shared.m_name == "$item_" + prefabName.ToLowerInvariant());
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

        private static float NormalizeRadius(float radius)
        {
            return Mathf.Clamp(radius, MinimumRadius, MaximumRadius);
        }

        private static bool IsValidFuelSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < FuelSlotCount;
        }

        private static bool IsValidStoneSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < StoneSlotCount;
        }
    }

    internal sealed class TerraformingState
    {
        public bool Enabled { get; private set; }

        public bool Running { get; private set; }

        public float Radius { get; private set; }

        public float TargetHeight { get; private set; }

        public int[] FuelSlots { get; private set; }

        public int[] StoneSlots { get; private set; }

        public int FuelStored { get; private set; }

        public int StoneStored { get; private set; }

        public bool HoeStored { get; private set; }

        public bool PickaxeStored { get; private set; }

        public float ScanProgress { get; private set; }

        public int ScanIndex { get; private set; }

        public TerraformingState(
            bool enabled,
            bool running,
            float radius,
            float targetHeight,
            int[] fuelSlots,
            int[] stoneSlots,
            bool hoeStored,
            bool pickaxeStored,
            float scanProgress,
            int scanIndex)
        {
            Enabled = enabled;
            Running = running;
            Radius = radius;
            TargetHeight = targetHeight;
            FuelSlots = fuelSlots ?? new int[5];
            StoneSlots = stoneSlots ?? new int[5];
            FuelStored = Sum(FuelSlots);
            StoneStored = Sum(StoneSlots);
            HoeStored = hoeStored;
            PickaxeStored = pickaxeStored;
            ScanProgress = scanProgress;
            ScanIndex = scanIndex;
        }

        public static TerraformingState Disabled()
        {
            return new TerraformingState(
                false,
                false,
                12f,
                0f,
                new int[5],
                new int[5],
                false,
                false,
                0f,
                0);
        }

        private static int Sum(int[] values)
        {
            if (values == null)
                return 0;

            int total = 0;

            for (int i = 0; i < values.Length; i++)
                total += values[i];

            return total;
        }
    }
}
