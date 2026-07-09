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
        private const string PreparationChestPrefabName = "piece_chest_wood";
        private const string PreparationChestName = "$piece_chestwood";
        private const int PreparationChestWidth = 5;
        private const int PreparationChestHeight = 3;

        private static readonly FieldInfo InventoryWidthField =
            AccessTools.Field(
                typeof(Inventory),
                "m_width");

        private static readonly FieldInfo InventoryHeightField =
            AccessTools.Field(
                typeof(Inventory),
                "m_height");

        private static readonly MethodInfo InventoryChangedMethod =
            AccessTools.Method(
                typeof(Inventory),
                "Changed");

        private static readonly Dictionary<Inventory, Container> PreparationContainerByInventory =
            new Dictionary<Inventory, Container>();

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

        public bool RequestOpenPreparationChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (!ValidateCreatorAction("OpenPreparationChest", wardId, privateArea, player))
                return false;

            Container container = GetOrCreatePreparationChest(
                wardId,
                privateArea,
                player);

            if (container == null)
                return false;

            ConfigurePreparationChest(container);

            if (InventoryGui.instance != null)
            {
                InventoryGui.instance.Hide();
            }

            return container.Interact(
                player,
                false,
                false);
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

        private Container GetOrCreatePreparationChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            Container existing = FindLinkedPreparationChest(privateArea);

            if (existing != null)
                return existing;

            if (ZNetScene.instance == null)
            {
                ModLog.Debug("[TerritoryTerraforming] Preparation chest ignored. ZNetScene is null: " + wardId);
                return null;
            }

            GameObject prefab = ZNetScene.instance.GetPrefab(PreparationChestPrefabName);

            if (prefab == null)
            {
                ModLog.Debug("[TerritoryTerraforming] Preparation chest prefab not found: " + PreparationChestPrefabName);
                return null;
            }

            Vector3 position = CalculateChestPosition(privateArea, player);
            Quaternion rotation = Quaternion.identity;

            if (player != null)
                rotation = Quaternion.Euler(0f, player.transform.eulerAngles.y, 0f);

            GameObject chestObject = Object.Instantiate(
                prefab,
                position,
                rotation);

            if (chestObject == null)
                return null;

            Container container = chestObject.GetComponent<Container>();

            if (container == null)
            {
                ModLog.Debug("[TerritoryTerraforming] Preparation chest ignored. Container component is missing.");
                return null;
            }

            Piece piece = chestObject.GetComponent<Piece>();

            if (piece != null && player != null)
                piece.SetCreator(player.GetPlayerID());

            ZNetView chestView = chestObject.GetComponent<ZNetView>();
            ZDO chestZdo = chestView != null && chestView.IsValid()
                ? chestView.GetZDO()
                : null;

            ZDO wardZdo = GetZdo(privateArea);

            if (wardZdo != null && chestZdo != null)
            {
                chestZdo.Set(TerritoryZdoKeys.TerraformingChestMarker, true);
                wardZdo.Set(TerritoryZdoKeys.TerraformingChestZdoUser, chestZdo.m_uid.UserID);
                wardZdo.Set(TerritoryZdoKeys.TerraformingChestZdoId, (int)chestZdo.m_uid.ID);
            }

            ConfigurePreparationChest(container);

            ModLog.Info("[TerritoryTerraforming] Real preparation chest created from " + PreparationChestPrefabName + ": " + wardId);
            return container;
        }

        private Container FindLinkedPreparationChest(PrivateArea privateArea)
        {
            ZDO wardZdo = GetZdo(privateArea);

            if (wardZdo == null)
                return null;

            long userId = wardZdo.GetLong(TerritoryZdoKeys.TerraformingChestZdoUser, 0L);
            int id = wardZdo.GetInt(TerritoryZdoKeys.TerraformingChestZdoId, 0);

            if (userId == 0L || id == 0 || ZNetScene.instance == null)
                return null;

            GameObject chestObject = ZNetScene.instance.FindInstance(
                new ZDOID(
                    userId,
                    (uint)id));

            if (chestObject == null)
                return null;

            Container container = chestObject.GetComponent<Container>();

            if (container == null)
                return null;

            ConfigurePreparationChest(container);
            return container;
        }

        private static Vector3 CalculateChestPosition(
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
                return player != null ? player.transform.position : Vector3.zero;

            Vector3 forward = player != null
                ? player.transform.forward
                : privateArea.transform.forward;

            if (forward == Vector3.zero)
                forward = Vector3.forward;

            Vector3 position =
                privateArea.transform.position +
                forward.normalized * 2.25f;

            float groundHeight = position.y;

            if (ZoneSystem.instance != null &&
                ZoneSystem.instance.GetGroundHeight(position, out groundHeight))
            {
                position.y = groundHeight;
            }
            else
            {
                position.y = privateArea.transform.position.y;
            }

            return position;
        }

        private static void ConfigurePreparationChest(Container container)
        {
            if (container == null)
                return;

            container.m_name = "Territory Leveling Chest";
            container.m_width = PreparationChestWidth;
            container.m_height = PreparationChestHeight;
            container.m_privacy = Container.PrivacySetting.Private;
            container.m_checkGuardStone = false;
            container.m_autoDestroyEmpty = false;

            ZNetView zNetView = container.GetComponent<ZNetView>();

            if (zNetView != null && zNetView.IsValid())
                zNetView.GetZDO().Set(TerritoryZdoKeys.TerraformingChestMarker, true);

            Inventory inventory = container.GetInventory();

            if (inventory == null)
                return;

            if (InventoryWidthField != null)
                InventoryWidthField.SetValue(inventory, PreparationChestWidth);

            if (InventoryHeightField != null)
                InventoryHeightField.SetValue(inventory, PreparationChestHeight);

            PreparationContainerByInventory[inventory] = container;
            NormalizePreparationChestInventory(inventory);
        }

        public static bool IsPreparationChestInventory(Inventory inventory)
        {
            if (inventory == null)
                return false;

            return PreparationContainerByInventory.ContainsKey(inventory);
        }

        public static bool TryMoveItemToPreparationSlot(
            Inventory targetInventory,
            Inventory sourceInventory,
            ItemDrop.ItemData item,
            int amount,
            Vector2i slot,
            out bool result)
        {
            result = false;

            if (!IsPreparationChestInventory(targetInventory))
                return false;

            if (sourceInventory == null || item == null)
                return true;

            if (!IsAllowedPreparationItemAtSlot(item, slot))
            {
                ShowPreparationSlotMessage(slot);
                return true;
            }

            if (sourceInventory == targetInventory)
            {
                ItemDrop.ItemData existing = targetInventory.GetItemAt(slot.x, slot.y);

                if (existing != null && existing != item)
                {
                    PlayerMessage("Preparation slot is occupied");
                    return true;
                }

                item.m_gridPos = slot;
                InvokeInventoryChanged(targetInventory);
                result = true;
                return true;
            }

            int capacity = GetPreparationSlotCapacity(slot);
            ItemDrop.ItemData targetItem = targetInventory.GetItemAt(slot.x, slot.y);

            if (targetItem != null && !IsSameItemStack(targetItem, item))
            {
                PlayerMessage("Preparation slot already contains another item");
                return true;
            }

            int space = capacity - (targetItem != null ? targetItem.m_stack : 0);

            if (space <= 0)
            {
                PlayerMessage("Preparation slot is full");
                return true;
            }

            int moved = Mathf.Min(
                Mathf.Min(item.m_stack, Mathf.Max(1, amount)),
                space);

            if (moved <= 0)
                return true;

            if (targetItem != null)
            {
                targetItem.m_stack += moved;
            }
            else
            {
                ItemDrop.ItemData clone = item.Clone();
                clone.m_stack = moved;
                clone.m_gridPos = slot;
                targetInventory.GetAllItems().Add(clone);
            }

            sourceInventory.RemoveItem(item, moved);
            InvokeInventoryChanged(targetInventory);
            InvokeInventoryChanged(sourceInventory);
            result = true;
            return true;
        }

        public static bool TryAutoMoveItemToPreparationChest(
            Inventory targetInventory,
            Inventory sourceInventory,
            ItemDrop.ItemData item)
        {
            if (!IsPreparationChestInventory(targetInventory))
                return false;

            if (sourceInventory == null || item == null)
                return true;

            Vector2i slot;

            if (!TryFindPreparationSlotForItem(targetInventory, item, out slot))
            {
                PlayerMessage("No valid preparation slot for this item");
                return true;
            }

            bool moved;
            TryMoveItemToPreparationSlot(
                targetInventory,
                sourceInventory,
                item,
                item.m_stack,
                slot,
                out moved);

            return true;
        }

        private static void NormalizePreparationChestInventory(Inventory inventory)
        {
            if (inventory == null)
                return;

            List<ItemDrop.ItemData> items =
                new List<ItemDrop.ItemData>(inventory.GetAllItems());

            bool changed = false;

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];

                if (item == null)
                    continue;

                if (IsAllowedPreparationItemAtSlot(item, item.m_gridPos) &&
                    item.m_stack <= GetPreparationSlotCapacity(item.m_gridPos))
                {
                    continue;
                }

                Vector2i slot;

                if (TryFindPreparationSlotForItem(inventory, item, out slot))
                {
                    item.m_gridPos = slot;
                    item.m_stack = Mathf.Min(
                        item.m_stack,
                        GetPreparationSlotCapacity(slot));

                    changed = true;
                    continue;
                }

                inventory.RemoveItem(item);
                changed = true;
            }

            if (changed)
                InvokeInventoryChanged(inventory);
        }

        private static bool TryFindPreparationSlotForItem(
            Inventory inventory,
            ItemDrop.ItemData item,
            out Vector2i slot)
        {
            slot = new Vector2i(-1, -1);

            if (inventory == null || item == null)
                return false;

            for (int y = 0; y < PreparationChestHeight; y++)
            {
                for (int x = 0; x < PreparationChestWidth; x++)
                {
                    Vector2i candidate = new Vector2i(x, y);

                    if (!IsAllowedPreparationItemAtSlot(item, candidate))
                        continue;

                    ItemDrop.ItemData existing = inventory.GetItemAt(x, y);

                    if (existing == null)
                    {
                        slot = candidate;
                        return true;
                    }

                    if (!IsSameItemStack(existing, item))
                        continue;

                    if (existing.m_stack >= GetPreparationSlotCapacity(candidate))
                        continue;

                    slot = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool IsAllowedPreparationItemAtSlot(
            ItemDrop.ItemData item,
            Vector2i slot)
        {
            if (item == null)
                return false;

            if (slot.y == 0)
            {
                if (slot.x == 0)
                    return IsPickaxe(item);

                if (slot.x == 1)
                    return IsHoe(item);

                return false;
            }

            if (slot.y == 1)
                return IsFuel(item);

            if (slot.y == 2)
                return IsStone(item);

            return false;
        }

        private static int GetPreparationSlotCapacity(Vector2i slot)
        {
            if (slot.y == 0)
                return 1;

            return 500;
        }

        private static bool IsSameItemStack(
            ItemDrop.ItemData left,
            ItemDrop.ItemData right)
        {
            if (left == null || right == null)
                return false;

            if (left.m_shared == null || right.m_shared == null)
                return false;

            return left.m_shared.m_name == right.m_shared.m_name &&
                   left.m_quality == right.m_quality &&
                   left.m_worldLevel == right.m_worldLevel;
        }

        private static bool IsPickaxe(ItemDrop.ItemData item)
        {
            string prefabName = GetItemPrefabName(item);
            string sharedName = GetItemSharedName(item);

            return ContainsIgnoreCase(prefabName, "Pickaxe") ||
                   ContainsIgnoreCase(sharedName, "pickaxe");
        }

        private static bool IsHoe(ItemDrop.ItemData item)
        {
            string prefabName = GetItemPrefabName(item);
            string sharedName = GetItemSharedName(item);

            return EqualsIgnoreCase(prefabName, "Hoe") ||
                   ContainsIgnoreCase(sharedName, "hoe");
        }

        private static bool IsFuel(ItemDrop.ItemData item)
        {
            string prefabName = GetItemPrefabName(item);

            return EqualsIgnoreCase(prefabName, "Wood") ||
                   EqualsIgnoreCase(prefabName, "Coal") ||
                   EqualsIgnoreCase(prefabName, "Resin");
        }

        private static bool IsStone(ItemDrop.ItemData item)
        {
            return EqualsIgnoreCase(GetItemPrefabName(item), "Stone");
        }

        private static string GetItemPrefabName(ItemDrop.ItemData item)
        {
            if (item == null || item.m_dropPrefab == null)
                return "";

            return global::Utils.GetPrefabName(item.m_dropPrefab.name);
        }

        private static string GetItemSharedName(ItemDrop.ItemData item)
        {
            if (item == null || item.m_shared == null)
                return "";

            return item.m_shared.m_name ?? "";
        }

        private static bool EqualsIgnoreCase(string value, string expected)
        {
            return string.Equals(
                value,
                expected,
                System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsIgnoreCase(string value, string expectedPart)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(expectedPart))
                return false;

            return value.IndexOf(
                       expectedPart,
                       System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ShowPreparationSlotMessage(Vector2i slot)
        {
            if (slot.y == 0 && slot.x == 0)
            {
                PlayerMessage("Only pickaxe can be placed in this slot");
                return;
            }

            if (slot.y == 0 && slot.x == 1)
            {
                PlayerMessage("Only hoe can be placed in this slot");
                return;
            }

            if (slot.y == 1)
            {
                PlayerMessage("Only fuel can be placed in this row");
                return;
            }

            if (slot.y == 2)
            {
                PlayerMessage("Only stone can be placed in this row");
                return;
            }

            PlayerMessage("This preparation slot is reserved");
        }

        private static void PlayerMessage(string message)
        {
            if (Player.m_localPlayer == null)
                return;

            Player.m_localPlayer.Message(
                MessageHud.MessageType.Center,
                message);
        }

        private static void InvokeInventoryChanged(Inventory inventory)
        {
            if (inventory == null || InventoryChangedMethod == null)
                return;

            InventoryChangedMethod.Invoke(
                inventory,
                null);
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
