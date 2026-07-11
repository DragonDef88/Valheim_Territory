using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ClanTerritory.Core;
using ClanTerritory.Features.Territory.WorldDiscovery.Scanners;
using ClanTerritory.Features.TerritoryInteraction.Services;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WardDetection.Services;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Utils;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(PrivateArea))]
    internal static class PrivateAreaHooks
    {
        private static readonly PrivateAreaScanner Scanner =
            new PrivateAreaScanner();

        private static readonly MethodInfo InventoryGuiIsVisibleMethod =
            AccessTools.Method(
                typeof(InventoryGui),
                "IsVisible",
                Type.EmptyTypes);

        private static bool IsInteractionBlockedByOpenUi()
        {
            if (IsInventoryGuiVisible())
                return true;

            if (Console.IsVisible())
                return true;

            if (Menu.IsVisible())
                return true;

            if (Minimap.IsOpen())
                return true;

            return false;
        }

        private static bool IsInventoryGuiVisible()
        {
            if (InventoryGui.instance == null)
                return false;

            try
            {
                if (InventoryGuiIsVisibleMethod != null)
                {
                    object result =
                        InventoryGuiIsVisibleMethod.Invoke(
                            InventoryGui.instance,
                            null);

                    if (result is bool)
                        return (bool)result;
                }
            }
            catch (Exception exception)
            {
                ModLog.Debug("[Compatibility] InventoryGui.IsVisible reflection failed: " + exception.GetType().Name);
            }

            GameObject gameObject = InventoryGui.instance.gameObject;

            return gameObject != null && gameObject.activeInHierarchy;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void AwakePostfix(PrivateArea __instance)
        {
            ITerritoryNamingService territoryNamingService;

            if (ServiceContainer.TryGet<ITerritoryNamingService>(out territoryNamingService))
                territoryNamingService.RegisterRpc(__instance);

            TerritoryWardRadiusService territoryWardRadiusService;

            if (ServiceContainer.TryGet<TerritoryWardRadiusService>(out territoryWardRadiusService))
            {
                territoryWardRadiusService.RegisterRpc(__instance);
                territoryWardRadiusService.ApplyStoredOrConfiguredRadius(__instance);
            }

            TerritoryRuleService territoryRuleService;

            if (ServiceContainer.TryGet<TerritoryRuleService>(out territoryRuleService))
                territoryRuleService.RegisterRpc(__instance);

            TerritoryTerraformingService territoryTerraformingService;

            if (ServiceContainer.TryGet<TerritoryTerraformingService>(out territoryTerraformingService))
                territoryTerraformingService.RegisterRpc(__instance);

            if (!Scanner.TryCreateWardModel(
                    __instance,
                    out WardModel model))
                return;

            IWardService wardService;

            if (ServiceContainer.TryGet<IWardService>(out wardService))
                wardService.RegisterWard(model);
        }

        [HarmonyPostfix]
        [HarmonyPatch("IsPermitted", new[] { typeof(long) })]
        private static void IsPermittedPostfix(
            PrivateArea __instance,
            long playerID,
            ref bool __result)
        {
            if (__result)
                return;

            if (__instance == null || playerID == 0L)
                return;

            ZNetView zNetView = __instance.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return;

            if (TerritoryGuildAccess.HasGuildAccess(
                    zdo,
                    playerID))
            {
                __result = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("Interact")]
        private static bool InteractPrefix(
            PrivateArea __instance,
            Humanoid human,
            bool hold,
            bool alt,
            ref bool __result)
        {
            if (hold)
            {
                __result = false;
                return false;
            }

            Player player = human as Player;

            if (player == null)
                return true;

            if (IsInteractionBlockedByOpenUi())
            {
                ModLog.Debug("[Compatibility] PrivateArea interaction ignored because another UI/container is open.");
                __result = false;
                return false;
            }

            ITerritoryInteractionService territoryInteractionService;

            if (!ServiceContainer.TryGet<ITerritoryInteractionService>(out territoryInteractionService))
                return true;

            if (!territoryInteractionService.TryOpenTerritoryMenu(__instance, player))
                return true;

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryGrid))]
    internal static class InventoryGridDropItemHook
    {
        [HarmonyPrefix]
        [HarmonyPatch("DropItem")]
        private static bool DropItemPrefix(
            InventoryGrid __instance,
            Inventory fromInventory,
            ItemDrop.ItemData item,
            int amount,
            Vector2i pos,
            ref bool __result)
        {
            if (__instance == null)
                return true;

            Inventory targetInventory = __instance.GetInventory();

            bool handled =
                TerritoryTerraformingService.TryMoveItemToPreparationSlot(
                    targetInventory,
                    fromInventory,
                    item,
                    amount,
                    pos,
                    out __result);

            if (handled)
                return false;

            handled =
                TerritoryTerraformingService.TryMoveItemToTreasurySlot(
                    targetInventory,
                    fromInventory,
                    item,
                    amount,
                    pos,
                    out __result);

            return !handled;
        }
    }

    [HarmonyPatch]
    internal static class InventoryMoveItemToPreparationChestHook
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(Inventory),
                "MoveItemToThis",
                new[]
                {
                    typeof(Inventory),
                    typeof(ItemDrop.ItemData)
                });
        }

        private static bool Prefix(
            Inventory __instance,
            Inventory fromInventory,
            ItemDrop.ItemData item)
        {
            if (TerritoryTerraformingService.IsPreparationChestInventory(__instance))
            {
                TerritoryTerraformingService.TryAutoMoveItemToPreparationChest(
                    __instance,
                    fromInventory,
                    item);

                return false;
            }

            if (TerritoryTerraformingService.IsTreasuryChestInventory(__instance))
            {
                TerritoryTerraformingService.TryAutoMoveItemToTreasuryChest(
                    __instance,
                    fromInventory,
                    item);

                return false;
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(InventoryGrid))]
    internal static class InventoryGridUpdatePreparationChestVisibilityHook
    {
        [HarmonyPostfix]
        [HarmonyPatch("UpdateInventory")]
        private static void UpdateInventoryPostfix(
            InventoryGrid __instance,
            Inventory inventory)
        {
            TerritoryTerraformingService.ApplyPreparationChestGridVisibility(
                __instance,
                inventory);
        }
    }


    [HarmonyPatch(typeof(InventoryGui))]
    internal static class InventoryGuiHideVirtualTerritoryContainerHook
    {
        private static readonly FieldInfo CurrentContainerField =
            AccessTools.Field(
                typeof(InventoryGui),
                "m_currentContainer");

        private static Container _closingContainer;

        [HarmonyPrefix]
        [HarmonyPatch("Hide")]
        private static void HidePrefix(InventoryGui __instance)
        {
            _closingContainer = null;

            if (__instance == null || CurrentContainerField == null)
                return;

            Container container = CurrentContainerField.GetValue(__instance) as Container;

            if (!TerritoryTerraformingService.IsVirtualTerritoryContainer(container))
                return;

            _closingContainer = container;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Hide")]
        private static void HidePostfix()
        {
            if (_closingContainer == null)
                return;

            TerritoryTerraformingService.CloseVirtualTerritoryContainer(_closingContainer);
            _closingContainer = null;
        }
    }

    [HarmonyPatch]
    internal static class TerritoryTerraformingLevelTerrainHook
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(TerrainComp),
                "LevelTerrain");
        }

        private static bool Prefix(
            TerrainComp __instance,
            Vector3 worldPos,
            float radius,
            bool square)
        {
            if (!TerritoryTerraformingService.ShouldUseWardHeightFalloffLeveling())
                return true;

            bool handled =
                TerritoryTerraformingService.TryApplyWardHeightFalloffLevelTerrain(
                    __instance,
                    worldPos,
                    radius,
                    square);

            return !handled;
        }
    }


}
