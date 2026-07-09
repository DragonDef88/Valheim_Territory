using System.Collections;
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
        private const string TreasuryChestPrefabName = "piece_chest_blackmetal";
        private const int TreasurySlotCapacity = 9999;

        private static readonly FieldInfo InventoryWidthField =
            AccessTools.Field(
                typeof(Inventory),
                "m_width");

        private static readonly FieldInfo InventoryHeightField =
            AccessTools.Field(
                typeof(Inventory),
                "m_height");

        private static readonly FieldInfo InventoryNameField =
            AccessTools.Field(
                typeof(Inventory),
                "m_name");

        private static readonly MethodInfo InventoryChangedMethod =
            AccessTools.Method(
                typeof(Inventory),
                "Changed");

        private static readonly MethodInfo MemberwiseCloneMethod =
            AccessTools.Method(
                typeof(object),
                "MemberwiseClone");

        private static readonly FieldInfo AllPrivateAreasField =
            AccessTools.Field(
                typeof(PrivateArea),
                "m_allAreas");

        private static readonly MethodInfo TerrainCompDoOperationMethod =
            AccessTools.Method(
                typeof(TerrainComp),
                "DoOperation");

        private static readonly MethodInfo HeightmapGetAndCreateTerrainCompilerMethod =
            AccessTools.Method(
                typeof(Heightmap),
                "GetAndCreateTerrainCompiler");

        private static readonly FieldInfo InventoryGridElementsField =
            AccessTools.Field(
                typeof(InventoryGrid),
                "m_elements");

        private static readonly Dictionary<Inventory, Container> PreparationContainerByInventory =
            new Dictionary<Inventory, Container>();

        private static readonly Dictionary<Inventory, Container> TreasuryContainerByInventory =
            new Dictionary<Inventory, Container>();

        private static readonly Dictionary<Inventory, VirtualContainerBinding> VirtualContainerBindings =
            new Dictionary<Inventory, VirtualContainerBinding>();

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
        private const float LevelingWorkerInterval = 0.35f;
        private const int LevelingMaxScanStepsPerTick = 12;
        private const float LevelingSampleSpacing = 2.6f;
        private const float LevelingOperationRadius = 2f;
        private const float LevelingTolerance = 0.15f;
        private const int LevelingFuelCost = 1;
        private const int LevelingStoneCostWhenRaising = 1;


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

        private float _nextLevelingWorkerTime;

        private sealed class VirtualContainerBinding
        {
            public readonly Container Container;
            public readonly ZDO WardZdo;
            public readonly string ItemKey;

            public VirtualContainerBinding(
                Container container,
                ZDO wardZdo,
                string itemKey)
            {
                Container = container;
                WardZdo = wardZdo;
                ItemKey = itemKey;
            }
        }

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
            if (Time.time < _nextLevelingWorkerTime)
                return;

            _nextLevelingWorkerTime = Time.time + LevelingWorkerInterval;
            ProcessLevelingWorkers();
        }

        private void ProcessLevelingWorkers()
        {
            List<PrivateArea> privateAreas = GetPrivateAreas();

            for (int i = 0; i < privateAreas.Count; i++)
            {
                if (TryProcessLevelingWorker(privateAreas[i]))
                    return;
            }
        }

        private bool TryProcessLevelingWorker(PrivateArea privateArea)
        {
            if (privateArea == null)
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid() || !zNetView.IsOwner())
                return false;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return false;

            if (!zdo.GetBool(TerritoryZdoKeys.TerraformingEnabled, false) ||
                !zdo.GetBool(TerritoryZdoKeys.TerraformingRunning, false))
            {
                return false;
            }

            if (ObjectDB.instance == null || ZoneSystem.instance == null)
                return true;

            EnsureDefaults(privateArea);

            Inventory preparationInventory = CreateTerraformingWorkerInventory(zdo);

            if (!HasRequiredLevelingTools(preparationInventory))
            {
                PauseLevelingWorker(
                    zdo,
                    "missing pickaxe or hoe in preparation chest");
                return true;
            }

            float radius = Mathf.Min(
                NormalizeRadius(zdo.GetFloat(TerritoryZdoKeys.TerraformingRadius, DefaultRadius)),
                Mathf.Max(0f, privateArea.m_radius));

            if (radius <= 0f)
            {
                PauseLevelingWorker(
                    zdo,
                    "invalid radius");
                return true;
            }

            Vector3 center = privateArea.transform.position;
            float targetHeight = center.y;
            int pointCount = Mathf.Max(
                1,
                CountPointsInSpiral(
                    radius + LevelingSampleSpacing,
                    LevelingSampleSpacing));

            int scanIndex = Mathf.Clamp(
                zdo.GetInt(TerritoryZdoKeys.TerraformingScanIndex, 0),
                0,
                Mathf.Max(0, pointCount - 1));

            for (int attempt = 0; attempt < LevelingMaxScanStepsPerTick; attempt++)
            {
                if (scanIndex >= pointCount)
                    scanIndex = 0;

                Vector3 point = GetLevelingSpiralPoint(
                    center,
                    scanIndex,
                    radius);

                scanIndex++;

                zdo.Set(
                    TerritoryZdoKeys.TerraformingScanIndex,
                    scanIndex);

                zdo.Set(
                    TerritoryZdoKeys.TerraformingScanProgress,
                    (float)scanIndex);

                if (!IsInsideTerraformingRadius(center, point, radius))
                    continue;

                float groundHeight;

                if (!TryGetGroundHeight(point, out groundHeight))
                    continue;

                float delta = targetHeight - groundHeight;

                if (Mathf.Abs(delta) <= LevelingTolerance)
                    continue;

                bool raising = delta > 0f;

                if (!HasLevelingFuel(preparationInventory))
                {
                    PersistTerraformingWorkerInventory(
                        zdo,
                        preparationInventory);

                    PauseLevelingWorker(
                        zdo,
                        "fuel is empty");
                    return true;
                }

                if (raising && !HasLevelingStone(preparationInventory))
                {
                    PersistTerraformingWorkerInventory(
                        zdo,
                        preparationInventory);

                    PauseLevelingWorker(
                        zdo,
                        "stone is empty");
                    return true;
                }

                if (!ApplyWardHeightLevelingOperation(
                        point,
                        targetHeight))
                {
                    continue;
                }

                ConsumeLevelingFuel(preparationInventory);

                if (raising)
                    ConsumeLevelingStone(preparationInventory);

                PersistTerraformingWorkerInventory(
                    zdo,
                    preparationInventory);

                ModLog.Debug(
                    "[TerritoryTerraforming] Leveling step applied from ward height. target: " +
                    targetHeight +
                    ", point: " +
                    point +
                    ", delta: " +
                    delta);

                return true;
            }

            PersistTerraformingWorkerInventory(
                zdo,
                preparationInventory);

            return true;
        }

        private static List<PrivateArea> GetPrivateAreas()
        {
            if (AllPrivateAreasField == null)
                return new List<PrivateArea>();

            List<PrivateArea> privateAreas =
                AllPrivateAreasField.GetValue(null) as List<PrivateArea>;

            if (privateAreas == null)
                return new List<PrivateArea>();

            return privateAreas;
        }

        private static Inventory CreateTerraformingWorkerInventory(ZDO zdo)
        {
            Inventory inventory = new Inventory(
                "Territory Leveling Chest",
                null,
                PreparationChestWidth,
                PreparationChestHeight);

            if (zdo == null)
                return inventory;

            string serializedItems = zdo.GetString(
                TerritoryZdoKeys.TerraformingChestItems,
                "");

            if (!string.IsNullOrEmpty(serializedItems))
            {
                try
                {
                    LoadVirtualInventoryPackage(
                        inventory,
                        TerritoryZdoKeys.TerraformingChestItems,
                        new ZPackage(serializedItems));
                }
                catch (System.Exception exception)
                {
                    ModLog.Debug(
                        "[TerritoryTerraforming] Worker inventory load failed: " +
                        exception.Message);
                }
            }

            NormalizePreparationChestInventory(inventory);
            SyncPreparationInventoryToZdo(
                zdo,
                inventory);

            return inventory;
        }

        private static void PersistTerraformingWorkerInventory(
            ZDO zdo,
            Inventory inventory)
        {
            if (zdo == null || inventory == null)
                return;

            ZPackage package = new ZPackage();
            inventory.Save(package);

            zdo.Set(
                TerritoryZdoKeys.TerraformingChestItems,
                package.GetBase64());

            SyncPreparationInventoryToZdo(
                zdo,
                inventory);
        }

        private static void SyncPreparationInventoryToZdo(
            ZDO zdo,
            Inventory inventory)
        {
            if (zdo == null || inventory == null)
                return;

            int fuelTotal = 0;
            int stoneTotal = 0;

            for (int x = 0; x < FuelSlotCount; x++)
            {
                ItemDrop.ItemData fuelItem = inventory.GetItemAt(x, 1);
                int fuelAmount = fuelItem != null && IsFuel(fuelItem)
                    ? Mathf.Clamp(fuelItem.m_stack, 0, SlotCapacity)
                    : 0;

                SetSlot(
                    zdo,
                    TerritoryZdoKeys.TerraformingFuelSlotPrefix,
                    x,
                    fuelAmount);

                fuelTotal += fuelAmount;

                ItemDrop.ItemData stoneItem = inventory.GetItemAt(x, 2);
                int stoneAmount = stoneItem != null && IsStone(stoneItem)
                    ? Mathf.Clamp(stoneItem.m_stack, 0, SlotCapacity)
                    : 0;

                SetSlot(
                    zdo,
                    TerritoryZdoKeys.TerraformingStoneSlotPrefix,
                    x,
                    stoneAmount);

                stoneTotal += stoneAmount;
            }

            zdo.Set(
                TerritoryZdoKeys.TerraformingFuelStored,
                (float)fuelTotal);

            zdo.Set(
                TerritoryZdoKeys.TerraformingStoneStored,
                (float)stoneTotal);

            zdo.Set(
                TerritoryZdoKeys.TerraformingPickaxeStored,
                IsPickaxe(inventory.GetItemAt(0, 0)));

            zdo.Set(
                TerritoryZdoKeys.TerraformingHoeStored,
                IsHoe(inventory.GetItemAt(1, 0)));
        }

        private static bool HasRequiredLevelingTools(Inventory inventory)
        {
            if (inventory == null)
                return false;

            return IsPickaxe(inventory.GetItemAt(0, 0)) &&
                   IsHoe(inventory.GetItemAt(1, 0));
        }

        private static bool HasLevelingFuel(Inventory inventory)
        {
            return FindPreparationRowItem(
                       inventory,
                       1,
                       IsFuel) != null;
        }

        private static bool HasLevelingStone(Inventory inventory)
        {
            return FindPreparationRowItem(
                       inventory,
                       2,
                       IsStone) != null;
        }

        private static bool ConsumeLevelingFuel(Inventory inventory)
        {
            return ConsumePreparationRowItem(
                inventory,
                1,
                IsFuel,
                LevelingFuelCost);
        }

        private static bool ConsumeLevelingStone(Inventory inventory)
        {
            return ConsumePreparationRowItem(
                inventory,
                2,
                IsStone,
                LevelingStoneCostWhenRaising);
        }

        private static ItemDrop.ItemData FindPreparationRowItem(
            Inventory inventory,
            int row,
            System.Predicate<ItemDrop.ItemData> predicate)
        {
            if (inventory == null || predicate == null)
                return null;

            for (int x = 0; x < PreparationChestWidth; x++)
            {
                ItemDrop.ItemData item = inventory.GetItemAt(x, row);

                if (item == null || item.m_stack <= 0)
                    continue;

                if (predicate(item))
                    return item;
            }

            return null;
        }

        private static bool ConsumePreparationRowItem(
            Inventory inventory,
            int row,
            System.Predicate<ItemDrop.ItemData> predicate,
            int amount)
        {
            if (inventory == null || amount <= 0)
                return false;

            ItemDrop.ItemData item = FindPreparationRowItem(
                inventory,
                row,
                predicate);

            if (item == null)
                return false;

            int consumed = Mathf.Min(
                amount,
                item.m_stack);

            if (consumed <= 0)
                return false;

            item.m_stack -= consumed;

            if (item.m_stack <= 0)
                inventory.RemoveItem(item);

            InvokeInventoryChanged(inventory);
            return true;
        }

        private static bool IsInsideTerraformingRadius(
            Vector3 center,
            Vector3 point,
            float radius)
        {
            return global::Utils.DistanceXZ(
                       center,
                       point) <= radius;
        }

        private static bool TryGetGroundHeight(
            Vector3 point,
            out float groundHeight)
        {
            groundHeight = point.y;

            if (ZoneSystem.instance == null)
                return false;

            return ZoneSystem.instance.GetGroundHeight(
                point,
                out groundHeight);
        }

        private static Vector3 GetLevelingSpiralPoint(
            Vector3 center,
            int scanIndex,
            float radius)
        {
            float angle;
            float spiralRadius;

            PolarPointOnSpiral(
                scanIndex,
                LevelingSampleSpacing,
                out angle,
                out spiralRadius);

            float distance = Mathf.Min(
                spiralRadius,
                radius);

            return center +
                   new Vector3(
                       Mathf.Sin(angle) * distance,
                       0f,
                       Mathf.Cos(angle) * distance);
        }

        private static void PolarPointOnSpiral(
            float t,
            float spacing,
            out float angle,
            out float radius)
        {
            angle = Mathf.Sqrt(t) * 3.542f;
            radius = angle * spacing / (Mathf.PI * 2f);
        }

        private static int CountPointsInSpiral(
            float radius,
            float spacing)
        {
            float normalized = radius / spacing;
            return Mathf.CeilToInt(
                normalized *
                normalized *
                3.146755f);
        }

        private static bool ApplyWardHeightLevelingOperation(
            Vector3 point,
            float targetHeight)
        {
            TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(point);

            if (terrainComp == null)
            {
                Heightmap heightmap = Heightmap.FindHeightmap(point);

                if (heightmap == null)
                    return false;

                if (HeightmapGetAndCreateTerrainCompilerMethod != null)
                {
                    terrainComp =
                        HeightmapGetAndCreateTerrainCompilerMethod.Invoke(
                            heightmap,
                            null) as TerrainComp;
                }
            }

            if (terrainComp == null)
                return false;

            if (TerrainCompDoOperationMethod == null)
            {
                ModLog.Debug("[TerritoryTerraforming] TerrainComp.DoOperation reflection missing.");
                return false;
            }

            TerrainOp.Settings settings = CreateWardLevelingSettings();
            Vector3 operationPoint = new Vector3(
                point.x,
                targetHeight,
                point.z);

            try
            {
                TerrainCompDoOperationMethod.Invoke(
                    terrainComp,
                    new object[]
                    {
                        operationPoint,
                        settings
                    });

                return true;
            }
            catch (System.Exception exception)
            {
                ModLog.Debug(
                    "[TerritoryTerraforming] Terrain leveling operation failed: " +
                    exception.Message);
                return false;
            }
        }

        private static TerrainOp.Settings CreateWardLevelingSettings()
        {
            TerrainOp.Settings settings = new TerrainOp.Settings();
            settings.m_levelOffset = 0f;
            settings.m_level = true;
            settings.m_levelRadius = LevelingOperationRadius;
            settings.m_square = false;
            settings.m_raise = false;
            settings.m_raiseRadius = 0f;
            settings.m_raisePower = 0f;
            settings.m_raiseDelta = 0f;
            settings.m_smooth = false;
            settings.m_smoothRadius = 0f;
            settings.m_smoothPower = 0f;
            settings.m_paintCleared = false;
            settings.m_paintHeightCheck = false;
            settings.m_paintRadius = 0f;
            return settings;
        }

        private static void PauseLevelingWorker(
            ZDO zdo,
            string reason)
        {
            if (zdo == null)
                return;

            zdo.Set(
                TerritoryZdoKeys.TerraformingRunning,
                false);

            ModLog.Info(
                "[TerritoryTerraforming] Worker paused: " +
                reason);
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

            if (InventoryGui.instance == null)
                return false;

            InventoryGui.instance.Hide();

            Container container = CreateVirtualPreparationChest(
                wardId,
                privateArea,
                player);

            if (container == null)
                return false;

            InventoryGui.instance.Show(container);
            return true;
        }

        public bool RequestOpenTreasuryChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (!ValidateCreatorAction("OpenTreasuryChest", wardId, privateArea, player))
                return false;

            if (InventoryGui.instance == null)
                return false;

            InventoryGui.instance.Hide();

            Container container = CreateVirtualTreasuryChest(
                wardId,
                privateArea,
                player);

            if (container == null)
                return false;

            InventoryGui.instance.Show(container);
            return true;
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

        private Container CreateVirtualPreparationChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            return CreateVirtualContainer(
                wardId,
                privateArea,
                player,
                PreparationChestPrefabName,
                "ClanTerritory_VirtualPreparationChest",
                "Territory Leveling Chest",
                TerritoryZdoKeys.TerraformingChestItems,
                ConfigurePreparationChest);
        }

        private Container CreateVirtualTreasuryChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            return CreateVirtualContainer(
                wardId,
                privateArea,
                player,
                TreasuryChestPrefabName,
                "ClanTerritory_VirtualTreasuryChest",
                "Territory Treasury",
                TerritoryZdoKeys.TreasuryChestItems,
                ConfigureTreasuryChest);
        }

        private Container CreateVirtualContainer(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string prefabName,
            string objectName,
            string inventoryName,
            string itemKey,
            System.Action<Container> configureAction)
        {
            if (GetZdo(privateArea) == null)
            {
                ModLog.Debug("[TerritoryContainers] Virtual container ignored. Ward ZDO is null: " + wardId);
                return null;
            }

            if (ZNetScene.instance == null)
            {
                ModLog.Debug("[TerritoryContainers] Virtual container ignored. ZNetScene is null: " + wardId);
                return null;
            }

            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);

            if (prefab == null)
            {
                ModLog.Debug("[TerritoryContainers] Virtual container prefab not found: " + prefabName);
                return null;
            }

            GameObject chestObject = Object.Instantiate(
                prefab,
                CalculateVirtualContainerPosition(privateArea, player),
                Quaternion.identity);

            if (chestObject == null)
                return null;

            chestObject.name = objectName;
            HideVirtualContainerObject(chestObject);

            Container container = chestObject.GetComponent<Container>();

            if (container == null)
            {
                ModLog.Debug("[TerritoryContainers] Virtual container ignored. Container component is missing: " + prefabName);
                DestroyVirtualContainerObject(chestObject);
                return null;
            }

            container.m_name = inventoryName;
            container.m_privacy = Container.PrivacySetting.Public;
            container.m_checkGuardStone = false;
            container.m_autoDestroyEmpty = false;
            container.m_openEffects = new EffectList();
            container.m_closeEffects = new EffectList();

            if (container.m_open != null)
                container.m_open.SetActive(false);

            if (container.m_closed != null)
                container.m_closed.SetActive(false);

            if (configureAction != null)
                configureAction(container);

            Inventory inventory = container.GetInventory();

            if (inventory == null)
            {
                DestroyVirtualContainerObject(chestObject);
                return null;
            }

            SetInventoryName(
                inventory,
                inventoryName);

            LoadVirtualInventory(
                privateArea,
                inventory,
                itemKey);

            RegisterVirtualContainer(
                container,
                privateArea,
                itemKey);

            ModLog.Info("[TerritoryContainers] Virtual container opened without world chest: " + prefabName + ", ward: " + wardId);
            return container;
        }

        private static Vector3 CalculateVirtualContainerPosition(
            PrivateArea privateArea,
            Player player)
        {
            if (player != null)
                return player.transform.position;

            if (privateArea != null)
                return privateArea.transform.position;

            return Vector3.zero;
        }

        private static void HideVirtualContainerObject(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = false;

            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true);

            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }

        private static void RegisterVirtualContainer(
            Container container,
            PrivateArea privateArea,
            string itemKey)
        {
            if (container == null)
                return;

            Inventory inventory = container.GetInventory();

            if (inventory == null)
                return;

            ZDO wardZdo = GetZdo(privateArea);

            if (wardZdo == null)
                return;

            VirtualContainerBindings[inventory] = new VirtualContainerBinding(
                container,
                wardZdo,
                itemKey);

            inventory.m_onChanged = (System.Action)System.Delegate.Combine(
                inventory.m_onChanged,
                new System.Action(
                    delegate
                    {
                        PersistVirtualInventory(inventory);
                    }));

            PersistVirtualInventory(inventory);
        }

        private static void LoadVirtualInventory(
            PrivateArea privateArea,
            Inventory inventory,
            string itemKey)
        {
            if (inventory == null)
                return;

            ZDO wardZdo = GetZdo(privateArea);

            if (wardZdo == null)
                return;

            string serializedItems = wardZdo.GetString(itemKey, "");

            if (string.IsNullOrEmpty(serializedItems))
                return;

            try
            {
                ZPackage package = new ZPackage(serializedItems);
                LoadVirtualInventoryPackage(
                    inventory,
                    itemKey,
                    package);
            }
            catch (System.Exception exception)
            {
                ModLog.Debug("[TerritoryContainers] Failed to load virtual container inventory: " + exception.Message);
            }
        }

        private static void LoadVirtualInventoryPackage(
            Inventory inventory,
            string itemKey,
            ZPackage package)
        {
            if (inventory == null || package == null)
                return;

            int version = package.ReadInt();
            int count = package.ReadInt();

            inventory.GetAllItems().Clear();

            for (int i = 0; i < count; i++)
            {
                string prefabName = package.ReadString();
                int stack = package.ReadInt();
                float durability = package.ReadSingle();
                Vector2i gridPosition = package.ReadVector2i();
                bool equipped = package.ReadBool();

                int quality = 1;
                int variant = 0;
                long crafterId = 0L;
                string crafterName = "";
                Dictionary<string, string> customData = new Dictionary<string, string>();
                int worldLevel = 0;
                bool pickedUp = false;

                if (version >= 101)
                    quality = package.ReadInt();

                if (version >= 102)
                    variant = package.ReadInt();

                if (version >= 103)
                {
                    crafterId = package.ReadLong();
                    crafterName = package.ReadString();
                }

                if (version >= 104)
                {
                    int customDataCount = package.ReadInt();

                    for (int dataIndex = 0; dataIndex < customDataCount; dataIndex++)
                    {
                        customData[package.ReadString()] = package.ReadString();
                    }
                }

                if (version >= 105)
                    worldLevel = package.ReadInt();

                if (version >= 106)
                    pickedUp = package.ReadBool();

                if (string.IsNullOrEmpty(prefabName))
                    continue;

                ItemDrop.ItemData itemData = CreateVirtualItemData(
                    prefabName,
                    stack,
                    durability,
                    gridPosition,
                    equipped,
                    quality,
                    variant,
                    crafterId,
                    crafterName,
                    customData,
                    worldLevel,
                    pickedUp,
                    GetVirtualSlotCapacity(
                        itemKey,
                        gridPosition));

                if (itemData == null)
                    continue;

                inventory.GetAllItems().Add(itemData);
            }

            InvokeInventoryChanged(inventory);
        }

        private static int GetVirtualSlotCapacity(
            string itemKey,
            Vector2i slot)
        {
            if (itemKey == TerritoryZdoKeys.TreasuryChestItems)
                return TreasurySlotCapacity;

            if (itemKey == TerritoryZdoKeys.TerraformingChestItems)
                return GetPreparationSlotCapacity(slot);

            return 1;
        }

        private static void ApplyVirtualStackLimit(
            ItemDrop.ItemData item,
            int stackLimit)
        {
            if (item == null || item.m_shared == null)
                return;

            int limit = Mathf.Max(1, stackLimit);

            if (item.m_shared.m_maxStackSize >= limit)
                return;

            ItemDrop.ItemData.SharedData clonedShared =
                CloneSharedData(item.m_shared);

            if (clonedShared == null)
                return;

            clonedShared.m_maxStackSize = limit;
            item.m_shared = clonedShared;
        }

        private static ItemDrop.ItemData.SharedData CloneSharedData(
            ItemDrop.ItemData.SharedData sharedData)
        {
            if (sharedData == null || MemberwiseCloneMethod == null)
                return null;

            return MemberwiseCloneMethod.Invoke(
                       sharedData,
                       null) as ItemDrop.ItemData.SharedData;
        }

        private static ItemDrop.ItemData CreateVirtualItemData(
            string prefabName,
            int stack,
            float durability,
            Vector2i gridPosition,
            bool equipped,
            int quality,
            int variant,
            long crafterId,
            string crafterName,
            Dictionary<string, string> customData,
            int worldLevel,
            bool pickedUp,
            int stackLimit)
        {
            if (ObjectDB.instance == null)
                return null;

            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);

            if (itemPrefab == null)
            {
                ModLog.Debug("[TerritoryContainers] Virtual item prefab not found: " + prefabName);
                return null;
            }

            ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();

            if (itemDrop == null)
                return null;

            ItemDrop.ItemData itemData = itemDrop.m_itemData.Clone();
            itemData.m_dropPrefab = itemPrefab;
            itemData.m_stack = Mathf.Clamp(stack, 1, Mathf.Max(1, stackLimit));
            itemData.m_durability = durability;
            itemData.m_gridPos = gridPosition;
            itemData.m_equipped = equipped;
            itemData.m_quality = quality;
            itemData.m_variant = variant;
            itemData.m_crafterID = crafterId;
            itemData.m_crafterName = crafterName;
            itemData.m_customData = customData != null
                ? new Dictionary<string, string>(customData)
                : new Dictionary<string, string>();
            itemData.m_worldLevel = worldLevel;
            itemData.m_pickedUp = pickedUp;

            ApplyVirtualStackLimit(
                itemData,
                stackLimit);

            return itemData;
        }

        private static void PersistVirtualInventory(Inventory inventory)
        {
            if (inventory == null)
                return;

            VirtualContainerBinding binding;

            if (!VirtualContainerBindings.TryGetValue(inventory, out binding))
                return;

            if (binding == null || binding.WardZdo == null)
                return;

            ZPackage package = new ZPackage();
            inventory.Save(package);
            binding.WardZdo.Set(binding.ItemKey, package.GetBase64());

            if (binding.ItemKey == TerritoryZdoKeys.TerraformingChestItems)
            {
                SyncPreparationInventoryToZdo(
                    binding.WardZdo,
                    inventory);
            }
        }

        private static void SetInventoryName(
            Inventory inventory,
            string name)
        {
            if (inventory == null || InventoryNameField == null)
                return;

            InventoryNameField.SetValue(
                inventory,
                name);
        }

        public static bool IsVirtualTerritoryContainer(Container container)
        {
            if (container == null)
                return false;

            Inventory inventory = container.GetInventory();

            if (inventory == null)
                return false;

            return VirtualContainerBindings.ContainsKey(inventory);
        }

        public static void CloseVirtualTerritoryContainer(Container container)
        {
            if (container == null)
                return;

            Inventory inventory = container.GetInventory();

            if (inventory != null)
            {
                PersistVirtualInventory(inventory);
                VirtualContainerBindings.Remove(inventory);
                PreparationContainerByInventory.Remove(inventory);
                TreasuryContainerByInventory.Remove(inventory);
            }

            DestroyVirtualContainerObject(container.gameObject);
        }

        private static void DestroyVirtualContainerObject(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            ZNetView zNetView = gameObject.GetComponent<ZNetView>();

            if (zNetView != null && zNetView.IsValid() && ZNetScene.instance != null)
            {
                ZNetScene.instance.Destroy(gameObject);
                return;
            }

            Object.Destroy(gameObject);
        }

        private Container GetOrCreateTreasuryChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            Container existing = FindLinkedTreasuryChest(privateArea);

            if (existing != null)
                return existing;

            if (ZNetScene.instance == null)
            {
                ModLog.Debug("[TerritoryTreasury] Treasury chest ignored. ZNetScene is null: " + wardId);
                return null;
            }

            GameObject prefab = ZNetScene.instance.GetPrefab(TreasuryChestPrefabName);

            if (prefab == null)
            {
                ModLog.Debug("[TerritoryTreasury] Treasury chest prefab not found: " + TreasuryChestPrefabName);
                return null;
            }

            Vector3 position = CalculateTreasuryChestPosition(privateArea, player);
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
                ModLog.Debug("[TerritoryTreasury] Treasury chest ignored. Container component is missing.");
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
                chestZdo.Set(TerritoryZdoKeys.TreasuryChestMarker, true);
                wardZdo.Set(TerritoryZdoKeys.TreasuryChestZdoUser, chestZdo.m_uid.UserID);
                wardZdo.Set(TerritoryZdoKeys.TreasuryChestZdoId, (int)chestZdo.m_uid.ID);
            }

            ConfigureTreasuryChest(container);

            ModLog.Info("[TerritoryTreasury] Real treasury chest created from " + TreasuryChestPrefabName + ": " + wardId);
            return container;
        }

        private Container FindLinkedTreasuryChest(PrivateArea privateArea)
        {
            ZDO wardZdo = GetZdo(privateArea);

            if (wardZdo == null)
                return null;

            long userId = wardZdo.GetLong(TerritoryZdoKeys.TreasuryChestZdoUser, 0L);
            int id = wardZdo.GetInt(TerritoryZdoKeys.TreasuryChestZdoId, 0);

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

            ConfigureTreasuryChest(container);
            return container;
        }

        private static Vector3 CalculateTreasuryChestPosition(
            PrivateArea privateArea,
            Player player)
        {
            Vector3 basePosition = privateArea != null
                ? privateArea.transform.position
                : player != null
                    ? player.transform.position
                    : Vector3.zero;

            Vector3 right = player != null
                ? player.transform.right
                : privateArea != null
                    ? privateArea.transform.right
                    : Vector3.right;

            if (right == Vector3.zero)
                right = Vector3.right;

            Vector3 position =
                basePosition +
                right.normalized * 2.75f;

            float groundHeight;

            if (ZoneSystem.instance != null &&
                ZoneSystem.instance.GetGroundHeight(position, out groundHeight))
            {
                position.y = groundHeight;
            }
            else
            {
                position.y = basePosition.y;
            }

            return position;
        }

        private static void ConfigureTreasuryChest(Container container)
        {
            if (container == null)
                return;

            container.m_name = "Territory Treasury";
            container.m_privacy = Container.PrivacySetting.Private;
            container.m_checkGuardStone = false;
            container.m_autoDestroyEmpty = false;

            ZNetView zNetView = container.GetComponent<ZNetView>();

            if (zNetView != null && zNetView.IsValid())
                zNetView.GetZDO().Set(TerritoryZdoKeys.TreasuryChestMarker, true);

            Inventory inventory = container.GetInventory();

            if (inventory == null)
                return;

            TreasuryContainerByInventory[inventory] = container;
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
            return FindLinkedChest(
                privateArea,
                TerritoryZdoKeys.TerraformingChestZdoUser,
                TerritoryZdoKeys.TerraformingChestZdoId,
                ConfigurePreparationChest);
        }

        private Container FindLinkedChest(
            PrivateArea privateArea,
            string userKey,
            string idKey,
            System.Action<Container> configureAction)
        {
            ZDO wardZdo = GetZdo(privateArea);

            if (wardZdo == null)
                return null;

            long userId = wardZdo.GetLong(userKey, 0L);
            int id = wardZdo.GetInt(idKey, 0);

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

            if (configureAction != null)
                configureAction(container);

            return container;
        }

        private static Vector3 CalculateChestPosition(
            PrivateArea privateArea,
            Player player)
        {
            return CalculateChestPosition(
                privateArea,
                player,
                2.25f);
        }

        private static Vector3 CalculateChestPosition(
            PrivateArea privateArea,
            Player player,
            float distance)
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
                forward.normalized * distance;

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

        public static bool IsTreasuryChestInventory(Inventory inventory)
        {
            if (inventory == null)
                return false;

            return TreasuryContainerByInventory.ContainsKey(inventory);
        }

        public static bool TryMoveItemToTreasurySlot(
            Inventory targetInventory,
            Inventory sourceInventory,
            ItemDrop.ItemData item,
            int amount,
            Vector2i slot,
            out bool result)
        {
            result = false;

            if (!IsTreasuryChestInventory(targetInventory))
                return false;

            if (sourceInventory == null || item == null)
                return true;

            if (slot.x < 0 || slot.y < 0 ||
                slot.x >= targetInventory.GetWidth() ||
                slot.y >= targetInventory.GetHeight())
            {
                PlayerMessage("Invalid treasury slot");
                return true;
            }

            result = MoveItemToLargeStackSlot(
                targetInventory,
                sourceInventory,
                item,
                amount,
                slot,
                TreasurySlotCapacity);

            return true;
        }

        public static bool TryAutoMoveItemToTreasuryChest(
            Inventory targetInventory,
            Inventory sourceInventory,
            ItemDrop.ItemData item)
        {
            if (!IsTreasuryChestInventory(targetInventory))
                return false;

            if (sourceInventory == null || item == null)
                return true;

            Vector2i slot;

            if (!TryFindTreasurySlotForItem(
                    targetInventory,
                    item,
                    out slot))
            {
                PlayerMessage("Treasury is full");
                return true;
            }

            bool moved;
            TryMoveItemToTreasurySlot(
                targetInventory,
                sourceInventory,
                item,
                item.m_stack,
                slot,
                out moved);

            return true;
        }

        public static void ApplyPreparationChestGridVisibility(
            InventoryGrid grid,
            Inventory inventory)
        {
            if (grid == null || inventory == null)
                return;

            if (!IsPreparationChestInventory(inventory))
                return;

            if (InventoryGridElementsField == null)
                return;

            IList elements = InventoryGridElementsField.GetValue(grid) as IList;

            if (elements == null)
                return;

            for (int i = 0; i < elements.Count; i++)
            {
                object element = elements[i];

                if (element == null)
                    continue;

                FieldInfo goField =
                    element.GetType().GetField(
                        "m_go",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                FieldInfo posField =
                    element.GetType().GetField(
                        "m_pos",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (goField == null || posField == null)
                    continue;

                GameObject gameObject = goField.GetValue(element) as GameObject;

                if (gameObject == null)
                    continue;

                Vector2i position = (Vector2i)posField.GetValue(element);
                bool hiddenReservedTopCell = position.y == 0 && position.x >= 2;

                gameObject.SetActive(!hiddenReservedTopCell);
            }
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

            ApplyVirtualStackLimit(
                item,
                capacity);

            if (targetItem != null)
                ApplyVirtualStackLimit(
                    targetItem,
                    capacity);

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
                ApplyVirtualStackLimit(
                    clone,
                    capacity);
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

        private static bool MoveItemToLargeStackSlot(
            Inventory targetInventory,
            Inventory sourceInventory,
            ItemDrop.ItemData item,
            int amount,
            Vector2i slot,
            int capacity)
        {
            if (targetInventory == null || sourceInventory == null || item == null)
                return false;

            ItemDrop.ItemData targetItem = targetInventory.GetItemAt(slot.x, slot.y);
            int requestedAmount = amount > 0 ? amount : item.m_stack;

            ApplyVirtualStackLimit(
                item,
                capacity);

            if (targetItem != null)
                ApplyVirtualStackLimit(
                    targetItem,
                    capacity);

            if (sourceInventory == targetInventory)
            {
                if (targetItem == item)
                    return true;

                if (targetItem == null)
                {
                    if (requestedAmount < item.m_stack)
                    {
                        ItemDrop.ItemData clone = item.Clone();
                        clone.m_stack = requestedAmount;
                        clone.m_gridPos = slot;
                        ApplyVirtualStackLimit(
                            clone,
                            capacity);
                        targetInventory.GetAllItems().Add(clone);
                        item.m_stack -= requestedAmount;
                        InvokeInventoryChanged(targetInventory);
                        return true;
                    }

                    item.m_gridPos = slot;
                    InvokeInventoryChanged(targetInventory);
                    return true;
                }

                if (IsSameItemStack(targetItem, item))
                {
                    int space = capacity - targetItem.m_stack;
                    int moved = Mathf.Min(item.m_stack, Mathf.Max(0, space));

                    if (moved <= 0)
                    {
                        PlayerMessage("Treasury slot is full");
                        return false;
                    }

                    targetItem.m_stack += moved;
                    item.m_stack -= moved;

                    if (item.m_stack <= 0)
                        targetInventory.RemoveItem(item);
                    else
                        InvokeInventoryChanged(targetInventory);

                    return true;
                }

                if (requestedAmount >= item.m_stack)
                {
                    Vector2i originalPosition = item.m_gridPos;
                    item.m_gridPos = slot;
                    targetItem.m_gridPos = originalPosition;
                    InvokeInventoryChanged(targetInventory);
                    return true;
                }

                PlayerMessage("Cannot split into occupied treasury slot");
                return false;
            }

            if (targetItem != null && !IsSameItemStack(targetItem, item))
            {
                PlayerMessage("Treasury slot already contains another item");
                return false;
            }

            int current = targetItem != null ? targetItem.m_stack : 0;
            int spaceAvailable = capacity - current;

            if (spaceAvailable <= 0)
            {
                PlayerMessage("Treasury slot is full");
                return false;
            }

            int movedAmount = Mathf.Min(
                Mathf.Min(item.m_stack, requestedAmount),
                spaceAvailable);

            if (movedAmount <= 0)
                return false;

            if (targetItem != null)
            {
                targetItem.m_stack += movedAmount;
            }
            else
            {
                ItemDrop.ItemData clone = item.Clone();
                clone.m_stack = movedAmount;
                clone.m_gridPos = slot;
                ApplyVirtualStackLimit(
                    clone,
                    capacity);
                targetInventory.GetAllItems().Add(clone);
            }

            sourceInventory.RemoveItem(item, movedAmount);
            InvokeInventoryChanged(targetInventory);
            InvokeInventoryChanged(sourceInventory);
            return true;
        }

        private static bool TryFindTreasurySlotForItem(
            Inventory inventory,
            ItemDrop.ItemData item,
            out Vector2i slot)
        {
            slot = new Vector2i(-1, -1);

            if (inventory == null || item == null)
                return false;

            int width = inventory.GetWidth();
            int height = inventory.GetHeight();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ItemDrop.ItemData existing = inventory.GetItemAt(x, y);

                    if (existing == null)
                        continue;

                    if (!IsSameItemStack(existing, item))
                        continue;

                    if (existing.m_stack >= TreasurySlotCapacity)
                        continue;

                    slot = new Vector2i(x, y);
                    return true;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (inventory.GetItemAt(x, y) != null)
                        continue;

                    slot = new Vector2i(x, y);
                    return true;
                }
            }

            return false;
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
                    ApplyVirtualStackLimit(
                        item,
                        GetPreparationSlotCapacity(item.m_gridPos));
                    continue;
                }

                Vector2i slot;

                if (TryFindPreparationSlotForItem(inventory, item, out slot))
                {
                    item.m_gridPos = slot;
                    int capacity = GetPreparationSlotCapacity(slot);
                    item.m_stack = Mathf.Min(
                        item.m_stack,
                        capacity);
                    ApplyVirtualStackLimit(
                        item,
                        capacity);

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
