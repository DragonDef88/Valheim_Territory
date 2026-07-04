using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.Runtime.Registry
{
    internal interface IRuntimeRegistry
    {
        bool TryAdd(RuntimeWard ward);

        bool Remove(WardId wardId);

        bool TryGet(WardId wardId, out RuntimeWard ward);

        IReadOnlyCollection<RuntimeWard> GetAll();

        void Clear();
    }
}