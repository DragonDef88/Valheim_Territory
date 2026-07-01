using System.Collections;
using UnityEngine;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WardDetection.Registry;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardDetection.Services
{
    internal sealed class WardService : IWardService
    {
        private const string WardPrefabName = "guard_stone";

        private readonly WardRegistry _registry;

        public WardService(WardRegistry registry)
        {
            _registry = registry;
        }

        public void RegisterWardAfterPlacement(Player player, Vector3 position)
        {
            if (Globals.Plugin == null)
                return;

            Globals.Plugin.StartCoroutine(RegisterWardRoutine(player, position));
        }

        private IEnumerator RegisterWardRoutine(Player player, Vector3 position)
        {
            yield return new WaitForSeconds(0.25f);

            PrivateArea ward = FindNearestWard(position);

            if (ward == null)
            {
                ModLog.Warning("Ward placement detected, but real ward object was not found.");
                yield break;
            }

            ZNetView zNetView = ward.GetComponent<ZNetView>();

            if (zNetView == null || zNetView.GetZDO() == null)
            {
                ModLog.Warning("Ward skipped: missing ZNetView or ZDO.");
                yield break;
            }

            string wardId = zNetView.GetZDO().m_uid.ToString();

            WardModel model = new WardModel(
                wardId,
                player != null ? player.GetPlayerID() : 0L,
                player != null ? player.GetPlayerName() : "Unknown",
                ward.transform.position,
                ward.isActiveAndEnabled
            );

            if (_registry.Register(model))
            {
                ModLog.Info("Ward registered: " + model.Id + ", owner: " + model.OwnerName);

                EventBus eventBus;

                if (ServiceContainer.TryGet<EventBus>(out eventBus))
                    eventBus.Publish(new WardRegisteredEvent(model));
            }
        }

        private PrivateArea FindNearestWard(Vector3 position)
        {
            PrivateArea[] areas = Object.FindObjectsByType<PrivateArea>(FindObjectsSortMode.None);

            PrivateArea best = null;
            float bestDistance = 999999f;

            for (int i = 0; i < areas.Length; i++)
            {
                PrivateArea area = areas[i];

                if (area == null)
                    continue;

                if (!area.name.Contains(WardPrefabName))
                    continue;

                float distance = Vector3.Distance(area.transform.position, position);

                if (distance < bestDistance)
                {
                    best = area;
                    bestDistance = distance;
                }
            }

            if (bestDistance > 5f)
                return null;

            return best;
        }
    }
}