using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.Runtime.Restore;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.Territory.Zdo;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class RuntimeRestoreStep : IRuntimeStep
    {
        private readonly TerritoryZdoService _zdoService;
        private readonly IRuntimeRegistryRestoreService _registryRestoreService;
        private readonly ITerritoryService _territoryService;
        private readonly TerritoryRegistry _territoryRegistry;
        private readonly PersistenceWriteGate _writeGate;

        public RuntimeRestoreStep(
            TerritoryZdoService zdoService,
            IRuntimeRegistryRestoreService registryRestoreService,
            ITerritoryService territoryService,
            TerritoryRegistry territoryRegistry,
            PersistenceWriteGate writeGate)
        {
            _zdoService = zdoService;
            _registryRestoreService = registryRestoreService;
            _territoryService = territoryService;
            _territoryRegistry = territoryRegistry;
            _writeGate = writeGate;
        }

        public RuntimeState InputState
        {
            get { return RuntimeState.DiscoveryCompleted; }
        }

        public RuntimeState OutputState
        {
            get { return RuntimeState.GameplayReady; }
        }

        public void Execute()
        {
            ModLog.Info("[Runtime Pipeline] Restoring territories from Valheim ZDO.");

            List<WardModel> wards = _zdoService.GetAllWards();
            List<RuntimeWardRestoreRecord> records =
                new List<RuntimeWardRestoreRecord>();

            foreach (WardModel ward in wards)
            {
                records.Add(
                    new RuntimeWardRestoreRecord(
                        new WardId(ward.Id),
                        ward.Position));
            }

            RuntimeRestoreSnapshot snapshot =
                new RuntimeRestoreSnapshot(records);

            _registryRestoreService.Restore(snapshot);

            if (_territoryRegistry != null)
                _territoryRegistry.Clear();

            foreach (WardModel ward in wards)
                _territoryService.CreateTerritoryFromWard(ward);

            _writeGate.Open();
            ModLog.Info("[Persistence] Write gate opened after runtime restore.");

            ModLog.Info(
                "[Runtime Pipeline] ZDO runtime restore completed. Wards: " +
                wards.Count);
        }
    }
}