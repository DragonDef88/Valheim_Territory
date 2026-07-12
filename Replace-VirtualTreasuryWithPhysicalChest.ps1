param(
    [string]$RepositoryPath = "D:\Git\ClanTerritory"
)

$ErrorActionPreference = "Stop"

function Normalize-NewLines([string]$Text) {
    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function Replace-Once(
    [string]$Text,
    [string]$OldText,
    [string]$NewText,
    [string]$Label
) {
    $oldNormalized = Normalize-NewLines $OldText
    $newNormalized = Normalize-NewLines $NewText

    $first = $Text.IndexOf(
        $oldNormalized,
        [System.StringComparison]::Ordinal
    )

    if ($first -lt 0) {
        if ($Text.Contains($newNormalized)) {
            return $Text
        }

        throw "Expected block not found: $Label"
    }

    $second = $Text.IndexOf(
        $oldNormalized,
        $first + $oldNormalized.Length,
        [System.StringComparison]::Ordinal
    )

    if ($second -ge 0) {
        throw "Expected exactly one block: $Label"
    }

    return $Text.Substring(0, $first) +
        $newNormalized +
        $Text.Substring($first + $oldNormalized.Length)
}

function Write-Utf8NoBom(
    [string]$Path,
    [string]$Content,
    [string]$NewLine
) {
    $utf8NoBom =
        New-Object System.Text.UTF8Encoding($false)

    $normalized =
        Normalize-NewLines $Content

    [System.IO.Directory]::CreateDirectory(
        [System.IO.Path]::GetDirectoryName($Path)
    ) | Out-Null

    [System.IO.File]::WriteAllText(
        $Path,
        $normalized.Replace("`n", $NewLine),
        $utf8NoBom
    )
}

if (-not (Test-Path -LiteralPath $RepositoryPath -PathType Container)) {
    throw "Repository path not found: $RepositoryPath"
}

if (-not (Test-Path -LiteralPath (Join-Path $RepositoryPath ".git") -PathType Container)) {
    throw "Not a Git repository: $RepositoryPath"
}

Push-Location $RepositoryPath

try {
    $workingTree = & git status --porcelain

    if ($LASTEXITCODE -ne 0) {
        throw "git status failed."
    }

    if ($workingTree) {
        throw "Working tree is not clean. Commit or restore existing changes first."
    }

    $modulePath = Join-Path $RepositoryPath `
        "Source\ClanTerritory\Features\Territory\TerritoryModule.cs"

    $keysPath = Join-Path $RepositoryPath `
        "Source\ClanTerritory\Features\Territory\TerritoryZdoKeys.cs"

    $projectPath = Join-Path $RepositoryPath `
        "Source\ClanTerritory\ClanTerritory.csproj"

    $servicePath = Join-Path $RepositoryPath `
        "Source\ClanTerritory\Features\Territory\Services\PhysicalTreasuryService.cs"

    $researchPath = Join-Path $RepositoryPath `
        "Docs\Research\Valheim\090_Physical_Treasury_Chest_Linked_To_Ward.md"

    foreach ($requiredPath in @($modulePath, $keysPath, $projectPath)) {
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            throw "Required file not found: $requiredPath"
        }
    }

    $moduleOriginal =
        [System.IO.File]::ReadAllText($modulePath)

    $moduleNewLine =
        if ($moduleOriginal.Contains("`r`n")) {
            "`r`n"
        }
        else {
            "`n"
        }

    $module = Normalize-NewLines $moduleOriginal

    $module = Replace-Once $module @'
        private TerritoryRuleService _ruleService;
        private TerritoryTerraformingService _terraformingService;
'@ @'
        private TerritoryRuleService _ruleService;
        private PhysicalTreasuryService _physicalTreasuryService;
        private TerritoryTerraformingService _terraformingService;
'@ "module service field"

    $module = Replace-Once $module @'
            _ruleService = new TerritoryRuleService();
            _terraformingService = new TerritoryTerraformingService();
'@ @'
            _ruleService = new TerritoryRuleService();
            _physicalTreasuryService =
                new PhysicalTreasuryService();
            _terraformingService = new TerritoryTerraformingService();
'@ "module service creation"

    $module = Replace-Once $module @'
            ServiceContainer.Register<TerritoryRuleService>(_ruleService);
            ServiceContainer.Register<TerritoryTerraformingService>(_terraformingService);
'@ @'
            ServiceContainer.Register<TerritoryRuleService>(_ruleService);
            ServiceContainer.Register<PhysicalTreasuryService>(
                _physicalTreasuryService);
            ServiceContainer.Register<TerritoryTerraformingService>(_terraformingService);
'@ "module service registration"

    $module = Replace-Once $module @'
                eventBus.Subscribe<WardRegisteredEvent>(_service);
                eventBus.Subscribe<WardDestroyedEvent>(_service);
                eventBus.Subscribe<TerritoryRadiusChangedEvent>(_service);
'@ @'
                eventBus.Subscribe<WardRegisteredEvent>(_service);
                eventBus.Subscribe<WardDestroyedEvent>(_service);
                eventBus.Subscribe<WardDestroyedEvent>(
                    _physicalTreasuryService);
                eventBus.Subscribe<TerritoryRadiusChangedEvent>(_service);
'@ "treasury destruction subscription"

    $module = Replace-Once $module @'
            _runner.Initialize(
                _ruleService,
                _terraformingService,
                _presenceService);
'@ @'
            _runner.Initialize(
                _ruleService,
                _physicalTreasuryService,
                _terraformingService,
                _presenceService);
'@ "runner initialization"

    $module = Replace-Once $module @'
            _presenceService = null;
            _terraformingService = null;
            _ruleService = null;
'@ @'
            _presenceService = null;
            _terraformingService = null;
            _physicalTreasuryService = null;
            _ruleService = null;
'@ "module shutdown"

    $module = Replace-Once $module @'
            private TerritoryRuleService _ruleService;
            private TerritoryTerraformingService _terraformingService;
            private TerritoryPresenceService _presenceService;
'@ @'
            private TerritoryRuleService _ruleService;
            private PhysicalTreasuryService _physicalTreasuryService;
            private TerritoryTerraformingService _terraformingService;
            private TerritoryPresenceService _presenceService;
'@ "runner fields"

    $module = Replace-Once $module @'
            public void Initialize(
                TerritoryRuleService ruleService,
                TerritoryTerraformingService terraformingService,
                TerritoryPresenceService presenceService)
'@ @'
            public void Initialize(
                TerritoryRuleService ruleService,
                PhysicalTreasuryService physicalTreasuryService,
                TerritoryTerraformingService terraformingService,
                TerritoryPresenceService presenceService)
'@ "runner parameters"

    $module = Replace-Once $module @'
                _ruleService = ruleService;
                _terraformingService = terraformingService;
                _presenceService = presenceService;
'@ @'
                _ruleService = ruleService;
                _physicalTreasuryService =
                    physicalTreasuryService;
                _terraformingService = terraformingService;
                _presenceService = presenceService;
'@ "runner assignment"

    $module = Replace-Once $module @'
                if (_ruleService != null)
                    _ruleService.Update();

                if (_terraformingService != null)
                    _terraformingService.Update();
'@ @'
                if (_ruleService != null)
                    _ruleService.Update();

                if (_physicalTreasuryService != null)
                    _physicalTreasuryService.Update();

                if (_terraformingService != null)
                    _terraformingService.Update();
'@ "runner update"

    $module = Replace-Once $module `
        '        private const bool BuiltInTerraformingEnabled = false;' `
        '        private static readonly bool BuiltInTerraformingEnabled = false;' `
        "compile-time disabled flag"

    $module = Replace-Once $module @'
            Inventory preparationInventory = CreateTerraformingWorkerInventory(zdo);
            Inventory treasuryInventory = CreateVirtualStorageInventory(
                "Territory Treasury",
                TreasuryFallbackWidth,
                TreasuryFallbackHeight,
                zdo,
                TerritoryZdoKeys.TreasuryChestItems);
            bool preparationChanged = false;
            bool treasuryChanged = false;
'@ @'
            Inventory preparationInventory =
                CreateTerraformingWorkerInventory(zdo);

            Inventory treasuryInventory =
                PhysicalTreasuryService.GetTreasuryInventory(
                    privateArea);

            bool preparationChanged = false;
'@ "physical treasury absorption inventory"

    $module = Replace-Once $module @'
                if (absorbed <= 0 &&
                    TryAbsorbIntoExistingTreasuryStack(
                        treasuryInventory,
                        drop.m_itemData,
                        out absorbed))
                {
                    treasuryChanged = true;
                }
'@ @'
                if (absorbed <= 0)
                {
                    TryAbsorbIntoExistingTreasuryStack(
                        treasuryInventory,
                        drop.m_itemData,
                        out absorbed);
                }
'@ "physical treasury absorb operation"

    $module = Replace-Once $module @'
            if (treasuryChanged)
            {
                PersistVirtualInventoryToZdo(
                    zdo,
                    treasuryInventory,
                    TerritoryZdoKeys.TreasuryChestItems);
            }

'@ "" "remove virtual treasury persistence"

    $module = Replace-Once $module @'
            if (!drop.CanPickup(false))
            {
                drop.RequestOwn();
                return false;
            }
'@ @'
            if (!PhysicalTreasuryService
                .EnsureGroundItemOwnership(drop))
            {
                return false;
            }
'@ "safe ground item ownership"

    $module = Replace-Once $module @'
        public bool RequestOpenTreasuryChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (!ValidateCreatorAction("OpenTreasuryChest", wardId, privateArea, player))
                return false;

            if (InventoryGui.instance == null)
                return false;

            if (IsVirtualContainerBlockedByOpenInventoryGui(
                    "OpenTreasuryChest",
                    wardId,
                    player))
            {
                return false;
            }

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
'@ @'
        public bool RequestOpenTreasuryChest(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            PhysicalTreasuryService service;

            if (!ServiceContainer.TryGet<
                    PhysicalTreasuryService>(
                    out service) ||
                service == null)
            {
                return false;
            }

            return service.RequestOpen(
                wardId,
                privateArea,
                player);
        }
'@ "physical treasury open request"

    $module = Replace-Once $module @'
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
'@ @'
        internal static void ConfigureTreasuryChest(
            Container container)
        {
            if (container == null)
                return;

            container.m_name = "Territory Treasury";
            container.m_width = TreasuryFallbackWidth;
            container.m_height = TreasuryFallbackHeight;
            container.m_privacy =
                Container.PrivacySetting.Private;
            container.m_checkGuardStone = false;
            container.m_autoDestroyEmpty = false;

            WearNTear wearNTear =
                container.GetComponent<WearNTear>();

            if (wearNTear != null)
            {
                wearNTear.m_noRoofWear = true;
                wearNTear.m_noSupportWear = true;
                wearNTear.m_ashDamageImmune = true;
                wearNTear.m_burnable = false;
            }

            ZNetView zNetView =
                container.GetComponent<ZNetView>();

            if (zNetView != null &&
                zNetView.IsValid() &&
                zNetView.GetZDO() != null)
            {
                zNetView.GetZDO().Set(
                    TerritoryZdoKeys.TreasuryChestMarker,
                    true);
            }

            Inventory inventory =
                container.GetInventory();

            if (inventory == null)
                return;

            if (InventoryWidthField != null)
            {
                InventoryWidthField.SetValue(
                    inventory,
                    TreasuryFallbackWidth);
            }

            if (InventoryHeightField != null)
            {
                InventoryHeightField.SetValue(
                    inventory,
                    TreasuryFallbackHeight);
            }

            SetInventoryName(
                inventory,
                "Territory Treasury");

            TreasuryContainerByInventory[inventory] =
                container;

            List<ItemDrop.ItemData> items =
                inventory.GetAllItems();

            bool changed = false;

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];

                if (item == null ||
                    item.m_shared == null ||
                    item.m_shared.m_maxStackSize >=
                        TreasurySlotCapacity)
                {
                    continue;
                }

                ApplyVirtualStackLimit(
                    item,
                    TreasurySlotCapacity);

                changed = true;
            }

            if (changed)
                InvokeInventoryChanged(inventory);
        }

        internal static void UnregisterTreasuryChest(
            Container container)
        {
            if (container == null)
                return;

            Inventory inventory =
                container.GetInventory();

            if (inventory != null)
            {
                TreasuryContainerByInventory.Remove(
                    inventory);
            }
        }

        internal static bool TryMigrateVirtualTreasuryInventory(
            ZDO wardZdo,
            Container container)
        {
            if (wardZdo == null ||
                container == null)
            {
                return false;
            }

            string serializedItems =
                wardZdo.GetString(
                    TerritoryZdoKeys.TreasuryChestItems,
                    "");

            if (string.IsNullOrEmpty(
                    serializedItems))
            {
                return false;
            }

            Inventory inventory =
                container.GetInventory();

            if (inventory == null)
                return false;

            if (inventory.NrOfItems() > 0)
            {
                ModLog.Info(
                    "[TerritoryTreasury] Legacy virtual Treasury migration postponed because physical chest is not empty.");

                return false;
            }

            try
            {
                LoadVirtualInventoryPackage(
                    inventory,
                    TerritoryZdoKeys.TreasuryChestItems,
                    new ZPackage(serializedItems));

                wardZdo.Set(
                    TerritoryZdoKeys.TreasuryChestItems,
                    "");

                InvokeInventoryChanged(inventory);

                ModLog.Info(
                    "[TerritoryTreasury] Legacy virtual Treasury migrated into physical chest.");

                return true;
            }
            catch (Exception exception)
            {
                ModLog.Debug(
                    "[TerritoryTreasury] Legacy migration failed: " +
                    exception.Message);

                return false;
            }
        }
'@ "physical treasury configuration and migration"

    Write-Utf8NoBom `
        $modulePath `
        $module `
        $moduleNewLine

    $keysOriginal =
        [System.IO.File]::ReadAllText($keysPath)

    $keysNewLine =
        if ($keysOriginal.Contains("`r`n")) {
            "`r`n"
        }
        else {
            "`n"
        }

    $keys = Normalize-NewLines $keysOriginal

    $keys = Replace-Once $keys @'
        public const string TreasuryChestMarker = "ct_territory_treasury_chest_marker";
        public const string TreasuryChestItems = "ct_territory_treasury_chest_items";
'@ @'
        public const string TreasuryChestMarker = "ct_territory_treasury_chest_marker";
        public const string TreasuryWardId = "ct_territory_treasury_ward_id";
        public const string TreasuryChestItems = "ct_territory_treasury_chest_items";
'@ "treasury ward link key"

    Write-Utf8NoBom `
        $keysPath `
        $keys `
        $keysNewLine

    $projectOriginal =
        [System.IO.File]::ReadAllText($projectPath)

    $projectNewLine =
        if ($projectOriginal.Contains("`r`n")) {
            "`r`n"
        }
        else {
            "`n"
        }

    $project = Normalize-NewLines $projectOriginal

    $project = Replace-Once $project @'
    <Compile Include="Features\Territory\Services\TerritoryRuleService.cs" />
    <Compile Include="Features\Territory\Services\TerritoryWardRadiusService.cs" />
'@ @'
    <Compile Include="Features\Territory\Services\PhysicalTreasuryService.cs" />
    <Compile Include="Features\Territory\Services\TerritoryRuleService.cs" />
    <Compile Include="Features\Territory\Services\TerritoryWardRadiusService.cs" />
'@ "physical treasury compile include"

    Write-Utf8NoBom `
        $projectPath `
        $project `
        $projectNewLine

    $serviceSource = @'
using System;
using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Territory;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class PhysicalTreasuryService :
        IEventHandler<WardDestroyedEvent>
    {
        private const string TreasuryPrefabName =
            "piece_chest_blackmetal";

        private const float EnsureInterval = 1.5f;
        private const float ChestDistanceBehindWard = 1.75f;

        private static readonly FieldInfo AllPrivateAreasField =
            AccessTools.Field(
                typeof(PrivateArea),
                "m_allAreas");

        private readonly Dictionary<string, ZDOID> _chestIdsByWardId =
            new Dictionary<string, ZDOID>(
                StringComparer.Ordinal);

        private float _nextEnsureTime;

        public void Update()
        {
            if (Time.time < _nextEnsureTime)
                return;

            _nextEnsureTime =
                Time.time + EnsureInterval;

            if (ZNetScene.instance == null ||
                ZDOMan.instance == null)
            {
                return;
            }

            List<PrivateArea> privateAreas =
                GetPrivateAreas();

            for (int i = 0; i < privateAreas.Count; i++)
            {
                PrivateArea privateArea =
                    privateAreas[i];

                if (privateArea == null)
                    continue;

                EnsureTreasuryChest(privateArea);
            }
        }

        public bool RequestOpen(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null ||
                player == null)
            {
                return false;
            }

            ZDO wardZdo =
                GetZdo(privateArea);

            if (wardZdo == null)
                return false;

            long creatorId =
                wardZdo.GetLong(
                    ZDOVars.s_creator,
                    0L);

            if (creatorId != player.GetPlayerID())
            {
                player.Message(
                    MessageHud.MessageType.Center,
                    "$msg_noaccess");

                ModLog.Debug(
                    "[TerritoryTreasury] Open denied. Player is not ward creator: " +
                    wardId);

                return false;
            }

            Container container =
                EnsureTreasuryChest(privateArea);

            if (container == null)
            {
                container =
                    FindLinkedTreasuryChest(
                        privateArea,
                        true);
            }

            if (container == null)
            {
                player.Message(
                    MessageHud.MessageType.Center,
                    "Territory Treasury is not ready.");

                return false;
            }

            return container.Interact(
                player,
                false,
                false);
        }

        public void Handle(
            WardDestroyedEvent eventData)
        {
            if (eventData == null)
                return;

            string wardId =
                eventData.WardId.ToString();

            DestroyTreasuryForWard(wardId);
        }

        public static Inventory GetTreasuryInventory(
            PrivateArea privateArea)
        {
            Container container =
                FindLinkedTreasuryChest(
                    privateArea,
                    false);

            if (container == null ||
                container.IsInUse())
            {
                return null;
            }

            TerritoryTerraformingService
                .ConfigureTreasuryChest(container);

            return container.GetInventory();
        }

        public static bool EnsureGroundItemOwnership(
            ItemDrop drop)
        {
            if (drop == null)
                return false;

            ZNetView zNetView =
                drop.GetComponent<ZNetView>();

            if (zNetView == null ||
                !zNetView.IsValid())
            {
                return drop.CanPickup(false);
            }

            if (!zNetView.IsOwner())
                zNetView.ClaimOwnership();

            return zNetView.IsOwner() &&
                   drop.CanPickup(false);
        }

        public static bool IsTreasuryObject(
            GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            ZNetView zNetView =
                gameObject.GetComponent<ZNetView>();

            if (zNetView == null)
            {
                zNetView =
                    gameObject.GetComponentInParent<ZNetView>();
            }

            if (zNetView == null ||
                !zNetView.IsValid())
            {
                return false;
            }

            ZDO zdo =
                zNetView.GetZDO();

            return zdo != null &&
                   zdo.GetBool(
                       TerritoryZdoKeys.TreasuryChestMarker,
                       false);
        }

        private Container EnsureTreasuryChest(
            PrivateArea privateArea)
        {
            if (privateArea == null ||
                ZNetScene.instance == null ||
                ZDOMan.instance == null)
            {
                return null;
            }

            ZNetView wardView =
                privateArea.GetComponent<ZNetView>();

            if (wardView == null ||
                !wardView.IsValid() ||
                !wardView.IsOwner())
            {
                return FindLinkedTreasuryChest(
                    privateArea,
                    false);
            }

            ZDO wardZdo =
                wardView.GetZDO();

            if (wardZdo == null)
                return null;

            string wardId =
                wardZdo.m_uid.ToString();

            Container existing =
                FindLinkedTreasuryChest(
                    privateArea,
                    true);

            if (existing != null)
            {
                RegisterRuntimeLink(
                    wardId,
                    existing);

                TerritoryTerraformingService
                    .ConfigureTreasuryChest(existing);

                TerritoryTerraformingService
                    .TryMigrateVirtualTreasuryInventory(
                        wardZdo,
                        existing);

                return existing;
            }

            if (HasLiveLinkedTreasuryZdo(
                    wardZdo))
            {
                return null;
            }

            GameObject prefab =
                ZNetScene.instance.GetPrefab(
                    TreasuryPrefabName);

            if (prefab == null)
            {
                ModLog.Debug(
                    "[TerritoryTreasury] Prefab not found: " +
                    TreasuryPrefabName);

                return null;
            }

            Vector3 position =
                CalculateTreasuryPosition(
                    privateArea);

            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    privateArea.transform.eulerAngles.y,
                    0f);

            GameObject chestObject =
                UnityEngine.Object.Instantiate(
                    prefab,
                    position,
                    rotation);

            if (chestObject == null)
                return null;

            Container container =
                chestObject.GetComponent<Container>();

            ZNetView chestView =
                chestObject.GetComponent<ZNetView>();

            if (container == null ||
                chestView == null ||
                !chestView.IsValid())
            {
                DestroyCreatedObject(chestObject);
                return null;
            }

            ZDO chestZdo =
                chestView.GetZDO();

            if (chestZdo == null)
            {
                DestroyCreatedObject(chestObject);
                return null;
            }

            long creatorId =
                wardZdo.GetLong(
                    ZDOVars.s_creator,
                    0L);

            Piece piece =
                chestObject.GetComponent<Piece>();

            if (piece != null &&
                creatorId != 0L)
            {
                piece.SetCreator(creatorId);
            }

            chestZdo.Set(
                TerritoryZdoKeys.TreasuryChestMarker,
                true);

            chestZdo.Set(
                TerritoryZdoKeys.TreasuryWardId,
                wardId);

            wardZdo.Set(
                TerritoryZdoKeys.TreasuryChestZdoUser,
                chestZdo.m_uid.UserID);

            wardZdo.Set(
                TerritoryZdoKeys.TreasuryChestZdoId,
                (int)chestZdo.m_uid.ID);

            TerritoryTerraformingService
                .ConfigureTreasuryChest(container);

            TerritoryTerraformingService
                .TryMigrateVirtualTreasuryInventory(
                    wardZdo,
                    container);

            RegisterRuntimeLink(
                wardId,
                container);

            ModLog.Info(
                "[TerritoryTreasury] Physical blackmetal treasury created behind ward: " +
                wardId);

            return container;
        }

        private void RegisterRuntimeLink(
            string wardId,
            Container container)
        {
            if (string.IsNullOrEmpty(wardId) ||
                container == null)
            {
                return;
            }

            ZNetView zNetView =
                container.GetComponent<ZNetView>();

            if (zNetView == null ||
                !zNetView.IsValid() ||
                zNetView.GetZDO() == null)
            {
                return;
            }

            _chestIdsByWardId[wardId] =
                zNetView.GetZDO().m_uid;
        }

        private void DestroyTreasuryForWard(
            string wardId)
        {
            if (string.IsNullOrEmpty(wardId))
                return;

            Container container = null;
            ZDOID chestId;

            if (_chestIdsByWardId.TryGetValue(
                    wardId,
                    out chestId) &&
                ZNetScene.instance != null)
            {
                GameObject linkedObject =
                    ZNetScene.instance.FindInstance(
                        chestId);

                if (linkedObject != null)
                {
                    container =
                        linkedObject.GetComponent<Container>();
                }
            }

            if (container == null)
            {
                Container[] containers =
                    UnityEngine.Object
                        .FindObjectsByType<Container>(
                            UnityEngine.FindObjectsSortMode.None);

                for (int i = 0; i < containers.Length; i++)
                {
                    Container candidate =
                        containers[i];

                    if (candidate == null ||
                        !IsTreasuryObject(
                            candidate.gameObject))
                    {
                        continue;
                    }

                    ZNetView candidateView =
                        candidate.GetComponent<ZNetView>();

                    if (candidateView == null ||
                        !candidateView.IsValid())
                    {
                        continue;
                    }

                    ZDO candidateZdo =
                        candidateView.GetZDO();

                    if (candidateZdo == null)
                        continue;

                    string candidateWardId =
                        candidateZdo.GetString(
                            TerritoryZdoKeys.TreasuryWardId,
                            "");

                    if (!string.Equals(
                            candidateWardId,
                            wardId,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    container = candidate;
                    break;
                }
            }

            _chestIdsByWardId.Remove(wardId);

            if (container == null)
                return;

            DropTreasuryContents(container);

            TerritoryTerraformingService
                .UnregisterTreasuryChest(container);

            ZNetView zNetView =
                container.GetComponent<ZNetView>();

            if (zNetView != null &&
                zNetView.IsValid())
            {
                if (!zNetView.IsOwner())
                    zNetView.ClaimOwnership();

                if (zNetView.IsOwner())
                {
                    zNetView.Destroy();

                    ModLog.Info(
                        "[TerritoryTreasury] Linked treasury destroyed with ward: " +
                        wardId);

                    return;
                }
            }

            ModLog.Debug(
                "[TerritoryTreasury] Linked treasury could not be destroyed because local peer is not owner: " +
                wardId);
        }

        private static void DropTreasuryContents(
            Container container)
        {
            if (container == null)
                return;

            Inventory inventory =
                container.GetInventory();

            if (inventory == null ||
                inventory.NrOfItems() <= 0)
            {
                return;
            }

            List<ItemDrop.ItemData> items =
                new List<ItemDrop.ItemData>(
                    inventory.GetAllItems());

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item =
                    items[i];

                if (item == null)
                    continue;

                Vector3 position =
                    container.transform.position +
                    Vector3.up * 0.5f +
                    UnityEngine.Random.insideUnitSphere * 0.3f;

                Quaternion rotation =
                    Quaternion.Euler(
                        0f,
                        UnityEngine.Random.Range(
                            0,
                            360),
                        0f);

                ItemDrop.DropItem(
                    item,
                    0,
                    position,
                    rotation);
            }

            inventory.RemoveAll();
        }

        private static Container FindLinkedTreasuryChest(
            PrivateArea privateArea,
            bool clearInvalidLink)
        {
            ZDO wardZdo =
                GetZdo(privateArea);

            if (wardZdo == null ||
                ZNetScene.instance == null)
            {
                return null;
            }

            long userId =
                wardZdo.GetLong(
                    TerritoryZdoKeys.TreasuryChestZdoUser,
                    0L);

            int id =
                wardZdo.GetInt(
                    TerritoryZdoKeys.TreasuryChestZdoId,
                    0);

            if (userId == 0L ||
                id == 0)
            {
                return null;
            }

            ZDOID chestId =
                new ZDOID(
                    userId,
                    (uint)id);

            GameObject chestObject =
                ZNetScene.instance.FindInstance(
                    chestId);

            if (chestObject == null)
            {
                ZDO linkedZdo =
                    ZDOMan.instance != null
                        ? ZDOMan.instance.GetZDO(chestId)
                        : null;

                if (linkedZdo == null &&
                    clearInvalidLink)
                {
                    ClearWardChestLink(wardZdo);
                }

                return null;
            }

            Container container =
                chestObject.GetComponent<Container>();

            if (container == null)
            {
                if (clearInvalidLink)
                    ClearWardChestLink(wardZdo);

                return null;
            }

            ZNetView chestView =
                chestObject.GetComponent<ZNetView>();

            if (chestView != null &&
                chestView.IsValid() &&
                chestView.GetZDO() != null)
            {
                chestView.GetZDO().Set(
                    TerritoryZdoKeys.TreasuryChestMarker,
                    true);

                chestView.GetZDO().Set(
                    TerritoryZdoKeys.TreasuryWardId,
                    wardZdo.m_uid.ToString());
            }

            TerritoryTerraformingService
                .ConfigureTreasuryChest(container);

            return container;
        }

        private static bool HasLiveLinkedTreasuryZdo(
            ZDO wardZdo)
        {
            if (wardZdo == null ||
                ZDOMan.instance == null)
            {
                return false;
            }

            long userId =
                wardZdo.GetLong(
                    TerritoryZdoKeys.TreasuryChestZdoUser,
                    0L);

            int id =
                wardZdo.GetInt(
                    TerritoryZdoKeys.TreasuryChestZdoId,
                    0);

            if (userId == 0L ||
                id == 0)
            {
                return false;
            }

            ZDO linkedZdo =
                ZDOMan.instance.GetZDO(
                    new ZDOID(
                        userId,
                        (uint)id));

            if (linkedZdo != null)
                return true;

            ClearWardChestLink(wardZdo);
            return false;
        }

        private static void ClearWardChestLink(
            ZDO wardZdo)
        {
            if (wardZdo == null)
                return;

            wardZdo.Set(
                TerritoryZdoKeys.TreasuryChestZdoUser,
                0L);

            wardZdo.Set(
                TerritoryZdoKeys.TreasuryChestZdoId,
                0);
        }

        private static Vector3 CalculateTreasuryPosition(
            PrivateArea privateArea)
        {
            if (privateArea == null)
                return Vector3.zero;

            Vector3 forward =
                privateArea.transform.forward;

            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f)
                forward = Vector3.forward;

            Vector3 position =
                privateArea.transform.position -
                forward.normalized *
                ChestDistanceBehindWard;

            float groundHeight;

            if (ZoneSystem.instance != null &&
                ZoneSystem.instance.GetGroundHeight(
                    position,
                    out groundHeight))
            {
                position.y = groundHeight;
            }
            else
            {
                position.y =
                    privateArea.transform.position.y;
            }

            return position;
        }

        private static ZDO GetZdo(
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

        private static void DestroyCreatedObject(
            GameObject gameObject)
        {
            if (gameObject == null)
                return;

            ZNetView zNetView =
                gameObject.GetComponent<ZNetView>();

            if (zNetView != null &&
                zNetView.IsValid())
            {
                if (!zNetView.IsOwner())
                    zNetView.ClaimOwnership();

                if (zNetView.IsOwner())
                {
                    zNetView.Destroy();
                    return;
                }
            }

            UnityEngine.Object.Destroy(gameObject);
        }
    }
}

namespace ClanTerritory.Integration.Valheim.Harmony
{
    using ClanTerritory.Features.Territory.Services;

    [HarmonyPatch(typeof(WearNTear))]
    internal static class PhysicalTreasuryWearProtectionHooks
    {
        [HarmonyPrefix]
        [HarmonyPatch("RPC_Damage")]
        private static bool RPCDamagePrefix(
            WearNTear __instance)
        {
            return __instance == null ||
                   !PhysicalTreasuryService.IsTreasuryObject(
                       __instance.gameObject);
        }

        [HarmonyPrefix]
        [HarmonyPatch("ApplyDamage")]
        private static bool ApplyDamagePrefix(
            WearNTear __instance,
            ref bool __result)
        {
            if (__instance == null ||
                !PhysicalTreasuryService.IsTreasuryObject(
                    __instance.gameObject))
            {
                return true;
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RPC_Remove")]
        private static bool RPCRemovePrefix(
            WearNTear __instance)
        {
            return __instance == null ||
                   !PhysicalTreasuryService.IsTreasuryObject(
                       __instance.gameObject);
        }

        [HarmonyPrefix]
        [HarmonyPatch("Destroy")]
        private static bool DestroyPrefix(
            WearNTear __instance)
        {
            return __instance == null ||
                   !PhysicalTreasuryService.IsTreasuryObject(
                       __instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(Player), "RemovePiece")]
    internal static class PhysicalTreasuryPieceRemovalHook
    {
        private static readonly FieldInfo HoveringPieceField =
            AccessTools.Field(
                typeof(Player),
                "m_hoveringPiece");

        private static bool Prefix(
            Player __instance)
        {
            if (__instance == null ||
                HoveringPieceField == null)
            {
                return true;
            }

            Piece piece =
                HoveringPieceField.GetValue(
                    __instance) as Piece;

            if (piece == null ||
                !PhysicalTreasuryService.IsTreasuryObject(
                    piece.gameObject))
            {
                return true;
            }

            __instance.Message(
                MessageHud.MessageType.Center,
                "Treasury is linked to the ward.");

            return false;
        }
    }
}
'@
    $researchSource = @'
# Investigation 090: Physical treasury chest linked to ward

## Decision

The virtual Treasury container is removed from the active runtime path.

Each ward owns one real `piece_chest_blackmetal` placed on the ward local
center line, exactly behind the ward:

```text
position = ward.position - ward.forward * 1.75
```

The chest uses the ward yaw and ground height at the target position.

## Ownership link

The ward stores the treasury ZDO ID:

```text
ct_territory_treasury_chest_zdo_user
ct_territory_treasury_chest_zdo_id
```

The chest stores the ward ID:

```text
ct_territory_treasury_ward_id
```

Both objects therefore have a persistent two-way link.

## Lifetime

The physical chest is protected from:

- normal `WearNTear` damage;
- environmental `ApplyDamage`;
- `RPC_Remove`;
- hammer removal;
- normal `WearNTear.Destroy`.

When `WardDestroyedEvent` is published, the linked chest contents are
dropped and the chest is destroyed with the normal `ZNetView.Destroy()`
path.

## Inventory settings

The real blackmetal chest replaces the old virtual Treasury settings:

- name: `Territory Treasury`;
- size: 8 x 4;
- privacy: ward creator only;
- no guard-stone secondary check;
- no auto-destroy when empty;
- no rain/support/ash/burn wear;
- custom Treasury stack capacity: 9999 per slot.

The existing large-stack inventory hooks continue to recognize the real
chest through `TreasuryContainerByInventory`.

## Migration

If the ward still has legacy serialized virtual Treasury data in:

```text
ct_territory_treasury_chest_items
```

the data is loaded once into an empty physical chest. After a successful
migration, the old ward value is cleared.

The old key remains in code only for this migration and is not used as
active Treasury storage.

## Resource absorption

Ground resource absorption obtains the inventory of the linked physical
Treasury. It no longer creates a temporary hidden blackmetal chest and no
longer persists Treasury items into the ward ZDO.

Before the worker mutates or destroys a ground `ItemDrop`, it claims the
network object with `ZNetView.ClaimOwnership()` and verifies ownership.

## Expected effect on ZNetScene errors

The previous virtual Treasury instantiated a network prefab, hid it, and
destroyed it whenever the inventory UI closed. Removing that lifecycle
eliminates the most likely source of a `ZNetView` remaining in
`ZNetScene.m_instances` after its ZDO was reset.

The fix is confirmed only when a fresh runtime log no longer contains:

```text
ZNetScene.DMD<ZNetScene::RemoveObjects>
NullReferenceException
```

## Validation

1. Rebuild `ClanTerritory.sln`.
2. Confirm zero build errors.
3. Enter a world containing an existing ward.
4. Confirm one blackmetal chest appears 1.75 m behind the ward.
5. Confirm the chest center is aligned with the ward local X axis.
6. Open Treasury from the ward menu and directly from the chest.
7. Confirm the old virtual Treasury items migrated.
8. Confirm Treasury stacks may exceed vanilla stack limits up to 9999.
9. Confirm weapons, enemies, weather and the hammer cannot destroy it.
10. Remove the ward and confirm the Treasury contents drop and the chest
    disappears.
11. Drop a matching resource in the territory and confirm it is absorbed
    into the physical Treasury.
12. Inspect a fresh single-run BepInEx log for `RemoveObjects` errors.
'@

    Write-Utf8NoBom `
        $servicePath `
        $serviceSource `
        $moduleNewLine

    Write-Utf8NoBom `
        $researchPath `
        $researchSource `
        $moduleNewLine

    & git diff --check

    if ($LASTEXITCODE -ne 0) {
        throw "git diff --check failed."
    }

    $moduleValidation =
        Normalize-NewLines (
            [System.IO.File]::ReadAllText(
                $modulePath)
        )

    $serviceValidation =
        Normalize-NewLines (
            [System.IO.File]::ReadAllText(
                $servicePath)
        )

    $projectValidation =
        Normalize-NewLines (
            [System.IO.File]::ReadAllText(
                $projectPath)
        )

    if ($moduleValidation.Contains(
        "Container container = CreateVirtualTreasuryChest("))
    {
        throw "Virtual Treasury open path remains active."
    }

    if ($moduleValidation.Contains(
        'Inventory treasuryInventory = CreateVirtualStorageInventory('))
    {
        throw "Virtual Treasury absorption path remains active."
    }

    if ($moduleValidation.Contains(
        "drop.RequestOwn();"))
    {
        throw "Obsolete ItemDrop.RequestOwn call remains."
    }

    if (-not $moduleValidation.Contains(
        "PhysicalTreasuryService.GetTreasuryInventory("))
    {
        throw "Physical Treasury absorption link is missing."
    }

    if (-not $serviceValidation.Contains(
        'TreasuryPrefabName ='))
    {
        throw "Physical Treasury service was not created."
    }

    if (-not $serviceValidation.Contains(
        'ChestDistanceBehindWard = 1.75f;'))
    {
        throw "Treasury position rule is missing."
    }

    if (-not $serviceValidation.Contains(
        'zNetView.Destroy();'))
    {
        throw "Safe network destroy path is missing."
    }

    $includeCount = (
        [System.Text.RegularExpressions.Regex]::Matches(
            $projectValidation,
            [System.Text.RegularExpressions.Regex]::Escape(
                'Features\Territory\Services\PhysicalTreasuryService.cs'
            )
        )
    ).Count

    if ($includeCount -ne 1) {
        throw "Expected exactly one physical Treasury compile include."
    }

    Write-Host ""
    Write-Host "Virtual Treasury active path removed."
    Write-Host "Physical piece_chest_blackmetal Treasury added."
    Write-Host "Legacy Treasury data will migrate once."
    Write-Host "Do not commit before build and runtime validation."
    Write-Host ""
    & git status --short
}
finally {
    Pop-Location
}
