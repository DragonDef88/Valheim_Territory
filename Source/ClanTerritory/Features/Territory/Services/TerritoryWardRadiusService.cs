using ClanTerritory.Config;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryWardRadiusService
    {
        public void ApplyConfiguredRadius(PrivateArea privateArea)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRadius] Apply ignored. PrivateArea is null.");
                return;
            }

            float radius = ConfigValues.TerritoryRadius;

            privateArea.m_radius = radius;

            if (privateArea.m_areaMarker != null)
                privateArea.m_areaMarker.m_radius = radius;

            ModLog.Debug("[TerritoryRadius] Configured territory radius applied to ward: " + radius);
        }
    }
}