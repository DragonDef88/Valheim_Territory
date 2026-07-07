using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Features.Runtime;
using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Utils;

using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryService :
        ITerritoryService,
        IEventHandler<WardRegisteredEvent>,
        IEventHandler<WardDestroyedEvent>
    {
        private readonly TerritoryRegistry _registry;
        private readonly TerritoryFactory _factory;

        public TerritoryService(TerritoryRegistry registry, TerritoryFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public void Handle(WardRegisteredEvent eventData)
        {
            if (eventData == null)
                return;

            if (!IsRuntimeGameplayReady())
            {
                ModLog.Info(
                    "Territory creation skipped: runtime is not gameplay-ready.");

                return;
            }

            CreateTerritoryFromWard(eventData.Ward);
        }

        public void Handle(WardDestroyedEvent eventData)
        {
            if (eventData == null)
                return;

            if (!IsRuntimeGameplayReady())
            {
                ModLog.Info(
                    "Territory removal skipped: runtime is not gameplay-ready.");

                return;
            }

            RemoveTerritoryFromWard(eventData.WardId);
        }

        public void CreateTerritoryFromWard(WardModel ward)
        {
            if (ward == null)
                return;

            TerritoryEntity territory = _factory.CreateFromWard(ward);

            if (_registry.Register(territory))
            {
                ModLog.Info(
                    "Territory cached from ward: " +
                    territory.Id +
                    ", owner: " +
                    territory.Owner.DisplayName +
                    ", radius: " +
                    territory.Radius.Value +
                    ", total: " +
                    _registry.Count);

                SaveNow();

                return;
            }

            ModLog.Info(
                "Territory cache already contains ward: " +
                ward.Id);
        }

        private void RemoveTerritoryFromWard(ClanTerritory.Domain.Identifiers.WardId wardId)
        {
            MarkWardDeleted(wardId.ToString());

            if (_registry.RemoveByWard(wardId))
            {
                ModLog.Info(
                    "Territory removed for destroyed ward: " +
                    wardId +
                    ", total: " +
                    _registry.Count);
            }
            else
            {
                ModLog.Warning(
                    "Ward destroyed, but territory was not found in runtime cache: " +
                    wardId);
            }

            SaveNow();
        }

        private static bool IsRuntimeGameplayReady()
        {
            RuntimeStateMachine stateMachine;

            if (!ServiceContainer.TryGet<RuntimeStateMachine>(out stateMachine))
                return false;

            return stateMachine.State == RuntimeState.GameplayReady;
        }

        private static void MarkWardDeleted(string wardId)
        {
            IPersistenceService persistenceService;

            if (ServiceContainer.TryGet<IPersistenceService>(out persistenceService))
                persistenceService.MarkWardDeleted(wardId);
        }

        private void SaveNow()
        {
            IPersistenceService persistenceService;

            if (ServiceContainer.TryGet<IPersistenceService>(out persistenceService))
                persistenceService.SaveNow();
        }
    }
}