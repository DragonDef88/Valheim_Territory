using System;
using System.Reflection;
using UnityEngine;
using ClanTerritory.Utils;

using Object = UnityEngine.Object;

namespace ClanTerritory.Features.Diagnostics.Services
{
    internal sealed class DiagnosticsService : IDiagnosticsService
    {
        public void LogCheckpoint(string checkpoint)
        {
            ModLog.Info("[Diagnostics] " + checkpoint);
        }

        public void LogWorldState(string checkpoint)
        {
            int privateAreas = CountObjects<PrivateArea>();
            int players = CountObjects<Player>();

            string zNetState = GetInstanceState(typeof(ZNet));
            string zNetSceneState = GetInstanceState(typeof(ZNetScene));
            string zoneSystemState = GetInstanceState(typeof(ZoneSystem));

            ModLog.Info(
                "[Diagnostics] Runtime Checkpoint\n" +
                "Checkpoint: " + checkpoint + "\n" +
                "PrivateArea Count: " + privateAreas + "\n" +
                "Player Count: " + players + "\n" +
                "ZNet: " + zNetState + "\n" +
                "ZNetScene: " + zNetSceneState + "\n" +
                "ZoneSystem: " + zoneSystemState);
        }

        private static int CountObjects<T>() where T : Object
        {
            try
            {
                T[] objects =
                    Object.FindObjectsByType<T>(
                        FindObjectsSortMode.None);

                return objects == null ? 0 : objects.Length;
            }
            catch (Exception ex)
            {
                ModLog.Warning(
                    "[Diagnostics] Count failed: " +
                    typeof(T).Name +
                    " — " +
                    ex.Message);

                return -1;
            }
        }

        private static string GetInstanceState(Type type)
        {
            try
            {
                FieldInfo field =
                    type.GetField(
                        "instance",
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static);

                if (field == null)
                    return "NO_INSTANCE_FIELD";

                object value = field.GetValue(null);

                return value == null ? "NULL" : "READY";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }
}