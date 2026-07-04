using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Domain.ValueObjects;
using UnityEngine;
using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;

namespace ClanTerritory.Features.Territory.Registry
{
    internal sealed class TerritoryRegistry
    {
        private readonly Dictionary<string, TerritoryEntity> _territories =
            new Dictionary<string, TerritoryEntity>();

        public int Count
        {
            get { return _territories.Count; }
        }

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
        public int CountByOwner(PlayerId ownerId)
        {
            int count = 0;

            foreach (TerritoryEntity territory in _territories.Values)
            {
                if (territory.Owner.PlayerId.Equals(ownerId))
                    count++;
            }

            return count;
        }
        public bool Unregister(TerritoryId id)
        {
            return Unregister(id.ToString());
        }

        public bool Unregister(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return _territories.Remove(id);
        }

        public bool RemoveByWard(WardId wardId)
        {
            TerritoryEntity territory = FindByWard(wardId);

            if (territory == null)
                return false;

            return Unregister(territory.Id);
        }

        public bool TryGet(TerritoryId id, out TerritoryEntity territory)
        {
            return TryGet(id.ToString(), out territory);
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

        public TerritoryEntity FindByWard(WardId wardId)
        {
            foreach (TerritoryEntity territory in _territories.Values)
            {
                if (territory.WardId.Equals(wardId))
                    return territory;
            }

            return null;
        }

        public TerritoryEntity FindContaining(WorldPosition position)
        {
            foreach (TerritoryEntity territory in _territories.Values)
            {
                if (territory.Contains(position))
                    return territory;
            }

            return null;
        }

        public TerritoryEntity FindContaining(Vector3 position)
        {
            return FindContaining(new WorldPosition(position.x, position.y, position.z));
        }

        public TerritoryEntity FindNearest(WorldPosition position)
        {
            TerritoryEntity nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (TerritoryEntity territory in _territories.Values)
            {
                float distance = territory.Position.DistanceTo(position);

                if (distance < nearestDistance)
                {
                    nearest = territory;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public TerritoryEntity FindNearest(Vector3 position)
        {
            return FindNearest(new WorldPosition(position.x, position.y, position.z));
        }

        public TerritoryEntity FindIntersecting(TerritoryEntity territory)
        {
            if (territory == null)
                return null;

            foreach (TerritoryEntity existing in _territories.Values)
            {
                if (existing.Id.Equals(territory.Id))
                    continue;

                if (existing.Overlaps(territory))
                    return existing;
            }

            return null;
        }

        public bool HasOverlap(TerritoryEntity territory)
        {
            return FindIntersecting(territory) != null;
        }

        public List<TerritoryEntity> GetAll()
        {
            return new List<TerritoryEntity>(_territories.Values);
        }

        public void Clear()
        {
            _territories.Clear();
        }
    }
}