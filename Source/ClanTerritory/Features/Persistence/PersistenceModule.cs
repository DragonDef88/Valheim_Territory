using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Features.Persistence.FileSystem;
using ClanTerritory.Features.Persistence.Mappers;
using ClanTerritory.Features.Persistence.Serialization;
using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Features.Persistence.Storage;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Persistence
{
    internal sealed class PersistenceModule : IInitializable, IDisposableModule
    {
        private PersistenceFileSystem _fileSystem;
        private JsonSerializerService _serializer;
        private JsonStorage _storage;
        private TerritoryMapper _territoryMapper;
        private PersistenceService _service;

        public void Initialize()
        {
            _fileSystem = new PersistenceFileSystem();
            _fileSystem.EnsureDirectories();

            _serializer = new JsonSerializerService();
            _storage = new JsonStorage(_serializer);
            _territoryMapper = new TerritoryMapper();
            _service = new PersistenceService(_storage, _territoryMapper);

            ServiceContainer.Register<PersistenceFileSystem>(_fileSystem);
            ServiceContainer.Register<JsonSerializerService>(_serializer);
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