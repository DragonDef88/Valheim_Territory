using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ClanTerritory.Config;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Persistence;
using ClanTerritory.Features.Territory;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Features.WorldDiscovery;
using ClanTerritory.Utils;

namespace ClanTerritory.Core
{
    internal static class Bootstrap
    {
        private static bool _initialized;
        private static ModuleManager _moduleManager;
        private static EventBus _eventBus;

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

            _eventBus = new EventBus();
            ServiceContainer.Register<EventBus>(_eventBus);

            _moduleManager = new ModuleManager();

            _moduleManager.Register(new PersistenceModule());
            _moduleManager.Register(new TerritoryModule());
            _moduleManager.Register(new WardDetectionModule());
            _moduleManager.Register(new WorldDiscoveryModule());

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

            if (_eventBus != null)
                _eventBus.Clear();

            ServiceContainer.Clear();

            _eventBus = null;
            _moduleManager = null;
            _initialized = false;

            ModLog.Info(ModInfo.Name + " shutdown.");
        }
    }
}