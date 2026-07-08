using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Models
{
    internal sealed class WardMenuModel
    {
        public WardMenuWardSection Ward { get; private set; }

        public WardMenuTerritorySection Territory { get; private set; }

        public WardMenuModel(
            WardMenuWardSection ward,
            WardMenuTerritorySection territory)
        {
            Ward = ward;
            Territory = territory;
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

        public string RulesSummary { get; private set; }

        public WardMenuTerritorySection(
            string name,
            bool runtimeActive,
            bool guildAccessEnabled,
            bool groupAccessEnabled,
            bool doorLockEnabled,
            bool structureDamageProtectionEnabled,
            string rulesSummary)
        {
            Name = name;
            RuntimeActive = runtimeActive;
            GuildAccessEnabled = guildAccessEnabled;
            GroupAccessEnabled = groupAccessEnabled;
            DoorLockEnabled = doorLockEnabled;
            StructureDamageProtectionEnabled = structureDamageProtectionEnabled;
            RulesSummary = rulesSummary;
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
