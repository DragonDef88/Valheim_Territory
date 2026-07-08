using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Models
{
    internal sealed class WardMenuModel
    {
        public WardMenuWardSection Ward { get; private set; }

        public WardMenuTerritorySection Territory { get; private set; }

        public WardMenuTerraformingSection Terraforming { get; private set; }

        public WardMenuModel(
            WardMenuWardSection ward,
            WardMenuTerritorySection territory,
            WardMenuTerraformingSection terraforming)
        {
            Ward = ward;
            Territory = territory;
            Terraforming = terraforming;
        }
    }

    internal sealed class WardMenuWardSection
    {
        private readonly List<WardMenuPlayerModel> _permittedPlayers;

        public WardId WardId { get; private set; }

        public string OwnerName { get; private set; }

        public float Radius { get; private set; }

        public bool Enabled { get; private set; }

        public bool IsCurrentPlayerCreator { get; private set; }

        public bool IsCurrentPlayerPermitted { get; private set; }

        public IReadOnlyList<WardMenuPlayerModel> PermittedPlayers
        {
            get { return _permittedPlayers; }
        }

        public WardMenuWardSection(
            WardId wardId,
            string ownerName,
            float radius,
            bool enabled,
            bool isCurrentPlayerCreator,
            bool isCurrentPlayerPermitted,
            List<WardMenuPlayerModel> permittedPlayers)
        {
            WardId = wardId;
            OwnerName = ownerName;
            Radius = radius;
            Enabled = enabled;
            IsCurrentPlayerCreator = isCurrentPlayerCreator;
            IsCurrentPlayerPermitted = isCurrentPlayerPermitted;
            _permittedPlayers = permittedPlayers ?? new List<WardMenuPlayerModel>();
        }
    }

    internal sealed class WardMenuTerritorySection
    {
        public string Name { get; private set; }

        public bool RuntimeActive { get; private set; }

        public bool GuildAccessEnabled { get; private set; }

        public bool GroupAccessEnabled { get; private set; }

        public bool DoorLockEnabled { get; private set; }

        public bool StructureDamageProtectionEnabled { get; private set; }

        public int DoorAutoCloseSeconds { get; private set; }

        public string RulesSummary { get; private set; }

        public WardMenuTerritorySection(
            string name,
            bool runtimeActive,
            bool guildAccessEnabled,
            bool groupAccessEnabled,
            bool doorLockEnabled,
            bool structureDamageProtectionEnabled,
            int doorAutoCloseSeconds,
            string rulesSummary)
        {
            Name = name;
            RuntimeActive = runtimeActive;
            GuildAccessEnabled = guildAccessEnabled;
            GroupAccessEnabled = groupAccessEnabled;
            DoorLockEnabled = doorLockEnabled;
            StructureDamageProtectionEnabled = structureDamageProtectionEnabled;
            DoorAutoCloseSeconds = doorAutoCloseSeconds;
            RulesSummary = rulesSummary;
        }
    }

    internal sealed class WardMenuTerraformingSection
    {
        public bool Enabled { get; private set; }

        public bool Running { get; private set; }

        public string Mode { get; private set; }

        public float Radius { get; private set; }

        public float TargetHeight { get; private set; }

        public float FuelStored { get; private set; }

        public float StoneStored { get; private set; }

        public bool HoeStored { get; private set; }

        public bool PickaxeStored { get; private set; }

        public float ScanProgress { get; private set; }

        public int ScanIndex { get; private set; }

        public string Status { get; private set; }

        public WardMenuTerraformingSection(
            bool enabled,
            bool running,
            string mode,
            float radius,
            float targetHeight,
            float fuelStored,
            float stoneStored,
            bool hoeStored,
            bool pickaxeStored,
            float scanProgress,
            int scanIndex,
            string status)
        {
            Enabled = enabled;
            Running = running;
            Mode = mode;
            Radius = radius;
            TargetHeight = targetHeight;
            FuelStored = fuelStored;
            StoneStored = stoneStored;
            HoeStored = hoeStored;
            PickaxeStored = pickaxeStored;
            ScanProgress = scanProgress;
            ScanIndex = scanIndex;
            Status = status;
        }

        public static WardMenuTerraformingSection Disabled()
        {
            return new WardMenuTerraformingSection(
                false,
                false,
                "Level",
                12f,
                0f,
                0f,
                0f,
                false,
                false,
                0f,
                0,
                "Terraforming service unavailable");
        }
    }

    internal sealed class WardMenuPlayerModel
    {
        public long PlayerId { get; private set; }

        public string PlayerName { get; private set; }

        public WardMenuPlayerModel(long playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
    }
}
