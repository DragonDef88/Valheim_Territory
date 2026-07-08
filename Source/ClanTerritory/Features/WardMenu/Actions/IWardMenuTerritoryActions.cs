using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal interface IWardMenuTerritoryActions
    {
        void RenameTerritory(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string name);

        bool ToggleDoorLock(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool SetDoorAutoCloseSeconds(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int seconds);

        bool ToggleStructureDamageProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool ToggleTerraforming(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool ToggleTerraformingRunning(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool DecreaseTerraformingRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool IncreaseTerraformingRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool StoreTerraformingHoe(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool StoreTerraformingPickaxe(
            WardId wardId,
            PrivateArea privateArea,
            Player player);

        bool AddTerraformingFuelSlot(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int slotIndex);

        bool AddTerraformingStoneSlot(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int slotIndex);

        void ToggleGuildAccess(WardId wardId);

        void ToggleGroupAccess(WardId wardId);
    }
}
