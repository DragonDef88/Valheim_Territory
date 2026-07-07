namespace ClanTerritory.Features.TerritoryInteraction.Services
{
    internal interface ITerritoryInteractionService
    {
        bool TryOpenTerritoryMenu(PrivateArea privateArea, Player player);
    }
}