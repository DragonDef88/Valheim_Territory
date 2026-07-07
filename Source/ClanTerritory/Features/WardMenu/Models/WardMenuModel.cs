using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardMenu.Models
{
    internal sealed class WardMenuModel
    {
        private readonly List<WardMenuPlayerModel> _permittedPlayers;

        public WardId WardId { get; private set; }

        public string OwnerName { get; private set; }

        public float Radius { get; private set; }

        public bool Enabled { get; private set; }

        public bool RuntimeActive { get; private set; }

        public IReadOnlyList<WardMenuPlayerModel> PermittedPlayers
        {
            get { return _permittedPlayers; }
        }

        public WardMenuModel(
            WardId wardId,
            string ownerName,
            float radius,
            bool enabled,
            bool runtimeActive,
            List<WardMenuPlayerModel> permittedPlayers)
        {
            WardId = wardId;
            OwnerName = ownerName;
            Radius = radius;
            Enabled = enabled;
            RuntimeActive = runtimeActive;
            _permittedPlayers = permittedPlayers ?? new List<WardMenuPlayerModel>();
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