namespace ClanTerritory.Features.WardInteraction.Services
{
    internal interface IWardInteractionService
    {
        bool TryOpenWardMenu(PrivateArea privateArea, Player player);
    }
}