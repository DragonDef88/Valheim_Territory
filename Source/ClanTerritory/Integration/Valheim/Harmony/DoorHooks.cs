using ClanTerritory.Core;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Localization;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(Door))]
    internal static class DoorHooks
    {
        [HarmonyPrefix]
        [HarmonyPatch("Interact")]
        private static bool InteractPrefix(
            Door __instance,
            Humanoid character,
            bool hold,
            bool alt,
            ref bool __result)
        {
            if (hold)
                return true;

            Player player = character as Player;

            if (player == null)
                return true;

            if (!IsDoorLocked(
                    __instance,
                    player))
            {
                return true;
            }

            BlockDoorInteraction(
                __instance,
                character);

            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UseItem")]
        private static bool UseItemPrefix(
            Door __instance,
            Humanoid user,
            ItemDrop.ItemData item,
            ref bool __result)
        {
            Player player = user as Player;

            if (player == null)
                return true;

            if (!IsDoorLocked(
                    __instance,
                    player))
            {
                return true;
            }

            BlockDoorInteraction(
                __instance,
                user);

            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RPC_UseDoor")]
        private static void RPCUseDoorPostfix(Door __instance)
        {
            TerritoryRuleService ruleService;

            if (!ServiceContainer.TryGet<TerritoryRuleService>(out ruleService))
                return;

            ruleService.ScheduleDoorAutoClose(__instance);
        }

        private static bool IsDoorLocked(
            Door door,
            Player player)
        {
            if (door == null || player == null)
                return false;

            TerritoryRuleService ruleService;

            if (!ServiceContainer.TryGet<TerritoryRuleService>(out ruleService))
                return false;

            if (!ruleService.IsDoorLockedForPlayer(
                    door.transform.position,
                    player))
            {
                return false;
            }

            return !TerritoryGuildAccess.HasGuildAccessAt(
                door.transform.position,
                player);
        }

        private static void BlockDoorInteraction(
            Door door,
            Humanoid user)
        {
            if (door != null)
            {
                door.m_lockedEffects.Create(
                    door.transform.position,
                    door.transform.rotation);
            }

            if (user != null)
            {
                user.Message(
                    MessageHud.MessageType.Center,
                    CtLocalization.Get("ct.message.doors_locked"));
            }
        }
    }
}
