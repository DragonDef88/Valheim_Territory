using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Map.Services;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Integration.Guilds;
using ClanTerritory.Core;
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

            TerritoryGuildAccess.SyncWardGuildFromPlayer(
                privateArea,
                player,
                true);

            UpdateWardMapIcon(
                wardId,
                runtimeWard,
                zdo);

            ModLog.Info("[TerritoryInteraction] Territory menu requested: " + wardId);

            _eventBus.Publish(new TerritoryInteractionRequestedEvent(
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
