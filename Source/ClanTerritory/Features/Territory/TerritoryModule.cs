using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Abstractions;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Map.Services;
using ClanTerritory.Features.Territory.Events;
using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Placement;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.Territory.Zdo;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Features.Territory
{
    internal sealed class TerritoryModule :
        IInitializable,
        IDisposableModule
    {
        private TerritoryRegistry _registry;
        private TerritoryFactory _factory;
        private TerritoryService _service;
        private TerritoryZdoService _zdoService;
        private WardMapIconService _mapIconService;
        private TerritoryWardRadiusService _wardRadiusService;
        private TerritoryRuleService _ruleService;
        private TerritoryTerraformingService _terraformingService;
        private TerritoryPresenceService _presenceService;
        private GameObject _runnerObject;
        private TerritoryRunner _runner;

        public void Initialize()
        {
            _registry = new TerritoryRegistry();
            _factory = new TerritoryFactory();
            _zdoService = new TerritoryZdoService();

            EventBus eventBus = ServiceContainer.Get<EventBus>();

            _wardRadiusService = new TerritoryWardRadiusService(eventBus);
            _ruleService = new TerritoryRuleService();
            _terraformingService = new TerritoryTerraformingService();
            _presenceService = new TerritoryPresenceService();

            _mapIconService = new WardMapIconService(
                _zdoService,
                _registry);

            _mapIconService.Initialize();

            _service = new TerritoryService(
                _registry,
                _factory,
                _mapIconService);

            ServiceContainer.Register<TerritoryRegistry>(_registry);
            ServiceContainer.Register<TerritoryZdoService>(_zdoService);
            ServiceContainer.Register<TerritoryWardRadiusService>(_wardRadiusService);
            ServiceContainer.Register<TerritoryRuleService>(_ruleService);
            ServiceContainer.Register<TerritoryTerraformingService>(_terraformingService);
            ServiceContainer.Register<WardMapIconService>(_mapIconService);
            ServiceContainer.Register<ITerritoryService>(_service);

            IWardPlacementPolicy placementPolicy = WardPlacementPolicyFactory.Create(_zdoService);
            ServiceContainer.Register<IWardPlacementPolicy>(placementPolicy);

            if (ServiceContainer.TryGet<EventBus>(out eventBus))
            {
                eventBus.Subscribe<WardRegisteredEvent>(_service);
                eventBus.Subscribe<WardDestroyedEvent>(_service);
                eventBus.Subscribe<TerritoryRadiusChangedEvent>(_service);
            }

            _runnerObject = new GameObject("ClanTerritory_TerritoryRunner");
            Object.DontDestroyOnLoad(_runnerObject);

            _runner = _runnerObject.AddComponent<TerritoryRunner>();
            _runner.Initialize(
                _ruleService,
                _terraformingService,
                _presenceService);

            ModLog.Info("Territory module initialized.");
        }

        public void Shutdown()
        {
            if (_mapIconService != null)
                _mapIconService.RemoveAll();

            if (_registry != null)
                _registry.Clear();

            if (_runnerObject != null)
                Object.Destroy(_runnerObject);

            _runner = null;
            _runnerObject = null;
            _presenceService = null;
            _terraformingService = null;
            _ruleService = null;

            ModLog.Info("Territory module shutdown.");
        }

        private sealed class TerritoryRunner : MonoBehaviour
        {
            private TerritoryRuleService _ruleService;
            private TerritoryTerraformingService _terraformingService;
            private TerritoryPresenceService _presenceService;

            public void Initialize(
                TerritoryRuleService ruleService,
                TerritoryTerraformingService terraformingService,
                TerritoryPresenceService presenceService)
            {
                _ruleService = ruleService;
                _terraformingService = terraformingService;
                _presenceService = presenceService;
            }

            private void Update()
            {
                if (_ruleService != null)
                    _ruleService.Update();

                if (_terraformingService != null)
                    _terraformingService.Update();

                if (_presenceService != null)
                    _presenceService.Update();
            }
        }

        private sealed class TerritoryPresenceService
        {
            private const float CheckInterval = 0.5f;

            private static readonly FieldInfo AllAreasField =
                AccessTools.Field(typeof(PrivateArea), "m_allAreas");

            private string _currentWardId = "";
            private string _currentTerritoryName = "";
            private float _nextCheckTime;

            public void Update()
            {
                if (Time.time < _nextCheckTime)
                    return;

                _nextCheckTime = Time.time + CheckInterval;

                Player player = Player.m_localPlayer;

                if (player == null)
                {
                    ClearCurrentTerritory();
                    return;
                }

                PrivateArea currentArea = FindCurrentTerritoryArea(player.transform.position);

                if (currentArea == null)
                {
                    if (!string.IsNullOrEmpty(_currentWardId))
                    {
                        ShowMessage(player, "Left territory: " + _currentTerritoryName);
                        ModLog.Info("[TerritoryPresence] Left territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
                    }

                    ClearCurrentTerritory();
                    return;
                }

                string wardId = GetWardId(currentArea);

                if (string.IsNullOrEmpty(wardId))
                    return;

                if (wardId == _currentWardId)
                    return;

                if (!string.IsNullOrEmpty(_currentWardId))
                {
                    ShowMessage(player, "Left territory: " + _currentTerritoryName);
                    ModLog.Info("[TerritoryPresence] Left territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
                }

                _currentWardId = wardId;
                _currentTerritoryName = GetTerritoryName(currentArea);

                ShowMessage(player, "Entered territory: " + _currentTerritoryName);
                ModLog.Info("[TerritoryPresence] Entered territory: " + _currentTerritoryName + ", ward: " + _currentWardId);
            }

            private static PrivateArea FindCurrentTerritoryArea(Vector3 position)
            {
                List<PrivateArea> areas = GetPrivateAreas();
                PrivateArea nearestArea = null;
                float nearestDistance = float.MaxValue;

                for (int i = 0; i < areas.Count; i++)
                {
                    PrivateArea privateArea = areas[i];

                    if (privateArea == null)
                        continue;

                    if (!IsInside(privateArea, position))
                        continue;

                    float distance = global::Utils.DistanceXZ(privateArea.transform.position, position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestArea = privateArea;
                    }
                }

                return nearestArea;
            }

            private static bool IsInside(PrivateArea privateArea, Vector3 position)
            {
                if (privateArea == null)
                    return false;

                return global::Utils.DistanceXZ(privateArea.transform.position, position) < privateArea.m_radius;
            }

            private static string GetWardId(PrivateArea privateArea)
            {
                if (privateArea == null)
                    return "";

                ZNetView zNetView = privateArea.GetComponent<ZNetView>();

                if (zNetView == null || !zNetView.IsValid())
                    return "";

                ZDO zdo = zNetView.GetZDO();

                if (zdo == null)
                    return "";

                return zdo.m_uid.ToString();
            }

            private static string GetTerritoryName(PrivateArea privateArea)
            {
                ITerritoryNamingService namingService;

                if (ServiceContainer.TryGet<ITerritoryNamingService>(out namingService))
                {
                    string name = namingService.GetTerritoryName(privateArea);

                    if (!string.IsNullOrWhiteSpace(name))
                        return name;
                }

                return "Unnamed Territory";
            }

            private static void ShowMessage(Player player, string message)
            {
                if (player == null)
                    return;

                player.Message(MessageHud.MessageType.Center, message);
            }

            private static List<PrivateArea> GetPrivateAreas()
            {
                if (AllAreasField == null)
                    return new List<PrivateArea>();

                List<PrivateArea> areas = AllAreasField.GetValue(null) as List<PrivateArea>;

                if (areas == null)
                    return new List<PrivateArea>();

                return areas;
            }

            private void ClearCurrentTerritory()
            {
                _currentWardId = "";
                _currentTerritoryName = "";
            }
        }
    }
}


namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryTerraformingService
    {
        private const string SetEnabledRpc = "CT_SetTerraformingEnabled";
        private const string SetRunningRpc = "CT_SetTerraformingRunning";
        private const string SetModeRpc = "CT_SetTerraformingMode";
        private const string SetRadiusRpc = "CT_SetTerraformingRadius";
        private const string SetTargetHeightRpc = "CT_SetTerraformingTargetHeight";
        private const string SetHoeStoredRpc = "CT_SetTerraformingHoeStored";
        private const string SetPickaxeStoredRpc = "CT_SetTerraformingPickaxeStored";
        private const string AddFuelRpc = "CT_AddTerraformingFuel";
        private const string AddStoneRpc = "CT_AddTerraformingStone";

        private const float DefaultRadius = 12f;
        private const float MinimumRadius = 2f;
        private const float MaximumRadius = 80f;
        private const float RadiusStep = 2f;
        private const float DefaultFuelAdd = 5f;
        private const float DefaultStoneAdd = 10f;
        private const float MaximumFuel = 250f;
        private const float MaximumStone = 500f;

        public void RegisterRpc(PrivateArea privateArea)
        {
            if (privateArea == null)
                return;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || zNetView.GetZDO() == null)
            {
                ModLog.Debug("[TerritoryTerraforming] RPC registration ignored. ZNetView or ZDO is null.");
                return;
            }

            zNetView.Register<long, bool>(
                SetEnabledRpc,
                delegate(long sender, long playerId, bool enabled)
                {
                    RPC_SetEnabled(privateArea, zNetView, playerId, enabled);
                });

            zNetView.Register<long, bool>(
                SetRunningRpc,
                delegate(long sender, long playerId, bool running)
                {
                    RPC_SetRunning(privateArea, zNetView, playerId, running);
                });

            zNetView.Register<long, int>(
                SetModeRpc,
                delegate(long sender, long playerId, int mode)
                {
                    RPC_SetMode(privateArea, zNetView, playerId, mode);
                });

            zNetView.Register<long, float>(
                SetRadiusRpc,
                delegate(long sender, long playerId, float radius)
                {
                    RPC_SetRadius(privateArea, zNetView, playerId, radius);
                });

            zNetView.Register<long, float>(
                SetTargetHeightRpc,
                delegate(long sender, long playerId, float targetHeight)
                {
                    RPC_SetTargetHeight(privateArea, zNetView, playerId, targetHeight);
                });

            zNetView.Register<long, bool>(
                SetHoeStoredRpc,
                delegate(long sender, long playerId, bool stored)
                {
                    RPC_SetHoeStored(privateArea, zNetView, playerId, stored);
                });

            zNetView.Register<long, bool>(
                SetPickaxeStoredRpc,
                delegate(long sender, long playerId, bool stored)
                {
                    RPC_SetPickaxeStored(privateArea, zNetView, playerId, stored);
                });

            zNetView.Register<long, float>(
                AddFuelRpc,
                delegate(long sender, long playerId, float amount)
                {
                    RPC_AddFuel(privateArea, zNetView, playerId, amount);
                });

            zNetView.Register<long, float>(
                AddStoneRpc,
                delegate(long sender, long playerId, float amount)
                {
                    RPC_AddStone(privateArea, zNetView, playerId, amount);
                });

            EnsureDefaults(privateArea);

            ModLog.Debug("[TerritoryTerraforming] RPCs registered for ward.");
        }

        public void Update()
        {
            // Worker will be added in the next terrain-operation step.
            // This package intentionally establishes the persistent controls and menu first.
        }

        public TerraformingState GetState(PrivateArea privateArea)
        {
            EnsureDefaults(privateArea);

            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return TerraformingState.Disabled();

            return new TerraformingState(
                zdo.GetBool(TerritoryZdoKeys.TerraformingEnabled, false),
                zdo.GetBool(TerritoryZdoKeys.TerraformingRunning, false),
                NormalizeMode(zdo.GetInt(TerritoryZdoKeys.TerraformingMode, (int)TerraformingMode.Level)),
                NormalizeRadius(zdo.GetFloat(TerritoryZdoKeys.TerraformingRadius, DefaultRadius)),
                zdo.GetFloat(TerritoryZdoKeys.TerraformingTargetHeight, privateArea.transform.position.y),
                Mathf.Clamp(zdo.GetFloat(TerritoryZdoKeys.TerraformingFuelStored, 0f), 0f, MaximumFuel),
                Mathf.Clamp(zdo.GetFloat(TerritoryZdoKeys.TerraformingStoneStored, 0f), 0f, MaximumStone),
                zdo.GetBool(TerritoryZdoKeys.TerraformingHoeStored, false),
                zdo.GetBool(TerritoryZdoKeys.TerraformingPickaxeStored, false),
                Mathf.Max(0f, zdo.GetFloat(TerritoryZdoKeys.TerraformingScanProgress, 0f)),
                Mathf.Max(0, zdo.GetInt(TerritoryZdoKeys.TerraformingScanIndex, 0)));
        }

        public bool RequestToggleEnabled(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetEnabled(
                wardId,
                privateArea,
                player,
                !state.Enabled);
        }

        public bool RequestToggleRunning(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetRunning(
                wardId,
                privateArea,
                player,
                !state.Running);
        }

        public bool RequestCycleMode(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);
            int nextMode = ((int)state.Mode + 1) % 4;

            return RequestSetMode(
                wardId,
                privateArea,
                player,
                nextMode);
        }

        public bool RequestDecreaseRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetRadius(
                wardId,
                privateArea,
                player,
                state.Radius - RadiusStep);
        }

        public bool RequestIncreaseRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            TerraformingState state = GetState(privateArea);

            return RequestSetRadius(
                wardId,
                privateArea,
                player,
                state.Radius + RadiusStep);
        }

        public bool RequestSetTargetHeightFromWard(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
                return false;

            return RequestSetTargetHeight(
                wardId,
                privateArea,
                player,
                privateArea.transform.position.y);
        }

        public bool RequestSetTargetHeightFromPlayer(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (player == null)
                return false;

            return RequestSetTargetHeight(
                wardId,
                privateArea,
                player,
                player.transform.position.y);
        }

        public bool RequestStoreHoe(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            return RequestSetHoeStored(
                wardId,
                privateArea,
                player,
                true);
        }

        public bool RequestStorePickaxe(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            return RequestSetPickaxeStored(
                wardId,
                privateArea,
                player,
                true);
        }

        public bool RequestAddFuel(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            return RequestAddFuel(
                wardId,
                privateArea,
                player,
                DefaultFuelAdd);
        }

        public bool RequestAddStone(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            return RequestAddStone(
                wardId,
                privateArea,
                player,
                DefaultStoneAdd);
        }

        private bool RequestSetEnabled(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool enabled)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingEnabled",
                SetEnabledRpc,
                enabled);
        }

        private bool RequestSetRunning(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool running)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingRunning",
                SetRunningRpc,
                running);
        }

        private bool RequestSetMode(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            int mode)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingMode",
                SetModeRpc,
                NormalizeMode(mode));
        }

        private bool RequestSetRadius(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            float radius)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingRadius",
                SetRadiusRpc,
                NormalizeRadius(radius));
        }

        private bool RequestSetTargetHeight(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            float targetHeight)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingTargetHeight",
                SetTargetHeightRpc,
                targetHeight);
        }

        private bool RequestSetHoeStored(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool stored)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingHoeStored",
                SetHoeStoredRpc,
                stored);
        }

        private bool RequestSetPickaxeStored(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            bool stored)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "SetTerraformingPickaxeStored",
                SetPickaxeStoredRpc,
                stored);
        }

        private bool RequestAddFuel(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            float amount)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "AddTerraformingFuel",
                AddFuelRpc,
                amount);
        }

        private bool RequestAddStone(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            float amount)
        {
            return InvokeOwnerRpc(
                wardId,
                privateArea,
                player,
                "AddTerraformingStone",
                AddStoneRpc,
                amount);
        }

        private bool InvokeOwnerRpc(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string actionName,
            string rpcName,
            bool value)
        {
            if (!ValidateCreatorAction(actionName, wardId, privateArea, player))
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                rpcName,
                player.GetPlayerID(),
                value);

            ModLog.Info("[TerritoryTerraforming] " + actionName + " requested: " + wardId + ", value: " + value);
            return true;
        }

        private bool InvokeOwnerRpc(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string actionName,
            string rpcName,
            int value)
        {
            if (!ValidateCreatorAction(actionName, wardId, privateArea, player))
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                rpcName,
                player.GetPlayerID(),
                value);

            ModLog.Info("[TerritoryTerraforming] " + actionName + " requested: " + wardId + ", value: " + value);
            return true;
        }

        private bool InvokeOwnerRpc(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            string actionName,
            string rpcName,
            float value)
        {
            if (!ValidateCreatorAction(actionName, wardId, privateArea, player))
                return false;

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            zNetView.InvokeRPC(
                rpcName,
                player.GetPlayerID(),
                value);

            ModLog.Info("[TerritoryTerraforming] " + actionName + " requested: " + wardId + ", value: " + value);
            return true;
        }

        private void RPC_SetEnabled(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool enabled)
        {
            if (!TryGetOwnerZdo("SetTerraformingEnabled", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingEnabled, enabled);

            if (!enabled)
                zdo.Set(TerritoryZdoKeys.TerraformingRunning, false);

            ModLog.Info("[TerritoryTerraforming] Enabled saved: " + enabled);
        }

        private void RPC_SetRunning(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool running)
        {
            if (!TryGetOwnerZdo("SetTerraformingRunning", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            if (running && !zdo.GetBool(TerritoryZdoKeys.TerraformingEnabled, false))
            {
                ModLog.Debug("[TerritoryTerraforming] Running ignored. Terraforming is disabled.");
                return;
            }

            zdo.Set(TerritoryZdoKeys.TerraformingRunning, running);
            ModLog.Info("[TerritoryTerraforming] Running saved: " + running);
        }

        private void RPC_SetMode(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            int mode)
        {
            if (!TryGetOwnerZdo("SetTerraformingMode", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingMode, NormalizeMode(mode));
            zdo.Set(TerritoryZdoKeys.TerraformingScanProgress, 0f);
            zdo.Set(TerritoryZdoKeys.TerraformingScanIndex, 0);
            ModLog.Info("[TerritoryTerraforming] Mode saved: " + NormalizeMode(mode));
        }

        private void RPC_SetRadius(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            float radius)
        {
            if (!TryGetOwnerZdo("SetTerraformingRadius", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingRadius, NormalizeRadius(radius));
            zdo.Set(TerritoryZdoKeys.TerraformingScanProgress, 0f);
            zdo.Set(TerritoryZdoKeys.TerraformingScanIndex, 0);
            ModLog.Info("[TerritoryTerraforming] Radius saved: " + NormalizeRadius(radius));
        }

        private void RPC_SetTargetHeight(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            float targetHeight)
        {
            if (!TryGetOwnerZdo("SetTerraformingTargetHeight", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingTargetHeight, targetHeight);
            zdo.Set(TerritoryZdoKeys.TerraformingScanProgress, 0f);
            zdo.Set(TerritoryZdoKeys.TerraformingScanIndex, 0);
            ModLog.Info("[TerritoryTerraforming] Target height saved: " + targetHeight);
        }

        private void RPC_SetHoeStored(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool stored)
        {
            if (!TryGetOwnerZdo("SetTerraformingHoeStored", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingHoeStored, stored);
            ModLog.Info("[TerritoryTerraforming] Hoe slot saved: " + stored);
        }

        private void RPC_SetPickaxeStored(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            bool stored)
        {
            if (!TryGetOwnerZdo("SetTerraformingPickaxeStored", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            zdo.Set(TerritoryZdoKeys.TerraformingPickaxeStored, stored);
            ModLog.Info("[TerritoryTerraforming] Pickaxe slot saved: " + stored);
        }

        private void RPC_AddFuel(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            float amount)
        {
            if (!TryGetOwnerZdo("AddTerraformingFuel", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            float current = zdo.GetFloat(TerritoryZdoKeys.TerraformingFuelStored, 0f);
            float next = Mathf.Clamp(current + Mathf.Max(0f, amount), 0f, MaximumFuel);

            zdo.Set(TerritoryZdoKeys.TerraformingFuelStored, next);
            ModLog.Info("[TerritoryTerraforming] Fuel stored: " + next);
        }

        private void RPC_AddStone(
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            float amount)
        {
            if (!TryGetOwnerZdo("AddTerraformingStone", privateArea, zNetView, playerId, out ZDO zdo))
                return;

            float current = zdo.GetFloat(TerritoryZdoKeys.TerraformingStoneStored, 0f);
            float next = Mathf.Clamp(current + Mathf.Max(0f, amount), 0f, MaximumStone);

            zdo.Set(TerritoryZdoKeys.TerraformingStoneStored, next);
            ModLog.Info("[TerritoryTerraforming] Stone stored: " + next);
        }

        private static void EnsureDefaults(PrivateArea privateArea)
        {
            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return;

            if (zdo.GetFloat(TerritoryZdoKeys.TerraformingRadius, 0f) <= 0f)
                zdo.Set(TerritoryZdoKeys.TerraformingRadius, DefaultRadius);

            if (zdo.GetFloat(TerritoryZdoKeys.TerraformingTargetHeight, -99999f) < -99990f)
                zdo.Set(TerritoryZdoKeys.TerraformingTargetHeight, privateArea.transform.position.y);
        }

        private static bool ValidateCreatorAction(
            string actionName,
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. Piece is null: " + wardId);
                return false;
            }

            if (piece.GetCreator() != player.GetPlayerID())
            {
                ModLog.Debug("[TerritoryTerraforming] " + actionName + " ignored. Player is not ward creator: " + wardId);
                return false;
            }

            return true;
        }

        private static bool TryGetOwnerZdo(
            string actionName,
            PrivateArea privateArea,
            ZNetView zNetView,
            long playerId,
            out ZDO zdo)
        {
            zdo = null;

            if (privateArea == null || zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. Invalid ward: " + actionName);
                return false;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. ZNetView is not owner: " + actionName);
                return false;
            }

            zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. ZDO is null: " + actionName);
                return false;
            }

            if (zdo.GetLong(ZDOVars.s_creator, 0L) != playerId)
            {
                ModLog.Debug("[TerritoryTerraforming] RPC ignored. Player is not ward creator: " + actionName + ", playerId: " + playerId);
                return false;
            }

            return true;
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

        private static float NormalizeRadius(float radius)
        {
            return Mathf.Clamp(radius, MinimumRadius, MaximumRadius);
        }

        private static int NormalizeMode(int mode)
        {
            if (mode < 0)
                return 0;

            if (mode > 3)
                return 3;

            return mode;
        }
    }

    internal enum TerraformingMode
    {
        Level = 0,
        Raise = 1,
        Lower = 2,
        Smooth = 3
    }

    internal sealed class TerraformingState
    {
        public bool Enabled { get; private set; }

        public bool Running { get; private set; }

        public TerraformingMode Mode { get; private set; }

        public float Radius { get; private set; }

        public float TargetHeight { get; private set; }

        public float FuelStored { get; private set; }

        public float StoneStored { get; private set; }

        public bool HoeStored { get; private set; }

        public bool PickaxeStored { get; private set; }

        public float ScanProgress { get; private set; }

        public int ScanIndex { get; private set; }

        public TerraformingState(
            bool enabled,
            bool running,
            int mode,
            float radius,
            float targetHeight,
            float fuelStored,
            float stoneStored,
            bool hoeStored,
            bool pickaxeStored,
            float scanProgress,
            int scanIndex)
        {
            Enabled = enabled;
            Running = running;
            Mode = (TerraformingMode)mode;
            Radius = radius;
            TargetHeight = targetHeight;
            FuelStored = fuelStored;
            StoneStored = stoneStored;
            HoeStored = hoeStored;
            PickaxeStored = pickaxeStored;
            ScanProgress = scanProgress;
            ScanIndex = scanIndex;
        }

        public static TerraformingState Disabled()
        {
            return new TerraformingState(
                false,
                false,
                (int)TerraformingMode.Level,
                12f,
                0f,
                0f,
                0f,
                false,
                false,
                0f,
                0);
        }
    }
}
