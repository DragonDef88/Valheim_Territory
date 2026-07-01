using ClanTerritory.Config;
using ClanTerritory.Core;

namespace ClanTerritory.Utils
{
    internal static class ModLog
    {
        public static void Info(string message)
        {
            Globals.Log.LogInfo(message);
        }

        public static void Warning(string message)
        {
            Globals.Log.LogWarning(message);
        }

        public static void Error(string message)
        {
            Globals.Log.LogError(message);
        }

        public static void Debug(string message)
        {
            if (ConfigValues.DebugMode)
                Globals.Log.LogInfo("[DEBUG] " + message);
        }
    }
}