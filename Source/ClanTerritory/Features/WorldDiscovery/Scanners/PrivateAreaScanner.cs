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

            long ownerId = piece.GetCreator();

            if (ownerId == 0L)
                return false;

            string ownerName = zNetView.GetZDO().GetString(
                ZDOVars.s_creatorName,
                "Unknown");

            string wardId = zNetView.GetZDO().m_uid.ToString();

            model = new WardModel(
                wardId,
                ownerId,
                string.IsNullOrWhiteSpace(ownerName) ? "Unknown" : ownerName,
                area.transform.position,
                area.isActiveAndEnabled);

            return true;
        }
    }
}