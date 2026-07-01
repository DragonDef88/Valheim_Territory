namespace ClanTerritory.API
{
    public static class ClanTerritoryApiProvider
    {
        private static readonly IClanTerritoryApi ApiInstance = new ClanTerritoryApi();

        public static IClanTerritoryApi Instance
        {
            get { return ApiInstance; }
        }
    }
}