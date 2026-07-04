using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
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

            ModLog.Info(
                "Ward registered: " +
                ward.Id +
                ", owner: " +
                ward.OwnerName);

            EventBus eventBus;

            if (ServiceContainer.TryGet<EventBus>(out eventBus))
                eventBus.Publish(new WardRegisteredEvent(ward));
        }

        public void UnregisterWard(WardId wardId)
        {
            if (_registry.Unregister(wardId.ToString()))
            {
                EventBus eventBus;

                if (ServiceContainer.TryGet<EventBus>(out eventBus))
                    eventBus.Publish(new WardDestroyedEvent(wardId));
            }
        }
    }
}