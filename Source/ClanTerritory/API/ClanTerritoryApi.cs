using ClanTerritory.Core;

namespace ClanTerritory.API
{
    public sealed class ClanTerritoryApi : IClanTerritoryApi
    {
        public string ModName
        {
            get { return ModInfo.Name; }
        }

        public string ModVersion
        {
            get { return ModInfo.Version; }
        }

        public bool IsInitialized
        {
            get { return Bootstrap.IsInitialized; }
        }
    }
}