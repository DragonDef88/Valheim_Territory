using ClanTerritory.Config;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Territory.Events;
using ClanTerritory.Utils;
using UnityEngine;

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
                radius,
                false);
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
            ApplyRadius(
                privateArea,
                radius,
                true);
        }

        public void ApplyRadius(
            PrivateArea privateArea,
            float radius,
            bool showMarker)
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

            if (showMarker)
                PulseRadiusMarker(privateArea, normalizedRadius);

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
                normalizedRadius,
                true);

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

        private static void PulseRadiusMarker(
            PrivateArea privateArea,
            float radius)
        {
            if (privateArea == null)
                return;

            if (privateArea.m_areaMarker != null)
            {
                privateArea.m_areaMarker.m_radius = radius;
                privateArea.ShowAreaMarker();
            }

            TerritoryRadiusPulseRenderer.Show(
                privateArea,
                radius);

            privateArea.PokeConnectionEffects();

            ModLog.Debug("[TerritoryRadius] Radius visual pulse shown: " + radius);
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

    internal static class TerritoryRadiusPulseRenderer
    {
        private const int SegmentCount = 160;
        private const float PulseDurationSeconds = 3f;
        private const float LineWidth = 0.22f;
        private const float HeightOffset = 0.35f;

        public static void Show(
            PrivateArea privateArea,
            float radius)
        {
            if (privateArea == null)
                return;

            GameObject pulseRoot = new GameObject(
                "ClanTerritory_RadiusPulse_" + GetPulseKey(privateArea));

            pulseRoot.transform.position = privateArea.transform.position;

            LineRenderer lineRenderer = pulseRoot.AddComponent<LineRenderer>();
            Material material = CreatePulseMaterial();

            lineRenderer.material = material;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.positionCount = SegmentCount;
            lineRenderer.widthMultiplier = LineWidth;
            lineRenderer.startColor = new Color(1f, 0.62f, 0.15f, 0.9f);
            lineRenderer.endColor = new Color(1f, 0.62f, 0.15f, 0.9f);

            Vector3 center = privateArea.transform.position;
            center.y += HeightOffset;

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = ((float)i / SegmentCount) * Mathf.PI * 2f;
                Vector3 point = new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y,
                    center.z + Mathf.Sin(angle) * radius);

                lineRenderer.SetPosition(
                    i,
                    point);
            }

            TerritoryRadiusPulseDestroyer destroyer =
                pulseRoot.AddComponent<TerritoryRadiusPulseDestroyer>();

            destroyer.Initialize(material);

            UnityEngine.Object.Destroy(
                pulseRoot,
                PulseDurationSeconds);
        }

        private static Material CreatePulseMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            Material material = new Material(shader);
            material.color = new Color(1f, 0.62f, 0.15f, 0.9f);

            return material;
        }

        private static string GetPulseKey(PrivateArea privateArea)
        {
            if (privateArea == null)
                return "unknown";

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return privateArea.GetInstanceID().ToString();

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
                return privateArea.GetInstanceID().ToString();

            return zdo.m_uid.ToString();
        }

        private sealed class TerritoryRadiusPulseDestroyer : MonoBehaviour
        {
            private Material _material;

            public void Initialize(Material material)
            {
                _material = material;
            }

            private void OnDestroy()
            {
                if (_material != null)
                    UnityEngine.Object.Destroy(_material);

                _material = null;
            }
        }
    }
}
