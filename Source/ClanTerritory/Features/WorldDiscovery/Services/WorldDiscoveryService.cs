using System.Collections.Generic;
using ClanTerritory.Features.Territory.WorldDiscovery.Scanners;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Utils;
using UnityEngine;

namespace ClanTerritory.Features.WorldDiscovery.Services
{
    internal sealed class WorldDiscoveryService : IWorldDiscoveryService
    {
        private readonly PrivateAreaScanner _scanner;

        public WorldDiscoveryService()
        {
            _scanner = new PrivateAreaScanner();
        }

        public IReadOnlyList<WardModel> Discover()
        {
            List<WardModel> wards = new List<WardModel>();

            PrivateArea[] areas =
                Object.FindObjectsByType<PrivateArea>(
                    FindObjectsSortMode.None);

            foreach (PrivateArea area in areas)
            {
                WardModel model;

                if (_scanner.TryCreateWardModel(area, out model))
                    wards.Add(model);
            }

            ModLog.Info(
                "World discovery completed. Wards found: " +
                wards.Count);

            return wards;
        }
    }
}