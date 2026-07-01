using ClanTerritory.Features.WardDetection.Models;

namespace ClanTerritory.Features.Territory.Services
{
    internal interface ITerritoryService
    {
        void CreateTerritoryFromWard(WardModel ward);
    }
}