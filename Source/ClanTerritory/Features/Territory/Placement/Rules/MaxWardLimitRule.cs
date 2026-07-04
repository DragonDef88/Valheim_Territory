using ClanTerritory.Config;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Territory.Registry;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Placement.Rules
{
    internal sealed class MaxWardLimitRule : IPlacementRule
    {
        private readonly TerritoryRegistry _registry;

        public MaxWardLimitRule(TerritoryRegistry registry)
        {
            _registry = registry;
        }

        public PlacementValidationResult Validate(Player player, Vector3 position)
        {
            if (player == null)
            {
                return PlacementValidationResult.Failure(
                    PlacementResult.InvalidPlayer,
                    "Cannot place Ward: invalid player.");
            }

            PlayerId playerId = new PlayerId(player.GetPlayerID());

            if (_registry.CountByOwner(playerId) >= ConfigValues.MaxWardsPerPlayer)
            {
                return PlacementValidationResult.Failure(
                    PlacementResult.MaxWardLimit,
                    "Cannot place Ward: maximum number of territories reached.");
            }

            return PlacementValidationResult.Success;
        }
    }
}