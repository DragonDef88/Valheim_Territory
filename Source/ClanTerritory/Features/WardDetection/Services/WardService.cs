using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime;
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

            bool runtimeAccepted = RegisterRuntimeWard(ward);

            ModLog.Info(
                "Ward registered: " +
                ward.Id +
                ", owner: " +
                ward.OwnerName);

            if (!runtimeAccepted)
                return;

            EventBus eventBus;

            if (ServiceContainer.TryGet(out eventBus))
                eventBus.Publish(new WardRegisteredEvent(ward));
        }

        public void UnregisterWard(WardId wardId)
        {
            _registry.Unregister(wardId.ToString());

            if (!IsGameplayReady())
            {
                ModLog.Info(
                    "[Runtime] Ward destroy ignored before gameplay-ready: " +
                    wardId);

                return;
            }

            UnregisterRuntimeWard(wardId);

            EventBus eventBus;

            if (ServiceContainer.TryGet(out eventBus))
                eventBus.Publish(new WardDestroyedEvent(wardId));
        }

        private static bool RegisterRuntimeWard(WardModel ward)
        {
            IRuntimeRegistry runtimeRegistry;

            if (!ServiceContainer.TryGet(out runtimeRegistry))
                return false;

            WardId wardId = new WardId(ward.Id);

            if (!IsGameplayReady())
            {
                ModLog.Info(
                    "[Runtime] Ward load observed before gameplay-ready: " +
                    ward.Id);

                return false;
            }

            RuntimeWard existingWard;

            if (runtimeRegistry.TryGet(wardId, out existingWard))
            {
                ModLog.Info(
                    "[Runtime] Ward already known, load ignored: " +
                    ward.Id);

                return false;
            }

            RuntimeWard runtimeWard =
                new RuntimeWard(
                    wardId,
                    ward.Position);

            if (runtimeRegistry.TryAdd(runtimeWard))
            {
                ModLog.Info(
                    "[Runtime] New ward accepted: " +
                    ward.Id);

                return true;
            }

            return false;
        }

        private static bool UnregisterRuntimeWard(WardId wardId)
        {
            IRuntimeRegistry runtimeRegistry;

            if (!ServiceContainer.TryGet(out runtimeRegistry))
                return false;

            if (runtimeRegistry.Remove(wardId))
            {
                ModLog.Info(
                    "[Runtime] Ward destroyed: " +
                    wardId);

                return true;
            }

            ModLog.Info(
                "[Runtime] Ward destroy observed, but runtime ward was not found: " +
                wardId);

            return false;
        }

        private static bool IsGameplayReady()
        {
            RuntimeStateMachine stateMachine;

            if (!ServiceContainer.TryGet<RuntimeStateMachine>(out stateMachine))
                return false;

            return stateMachine.State == RuntimeState.GameplayReady;
        }
    }
}