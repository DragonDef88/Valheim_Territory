namespace ClanTerritory.API
{
    public interface IClanTerritoryApi
    {
        string ModName { get; }

        string ModVersion { get; }

        bool IsInitialized { get; }
    }
}