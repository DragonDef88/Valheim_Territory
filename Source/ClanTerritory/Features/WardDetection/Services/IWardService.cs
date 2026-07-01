using UnityEngine;

namespace ClanTerritory.Features.WardDetection.Services
{
    internal interface IWardService
    {
        void RegisterWardAfterPlacement(Player player, Vector3 position);
    }
}