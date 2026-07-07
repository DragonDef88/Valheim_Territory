using ClanTerritory.Config;
using ClanTerritory.Features.Territory.Zdo;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Placement.Rules
{
    internal sealed class MaxWardLimitRule : IPlacementRule
    {
        private readonly TerritoryZdoService _zdoService;

        public MaxWardLimitRule(TerritoryZdoService zdoService)
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

            if (_zdoService.CountByOwner(player.GetPlayerID()) >=
                ConfigValues.MaxWardsPerPlayer)
            {
                return PlacementValidationResult.Failure(
                    PlacementResult.MaxWardLimit,
                    "Cannot place Ward: maximum number of territories reached.");
            }

            return PlacementValidationResult.Success;
        }
    }
}