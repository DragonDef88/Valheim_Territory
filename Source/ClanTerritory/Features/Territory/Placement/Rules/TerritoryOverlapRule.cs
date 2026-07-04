using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Domain.ValueObjects;
using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.WardDetection.Models;
using UnityEngine;

namespace ClanTerritory.Features.Territory.Placement.Rules
{
    internal sealed class TerritoryOverlapRule : IPlacementRule
    {
        private readonly TerritoryRegistry _registry;
        private readonly TerritoryFactory _factory;

        public TerritoryOverlapRule(
            TerritoryRegistry registry,
            TerritoryFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public PlacementValidationResult Validate(Player player, Vector3 position)
        {
            if (player == null)
            {
                return PlacementValidationResult.Failure(
                    PlacementResult.InvalidPlayer,
                    "Cannot place Ward: invalid player.");
            }

            WardModel candidateWard = new WardModel(
                "preview",
                player.GetPlayerID(),
                player.GetPlayerName(),
                position,
                true);

            Domain.Entities.Territory candidate =
                _factory.CreateFromWard(candidateWard);

            if (_registry.HasOverlap(candidate))
            {
                return PlacementValidationResult.Failure(
                    PlacementResult.TerritoryOverlap,
                    "Cannot place Ward: territory overlaps an existing territory.");
            }

            return PlacementValidationResult.Success;
        }
    }
}