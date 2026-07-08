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

        public static int DoorAutoCloseSeconds
        {
            get
            {
                int value = ConfigManager.DoorAutoCloseSeconds.Value;

                if (value < 3)
                    return 3;

                if (value > 10)
                    return 10;

                return value;
            }
        }

        public static bool DebugMode
        {
            get { return ConfigManager.DebugMode.Value; }
        }

        public const int MaxWardsPerPlayer = 3;
    }
}
