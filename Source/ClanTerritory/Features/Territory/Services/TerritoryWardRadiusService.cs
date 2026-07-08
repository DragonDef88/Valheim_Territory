using ClanTerritory.Config;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Territory.Events;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryWardRadiusService
    {
        private const string SetTerritoryRadiusRpc = "CT_SetTerritoryRadius";

        private readonly EventBus _eventBus;

        public TerritoryWardRadiusService(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void RegisterRpc(PrivateArea privateArea)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRadius] RPC registration ignored. PrivateArea is null.");
                return;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || zNetView.GetZDO() == null)
            {
                ModLog.Debug("[TerritoryRadius] RPC registration ignored. ZNetView or ZDO is null.");
                return;
            }

            zNetView.Register<long, float>(
                SetTerritoryRadiusRpc,
                delegate (long sender, long playerId, float radius)
                {
                    RPC_SetTerritoryRadius(
                        privateArea,
                        zNetView,
                        sender,
                        playerId,
                        radius);
                });

            ModLog.Debug("[TerritoryRadius] RPC registered for ward.");
        }

        public void ApplyStoredOrConfiguredRadius(PrivateArea privateArea)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRadius] Apply ignored. PrivateArea is null.");
                return;
            }

            ZDO zdo = GetZdo(privateArea);

            float radius = zdo != null
                ? zdo.GetFloat(
                    TerritoryZdoKeys.Radius,
                    ConfigValues.TerritoryRadius)
                : ConfigValues.TerritoryRadius;

            ApplyRadius(
                privateArea,
                radius);
        }

        public void RequestSetRadius(
            PrivateArea privateArea,
            Player player,
            float radius)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRadius] Radius request ignored. PrivateArea is null.");
                return;
            }

            if (player == null)
            {
                ModLog.Debug("[TerritoryRadius] Radius request ignored. Player is null.");
                return;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryRadius] Radius request ignored. ZNetView is invalid.");
                return;
            }

            float normalizedRadius = NormalizeRadius(radius);

            zNetView.InvokeRPC(
                SetTerritoryRadiusRpc,
                player.GetPlayerID(),
                normalizedRadius);

            ModLog.Info("[TerritoryRadius] Radius change requested: " + normalizedRadius);
        }

        public void ApplyRadius(
            PrivateArea privateArea,
            float radius)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRadius] Apply ignored. PrivateArea is null.");
                return;
            }

            float normalizedRadius = NormalizeRadius(radius);

            privateArea.m_radius = normalizedRadius;

            if (privateArea.m_areaMarker != null)
                privateArea.m_areaMarker.m_radius = normalizedRadius;

            ModLog.Info("[TerritoryRadius] Territory radius applied to ward: " + normalizedRadius);
        }

        private void RPC_SetTerritoryRadius(
            PrivateArea privateArea,
            ZNetView zNetView,
            long sender,
            long playerId,
            float radius)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRadius] RPC ignored. PrivateArea is null.");
                return;
            }

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryRadius] RPC ignored. ZNetView is invalid.");
                return;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[TerritoryRadius] RPC ignored. ZNetView is not owner.");
                return;
            }

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[TerritoryRadius] RPC ignored. ZDO is null.");
                return;
            }

            float normalizedRadius = NormalizeRadius(radius);

            zdo.Set(
                TerritoryZdoKeys.Radius,
                normalizedRadius);

            ApplyRadius(
                privateArea,
                normalizedRadius);

            WardId wardId = new WardId(zdo.m_uid.ToString());

            if (_eventBus != null)
            {
                _eventBus.Publish(
                    new TerritoryRadiusChangedEvent(
                        wardId,
                        normalizedRadius));
            }

            ModLog.Info(
                "[TerritoryRadius] Territory radius saved: " +
                normalizedRadius +
                ", playerId: " +
                playerId);
        }

        private static ZDO GetZdo(PrivateArea privateArea)
        {
            if (privateArea == null)
                return null;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return null;

            return zNetView.GetZDO();
        }

        private static float NormalizeRadius(float radius)
        {
            if (radius < 50f)
                return 50f;

            if (radius > 200f)
                return 200f;

            return radius;
        }
    }
}