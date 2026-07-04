using UnityEngine;

namespace ClanTerritory.Features.Territory.Placement
{
    internal interface IWardPlacementPolicy
    {
        PlacementValidationResult Validate(
    Player player,
    Vector3 position);
    }
}