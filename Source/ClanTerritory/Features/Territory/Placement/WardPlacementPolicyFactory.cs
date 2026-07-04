using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Placement.Rules;
using ClanTerritory.Features.Territory.Registry;

namespace ClanTerritory.Features.Territory.Placement
{
    internal static class WardPlacementPolicyFactory
    {
        public static IWardPlacementPolicy Create(
            TerritoryRegistry registry,
            TerritoryFactory factory)
        {
            IPlacementRule[] rules =
            {
                new TerritoryOverlapRule(
                    registry,
                    factory),

                new MaxWardLimitRule(
                    registry)
            };

            return new WardPlacementPolicy(rules);
        }
    }
}