using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.TerritoryInteraction;
using ClanTerritory.Features.TerritoryNaming.Events;
using ClanTerritory.Features.WardMenu.Builders;
using ClanTerritory.Features.WardMenu.Controllers;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Services
{
    internal sealed class WardMenuService :
        IWardMenuService,
        IEventHandler<TerritoryInteractionRequestedEvent>,
        IEventHandler<TerritoryRenamedEvent>
    {
        private readonly WardMenuController _controller;
        private readonly WardMenuModelBuilder _modelBuilder;

        private WardId _currentWardId;
        private RuntimeWard _currentRuntimeWard;
        private PrivateArea _currentPrivateArea;
        private Player _currentPlayer;
        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public WardId CurrentWardId
        {
            get { return _currentWardId; }
        }

        public WardMenuService(
            WardMenuController controller,
            WardMenuModelBuilder modelBuilder)
        {
            _controller = controller;
            _modelBuilder = modelBuilder;
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

        public void Handle(TerritoryRenamedEvent eventData)
        {
            if (!_isOpen)
                return;

            if (eventData == null)
                return;

            if (!eventData.WardId.Equals(_currentWardId))
                return;

            Refresh();
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

            WardMenuModel model = _modelBuilder.Build(
                wardId,
                runtimeWard,
                privateArea);

            _currentWardId = wardId;
            _currentRuntimeWard = runtimeWard;
            _currentPrivateArea = privateArea;
            _currentPlayer = player;
            _isOpen = true;

            _controller.Show(
                model,
                privateArea,
                player);

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

        public void CloseWithReason(string reason)
        {
            if (!_isOpen)
                return;

            _controller.Hide();

            ModLog.Info(
                "[WardMenu] Closed ward territory menu: " + _currentWardId +
                ", reason: " + reason);

            _currentRuntimeWard = null;
            _currentPrivateArea = null;
            _currentPlayer = null;
            _isOpen = false;
        }

        public void Refresh()
        {
            if (!_isOpen)
                return;

            if (_currentPrivateArea == null)
                return;

            WardMenuModel model = _modelBuilder.Build(
                _currentWardId,
                _currentRuntimeWard,
                _currentPrivateArea);

            _controller.Refresh(model);

            ModLog.Info(
                "[WardMenu] Refreshed ward territory menu: " +
                model.Ward.WardId +
                ", territory: " +
                model.Territory.Name);
        }

        public void Update()
        {
            if (!_isOpen)
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

            _controller.Tick(_currentPrivateArea, _currentPlayer);
        }

        public void Shutdown()
        {
            CloseWithReason("Shutdown");

            if (_controller != null)
                _controller.Destroy();
        }
    }
}