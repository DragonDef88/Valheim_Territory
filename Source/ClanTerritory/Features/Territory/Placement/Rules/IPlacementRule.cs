using UnityEngine;

namespace ClanTerritory.Features.Territory.Placement.Rules
{
    internal interface IPlacementRule
    {
        PlacementValidationResult Validate(
    Player player,
    Vector3 position);
    }
}