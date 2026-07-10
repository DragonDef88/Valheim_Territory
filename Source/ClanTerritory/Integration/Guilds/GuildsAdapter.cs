using System;
using System.Collections.Generic;
using System.Reflection;
using ClanTerritory.Core;
using ClanTerritory.Features.Territory;
using ClanTerritory.Utils;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Integration.Guilds
{
    internal sealed class GuildsAdapter : IGuildService
    {
        private const string GuildsAssemblyName = "Guilds";
        private const string GuildsApiTypeName = "Guilds.API";
        private readonly object _syncRoot = new object();

        private Assembly _assembly;
        private Type _apiType;
        private MethodInfo _isLoadedMethod;
        private MethodInfo _getPlayerGuildMethod;
        private MethodInfo _getGuildLeaderMethod;
        private MethodInfo _getGuildByNameMethod;
        private MethodInfo _getGuildIconMethod;
        private bool _resolved;
        private bool _candidateLogWritten;

        public bool IsAvailable
        {
            get
            {
                EnsureResolved();

                if (_apiType == null)
                    return false;

                if (_isLoadedMethod == null)
                    return true;

                try
                {
                    object result = _isLoadedMethod.Invoke(null, null);

                    if (result is bool)
                        return (bool)result;
                }
                catch
                {
                }

                return true;
            }
        }

        public bool TryGetPlayerGuildId(long playerId, out string guildId)
        {
            guildId = null;

            object guild;

            if (!TryGetPlayerGuild(playerId, out guild))
                return false;

            guildId = ExtractGuildId(guild);
            return !string.IsNullOrEmpty(guildId);
        }

        public bool TryGetPlayerGuildName(long playerId, out string guildName)
        {
            guildName = null;

            object guild;

            if (!TryGetPlayerGuild(playerId, out guild))
                return false;

            guildName = ExtractGuildName(guild);
            return !string.IsNullOrEmpty(guildName);
        }

        public bool TryGetPlayerGuildColor(long playerId, out string color)
        {
            color = null;

            object guild;

            if (!TryGetPlayerGuild(playerId, out guild))
                return false;

            color = ExtractGuildColor(guild);
            return !string.IsNullOrEmpty(color);
        }

        public bool TryGetGuildIcon(string guildName, out Sprite sprite)
        {
            sprite = null;

            if (string.IsNullOrEmpty(guildName))
                return false;

            EnsureResolved();

            if (_getGuildByNameMethod == null || _getGuildIconMethod == null)
                return false;

            try
            {
                object guild = _getGuildByNameMethod.Invoke(
                    null,
                    new object[]
                    {
                        guildName
                    });

                if (guild == null)
                    return false;

                sprite = _getGuildIconMethod.Invoke(
                    null,
                    new[]
                    {
                        guild
                    }) as Sprite;

                return sprite != null;
            }
            catch (Exception exception)
            {
                ModLog.Debug("[Guilds] GetGuildIcon failed: " + exception.GetType().Name);
                return false;
            }
        }


        public bool TryGetGuildDescription(string guildName, out string description)
        {
            description = null;

            if (string.IsNullOrEmpty(guildName))
                return false;

            EnsureResolved();

            if (_getGuildByNameMethod == null)
                return false;

            try
            {
                object guild = _getGuildByNameMethod.Invoke(
                    null,
                    new object[]
                    {
                        guildName
                    });

                if (guild == null)
                    return false;

                description = ExtractGuildDescription(guild);
                return !string.IsNullOrEmpty(description);
            }
            catch (Exception exception)
            {
                ModLog.Debug("[Guilds] GetGuildDescription failed: " + exception.GetType().Name);
                return false;
            }
        }

        public bool ArePlayersInSameGuild(long firstPlayerId, long secondPlayerId)
        {
            if (firstPlayerId == secondPlayerId && firstPlayerId != 0L)
                return true;

            string firstGuildId;
            string secondGuildId;

            if (!TryGetPlayerGuildId(firstPlayerId, out firstGuildId))
                return false;

            if (!TryGetPlayerGuildId(secondPlayerId, out secondGuildId))
                return false;

            return !string.IsNullOrEmpty(firstGuildId) &&
                   string.Equals(
                       firstGuildId,
                       secondGuildId,
                       StringComparison.OrdinalIgnoreCase);
        }

        public bool IsPlayerGuildLeader(long playerId)
        {
            object guild;

            if (!TryGetPlayerGuild(playerId, out guild))
                return false;

            object leader = GetGuildLeader(guild);

            if (leader == null)
                return false;

            string leaderName = ExtractNamedMember(
                leader,
                new[]
                {
                    "name",
                    "Name"
                });

            Player player = FindPlayer(playerId);

            if (player == null)
                return false;

            return !string.IsNullOrEmpty(leaderName) &&
                   string.Equals(
                       leaderName,
                       player.GetPlayerName(),
                       StringComparison.OrdinalIgnoreCase);
        }

        private bool TryGetPlayerGuild(long playerId, out object guild)
        {
            guild = null;

            EnsureResolved();

            if (_getPlayerGuildMethod == null)
            {
                WriteCandidateLog();
                return false;
            }

            Player player = FindPlayer(playerId);

            if (player == null)
                return false;

            try
            {
                guild = _getPlayerGuildMethod.Invoke(
                    null,
                    new object[]
                    {
                        player
                    });

                return guild != null;
            }
            catch (Exception exception)
            {
                ModLog.Debug("[Guilds] GetPlayerGuild(Player) failed: " + exception.GetType().Name);
                return false;
            }
        }

        private object GetGuildLeader(object guild)
        {
            EnsureResolved();

            if (guild == null || _getGuildLeaderMethod == null)
                return null;

            try
            {
                return _getGuildLeaderMethod.Invoke(
                    null,
                    new[]
                    {
                        guild
                    });
            }
            catch
            {
                return null;
            }
        }

        private void EnsureResolved()
        {
            if (_resolved)
                return;

            lock (_syncRoot)
            {
                if (_resolved)
                    return;

                _assembly = ResolveGuildsAssembly();

                if (_assembly != null)
                {
                    _apiType = _assembly.GetType(GuildsApiTypeName);

                    if (_apiType != null)
                    {
                        _isLoadedMethod = _apiType.GetMethod(
                            "IsLoaded",
                            BindingFlags.Public | BindingFlags.Static);

                        _getPlayerGuildMethod = _apiType.GetMethod(
                            "GetPlayerGuild",
                            BindingFlags.Public | BindingFlags.Static,
                            null,
                            new[]
                            {
                                typeof(Player)
                            },
                            null);

                        _getGuildByNameMethod = _apiType.GetMethod(
                            "GetGuild",
                            BindingFlags.Public | BindingFlags.Static,
                            null,
                            new[]
                            {
                                typeof(string)
                            },
                            null);

                        _getGuildLeaderMethod = FindGetGuildLeaderMethod(_apiType);
                        _getGuildIconMethod = FindGetGuildIconMethod(_apiType);
                    }
                }

                _resolved = true;

                if (_apiType != null)
                {
                    ModLog.Info(
                        "[Guilds] API connected. GetPlayerGuild(Player): " +
                        (_getPlayerGuildMethod != null) +
                        ", GetGuild(string): " +
                        (_getGuildByNameMethod != null) +
                        ", GetGuildIcon(Guild): " +
                        (_getGuildIconMethod != null));
                }
                else
                {
                    ModLog.Debug("[Guilds] API type was not found.");
                }
            }
        }

        private static Assembly ResolveGuildsAssembly()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];

                if (assembly == null)
                    continue;

                AssemblyName name = assembly.GetName();

                if (name == null)
                    continue;

                if (string.Equals(
                        name.Name,
                        GuildsAssemblyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return assembly;
                }
            }

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];

                if (assembly == null)
                    continue;

                AssemblyName name = assembly.GetName();

                if (name == null)
                    continue;

                if (ContainsIgnoreCase(name.Name, "Guild"))
                    return assembly;
            }

            return null;
        }

        private static MethodInfo FindGetGuildLeaderMethod(Type apiType)
        {
            if (apiType == null)
                return null;

            MethodInfo[] methods =
                apiType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                if (method.Name != "GetGuildLeader")
                    continue;

                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 1)
                    return method;
            }

            return null;
        }

        private static MethodInfo FindGetGuildIconMethod(Type apiType)
        {
            if (apiType == null)
                return null;

            MethodInfo[] methods =
                apiType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                if (method.Name != "GetGuildIcon")
                    continue;

                if (!typeof(Sprite).IsAssignableFrom(method.ReturnType))
                    continue;

                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 1)
                    return method;
            }

            return null;
        }

        private static Player FindPlayer(long playerId)
        {
            if (playerId == 0L)
                return null;

            if (Player.m_localPlayer != null &&
                Player.m_localPlayer.GetPlayerID() == playerId)
            {
                return Player.m_localPlayer;
            }

            Player[] players = UnityEngine.Object.FindObjectsByType<Player>(UnityEngine.FindObjectsSortMode.None);

            for (int i = 0; i < players.Length; i++)
            {
                Player player = players[i];

                if (player == null)
                    continue;

                if (player.GetPlayerID() == playerId)
                    return player;
            }

            return null;
        }

        private static string ExtractGuildId(object guild)
        {
            if (guild == null)
                return null;

            object general = ExtractObjectMember(
                guild,
                "General");

            if (general != null)
            {
                string id = ExtractNamedMember(
                    general,
                    new[]
                    {
                        "id",
                        "Id",
                        "ID"
                    });

                if (!string.IsNullOrEmpty(id))
                    return id;
            }

            string name = ExtractGuildName(guild);

            if (!string.IsNullOrEmpty(name))
                return name;

            return guild.ToString();
        }

        private static string ExtractGuildName(object guild)
        {
            if (guild == null)
                return null;

            string name = ExtractNamedMember(
                guild,
                new[]
                {
                    "Name",
                    "name"
                });

            if (!string.IsNullOrEmpty(name))
                return name;

            return guild.ToString();
        }

        private static string ExtractGuildDescription(object guild)
        {
            if (guild == null)
                return null;

            string description =
                ExtractNamedMember(
                    guild,
                    new[]
                    {
                        "Description",
                        "description",
                        "Desc",
                        "desc",
                        "About",
                        "about",
                        "Info",
                        "info",
                        "Motto",
                        "motto",
                        "Bio",
                        "bio"
                    });

            if (!string.IsNullOrEmpty(description))
                return NormalizeDescription(description);

            object general =
                ExtractObjectMember(
                    guild,
                    "General");

            if (general != null)
            {
                description =
                    ExtractNamedMember(
                        general,
                        new[]
                        {
                            "description",
                            "Description",
                            "desc",
                            "Desc",
                            "about",
                            "About",
                            "info",
                            "Info",
                            "motto",
                            "Motto",
                            "bio",
                            "Bio"
                        });

                if (!string.IsNullOrEmpty(description))
                    return NormalizeDescription(description);
            }

            return null;
        }

        private static string NormalizeDescription(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            string normalized =
                value
                    .Replace("\\r\\n", "\n")
                    .Replace("\\n", "\n")
                    .Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Trim();

            return string.IsNullOrEmpty(normalized)
                ? null
                : normalized;
        }

        private static string ExtractGuildColor(object guild)
        {
            if (guild == null)
                return null;

            object general = ExtractObjectMember(
                guild,
                "General");

            if (general == null)
                return null;

            return ExtractNamedMember(
                general,
                new[]
                {
                    "color",
                    "Color",
                    "guild color",
                    "GuildColor"
                });
        }

        private static object ExtractObjectMember(object value, string name)
        {
            if (value == null || string.IsNullOrEmpty(name))
                return null;

            Type type = value.GetType();
            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            PropertyInfo property = type.GetProperty(name, flags);

            if (property != null)
            {
                try
                {
                    return property.GetValue(value, null);
                }
                catch
                {
                }
            }

            FieldInfo field = type.GetField(name, flags);

            if (field != null)
            {
                try
                {
                    return field.GetValue(value);
                }
                catch
                {
                }
            }

            return null;
        }

        private static string ExtractNamedMember(object value, string[] names)
        {
            if (value == null || names == null)
                return null;

            Type type = value.GetType();
            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            for (int i = 0; i < names.Length; i++)
            {
                PropertyInfo property = type.GetProperty(names[i], flags);

                if (property != null)
                {
                    try
                    {
                        object raw = property.GetValue(value, null);

                        if (raw != null)
                            return raw.ToString();
                    }
                    catch
                    {
                    }
                }

                FieldInfo field = type.GetField(names[i], flags);

                if (field != null)
                {
                    try
                    {
                        object raw = field.GetValue(value);

                        if (raw != null)
                            return raw.ToString();
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        private static bool ContainsIgnoreCase(string value, string part)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(part))
                return false;

            return value.IndexOf(
                       part,
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void WriteCandidateLog()
        {
            if (_candidateLogWritten)
                return;

            _candidateLogWritten = true;

            if (_apiType == null)
                return;

            MethodInfo[] methods =
                _apiType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                ModLog.Debug(
                    "[Guilds] API method: " +
                    method.Name +
                    " returns " +
                    method.ReturnType.FullName +
                    " params " +
                    method.GetParameters().Length);
            }
        }
    }

    internal static class TerritoryGuildAccess
    {
        private static readonly FieldInfo AllPrivateAreasField =
            AccessTools.Field(typeof(PrivateArea), "m_allAreas");

        public static bool SyncWardGuildFromPlayer(
            PrivateArea privateArea,
            Player player,
            bool clearCreatorGuildWhenMissing)
        {
            if (privateArea == null || player == null)
                return false;

            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return false;

            long creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);
            long playerId = player.GetPlayerID();

            bool isCreator = creatorId == playerId;
            bool hasExistingGuildAccess = HasGuildAccess(privateArea, player);

            if (!isCreator && !hasExistingGuildAccess)
                return false;

            IGuildService guildService;

            if (!ServiceContainer.TryGet<IGuildService>(out guildService) ||
                guildService == null ||
                !guildService.IsAvailable)
            {
                return false;
            }

            string guildId;
            string guildName;
            string guildColor;

            if (!guildService.TryGetPlayerGuildId(playerId, out guildId))
            {
                if (isCreator && clearCreatorGuildWhenMissing)
                    ClearWardGuild(zdo);

                return false;
            }

            guildService.TryGetPlayerGuildName(playerId, out guildName);
            guildService.TryGetPlayerGuildColor(playerId, out guildColor);

            zdo.Set(TerritoryZdoKeys.WardGuildId, guildId ?? "");
            zdo.Set(TerritoryZdoKeys.WardGuildName, guildName ?? "");
            zdo.Set(TerritoryZdoKeys.WardGuildColor, guildColor ?? "");

            ModLog.Info(
                "[Guilds] Ward guild synced. Guild: " +
                (string.IsNullOrEmpty(guildName) ? guildId : guildName) +
                ", ward: " +
                zdo.m_uid);

            return true;
        }

        public static bool HasGuildAccess(
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null || player == null)
                return false;

            ZDO zdo = GetZdo(privateArea);

            if (zdo == null)
                return false;

            return HasGuildAccess(
                zdo,
                player.GetPlayerID());
        }

        public static bool HasGuildAccess(
            long creatorId,
            long playerId)
        {
            if (creatorId == 0L || playerId == 0L || creatorId == playerId)
                return false;

            IGuildService guildService;

            if (!ServiceContainer.TryGet<IGuildService>(out guildService) ||
                guildService == null ||
                !guildService.IsAvailable)
            {
                return false;
            }

            return guildService.ArePlayersInSameGuild(
                creatorId,
                playerId);
        }

        public static bool HasGuildAccess(
            ZDO wardZdo,
            long playerId)
        {
            if (wardZdo == null || playerId == 0L)
                return false;

            long creatorId = wardZdo.GetLong(ZDOVars.s_creator, 0L);

            if (creatorId == playerId)
                return false;

            IGuildService guildService;

            if (!ServiceContainer.TryGet<IGuildService>(out guildService) ||
                guildService == null ||
                !guildService.IsAvailable)
            {
                return false;
            }

            string wardGuildId = wardZdo.GetString(
                TerritoryZdoKeys.WardGuildId,
                "");

            if (!string.IsNullOrEmpty(wardGuildId))
            {
                string playerGuildId;

                if (!guildService.TryGetPlayerGuildId(playerId, out playerGuildId))
                    return false;

                return string.Equals(
                    wardGuildId,
                    playerGuildId,
                    StringComparison.OrdinalIgnoreCase);
            }

            return guildService.ArePlayersInSameGuild(
                creatorId,
                playerId);
        }

        public static bool HasGuildAccessAt(
            Vector3 position,
            Player player)
        {
            if (player == null)
                return false;

            PrivateArea privateArea = FindPrivateAreaAt(position);

            if (privateArea == null)
                return false;

            return HasGuildAccess(privateArea, player);
        }

        public static bool TryGetWardGuildName(
            string wardId,
            out string guildName)
        {
            guildName = null;

            ZDO zdo = GetWardZdo(wardId);

            if (zdo == null)
                return false;

            guildName = zdo.GetString(
                TerritoryZdoKeys.WardGuildName,
                "");

            return !string.IsNullOrEmpty(guildName);
        }

        public static bool TryGetWardGuildId(
            string wardId,
            out string guildId)
        {
            guildId = null;

            ZDO zdo = GetWardZdo(wardId);

            if (zdo == null)
                return false;

            guildId = zdo.GetString(
                TerritoryZdoKeys.WardGuildId,
                "");

            return !string.IsNullOrEmpty(guildId);
        }

        public static bool TryGetWardGuildColor(
            string wardId,
            out Color color)
        {
            color = Color.clear;

            ZDO zdo = GetWardZdo(wardId);

            if (zdo == null)
                return false;

            string guildColor = zdo.GetString(
                TerritoryZdoKeys.WardGuildColor,
                "");

            if (string.IsNullOrEmpty(guildColor))
                return false;

            return ColorUtility.TryParseHtmlString(
                guildColor,
                out color);
        }

        public static bool IsLocalPlayerInWardGuild(string wardId)
        {
            if (Player.m_localPlayer == null)
                return false;

            string wardGuildId;

            if (!TryGetWardGuildId(wardId, out wardGuildId))
                return false;

            IGuildService guildService;

            if (!ServiceContainer.TryGet<IGuildService>(out guildService) ||
                guildService == null ||
                !guildService.IsAvailable)
            {
                return false;
            }

            string localGuildId;

            if (!guildService.TryGetPlayerGuildId(
                    Player.m_localPlayer.GetPlayerID(),
                    out localGuildId))
            {
                return false;
            }

            return string.Equals(
                wardGuildId,
                localGuildId,
                StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryGetWardGuildIcon(
            string wardId,
            out Sprite sprite)
        {
            sprite = null;

            string guildName;

            if (!TryGetWardGuildName(
                    wardId,
                    out guildName))
            {
                return false;
            }

            IGuildService guildService;

            if (!ServiceContainer.TryGet<IGuildService>(out guildService) ||
                guildService == null ||
                !guildService.IsAvailable)
            {
                return false;
            }

            return guildService.TryGetGuildIcon(
                guildName,
                out sprite);
        }

        public static PrivateArea FindPrivateAreaByWardId(string wardId)
        {
            if (string.IsNullOrEmpty(wardId))
                return null;

            List<PrivateArea> areas = GetPrivateAreas();

            for (int i = 0; i < areas.Count; i++)
            {
                PrivateArea privateArea = areas[i];
                ZDO zdo = GetZdo(privateArea);

                if (zdo == null)
                    continue;

                if (zdo.m_uid.ToString() == wardId)
                    return privateArea;
            }

            return null;
        }

        private static PrivateArea FindPrivateAreaAt(Vector3 position)
        {
            List<PrivateArea> areas = GetPrivateAreas();

            for (int i = 0; i < areas.Count; i++)
            {
                PrivateArea privateArea = areas[i];

                if (privateArea == null)
                    continue;

                if (global::Utils.DistanceXZ(
                        privateArea.transform.position,
                        position) < privateArea.m_radius)
                {
                    return privateArea;
                }
            }

            return null;
        }

        private static ZDO GetWardZdo(string wardId)
        {
            PrivateArea privateArea = FindPrivateAreaByWardId(wardId);

            return GetZdo(privateArea);
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

        private static void ClearWardGuild(ZDO zdo)
        {
            if (zdo == null)
                return;

            zdo.Set(TerritoryZdoKeys.WardGuildId, "");
            zdo.Set(TerritoryZdoKeys.WardGuildName, "");
            zdo.Set(TerritoryZdoKeys.WardGuildColor, "");
        }

        private static List<PrivateArea> GetPrivateAreas()
        {
            if (AllPrivateAreasField == null)
                return new List<PrivateArea>();

            List<PrivateArea> areas =
                AllPrivateAreasField.GetValue(null) as List<PrivateArea>;

            if (areas == null)
                return new List<PrivateArea>();

            return areas;
        }
    }
}
