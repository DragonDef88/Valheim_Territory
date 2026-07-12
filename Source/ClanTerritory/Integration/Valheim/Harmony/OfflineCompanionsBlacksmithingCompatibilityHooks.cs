using System;
using System.Reflection;
using ClanTerritory.Utils;
using HarmonyLib;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch]
    internal static class OfflineCompanionsBlacksmithingSkillFactorGuard
    {
        private static MethodBase _targetMethod;
        private static bool _targetLogged;
        private static bool _nullCharacterLogged;

        [HarmonyPrepare]
        private static bool Prepare()
        {
            _targetMethod =
                FindGetSkillFactorMethod();

            if (_targetMethod == null)
            {
                ModLog.Debug(
                    "[Compatibility] SkillManager.GetSkillFactor(Character, string) was not found. Offline Companions / Blacksmithing guard skipped.");

                return false;
            }

            if (!_targetLogged)
            {
                _targetLogged = true;

                ModLog.Info(
                    "[Compatibility] Offline Companions / Blacksmithing null-character skill guard enabled.");
            }

            return true;
        }

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            if (_targetMethod == null)
            {
                _targetMethod =
                    FindGetSkillFactorMethod();
            }

            return _targetMethod;
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Prefix(
            Character __0,
            ref float __result)
        {
            if (__0 != null)
                return true;

            __result = 0f;

            if (!_nullCharacterLogged)
            {
                _nullCharacterLogged = true;

                ModLog.Info(
                    "[Compatibility] Null Character passed to SkillManager.GetSkillFactor by a companion repair check. Base skill factor 0 was used.");
            }

            return false;
        }

        private static MethodBase FindGetSkillFactorMethod()
        {
            Type skillExtensionsType =
                AccessTools.TypeByName(
                    "SkillManager.SkillExtensions");

            if (skillExtensionsType == null)
                return null;

            MethodInfo[] methods =
                skillExtensionsType.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method =
                    methods[i];

                if (method == null ||
                    method.Name != "GetSkillFactor" ||
                    method.ReturnType != typeof(float))
                {
                    continue;
                }

                ParameterInfo[] parameters =
                    method.GetParameters();

                if (parameters.Length != 2)
                    continue;

                if (parameters[0].ParameterType !=
                        typeof(Character) ||
                    parameters[1].ParameterType !=
                        typeof(string))
                {
                    continue;
                }

                return method;
            }

            return null;
        }
    }

    [HarmonyPatch(
        typeof(CraftingStation),
        "GetLevel",
        new Type[] { typeof(bool) })]
    internal static class OfflineCompanionsBlacksmithingGetLevelFallback
    {
        private static bool _exceptionLogged;

        [HarmonyFinalizer]
        [HarmonyPriority(Priority.Last)]
        private static Exception Finalizer(
            Exception __exception,
            ref int __result)
        {
            if (__exception == null)
                return null;

            if (!IsKnownCompatibilityException(
                    __exception))
            {
                return __exception;
            }

            if (__result < 1)
                __result = 1;

            if (!_exceptionLogged)
            {
                _exceptionLogged = true;

                ModLog.Info(
                    "[Compatibility] Suppressed the known Offline Companions / Blacksmithing crafting-station level exception.");
            }

            return null;
        }

        private static bool IsKnownCompatibilityException(
            Exception exception)
        {
            if (!(exception is NullReferenceException))
                return false;

            string details =
                exception.ToString();

            if (string.IsNullOrEmpty(details))
                return false;

            bool hasSkillManagerFrame =
                details.IndexOf(
                    "SkillManager.SkillExtensions.GetSkillFactor",
                    StringComparison.Ordinal) >= 0;

            bool hasBlacksmithingFrame =
                details.IndexOf(
                    "Blacksmithing.Blacksmithing+IncreaseCraftingStationLevel.Postfix",
                    StringComparison.Ordinal) >= 0;

            return hasSkillManagerFrame &&
                   hasBlacksmithingFrame;
        }
    }
}
