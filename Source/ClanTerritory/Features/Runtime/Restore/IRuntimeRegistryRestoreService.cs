namespace ClanTerritory.Features.Runtime.Restore
{
    internal interface IRuntimeRegistryRestoreService
    {
        void Restore(RuntimeRestoreSnapshot snapshot);
    }
}