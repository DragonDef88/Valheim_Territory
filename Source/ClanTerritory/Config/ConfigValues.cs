namespace ClanTerritory.Config
{    
    internal static class ConfigValues
    {
        public static float TerritoryRadius
        {
            get { return ConfigManager.TerritoryRadius.Value; }
        }

        public static bool AllowOverlap
        {
            get { return ConfigManager.AllowOverlap.Value; }
        }

        public static bool DebugMode
        {
            get { return ConfigManager.DebugMode.Value; }
        }
        public const int MaxWardsPerPlayer = 3;
    }
}