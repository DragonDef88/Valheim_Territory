using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Reflection;
using ClanTerritory.Config;
using ClanTerritory.Abstractions;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Map.Services;
using ClanTerritory.Features.World.Services;
using ClanTerritory.Features.Persistence.FileSystem;
using ClanTerritory.Features.BiomeDominion;
using ClanTerritory.Features.Territory.Events;
using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Placement;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.Territory.Zdo;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Localization;
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
            UnityEngine.Object.DontDestroyOnLoad(_runnerObject);

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
                UnityEngine.Object.Destroy(_runnerObject);

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
                        ShowMessage(player, CtLocalization.Format("ct.message.left_territory", _currentTerritoryName));
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
                    ShowMessage(player, CtLocalization.Format("ct.message.left_territory", _currentTerritoryName));
                    ModLog.Info("[TerritoryPresence] Left territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
                }

                _currentWardId = wardId;
                _currentTerritoryName = GetTerritoryName(currentArea);

                ShowEnterTerritoryMessage(
                    player,
                    currentArea,
                    _currentTerritoryName);

                ModLog.Info("[TerritoryPresence] Entered territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
            }

            private void ClearCurrentTerritory()
            {
                _currentWardId = "";
                _currentTerritoryName = "";
            }

            private static void ShowEnterTerritoryMessage(
                Player player,
                PrivateArea currentArea,
                string territoryName)
            {
                BiomeDominionService biomeDominionService;
                BiomeDominionRecord dominion;

                if (ServiceContainer.TryGet<BiomeDominionService>(out biomeDominionService) &&
                    biomeDominionService.TryGetVassalStatus(
                        currentArea,
                        out dominion))
                {
                    ShowMessage(
                        player,
                        CtLocalization.Format(
                            "ct.message.entered_vassal_territory",
                            territoryName,
                            dominion.BiomeName,
                            dominion.DisplayName));
                    return;
                }

                ShowMessage(
                    player,
                    CtLocalization.Format(
                        "ct.message.entered_territory",
                        territoryName));
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

                return CtLocalization.Get("ct.territory.unnamed");
            }

            private static void ShowMessage(Player player, string message)
            {
                if (player == null)
                    return;

                player.Message(MessageHud.MessageType.Center, message);
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

        private static readonly FieldInfo TerrainCompHeightmapField =
            AccessTools.Field(
                typeof(TerrainComp),
                "m_hmap");

        private static readonly FieldInfo TerrainCompWidthField =
            AccessTools.Field(
                typeof(TerrainComp),
                "m_width");

        private static readonly FieldInfo TerrainCompModifiedHeightField =
            AccessTools.Field(
                typeof(TerrainComp),
                "m_modifiedHeight");

        private static readonly FieldInfo TerrainCompLevelDeltaField =
            AccessTools.Field(
                typeof(TerrainComp),
                "m_levelDelta");

        private static readonly FieldInfo TerrainCompSmoothDeltaField =
            AccessTools.Field(
                typeof(TerrainComp),
                "m_smoothDelta");

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

        private static readonly Dictionary<string, GameObject> LevelingSpiritByWardId =
            new Dictionary<string, GameObject>();

        private static readonly Dictionary<string, Vector3> LevelingSpiritVelocityByWardId =
            new Dictionary<string, Vector3>();

        private const string SetEnabledRpc = "CT_SetTerraformingEnabled";
        private const string SetRunningRpc = "CT_SetTerraformingRunning";
        private const string SetRadiusRpc = "CT_SetTerraformingRadius";
        private const string SetHoeStoredRpc = "CT_SetTerraformingHoeStored";
        private const string SetPickaxeStoredRpc = "CT_SetTerraformingPickaxeStored";
        private const string AddFuelSlotRpc = "CT_AddTerraformingFuelSlot";
        private const string AddStoneSlotRpc = "CT_AddTerraformingStoneSlot";

        private const float DefaultRadius = 12f;
        private const float MinimumRadius = 2f;
        private const float MaximumRadius = 200f;
        private const float RadiusStep = 2f;
        private const int SlotCapacity = 500;
        private const int FuelSlotCount = 5;
        private const int StoneSlotCount = 5;
        private const float LevelingWorkerInterval = 0.1f;
        private const int LevelingMaxScanStepsPerTick = 1;
        private const float LevelingSampleSpacing = 2.6f;
        private const float LevelingOperationRadius = 1.75f;
        private const float LevelingSampleRadius = 1.5f;
        private const float LevelingLocalSampleSpacing = 0.5f;
        private const float LevelingWorkThreshold = 0.45f;
        private const int LevelingVerifyPasses = 3;
        private const float LevelingScanTime = 0.55f;
        private const float LevelingFlatteningTime = 1.8f;
        private const float LevelingVerifyDelay = 0.75f;
        private const float LevelingStonePerLower = 0.05f;
        private const float LevelingTolerance = 0.25f;
        private const int LevelingFuelCost = 1;
        private const int LevelingStoneCostWhenRaising = 1;
        private const float LevelingMaxDeltaPerOperation = 0.18f;
        private const float LevelingSpiritSmoothTime = 0.95f;
        private const float LevelingSpiritMaxSpeed = 5f;
        private const float LevelingMineRockRadius = 2.5f;
        private const float LevelingMineRockHitRadius = 0.75f;
        private const float LevelingFallbackPickaxeDamage = 25f;
        private const float LevelingFallbackAxeDamage = 25f;
        private const float LevelingFuelWorkPerItem = 8f;
        private const float LevelingTerrainFuelWorkMultiplier = 0.45f;
        private const float LevelingRockFuelWork = 0.35f;
        private const float LevelingTreeFuelWork = 0.5f;
        private const float LevelingTreeRadius = 3f;
        private const float LevelingTreeHitRadius = 0.75f;
        private const float LevelingTreeWorkerInterval = 1.0f;
        private const float ResourceAbsorbInterval = 2f;
        private const int ResourceAbsorbMaxItemsPerWard = 32;
        private const int ResourceAbsorbMaxContainersPerWard = 64;
        private const int TreasuryFallbackWidth = 8;
        private const int TreasuryFallbackHeight = 4;




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

        private static readonly string[] AxePrefabNames =
        {
            "AxeStone",
            "AxeFlint",
            "AxeBronze",
            "AxeIron",
            "AxeBlackMetal",
            "Battleaxe",
            "BattleaxeCrystal"
        };

        private static bool _wardHeightLevelingOperationActive;

        private float _nextResourceAbsorbTime;

        private float _nextTreeWorkerTime;

        private struct LevelingEvaluation
        {
            public bool ShouldApply;
            public bool Raising;
            public float NetDelta;
            public float WorkScore;
            public float RaiseAmount;
            public float LowerAmount;
            public int FuelCost;
            public int StoneCost;
            public int StoneYield;
        }

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
            if (Time.time >= _nextResourceAbsorbTime)
            {
                _nextResourceAbsorbTime = Time.time + ResourceAbsorbInterval;
                ProcessGroundResourceAbsorption();
            }

            ProcessLevelingWorkers();
        }

        private void ProcessGroundResourceAbsorption()
        {
            if (ObjectDB.instance == null)
                return;

            List<PrivateArea> privateAreas = GetPrivateAreas();

            for (int i = 0; i < privateAreas.Count; i++)
            {
                TryAbsorbGroundItemsForArea(privateAreas[i]);
            }
        }

        private static void TryAbsorbGroundItemsForArea(PrivateArea privateArea)
        {
            if (privateArea == null)
                return;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid() || !zNetView.IsOwner())
                return;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return;

            Inventory preparationInventory = CreateTerraformingWorkerInventory(zdo);
            Inventory treasuryInventory = CreateVirtualStorageInventory(
                "Territory Treasury",
                TreasuryFallbackWidth,
                TreasuryFallbackHeight,
                zdo,
                TerritoryZdoKeys.TreasuryChestItems);

            bool preparationChanged = false;
            bool treasuryChanged = false;
            int absorbedItems = 0;
            int realContainerAbsorbedItems = 0;

            List<Container> territoryContainers =
                GetRealContainersInTerritory(privateArea);

            ItemDrop[] drops = UnityEngine.Object.FindObjectsOfType<ItemDrop>();

            for (int i = 0; i < drops.Length && absorbedItems < ResourceAbsorbMaxItemsPerWard; i++)
            {
                ItemDrop drop = drops[i];

                if (!IsAbsorbableGroundItem(
                        drop,
                        privateArea))
                {
                    continue;
                }

                int absorbed = 0;

                if (TryAbsorbIntoExistingPreparationStack(
                        preparationInventory,
                        drop.m_itemData,
                        out absorbed))
                {
                    preparationChanged = true;
                }

                if (absorbed <= 0 &&
                    TryAbsorbIntoExistingTreasuryStack(
                        treasuryInventory,
                        drop.m_itemData,
                        out absorbed))
                {
                    treasuryChanged = true;
                }

                if (absorbed <= 0 &&
                    TryAbsorbIntoRealTerritoryContainers(
                        territoryContainers,
                        drop.m_itemData,
                        out absorbed))
                {
                    realContainerAbsorbedItems++;
                }

                if (absorbed <= 0)
                    continue;

                ApplyGroundItemAbsorb(
                    drop,
                    absorbed);

                absorbedItems++;
            }

            if (preparationChanged)
            {
                PersistTerraformingWorkerInventory(
                    zdo,
                    preparationInventory);
            }

            if (treasuryChanged)
            {
                PersistVirtualInventoryToZdo(
                    zdo,
                    treasuryInventory,
                    TerritoryZdoKeys.TreasuryChestItems);
            }

            if (absorbedItems > 0)
            {
                ModLog.Debug(
                    "[TerritoryResources] Absorbed ground item stacks: " +
                    absorbedItems +
                    ", real containers: " +
                    realContainerAbsorbedItems);
            }
        }

        private static bool IsAbsorbableGroundItem(
            ItemDrop drop,
            PrivateArea privateArea)
        {
            if (drop == null ||
                drop.m_itemData == null ||
                drop.m_itemData.m_stack <= 0)
            {
                return false;
            }

            if (drop.IsPiece())
                return false;

            if (!drop.CanPickup(false))
            {
                drop.RequestOwn();
                return false;
            }

            return global::Utils.DistanceXZ(
                       privateArea.transform.position,
                       drop.transform.position) <= privateArea.m_radius;
        }

        private static Inventory CreateVirtualStorageInventory(
            string name,
            int width,
            int height,
            ZDO zdo,
            string itemKey)
        {
            Inventory inventory = new Inventory(
                name,
                null,
                width,
                height);

            if (zdo == null)
                return inventory;

            string serializedItems = zdo.GetString(
                itemKey,
                "");

            if (string.IsNullOrEmpty(serializedItems))
                return inventory;

            try
            {
                LoadVirtualInventoryPackage(
                    inventory,
                    itemKey,
                    new ZPackage(serializedItems));
            }
            catch (System.Exception exception)
            {
                ModLog.Debug(
                    "[TerritoryResources] Virtual inventory load failed: " +
                    exception.Message);
            }

            return inventory;
        }

        private static void PersistVirtualInventoryToZdo(
            ZDO zdo,
            Inventory inventory,
            string itemKey)
        {
            if (zdo == null || inventory == null)
                return;

            ZPackage package = new ZPackage();
            inventory.Save(package);

            zdo.Set(
                itemKey,
                package.GetBase64());
        }

        private static bool TryAbsorbIntoExistingPreparationStack(
            Inventory inventory,
            ItemDrop.ItemData groundItem,
            out int absorbed)
        {
            absorbed = 0;

            if (inventory == null || groundItem == null)
                return false;

            if (!HasMatchingStack(
                    inventory,
                    groundItem,
                    IsAllowedPreparationItemAtSlot))
            {
                return false;
            }

            absorbed += MoveGroundStackIntoMatchingStacks(
                inventory,
                groundItem,
                IsAllowedPreparationItemAtSlot,
                GetPreparationSlotCapacity,
                true);

            if (groundItem.m_stack > 0)
            {
                absorbed += MoveGroundStackIntoFreeSlots(
                    inventory,
                    groundItem,
                    IsAllowedPreparationItemAtSlot,
                    GetPreparationSlotCapacity,
                    true);
            }

            if (absorbed > 0)
                InvokeInventoryChanged(inventory);

            return absorbed > 0;
        }

        private static bool TryAbsorbIntoExistingTreasuryStack(
            Inventory inventory,
            ItemDrop.ItemData groundItem,
            out int absorbed)
        {
            absorbed = 0;

            if (inventory == null || groundItem == null)
                return false;

            if (!HasMatchingStack(
                    inventory,
                    groundItem,
                    IsAnyTreasurySlotAllowed))
            {
                return false;
            }

            absorbed += MoveGroundStackIntoMatchingStacks(
                inventory,
                groundItem,
                IsAnyTreasurySlotAllowed,
                GetTreasurySlotCapacity,
                true);

            if (groundItem.m_stack > 0)
            {
                absorbed += MoveGroundStackIntoFreeSlots(
                    inventory,
                    groundItem,
                    IsAnyTreasurySlotAllowed,
                    GetTreasurySlotCapacity,
                    true);
            }

            if (absorbed > 0)
                InvokeInventoryChanged(inventory);

            return absorbed > 0;
        }

        private static bool TryAbsorbIntoRealTerritoryContainers(
            List<Container> containers,
            ItemDrop.ItemData groundItem,
            out int absorbed)
        {
            absorbed = 0;

            if (containers == null || groundItem == null)
                return false;

            for (int i = 0; i < containers.Count && groundItem.m_stack > 0; i++)
            {
                Container container = containers[i];

                if (container == null)
                    continue;

                Inventory inventory = container.GetInventory();

                if (inventory == null)
                    continue;

                if (!HasMatchingStack(
                        inventory,
                        groundItem,
                        IsAnyRealContainerSlotAllowed))
                {
                    continue;
                }

                int moved = 0;

                moved += MoveGroundStackIntoMatchingStacks(
                    inventory,
                    groundItem,
                    IsAnyRealContainerSlotAllowed,
                    GetRealContainerSlotCapacity,
                    false);

                if (groundItem.m_stack > 0)
                {
                    moved += MoveGroundStackIntoFreeSlots(
                        inventory,
                        groundItem,
                        IsAnyRealContainerSlotAllowed,
                        GetRealContainerSlotCapacity,
                        false);
                }

                if (moved <= 0)
                    continue;

                absorbed += moved;
                InvokeInventoryChanged(inventory);
            }

            return absorbed > 0;
        }

        private static List<Container> GetRealContainersInTerritory(
            PrivateArea privateArea)
        {
            List<Container> result = new List<Container>();

            if (privateArea == null)
                return result;

            Container[] containers = UnityEngine.Object.FindObjectsOfType<Container>();

            for (int i = 0; i < containers.Length && result.Count < ResourceAbsorbMaxContainersPerWard; i++)
            {
                Container container = containers[i];

                if (!IsRealAbsorbTargetContainer(
                        container,
                        privateArea))
                {
                    continue;
                }

                result.Add(container);
            }

            return result;
        }

        private static bool IsRealAbsorbTargetContainer(
            Container container,
            PrivateArea privateArea)
        {
            if (container == null || privateArea == null)
                return false;

            if (!container.gameObject.activeInHierarchy)
                return false;

            Inventory inventory = container.GetInventory();

            if (inventory == null)
                return false;

            if (VirtualContainerBindings.ContainsKey(inventory))
                return false;

            if (PreparationContainerByInventory.ContainsKey(inventory) ||
                TreasuryContainerByInventory.ContainsKey(inventory))
            {
                return false;
            }

            if (!container.IsOwner())
                return false;

            if (container.IsInUse())
                return false;

            return global::Utils.DistanceXZ(
                       privateArea.transform.position,
                       container.transform.position) <= privateArea.m_radius;
        }

        private static bool HasMatchingStack(
            Inventory inventory,
            ItemDrop.ItemData groundItem,
            System.Func<ItemDrop.ItemData, Vector2i, bool> slotRule)
        {
            if (inventory == null || groundItem == null || slotRule == null)
                return false;

            List<ItemDrop.ItemData> items = inventory.GetAllItems();

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];

                if (item == null)
                    continue;

                if (!IsSameItemStack(
                        item,
                        groundItem))
                {
                    continue;
                }

                if (!slotRule(
                        item,
                        item.m_gridPos))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static int MoveGroundStackIntoMatchingStacks(
            Inventory inventory,
            ItemDrop.ItemData groundItem,
            System.Func<ItemDrop.ItemData, Vector2i, bool> slotRule,
            System.Func<ItemDrop.ItemData, Vector2i, int> capacityProvider,
            bool applyVirtualStackLimit)
        {
            if (inventory == null ||
                groundItem == null ||
                slotRule == null ||
                capacityProvider == null)
            {
                return 0;
            }

            int absorbed = 0;
            List<ItemDrop.ItemData> items = inventory.GetAllItems();

            for (int i = 0; i < items.Count && groundItem.m_stack > 0; i++)
            {
                ItemDrop.ItemData existing = items[i];

                if (existing == null)
                    continue;

                if (!IsSameItemStack(
                        existing,
                        groundItem))
                {
                    continue;
                }

                if (!slotRule(
                        existing,
                        existing.m_gridPos))
                {
                    continue;
                }

                int moved = MoveGroundStackIntoExistingStack(
                    existing,
                    groundItem,
                    capacityProvider(existing, existing.m_gridPos),
                    applyVirtualStackLimit);

                absorbed += moved;
            }

            return absorbed;
        }

        private static int MoveGroundStackIntoFreeSlots(
            Inventory inventory,
            ItemDrop.ItemData groundItem,
            System.Func<ItemDrop.ItemData, Vector2i, bool> slotRule,
            System.Func<ItemDrop.ItemData, Vector2i, int> capacityProvider,
            bool applyVirtualStackLimit)
        {
            if (inventory == null ||
                groundItem == null ||
                slotRule == null ||
                capacityProvider == null)
            {
                return 0;
            }

            int absorbed = 0;
            int width = inventory.GetWidth();
            int height = inventory.GetHeight();

            for (int y = 0; y < height && groundItem.m_stack > 0; y++)
            {
                for (int x = 0; x < width && groundItem.m_stack > 0; x++)
                {
                    if (inventory.GetItemAt(x, y) != null)
                        continue;

                    Vector2i slot = new Vector2i(x, y);

                    if (!slotRule(
                            groundItem,
                            slot))
                    {
                        continue;
                    }

                    int capacity = capacityProvider(groundItem, slot);
                    int moved = Mathf.Min(
                        capacity,
                        groundItem.m_stack);

                    if (moved <= 0)
                        continue;

                    ItemDrop.ItemData clone = groundItem.Clone();
                    clone.m_stack = moved;
                    clone.m_gridPos = slot;

                    if (applyVirtualStackLimit)
                    {
                        ApplyVirtualStackLimit(
                            clone,
                            capacity);
                    }

                    inventory.GetAllItems().Add(clone);
                    groundItem.m_stack -= moved;
                    absorbed += moved;
                }
            }

            return absorbed;
        }

        private static int MoveGroundStackIntoExistingStack(
            ItemDrop.ItemData existing,
            ItemDrop.ItemData groundItem,
            int capacity,
            bool applyVirtualStackLimit)
        {
            if (existing == null || groundItem == null)
                return 0;

            if (applyVirtualStackLimit)
            {
                ApplyVirtualStackLimit(
                    existing,
                    capacity);
            }

            int space = Mathf.Max(
                0,
                capacity - existing.m_stack);

            if (space <= 0)
                return 0;

            int moved = Mathf.Min(
                space,
                groundItem.m_stack);

            if (moved <= 0)
                return 0;

            existing.m_stack += moved;
            groundItem.m_stack -= moved;
            return moved;
        }

        private static bool IsAnyTreasurySlotAllowed(
            ItemDrop.ItemData item,
            Vector2i slot)
        {
            return item != null;
        }

        private static bool IsAnyRealContainerSlotAllowed(
            ItemDrop.ItemData item,
            Vector2i slot)
        {
            return item != null &&
                   item.m_shared != null &&
                   item.m_shared.m_maxStackSize > 1;
        }

        private static int GetTreasurySlotCapacity(ItemDrop.ItemData item, Vector2i slot)
        {
            return TreasurySlotCapacity;
        }

        private static int GetRealContainerSlotCapacity(
            ItemDrop.ItemData item,
            Vector2i slot)
        {
            if (item == null || item.m_shared == null)
                return 1;

            return Mathf.Max(
                1,
                item.m_shared.m_maxStackSize);
        }


        private static void ApplyGroundItemAbsorb(
            ItemDrop drop,
            int absorbed)
        {
            if (drop == null || absorbed <= 0)
                return;

            if (drop.m_itemData == null || drop.m_itemData.m_stack <= 0)
            {
                DestroyGroundItem(drop);
                return;
            }

            drop.SetStack(drop.m_itemData.m_stack);
        }

        private static void DestroyGroundItem(ItemDrop drop)
        {
            if (drop == null)
                return;

            ZNetView zNetView = drop.GetComponent<ZNetView>();

            if (zNetView != null && zNetView.IsValid() && zNetView.IsOwner())
            {
                zNetView.Destroy();
                return;
            }

            UnityEngine.Object.Destroy(drop.gameObject);
        }

        private void ProcessLevelingWorkers()
        {
            List<PrivateArea> privateAreas = GetPrivateAreas();
            bool activeTerrainWorkerProcessed = false;
            bool treeWorkerAttempted = false;
            bool treeWorkerAllowed = Time.time >= _nextTreeWorkerTime;

            for (int i = 0; i < privateAreas.Count; i++)
            {
                PrivateArea privateArea = privateAreas[i];

                if (privateArea == null)
                    continue;

                if (IsLevelingWorkerRunning(privateArea))
                {
                    if (treeWorkerAllowed && !treeWorkerAttempted)
                    {
                        treeWorkerAttempted = true;
                        TryProcessTreeWorker(privateArea);
                    }

                    if (!activeTerrainWorkerProcessed)
                    {
                        activeTerrainWorkerProcessed = TryProcessLevelingWorker(privateArea);
                    }
                    else
                    {
                        UpdateLevelingSpirit(
                            privateArea,
                            privateArea.transform.position,
                            false);
                    }
                }
                else
                {
                    UpdateLevelingSpirit(
                        privateArea,
                        privateArea.transform.position,
                        false);
                }
            }

            if (treeWorkerAttempted)
                _nextTreeWorkerTime = Time.time + LevelingTreeWorkerInterval;
        }

        private bool IsLevelingWorkerRunning(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return false;

            return zdo.GetBool(TerritoryZdoKeys.TerraformingEnabled, false) &&
                   zdo.GetBool(TerritoryZdoKeys.TerraformingRunning, false);
        }

        private bool TryProcessTreeWorker(PrivateArea privateArea)
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

            if (ObjectDB.instance == null)
                return false;

            EnsureDefaults(privateArea);

            Inventory preparationInventory = CreateTerraformingWorkerInventory(zdo);

            if (!HasTreeWorkerTool(preparationInventory))
                return false;

            if (!HasLevelingFuelForWork(
                    zdo,
                    preparationInventory,
                    LevelingTreeFuelWork))
            {
                return false;
            }

            Vector3 center = privateArea.transform.position;
            float territoryRadius = Mathf.Max(
                0f,
                privateArea.m_radius);

            if (territoryRadius <= 0f)
                return false;

            if (!TryChopTreeInTerritory(
                    center,
                    territoryRadius,
                    preparationInventory))
            {
                return false;
            }

            if (!SpendLevelingFuelForWork(
                    zdo,
                    preparationInventory,
                    LevelingTreeFuelWork))
            {
                return false;
            }

            PersistTerraformingWorkerInventory(
                zdo,
                preparationInventory);

            ModLog.Debug(
                "[TerritoryTerraforming] Tree chopping worker hit applied in territory radius: " +
                territoryRadius);

            return true;
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
                UpdateLevelingSpirit(
                    privateArea,
                    privateArea.transform.position,
                    false);

                return false;
            }

            if (ObjectDB.instance == null || ZoneSystem.instance == null)
                return true;

            EnsureDefaults(privateArea);

            Inventory preparationInventory = CreateTerraformingWorkerInventory(zdo);

            if (!HasRequiredLevelingTools(preparationInventory))
            {
                UpdateLevelingSpirit(
                    privateArea,
                    privateArea.transform.position,
                    false);

                return false;
            }

            float radius = Mathf.Min(
                NormalizeRadius(zdo.GetFloat(TerritoryZdoKeys.TerraformingRadius, DefaultRadius)),
                Mathf.Max(0f, privateArea.m_radius));

            if (radius <= 0f)
            {
                PauseLevelingWorker(
                    zdo,
                    "invalid radius");

                UpdateLevelingSpirit(
                    privateArea,
                    privateArea.transform.position,
                    false);

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

            float scanProgress = zdo.GetFloat(
                TerritoryZdoKeys.TerraformingScanProgress,
                0f);

            float scanSpeed = zdo.GetFloat(
                TerritoryZdoKeys.TerraformingScanSpeed,
                1f / LevelingScanTime);

            scanProgress += Time.deltaTime * Mathf.Max(
                0.01f,
                scanSpeed);

            if (scanProgress >= pointCount)
            {
                scanProgress = 0f;
                scanIndex = 0;
                zdo.Set(
                    TerritoryZdoKeys.TerraformingScanIndex,
                    0);
                zdo.Set(
                    TerritoryZdoKeys.TerraformingPendingScanIndex,
                    -1);
                zdo.Set(
                    TerritoryZdoKeys.TerraformingVerifyCount,
                    0);
                zdo.Set(
                    TerritoryZdoKeys.TerraformingNextVerifyTime,
                    0f);
            }

            Vector3 spiritPoint = GetLevelingSpiralPoint(
                center,
                Mathf.Clamp(
                    scanProgress,
                    0f,
                    Mathf.Max(0, pointCount - 1)),
                radius);

            UpdateLevelingSpirit(
                privateArea,
                spiritPoint,
                true);

            zdo.Set(
                TerritoryZdoKeys.TerraformingScanProgress,
                scanProgress);

            if (scanProgress < scanIndex)
                return true;

            if (scanIndex >= pointCount)
                scanIndex = 0;

            Vector3 point = GetLevelingSpiralPoint(
                center,
                scanIndex,
                radius);

            if (!IsInsideTerraformingRadius(center, point, radius))
            {
                AdvanceLevelingScan(
                    zdo,
                    scanIndex,
                    scanProgress,
                    pointCount,
                    false);

                return true;
            }

            if (HasLevelingFuelForWork(
                    zdo,
                    preparationInventory,
                    LevelingRockFuelWork) &&
                TryMineRockAtPoint(
                    point,
                    center,
                    radius,
                    preparationInventory))
            {
                SpendLevelingFuelForWork(
                    zdo,
                    preparationInventory,
                    LevelingRockFuelWork);

                PersistTerraformingWorkerInventory(
                    zdo,
                    preparationInventory);

                zdo.Set(
                    TerritoryZdoKeys.TerraformingPendingScanIndex,
                    -1);

                zdo.Set(
                    TerritoryZdoKeys.TerraformingVerifyCount,
                    0);

                zdo.Set(
                    TerritoryZdoKeys.TerraformingNextVerifyTime,
                    0f);

                AdvanceLevelingScan(
                    zdo,
                    scanIndex,
                    scanProgress,
                    pointCount,
                    true);

                ModLog.Debug(
                    "[TerritoryTerraforming] Mining hit applied near scan point: " +
                    point);

                return true;
            }

            LevelingEvaluation evaluation;

            if (!TryEvaluateLevelingPoint(
                    point,
                    targetHeight,
                    out evaluation))
            {
                AdvanceLevelingScan(
                    zdo,
                    scanIndex,
                    scanProgress,
                    pointCount,
                    false);

                return true;
            }

            if (!HasLevelingFuelForWork(
                    zdo,
                    preparationInventory,
                    evaluation.WorkScore * LevelingTerrainFuelWorkMultiplier))
            {
                PersistTerraformingWorkerInventory(
                    zdo,
                    preparationInventory);

                PauseLevelingWorker(
                    zdo,
                    "fuel is empty");

                UpdateLevelingSpirit(
                    privateArea,
                    point,
                    false);

                return true;
            }

            if (evaluation.Raising &&
                !HasLevelingStone(
                    preparationInventory,
                    evaluation.StoneCost))
            {
                PersistTerraformingWorkerInventory(
                    zdo,
                    preparationInventory);

                PauseLevelingWorker(
                    zdo,
                    "stone is empty");

                UpdateLevelingSpirit(
                    privateArea,
                    point,
                    false);

                return true;
            }

            if (!evaluation.Raising &&
                !HasStoneCapacity(
                    preparationInventory,
                    evaluation.StoneYield))
            {
                AdvanceLevelingScan(
                    zdo,
                    scanIndex,
                    scanProgress,
                    pointCount,
                    false);

                return true;
            }

            int pendingIndex = zdo.GetInt(
                TerritoryZdoKeys.TerraformingPendingScanIndex,
                -1);

            int verifyCount = zdo.GetInt(
                TerritoryZdoKeys.TerraformingVerifyCount,
                0);

            if (pendingIndex == scanIndex && verifyCount > 0)
            {
                float nextVerifyTime = zdo.GetFloat(
                    TerritoryZdoKeys.TerraformingNextVerifyTime,
                    0f);

                if (nextVerifyTime > 0f && Time.time < nextVerifyTime)
                    return true;
            }

            if (pendingIndex == scanIndex)
                verifyCount++;
            else
                verifyCount = 1;

            zdo.Set(
                TerritoryZdoKeys.TerraformingPendingScanIndex,
                scanIndex);

            zdo.Set(
                TerritoryZdoKeys.TerraformingVerifyCount,
                verifyCount);

            if (verifyCount < LevelingVerifyPasses)
            {
                zdo.Set(
                    TerritoryZdoKeys.TerraformingNextVerifyTime,
                    Time.time + LevelingVerifyDelay);

                zdo.Set(
                    TerritoryZdoKeys.TerraformingScanSpeed,
                    1f / LevelingScanTime * 0.35f);

                return true;
            }

            zdo.Set(
                TerritoryZdoKeys.TerraformingNextVerifyTime,
                0f);

            if (!ApplyWardHeightLevelingOperation(
                    point,
                    targetHeight))
            {
                AdvanceLevelingScan(
                    zdo,
                    scanIndex,
                    scanProgress,
                    pointCount,
                    false);

                return true;
            }

            if (!SpendLevelingFuelForWork(
                    zdo,
                    preparationInventory,
                    evaluation.WorkScore * LevelingTerrainFuelWorkMultiplier))
            {
                PersistTerraformingWorkerInventory(
                    zdo,
                    preparationInventory);

                PauseLevelingWorker(
                    zdo,
                    "fuel is empty");

                UpdateLevelingSpirit(
                    privateArea,
                    point,
                    false);

                return true;
            }

            if (evaluation.Raising)
            {
                ConsumeLevelingStone(
                    preparationInventory,
                    evaluation.StoneCost);
            }
            else
            {
                AddLevelingStone(
                    preparationInventory,
                    evaluation.StoneYield);
            }

            PersistTerraformingWorkerInventory(
                zdo,
                preparationInventory);

            zdo.Set(
                TerritoryZdoKeys.TerraformingPendingScanIndex,
                -1);

            zdo.Set(
                TerritoryZdoKeys.TerraformingVerifyCount,
                0);

            zdo.Set(
                TerritoryZdoKeys.TerraformingNextVerifyTime,
                0f);

            AdvanceLevelingScan(
                zdo,
                scanIndex,
                scanProgress,
                pointCount,
                true);

            ModLog.Debug(
                "[TerritoryTerraforming] Plateautem-style leveling step applied. target: " +
                targetHeight +
                ", point: " +
                point +
                ", netDelta: " +
                evaluation.NetDelta +
                ", workScore: " +
                evaluation.WorkScore +
                ", stoneYield: " +
                evaluation.StoneYield);

            return true;
        }

        private static void AdvanceLevelingScan(
            ZDO zdo,
            int scanIndex,
            float scanProgress,
            int pointCount,
            bool appliedWork)
        {
            if (zdo == null)
                return;

            int nextIndex = scanIndex + 1;

            if (nextIndex >= pointCount)
            {
                nextIndex = 0;
                scanProgress = 0f;
            }

            zdo.Set(
                TerritoryZdoKeys.TerraformingScanIndex,
                nextIndex);

            zdo.Set(
                TerritoryZdoKeys.TerraformingScanProgress,
                scanProgress);

            zdo.Set(
                TerritoryZdoKeys.TerraformingScanSpeed,
                appliedWork
                    ? 1f / LevelingFlatteningTime
                    : 1f / LevelingScanTime);

            if (!appliedWork)
            {
                zdo.Set(
                    TerritoryZdoKeys.TerraformingPendingScanIndex,
                    -1);

                zdo.Set(
                    TerritoryZdoKeys.TerraformingVerifyCount,
                    0);

                zdo.Set(
                    TerritoryZdoKeys.TerraformingNextVerifyTime,
                    0f);
            }
        }

        private static void UpdateLevelingSpirit(
            PrivateArea privateArea,
            Vector3 groundPoint,
            bool active)
        {
            if (privateArea == null)
                return;

            string wardId = GetWardRuntimeId(privateArea);

            if (string.IsNullOrEmpty(wardId))
                return;

            if (!active)
            {
                DestroyLevelingSpirit(wardId);
                return;
            }

            GameObject spirit = GetOrCreateLevelingSpirit(
                wardId);

            if (spirit == null)
                return;

            Vector3 position = groundPoint + Vector3.up * 1.35f;

            if (!spirit.activeSelf)
            {
                spirit.transform.position = position;
                spirit.SetActive(true);
                LevelingSpiritVelocityByWardId[wardId] = Vector3.zero;
                return;
            }

            Vector3 velocity;

            if (!LevelingSpiritVelocityByWardId.TryGetValue(wardId, out velocity))
                velocity = Vector3.zero;

            spirit.transform.position = Vector3.SmoothDamp(
                spirit.transform.position,
                position,
                ref velocity,
                LevelingSpiritSmoothTime,
                LevelingSpiritMaxSpeed,
                Time.deltaTime);

            LevelingSpiritVelocityByWardId[wardId] = velocity;
        }

        private static GameObject GetOrCreateLevelingSpirit(string wardId)
        {
            GameObject spirit;

            if (LevelingSpiritByWardId.TryGetValue(wardId, out spirit) &&
                spirit != null)
            {
                return spirit;
            }

            spirit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spirit.name = "ClanTerritory_LevelingSpirit_" + wardId;
            spirit.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);

            Collider collider = spirit.GetComponent<Collider>();

            if (collider != null)
                UnityEngine.Object.Destroy(collider);

            Renderer renderer = spirit.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material material = renderer.material;
                material.color = new Color(0.15f, 0.95f, 1f, 1f);
                material.EnableKeyword("_EMISSION");
                material.SetColor(
                    "_EmissionColor",
                    new Color(0.15f, 0.95f, 1f, 1f));
            }

            Light light = spirit.AddComponent<Light>();
            light.color = new Color(0.15f, 0.95f, 1f, 1f);
            light.range = 5f;
            light.intensity = 2.5f;

            LevelingSpiritByWardId[wardId] = spirit;
            return spirit;
        }

        private static void DestroyLevelingSpirit(string wardId)
        {
            if (string.IsNullOrEmpty(wardId))
                return;

            GameObject spirit;

            if (!LevelingSpiritByWardId.TryGetValue(wardId, out spirit))
                return;

            LevelingSpiritByWardId.Remove(wardId);
            LevelingSpiritVelocityByWardId.Remove(wardId);

            if (spirit != null)
                UnityEngine.Object.Destroy(spirit);
        }

        private static string GetWardRuntimeId(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return "";

            return zdo.m_uid.ToString();
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

            zdo.Set(
                TerritoryZdoKeys.TerraformingAxeStored,
                IsAxe(inventory.GetItemAt(2, 0)));
        }

        private static bool HasRequiredLevelingTools(Inventory inventory)
        {
            if (inventory == null)
                return false;

            return IsPickaxe(inventory.GetItemAt(0, 0)) &&
                   IsHoe(inventory.GetItemAt(1, 0));
        }


        private static bool HasTreeWorkerTool(Inventory inventory)
        {
            if (inventory == null)
                return false;

            return IsAxe(inventory.GetItemAt(2, 0));
        }

        private static bool HasAnyLevelingFuel(Inventory inventory)
        {
            return CountPreparationRowItems(
                       inventory,
                       1,
                       IsFuel) > 0;
        }

        private static bool HasLevelingFuelForWork(
            ZDO zdo,
            Inventory inventory,
            float workUnits)
        {
            if (!HasAnyLevelingFuel(inventory))
                return false;

            if (zdo == null)
                return true;

            float progress =
                zdo.GetFloat(
                    TerritoryZdoKeys.TerraformingFuelWorkProgress,
                    0f) +
                Mathf.Max(
                    0f,
                    workUnits);

            int requiredFuel = Mathf.FloorToInt(
                progress / LevelingFuelWorkPerItem);

            if (requiredFuel <= 0)
                return true;

            return CountPreparationRowItems(
                       inventory,
                       1,
                       IsFuel) >= requiredFuel;
        }

        private static bool SpendLevelingFuelForWork(
            ZDO zdo,
            Inventory inventory,
            float workUnits)
        {
            if (zdo == null || inventory == null)
                return false;

            float progress =
                zdo.GetFloat(
                    TerritoryZdoKeys.TerraformingFuelWorkProgress,
                    0f) +
                Mathf.Max(
                    0f,
                    workUnits);

            int requiredFuel = Mathf.FloorToInt(
                progress / LevelingFuelWorkPerItem);

            if (requiredFuel <= 0)
            {
                zdo.Set(
                    TerritoryZdoKeys.TerraformingFuelWorkProgress,
                    progress);

                return true;
            }

            if (!ConsumePreparationRowItem(
                    inventory,
                    1,
                    IsFuel,
                    requiredFuel))
            {
                return false;
            }

            zdo.Set(
                TerritoryZdoKeys.TerraformingFuelWorkProgress,
                progress - requiredFuel * LevelingFuelWorkPerItem);

            return true;
        }

        private static bool HasLevelingFuel(
            Inventory inventory,
            int amount)
        {
            return CountPreparationRowItems(
                       inventory,
                       1,
                       IsFuel) >= Mathf.Max(
                       1,
                       amount);
        }

        private static bool HasLevelingStone(
            Inventory inventory,
            int amount)
        {
            return CountPreparationRowItems(
                       inventory,
                       2,
                       IsStone) >= Mathf.Max(
                       1,
                       amount);
        }

        private static bool ConsumeLevelingFuel(
            Inventory inventory,
            int amount)
        {
            return ConsumePreparationRowItem(
                inventory,
                1,
                IsFuel,
                Mathf.Max(
                    LevelingFuelCost,
                    amount));
        }

        private static bool ConsumeLevelingStone(
            Inventory inventory,
            int amount)
        {
            return ConsumePreparationRowItem(
                inventory,
                2,
                IsStone,
                Mathf.Max(
                    LevelingStoneCostWhenRaising,
                    amount));
        }

        private static bool HasStoneCapacity(
            Inventory inventory,
            int amount)
        {
            if (inventory == null || amount <= 0)
                return true;

            int remaining = amount;

            for (int x = 0; x < PreparationChestWidth; x++)
            {
                ItemDrop.ItemData item = inventory.GetItemAt(x, 2);

                if (item == null)
                {
                    remaining -= SlotCapacity;
                }
                else if (IsStone(item))
                {
                    remaining -= Mathf.Max(
                        0,
                        SlotCapacity - item.m_stack);
                }

                if (remaining <= 0)
                    return true;
            }

            return false;
        }

        private static int AddLevelingStone(
            Inventory inventory,
            int amount)
        {
            if (inventory == null || amount <= 0)
                return 0;

            int remaining = amount;
            int added = 0;

            for (int x = 0; x < PreparationChestWidth && remaining > 0; x++)
            {
                ItemDrop.ItemData item = inventory.GetItemAt(x, 2);

                if (item == null || !IsStone(item))
                    continue;

                ApplyVirtualStackLimit(
                    item,
                    SlotCapacity);

                int moved = Mathf.Min(
                    remaining,
                    Mathf.Max(
                        0,
                        SlotCapacity - item.m_stack));

                if (moved <= 0)
                    continue;

                item.m_stack += moved;
                remaining -= moved;
                added += moved;
            }

            for (int x = 0; x < PreparationChestWidth && remaining > 0; x++)
            {
                if (inventory.GetItemAt(x, 2) != null)
                    continue;

                int moved = Mathf.Min(
                    remaining,
                    SlotCapacity);

                ItemDrop.ItemData item = CreateVirtualItemData(
                    "Stone",
                    moved,
                    0f,
                    new Vector2i(x, 2),
                    false,
                    1,
                    0,
                    0L,
                    "",
                    new Dictionary<string, string>(),
                    0,
                    false,
                    SlotCapacity);

                if (item == null)
                    continue;

                inventory.GetAllItems().Add(item);
                remaining -= moved;
                added += moved;
            }

            if (added > 0)
                InvokeInventoryChanged(inventory);

            return added;
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

        private static int CountPreparationRowItems(
            Inventory inventory,
            int row,
            System.Predicate<ItemDrop.ItemData> predicate)
        {
            if (inventory == null || predicate == null)
                return 0;

            int total = 0;

            for (int x = 0; x < PreparationChestWidth; x++)
            {
                ItemDrop.ItemData item = inventory.GetItemAt(x, row);

                if (item == null || item.m_stack <= 0)
                    continue;

                if (predicate(item))
                    total += item.m_stack;
            }

            return total;
        }

        private static bool ConsumePreparationRowItem(
            Inventory inventory,
            int row,
            System.Predicate<ItemDrop.ItemData> predicate,
            int amount)
        {
            if (inventory == null || amount <= 0 || predicate == null)
                return false;

            int remaining = amount;
            bool changed = false;

            for (int x = 0; x < PreparationChestWidth && remaining > 0; x++)
            {
                ItemDrop.ItemData item = inventory.GetItemAt(x, row);

                if (item == null || item.m_stack <= 0)
                    continue;

                if (!predicate(item))
                    continue;

                int consumed = Mathf.Min(
                    remaining,
                    item.m_stack);

                if (consumed <= 0)
                    continue;

                item.m_stack -= consumed;
                remaining -= consumed;
                changed = true;

                if (item.m_stack <= 0)
                    inventory.RemoveItem(item);
            }

            if (changed)
                InvokeInventoryChanged(inventory);

            return remaining <= 0;
        }

        private static bool TryChopTreeInTerritory(
            Vector3 center,
            float territoryRadius,
            Inventory preparationInventory)
        {
            ItemDrop.ItemData axe = preparationInventory != null
                ? preparationInventory.GetItemAt(2, 0)
                : null;

            if (!IsAxe(axe))
                return false;

            TreeBase bestTree = null;
            TreeLog bestLog = null;
            Destructible bestStump = null;
            Collider bestCollider = null;
            float bestDistance = float.MaxValue;

            TreeBase[] trees = UnityEngine.Object.FindObjectsOfType<TreeBase>();

            for (int i = 0; i < trees.Length; i++)
            {
                TreeBase tree = trees[i];

                if (!IsFullyGrownTreeBase(tree))
                    continue;

                Collider collider = FindNearestActiveCollider(
                    tree.gameObject,
                    center,
                    territoryRadius + LevelingTreeRadius);

                if (collider == null)
                    continue;

                Vector3 colliderPoint = GetColliderCenter(collider);

                if (!IsInsideTerraformingRadius(
                        center,
                        colliderPoint,
                        territoryRadius))
                {
                    continue;
                }

                float distance = global::Utils.DistanceXZ(
                    center,
                    colliderPoint);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCollider = collider;
                    bestTree = tree;
                    bestLog = null;
                    bestStump = null;
                }
            }

            TreeLog[] logs = UnityEngine.Object.FindObjectsOfType<TreeLog>();

            for (int i = 0; i < logs.Length; i++)
            {
                TreeLog log = logs[i];

                if (log == null || !log.gameObject.activeInHierarchy)
                    continue;

                Collider collider = FindNearestActiveCollider(
                    log.gameObject,
                    center,
                    territoryRadius + LevelingTreeRadius);

                if (collider == null)
                    continue;

                Vector3 colliderPoint = GetColliderCenter(collider);

                if (!IsInsideTerraformingRadius(
                        center,
                        colliderPoint,
                        territoryRadius))
                {
                    continue;
                }

                float distance = global::Utils.DistanceXZ(
                    center,
                    colliderPoint);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCollider = collider;
                    bestTree = null;
                    bestLog = log;
                    bestStump = null;
                }
            }

            Destructible[] destructibles = UnityEngine.Object.FindObjectsOfType<Destructible>();

            for (int i = 0; i < destructibles.Length; i++)
            {
                Destructible destructible = destructibles[i];

                if (!IsTreeStumpDestructible(destructible))
                    continue;

                Collider collider = FindNearestActiveCollider(
                    destructible.gameObject,
                    center,
                    territoryRadius + LevelingTreeRadius);

                if (collider == null)
                    continue;

                Vector3 colliderPoint = GetColliderCenter(collider);

                if (!IsInsideTerraformingRadius(
                        center,
                        colliderPoint,
                        territoryRadius))
                {
                    continue;
                }

                float distance = global::Utils.DistanceXZ(
                    center,
                    colliderPoint);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCollider = collider;
                    bestTree = null;
                    bestLog = null;
                    bestStump = destructible;
                }
            }

            if (bestCollider == null)
                return false;

            HitData hit = CreateLevelingAxeHit(
                axe,
                bestCollider);

            if (bestTree != null)
            {
                bestTree.Damage(hit);
                return true;
            }

            if (bestLog != null)
            {
                bestLog.Damage(hit);
                return true;
            }

            if (bestStump != null)
            {
                bestStump.Damage(hit);
                return true;
            }

            return false;
        }

        private static bool IsFullyGrownTreeBase(TreeBase tree)
        {
            if (tree == null || !tree.gameObject.activeInHierarchy)
                return false;

            if (tree.GetComponent<Growup>() != null)
                return false;

            if (tree.m_logPrefab == null)
                return false;

            if (tree.m_trunk == null || !tree.m_trunk.activeInHierarchy)
                return false;

            return true;
        }

        private static bool IsTreeStumpDestructible(Destructible destructible)
        {
            if (destructible == null || !destructible.gameObject.activeInHierarchy)
                return false;

            if (destructible.GetComponent<TreeBase>() != null ||
                destructible.GetComponent<TreeLog>() != null ||
                destructible.GetComponent<MineRock>() != null ||
                destructible.GetComponent<MineRock5>() != null ||
                destructible.GetComponent<Growup>() != null)
            {
                return false;
            }

            string prefabName = global::Utils.GetPrefabName(
                destructible.gameObject.name);

            return ContainsIgnoreCase(
                       prefabName,
                       "stump") ||
                   ContainsIgnoreCase(
                       prefabName,
                       "stub");
        }

        private static bool TryMineRockAtPoint(
            Vector3 point,
            Vector3 center,
            float territoryRadius,
            Inventory preparationInventory)
        {
            ItemDrop.ItemData pickaxe = preparationInventory != null
                ? preparationInventory.GetItemAt(0, 0)
                : null;

            if (!IsPickaxe(pickaxe))
                return false;

            Collider bestCollider = null;
            MineRock bestMineRock = null;
            MineRock5 bestMineRock5 = null;
            float bestDistance = float.MaxValue;

            MineRock[] mineRocks = UnityEngine.Object.FindObjectsOfType<MineRock>();

            for (int i = 0; i < mineRocks.Length; i++)
            {
                MineRock mineRock = mineRocks[i];

                if (mineRock == null)
                    continue;

                Collider collider = FindNearestActiveCollider(
                    mineRock.gameObject,
                    point,
                    LevelingMineRockRadius);

                if (collider == null)
                    continue;

                Vector3 colliderPoint = GetColliderCenter(collider);

                if (!IsInsideTerraformingRadius(
                        center,
                        colliderPoint,
                        territoryRadius))
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    point,
                    colliderPoint);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCollider = collider;
                    bestMineRock = mineRock;
                    bestMineRock5 = null;
                }
            }

            MineRock5[] mineRocks5 = UnityEngine.Object.FindObjectsOfType<MineRock5>();

            for (int i = 0; i < mineRocks5.Length; i++)
            {
                MineRock5 mineRock = mineRocks5[i];

                if (mineRock == null)
                    continue;

                Collider collider = FindNearestActiveCollider(
                    mineRock.gameObject,
                    point,
                    LevelingMineRockRadius);

                if (collider == null)
                    continue;

                Vector3 colliderPoint = GetColliderCenter(collider);

                if (!IsInsideTerraformingRadius(
                        center,
                        colliderPoint,
                        territoryRadius))
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    point,
                    colliderPoint);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCollider = collider;
                    bestMineRock = null;
                    bestMineRock5 = mineRock;
                }
            }

            if (bestCollider == null)
                return false;

            HitData hit = CreateLevelingPickaxeHit(
                pickaxe,
                bestCollider);

            if (bestMineRock != null)
            {
                bestMineRock.Damage(hit);
                return true;
            }

            if (bestMineRock5 != null)
            {
                bestMineRock5.Damage(hit);
                return true;
            }

            return false;
        }

        private static Collider FindNearestActiveCollider(
            GameObject root,
            Vector3 point,
            float maxDistance)
        {
            if (root == null)
                return null;

            Collider[] colliders = root.GetComponentsInChildren<Collider>();
            Collider nearest = null;
            float nearestDistance = maxDistance;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];

                if (collider == null ||
                    !collider.enabled ||
                    !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    point,
                    GetColliderCenter(collider));

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = collider;
                }
            }

            return nearest;
        }

        private static Vector3 GetColliderCenter(Collider collider)
        {
            if (collider == null)
                return Vector3.zero;

            Bounds bounds = collider.bounds;
            return bounds.center;
        }

        private static HitData CreateLevelingPickaxeHit(
            ItemDrop.ItemData pickaxe,
            Collider collider)
        {
            HitData hit = new HitData();
            hit.m_damage.m_pickaxe = GetPickaxeDamage(pickaxe);
            hit.m_toolTier = (short)GetPickaxeToolTier(pickaxe);
            hit.m_point = GetColliderCenter(collider);
            hit.m_dir = Vector3.down;
            hit.m_radius = LevelingMineRockHitRadius;
            hit.m_hitCollider = collider;
            hit.m_hitType = HitData.HitType.Structural;
            hit.m_skill = Skills.SkillType.Pickaxes;
            return hit;
        }

        private static HitData CreateLevelingAxeHit(
            ItemDrop.ItemData axe,
            Collider collider)
        {
            HitData hit = new HitData();
            hit.m_damage.m_chop = GetAxeDamage(axe);
            hit.m_toolTier = (short)GetAxeToolTier(axe);
            hit.m_point = GetColliderCenter(collider);
            hit.m_dir = Vector3.down;
            hit.m_radius = LevelingTreeHitRadius;
            hit.m_hitCollider = collider;
            hit.m_hitType = HitData.HitType.Structural;
            hit.m_skill = Skills.SkillType.WoodCutting;
            return hit;
        }

        private static float GetAxeDamage(ItemDrop.ItemData axe)
        {
            if (axe == null || axe.m_shared == null)
                return LevelingFallbackAxeDamage;

            float damage =
                axe.m_shared.m_damages.m_chop +
                axe.m_shared.m_damagesPerLevel.m_chop *
                Mathf.Max(
                    0,
                    axe.m_quality - 1);

            return Mathf.Max(
                LevelingFallbackAxeDamage,
                damage);
        }

        private static int GetAxeToolTier(ItemDrop.ItemData axe)
        {
            if (axe == null || axe.m_shared == null)
                return 0;

            return Mathf.Max(
                0,
                axe.m_shared.m_toolTier);
        }

        private static float GetPickaxeDamage(ItemDrop.ItemData pickaxe)
        {
            if (pickaxe == null || pickaxe.m_shared == null)
                return LevelingFallbackPickaxeDamage;

            float damage =
                pickaxe.m_shared.m_damages.m_pickaxe +
                pickaxe.m_shared.m_damagesPerLevel.m_pickaxe *
                Mathf.Max(
                    0,
                    pickaxe.m_quality - 1);

            return Mathf.Max(
                LevelingFallbackPickaxeDamage,
                damage);
        }

        private static int GetPickaxeToolTier(ItemDrop.ItemData pickaxe)
        {
            if (pickaxe == null || pickaxe.m_shared == null)
                return 0;

            return Mathf.Max(
                0,
                pickaxe.m_shared.m_toolTier);
        }

        private static bool TryEvaluateLevelingPoint(
            Vector3 point,
            float targetHeight,
            out LevelingEvaluation evaluation)
        {
            evaluation = new LevelingEvaluation();

            int sampleCount = Mathf.Max(
                1,
                CountPointsInSpiral(
                    LevelingSampleRadius,
                    LevelingLocalSampleSpacing));

            float raiseAmount = 0f;
            float lowerAmount = 0f;
            float workScore = 0f;
            int validSamples = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                float angle;
                float sampleRadius;

                PolarPointOnSpiral(
                    i,
                    LevelingLocalSampleSpacing,
                    out angle,
                    out sampleRadius);

                Vector3 samplePoint =
                    point +
                    new Vector3(
                        Mathf.Sin(angle) * sampleRadius,
                        0f,
                        Mathf.Cos(angle) * sampleRadius);

                float groundHeight;

                if (!TryGetGroundHeight(
                        samplePoint,
                        out groundHeight))
                {
                    continue;
                }

                validSamples++;

                float difference = targetHeight - groundHeight;

                if (Mathf.Abs(difference) <= LevelingTolerance)
                    continue;

                float amount = Mathf.Min(Mathf.Abs(difference), LevelingMaxDeltaPerOperation) * 0.35f;
                float weight =
                    Mathf.Pow(
                        Mathf.Clamp01(
                            1f - sampleRadius / LevelingSampleRadius),
                        0.3f);

                workScore += amount * weight;

                if (difference > 0f)
                    raiseAmount += amount;
                else
                    lowerAmount += amount;
            }

            if (validSamples <= 0)
                return false;

            float netDelta = raiseAmount - lowerAmount;

            if (Mathf.Abs(netDelta) <= LevelingTolerance)
                return false;

            if (workScore <= LevelingWorkThreshold)
                return false;

            evaluation.ShouldApply = true;
            evaluation.Raising = netDelta > 0f;
            evaluation.NetDelta = netDelta;
            evaluation.WorkScore = workScore;
            evaluation.RaiseAmount = raiseAmount;
            evaluation.LowerAmount = lowerAmount;
            evaluation.FuelCost = Mathf.Max(
                1,
                Mathf.CeilToInt(workScore * 0.5f));
            evaluation.StoneCost = evaluation.Raising
                ? Mathf.Max(
                    1,
                    Mathf.CeilToInt(raiseAmount * 0.5f))
                : 0;
            evaluation.StoneYield = evaluation.Raising
                ? 0
                : Mathf.Max(
                    1,
                    Mathf.CeilToInt(lowerAmount * LevelingStonePerLower));

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
            float scanProgress,
            float radius)
        {
            float angle;
            float spiralRadius;

            PolarPointOnSpiral(
                scanProgress,
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
                _wardHeightLevelingOperationActive = true;

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
            finally
            {
                _wardHeightLevelingOperationActive = false;
            }
        }

        public static bool ShouldUseWardHeightFalloffLeveling()
        {
            return _wardHeightLevelingOperationActive;
        }

        public static bool TryApplyWardHeightFalloffLevelTerrain(
            TerrainComp terrainComp,
            Vector3 worldPos,
            float radius,
            bool square)
        {
            if (terrainComp == null ||
                TerrainCompHeightmapField == null ||
                TerrainCompWidthField == null ||
                TerrainCompModifiedHeightField == null ||
                TerrainCompLevelDeltaField == null ||
                TerrainCompSmoothDeltaField == null)
            {
                return false;
            }

            Heightmap heightmap =
                TerrainCompHeightmapField.GetValue(terrainComp) as Heightmap;

            if (heightmap == null)
                return false;

            int width = (int)TerrainCompWidthField.GetValue(terrainComp);
            bool[] modifiedHeight =
                TerrainCompModifiedHeightField.GetValue(terrainComp) as bool[];
            float[] levelDelta =
                TerrainCompLevelDeltaField.GetValue(terrainComp) as float[];
            float[] smoothDelta =
                TerrainCompSmoothDeltaField.GetValue(terrainComp) as float[];

            if (modifiedHeight == null ||
                levelDelta == null ||
                smoothDelta == null)
            {
                return false;
            }

            int vertexX;
            int vertexY;

            heightmap.WorldToVertex(
                worldPos,
                out vertexX,
                out vertexY);

            Vector3 localTarget =
                worldPos -
                terrainComp.transform.position;

            float scaledRadius = radius / heightmap.m_scale;
            int radiusInVertices = Mathf.CeilToInt(scaledRadius);
            int vertexWidth = width + 1;
            Vector2 center =
                new Vector2(
                    vertexX,
                    vertexY);

            for (int y = vertexY - radiusInVertices; y <= vertexY + radiusInVertices; y++)
            {
                for (int x = vertexX - radiusInVertices; x <= vertexX + radiusInVertices; x++)
                {
                    float distance =
                        Vector2.Distance(
                            center,
                            new Vector2(
                                x,
                                y));

                    if ((!square && distance > scaledRadius) ||
                        x < 0 ||
                        y < 0 ||
                        x >= vertexWidth ||
                        y >= vertexWidth)
                    {
                        continue;
                    }

                    float weight = Mathf.Clamp01(1f - distance / Mathf.Max(0.001f, scaledRadius));
                    weight = Mathf.SmoothStep(
                        0f,
                        1f,
                        weight);

                    float height = heightmap.GetHeight(
                        x,
                        y);

                    int index = y * vertexWidth + x;
                    float rawDelta =
                        (localTarget.y - height) *
                        weight;

                    float delta = Mathf.Clamp(
                        rawDelta,
                        -LevelingMaxDeltaPerOperation,
                        LevelingMaxDeltaPerOperation);

                    if (Mathf.Abs(delta) <= 0.01f)
                        continue;

                    delta += smoothDelta[index] * 0.25f;
                    smoothDelta[index] = 0f;
                    levelDelta[index] += delta;
                    levelDelta[index] = Mathf.Clamp(
                        levelDelta[index],
                        -8f,
                        8f);
                    modifiedHeight[index] = true;
                }
            }

            return true;
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

        private static void ResetLevelingScan(ZDO zdo)
        {
            if (zdo == null)
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingScanProgress, 0f);
            zdo.Set(TerritoryZdoKeys.TerraformingScanIndex, 0);
            zdo.Set(TerritoryZdoKeys.TerraformingScanSpeed, 1f / LevelingScanTime);
            zdo.Set(TerritoryZdoKeys.TerraformingPendingScanIndex, -1);
            zdo.Set(TerritoryZdoKeys.TerraformingVerifyCount, 0);
            zdo.Set(TerritoryZdoKeys.TerraformingNextVerifyTime, 0f);
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

            zdo.Set(
                TerritoryZdoKeys.TerraformingPendingScanIndex,
                -1);

            zdo.Set(
                TerritoryZdoKeys.TerraformingVerifyCount,
                0);

            zdo.Set(
                TerritoryZdoKeys.TerraformingFuelWorkProgress,
                0f);

            zdo.Set(
                TerritoryZdoKeys.TerraformingNextVerifyTime,
                0f);

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
                zdo.GetBool(TerritoryZdoKeys.TerraformingAxeStored, false),
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

            GameObject chestObject = UnityEngine.Object.Instantiate(
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

            UnityEngine.Object.Destroy(gameObject);
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

            GameObject chestObject = UnityEngine.Object.Instantiate(
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

            GameObject chestObject = UnityEngine.Object.Instantiate(
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
                bool hiddenReservedTopCell = position.y == 0 && position.x >= 3;

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

                if (slot.x == 2)
                    return IsAxe(item);

                return false;
            }

            if (slot.y == 1)
                return IsFuel(item);

            if (slot.y == 2)
                return IsStone(item);

            return false;
        }

        private static int GetPreparationSlotCapacity(ItemDrop.ItemData item, Vector2i slot)
        {
            if (slot.y == 0)
                return 1;

            return 500;
        }

        private static int GetPreparationSlotCapacity(Vector2i slot)
        {
            return GetPreparationSlotCapacity(
                null,
                slot);
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

        private static bool IsAxe(ItemDrop.ItemData item)
        {
            string prefabName = GetItemPrefabName(item);
            string sharedName = GetItemSharedName(item);

            if (ContainsIgnoreCase(prefabName, "Pickaxe") ||
                ContainsIgnoreCase(sharedName, "pickaxe"))
            {
                return false;
            }

            return ContainsIgnoreCase(prefabName, "Axe") ||
                   ContainsIgnoreCase(sharedName, "axe");
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

            if (slot.y == 0 && slot.x == 2)
            {
                PlayerMessage("Only axe can be placed in this slot");
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
            {
                zdo.Set(TerritoryZdoKeys.TerraformingRunning, false);
                DestroyLevelingSpirit(GetWardRuntimeId(privateArea));
            }

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

            bool wasRunning = zdo.GetBool(
                TerritoryZdoKeys.TerraformingRunning,
                false);

            zdo.Set(TerritoryZdoKeys.TerraformingRunning, running);

            if (running && !wasRunning)
            {
                ResetLevelingScan(zdo);
                DestroyLevelingSpirit(GetWardRuntimeId(privateArea));
            }
            else if (!running)
            {
                DestroyLevelingSpirit(GetWardRuntimeId(privateArea));
                zdo.Set(TerritoryZdoKeys.TerraformingFuelWorkProgress, 0f);
                zdo.Set(TerritoryZdoKeys.TerraformingNextVerifyTime, 0f);
            }

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
            ResetLevelingScan(zdo);
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

            if (zdo.GetFloat(TerritoryZdoKeys.TerraformingScanSpeed, 0f) <= 0f)
                zdo.Set(TerritoryZdoKeys.TerraformingScanSpeed, 1f / LevelingScanTime);

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

            long creatorId = piece.GetCreator();
            long playerId = player.GetPlayerID();

            if (creatorId != playerId &&
                !TerritoryGuildAccess.HasGuildAccess(
                    privateArea,
                    player))
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. Player is not ward creator or guild member: " + wardId);
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

            long creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);

            if (creatorId != playerId &&
                !TerritoryGuildAccess.HasGuildAccess(
                    zdo,
                    playerId))
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. Player is not ward creator or guild member: " + actionName + ", playerId: " + playerId);
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

        public bool AxeStored { get; private set; }

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
            bool axeStored,
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
            AxeStored = axeStored;
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

namespace ClanTerritory.Features.BiomeDominion
{
    internal sealed class BiomeDominionRecord
    {
        public string BiomeName;
        public string GuildId;
        public string GuildName;
        public string GuildColor;
        public long ClaimedByPlayerId;
        public string ClaimedByPlayerName;
        public bool DoorLockEnabled;
        public bool StructureDamageProtectionEnabled;
        public int DoorAutoCloseSeconds;
        public string UpdatedAtUtc;

        public string DisplayName
        {
            get
            {
                return string.IsNullOrEmpty(GuildName)
                    ? GuildId
                    : GuildName;
            }
        }
    }

    internal sealed class BiomeDominionModule :
        IInitializable,
        IDisposableModule
    {
        private BiomeDominionService _service;
        private GameObject _runnerObject;
        private BiomeDominionRunner _runner;

        public void Initialize()
        {
            _service = new BiomeDominionService();
            _service.Initialize();

            ServiceContainer.Register<BiomeDominionService>(_service);

            _runnerObject = new GameObject("ClanTerritory_BiomeDominionRunner");
            UnityEngine.Object.DontDestroyOnLoad(_runnerObject);

            _runner = _runnerObject.AddComponent<BiomeDominionRunner>();
            _runner.Initialize(_service);

            ModLog.Info("[BiomeDominion] Module initialized.");
        }

        public void Shutdown()
        {
            if (_service != null)
                _service.Shutdown();

            if (_runnerObject != null)
                UnityEngine.Object.Destroy(_runnerObject);

            _runner = null;
            _runnerObject = null;
            _service = null;

            ModLog.Info("[BiomeDominion] Module shutdown.");
        }

        private sealed class BiomeDominionRunner : MonoBehaviour
        {
            private BiomeDominionService _service;

            public void Initialize(BiomeDominionService service)
            {
                _service = service;
            }

            private void Update()
            {
                if (_service != null)
                    _service.Update();
            }
        }
    }

    internal sealed class BiomeDominionService
    {
        private const string FileSuffix = ".biome_dominions.txt";

        private static readonly FieldInfo AllAreasField =
            AccessTools.Field(typeof(PrivateArea), "m_allAreas");

        private static readonly FieldInfo DoorZNetViewField =
            AccessTools.Field(typeof(Door), "m_nview");

        private static readonly MethodInfo DoorUpdateStateMethod =
            AccessTools.Method(typeof(Door), "UpdateState");

        private readonly Dictionary<string, BiomeDominionRecord> _recordsByBiome =
            new Dictionary<string, BiomeDominionRecord>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<ZDOID, ScheduledDoorClose> _scheduledDoorClosures =
            new Dictionary<ZDOID, ScheduledDoorClose>();

        private readonly PersistenceFileSystem _fileSystem =
            new PersistenceFileSystem();

        private bool _commandsRegistered;

        public void Initialize()
        {
            _fileSystem.EnsureDirectories();
            Load();

            if (!_commandsRegistered)
            {
                RegisterCommands();
                _commandsRegistered = true;
            }

            ModLog.Info("[BiomeDominion] Service initialized. Claims: " + _recordsByBiome.Count);
        }

        public void Shutdown()
        {
            Save();
            _recordsByBiome.Clear();
            _scheduledDoorClosures.Clear();

            ModLog.Info("[BiomeDominion] Service shutdown.");
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

        public bool TryGetDominionAt(Vector3 position, out BiomeDominionRecord record)
        {
            record = null;
            string biomeName = GetBiomeName(position);

            if (string.IsNullOrEmpty(biomeName))
                return false;

            return _recordsByBiome.TryGetValue(biomeName, out record);
        }

        public bool TryGetVassalStatus(PrivateArea privateArea, out BiomeDominionRecord record)
        {
            record = null;

            if (privateArea == null)
                return false;

            if (!TryGetDominionAt(privateArea.transform.position, out record))
                return false;

            string wardGuildId;

            if (!TryGetWardGuildId(privateArea, out wardGuildId))
                return true;

            if (string.IsNullOrEmpty(wardGuildId))
                return true;

            return !string.Equals(wardGuildId, record.GuildId, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDoorLockedForPlayer(Vector3 position, Player player)
        {
            if (player == null)
                return false;

            BiomeDominionRecord record;

            if (!TryGetDominionAt(position, out record))
                return false;

            if (!record.DoorLockEnabled)
                return false;

            if (IsPlayerInDominionGuild(player, record))
                return false;

            if (HasLocalTerritoryAccess(position, player))
                return false;

            return true;
        }

        public bool IsStructureDamageProtected(Vector3 position)
        {
            BiomeDominionRecord record;

            return TryGetDominionAt(position, out record) &&
                   record.StructureDamageProtectionEnabled;
        }

        public void ScheduleDoorAutoClose(Door door)
        {
            if (door == null)
                return;

            BiomeDominionRecord record;

            if (!TryGetDominionAt(door.transform.position, out record) ||
                !record.DoorLockEnabled)
            {
                RemoveScheduledDoor(door);
                return;
            }

            ZNetView zNetView = GetDoorZNetView(door);

            if (zNetView == null || !zNetView.IsValid() || !zNetView.IsOwner())
                return;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return;

            if (zdo.GetInt(ZDOVars.s_state) == 0)
            {
                _scheduledDoorClosures.Remove(zdo.m_uid);
                return;
            }

            int seconds = NormalizeDoorAutoCloseSeconds(record.DoorAutoCloseSeconds);
            float dueTime = Time.time + seconds;

            _scheduledDoorClosures[zdo.m_uid] = new ScheduledDoorClose(door, dueTime);

            ModLog.Debug("[BiomeDominion] Biome door auto-close scheduled: " + zdo.m_uid + ", biome: " + record.BiomeName + ", seconds: " + seconds);
        }

        private void RegisterCommands()
        {
            new Terminal.ConsoleCommand(
                "ctbiome",
                "Clan Territory biome dominion commands",
                HandleBiomeCommand,
                false,
                false,
                false,
                false,
                true);
        }

        private object HandleBiomeCommand(Terminal.ConsoleEventArgs args)
        {
            if (args == null)
                return false;

            if (args.Length <= 1 || IsHelp(args[1]))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.help"));
                return true;
            }

            string action = args[1].ToLowerInvariant();

            if (action == "claim")
                return ClaimCurrentBiome(args);

            if (action == "release")
                return ReleaseCurrentBiome(args);

            if (action == "status")
                return ShowCurrentBiomeStatus(args);

            if (action == "list")
                return ListDominions(args);

            if (action == "set")
                return SetRule(args);

            Reply(args, CtLocalization.Get("ct.biome.command.help"));
            return true;
        }

        private object ClaimCurrentBiome(Terminal.ConsoleEventArgs args)
        {
            if (!IsServerOrSinglePlayer())
            {
                Reply(args, CtLocalization.Get("ct.biome.command.server_only"));
                return true;
            }

            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Reply(args, CtLocalization.Get("ct.biome.command.no_player"));
                return true;
            }

            IGuildService guildService;

            if (!TryGetGuildService(out guildService))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.no_guilds"));
                return true;
            }

            long playerId = player.GetPlayerID();
            string guildId;
            string guildName;
            string guildColor;

            if (!guildService.TryGetPlayerGuildId(playerId, out guildId) || string.IsNullOrEmpty(guildId))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.no_guild"));
                return true;
            }

            if (!guildService.TryGetPlayerGuildName(playerId, out guildName) || string.IsNullOrEmpty(guildName))
                guildName = guildId;

            guildService.TryGetPlayerGuildColor(playerId, out guildColor);

            if (!guildService.IsPlayerGuildLeader(playerId))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.leader_only"));
                return true;
            }

            string biomeName = GetBiomeName(player.transform.position);

            if (string.IsNullOrEmpty(biomeName) || biomeName == Heightmap.Biome.None.ToString())
            {
                Reply(args, CtLocalization.Get("ct.biome.command.no_biome"));
                return true;
            }

            BiomeDominionRecord existing;

            if (_recordsByBiome.TryGetValue(biomeName, out existing) &&
                !string.Equals(existing.GuildId, guildId, StringComparison.OrdinalIgnoreCase))
            {
                Reply(args, CtLocalization.Format("ct.biome.command.already_claimed", biomeName, existing.DisplayName));
                return true;
            }

            BiomeDominionRecord record = new BiomeDominionRecord();
            record.BiomeName = biomeName;
            record.GuildId = guildId;
            record.GuildName = guildName;
            record.GuildColor = guildColor ?? "";
            record.ClaimedByPlayerId = playerId;
            record.ClaimedByPlayerName = player.GetPlayerName();
            record.DoorLockEnabled = existing != null && existing.DoorLockEnabled;
            record.StructureDamageProtectionEnabled = existing != null && existing.StructureDamageProtectionEnabled;
            record.DoorAutoCloseSeconds = existing != null ? NormalizeDoorAutoCloseSeconds(existing.DoorAutoCloseSeconds) : ClanTerritory.Config.ConfigValues.DoorAutoCloseSeconds;
            record.UpdatedAtUtc = DateTime.UtcNow.ToString("o");

            _recordsByBiome[biomeName] = record;
            Save();

            Reply(args, CtLocalization.Format("ct.biome.command.claimed", biomeName, guildName));
            ModLog.Info("[BiomeDominion] Biome claimed: " + biomeName + ", guild: " + guildName + ", player: " + player.GetPlayerName());
            return true;
        }

        private object ReleaseCurrentBiome(Terminal.ConsoleEventArgs args)
        {
            if (!IsServerOrSinglePlayer())
            {
                Reply(args, CtLocalization.Get("ct.biome.command.server_only"));
                return true;
            }

            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Reply(args, CtLocalization.Get("ct.biome.command.no_player"));
                return true;
            }

            string biomeName = GetBiomeName(player.transform.position);
            BiomeDominionRecord record;

            if (string.IsNullOrEmpty(biomeName) || !_recordsByBiome.TryGetValue(biomeName, out record))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.not_claimed"));
                return true;
            }

            if (!CanManageDominion(player, record))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.not_owner"));
                return true;
            }

            _recordsByBiome.Remove(biomeName);
            Save();

            Reply(args, CtLocalization.Format("ct.biome.command.released", biomeName));
            ModLog.Info("[BiomeDominion] Biome released: " + biomeName + ", guild: " + record.DisplayName);
            return true;
        }

        private object ShowCurrentBiomeStatus(Terminal.ConsoleEventArgs args)
        {
            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Reply(args, CtLocalization.Get("ct.biome.command.no_player"));
                return true;
            }

            string biomeName = GetBiomeName(player.transform.position);
            BiomeDominionRecord record;

            if (string.IsNullOrEmpty(biomeName) || !_recordsByBiome.TryGetValue(biomeName, out record))
            {
                Reply(args, CtLocalization.Format("ct.biome.command.status_free", biomeName));
                return true;
            }

            Reply(args, CtLocalization.Format(
                "ct.biome.command.status_claimed",
                record.BiomeName,
                record.DisplayName,
                FormatBool(record.DoorLockEnabled),
                FormatBool(record.StructureDamageProtectionEnabled),
                NormalizeDoorAutoCloseSeconds(record.DoorAutoCloseSeconds)));
            return true;
        }

        private object ListDominions(Terminal.ConsoleEventArgs args)
        {
            if (_recordsByBiome.Count <= 0)
            {
                Reply(args, CtLocalization.Get("ct.biome.command.list_empty"));
                return true;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(CtLocalization.Get("ct.biome.command.list_header"));

            foreach (BiomeDominionRecord record in _recordsByBiome.Values)
            {
                builder.AppendLine();
                builder.Append("- ");
                builder.Append(record.BiomeName);
                builder.Append(": ");
                builder.Append(record.DisplayName);
                builder.Append(" | doorlock=");
                builder.Append(FormatBool(record.DoorLockEnabled));
                builder.Append(", protection=");
                builder.Append(FormatBool(record.StructureDamageProtectionEnabled));
                builder.Append(", autoclose=");
                builder.Append(NormalizeDoorAutoCloseSeconds(record.DoorAutoCloseSeconds));
                builder.Append("s");
            }

            Reply(args, builder.ToString());
            return true;
        }

        private object SetRule(Terminal.ConsoleEventArgs args)
        {
            if (!IsServerOrSinglePlayer())
            {
                Reply(args, CtLocalization.Get("ct.biome.command.server_only"));
                return true;
            }

            if (args.Length < 4)
            {
                Reply(args, CtLocalization.Get("ct.biome.command.set_help"));
                return true;
            }

            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Reply(args, CtLocalization.Get("ct.biome.command.no_player"));
                return true;
            }

            string biomeName = GetBiomeName(player.transform.position);
            BiomeDominionRecord record;

            if (string.IsNullOrEmpty(biomeName) || !_recordsByBiome.TryGetValue(biomeName, out record))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.not_claimed"));
                return true;
            }

            if (!CanManageDominion(player, record))
            {
                Reply(args, CtLocalization.Get("ct.biome.command.not_owner"));
                return true;
            }

            string rule = args[2].ToLowerInvariant();
            string value = args[3].ToLowerInvariant();

            if (rule == "doorlock" || rule == "doors")
            {
                bool enabled;
                if (!TryParseBool(value, out enabled))
                {
                    Reply(args, CtLocalization.Get("ct.biome.command.invalid_value"));
                    return true;
                }

                record.DoorLockEnabled = enabled;
                record.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
                Save();
                Reply(args, CtLocalization.Format("ct.biome.command.rule_saved", record.BiomeName, "doorlock", FormatBool(enabled)));
                return true;
            }

            if (rule == "protection" || rule == "structures")
            {
                bool enabled;
                if (!TryParseBool(value, out enabled))
                {
                    Reply(args, CtLocalization.Get("ct.biome.command.invalid_value"));
                    return true;
                }

                record.StructureDamageProtectionEnabled = enabled;
                record.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
                Save();
                Reply(args, CtLocalization.Format("ct.biome.command.rule_saved", record.BiomeName, "protection", FormatBool(enabled)));
                return true;
            }

            if (rule == "autoclose")
            {
                int seconds;
                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds))
                {
                    Reply(args, CtLocalization.Get("ct.biome.command.invalid_value"));
                    return true;
                }

                record.DoorAutoCloseSeconds = NormalizeDoorAutoCloseSeconds(seconds);
                record.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
                Save();
                Reply(args, CtLocalization.Format("ct.biome.command.autoclose_saved", record.BiomeName, record.DoorAutoCloseSeconds));
                return true;
            }

            Reply(args, CtLocalization.Get("ct.biome.command.invalid_rule"));
            return true;
        }

        private bool CanManageDominion(Player player, BiomeDominionRecord record)
        {
            if (player == null || record == null)
                return false;

            IGuildService guildService;
            if (!TryGetGuildService(out guildService))
                return false;

            long playerId = player.GetPlayerID();
            string guildId;

            if (!guildService.TryGetPlayerGuildId(playerId, out guildId))
                return false;

            if (!string.Equals(guildId, record.GuildId, StringComparison.OrdinalIgnoreCase))
                return false;

            return guildService.IsPlayerGuildLeader(playerId);
        }

        private bool IsPlayerInDominionGuild(Player player, BiomeDominionRecord record)
        {
            if (player == null || record == null)
                return false;

            IGuildService guildService;
            if (!TryGetGuildService(out guildService))
                return false;

            string guildId;
            return guildService.TryGetPlayerGuildId(player.GetPlayerID(), out guildId) &&
                   string.Equals(guildId, record.GuildId, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasLocalTerritoryAccess(Vector3 position, Player player)
        {
            if (player == null)
                return false;

            List<PrivateArea> areas = GetPrivateAreas();

            for (int i = 0; i < areas.Count; i++)
            {
                PrivateArea privateArea = areas[i];

                if (privateArea == null)
                    continue;

                if (!IsInside(privateArea, position))
                    continue;

                if (HasPrivateAreaAccess(privateArea, player))
                    return true;
            }

            return false;
        }

        private static bool HasPrivateAreaAccess(PrivateArea privateArea, Player player)
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

            return TerritoryGuildAccess.HasGuildAccess(privateArea, player);
        }

        private static bool TryGetWardGuildId(PrivateArea privateArea, out string guildId)
        {
            guildId = "";
            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return false;

            return TerritoryGuildAccess.TryGetWardGuildId(zdo.m_uid.ToString(), out guildId);
        }

        private static bool TryGetGuildService(out IGuildService guildService)
        {
            guildService = null;
            return ServiceContainer.TryGet<IGuildService>(out guildService) &&
                   guildService != null &&
                   guildService.IsAvailable;
        }

        private static string GetBiomeName(Vector3 position)
        {
            try
            {
                Heightmap.Biome biome = Heightmap.Biome.None;

                if (WorldGenerator.instance != null)
                    biome = WorldGenerator.instance.GetBiome(position);
                else if (Player.m_localPlayer != null)
                    biome = Player.m_localPlayer.GetCurrentBiome();

                return biome.ToString();
            }
            catch (Exception exception)
            {
                ModLog.Warning("[BiomeDominion] Failed to resolve biome: " + exception.Message);
                return Heightmap.Biome.None.ToString();
            }
        }

        private static bool IsInside(PrivateArea privateArea, Vector3 position)
        {
            if (privateArea == null)
                return false;

            return global::Utils.DistanceXZ(privateArea.transform.position, position) < privateArea.m_radius;
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

        private void RemoveScheduledDoor(Door door)
        {
            ZNetView zNetView = GetDoorZNetView(door);

            if (zNetView == null || !zNetView.IsValid())
                return;

            ZDO zdo = zNetView.GetZDO();

            if (zdo != null)
                _scheduledDoorClosures.Remove(zdo.m_uid);
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

            BiomeDominionRecord record;
            if (!TryGetDominionAt(door.transform.position, out record) || !record.DoorLockEnabled)
                return;

            zdo.Set(ZDOVars.s_state, 0);

            if (DoorUpdateStateMethod != null)
                DoorUpdateStateMethod.Invoke(door, null);

            ModLog.Debug("[BiomeDominion] Biome door auto-closed: " + zdo.m_uid);
        }

        private void Load()
        {
            _recordsByBiome.Clear();
            string path = GetSavePath();

            if (!File.Exists(path))
            {
                ModLog.Info("[BiomeDominion] No biome dominion save found: " + path);
                return;
            }

            BiomeDominionRecord current = null;
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);

            for (int i = 0; i < lines.Length; i++)
            {
                string rawLine = lines[i];

                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                string line = rawLine.Trim();

                if (line.StartsWith("#", StringComparison.Ordinal) || line.StartsWith("//", StringComparison.Ordinal))
                    continue;

                if (line.StartsWith("[Biome:", StringComparison.OrdinalIgnoreCase) && line.EndsWith("]", StringComparison.Ordinal))
                {
                    if (current != null && !string.IsNullOrEmpty(current.BiomeName))
                        _recordsByBiome[current.BiomeName] = current;

                    current = new BiomeDominionRecord();
                    current.BiomeName = Unescape(line.Substring("[Biome:".Length, line.Length - "[Biome:".Length - 1));
                    current.DoorAutoCloseSeconds = ClanTerritory.Config.ConfigValues.DoorAutoCloseSeconds;
                    continue;
                }

                if (current == null)
                    continue;

                int separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;

                string key = line.Substring(0, separator).Trim();
                string value = Unescape(line.Substring(separator + 1).Trim());
                ApplyLoadedValue(current, key, value);
            }

            if (current != null && !string.IsNullOrEmpty(current.BiomeName))
                _recordsByBiome[current.BiomeName] = current;

            ModLog.Info("[BiomeDominion] Biome dominions loaded. Count: " + _recordsByBiome.Count);
        }

        private void Save()
        {
            try
            {
                _fileSystem.EnsureDirectories();
                string path = GetSavePath();
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("# Clan Territory biome dominions");
                builder.AppendLine("# One claimed biome per section.");
                builder.AppendLine();

                foreach (BiomeDominionRecord record in _recordsByBiome.Values)
                {
                    builder.AppendLine("[Biome:" + Escape(record.BiomeName) + "]");
                    builder.AppendLine("GuildId=" + Escape(record.GuildId));
                    builder.AppendLine("GuildName=" + Escape(record.GuildName));
                    builder.AppendLine("GuildColor=" + Escape(record.GuildColor));
                    builder.AppendLine("ClaimedByPlayerId=" + record.ClaimedByPlayerId);
                    builder.AppendLine("ClaimedByPlayerName=" + Escape(record.ClaimedByPlayerName));
                    builder.AppendLine("DoorLockEnabled=" + record.DoorLockEnabled);
                    builder.AppendLine("StructureDamageProtectionEnabled=" + record.StructureDamageProtectionEnabled);
                    builder.AppendLine("DoorAutoCloseSeconds=" + NormalizeDoorAutoCloseSeconds(record.DoorAutoCloseSeconds));
                    builder.AppendLine("UpdatedAtUtc=" + Escape(record.UpdatedAtUtc));
                    builder.AppendLine();
                }

                File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
                ModLog.Info("[BiomeDominion] Biome dominions saved. Count: " + _recordsByBiome.Count);
            }
            catch (Exception exception)
            {
                ModLog.Warning("[BiomeDominion] Failed to save biome dominions: " + exception.Message);
            }
        }

        private string GetSavePath()
        {
            string worldName = "Unknown";
            IWorldInfoService worldInfoService;

            if (ServiceContainer.TryGet<IWorldInfoService>(out worldInfoService) && worldInfoService != null)
                worldName = worldInfoService.GetWorldName();

            if (string.IsNullOrWhiteSpace(worldName))
                worldName = "Unknown";

            return Path.Combine(_fileSystem.WorldsDirectory, worldName + FileSuffix);
        }

        private static void ApplyLoadedValue(BiomeDominionRecord record, string key, string value)
        {
            if (record == null || string.IsNullOrEmpty(key))
                return;

            if (string.Equals(key, "GuildId", StringComparison.OrdinalIgnoreCase)) { record.GuildId = value; return; }
            if (string.Equals(key, "GuildName", StringComparison.OrdinalIgnoreCase)) { record.GuildName = value; return; }
            if (string.Equals(key, "GuildColor", StringComparison.OrdinalIgnoreCase)) { record.GuildColor = value; return; }
            if (string.Equals(key, "ClaimedByPlayerName", StringComparison.OrdinalIgnoreCase)) { record.ClaimedByPlayerName = value; return; }
            if (string.Equals(key, "UpdatedAtUtc", StringComparison.OrdinalIgnoreCase)) { record.UpdatedAtUtc = value; return; }

            if (string.Equals(key, "ClaimedByPlayerId", StringComparison.OrdinalIgnoreCase))
            {
                long playerId;
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out playerId))
                    record.ClaimedByPlayerId = playerId;
                return;
            }

            if (string.Equals(key, "DoorLockEnabled", StringComparison.OrdinalIgnoreCase))
            {
                bool enabled;
                if (bool.TryParse(value, out enabled))
                    record.DoorLockEnabled = enabled;
                return;
            }

            if (string.Equals(key, "StructureDamageProtectionEnabled", StringComparison.OrdinalIgnoreCase))
            {
                bool enabled;
                if (bool.TryParse(value, out enabled))
                    record.StructureDamageProtectionEnabled = enabled;
                return;
            }

            if (string.Equals(key, "DoorAutoCloseSeconds", StringComparison.OrdinalIgnoreCase))
            {
                int seconds;
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds))
                    record.DoorAutoCloseSeconds = NormalizeDoorAutoCloseSeconds(seconds);
            }
        }

        private static bool IsServerOrSinglePlayer()
        {
            return ZNet.instance == null || ZNet.instance.IsServer();
        }

        private static bool IsHelp(string value)
        {
            return string.IsNullOrEmpty(value) || value == "help" || value == "?";
        }

        private static bool TryParseBool(string value, out bool result)
        {
            result = false;
            if (string.IsNullOrEmpty(value))
                return false;

            value = value.Trim().ToLowerInvariant();

            if (value == "on" || value == "true" || value == "1" || value == "yes" || value == "enable" || value == "enabled")
            {
                result = true;
                return true;
            }

            if (value == "off" || value == "false" || value == "0" || value == "no" || value == "disable" || value == "disabled")
            {
                result = false;
                return true;
            }

            return false;
        }

        private static string FormatBool(bool value)
        {
            return value ? CtLocalization.Get("ct.menu.value.enabled") : CtLocalization.Get("ct.menu.value.disabled");
        }

        private static int NormalizeDoorAutoCloseSeconds(int seconds)
        {
            if (seconds < 3)
                return 3;
            if (seconds > 10)
                return 10;
            return seconds;
        }

        private static string Escape(string value)
        {
            if (value == null)
                return "";

            return value.Replace("\\", "\\\\").Replace("\r", "").Replace("\n", "\\n").Replace("]", "\\]");
        }

        private static string Unescape(string value)
        {
            if (value == null)
                return "";

            return value.Replace("\\]", "]").Replace("\\n", "\n").Replace("\\\\", "\\");
        }

        private static void Reply(Terminal.ConsoleEventArgs args, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (args != null && args.Context != null)
                args.Context.AddString(message);

            if (Player.m_localPlayer != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, message);
        }

        private sealed class ScheduledDoorClose
        {
            public Door Door { get; private set; }
            public float DueTime { get; private set; }

            public ScheduledDoorClose(Door door, float dueTime)
            {
                Door = door;
                DueTime = dueTime;
            }
        }
    }
}
