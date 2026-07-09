using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Map.Services;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Core;
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

            TerritoryGuildAccess.SyncWardGuildFromPlayer(
                privateArea,
                player,
                true);

            UpdateWardMapIcon(
                wardId,
                runtimeWard,
                zdo);

            ModLog.Info("Ward menu requested: " + wardId);

            _eventBus.Publish(new WardInteractionRequestedEvent(
                wardId,
                runtimeWard,
                privateArea,
                player));

            return true;
        }

        private static void UpdateWardMapIcon(
            WardId wardId,
            RuntimeWard runtimeWard,
            ZDO zdo)
        {
            if (runtimeWard == null || zdo == null)
                return;

            WardMapIconService mapIconService;

            if (!ServiceContainer.TryGet<WardMapIconService>(out mapIconService))
                return;

            mapIconService.AddOrUpdate(
                new WardModel(
                    wardId.ToString(),
                    zdo.GetLong(ZDOVars.s_creator, 0L),
                    zdo.GetString(ZDOVars.s_creatorName, "Unknown"),
                    runtimeWard.Position,
                    0f,
                    zdo.GetBool(ZDOVars.s_enabled)));
        }
    }
}
