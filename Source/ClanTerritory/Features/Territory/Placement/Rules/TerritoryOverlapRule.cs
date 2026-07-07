using ClanTerritory.Config;
using ClanTerritory.Features.Territory.Zdo;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Placement.Rules
{
    internal sealed class TerritoryOverlapRule : IPlacementRule
    {
        private readonly TerritoryZdoService _zdoService;

        public TerritoryOverlapRule(TerritoryZdoService zdoService)
        {
            _zdoService = zdoService;
        }

        public PlacementValidationResult Validate(Player player, Vector3 position)
        {
            if (player == null)
            {
                return PlacementValidationResult.Failure(
                    PlacementResult.InvalidPlayer,
                    "Cannot place Ward: invalid player.");
            }

            if (ConfigValues.AllowOverlap)
                return PlacementValidationResult.Success;

            if (_zdoService.HasOverlap(position, ConfigValues.TerritoryRadius))
            {
                return PlacementValidationResult.Failure(
                    PlacementResult.TerritoryOverlap,
                    "Cannot place Ward: territory overlaps an existing territory.");
            }

            return PlacementValidationResult.Success;
        }
    }
}