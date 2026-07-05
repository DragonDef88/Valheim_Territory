using ClanTerritory.Features.Persistence.Models;

namespace ClanTerritory.Features.Persistence.Services
{
    internal interface IPersistenceService
    {
        void SaveNow();

        void LoadNow();

        SaveFileModel LoadSnapshot();

        void MarkWardDeleted(string wardId);
    }
}