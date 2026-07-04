namespace ClanTerritory.Features.Territory.Placement
{
    internal enum PlacementResult
    {
        Success,

        TerritoryOverlap,

        MaxWardLimit,

        InvalidPlayer,

        InvalidPosition,

        UnknownError
    }
}