namespace ClanTerritory.Features.Persistence.Services
{
    internal interface IPersistenceService
    {
        void SaveNow();
        void LoadNow();
    }
}