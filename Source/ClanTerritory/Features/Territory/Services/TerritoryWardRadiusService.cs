using ClanTerritory.Config;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryWardRadiusService
    {
        public void ApplyConfiguredRadius(PrivateArea privateArea)
        {
            ApplyRadius(
                privateArea,
                ConfigValues.TerritoryRadius);
        }

        public void ApplyRadius(
            PrivateArea privateArea,
            float radius)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryRadius] Apply ignored. PrivateArea is null.");
                return;
            }

            if (radius <= 0f)
            {
                ModLog.Debug("[TerritoryRadius] Apply ignored. Radius is invalid: " + radius);
                return;
            }

            privateArea.m_radius = radius;

            if (privateArea.m_areaMarker != null)
                privateArea.m_areaMarker.m_radius = radius;

            ModLog.Info("[TerritoryRadius] Territory radius applied to ward: " + radius);
        }
    }
}