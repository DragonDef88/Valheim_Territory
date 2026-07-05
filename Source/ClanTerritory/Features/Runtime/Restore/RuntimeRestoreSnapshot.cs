using System.Collections.Generic;

namespace ClanTerritory.Features.Runtime.Restore
{
    internal sealed class RuntimeRestoreSnapshot
    {
        public IReadOnlyList<RuntimeWardRestoreRecord> Wards { get; private set; }

        public RuntimeRestoreSnapshot(
            IReadOnlyList<RuntimeWardRestoreRecord> wards)
        {
            Wards = wards ?? new List<RuntimeWardRestoreRecord>();
        }
    }
}