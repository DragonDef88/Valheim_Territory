using BepInEx.Configuration;
using BepInEx.Logging;
using ClanTerritory.Config;
using ClanTerritory.Utils;
using HarmonyLib;
using System.ComponentModel.Design;

namespace ClanTerritory.Core
{
    internal static class Bootstrap
    {
        private static bool _initialized;
        private static ModuleManager _moduleManager;

        public static bool IsInitialized
        {
            get { return _initialized; }
        }

        public static void Initialize(Plugin plugin, ManualLogSource logger, ConfigFile config)
        {
            if (_initialized)
                return;

            Globals.Plugin = plugin;
            Globals.Log = logger;
            Globals.Config = config;
            Globals.Harmony = new Harmony(ModInfo.Guid);

            ConfigManager.Initialize(config);

            ServiceContainer.Clear();

            _moduleManager = new ModuleManager();
            ServiceContainer.Register<ModuleManager>(_moduleManager);

            _moduleManager.InitializeAll();

            Globals.Harmony.PatchAll();

            _initialized = true;

            ModLog.Info(ModInfo.Name + " v" + ModInfo.Version + " initialized.");
        }

        public static void Shutdown()
        {
            if (!_initialized)
                return;

            Globals.Harmony.UnpatchSelf();

            if (_moduleManager != null)
                _moduleManager.ShutdownAll();

            ServiceContainer.Clear();

            _moduleManager = null;
            _initialized = false;

            ModLog.Info(ModInfo.Name + " shutdown.");
        }
    }
}