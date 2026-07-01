using System.Collections.Generic;
using ClanTerritory.Abstractions;
using ClanTerritory.Features.WardDetection.Models;

namespace ClanTerritory.Features.WardDetection.Registry
{
    internal sealed class WardRegistry : IRegistry<WardModel>
    {
        private readonly Dictionary<string, WardModel> _wards = new Dictionary<string, WardModel>();

        public IReadOnlyCollection<WardModel> All
        {
            get { return _wards.Values; }
        }

        public bool Register(WardModel entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id))
                return false;

            if (_wards.ContainsKey(entity.Id))
                return false;

            _wards.Add(entity.Id, entity);
            return true;
        }

        public bool Unregister(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return _wards.Remove(id);
        }

        public bool TryGet(string id, out WardModel entity)
        {
            if (string.IsNullOrEmpty(id))
            {
                entity = null;
                return false;
            }

            return _wards.TryGetValue(id, out entity);
        }

        public void Clear()
        {
            _wards.Clear();
        }
    }
}