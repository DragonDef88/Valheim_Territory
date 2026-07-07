using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.TerritoryNaming.Events;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.TerritoryNaming.Services
{
    internal sealed class TerritoryNamingService : ITerritoryNamingService
    {
        private const string SetTerritoryNameRpc = "CT_SetTerritoryName";
        private const int MaxTerritoryNameLength = 50;

        private readonly EventBus _eventBus;

        public TerritoryNamingService(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void RegisterRpc(PrivateArea privateArea)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryNaming] RPC registration ignored. PrivateArea is null.");
                return;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || zNetView.GetZDO() == null)
            {
                ModLog.Debug("[TerritoryNaming] RPC registration ignored. ZNetView or ZDO is null.");
                return;
            }

            zNetView.Register<long, string>(
                SetTerritoryNameRpc,
                delegate (long sender, long playerId, string name)
                {
                    RPC_SetTerritoryName(
                        zNetView,
                        sender,
                        playerId,
                        name);
                });

            ModLog.Debug("[TerritoryNaming] RPC registered for ward.");
        }

        public string GetTerritoryName(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return "Unnamed Territory";

            return zdo.GetString(
                TerritoryNamingZdoKeys.TerritoryName,
                "Unnamed Territory");
        }

        public void RequestRename(
            PrivateArea privateArea,
            Player player,
            string name)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryNaming] Rename ignored. PrivateArea is null.");
                return;
            }

            if (player == null)
            {
                ModLog.Debug("[TerritoryNaming] Rename ignored. Player is null.");
                return;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryNaming] Rename ignored. ZNetView is invalid.");
                return;
            }

            string normalizedName = NormalizeName(name);

            if (string.IsNullOrEmpty(normalizedName))
            {
                ModLog.Debug("[TerritoryNaming] Rename ignored. Name is empty.");
                return;
            }

            zNetView.InvokeRPC(
                SetTerritoryNameRpc,
                player.GetPlayerID(),
                normalizedName);

            ModLog.Info("[TerritoryNaming] Rename requested: " + normalizedName);
        }

        private void RPC_SetTerritoryName(
            ZNetView zNetView,
            long sender,
            long playerId,
            string name)
        {
            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryNaming] RPC ignored. ZNetView is invalid.");
                return;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[TerritoryNaming] RPC ignored. ZNetView is not owner.");
                return;
            }

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[TerritoryNaming] RPC ignored. ZDO is null.");
                return;
            }

            string normalizedName = NormalizeName(name);

            if (string.IsNullOrEmpty(normalizedName))
            {
                ModLog.Debug("[TerritoryNaming] RPC ignored. Name is empty.");
                return;
            }

            zdo.Set(
                TerritoryNamingZdoKeys.TerritoryName,
                normalizedName);

            WardId wardId = new WardId(zdo.m_uid.ToString());

            if (_eventBus != null)
            {
                _eventBus.Publish(
                    new TerritoryRenamedEvent(
                        wardId,
                        normalizedName));
            }

            ModLog.Info(
                "[TerritoryNaming] Territory name saved: " +
                normalizedName +
                ", playerId: " +
                playerId);
        }

        private static ZDO GetZdo(PrivateArea privateArea)
        {
            if (privateArea == null)
                return null;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return null;

            return zNetView.GetZDO();
        }

        private static string NormalizeName(string name)
        {
            if (name == null)
                return "";

            string normalized = name.Trim();

            if (normalized.Length > MaxTerritoryNameLength)
                normalized = normalized.Substring(0, MaxTerritoryNameLength);

            return normalized;
        }
    }
}