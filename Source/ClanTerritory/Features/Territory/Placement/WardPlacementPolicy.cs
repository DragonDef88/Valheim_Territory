using System.Collections.Generic;
using UnityEngine;
using ClanTerritory.Features.Territory.Placement.Rules;

namespace ClanTerritory.Features.Territory.Placement
{
    internal sealed class WardPlacementPolicy : IWardPlacementPolicy
    {
        private readonly IReadOnlyList<IPlacementRule> _rules;

        public WardPlacementPolicy(IEnumerable<IPlacementRule> rules)
        {
            _rules = new List<IPlacementRule>(rules);
        }

        public PlacementValidationResult Validate(
    Player player,
    Vector3 position)
        {
            foreach (IPlacementRule rule in _rules)
            {
                PlacementValidationResult result =
                    rule.Validate(player, position);

                if (!result.IsSuccess)
                    return result;
            }

            return PlacementValidationResult.Success;

            }
    }
}