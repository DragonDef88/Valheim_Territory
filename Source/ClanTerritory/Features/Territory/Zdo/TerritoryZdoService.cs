using System.Collections.Generic;
using ClanTerritory.Features.WardDetection.Models;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Zdo
{
    internal sealed class TerritoryZdoService
    {
        private const string WardPieceName = "guard_stone";

        private static readonly int WardPrefabHash =
    WardPieceName.GetStableHashCode();

        public List<WardModel> GetAllWards()
        {
            List<WardModel> wards = new List<WardModel>();

            Dictionary<ZDOID, ZDO> objects = GetObjectsById();

            if (objects == null)
                return wards;

            foreach (ZDO zdo in objects.Values)
            {
                WardModel ward;

                if (TryCreateWardModel(zdo, out ward))
                    wards.Add(ward);
            }

            return wards;
        }

        public int CountByOwner(long ownerId)
        {
            int count = 0;

            foreach (WardModel ward in GetAllWards())
            {
                if (ward.OwnerId == ownerId)
                    count++;
            }

            return count;
        }

        public bool HasOverlap(Vector3 position, float radius)
        {
            foreach (WardModel ward in GetAllWards())
            {
                float distance =
                    global::Utils.DistanceXZ(position, ward.Position);

                if (distance < radius + radius)
                    return true;
            }

            return false;
        }

        public bool TryCreateWardModel(
            ZDO zdo,
            out WardModel ward)
        {
            ward = null;

            if (zdo == null)
                return false;

            if (zdo.GetPrefab() != WardPrefabHash)
                return false;

            long ownerId = zdo.GetLong(ZDOVars.s_creator, 0L);

            if (ownerId == 0L)
                return false;

            string ownerName =
                zdo.GetString(ZDOVars.s_creatorName, "Unknown");

            ward = new WardModel(
                zdo.m_uid.ToString(),
                ownerId,
                string.IsNullOrWhiteSpace(ownerName) ? "Unknown" : ownerName,
                zdo.GetPosition(),
                zdo.GetBool(ZDOVars.s_enabled, false));

            return true;
        }

        private static Dictionary<ZDOID, ZDO> GetObjectsById()
        {
            if (ZDOMan.instance == null)
                return null;

            return AccessTools
                .Field(typeof(ZDOMan), "m_objectsByID")
                .GetValue(ZDOMan.instance) as Dictionary<ZDOID, ZDO>;
        }
    }
}