using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.WardInteraction;
using ClanTerritory.Features.WardMenu.UI;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Services
{
    internal sealed class WardMenuService :
        IWardMenuService,
        IEventHandler<WardInteractionRequestedEvent>
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

        public void Handle(WardInteractionRequestedEvent eventData)
        {
            if (eventData == null)
                return;

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
                return;

            if (player == null)
                return;

            _currentWardId = wardId;
            _currentRuntimeWard = runtimeWard;
            _currentPrivateArea = privateArea;
            _currentPlayer = player;

            _view.Show(
                wardId,
                runtimeWard,
                privateArea,
                player,
                Close);

            ModLog.Info("Ward menu opened: " + wardId);
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            _view.Hide();

            ModLog.Info("Ward menu closed: " + _currentWardId);

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
                Close();
                return;
            }

            if (_currentPlayer == null || _currentPlayer.IsDead() || _currentPlayer.InCutscene())
            {
                Close();
                return;
            }

            _view.Tick(_currentPrivateArea, _currentPlayer);
        }

        public void Shutdown()
        {
            Close();

            if (_view != null)
                _view.Destroy();
        }
    }
}