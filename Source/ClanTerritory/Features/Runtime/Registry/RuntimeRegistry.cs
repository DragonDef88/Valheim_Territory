using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.Runtime.Registry
{
    internal sealed class RuntimeRegistry : IRuntimeRegistry
    {
        private readonly Dictionary<WardId, RuntimeWard> _wards =
            new Dictionary<WardId, RuntimeWard>();

        public bool TryAdd(RuntimeWard ward)
        {
            if (ward == null)
                return false;

            if (_wards.ContainsKey(ward.WardId))
                return false;

            _wards.Add(ward.WardId, ward);
            return true;
        }

        public bool Remove(WardId wardId)
        {
            return _wards.Remove(wardId);
        }

        public bool TryGet(WardId wardId, out RuntimeWard ward)
        {
            return _wards.TryGetValue(wardId, out ward);
        }

        public IReadOnlyCollection<RuntimeWard> GetAll()
        {
            return _wards.Values;
        }

        public void Clear()
        {
            _wards.Clear();
        }
    }
}