using BepInEx;

namespace ClanTerritory.Core
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
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