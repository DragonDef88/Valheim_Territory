using System.Collections.Generic;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Utils;
using UnityEngine;

namespace ClanTerritory.Features.WorldDiscovery.Services
{
    internal sealed class WorldDiscoveryService : IWorldDiscoveryService
    {
        private const string WardPieceName = "guard_stone";

        public IReadOnlyList<WardModel> Discover()
        {
            List<WardModel> wards = new List<WardModel>();

            PrivateArea[] areas = Object.FindObjectsByType<PrivateArea>(FindObjectsSortMode.None);

            for (int i = 0; i < areas.Length; i++)
            {
                PrivateArea area = areas[i];

                if (area == null)
                    continue;

                if (!area.name.Contains(WardPieceName))
                    continue;

                ZNetView zNetView = area.GetComponent<ZNetView>();

                if (zNetView == null || zNetView.GetZDO() == null)
                    continue;

                string wardId = zNetView.GetZDO().m_uid.ToString();

                WardModel model = new WardModel(
                    wardId,
                    0L,
                    "Unknown",
                    area.transform.position,
                    area.isActiveAndEnabled);

                wards.Add(model);
            }

            ModLog.Info("World discovery completed. Wards found: " + wards.Count);

            return wards;
        }
    }
}