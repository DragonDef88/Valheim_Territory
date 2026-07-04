namespace ClanTerritory.Features.Diagnostics.Services
{
    internal interface IDiagnosticsService
    {
        void LogCheckpoint(string checkpoint);
        void LogWorldState(string checkpoint);
    }
}