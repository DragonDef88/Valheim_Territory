using ClanTerritory.Config;
using ClanTerritory.Features.Territory;
using ClanTerritory.Features.WardDetection.Models;
using UnityEngine;

namespace ClanTerritory.Features.Territory.WorldDiscovery.Scanners
{
    internal sealed class PrivateAreaScanner
    {
        private const string WardPieceName = "guard_stone";

        public bool TryCreateWardModel(
            PrivateArea area,
            out WardModel model)
        {
            model = null;

            if (area == null)
                return false;

            if (!area.name.Contains(WardPieceName))
                return false;

            ZNetView zNetView = area.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return false;

            Piece piece = area.GetComponent<Piece>();

            if (piece == null)
                return false;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return false;

            long ownerId = piece.GetCreator();

            if (ownerId == 0L)
                return false;

            string ownerName = zdo.GetString(
                ZDOVars.s_creatorName,
                "Unknown");

            string wardId = zdo.m_uid.ToString();

            float radius = zdo.GetFloat(
                TerritoryZdoKeys.Radius,
                area.m_radius > 0f
                    ? area.m_radius
                    : ConfigValues.TerritoryRadius);

            model = new WardModel(
                wardId,
                ownerId,
                string.IsNullOrWhiteSpace(ownerName) ? "Unknown" : ownerName,
                area.transform.position,
                radius,
                area.isActiveAndEnabled);

            return true;
        }
    }
}
