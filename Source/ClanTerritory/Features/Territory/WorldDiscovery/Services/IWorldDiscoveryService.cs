using System.Collections.Generic;
using ClanTerritory.Features.WardDetection.Models;

namespace ClanTerritory.Features.WorldDiscovery.Services
{
    internal interface IWorldDiscoveryService
    {
        IReadOnlyList<WardModel> Discover();
    }
}