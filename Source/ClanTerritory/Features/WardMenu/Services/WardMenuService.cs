using System.Collections.Generic;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.WardInteraction;
using ClanTerritory.Features.WardMenu.Models;
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

        private static WardMenuModel BuildModel(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea)
        {
            ZNetView zNetView = privateArea.GetComponent<ZNetView>();
            ZDO zdo = zNetView != null && zNetView.IsValid()
                ? zNetView.GetZDO()
                : null;

            string ownerName = zdo != null
                ? zdo.GetString(ZDOVars.s_creatorName, "Unknown")
                : "Unknown";

            bool enabled = zdo != null && zdo.GetBool(ZDOVars.s_enabled);

            List<WardMenuPlayerModel> permittedPlayers =
                BuildPermittedPlayers(zdo);

            return new WardMenuModel(
                wardId,
                ownerName,
                privateArea.m_radius,
                enabled,
                runtimeWard != null && runtimeWard.IsActive,
                permittedPlayers);
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
                    continue;

                players.Add(new WardMenuPlayerModel(
                    playerId,
                    playerName));
            }

            return players;
        }
    }
}