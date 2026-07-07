using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Builders
{
    internal sealed class WardMenuModelBuilder
    {
        public WardMenuModel Build(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea)
        {
            ZNetView zNetView = privateArea.GetComponent<ZNetView>();
            ZDO zdo = zNetView != null && zNetView.IsValid()
                ? zNetView.GetZDO()
                : null;

            if (zdo == null)
                ModLog.Debug("[WardMenu] Building ward territory model without ZDO: " + wardId);

            string ownerName = zdo != null
                ? zdo.GetString(ZDOVars.s_creatorName, "Unknown")
                : "Unknown";

            bool enabled = zdo != null && zdo.GetBool(ZDOVars.s_enabled);

            List<WardMenuPlayerModel> permittedPlayers =
                BuildPermittedPlayers(zdo);

            WardMenuWardSection wardSection = new WardMenuWardSection(
                wardId,
                ownerName,
                privateArea.m_radius,
                enabled,
                permittedPlayers);

            WardMenuTerritorySection territorySection = new WardMenuTerritorySection(
                "Unnamed Territory",
                runtimeWard != null && runtimeWard.IsActive,
                false,
                false,
                "Default rules");

            WardMenuModel model = new WardMenuModel(
                wardSection,
                territorySection);

            ModLog.Debug(
                "[WardMenu] Ward territory model created: " + wardId +
                ", owner: " + wardSection.OwnerName +
                ", radius: " + wardSection.Radius +
                ", enabled: " + wardSection.Enabled +
                ", runtimeActive: " + territorySection.RuntimeActive +
                ", permitted: " + wardSection.PermittedPlayers.Count);

            return model;
        }

        private static List<WardMenuPlayerModel> BuildPermittedPlayers(ZDO zdo)
        {
            List<WardMenuPlayerModel> players =
                new List<WardMenuPlayerModel>();

            if (zdo == null)
                return players;

            int permittedCount = zdo.GetInt(ZDOVars.s_permitted);

            for (int i = 0; i < permittedCount; i++)
            {
                long playerId = zdo.GetLong("pu_id" + i, 0L);
                string playerName = zdo.GetString("pu_name" + i, "Unknown");

                if (playerId == 0L)
                {
                    ModLog.Debug("[WardMenu] Ignored permitted player with empty id at index: " + i);
                    continue;
                }

                players.Add(new WardMenuPlayerModel(
                    playerId,
                    playerName));
            }

            return players;
        }
    }
}