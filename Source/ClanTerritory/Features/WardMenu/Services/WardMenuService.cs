using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.TerritoryInteraction;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Features.WardMenu.UI;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Services
{
    internal sealed class WardMenuService :
        IWardMenuService,
        IEventHandler<TerritoryInteractionRequestedEvent>
    {
        private readonly WardMenuView _view;

        private WardId _currentWardId;
        private RuntimeWard _currentRuntimeWard;
        private PrivateArea _currentPrivateArea;
        private Player _currentPlayer;

        public bool IsOpen
        {
            get { return _view != null && _view.IsVisible; }
        }

        public WardId CurrentWardId
        {
            get { return _currentWardId; }
        }

        public WardMenuService(WardMenuView view)
        {
            _view = view;
        }

        public void Handle(TerritoryInteractionRequestedEvent eventData)
        {
            if (eventData == null)
            {
                ModLog.Debug("[WardMenu] Ignored null territory interaction event.");
                return;
            }

            Open(
                eventData.WardId,
                eventData.RuntimeWard,
                eventData.PrivateArea,
                eventData.Player);
        }

        public void Open(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenu] Open ignored. PrivateArea is null.");
                return;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenu] Open ignored. Player is null.");
                return;
            }

            WardMenuModel model = BuildModel(
                wardId,
                runtimeWard,
                privateArea);

            _currentWardId = wardId;
            _currentRuntimeWard = runtimeWard;
            _currentPrivateArea = privateArea;
            _currentPlayer = player;

            _view.Show(
                model,
                CloseByInput,
                CloseByDistance);

            ModLog.Info(
                "[WardMenu] Opened ward territory menu: " + model.Ward.WardId +
                ", owner: " + model.Ward.OwnerName +
                ", enabled: " + model.Ward.Enabled +
                ", permitted: " + model.Ward.PermittedPlayers.Count +
                ", territory: " + model.Territory.Name);
        }

        public void Close()
        {
            CloseWithReason("Manual");
        }

        private void CloseByInput()
        {
            CloseWithReason("Input");
        }

        private void CloseByDistance()
        {
            CloseWithReason("Distance");
        }

        private void CloseWithReason(string reason)
        {
            if (!IsOpen)
                return;

            _view.Hide();

            ModLog.Info(
                "[WardMenu] Closed ward territory menu: " + _currentWardId +
                ", reason: " + reason);

            _currentRuntimeWard = null;
            _currentPrivateArea = null;
            _currentPlayer = null;
        }

        public void Update()
        {
            if (!IsOpen)
                return;

            if (_currentPrivateArea == null)
            {
                CloseWithReason("InvalidPrivateArea");
                return;
            }

            if (_currentPlayer == null)
            {
                CloseWithReason("InvalidPlayer");
                return;
            }

            if (_currentPlayer.IsDead())
            {
                CloseWithReason("PlayerDead");
                return;
            }

            if (_currentPlayer.InCutscene())
            {
                CloseWithReason("PlayerInCutscene");
                return;
            }

            _view.Tick(_currentPrivateArea, _currentPlayer);
        }

        public void Shutdown()
        {
            CloseWithReason("Shutdown");

            if (_view != null)
                _view.Destroy();
        }

        private static WardMenuModel BuildModel(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea)
        {
            ZNetView zNetView = privateArea.GetComponent<ZNetView>();
            ZDO zdo = zNetView != null && zNetView.IsValid()
                ? zNetView.GetZDO()
                : null;

            if (zdo == null)
                ModLog.Debug("[WardMenu] Building ward territory model without ZDO: " + wardId);

            string ownerName = zdo != null
                ? zdo.GetString(ZDOVars.s_creatorName, "Unknown")
                : "Unknown";

            bool enabled = zdo != null && zdo.GetBool(ZDOVars.s_enabled);

            List<WardMenuPlayerModel> permittedPlayers =
                BuildPermittedPlayers(zdo);

            WardMenuWardSection wardSection = new WardMenuWardSection(
                wardId,
                ownerName,
                privateArea.m_radius,
                enabled,
                permittedPlayers);

            WardMenuTerritorySection territorySection = new WardMenuTerritorySection(
                "Unnamed Territory",
                runtimeWard != null && runtimeWard.IsActive,
                false,
                false,
                "Default rules");

            WardMenuModel model = new WardMenuModel(
                wardSection,
                territorySection);

            ModLog.Debug(
                "[WardMenu] Ward territory model created: " + wardId +
                ", owner: " + wardSection.OwnerName +
                ", radius: " + wardSection.Radius +
                ", enabled: " + wardSection.Enabled +
                ", runtimeActive: " + territorySection.RuntimeActive +
                ", permitted: " + wardSection.PermittedPlayers.Count);

            return model;
        }

        private static List<WardMenuPlayerModel> BuildPermittedPlayers(ZDO zdo)
        {
            List<WardMenuPlayerModel> players =
                new List<WardMenuPlayerModel>();

            if (zdo == null)
                return players;

            int permittedCount = zdo.GetInt(ZDOVars.s_permitted);

            for (int i = 0; i < permittedCount; i++)
            {
                long playerId = zdo.GetLong("pu_id" + i, 0L);
                string playerName = zdo.GetString("pu_name" + i, "Unknown");

                if (playerId == 0L)
                {
                    ModLog.Debug("[WardMenu] Ignored permitted player with empty id at index: " + i);
                    continue;
                }

                players.Add(new WardMenuPlayerModel(
                    playerId,
                    playerName));
            }

            return players;
        }
    }
}