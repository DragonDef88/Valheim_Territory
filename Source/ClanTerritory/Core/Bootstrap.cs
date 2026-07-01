using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ClanTerritory.Config;
using ClanTerritory.Utils;

namespace ClanTerritory.Core
{
    internal static class Bootstrap
    {
        private static bool _initialized;

        public static void Initialize(Plugin plugin, ManualLogSource logger, ConfigFile config)
        {
            if (_initialized)
                return;

            Globals.Plugin = plugin;
            Globals.Log = logger;
            Globals.Config = config;
            Globals.Harmony = new Harmony(ModInfo.Guid);

            ConfigManager.Initialize(config);

            Globals.Harmony.PatchAll();

            _initialized = true;

            ModLog.Info(ModInfo.Name + " v" + ModInfo.Version + " initialized.");
        }

        public static void Shutdown()
        {
            if (!_initialized)
                return;

            Globals.Harmony.UnpatchSelf();

            _initialized = false;

            ModLog.Info(ModInfo.Name + " shutdown.");
        }
    }
}