using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Core;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Builders
{
    internal sealed class WardMenuModelBuilder
    {
        private readonly ITerritoryNamingService _territoryNamingService;
        private readonly TerritoryRuleService _territoryRuleService;
        private readonly TerritoryTerraformingService _territoryTerraformingService;

        public WardMenuModelBuilder(
            ITerritoryNamingService territoryNamingService,
            TerritoryRuleService territoryRuleService,
            TerritoryTerraformingService territoryTerraformingService)
        {
            _territoryNamingService = territoryNamingService;
            _territoryRuleService = territoryRuleService;
            _territoryTerraformingService = territoryTerraformingService;
        }

        public WardMenuModel Build(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea,
            Player player)
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

            string creatorGuildName = zdo != null
                ? BuildCreatorGuildName(zdo)
                : "";

            bool enabled = zdo != null && zdo.GetBool(ZDOVars.s_enabled);

            string territoryName = _territoryNamingService != null
                ? _territoryNamingService.GetTerritoryName(privateArea)
                : "Unnamed Territory";

            List<WardMenuPlayerModel> permittedPlayers = BuildPermittedPlayers(zdo);

            bool isCurrentPlayerCreator = HasCreatorOrGuildAccess(privateArea, player);
            bool isCurrentPlayerPermitted = IsCurrentPlayerPermitted(permittedPlayers, player);

            bool doorLockEnabled = _territoryRuleService != null && _territoryRuleService.GetDoorLockEnabled(privateArea);
            bool structureDamageProtectionEnabled = _territoryRuleService != null && _territoryRuleService.GetStructureDamageProtectionEnabled(privateArea);
            int doorAutoCloseSeconds = _territoryRuleService != null ? _territoryRuleService.GetDoorAutoCloseSeconds(privateArea) : 5;

            WardMenuWardSection wardSection = new WardMenuWardSection(
                wardId,
                ownerName,
                creatorGuildName,
                privateArea.m_radius,
                enabled,
                isCurrentPlayerCreator,
                isCurrentPlayerPermitted,
                permittedPlayers);

            WardMenuTerritorySection territorySection = new WardMenuTerritorySection(
                territoryName,
                runtimeWard != null && runtimeWard.IsActive,
                false,
                false,
                doorLockEnabled,
                structureDamageProtectionEnabled,
                doorAutoCloseSeconds,
                BuildRulesSummary(doorLockEnabled, structureDamageProtectionEnabled));

            WardMenuTerraformingSection terraformingSection =
                BuildTerraformingSection(privateArea);

            WardMenuModel model = new WardMenuModel(
                wardSection,
                territorySection,
                terraformingSection);

            ModLog.Debug(
                "[WardMenu] Ward territory model created: " + wardId +
                ", owner: " + wardSection.OwnerName +
                ", radius: " + wardSection.Radius +
                ", enabled: " + wardSection.Enabled +
                ", creator: " + wardSection.IsCurrentPlayerCreator +
                ", currentPermitted: " + wardSection.IsCurrentPlayerPermitted +
                ", doorsLocked: " + territorySection.DoorLockEnabled +
                ", structureDamageProtection: " + territorySection.StructureDamageProtectionEnabled +
                ", doorAutoCloseSeconds: " + territorySection.DoorAutoCloseSeconds +
                ", terraformingEnabled: " + terraformingSection.Enabled +
                ", territoryName: " + territorySection.Name +
                ", runtimeActive: " + territorySection.RuntimeActive +
                ", permitted: " + wardSection.PermittedPlayers.Count);

            return model;
        }

        private WardMenuTerraformingSection BuildTerraformingSection(PrivateArea privateArea)
        {
            if (_territoryTerraformingService == null)
                return WardMenuTerraformingSection.Disabled();

            TerraformingState state = _territoryTerraformingService.GetState(privateArea);

            return new WardMenuTerraformingSection(
                state.Enabled,
                state.Running,
                state.Radius,
                state.TargetHeight,
                state.FuelStored,
                state.StoneStored,
                state.FuelSlots,
                state.StoneSlots,
                state.HoeStored,
                state.PickaxeStored,
                state.AxeStored,
                state.ScanProgress,
                state.ScanIndex,
                BuildTerraformingStatus(state));
        }

        private static string BuildTerraformingStatus(TerraformingState state)
        {
            if (!state.Enabled)
                return "Disabled";

            if (state.Running)
                return "Running";

            return "Ready";
        }

        private static List<WardMenuPlayerModel> BuildPermittedPlayers(ZDO zdo)
        {
            List<WardMenuPlayerModel> players = new List<WardMenuPlayerModel>();

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

                players.Add(new WardMenuPlayerModel(playerId, playerName));
            }

            return players;
        }

        private static string BuildRulesSummary(bool doorLockEnabled, bool structureDamageProtectionEnabled)
        {
            return "Doors: " + (doorLockEnabled ? "Locked" : "Unlocked") +
                   "\nStructures: " + (structureDamageProtectionEnabled ? "Protected" : "Vulnerable");
        }

        private static string BuildCreatorGuildName(ZDO zdo)
        {
            if (zdo == null)
                return "";

            string guildName = zdo.GetString(
                ClanTerritory.Features.Territory.TerritoryZdoKeys.WardGuildName,
                "");

            return guildName ?? "";
        }

        private static bool HasCreatorOrGuildAccess(PrivateArea privateArea, Player player)
        {
            if (privateArea == null || player == null)
                return false;

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
                return false;

            if (piece.GetCreator() == player.GetPlayerID())
                return true;

            return TerritoryGuildAccess.HasGuildAccess(
                privateArea,
                player);
        }

        private static bool IsCurrentPlayerPermitted(List<WardMenuPlayerModel> permittedPlayers, Player player)
        {
            if (permittedPlayers == null || player == null)
                return false;

            long playerId = player.GetPlayerID();

            for (int i = 0; i < permittedPlayers.Count; i++)
            {
                WardMenuPlayerModel permittedPlayer = permittedPlayers[i];

                if (permittedPlayer.PlayerId == playerId)
                    return true;
            }

            return false;
        }
    }
}
