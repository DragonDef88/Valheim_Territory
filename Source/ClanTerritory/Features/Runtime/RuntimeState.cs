namespace ClanTerritory.Features.Runtime
{
    internal enum RuntimeState
    {
        PluginLoaded,
        InfrastructureReady,
        WorldLoading,
        WorldLoaded,
        DiscoveryCompleted,
        RegistrySynchronized,
        GameplayReady
    }
}