using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Features.Persistence.Mappers;
using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Features.Persistence.Storage;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Persistence
{
    internal sealed class PersistenceModule : IInitializable, IDisposableModule
    {
        private JsonStorage _storage;
        private TerritoryMapper _territoryMapper;
        private PersistenceService _service;

        public void Initialize()
        {
            _storage = new JsonStorage();
            _territoryMapper = new TerritoryMapper();
            _service = new PersistenceService(_storage, _territoryMapper);

            ServiceContainer.Register<JsonStorage>(_storage);
            ServiceContainer.Register<TerritoryMapper>(_territoryMapper);
            ServiceContainer.Register<IPersistenceService>(_service);

            ModLog.Info("Persistence module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Persistence module shutdown.");
        }
    }
}