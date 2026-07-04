using System;
using HarmonyLib;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.World.Services
{
    internal sealed class WorldInfoService : IWorldInfoService
    {
        private const string FallbackWorldName = "Unknown";

        public string GetWorldName()
        {
            try
            {
                if (ZNet.instance == null)
                    return FallbackWorldName;

                object world = AccessTools.Field(typeof(ZNet), "m_world")
                    ?.GetValue(ZNet.instance);

                if (world == null)
                    return FallbackWorldName;

                string worldName = AccessTools.Field(world.GetType(), "m_name")
                    ?.GetValue(world) as string;

                if (string.IsNullOrWhiteSpace(worldName))
                    return FallbackWorldName;

                return SanitizeFileName(worldName);
            }
            catch (Exception ex)
            {
                ModLog.Warning("Failed to get world name: " + ex.Message);
                return FallbackWorldName;
            }
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            return value.Trim();
        }
    }
}