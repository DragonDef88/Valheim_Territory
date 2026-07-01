using BepInEx.Configuration;

namespace ClanTerritory.Config
{
    internal static class ConfigManager
    {
        public static ConfigEntry<float> TerritoryRadius;
        public static ConfigEntry<bool> AllowOverlap;
        public static ConfigEntry<bool> DebugMode;

        public static void Initialize(ConfigFile config)
        {
            TerritoryRadius = config.Bind(
                "Territory",
                "Radius",
                100f,
                new ConfigDescription(
                    "Territory radius in meters.",
                    new AcceptableValueRange<float>(50f, 200f)));

            AllowOverlap = config.Bind(
                "Territory",
                "AllowOverlap",
                false,
                "Allow territories to overlap.");

            DebugMode = config.Bind(
                "Debug",
                "Enabled",
                false,
                "Enable debug logging.");
        }
    }
}