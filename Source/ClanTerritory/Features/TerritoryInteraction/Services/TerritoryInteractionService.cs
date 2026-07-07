using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.TerritoryInteraction.Services
{
    internal sealed class TerritoryInteractionService : ITerritoryInteractionService
    {
        private readonly IRuntimeRegistry _runtimeRegistry;
        private readonly EventBus _eventBus;

        public TerritoryInteractionService(
            IRuntimeRegistry runtimeRegistry,
            EventBus eventBus)
        {
            _runtimeRegistry = runtimeRegistry;
            _eventBus = eventBus;
        }

        public bool TryOpenTerritoryMenu(PrivateArea privateArea, Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryInteraction] Ignored. PrivateArea is null.");
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[TerritoryInteraction] Ignored. Player is null.");
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryInteraction] Ignored. ZNetView is invalid.");
                return false;
            }

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[TerritoryInteraction] Ignored. ZDO is null.");
                return false;
            }

            WardId wardId = new WardId(zdo.m_uid.ToString());

            RuntimeWard runtimeWard;

            if (!_runtimeRegistry.TryGet(wardId, out runtimeWard))
            {
                ModLog.Debug("[TerritoryInteraction] Ignored. Territory ward is not registered in RuntimeRegistry: " + wardId);
                return false;
            }

            ModLog.Info("[TerritoryInteraction] Territory menu requested: " + wardId);

            _eventBus.Publish(new TerritoryInteractionRequestedEvent(
                wardId,
                runtimeWard,
                privateArea,
                player));

            return true;
        }
    }
}