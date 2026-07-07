using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardInteraction.Services
{
    internal sealed class WardInteractionService : IWardInteractionService
    {
        private readonly IRuntimeRegistry _runtimeRegistry;
        private readonly EventBus _eventBus;

        public WardInteractionService(
            IRuntimeRegistry runtimeRegistry,
            EventBus eventBus)
        {
            _runtimeRegistry = runtimeRegistry;
            _eventBus = eventBus;
        }

        public bool TryOpenWardMenu(PrivateArea privateArea, Player player)
        {
            if (privateArea == null)
                return false;

            if (player == null)
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return false;

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return false;

            WardId wardId = new WardId(zdo.m_uid.ToString());

            RuntimeWard runtimeWard;

            if (!_runtimeRegistry.TryGet(wardId, out runtimeWard))
            {
                ModLog.Debug("Ward interaction ignored. Ward is not registered in RuntimeRegistry: " + wardId);
                return false;
            }

            ModLog.Info("Ward menu requested: " + wardId);

            _eventBus.Publish(new WardInteractionRequestedEvent(
                wardId,
                runtimeWard,
                privateArea,
                player));

            return true;
        }
    }
}