using ClanTerritory.Domain.Identifiers;
using UnityEngine;

namespace ClanTerritory.Features.Runtime.Registry
{
    internal sealed class RuntimeWard
    {
        public WardId WardId { get; }

        public Vector3 Position { get; private set; }

        public bool IsLoaded { get; private set; }

        public bool IsActive { get; private set; }

        public RuntimeWard(WardId wardId, Vector3 position)
        {
            WardId = wardId;
            Position = position;
            IsLoaded = true;
            IsActive = true;
        }

        public void Activate(Vector3 position)
        {
            Position = position;
            IsLoaded = true;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsLoaded = false;
            IsActive = false;
        }
    }
}