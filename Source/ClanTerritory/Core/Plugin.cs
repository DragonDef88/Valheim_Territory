using BepInEx;

namespace ClanTerritory.Core
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    [BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Bootstrap.Initialize(this, Logger, Config);
        }

        private void OnDestroy()
        {
            Bootstrap.Shutdown();
        }
    }
}