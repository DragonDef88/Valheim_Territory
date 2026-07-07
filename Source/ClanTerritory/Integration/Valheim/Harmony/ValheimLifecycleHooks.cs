using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Core;
using ClanTerritory.Features.Diagnostics.Services;
using HarmonyLib;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch]
    internal static class ValheimLifecycleHooks
    {
        private static readonly List<MethodBase> _methods =
            new List<MethodBase>();

        private static IEnumerable<MethodBase> TargetMethods()
        {
            AddIfExists(typeof(Game), "Start");
            AddIfExists(typeof(ZNet), "Awake");
            AddIfExists(typeof(ZNet), "Start");
            AddIfExists(typeof(ZNetScene), "Awake");
            AddIfExists(typeof(ZNetScene), "Start");
            AddIfExists(typeof(ZoneSystem), "Awake");
            AddIfExists(typeof(ZoneSystem), "Start");
            AddIfExists(typeof(Player), "Awake");
            AddIfExists(typeof(Player), "Start");

            foreach (MethodBase method in _methods)
                yield return method;
        }

        private static void AddIfExists(System.Type type, string methodName)
        {
            MethodInfo method =
                type.GetMethod(
                    methodName,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Static);

            if (method != null && !_methods.Contains(method))
                _methods.Add(method);
        }

        private static void Postfix(MethodBase __originalMethod)
        {
            IDiagnosticsService diagnostics;

            if (!ServiceContainer.TryGet<IDiagnosticsService>(out diagnostics))
                return;

            string checkpoint =
                __originalMethod.DeclaringType.Name +
                "." +
                __originalMethod.Name +
                "()";

            diagnostics.LogWorldState(checkpoint);
        }
    }
}