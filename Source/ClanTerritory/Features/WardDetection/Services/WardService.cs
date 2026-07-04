using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WardDetection.Registry;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardDetection.Services
{
    internal sealed class WardService : IWardService
    {
        private readonly WardRegistry _registry;

        public WardService(WardRegistry registry)
        {
            _registry = registry;
        }

        public void RegisterWard(WardModel ward)
        {
            if (ward == null)
                return;

            if (!_registry.Register(ward))
                return;

            RegisterRuntimeWard(ward);

            ModLog.Info(
                "Ward registered: " +
                ward.Id +
                ", owner: " +
                ward.OwnerName);

            EventBus eventBus;

            if (ServiceContainer.TryGet(out eventBus))
                eventBus.Publish(new WardRegisteredEvent(ward));
        }

        public void UnregisterWard(WardId wardId)
        {
            if (_registry.Unregister(wardId.ToString()))
            {
                UnregisterRuntimeWard(wardId);

                EventBus eventBus;

                if (ServiceContainer.TryGet(out eventBus))
                    eventBus.Publish(new WardDestroyedEvent(wardId));
            }
        }

        private static void RegisterRuntimeWard(WardModel ward)
        {
            IRuntimeRegistry runtimeRegistry;

            if (!ServiceContainer.TryGet(out runtimeRegistry))
                return;

            WardId wardId = new WardId(ward.Id);

            RuntimeWard runtimeWard =
                new RuntimeWard(
                    wardId,
                    ward.Position);

            if (runtimeRegistry.TryAdd(runtimeWard))
            {
                ModLog.Info(
                    "[Runtime] Ward loaded: " +
                    ward.Id);
            }
        }

        private static void UnregisterRuntimeWard(WardId wardId)
        {
            IRuntimeRegistry runtimeRegistry;

            if (!ServiceContainer.TryGet(out runtimeRegistry))
                return;

            if (runtimeRegistry.Remove(wardId))
            {
                ModLog.Info(
                    "[Runtime] Ward unloaded: " +
                    wardId);
            }
        }
    }
}