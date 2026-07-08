using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.TerritoryNaming.Services;
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

        public bool CycleTerraformingMode(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("CycleTerraformingMode", wardId))
                return false;

            return _territoryTerraformingService.RequestCycleMode(wardId, privateArea, player);
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

        public bool SetTerraformingTargetHeightFromWard(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("SetTerraformingTargetHeightFromWard", wardId))
                return false;

            return _territoryTerraformingService.RequestSetTargetHeightFromWard(wardId, privateArea, player);
        }

        public bool SetTerraformingTargetHeightFromPlayer(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("SetTerraformingTargetHeightFromPlayer", wardId))
                return false;

            return _territoryTerraformingService.RequestSetTargetHeightFromPlayer(wardId, privateArea, player);
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

        public bool AddTerraformingFuel(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("AddTerraformingFuel", wardId))
                return false;

            return _territoryTerraformingService.RequestAddFuel(wardId, privateArea, player);
        }

        public bool AddTerraformingStone(WardId wardId, PrivateArea privateArea, Player player)
        {
            if (!TryGetTerraformingService("AddTerraformingStone", wardId))
                return false;

            return _territoryTerraformingService.RequestAddStone(wardId, privateArea, player);
        }

        public void ToggleGuildAccess(WardId wardId)
        {
            ModLog.Debug("[WardMenuActions] ToggleGuildAccess requested: " + wardId);
        }

        public void ToggleGroupAccess(WardId wardId)
        {
            ModLog.Debug("[WardMenuActions] ToggleGroupAccess requested: " + wardId);
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

            if (piece.GetCreator() != player.GetPlayerID())
            {
                ModLog.Debug("[WardMenuActions] " + actionName + " ignored. Player is not ward creator: " + wardId);
                return false;
            }

            return true;
        }
    }
}
