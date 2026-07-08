using UnityEngine;
using ClanTerritory.Abstractions;

namespace ClanTerritory.Features.WardDetection.Models
{
    internal sealed class WardModel : IEntity
    {
        public string Id { get; private set; }
        public long OwnerId { get; private set; }
        public string OwnerName { get; private set; }
        public Vector3 Position { get; private set; }
        public float Radius { get; private set; }
        public bool Enabled { get; private set; }

        public WardModel(
            string id,
            long ownerId,
            string ownerName,
            Vector3 position,
            float radius,
            bool enabled)
        {
            Id = id;
            OwnerId = ownerId;
            OwnerName = ownerName;
            Position = position;
            Radius = radius;
            Enabled = enabled;
        }
    }
}
