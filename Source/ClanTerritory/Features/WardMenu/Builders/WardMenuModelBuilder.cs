using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Core;
using ClanTerritory.Features.BiomeDominion;
using ClanTerritory.Features.Diplomacy;
using ClanTerritory.Features.Economy;
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

            string creatorGuildDescription =
                BuildCreatorGuildDescription(creatorGuildName);

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
                creatorGuildDescription,
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

            WardMenuBiomeDominionSection biomeDominionSection =
                BuildBiomeDominionSection(
                    privateArea,
                    player);

            WardMenuEconomySection economySection =
                BuildEconomySection(
                    privateArea,
                    player);

            WardMenuDiplomacySection diplomacySection =
                BuildDiplomacySection(player);

            WardMenuModel model = new WardMenuModel(
                wardSection,
                territorySection,
                terraformingSection,
                biomeDominionSection,
                economySection,
                diplomacySection);

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
                ", biome: " + biomeDominionSection.BiomeName +
                ", biomeClaimed: " + biomeDominionSection.Claimed +
                ", biomeCanManage: " + biomeDominionSection.CanManage +
                ", economyAvailable: " + economySection.Available +
                ", economyBalance: " + economySection.Balance +
                ", terraformingEnabled: " + terraformingSection.Enabled +
                ", territoryName: " + territorySection.Name +
                ", runtimeActive: " + territorySection.RuntimeActive +
                ", permitted: " + wardSection.PermittedPlayers.Count);

            return model;
        }



        private static WardMenuDiplomacySection BuildDiplomacySection(Player player)
        {
            DiplomacyService diplomacyService;

            if (!ServiceContainer.TryGet<DiplomacyService>(out diplomacyService) ||
                diplomacyService == null)
            {
                return WardMenuDiplomacySection.Unavailable();
            }

            DiplomacyMenuState state =
                diplomacyService.BuildMenuState(player);

            if (state == null)
                return WardMenuDiplomacySection.Unavailable();

            return new WardMenuDiplomacySection(
                state.Available,
                state.StatusText,
                state.GuildName,
                state.RelationsText,
                state.CanChange);
        }

        private static WardMenuEconomySection BuildEconomySection(
            PrivateArea privateArea,
            Player player)
        {
            EconomyService economyService;

            if (!ServiceContainer.TryGet<EconomyService>(out economyService) ||
                economyService == null)
            {
                return WardMenuEconomySection.Unavailable();
            }

            EconomyMenuState state =
                economyService.BuildMenuState(
                    privateArea,
                    player);

            if (state == null)
                return WardMenuEconomySection.Unavailable();

            return new WardMenuEconomySection(
                state.Available,
                state.StatusText,
                state.GuildName,
                state.TerritoryGuildName,
                state.Balance,
                state.DepositedTotal,
                state.WithdrawnTotal,
                state.UpkeepPaidTotal,
                state.TributeReceivedTotal,
                state.TaxPaidTotal,
                state.TaxReceivedTotal,
                state.TransferSentTotal,
                state.TransferReceivedTotal,
                state.CanDeposit,
                state.CanWithdraw,
                state.CanPayUpkeep,
                state.CanPayTax,
                state.CanTransfer,
                state.DefaultAmount);
        }

        private static WardMenuBiomeDominionSection BuildBiomeDominionSection(
            PrivateArea privateArea,
            Player player)
        {
            BiomeDominionService biomeDominionService;

            if (!ServiceContainer.TryGet<BiomeDominionService>(out biomeDominionService) ||
                biomeDominionService == null)
            {
                return WardMenuBiomeDominionSection.Unavailable();
            }

            BiomeDominionMenuState state =
                biomeDominionService.BuildMenuState(
                    privateArea,
                    player);

            if (state == null)
                return WardMenuBiomeDominionSection.Unavailable();

            return new WardMenuBiomeDominionSection(
                state.BiomeName,
                state.Claimed,
                state.OwnerGuildName,
                state.Vassal,
                state.CanClaim,
                state.CanManage,
                state.DoorLockEnabled,
                state.StructureDamageProtectionEnabled,
                state.DoorAutoCloseSeconds);
        }

        private WardMenuTerraformingSection BuildTerraformingSection(PrivateArea privateArea)
        {
            return WardMenuTerraformingSection.Disabled();
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


        private static string BuildCreatorGuildDescription(string guildName)
        {
            if (string.IsNullOrEmpty(guildName))
                return "";

            IGuildService guildService;

            if (!ServiceContainer.TryGet<IGuildService>(out guildService) ||
                guildService == null ||
                !guildService.IsAvailable)
            {
                return "";
            }

            string description;

            if (!guildService.TryGetGuildDescription(
                    guildName,
                    out description))
            {
                return "";
            }

            return description ?? "";
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
