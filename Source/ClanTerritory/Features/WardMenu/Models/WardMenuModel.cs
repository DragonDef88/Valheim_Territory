using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Models
{
    internal sealed class WardMenuModel
    {
        public WardMenuWardSection Ward { get; private set; }

        public WardMenuTerritorySection Territory { get; private set; }

        public WardMenuTerraformingSection Terraforming { get; private set; }

        public WardMenuBiomeDominionSection BiomeDominion { get; private set; }

        public WardMenuEconomySection Economy { get; private set; }

        public WardMenuDiplomacySection Diplomacy { get; private set; }

        public WardMenuModel(
            WardMenuWardSection ward,
            WardMenuTerritorySection territory,
            WardMenuTerraformingSection terraforming,
            WardMenuBiomeDominionSection biomeDominion,
            WardMenuEconomySection economy,
            WardMenuDiplomacySection diplomacy)
        {
            Ward = ward;
            Territory = territory;
            Terraforming = terraforming;
            BiomeDominion = biomeDominion;
            Economy = economy;
            Diplomacy = diplomacy;
        }
    }


    internal sealed class WardMenuDiplomacySection
    {
        public bool Available { get; private set; }
        public string StatusText { get; private set; }
        public string GuildName { get; private set; }
        public string RelationsText { get; private set; }
        public bool CanChange { get; private set; }

        public WardMenuDiplomacySection(
            bool available,
            string statusText,
            string guildName,
            string relationsText,
            bool canChange)
        {
            Available = available;
            StatusText = statusText ?? "";
            GuildName = guildName ?? "";
            RelationsText = relationsText ?? "";
            CanChange = canChange;
        }

        public static WardMenuDiplomacySection Unavailable()
        {
            return new WardMenuDiplomacySection(
                false,
                "",
                "",
                "",
                false);
        }
    }

    internal sealed class WardMenuEconomySection
    {
        public bool Available { get; private set; }
        public string StatusText { get; private set; }
        public string GuildName { get; private set; }
        public string TerritoryGuildName { get; private set; }
        public long Balance { get; private set; }
        public long DepositedTotal { get; private set; }
        public long WithdrawnTotal { get; private set; }
        public long UpkeepPaidTotal { get; private set; }
        public long TributeReceivedTotal { get; private set; }
        public long TaxPaidTotal { get; private set; }
        public long TaxReceivedTotal { get; private set; }
        public long TransferSentTotal { get; private set; }
        public long TransferReceivedTotal { get; private set; }
        public bool CanDeposit { get; private set; }
        public bool CanWithdraw { get; private set; }
        public bool CanPayUpkeep { get; private set; }
        public bool CanPayTax { get; private set; }
        public bool CanTransfer { get; private set; }
        public int DefaultAmount { get; private set; }

        public WardMenuEconomySection(
            bool available,
            string statusText,
            string guildName,
            string territoryGuildName,
            long balance,
            long depositedTotal,
            long withdrawnTotal,
            long upkeepPaidTotal,
            long tributeReceivedTotal,
            long taxPaidTotal,
            long taxReceivedTotal,
            long transferSentTotal,
            long transferReceivedTotal,
            bool canDeposit,
            bool canWithdraw,
            bool canPayUpkeep,
            bool canPayTax,
            bool canTransfer,
            int defaultAmount)
        {
            Available = available;
            StatusText = statusText ?? "";
            GuildName = guildName ?? "";
            TerritoryGuildName = territoryGuildName ?? "";
            Balance = balance;
            DepositedTotal = depositedTotal;
            WithdrawnTotal = withdrawnTotal;
            UpkeepPaidTotal = upkeepPaidTotal;
            TributeReceivedTotal = tributeReceivedTotal;
            TaxPaidTotal = taxPaidTotal;
            TaxReceivedTotal = taxReceivedTotal;
            TransferSentTotal = transferSentTotal;
            TransferReceivedTotal = transferReceivedTotal;
            CanDeposit = canDeposit;
            CanWithdraw = canWithdraw;
            CanPayUpkeep = canPayUpkeep;
            CanPayTax = canPayTax;
            CanTransfer = canTransfer;
            DefaultAmount = defaultAmount;
        }

        public static WardMenuEconomySection Unavailable()
        {
            return new WardMenuEconomySection(
                false,
                "",
                "",
                "",
                0L,
                0L,
                0L,
                0L,
                0L,
                0L,
                0L,
                0L,
                0L,
                false,
                false,
                false,
                false,
                false,
                10);
        }
    }

    internal sealed class WardMenuWardSection
    {
        private readonly List<WardMenuPlayerModel> _permittedPlayers;

        public WardId WardId { get; private set; }

        public string OwnerName { get; private set; }

        public string CreatorGuildName { get; private set; }

        public string CreatorGuildDescription { get; private set; }

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
            string creatorGuildName,
            string creatorGuildDescription,
            float radius,
            bool enabled,
            bool isCurrentPlayerCreator,
            bool isCurrentPlayerPermitted,
            List<WardMenuPlayerModel> permittedPlayers)
        {
            WardId = wardId;
            OwnerName = ownerName;
            CreatorGuildName = creatorGuildName ?? "";
            CreatorGuildDescription = creatorGuildDescription ?? "";
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

    internal sealed class WardMenuBiomeDominionSection
    {
        public string BiomeName { get; private set; }

        public bool Claimed { get; private set; }

        public string OwnerGuildName { get; private set; }

        public bool Vassal { get; private set; }

        public bool CanClaim { get; private set; }

        public bool CanManage { get; private set; }

        public bool DoorLockEnabled { get; private set; }

        public bool StructureDamageProtectionEnabled { get; private set; }

        public int DoorAutoCloseSeconds { get; private set; }

        public WardMenuBiomeDominionSection(
            string biomeName,
            bool claimed,
            string ownerGuildName,
            bool vassal,
            bool canClaim,
            bool canManage,
            bool doorLockEnabled,
            bool structureDamageProtectionEnabled,
            int doorAutoCloseSeconds)
        {
            BiomeName = biomeName ?? "";
            Claimed = claimed;
            OwnerGuildName = ownerGuildName ?? "";
            Vassal = vassal;
            CanClaim = canClaim;
            CanManage = canManage;
            DoorLockEnabled = doorLockEnabled;
            StructureDamageProtectionEnabled = structureDamageProtectionEnabled;
            DoorAutoCloseSeconds = doorAutoCloseSeconds;
        }

        public static WardMenuBiomeDominionSection Unavailable()
        {
            return new WardMenuBiomeDominionSection(
                "",
                false,
                "",
                false,
                false,
                false,
                false,
                false,
                5);
        }
    }

    internal sealed class WardMenuTerraformingSection
    {
        public bool Enabled { get; private set; }

        public bool Running { get; private set; }

        public float Radius { get; private set; }

        public float TargetHeight { get; private set; }

        public int FuelStored { get; private set; }

        public int StoneStored { get; private set; }

        public int[] FuelSlots { get; private set; }

        public int[] StoneSlots { get; private set; }

        public bool HoeStored { get; private set; }

        public bool PickaxeStored { get; private set; }

        public bool AxeStored { get; private set; }

        public float ScanProgress { get; private set; }

        public int ScanIndex { get; private set; }

        public string Status { get; private set; }

        public WardMenuTerraformingSection(
            bool enabled,
            bool running,
            float radius,
            float targetHeight,
            int fuelStored,
            int stoneStored,
            int[] fuelSlots,
            int[] stoneSlots,
            bool hoeStored,
            bool pickaxeStored,
            bool axeStored,
            float scanProgress,
            int scanIndex,
            string status)
        {
            Enabled = enabled;
            Running = running;
            Radius = radius;
            TargetHeight = targetHeight;
            FuelStored = fuelStored;
            StoneStored = stoneStored;
            FuelSlots = fuelSlots ?? new int[5];
            StoneSlots = stoneSlots ?? new int[5];
            HoeStored = hoeStored;
            PickaxeStored = pickaxeStored;
            AxeStored = axeStored;
            ScanProgress = scanProgress;
            ScanIndex = scanIndex;
            Status = status;
        }

        public static WardMenuTerraformingSection Disabled()
        {
            return new WardMenuTerraformingSection(
                false,
                false,
                12f,
                0f,
                0,
                0,
                new int[5],
                new int[5],
                false,
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
