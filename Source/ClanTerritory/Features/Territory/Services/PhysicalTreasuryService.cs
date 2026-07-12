using System;
using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Runtime;
using ClanTerritory.Features.Territory;
using ClanTerritory.Features.WardDetection.Services;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class PhysicalTreasuryService
    {
        private const string TreasuryPrefabName =
            "piece_chest_blackmetal";

        private const float EnsureInterval = 1.5f;
        private const float ChestDistanceBehindWard = 1.75f;
        private const int TreasuryWidth = 8;
        private const int TreasuryHeight = 4;
        private const int TreasuryStackLimit = 9999;

        private static readonly FieldInfo AllPrivateAreasField =
            AccessTools.Field(
                typeof(PrivateArea),
                "m_allAreas");

        private static readonly FieldInfo ContainerWidthField =
            AccessTools.Field(
                typeof(Container),
                "m_width");

        private static readonly FieldInfo ContainerHeightField =
            AccessTools.Field(
                typeof(Container),
                "m_height");

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

        private static readonly FieldInfo TreasuryContainerMapField =
            AccessTools.Field(
                typeof(TerritoryTerraformingService),
                "TreasuryContainerByInventory");

        private static readonly MethodInfo LoadVirtualInventoryPackageMethod =
            AccessTools.Method(
                typeof(TerritoryTerraformingService),
                "LoadVirtualInventoryPackage");

        private readonly Dictionary<string, ZDOID> _chestIdsByWardId =
            new Dictionary<string, ZDOID>(
                StringComparer.Ordinal);

        private ZNetScene _observedScene;
        private float _nextEnsureTime;

        public void Update()
        {
            ZNetScene currentScene =
                ZNetScene.instance;

            if (!ReferenceEquals(
                    currentScene,
                    _observedScene))
            {
                _observedScene = currentScene;
                _chestIdsByWardId.Clear();
                _nextEnsureTime = 0f;
            }

            if (currentScene == null ||
                ZDOMan.instance == null)
            {
                return;
            }

            if (Time.time < _nextEnsureTime)
                return;

            _nextEnsureTime =
                Time.time + EnsureInterval;

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

                ModLog.Debug(
                    "[TerritoryTreasury] Open failed. Physical chest is not ready: " +
                    wardId);

                return false;
            }

            ConfigureTreasuryChest(
                container,
                wardZdo);

            if (InventoryGui.instance != null)
                InventoryGui.instance.Hide();

            return container.Interact(
                player,
                false,
                false);
        }

        public Inventory GetTreasuryInventory(
            ZDO wardZdo)
        {
            if (wardZdo == null)
                return null;

            PrivateArea privateArea =
                FindPrivateArea(wardZdo);

            if (privateArea == null)
                return null;

            Container container =
                EnsureTreasuryChest(privateArea);

            if (container == null ||
                container.IsInUse())
            {
                return null;
            }

            ConfigureTreasuryChest(
                container,
                wardZdo);

            return container.GetInventory();
        }

        public void DestroyTreasuryForWard(
            WardId wardId)
        {
            DestroyTreasuryForWard(
                wardId.ToString());
        }

        public static bool IsGameplayReady()
        {
            RuntimeStateMachine stateMachine;

            if (!ServiceContainer.TryGet<
                    RuntimeStateMachine>(
                    out stateMachine) ||
                stateMachine == null)
            {
                return false;
            }

            return stateMachine.State ==
                   RuntimeState.GameplayReady;
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
                !wardView.IsValid())
            {
                return null;
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

            if (existing == null)
            {
                existing =
                    FindTreasuryByWardId(
                        wardId);

                if (existing != null)
                {
                    WriteWardChestLink(
                        wardZdo,
                        existing);
                }
            }

            if (existing != null)
            {
                RegisterRuntimeLink(
                    wardId,
                    existing);

                ConfigureTreasuryChest(
                    existing,
                    wardZdo);

                TryMigrateVirtualTreasuryInventory(
                    wardZdo,
                    existing);

                return existing;
            }

            if (!wardView.IsOwner())
                return null;

            if (HasLiveLinkedTreasuryZdo(
                    wardZdo,
                    wardId))
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

            ConfigureTreasuryChest(
                container,
                wardZdo);

            TryMigrateVirtualTreasuryInventory(
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
                container =
                    FindTreasuryByWardId(
                        wardId);
            }

            _chestIdsByWardId.Remove(wardId);

            if (container == null)
                return;

            DropTreasuryContents(container);
            UnregisterTreasuryInventory(container);

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

                string expectedWardId =
                    wardZdo.m_uid.ToString();

                string linkedWardId =
                    linkedZdo != null
                        ? linkedZdo.GetString(
                            TerritoryZdoKeys.TreasuryWardId,
                            "")
                        : "";

                if (clearInvalidLink &&
                    (linkedZdo == null ||
                     !string.Equals(
                         linkedWardId,
                         expectedWardId,
                         StringComparison.Ordinal)))
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

            ConfigureTreasuryChest(
                container,
                wardZdo);

            return container;
        }

        private static Container FindTreasuryByWardId(
            string wardId)
        {
            if (string.IsNullOrEmpty(wardId))
                return null;

            Container[] containers =
                UnityEngine.Object
                    .FindObjectsByType<Container>(
                        UnityEngine.FindObjectsSortMode.None);

            for (int i = 0; i < containers.Length; i++)
            {
                Container candidate =
                    containers[i];

                if (candidate == null)
                    continue;

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

                candidateZdo.Set(
                    TerritoryZdoKeys.TreasuryChestMarker,
                    true);

                return candidate;
            }

            return null;
        }

        private static void WriteWardChestLink(
            ZDO wardZdo,
            Container container)
        {
            if (wardZdo == null ||
                container == null)
            {
                return;
            }

            ZNetView chestView =
                container.GetComponent<ZNetView>();

            if (chestView == null ||
                !chestView.IsValid() ||
                chestView.GetZDO() == null)
            {
                return;
            }

            ZDO chestZdo =
                chestView.GetZDO();

            chestZdo.Set(
                TerritoryZdoKeys.TreasuryChestMarker,
                true);

            chestZdo.Set(
                TerritoryZdoKeys.TreasuryWardId,
                wardZdo.m_uid.ToString());

            wardZdo.Set(
                TerritoryZdoKeys.TreasuryChestZdoUser,
                chestZdo.m_uid.UserID);

            wardZdo.Set(
                TerritoryZdoKeys.TreasuryChestZdoId,
                (int)chestZdo.m_uid.ID);
        }

        private static bool HasLiveLinkedTreasuryZdo(
            ZDO wardZdo,
            string wardId)
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

            if (linkedZdo == null)
            {
                ClearWardChestLink(wardZdo);
                return false;
            }

            string linkedWardId =
                linkedZdo.GetString(
                    TerritoryZdoKeys.TreasuryWardId,
                    "");

            if (!string.Equals(
                    linkedWardId,
                    wardId,
                    StringComparison.Ordinal))
            {
                ClearWardChestLink(wardZdo);
                return false;
            }

            return true;
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

        private static void ConfigureTreasuryChest(
            Container container,
            ZDO wardZdo)
        {
            if (container == null)
                return;

            SetFieldValue(
                ContainerWidthField,
                container,
                TreasuryWidth);

            SetFieldValue(
                ContainerHeightField,
                container,
                TreasuryHeight);

            container.m_name =
                "Territory Treasury";

            container.m_privacy =
                Container.PrivacySetting.Private;

            container.m_checkGuardStone = false;
            container.m_autoDestroyEmpty = false;

            WearNTear wearNTear =
                container.GetComponent<WearNTear>();

            SetBooleanField(
                wearNTear,
                "m_noRoofWear",
                true);

            SetBooleanField(
                wearNTear,
                "m_noSupportWear",
                true);

            SetBooleanField(
                wearNTear,
                "m_ashDamageImmune",
                true);

            SetBooleanField(
                wearNTear,
                "m_burnable",
                false);

            ZNetView zNetView =
                container.GetComponent<ZNetView>();

            if (zNetView != null &&
                zNetView.IsValid() &&
                zNetView.GetZDO() != null)
            {
                ZDO chestZdo =
                    zNetView.GetZDO();

                chestZdo.Set(
                    TerritoryZdoKeys.TreasuryChestMarker,
                    true);

                if (wardZdo != null)
                {
                    chestZdo.Set(
                        TerritoryZdoKeys.TreasuryWardId,
                        wardZdo.m_uid.ToString());
                }
            }

            if (wardZdo != null)
            {
                long creatorId =
                    wardZdo.GetLong(
                        ZDOVars.s_creator,
                        0L);

                Piece piece =
                    container.GetComponent<Piece>();

                if (piece != null &&
                    creatorId != 0L)
                {
                    piece.SetCreator(creatorId);
                }
            }

            Inventory inventory =
                container.GetInventory();

            if (inventory == null)
                return;

            SetFieldValue(
                InventoryWidthField,
                inventory,
                TreasuryWidth);

            SetFieldValue(
                InventoryHeightField,
                inventory,
                TreasuryHeight);

            SetFieldValue(
                InventoryNameField,
                inventory,
                "Territory Treasury");

            RegisterTreasuryInventory(
                inventory,
                container);

            List<ItemDrop.ItemData> items =
                inventory.GetAllItems();

            bool changed = false;

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item =
                    items[i];

                if (item == null ||
                    item.m_shared == null ||
                    item.m_shared.m_maxStackSize >=
                        TreasuryStackLimit)
                {
                    continue;
                }

                if (ApplyTreasuryStackLimit(item))
                    changed = true;
            }

            if (changed)
                InvokeInventoryChanged(inventory);
        }

        private static bool TryMigrateVirtualTreasuryInventory(
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

            if (LoadVirtualInventoryPackageMethod == null)
            {
                ModLog.Debug(
                    "[TerritoryTreasury] Legacy migration method was not found.");

                return false;
            }

            try
            {
                LoadVirtualInventoryPackageMethod.Invoke(
                    null,
                    new object[]
                    {
                        inventory,
                        TerritoryZdoKeys.TreasuryChestItems,
                        new ZPackage(serializedItems)
                    });

                wardZdo.Set(
                    TerritoryZdoKeys.TreasuryChestItems,
                    "");

                ConfigureTreasuryChest(
                    container,
                    wardZdo);

                InvokeInventoryChanged(inventory);

                ModLog.Info(
                    "[TerritoryTreasury] Legacy virtual Treasury migrated into physical chest.");

                return true;
            }
            catch (Exception exception)
            {
                Exception actualException =
                    exception is TargetInvocationException &&
                    exception.InnerException != null
                        ? exception.InnerException
                        : exception;

                ModLog.Debug(
                    "[TerritoryTreasury] Legacy migration failed: " +
                    actualException.Message);

                return false;
            }
        }

        private static bool ApplyTreasuryStackLimit(
            ItemDrop.ItemData item)
        {
            if (item == null ||
                item.m_shared == null ||
                MemberwiseCloneMethod == null)
            {
                return false;
            }

            try
            {
                ItemDrop.ItemData.SharedData clonedShared =
                    MemberwiseCloneMethod.Invoke(
                        item.m_shared,
                        null)
                    as ItemDrop.ItemData.SharedData;

                if (clonedShared == null)
                    return false;

                clonedShared.m_maxStackSize =
                    TreasuryStackLimit;

                item.m_shared =
                    clonedShared;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void RegisterTreasuryInventory(
            Inventory inventory,
            Container container)
        {
            if (inventory == null ||
                container == null ||
                TreasuryContainerMapField == null)
            {
                return;
            }

            try
            {
                Dictionary<Inventory, Container> map =
                    TreasuryContainerMapField.GetValue(null)
                    as Dictionary<Inventory, Container>;

                if (map != null)
                    map[inventory] = container;
            }
            catch (Exception exception)
            {
                ModLog.Debug(
                    "[TerritoryTreasury] Treasury inventory registration failed: " +
                    exception.Message);
            }
        }

        private static void UnregisterTreasuryInventory(
            Container container)
        {
            if (container == null ||
                TreasuryContainerMapField == null)
            {
                return;
            }

            Inventory inventory =
                container.GetInventory();

            if (inventory == null)
                return;

            try
            {
                Dictionary<Inventory, Container> map =
                    TreasuryContainerMapField.GetValue(null)
                    as Dictionary<Inventory, Container>;

                if (map != null)
                    map.Remove(inventory);
            }
            catch (Exception)
            {
            }
        }

        private static void InvokeInventoryChanged(
            Inventory inventory)
        {
            if (inventory == null ||
                InventoryChangedMethod == null)
            {
                return;
            }

            try
            {
                InventoryChangedMethod.Invoke(
                    inventory,
                    null);
            }
            catch (Exception)
            {
            }
        }

        private static void SetFieldValue(
            FieldInfo field,
            object instance,
            object value)
        {
            if (field == null ||
                instance == null)
            {
                return;
            }

            try
            {
                field.SetValue(
                    instance,
                    value);
            }
            catch (Exception)
            {
            }
        }

        private static void SetBooleanField(
            object instance,
            string fieldName,
            bool value)
        {
            if (instance == null ||
                string.IsNullOrEmpty(fieldName))
            {
                return;
            }

            FieldInfo field =
                AccessTools.Field(
                    instance.GetType(),
                    fieldName);

            SetFieldValue(
                field,
                instance,
                value);
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

        private static PrivateArea FindPrivateArea(
            ZDO wardZdo)
        {
            if (wardZdo == null)
                return null;

            List<PrivateArea> privateAreas =
                GetPrivateAreas();

            for (int i = 0; i < privateAreas.Count; i++)
            {
                PrivateArea privateArea =
                    privateAreas[i];

                ZDO candidateZdo =
                    GetZdo(privateArea);

                if (candidateZdo == null)
                    continue;

                if (candidateZdo.m_uid.UserID ==
                        wardZdo.m_uid.UserID &&
                    candidateZdo.m_uid.ID ==
                        wardZdo.m_uid.ID)
                {
                    return privateArea;
                }
            }

            return null;
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

    internal static class PhysicalTreasuryRuntime
    {
        private static PhysicalTreasuryService _service;
        private static GameObject _runnerObject;

        public static PhysicalTreasuryService Service
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
                    new PhysicalTreasuryService();
            }

            if (_runnerObject != null)
                return;

            _runnerObject =
                new GameObject(
                    "ClanTerritory_PhysicalTreasuryRunner");

            UnityEngine.Object.DontDestroyOnLoad(
                _runnerObject);

            _runnerObject.AddComponent<
                PhysicalTreasuryRunner>();

            ModLog.Info(
                "[TerritoryTreasury] Physical Treasury runtime started.");
        }

        public static void Update()
        {
            if (_service != null)
                _service.Update();
        }
    }

    internal sealed class PhysicalTreasuryRunner :
        MonoBehaviour
    {
        private void Update()
        {
            PhysicalTreasuryRuntime.Update();
        }
    }
}

namespace ClanTerritory.Integration.Valheim.Harmony
{
    using ClanTerritory.Domain.Identifiers;
    using ClanTerritory.Features.Territory;
    using ClanTerritory.Features.Territory.Services;
    using ClanTerritory.Features.WardDetection.Services;

    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class PhysicalTreasuryRuntimeStartHook
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            PhysicalTreasuryRuntime.EnsureStarted();
        }
    }

    [HarmonyPatch(
        typeof(TerritoryTerraformingService),
        "RequestOpenTreasuryChest")]
    internal static class PhysicalTreasuryOpenHook
    {
        [HarmonyPrefix]
        private static bool Prefix(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            ref bool __result)
        {
            __result =
                PhysicalTreasuryRuntime.Service
                    .RequestOpen(
                        wardId,
                        privateArea,
                        player);

            return false;
        }
    }

    [HarmonyPatch(
        typeof(TerritoryTerraformingService),
        "CreateVirtualStorageInventory")]
    internal static class PhysicalTreasuryStorageHook
    {
        [HarmonyPrefix]
        private static bool Prefix(
            ZDO zdo,
            string itemKey,
            ref Inventory __result)
        {
            if (itemKey !=
                TerritoryZdoKeys.TreasuryChestItems)
            {
                return true;
            }

            __result =
                PhysicalTreasuryRuntime.Service
                    .GetTreasuryInventory(zdo);

            return false;
        }
    }

    [HarmonyPatch(
        typeof(TerritoryTerraformingService),
        "PersistVirtualInventoryToZdo")]
    internal static class PhysicalTreasuryPersistenceHook
    {
        [HarmonyPrefix]
        private static bool Prefix(
            string itemKey)
        {
            return itemKey !=
                   TerritoryZdoKeys.TreasuryChestItems;
        }
    }

    [HarmonyPatch(
        typeof(TerritoryTerraformingService),
        "IsAbsorbableGroundItem")]
    internal static class PhysicalTreasuryGroundOwnershipHook
    {
        [HarmonyPrefix]
        private static bool Prefix(
            ItemDrop drop,
            PrivateArea privateArea,
            ref bool __result)
        {
            if (drop == null ||
                privateArea == null ||
                drop.m_itemData == null ||
                drop.m_itemData.m_stack <= 0)
            {
                __result = false;
                return false;
            }

            if (drop.IsPiece())
            {
                __result = false;
                return false;
            }

            if (!PhysicalTreasuryService
                    .EnsureGroundItemOwnership(drop))
            {
                __result = false;
                return false;
            }

            __result =
                global::Utils.DistanceXZ(
                    privateArea.transform.position,
                    drop.transform.position) <=
                privateArea.m_radius;

            return false;
        }
    }

    [HarmonyPatch(
        typeof(WardService),
        "UnregisterWard")]
    internal static class PhysicalTreasuryWardDestroyHook
    {
        [HarmonyPostfix]
        private static void Postfix(
            WardId wardId)
        {
            if (!PhysicalTreasuryService
                    .IsGameplayReady())
            {
                return;
            }

            PhysicalTreasuryRuntime.Service
                .DestroyTreasuryForWard(
                    wardId);
        }
    }

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

    [HarmonyPatch(
        typeof(Player),
        "RemovePiece")]
    internal static class PhysicalTreasuryPieceRemovalHook
    {
        private static readonly FieldInfo HoveringPieceField =
            AccessTools.Field(
                typeof(Player),
                "m_hoveringPiece");

        [HarmonyPrefix]
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
