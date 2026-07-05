using ClanTerritory.Domain.Identifiers;
using UnityEngine;

namespace ClanTerritory.Features.Runtime.Restore
{
    internal sealed class RuntimeWardRestoreRecord
    {
        public WardId WardId { get; private set; }

        public Vector3 Position { get; private set; }

        public RuntimeWardRestoreRecord(
            WardId wardId,
            Vector3 position)
        {
            WardId = wardId;
            Position = position;
        }
    }
}