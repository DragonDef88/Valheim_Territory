using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Core;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.BiomeDominion;
using ClanTerritory.Features.Diplomacy;
using ClanTerritory.Features.Economy;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal sealed class WardMenuTerritoryActions : IWardMenuTerritoryActions
    {
        private readonly ITerritoryNamingService _territoryNamingService;
        private readonly TerritoryRuleService _territoryRuleService;
        private readonly TerritoryTerraformingService _territoryTerraformingService;

        public WardMenuTerritoryActions(
            ITerritoryNamingService territoryNamingService,
            TerritoryRuleService territoryRuleService,
            TerritoryTerraformingService territoryTerraformingService)
        {
            _territoryNamingService = territoryNamingService;
            _territoryRuleService = territoryRuleService;
            _territoryTerraformingService = territoryTerraformingService;
        }

        public void RenameTerritory(WardId wardId, PrivateArea privateArea, Player player, string name)
        {
            if (_territoryNamingService == null)
            {
                ModLog.Debug("[WardMenuActions] RenameTerritory ignored. TerritoryNamingService is null: " + wardId);
                return;
            }

            if (!IsWardCreator(wardId, privateArea, player, "RenameTerritory"))
                return;

            _territoryNamingService.RequestRename(privateArea, player, name);
            ModLog.Info("[WardMenuActions] RenameTerritory requested: " + wardId + ", name: " + name);
        }

        public bool ToggleDoorLock(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (_territoryRuleService == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleDoorLock ignored. TerritoryRuleService is null: " + wardId);
                return false;
            }

            bool nextValue = !_territoryRuleService.GetDoorLockEnabled(privateArea);

            return _territoryRuleService.RequestSetDoorLock(wardId, privateArea, player, nextValue);
        }

        public bool SetDoorAutoCloseSeconds(WardId wardId, PrivateArea privateArea, Player player, int seconds)
        {
            if (_territoryRuleService == null)
            {
                ModLog.Debug("[WardMenuActions] SetDoorAutoCloseSeconds ignored. TerritoryRuleService is null: " + wardId);
                return false;
            }

            return _territoryRuleService.RequestSetDoorAutoCloseSeconds(wardId, privateArea, player, seconds);
        }

        public bool ToggleStructureDamageProtection(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (_territoryRuleService == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleStructureDamageProtection ignored. TerritoryRuleService is null: " + wardId);
                return false;
            }

            bool nextValue = !_territoryRuleService.GetStructureDamageProtectionEnabled(privateArea);

            return _territoryRuleService.RequestSetStructureDamageProtection(wardId, privateArea, player, nextValue);
        }

        public bool ToggleTerraforming(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("ToggleTerraforming", wardId))
                return false;

            return _territoryTerraformingService.RequestToggleEnabled(wardId, privateArea, player);
        }

        public bool ToggleTerraformingRunning(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("ToggleTerraformingRunning", wardId))
                return false;

            return _territoryTerraformingService.RequestToggleRunning(wardId, privateArea, player);
        }

        public bool OpenTerraformingPreparationChest(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("OpenTerraformingPreparationChest", wardId))
                return false;

            return _territoryTerraformingService.RequestOpenPreparationChest(wardId, privateArea, player);
        }

        public bool OpenTreasuryChest(WardId wardId, PrivateArea privateArea, Player player)
        {
            return PhysicalTreasuryRuntime.Service.RequestOpen(
                wardId,
                privateArea,
                player);
        }

        public bool DecreaseTerraformingRadius(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("DecreaseTerraformingRadius", wardId))
                return false;

            return _territoryTerraformingService.RequestDecreaseRadius(wardId, privateArea, player);
        }

        public bool IncreaseTerraformingRadius(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("IncreaseTerraformingRadius", wardId))
                return false;

            return _territoryTerraformingService.RequestIncreaseRadius(wardId, privateArea, player);
        }

        public bool StoreTerraformingHoe(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("StoreTerraformingHoe", wardId))
                return false;

            return _territoryTerraformingService.RequestStoreHoe(wardId, privateArea, player);
        }

        public bool StoreTerraformingPickaxe(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("StoreTerraformingPickaxe", wardId))
                return false;

            return _territoryTerraformingService.RequestStorePickaxe(wardId, privateArea, player);
        }

        public bool AddTerraformingFuelSlot(WardId wardId, PrivateArea privateArea, Player player, int slotIndex)
        {
            if (!TryGetTerraformingService("AddTerraformingFuelSlot", wardId))
                return false;

            return _territoryTerraformingService.RequestAddFuelSlot(wardId, privateArea, player, slotIndex);
        }

        public bool AddTerraformingStoneSlot(WardId wardId, PrivateArea privateArea, Player player, int slotIndex)
        {
            if (!TryGetTerraformingService("AddTerraformingStoneSlot", wardId))
                return false;

            return _territoryTerraformingService.RequestAddStoneSlot(wardId, privateArea, player, slotIndex);
        }

        public bool ClaimBiomeDominion(WardId wardId, PrivateArea privateArea, Player player)
        {
            BiomeDominionService biomeDominionService;

            if (!TryGetBiomeDominionService("ClaimBiomeDominion", wardId, out biomeDominionService))
                return false;

            return biomeDominionService.RequestClaimBiome(privateArea, player);
        }

        public bool ReleaseBiomeDominion(WardId wardId, PrivateArea privateArea, Player player)
        {
            BiomeDominionService biomeDominionService;

            if (!TryGetBiomeDominionService("ReleaseBiomeDominion", wardId, out biomeDominionService))
                return false;

            return biomeDominionService.RequestReleaseBiome(privateArea, player);
        }

        public bool ToggleBiomeDoorLock(WardId wardId, PrivateArea privateArea, Player player)
        {
            BiomeDominionService biomeDominionService;

            if (!TryGetBiomeDominionService("ToggleBiomeDoorLock", wardId, out biomeDominionService))
                return false;

            BiomeDominionMenuState state =
                biomeDominionService.BuildMenuState(
                    privateArea,
                    player);

            bool nextValue = state == null || !state.DoorLockEnabled;

            return biomeDominionService.RequestSetBiomeDoorLock(
                privateArea,
                player,
                nextValue);
        }

        public bool SetBiomeDoorAutoCloseSeconds(WardId wardId, PrivateArea privateArea, Player player, int seconds)
        {
            BiomeDominionService biomeDominionService;

            if (!TryGetBiomeDominionService("SetBiomeDoorAutoCloseSeconds", wardId, out biomeDominionService))
                return false;

            return biomeDominionService.RequestSetBiomeDoorAutoCloseSeconds(
                privateArea,
                player,
                seconds);
        }

        public bool ToggleBiomeStructureDamageProtection(WardId wardId, PrivateArea privateArea, Player player)
        {
            BiomeDominionService biomeDominionService;

            if (!TryGetBiomeDominionService("ToggleBiomeStructureDamageProtection", wardId, out biomeDominionService))
                return false;

            BiomeDominionMenuState state =
                biomeDominionService.BuildMenuState(
                    privateArea,
                    player);

            bool nextValue = state == null || !state.StructureDamageProtectionEnabled;

            return biomeDominionService.RequestSetBiomeStructureDamageProtection(
                privateArea,
                player,
                nextValue);
        }


        public bool DepositEconomyCoins(WardId wardId, PrivateArea privateArea, Player player, int amount)
        {
            EconomyService economyService;

            if (!TryGetEconomyService("DepositEconomyCoins", wardId, out economyService))
                return false;

            return economyService.RequestDepositCoins(
                privateArea,
                player,
                amount);
        }

        public bool WithdrawEconomyCoins(WardId wardId, PrivateArea privateArea, Player player, int amount)
        {
            EconomyService economyService;

            if (!TryGetEconomyService("WithdrawEconomyCoins", wardId, out economyService))
                return false;

            return economyService.RequestWithdrawCoins(
                privateArea,
                player,
                amount);
        }

        public bool PayEconomyUpkeep(WardId wardId, PrivateArea privateArea, Player player, int amount)
        {
            EconomyService economyService;

            if (!TryGetEconomyService("PayEconomyUpkeep", wardId, out economyService))
                return false;

            return economyService.RequestPayCurrentTerritoryUpkeep(
                privateArea,
                player,
                amount);
        }

        public bool PayEconomyTax(WardId wardId, PrivateArea privateArea, Player player, int amount)
        {
            EconomyService economyService;

            if (!TryGetEconomyService("PayEconomyTax", wardId, out economyService))
                return false;

            return economyService.RequestPayCurrentTerritoryTax(
                privateArea,
                player,
                amount);
        }

        public bool TransferEconomyCoins(WardId wardId, PrivateArea privateArea, Player player, string targetGuildName, int amount)
        {
            EconomyService economyService;

            if (!TryGetEconomyService("TransferEconomyCoins", wardId, out economyService))
                return false;

            return economyService.RequestTransferToGuild(
                player,
                targetGuildName,
                amount);
        }

        public void ToggleGuildAccess(WardId wardId)
        {
            ModLog.Debug("[WardMenuActions] ToggleGuildAccess requested: " + wardId);
        }

        public void ToggleGroupAccess(WardId wardId)
        {
            ModLog.Debug("[WardMenuActions] ToggleGroupAccess requested: " + wardId);
        }


        public bool SetDiplomacyRelation(WardId wardId, Player player, string targetGuildName, DiplomacyRelationKind relation)
        {
            DiplomacyService diplomacyService;

            if (!ServiceContainer.TryGet<DiplomacyService>(out diplomacyService) ||
                diplomacyService == null)
            {
                ModLog.Debug("[WardMenuActions] SetDiplomacyRelation ignored. DiplomacyService is null: " + wardId);
                return false;
            }

            return diplomacyService.RequestSetRelation(
                player,
                targetGuildName,
                relation);
        }

        private static bool TryGetEconomyService(
            string actionName,
            WardId wardId,
            out EconomyService economyService)
        {
            if (ServiceContainer.TryGet<EconomyService>(out economyService) &&
                economyService != null)
            {
                return true;
            }

            ModLog.Debug("[WardMenuActions] " + actionName + " ignored. EconomyService is null: " + wardId);
            return false;
        }

        private static bool TryGetBiomeDominionService(
            string actionName,
            WardId wardId,
            out BiomeDominionService biomeDominionService)
        {
            if (ServiceContainer.TryGet<BiomeDominionService>(out biomeDominionService) &&
                biomeDominionService != null)
            {
                return true;
            }

            ModLog.Debug("[WardMenuActions] " + actionName + " ignored. BiomeDominionService is null: " + wardId);
            return false;
        }

        private bool TryGetTerraformingService(string actionName, WardId wardId)
        {
            if (_territoryTerraformingService != null)
                return true;

            ModLog.Debug("[WardMenuActions] " + actionName + " ignored. TerritoryTerraformingService is null: " + wardId);
            return false;
        }

        private static bool IsWardCreator(WardId wardId, PrivateArea privateArea, Player player, string actionName)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. Piece is null: " + wardId);
                return false;
            }

            if (piece.GetCreator() != player.GetPlayerID() &&
                !TerritoryGuildAccess.HasGuildAccess(
                    privateArea,
                    player))
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. Player is not ward creator or guild member: " + wardId);
                return false;
            }

            return true;
        }
    }
}
