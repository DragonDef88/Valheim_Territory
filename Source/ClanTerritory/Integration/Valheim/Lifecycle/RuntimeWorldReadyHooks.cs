using HarmonyLib;
using ClanTerritory.Core;

namespace ClanTerritory.Integration.Valheim.Lifecycle
{
    [HarmonyPatch(typeof(Game), "UpdateRespawn")]
    internal static class RuntimeWorldReadyHooks
    {
        private static void Postfix()
        {
            RuntimeStateMachine stateMachine;

            if (!ServiceContainer.TryGet<RuntimeStateMachine>(out stateMachine))
                return;

            if (stateMachine.State != RuntimeState.InfrastructureReady)
                return;

            if (Player.m_localPlayer == null)
                return;

            PrivateArea[] privateAreas =
                UnityEngine.Object.FindObjectsByType<PrivateArea>(
                    UnityEngine.FindObjectsSortMode.None);

            if (privateAreas == null || privateAreas.Length == 0)
                return;

            stateMachine.SetState(RuntimeState.WorldLoaded);
        }
    }
}