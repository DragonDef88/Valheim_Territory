using System.Collections.Generic;

namespace ClanTerritory.Abstractions
{
    public interface IRegistry<T> where T : IEntity
    {
        IReadOnlyCollection<T> All { get; }

        bool Register(T entity);

        bool Unregister(string id);

        bool TryGet(string id, out T entity);

        void Clear();
    }
}