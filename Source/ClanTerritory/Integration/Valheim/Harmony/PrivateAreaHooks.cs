using System.Reflection;
using HarmonyLib;
using ClanTerritory.Core;
using ClanTerritory.Features.Territory.WorldDiscovery.Scanners;
using ClanTerritory.Features.TerritoryInteraction.Services;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WardDetection.Services;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(PrivateArea))]
    internal static class PrivateAreaHooks
    {
        private static readonly PrivateAreaScanner Scanner =
            new PrivateAreaScanner();

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
            if (!TerritoryTerraformingService.IsPreparationChestInventory(__instance))
                return true;

            TerritoryTerraformingService.TryAutoMoveItemToPreparationChest(
                __instance,
                fromInventory,
                item);

            return false;
        }
    }

}
