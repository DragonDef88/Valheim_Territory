using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Localization;
using ClanTerritory.Utils;
using UnityEngine;

namespace ClanTerritory.Features.Races
{
    internal enum RaceKind
    {
        None = 0,
        Werewolf = 1,
        Vampire = 2,
        OdinBlessed = 3
    }

    internal sealed class RaceModule :
        IInitializable,
        IDisposableModule
    {
        private RaceService _service;

        public void Initialize()
        {
            _service = new RaceService();
            _service.Initialize();

            ServiceContainer.Register<RaceService>(_service);

            ModLog.Info("[Races] Module initialized.");
        }

        public void Shutdown()
        {
            if (_service != null)
                _service.Shutdown();

            _service = null;

            ModLog.Info("[Races] Module shutdown.");
        }
    }

    internal sealed class RaceService
    {
        private const string FileSuffix = ".races.txt";
        private const string PlayerRaceZdoKey = "ct_player_race";
        private const int MissingRuntimeRaceValue = -1;

        private const float VampireFireMultiplier = 1.35f;
        private const float VampireSpiritMultiplier = 1.50f;
        private const float WerewolfSpiritMultiplier = 1.40f;
        private const float OdinSpiritMultiplier = 0.50f;
        private const float OdinFrostMultiplier = 0.75f;
        private const float OdinLightningMultiplier = 0.75f;

        private static readonly MethodInfo ZNetGetWorldNameMethod =
            AccessTools.Method(
                typeof(ZNet),
                "GetWorldName",
                Type.EmptyTypes);

        private readonly Dictionary<long, RaceKind> _racesByPlayerId =
            new Dictionary<long, RaceKind>();

        private ZNet _loadedZNetInstance;
        private string _loadedWorldName = "";
        private string _savePath = "";
        private bool _commandsRegistered;

        public void Initialize()
        {
            RegisterCommandsOnce();
            EnsureWorldLoaded();

            ModLog.Info(
                "[Races] Service initialized. Races: " +
                _racesByPlayerId.Count);
        }

        public void Shutdown()
        {
            Save();
            ClearLoadedWorld();

            ModLog.Info("[Races] Service shutdown.");
        }

        public void Save()
        {
            if (!EnsureWorldLoaded() || string.IsNullOrEmpty(_savePath))
                return;

            string temporaryPath = _savePath + ".tmp";

            try
            {
                string directory = Path.GetDirectoryName(_savePath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                List<long> playerIds = new List<long>(_racesByPlayerId.Keys);
                playerIds.Sort();

                List<string> lines = new List<string>();
                lines.Add("# Clan Territory races");
                lines.Add("# playerId=race");

                for (int i = 0; i < playerIds.Count; i++)
                {
                    long playerId = playerIds[i];
                    RaceKind race;

                    if (!_racesByPlayerId.TryGetValue(playerId, out race) ||
                        race == RaceKind.None)
                    {
                        continue;
                    }

                    lines.Add(
                        playerId.ToString(CultureInfo.InvariantCulture) +
                        "=" +
                        race.ToString());
                }

                File.WriteAllLines(
                    temporaryPath,
                    lines.ToArray(),
                    System.Text.Encoding.UTF8);

                ReplaceFile(temporaryPath, _savePath);
            }
            catch (Exception exception)
            {
                ModLog.Warning(
                    "[Races] Failed to save races: " +
                    exception.Message);

                TryDeleteFile(temporaryPath);
            }
        }

        public RaceKind GetRace(Player player)
        {
            if (player == null)
                return RaceKind.None;

            EnsureWorldLoaded();

            RaceKind runtimeRace;

            if (TryGetRuntimeRace(player, out runtimeRace))
                return runtimeRace;

            RaceKind persistedRace = GetPersistedRace(player.GetPlayerID());

            if (player == Player.m_localPlayer)
                PublishRuntimeRace(player, persistedRace);

            return persistedRace;
        }

        public RaceKind GetRace(long playerId)
        {
            EnsureWorldLoaded();
            return GetPersistedRace(playerId);
        }

        public bool SetRace(Player player, RaceKind race, bool allowChange)
        {
            if (player == null)
                return false;

            if (!IsSelectableRace(race))
                return false;

            if (!EnsureWorldLoaded())
            {
                ShowPlayerMessage(
                    player,
                    CtLocalization.Get("ct.race.no_world"));
                return false;
            }

            long playerId = player.GetPlayerID();
            RaceKind current = GetRace(player);

            if (current == race)
            {
                ShowPlayerMessage(
                    player,
                    CtLocalization.Format(
                        "ct.race.already_chosen",
                        FormatRace(current)));
                return true;
            }

            if (!allowChange && current != RaceKind.None)
            {
                ShowPlayerMessage(
                    player,
                    CtLocalization.Format(
                        "ct.race.already_chosen",
                        FormatRace(current)));
                return false;
            }

            _racesByPlayerId[playerId] = race;
            PublishRuntimeRace(player, race);
            Save();

            ShowPlayerMessage(
                player,
                CtLocalization.Format(
                    "ct.race.chosen",
                    FormatRace(race)));

            ModLog.Info(
                "[Races] Player race saved. PlayerId: " +
                playerId +
                ", race: " +
                race);

            return true;
        }

        public bool ResetRace(Player player)
        {
            if (player == null)
                return false;

            if (!EnsureWorldLoaded())
            {
                ShowPlayerMessage(
                    player,
                    CtLocalization.Get("ct.race.no_world"));
                return false;
            }

            long playerId = player.GetPlayerID();
            _racesByPlayerId.Remove(playerId);
            PublishRuntimeRace(player, RaceKind.None);
            Save();

            ShowPlayerMessage(
                player,
                CtLocalization.Get("ct.race.reset"));

            ModLog.Info(
                "[Races] Player race reset. PlayerId: " +
                playerId);

            return true;
        }

        public void PublishLocalPlayerRace(Player player)
        {
            if (player == null || player != Player.m_localPlayer)
                return;

            if (!EnsureWorldLoaded())
                return;

            PublishRuntimeRace(
                player,
                GetPersistedRace(player.GetPlayerID()));
        }

        public void ApplyIncomingDamageModifiers(Player player, HitData hit)
        {
            if (player == null || hit == null)
                return;

            RaceKind race = GetRace(player);

            if (race == RaceKind.None)
                return;

            HitData.DamageTypes damage = hit.m_damage;

            if (race == RaceKind.Vampire)
            {
                damage.m_poison = 0f;
                damage.m_fire *= VampireFireMultiplier;
                damage.m_spirit *= VampireSpiritMultiplier;
            }
            else if (race == RaceKind.Werewolf)
            {
                damage.m_frost = 0f;
                damage.m_spirit *= WerewolfSpiritMultiplier;
            }
            else if (race == RaceKind.OdinBlessed)
            {
                damage.m_spirit *= OdinSpiritMultiplier;
                damage.m_frost *= OdinFrostMultiplier;
                damage.m_lightning *= OdinLightningMultiplier;
            }

            hit.m_damage = damage;
        }

        private RaceKind GetPersistedRace(long playerId)
        {
            RaceKind race;

            if (_racesByPlayerId.TryGetValue(playerId, out race))
                return race;

            return RaceKind.None;
        }

        private bool EnsureWorldLoaded()
        {
            ZNet currentZNet = ZNet.instance;

            if (currentZNet == null)
            {
                if (!object.ReferenceEquals(_loadedZNetInstance, null))
                    ClearLoadedWorld();

                return false;
            }

            if (object.ReferenceEquals(_loadedZNetInstance, currentZNet) &&
                !string.IsNullOrEmpty(_loadedWorldName) &&
                !string.IsNullOrEmpty(_savePath))
            {
                return true;
            }

            return LoadCurrentWorld(currentZNet);
        }

        private bool LoadCurrentWorld(ZNet currentZNet)
        {
            string worldName = ResolveWorldName(currentZNet);

            if (string.IsNullOrEmpty(worldName))
            {
                ClearLoadedWorld();
                return false;
            }

            _racesByPlayerId.Clear();
            _loadedZNetInstance = currentZNet;
            _loadedWorldName = worldName;
            _savePath = BuildSavePath(worldName);

            if (!File.Exists(_savePath))
            {
                ModLog.Info(
                    "[Races] World race state initialized. World: " +
                    _loadedWorldName +
                    ", races: 0");
                return true;
            }

            try
            {
                string[] lines = File.ReadAllLines(
                    _savePath,
                    System.Text.Encoding.UTF8);

                for (int i = 0; i < lines.Length; i++)
                    TryLoadLine(lines[i]);

                ModLog.Info(
                    "[Races] World race state loaded. World: " +
                    _loadedWorldName +
                    ", races: " +
                    _racesByPlayerId.Count);
            }
            catch (Exception exception)
            {
                ModLog.Warning(
                    "[Races] Failed to load races: " +
                    exception.Message);

                ClearLoadedWorld();
                return false;
            }

            return true;
        }

        private void TryLoadLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            line = line.Trim();

            if (line.StartsWith("#", StringComparison.Ordinal) ||
                line.StartsWith("//", StringComparison.Ordinal))
            {
                return;
            }

            int separator = line.IndexOf('=');

            if (separator <= 0)
                return;

            string playerPart = line.Substring(0, separator).Trim();
            string racePart = line.Substring(separator + 1).Trim();

            long playerId;

            if (!long.TryParse(
                    playerPart,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out playerId))
            {
                return;
            }

            RaceKind race;

            if (!TryParseRace(racePart, out race) ||
                !IsSelectableRace(race))
            {
                return;
            }

            _racesByPlayerId[playerId] = race;
        }

        private void ClearLoadedWorld()
        {
            _racesByPlayerId.Clear();
            _loadedZNetInstance = null;
            _loadedWorldName = "";
            _savePath = "";
        }

        private static string ResolveWorldName(ZNet zNet)
        {
            if (zNet == null || ZNetGetWorldNameMethod == null)
                return null;

            try
            {
                object value = ZNetGetWorldNameMethod.Invoke(
                    zNet,
                    null);

                return SanitizeFileName(value as string);
            }
            catch (Exception exception)
            {
                ModLog.Debug(
                    "[Races] World name resolution failed: " +
                    exception.Message);
                return null;
            }
        }

        private static string BuildSavePath(string worldName)
        {
            string directory = Path.Combine(
                Paths.ConfigPath,
                "ClanTerritory",
                "worlds");

            return Path.Combine(
                directory,
                worldName + FileSuffix);
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim();
            char[] invalid = Path.GetInvalidFileNameChars();

            for (int i = 0; i < invalid.Length; i++)
                value = value.Replace(invalid[i], '_');

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        private static bool TryGetRuntimeRace(
            Player player,
            out RaceKind race)
        {
            race = RaceKind.None;

            ZDO zdo = GetPlayerZdo(player, false);

            if (zdo == null)
                return false;

            int rawValue = zdo.GetInt(
                PlayerRaceZdoKey,
                MissingRuntimeRaceValue);

            if (rawValue == MissingRuntimeRaceValue)
                return false;

            return TryConvertRace(
                rawValue,
                out race);
        }

        private static void PublishRuntimeRace(
            Player player,
            RaceKind race)
        {
            ZDO zdo = GetPlayerZdo(player, true);

            if (zdo == null)
                return;

            zdo.Set(
                PlayerRaceZdoKey,
                (int)race);
        }

        private static ZDO GetPlayerZdo(
            Player player,
            bool requireOwnership)
        {
            if (player == null)
                return null;

            ZNetView zNetView = player.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
                return null;

            if (requireOwnership && !zNetView.IsOwner())
                return null;

            return zNetView.GetZDO();
        }

        private static bool TryConvertRace(
            int value,
            out RaceKind race)
        {
            race = RaceKind.None;

            if (value < (int)RaceKind.None ||
                value > (int)RaceKind.OdinBlessed)
            {
                return false;
            }

            race = (RaceKind)value;
            return true;
        }

        private static bool IsSelectableRace(RaceKind race)
        {
            return race == RaceKind.Werewolf ||
                   race == RaceKind.Vampire ||
                   race == RaceKind.OdinBlessed;
        }

        private void RegisterCommandsOnce()
        {
            if (_commandsRegistered)
                return;

            new Terminal.ConsoleCommand(
                "ctrace",
                "Clan Territory race commands",
                HandleRaceCommand,
                false,
                false,
                false,
                false,
                true);

            new Terminal.ConsoleCommand(
                "ctraces",
                "Clan Territory race commands",
                HandleRaceCommand,
                false,
                false,
                false,
                false,
                true);

            _commandsRegistered = true;
        }

        private object HandleRaceCommand(Terminal.ConsoleEventArgs args)
        {
            if (args == null)
                return false;

            if (args.Length <= 1 || IsHelp(args[1]))
            {
                Reply(args, CtLocalization.Get("ct.race.help"));
                return true;
            }

            string action = args[1].ToLowerInvariant();

            if (action == "status")
                return ShowStatus(args);

            if (action == "choose")
                return ChooseRace(args);

            if (action == "reset")
                return ResetRaceCommand(args);

            Reply(args, CtLocalization.Get("ct.race.help"));
            return true;
        }

        private object ShowStatus(Terminal.ConsoleEventArgs args)
        {
            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Reply(args, CtLocalization.Get("ct.race.no_player"));
                return true;
            }

            if (!EnsureWorldLoaded())
            {
                Reply(args, CtLocalization.Get("ct.race.no_world"));
                return true;
            }

            RaceKind race = GetRace(player);

            if (race == RaceKind.None)
            {
                Reply(args, CtLocalization.Get("ct.race.status_none"));
                return true;
            }

            Reply(
                args,
                CtLocalization.Format(
                    "ct.race.status",
                    FormatRace(race),
                    GetRaceDescription(race)));

            return true;
        }

        private object ChooseRace(Terminal.ConsoleEventArgs args)
        {
            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Reply(args, CtLocalization.Get("ct.race.no_player"));
                return true;
            }

            if (args.Length <= 2)
            {
                Reply(args, CtLocalization.Get("ct.race.choose_usage"));
                return true;
            }

            RaceKind race;

            if (!TryParseRace(args[2], out race) ||
                !IsSelectableRace(race))
            {
                Reply(args, CtLocalization.Get("ct.race.unknown"));
                return true;
            }

            SetRace(
                player,
                race,
                false);

            return true;
        }

        private object ResetRaceCommand(Terminal.ConsoleEventArgs args)
        {
            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Reply(args, CtLocalization.Get("ct.race.no_player"));
                return true;
            }

            ResetRace(player);
            return true;
        }

        private static bool TryParseRace(
            string value,
            out RaceKind race)
        {
            race = RaceKind.None;

            if (string.IsNullOrEmpty(value))
                return false;

            value = value.Trim().ToLowerInvariant();

            if (value == "werewolf" ||
                value == "wolf" ||
                value == "оборотень" ||
                value == "oboroten")
            {
                race = RaceKind.Werewolf;
                return true;
            }

            if (value == "vampire" ||
                value == "vamp" ||
                value == "вампир")
            {
                race = RaceKind.Vampire;
                return true;
            }

            if (value == "odin" ||
                value == "odinblessed" ||
                value == "odin_blessed" ||
                value == "blessed" ||
                value == "один" ||
                value == "благословленный" ||
                value == "благословлённый")
            {
                race = RaceKind.OdinBlessed;
                return true;
            }

            return false;
        }

        private static string FormatRace(RaceKind race)
        {
            if (race == RaceKind.Werewolf)
                return CtLocalization.Get("ct.race.werewolf");

            if (race == RaceKind.Vampire)
                return CtLocalization.Get("ct.race.vampire");

            if (race == RaceKind.OdinBlessed)
                return CtLocalization.Get("ct.race.odin");

            return CtLocalization.Get("ct.race.none");
        }

        private static string GetRaceDescription(RaceKind race)
        {
            if (race == RaceKind.Werewolf)
                return CtLocalization.Get("ct.race.werewolf.desc");

            if (race == RaceKind.Vampire)
                return CtLocalization.Get("ct.race.vampire.desc");

            if (race == RaceKind.OdinBlessed)
                return CtLocalization.Get("ct.race.odin.desc");

            return "";
        }

        private static bool IsHelp(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            value = value.ToLowerInvariant();

            return value == "help" ||
                   value == "?" ||
                   value == "h";
        }

        private static void Reply(
            Terminal.ConsoleEventArgs args,
            string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (args != null && args.Context != null)
                args.Context.AddString(message);

            ShowPlayerMessage(
                Player.m_localPlayer,
                message);
        }

        private static void ShowPlayerMessage(
            Player player,
            string message)
        {
            if (player == null || string.IsNullOrEmpty(message))
                return;

            player.Message(
                MessageHud.MessageType.Center,
                message);
        }

        private static void ReplaceFile(
            string temporaryPath,
            string destinationPath)
        {
            if (!File.Exists(destinationPath))
            {
                File.Move(
                    temporaryPath,
                    destinationPath);
                return;
            }

            try
            {
                File.Replace(
                    temporaryPath,
                    destinationPath,
                    null);
            }
            catch
            {
                File.Copy(
                    temporaryPath,
                    destinationPath,
                    true);
                TryDeleteFile(temporaryPath);
            }
        }

        private static void TryDeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            try
            {
                File.Delete(path);
            }
            catch
            {
            }
        }
    }

    [HarmonyPatch]
    internal static class RaceIncomingDamageHook
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo[] methods =
                typeof(Character).GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                if (method == null || method.Name != "Damage")
                    continue;

                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length != 1 ||
                    parameters[0].ParameterType != typeof(HitData))
                {
                    continue;
                }

                yield return method;
            }
        }

        private static void Prefix(
            Character __instance,
            HitData __0)
        {
            if (__instance == null || __0 == null)
                return;

            Player player = __instance as Player;

            if (player == null)
                return;

            RaceService raceService;

            if (!ServiceContainer.TryGet<RaceService>(out raceService) ||
                raceService == null)
            {
                return;
            }

            raceService.ApplyIncomingDamageModifiers(
                player,
                __0);
        }
    }

    [HarmonyPatch(typeof(Player), "OnSpawned")]
    internal static class RacePlayerSpawnHook
    {
        private static void Postfix(Player __instance)
        {
            if (__instance == null || __instance != Player.m_localPlayer)
                return;

            RaceService raceService;

            if (!ServiceContainer.TryGet<RaceService>(out raceService) ||
                raceService == null)
            {
                return;
            }

            raceService.PublishLocalPlayerRace(__instance);
        }
    }
}
