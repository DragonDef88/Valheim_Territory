namespace ClanTerritory.Features.World.Services
{
    internal sealed class WorldInfoService : IWorldInfoService
    {
        public string GetWorldName()
        {
            // Пока временная реализация.
            // На следующем этапе будем получать настоящее имя мира из Valheim.
            return "Unknown";
        }
    }
}