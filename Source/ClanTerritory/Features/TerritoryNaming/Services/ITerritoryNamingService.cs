namespace ClanTerritory.Features.TerritoryNaming.Services
{
    internal interface ITerritoryNamingService
    {
        void RegisterRpc(PrivateArea privateArea);

        string GetTerritoryName(PrivateArea privateArea);

        void RequestRename(
            PrivateArea privateArea,
            Player player,
            string name);
    }
}