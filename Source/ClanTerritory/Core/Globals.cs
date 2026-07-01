using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ClanTerritory.Core
{
    internal static class Globals
    {
        public static Plugin Plugin;
        public static ManualLogSource Log;
        public static ConfigFile Config;
        public static Harmony Harmony;
    }
}