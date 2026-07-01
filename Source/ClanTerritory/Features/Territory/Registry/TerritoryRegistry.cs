using System.Collections.Generic;
using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;

namespace ClanTerritory.Features.Territory.Registry
{
    internal sealed class TerritoryRegistry
    {
        private readonly Dictionary<string, TerritoryEntity> _territories =
            new Dictionary<string, TerritoryEntity>();

        public IReadOnlyCollection<TerritoryEntity> All
        {
            get { return _territories.Values; }
        }

        public bool Register(TerritoryEntity territory)
        {
            if (territory == null)
                return false;

            string id = territory.Id.ToString();

            if (_territories.ContainsKey(id))
                return false;

            _territories.Add(id, territory);
            return true;
        }

        public bool Unregister(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return _territories.Remove(id);
        }

        public bool TryGet(string id, out TerritoryEntity territory)
        {
            if (string.IsNullOrEmpty(id))
            {
                territory = null;
                return false;
            }

            return _territories.TryGetValue(id, out territory);
        }

        public void Clear()
        {
            _territories.Clear();
        }
    }
}