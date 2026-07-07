using ClanTerritory.Features.Territory.Placement.Rules;
using ClanTerritory.Features.Territory.Zdo;

namespace ClanTerritory.Features.Territory.Placement
{
    internal static class WardPlacementPolicyFactory
    {
        public static IWardPlacementPolicy Create(
            TerritoryZdoService zdoService)
        {
            IPlacementRule[] rules =
            {
                new TerritoryOverlapRule(zdoService),
                new MaxWardLimitRule(zdoService)
            };

            return new WardPlacementPolicy(rules);
        }
    }
}