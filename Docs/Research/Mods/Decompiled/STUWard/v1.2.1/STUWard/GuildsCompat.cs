using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using Splatform;
using UnityEngine;

namespace STUWard;

internal static class GuildsCompat
{
	private enum AvailabilityState
	{
		Unknown,
		Available,
		Unavailable
	}

	private sealed class PendingWardGuildProjectionRefreshState
	{
		internal readonly Dictionary<long, WardGuildCharacterIdentity> TargetIdentitiesByPlayerId = new Dictionary<long, WardGuildCharacterIdentity>();

		internal readonly Dictionary<string, WardGuildCharacterIdentity> TargetIdentitiesByCharacterKey = new Dictionary<string, WardGuildCharacterIdentity>(StringComparer.Ordinal);

		internal readonly HashSet<int> AffectedGuildIds = new HashSet<int>();

		internal bool PendingFullRefresh;

		internal bool PendingLiveDisplayRefresh;

		internal string PendingLiveDisplayReason = string.Empty;

		internal DateTime FlushAtUtc = DateTime.MinValue;
	}

	private const string GuildIdKey = "stuw_guild_id";

	private const string GuildNameKey = "stuw_guild_name";

	private static readonly TimeSpan GuildLookupCacheDuration = TimeSpan.FromSeconds(30.0);

	private const string SyncPlayerGuildRpc = "STUWard_SyncPlayerGuild";

	private static readonly TimeSpan PendingPlayerGuildSyncLifetime = TimeSpan.FromSeconds(15.0);

	private static readonly Dictionary<long, SyncedWardGuildIdentity> ServerSyncedGuildByPlayerId = new Dictionary<long, SyncedWardGuildIdentity>();

	private static readonly Dictionary<string, SyncedWardGuildIdentity> ServerSyncedGuildByCharacterKey = new Dictionary<string, SyncedWardGuildIdentity>(StringComparer.Ordinal);

	private static readonly Dictionary<long, PendingPlayerGuildSync> PendingPlayerGuildSyncsBySender = new Dictionary<long, PendingPlayerGuildSync>();

	private static bool _syncRpcsRegistered;

	private static bool _localGuildSyncPending = true;

	private static long _lastSyncedLocalPlayerId;

	private static int _lastSyncedLocalGuildId = int.MinValue;

	private static string _lastSyncedLocalGuildName = string.Empty;

	private const string GuildsPluginGuid = "org.bepinex.plugins.guilds";

	private static readonly TimeSpan AvailabilityProbeBackoff = TimeSpan.FromSeconds(2.0);

	private static readonly Assembly? GuildsAssembly = GetPluginAssembly("org.bepinex.plugins.guilds");

	private static readonly Type? ApiType = GuildsAssembly?.GetType("Guilds.API");

	private static readonly Type? GuildType = GuildsAssembly?.GetType("Guilds.Guild");

	private static readonly Type? GuildGeneralType = GuildsAssembly?.GetType("Guilds.GuildGeneral");

	private static readonly Type? PlayerReferenceType = GuildsAssembly?.GetType("Guilds.PlayerReference");

	private static readonly Type? GuildJoinedDelegateType = ApiType?.GetNestedType("GuildJoined", BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly Type? GuildLeftDelegateType = ApiType?.GetNestedType("GuildLeft", BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly Type? GuildCreatedDelegateType = ApiType?.GetNestedType("GuildCreated", BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly Type? GuildDeletedDelegateType = ApiType?.GetNestedType("GuildDeleted", BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly MethodInfo? IsLoadedMethod = ((ApiType != null) ? AccessTools.Method(ApiType, "IsLoaded", (Type[])null, (Type[])null) : null);

	private static readonly MethodInfo? GetPlayerGuildByPlayerMethod = ((ApiType != null) ? AccessTools.Method(ApiType, "GetPlayerGuild", new Type[1] { typeof(Player) }, (Type[])null) : null);

	private static readonly MethodInfo? GetPlayerGuildByReferenceMethod = ((ApiType != null && PlayerReferenceType != null) ? AccessTools.Method(ApiType, "GetPlayerGuild", new Type[1] { PlayerReferenceType }, (Type[])null) : null);

	private static readonly MethodInfo? GetGuildsMethod = ((ApiType != null) ? AccessTools.Method(ApiType, "GetGuilds", Type.EmptyTypes, (Type[])null) : null);

	private static readonly MethodInfo? GetGuildByIdMethod = ((ApiType != null) ? AccessTools.Method(ApiType, "GetGuild", new Type[1] { typeof(int) }, (Type[])null) : null);

	private static readonly MethodInfo? PlayerReferenceFromStringMethod = ((PlayerReferenceType != null) ? AccessTools.Method(PlayerReferenceType, "fromString", new Type[1] { typeof(string) }, (Type[])null) : null);

	private static readonly MethodInfo? RegisterOnGuildJoinedMethod = ((ApiType != null && GuildJoinedDelegateType != null) ? AccessTools.Method(ApiType, "RegisterOnGuildJoined", new Type[1] { GuildJoinedDelegateType }, (Type[])null) : null);

	private static readonly MethodInfo? RegisterOnGuildLeftMethod = ((ApiType != null && GuildLeftDelegateType != null) ? AccessTools.Method(ApiType, "RegisterOnGuildLeft", new Type[1] { GuildLeftDelegateType }, (Type[])null) : null);

	private static readonly MethodInfo? RegisterOnGuildCreatedMethod = ((ApiType != null && GuildCreatedDelegateType != null) ? AccessTools.Method(ApiType, "RegisterOnGuildCreated", new Type[1] { GuildCreatedDelegateType }, (Type[])null) : null);

	private static readonly MethodInfo? RegisterOnGuildDeletedMethod = ((ApiType != null && GuildDeletedDelegateType != null) ? AccessTools.Method(ApiType, "RegisterOnGuildDeleted", new Type[1] { GuildDeletedDelegateType }, (Type[])null) : null);

	private static readonly MethodInfo? SaveGuildMethod = ((ApiType != null && GuildType != null) ? AccessTools.Method(ApiType, "SaveGuild", new Type[1] { GuildType }, (Type[])null) : null);

	private static readonly FieldInfo? PlayerInfoUserInfoField = AccessTools.Field(typeof(PlayerInfo), "m_userInfo");

	private static readonly FieldInfo? UserInfoIdField = ((PlayerInfoUserInfoField?.FieldType != null) ? AccessTools.Field(PlayerInfoUserInfoField.FieldType, "m_id") : null);

	private static readonly FieldInfo? GuildNameField = ((GuildType != null) ? AccessTools.Field(GuildType, "Name") : null);

	private static readonly FieldInfo? GuildGeneralField = ((GuildType != null) ? AccessTools.Field(GuildType, "General") : null);

	private static readonly FieldInfo? GuildGeneralIdField = ((GuildGeneralType != null) ? AccessTools.Field(GuildGeneralType, "id") : null);

	private static readonly FieldInfo? GuildMembersField = ((GuildType != null) ? AccessTools.Field(GuildType, "Members") : null);

	private static readonly FieldInfo? PlayerReferenceIdField = ((PlayerReferenceType != null) ? AccessTools.Field(PlayerReferenceType, "id") : null);

	private static readonly FieldInfo? PlayerReferenceNameField = ((PlayerReferenceType != null) ? (AccessTools.Field(PlayerReferenceType, "name") ?? AccessTools.Field(PlayerReferenceType, "Name")) : null);

	private static readonly bool HasGuildsApiSurface = ApiType != null && IsLoadedMethod != null;

	private static AvailabilityState _availabilityState = AvailabilityState.Unknown;

	private static DateTime _nextAvailabilityProbeUtc = DateTime.MinValue;

	private static readonly TimeSpan PendingWardGuildProjectionRefreshDebounce = TimeSpan.FromMilliseconds(250.0);

	private static readonly PendingWardGuildProjectionRefreshState PendingWardGuildProjectionRefresh = new PendingWardGuildProjectionRefreshState();

	private static readonly Dictionary<long, CachedWardGuildIdentity> PlayerGuildCache = new Dictionary<long, CachedWardGuildIdentity>();

	private static readonly Dictionary<long, CachedPlayerPlatformIdentity> PlayerPlatformIdCache = new Dictionary<long, CachedPlayerPlatformIdentity>();

	private static bool _guildHooksRegistered;

	private static bool _guildHooksActive;

	private static bool _saveGuildPatched;

	internal static void ResetRuntimeState()
	{
		PlayerGuildCache.Clear();
		PlayerPlatformIdCache.Clear();
		ResetPendingWardGuildProjectionRefreshes();
		ResetSyncedGuildState();
		_availabilityState = AvailabilityState.Unknown;
		_nextAvailabilityProbeUtc = DateTime.MinValue;
	}

	internal static void EnsureRuntimeBindings()
	{
		RegisterSyncRpcs();
	}

	internal static void OnZNetAwake()
	{
		ResetRuntimeState();
		EnsureRuntimeBindings();
	}

	internal static WardGuildIdentity GetPlayerGuildIdentity(Player? player)
	{
		if (!TryGetGuild(player, out var guild))
		{
			return default(WardGuildIdentity);
		}
		return guild;
	}

	internal static WardGuildIdentity GetPlayerGuildIdentity(long playerId)
	{
		if (!TryGetGuild(playerId, out var guild))
		{
			return default(WardGuildIdentity);
		}
		return guild;
	}

	internal static WardGuildIdentity GetWardGuildIdentity(PrivateArea? area)
	{
		return new WardGuildIdentity(GetWardGuildId(area), GetWardGuildName(area));
	}

	internal static WardGuildIdentity GetWardGuildIdentity(ZDO? zdo)
	{
		return new WardGuildIdentity(GetWardGuildId(zdo), GetWardGuildName(zdo));
	}

	internal static int GetPlayerGuildId(Player? player)
	{
		if (!TryGetGuild(player, out var guild))
		{
			return 0;
		}
		return guild.Id;
	}

	internal static int GetPlayerGuildId(long playerId)
	{
		if (!TryGetGuild(playerId, out var guild))
		{
			return 0;
		}
		return guild.Id;
	}

	internal static string GetPlayerGuildName(long playerId)
	{
		if (!TryGetGuild(playerId, out var guild))
		{
			return string.Empty;
		}
		return guild.Name;
	}

	internal static int GetWardGuildId(ZDO? zdo)
	{
		if (zdo == null)
		{
			return 0;
		}
		return zdo.GetInt("stuw_guild_id", 0);
	}

	internal static string GetWardGuildName(ZDO? zdo)
	{
		return ((zdo != null) ? zdo.GetString("stuw_guild_name", string.Empty) : null) ?? string.Empty;
	}

	internal static string BuildCharacterIdentityKey(string accountId, string playerName)
	{
		string text = WardOwnership.NormalizeAccountIdValue(accountId);
		string text2 = playerName?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(text2))
		{
			return string.Empty;
		}
		return text + "\n" + text2;
	}

	internal static void Update()
	{
		SyncLocalPlayerGuildIfNeeded(force: false);
		ProcessPendingPlayerGuildSyncs();
		ProcessPendingWardGuildProjectionRefreshes();
	}

	internal static void OnLocalPlayerStarted(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			_localGuildSyncPending = true;
			SyncLocalPlayerGuildIfNeeded(force: true);
		}
	}

	internal static void ResetSyncedGuildState()
	{
		ServerSyncedGuildByPlayerId.Clear();
		ServerSyncedGuildByCharacterKey.Clear();
		PendingPlayerGuildSyncsBySender.Clear();
		_syncRpcsRegistered = false;
		_localGuildSyncPending = true;
		_lastSyncedLocalPlayerId = 0L;
		_lastSyncedLocalGuildId = int.MinValue;
		_lastSyncedLocalGuildName = string.Empty;
	}

	internal static bool TryGetSyncedGuildIdentity(long playerId, string accountId, string playerName, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if (playerId != 0L && ServerSyncedGuildByPlayerId.TryGetValue(playerId, out var value))
		{
			guild = (value.HasGuild ? new WardGuildIdentity(value.GuildId, value.GuildName) : default(WardGuildIdentity));
			return true;
		}
		string text = BuildCharacterIdentityKey(accountId, playerName);
		if (string.IsNullOrWhiteSpace(text) || !ServerSyncedGuildByCharacterKey.TryGetValue(text, out var value2))
		{
			return false;
		}
		guild = (value2.HasGuild ? new WardGuildIdentity(value2.GuildId, value2.GuildName) : default(WardGuildIdentity));
		return true;
	}

	internal static bool TryGetSyncedGuildIdentity(string accountId, string playerName, out WardGuildIdentity guild)
	{
		return TryGetSyncedGuildIdentity(0L, accountId, playerName, out guild);
	}

	private static void RegisterSyncRpcs()
	{
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (!_syncRpcsRegistered && instance != null)
		{
			instance.Register<ZPackage>("STUWard_SyncPlayerGuild", (Action<long, ZPackage>)HandleSyncPlayerGuild);
			_syncRpcsRegistered = true;
		}
	}

	private static void SyncLocalPlayerGuildIfNeeded(bool force)
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		Player localPlayer = Player.m_localPlayer;
		ZNet instance = ZNet.instance;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)instance == (Object)null)
		{
			return;
		}
		long playerID = localPlayer.GetPlayerID();
		string playerAccountId = WardOwnership.GetPlayerAccountId(localPlayer);
		if (playerID == 0L || string.IsNullOrWhiteSpace(playerAccountId))
		{
			return;
		}
		WardGuildIdentity playerGuildIdentity = GetPlayerGuildIdentity(localPlayer);
		string text = playerGuildIdentity.Name ?? string.Empty;
		bool flag = _localGuildSyncPending || playerID != _lastSyncedLocalPlayerId || playerGuildIdentity.Id != _lastSyncedLocalGuildId || !string.Equals(text, _lastSyncedLocalGuildName, StringComparison.Ordinal);
		if (!force && !flag)
		{
			return;
		}
		_lastSyncedLocalPlayerId = playerID;
		_lastSyncedLocalGuildId = playerGuildIdentity.Id;
		_lastSyncedLocalGuildName = text;
		_localGuildSyncPending = false;
		if (instance.IsServer())
		{
			if (UpsertSyncedGuildIdentity(playerID, playerAccountId, localPlayer.GetPlayerName(), playerGuildIdentity, out var previousGuild))
			{
				RefreshWardGuildProjectionForCharacter(new WardGuildCharacterIdentity(playerID, playerAccountId, localPlayer.GetPlayerName()), liveDisplayRefresh: true, playerGuildIdentity.Id, previousGuild.Id);
			}
			return;
		}
		ZRoutedRpc instance2 = ZRoutedRpc.instance;
		if (instance2 != null)
		{
			long serverPeerID = instance2.GetServerPeerID();
			if (serverPeerID != 0L)
			{
				ZPackage val = new ZPackage();
				val.Write(playerGuildIdentity.Id);
				val.Write(text);
				instance2.InvokeRoutedRPC(serverPeerID, "STUWard_SyncPlayerGuild", new object[1] { val });
			}
		}
	}

	private static void HandleSyncPlayerGuild(long sender, ZPackage pkg)
	{
		if (!((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			int guildId = pkg.ReadInt();
			string guildName = pkg.ReadString();
			if (!TryApplySyncedGuildIdentity(sender, guildId, guildName))
			{
				PendingPlayerGuildSyncsBySender[sender] = new PendingPlayerGuildSync(sender, guildId, guildName, DateTime.UtcNow);
			}
		}
	}

	private static void ProcessPendingPlayerGuildSyncs()
	{
		if (PendingPlayerGuildSyncsBySender.Count == 0 || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		List<long> list = null;
		List<long> list2 = null;
		DateTime utcNow = DateTime.UtcNow;
		foreach (KeyValuePair<long, PendingPlayerGuildSync> item in PendingPlayerGuildSyncsBySender)
		{
			if (utcNow - item.Value.FirstSeenUtc > PendingPlayerGuildSyncLifetime)
			{
				if (list == null)
				{
					list = new List<long>();
				}
				list.Add(item.Key);
			}
			else if (TryApplySyncedGuildIdentity(item.Value.SenderUid, item.Value.GuildId, item.Value.GuildName))
			{
				if (list2 == null)
				{
					list2 = new List<long>();
				}
				list2.Add(item.Key);
			}
		}
		if (list != null)
		{
			foreach (long item2 in list)
			{
				PendingPlayerGuildSyncsBySender.Remove(item2);
			}
		}
		if (list2 == null)
		{
			return;
		}
		foreach (long item3 in list2)
		{
			PendingPlayerGuildSyncsBySender.Remove(item3);
		}
	}

	private static bool TryApplySyncedGuildIdentity(long sender, int guildId, string guildName)
	{
		if (!WardOwnership.TryResolveAuthoritativePlayerIdFromSender(sender, "GuildsCompat.Sync", out var playerId))
		{
			return false;
		}
		string text = WardOwnership.GetAuthoritativeAccountIdFromSender(sender, playerId);
		if (string.IsNullOrWhiteSpace(text))
		{
			text = WardOwnership.GetPlayerAccountId(playerId);
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		WardGuildIdentity guild = ((guildId != 0) ? new WardGuildIdentity(guildId, guildName) : default(WardGuildIdentity));
		string playerName = WardOwnership.GetPlayerName(playerId);
		if (UpsertSyncedGuildIdentity(playerId, text, playerName, guild, out var previousGuild))
		{
			WardOwnership.RefreshServerPlayerAccountIdForResolvedPlayer(playerId, text);
			RefreshWardGuildProjectionForCharacter(new WardGuildCharacterIdentity(playerId, text, playerName), liveDisplayRefresh: true, guild.Id, previousGuild.Id);
		}
		return true;
	}

	private static void NotifyGuildProjectionRefreshApplied(string reason, bool fullRefresh, HashSet<long>? targetPlayerIds, HashSet<string>? targetCharacterKeys, HashSet<int>? affectedGuildIds)
	{
		string reason2 = (string.IsNullOrWhiteSpace(reason) ? "guild projection refreshed" : reason);
		HashSet<long> hashSet = null;
		if (!fullRefresh)
		{
			if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
			{
				return;
			}
			List<ZNetPeer> peers = ZNet.instance.GetPeers();
			if (peers == null)
			{
				return;
			}
			hashSet = CollectGuildProjectionRefreshRecipients(peers, targetPlayerIds, targetCharacterKeys, affectedGuildIds);
			if (hashSet.Count == 0)
			{
				return;
			}
		}
		ManagedWardMapStateService.NotifyViewerProjectionChanged(reason2, fullRefresh, hashSet, refreshImmediatelyIfVisible: true);
	}

	private static HashSet<long> CollectGuildProjectionRefreshRecipients(List<ZNetPeer> peers, HashSet<long>? targetPlayerIds, HashSet<string>? targetCharacterKeys, HashSet<int>? affectedGuildIds)
	{
		HashSet<long> hashSet = new HashSet<long>();
		for (int i = 0; i < peers.Count; i++)
		{
			ZNetPeer val = peers[i];
			if (val != null && val.m_uid != 0L)
			{
				long playerIdFromSender = WardOwnership.GetPlayerIdFromSender(val.m_uid);
				string authoritativeAccountIdFromSender = WardOwnership.GetAuthoritativeAccountIdFromSender(val.m_uid, playerIdFromSender);
				string playerName = ((playerIdFromSender != 0L) ? WardOwnership.GetPlayerName(playerIdFromSender) : string.Empty);
				if (ShouldReceiveGuildProjectionRefresh(playerIdFromSender, authoritativeAccountIdFromSender, playerName, targetPlayerIds, targetCharacterKeys, affectedGuildIds))
				{
					hashSet.Add(val.m_uid);
				}
			}
		}
		return hashSet;
	}

	private static bool ShouldReceiveGuildProjectionRefresh(long playerId, string accountId, string playerName, HashSet<long>? targetPlayerIds, HashSet<string>? targetCharacterKeys, HashSet<int>? affectedGuildIds)
	{
		if (playerId != 0L && WardAdminDebugAccess.IsPlayerAdminDebugController(playerId))
		{
			return true;
		}
		if (targetPlayerIds != null && targetPlayerIds.Count > 0 && playerId != 0L && targetPlayerIds.Contains(playerId))
		{
			return true;
		}
		if (targetCharacterKeys != null && targetCharacterKeys.Count > 0)
		{
			string text = BuildCharacterIdentityKey(accountId, playerName);
			if (!string.IsNullOrWhiteSpace(text) && targetCharacterKeys.Contains(text))
			{
				return true;
			}
		}
		if (affectedGuildIds == null || affectedGuildIds.Count == 0)
		{
			return false;
		}
		if (TryGetSyncedGuildIdentity(playerId, accountId, playerName, out var guild) && guild.Id != 0)
		{
			return affectedGuildIds.Contains(guild.Id);
		}
		return false;
	}

	private static bool UpsertSyncedGuildIdentity(long playerId, string accountId, string playerName, WardGuildIdentity guild, out WardGuildIdentity previousGuild)
	{
		previousGuild = default(WardGuildIdentity);
		bool result = false;
		SyncedWardGuildIdentity value = new SyncedWardGuildIdentity(guild.Id != 0, guild.Id, guild.Name);
		if (playerId != 0L)
		{
			if (ServerSyncedGuildByPlayerId.TryGetValue(playerId, out var value2))
			{
				previousGuild = ToWardGuildIdentity(value2);
			}
			if (!ServerSyncedGuildByPlayerId.TryGetValue(playerId, out value2) || value2.HasGuild != value.HasGuild || value2.GuildId != value.GuildId || !string.Equals(value2.GuildName, value.GuildName, StringComparison.Ordinal))
			{
				ServerSyncedGuildByPlayerId[playerId] = value;
				result = true;
			}
		}
		string text = BuildCharacterIdentityKey(accountId, playerName);
		if (string.IsNullOrWhiteSpace(text))
		{
			return result;
		}
		if (previousGuild.Id == 0 && ServerSyncedGuildByCharacterKey.TryGetValue(text, out var value3))
		{
			previousGuild = ToWardGuildIdentity(value3);
		}
		if (!ServerSyncedGuildByCharacterKey.TryGetValue(text, out var value4) || value4.HasGuild != value.HasGuild || value4.GuildId != value.GuildId || !string.Equals(value4.GuildName, value.GuildName, StringComparison.Ordinal))
		{
			ServerSyncedGuildByCharacterKey[text] = value;
			result = true;
		}
		return result;
	}

	private static WardGuildIdentity ToWardGuildIdentity(SyncedWardGuildIdentity syncedGuild)
	{
		if (!syncedGuild.HasGuild)
		{
			return default(WardGuildIdentity);
		}
		return new WardGuildIdentity(syncedGuild.GuildId, syncedGuild.GuildName);
	}

	private static Assembly? GetPluginAssembly(string pluginGuid)
	{
		if (!Chainloader.PluginInfos.TryGetValue(pluginGuid, out var value))
		{
			return null;
		}
		return ((object)value.Instance)?.GetType().Assembly;
	}

	internal static bool IsAvailable()
	{
		if (!HasGuildsApiSurface)
		{
			return false;
		}
		if (_availabilityState == AvailabilityState.Available)
		{
			return true;
		}
		DateTime utcNow = DateTime.UtcNow;
		if (_availabilityState == AvailabilityState.Unavailable && utcNow < _nextAvailabilityProbeUtc)
		{
			return false;
		}
		try
		{
			if ((IsLoadedMethod.Invoke(null, Array.Empty<object>()) as bool?).GetValueOrDefault())
			{
				_availabilityState = AvailabilityState.Available;
				_nextAvailabilityProbeUtc = DateTime.MaxValue;
				return true;
			}
		}
		catch
		{
		}
		_availabilityState = AvailabilityState.Unavailable;
		_nextAvailabilityProbeUtc = utcNow + AvailabilityProbeBackoff;
		return false;
	}

	internal static void ResetPendingWardGuildProjectionRefreshes()
	{
		PendingWardGuildProjectionRefresh.TargetIdentitiesByPlayerId.Clear();
		PendingWardGuildProjectionRefresh.TargetIdentitiesByCharacterKey.Clear();
		PendingWardGuildProjectionRefresh.AffectedGuildIds.Clear();
		PendingWardGuildProjectionRefresh.PendingFullRefresh = false;
		PendingWardGuildProjectionRefresh.PendingLiveDisplayRefresh = false;
		PendingWardGuildProjectionRefresh.PendingLiveDisplayReason = string.Empty;
		PendingWardGuildProjectionRefresh.FlushAtUtc = DateTime.MinValue;
	}

	internal static bool TryStampLocalWardGuildMetadata(PrivateArea? area)
	{
		return TryStampLocalWardGuildMetadata(ManagedWardRef.FromArea(area));
	}

	internal static bool TryStampLocalWardGuildMetadata(ManagedWardRef ward)
	{
		if ((Object)(object)ward.Area == (Object)null || !ManagedWardIdentity.EnsureManagedComponent(ward))
		{
			return false;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return false;
		}
		if (!WardAccess.IsDirectWardOwner(ward, localPlayer.GetPlayerID()))
		{
			return false;
		}
		if (!ward.HasValidNetworkIdentity || !ward.IsOwner)
		{
			return false;
		}
		ZDO zdo = ward.Zdo;
		if (zdo == null)
		{
			return false;
		}
		WardGuildIdentity guild;
		WardGuildIdentity guild2 = ((TryGetGuild(localPlayer, out guild) && guild.Id != 0) ? new WardGuildIdentity(guild.Id, guild.Name ?? string.Empty) : default(WardGuildIdentity));
		ManagedWardProjection projection = ManagedWardProjectionService.ResolveExplicitProjection(localPlayer.GetPlayerID(), WardOwnership.GetPlayerAccountId(localPlayer), guild2);
		return ManagedWardMetadataMutationService.ApplyOwnedLocalProjection(zdo, projection, ManagedWardMapMutationKind.IndexAndPins, "local ward guild metadata stamp", forceSendWhenMetadataChanged: false).ProjectionResult.GuildChanged;
	}

	internal static int GetWardGuildId(PrivateArea? area)
	{
		if (TryGetStoredWardGuildIdentity(GetWardZdo(area), out var guild))
		{
			return guild.Id;
		}
		if (!TryResolveWardGuildIdentity(area, allowMetadataStamp: false, out var guild2))
		{
			return 0;
		}
		return guild2.Id;
	}

	internal static string GetWardGuildName(PrivateArea? area)
	{
		if (TryGetStoredWardGuildIdentity(GetWardZdo(area), out var guild) && !string.IsNullOrWhiteSpace(guild.Name))
		{
			return guild.Name;
		}
		if (TryResolveWardGuildIdentity(area, allowMetadataStamp: false, out var guild2))
		{
			return guild2.Name;
		}
		int id = guild.Id;
		if (id != 0 && TryGetGuildById(id, out guild2))
		{
			return guild2.Name;
		}
		Player localPlayer = Player.m_localPlayer;
		if (id != 0 && (Object)(object)localPlayer != (Object)null && TryGetGuild(localPlayer, out guild2) && guild2.Id == id)
		{
			return guild2.Name;
		}
		return string.Empty;
	}

	internal static WardGuildIdentity ResolveWardGuildIdentityReadOnly(ZDO? zdo)
	{
		if (TryGetStoredWardGuildIdentity(zdo, out var guild) && guild.Id != 0)
		{
			return guild;
		}
		if (zdo == null)
		{
			return default(WardGuildIdentity);
		}
		long @long = zdo.GetLong(ZDOVars.s_creator, 0L);
		string wardSteamAccountId = WardOwnership.ResolveWardSteamAccountId(zdo, @long, WardOwnership.GetWardSteamAccountId(zdo));
		string wardOwnerNameForProjection = GetWardOwnerNameForProjection(zdo);
		if (!TryResolveWardGuildIdentityReadOnly(@long, wardSteamAccountId, wardOwnerNameForProjection, treatResolvedNoGuildAsResolved: false, out var guild2))
		{
			return default(WardGuildIdentity);
		}
		return guild2;
	}

	private static bool TryGetStoredWardGuildIdentity(ZDO? zdo, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if (zdo == null)
		{
			return false;
		}
		int @int = zdo.GetInt("stuw_guild_id", 0);
		string text = zdo.GetString("stuw_guild_name", string.Empty) ?? string.Empty;
		if (@int == 0 && string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		guild = new WardGuildIdentity(@int, text.Trim());
		return true;
	}

	private static ZDO? GetWardZdo(PrivateArea? area)
	{
		return WardPrivateAreaSafeAccess.GetZdo(area);
	}

	private static bool TryResolveWardGuildIdentity(PrivateArea? area, bool allowMetadataStamp, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if ((Object)(object)area == (Object)null)
		{
			return false;
		}
		long canonicalCreatorPlayerId = WardAccess.GetCanonicalCreatorPlayerId(area);
		string wardSteamAccountId = WardOwnership.ResolveWardSteamAccountId(GetWardZdo(area), canonicalCreatorPlayerId, WardOwnership.GetWardSteamAccountId(area));
		string wardOwnerName = GetWardOwnerName(area);
		if (TryResolveWardGuildIdentityReadOnly(canonicalCreatorPlayerId, wardSteamAccountId, wardOwnerName, treatResolvedNoGuildAsResolved: true, out guild))
		{
			if (allowMetadataStamp)
			{
				StampResolvedWardGuildMetadata(area, canonicalCreatorPlayerId, wardSteamAccountId, guild);
			}
			return true;
		}
		return false;
	}

	private static string GetWardOwnerName(PrivateArea? area)
	{
		if ((Object)(object)area == (Object)null)
		{
			return string.Empty;
		}
		string creatorName = WardPrivateAreaSafeAccess.GetCreatorName(area);
		if (!string.IsNullOrWhiteSpace(creatorName))
		{
			return creatorName.Trim();
		}
		return GetWardOwnerNameForProjection(GetWardZdo(area));
	}

	internal static string GetWardOwnerNameForProjection(ZDO? zdo)
	{
		return WardPrivateAreaSafeAccess.GetCreatorName(zdo);
	}

	internal static bool TryResolveProjectedGuildIdentity(long ownerPlayerId, string normalizedAccountId, string ownerName, out WardGuildIdentity guild)
	{
		return TryResolveWardGuildIdentityReadOnly(ownerPlayerId, normalizedAccountId, ownerName, treatResolvedNoGuildAsResolved: true, out guild);
	}

	private static bool TryResolveWardGuildIdentityReadOnly(long ownerPlayerId, string wardSteamAccountId, string ownerName, bool treatResolvedNoGuildAsResolved, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		string text = WardOwnership.NormalizeAccountIdValue(wardSteamAccountId);
		string text2 = ownerName?.Trim() ?? string.Empty;
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer() && TryGetSyncedGuildIdentity(ownerPlayerId, text, text2, out guild))
		{
			if (!treatResolvedNoGuildAsResolved)
			{
				return guild.Id != 0;
			}
			return true;
		}
		if (ownerPlayerId != 0L && TryGetGuild(ownerPlayerId, out guild))
		{
			return true;
		}
		if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
		{
			return TryGetGuildByAccountAndName(text, text2, out guild);
		}
		return false;
	}

	private static void StampResolvedWardGuildMetadata(PrivateArea? area, long ownerPlayerId, string wardSteamAccountId, WardGuildIdentity guild)
	{
		if (guild.Id != 0 && !((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			ZDO wardZdo = GetWardZdo(area);
			if (wardZdo != null)
			{
				ManagedWardMetadataMutationService.ApplyExplicitProjection(wardZdo, ManagedWardProjectionService.ResolveExplicitProjection(ownerPlayerId, wardSteamAccountId, guild), ManagedWardMapMutationKind.IndexAndPins, "resolved ward guild metadata");
			}
		}
	}

	internal static void HandleGuildSaved(object? guild)
	{
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Refresh", "Queued guild save refresh for guild=" + DescribeGuildObject(guild) + ".");
		RefreshWardGuildProjectionForGuild(guild);
	}

	internal static void RefreshAllWardGuildProjections(bool liveDisplayRefresh = false)
	{
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Refresh", "Queued full ward guild projection refresh.");
		QueueWardGuildProjectionRefreshForAll(liveDisplayRefresh, "full guild projection refresh");
	}

	private static void RefreshWardGuildProjectionForGuild(object? guild)
	{
		WardGuildIdentity guild2;
		int affectedGuildId = (TryParseGuild(guild, out guild2) ? guild2.Id : 0);
		bool hadUnresolvedMembers;
		List<WardGuildCharacterIdentity> list = CollectGuildMemberCharacterIdentities(guild, out hadUnresolvedMembers);
		if (list.Count == 0 || hadUnresolvedMembers)
		{
			Plugin.LogWardDiagnosticFailure("GuildsCompat.Refresh", "Could not extract complete guild member character identities from guild=" + DescribeGuildObject(guild) + ". Falling back to full ward guild refresh.");
			QueueWardGuildProjectionRefreshForAll(liveDisplayRefresh: false, "full guild projection refresh");
			return;
		}
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Refresh", $"Queued ward guild projection refresh for {list.Count} guild member character(s) from guild={DescribeGuildObject(guild)}.");
		foreach (WardGuildCharacterIdentity item in list)
		{
			RefreshWardGuildProjectionForCharacter(item, liveDisplayRefresh: false, affectedGuildId);
		}
	}

	private static void RefreshWardGuildProjectionForCharacter(WardGuildCharacterIdentity identity, bool liveDisplayRefresh = false, int affectedGuildId = 0, int previousGuildId = 0)
	{
		if (identity.HasPlayerId || identity.HasAccountAndName)
		{
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Refresh", $"Queued ward guild projection refresh for character playerId={identity.PlayerId}, accountId='{identity.AccountId}', playerName='{identity.PlayerName}'.");
			if (!string.IsNullOrWhiteSpace(identity.AccountId))
			{
				InvalidateGuildCacheForAccountId(identity.AccountId);
			}
			QueueWardGuildProjectionRefreshForCharacter(identity, liveDisplayRefresh, $"guild projection refresh for playerId={identity.PlayerId}, accountId='{identity.AccountId}'", affectedGuildId, previousGuildId);
		}
	}

	internal static void ProcessPendingWardGuildProjectionRefreshes()
	{
		if (!((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			PendingWardGuildProjectionRefreshState pendingWardGuildProjectionRefresh = PendingWardGuildProjectionRefresh;
			if ((pendingWardGuildProjectionRefresh.PendingFullRefresh || pendingWardGuildProjectionRefresh.TargetIdentitiesByPlayerId.Count != 0 || pendingWardGuildProjectionRefresh.TargetIdentitiesByCharacterKey.Count != 0 || pendingWardGuildProjectionRefresh.AffectedGuildIds.Count != 0) && !(pendingWardGuildProjectionRefresh.FlushAtUtc > DateTime.UtcNow))
			{
				bool pendingFullRefresh = pendingWardGuildProjectionRefresh.PendingFullRefresh;
				bool pendingLiveDisplayRefresh = pendingWardGuildProjectionRefresh.PendingLiveDisplayRefresh;
				string liveDisplayReason = (string.IsNullOrWhiteSpace(pendingWardGuildProjectionRefresh.PendingLiveDisplayReason) ? "guild projection refreshed" : pendingWardGuildProjectionRefresh.PendingLiveDisplayReason);
				HashSet<long> targetPlayerIds = ((pendingWardGuildProjectionRefresh.TargetIdentitiesByPlayerId.Count == 0) ? null : new HashSet<long>(pendingWardGuildProjectionRefresh.TargetIdentitiesByPlayerId.Keys));
				HashSet<string> targetCharacterKeys = ((pendingWardGuildProjectionRefresh.TargetIdentitiesByCharacterKey.Count == 0) ? null : new HashSet<string>(pendingWardGuildProjectionRefresh.TargetIdentitiesByCharacterKey.Keys, StringComparer.Ordinal));
				HashSet<int> affectedGuildIds = ((pendingWardGuildProjectionRefresh.AffectedGuildIds.Count == 0) ? null : new HashSet<int>(pendingWardGuildProjectionRefresh.AffectedGuildIds));
				ResetPendingWardGuildProjectionRefreshes();
				RefreshWardGuildProjectionForManagedWards(targetPlayerIds, targetCharacterKeys, affectedGuildIds, pendingFullRefresh, pendingLiveDisplayRefresh, liveDisplayReason);
			}
		}
	}

	private static void QueueWardGuildProjectionRefreshForAll(bool liveDisplayRefresh, string liveDisplayReason)
	{
		PendingWardGuildProjectionRefresh.PendingFullRefresh = true;
		PendingWardGuildProjectionRefresh.TargetIdentitiesByPlayerId.Clear();
		PendingWardGuildProjectionRefresh.TargetIdentitiesByCharacterKey.Clear();
		UpdatePendingWardGuildProjectionRefreshWindow(liveDisplayRefresh, liveDisplayReason);
	}

	private static void QueueWardGuildProjectionRefreshForCharacter(WardGuildCharacterIdentity identity, bool liveDisplayRefresh, string liveDisplayReason, int affectedGuildId, int previousGuildId)
	{
		if (!PendingWardGuildProjectionRefresh.PendingFullRefresh)
		{
			if (identity.HasPlayerId)
			{
				PendingWardGuildProjectionRefresh.TargetIdentitiesByPlayerId[identity.PlayerId] = MergeQueuedWardGuildCharacterIdentity(PendingWardGuildProjectionRefresh.TargetIdentitiesByPlayerId.TryGetValue(identity.PlayerId, out var value) ? value : default(WardGuildCharacterIdentity), identity);
			}
			if (identity.HasAccountAndName)
			{
				string text = BuildCharacterIdentityKey(identity.AccountId, identity.PlayerName);
				if (!string.IsNullOrWhiteSpace(text))
				{
					PendingWardGuildProjectionRefresh.TargetIdentitiesByCharacterKey[text] = MergeQueuedWardGuildCharacterIdentity(PendingWardGuildProjectionRefresh.TargetIdentitiesByCharacterKey.TryGetValue(text, out var value2) ? value2 : default(WardGuildCharacterIdentity), identity);
				}
			}
		}
		if (affectedGuildId != 0)
		{
			PendingWardGuildProjectionRefresh.AffectedGuildIds.Add(affectedGuildId);
		}
		if (previousGuildId != 0)
		{
			PendingWardGuildProjectionRefresh.AffectedGuildIds.Add(previousGuildId);
		}
		UpdatePendingWardGuildProjectionRefreshWindow(liveDisplayRefresh, liveDisplayReason);
	}

	private static void UpdatePendingWardGuildProjectionRefreshWindow(bool liveDisplayRefresh, string liveDisplayReason)
	{
		if (liveDisplayRefresh)
		{
			PendingWardGuildProjectionRefresh.PendingLiveDisplayRefresh = true;
		}
		if (string.IsNullOrWhiteSpace(PendingWardGuildProjectionRefresh.PendingLiveDisplayReason))
		{
			PendingWardGuildProjectionRefresh.PendingLiveDisplayReason = (string.IsNullOrWhiteSpace(liveDisplayReason) ? "guild projection refreshed" : liveDisplayReason);
		}
		if (PendingWardGuildProjectionRefresh.FlushAtUtc == DateTime.MinValue)
		{
			PendingWardGuildProjectionRefresh.FlushAtUtc = DateTime.UtcNow + PendingWardGuildProjectionRefreshDebounce;
		}
	}

	private static WardGuildCharacterIdentity MergeQueuedWardGuildCharacterIdentity(WardGuildCharacterIdentity existingIdentity, WardGuildCharacterIdentity incomingIdentity)
	{
		long playerId = (existingIdentity.HasPlayerId ? existingIdentity.PlayerId : incomingIdentity.PlayerId);
		string accountId = ((!string.IsNullOrWhiteSpace(existingIdentity.AccountId)) ? existingIdentity.AccountId : incomingIdentity.AccountId);
		string playerName = ((!string.IsNullOrWhiteSpace(existingIdentity.PlayerName)) ? existingIdentity.PlayerName : incomingIdentity.PlayerName);
		return new WardGuildCharacterIdentity(playerId, accountId, playerName);
	}

	private static bool RefreshWardGuildProjectionForManagedWards(HashSet<long>? targetPlayerIds, HashSet<string>? targetCharacterKeys, HashSet<int>? affectedGuildIds, bool fullRefresh, bool liveDisplayRefresh, string liveDisplayReason)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return false;
		}
		if (!fullRefresh && (targetPlayerIds == null || targetPlayerIds.Count == 0) && (targetCharacterKeys == null || targetCharacterKeys.Count == 0) && (affectedGuildIds == null || affectedGuildIds.Count == 0))
		{
			return false;
		}
		if (fullRefresh)
		{
			InvalidateAllGuildCaches();
		}
		HashSet<ZDOID> hashSet = new HashSet<ZDOID>();
		int num = ManagedWardRegistry.CollectCandidateIds(hashSet, targetPlayerIds, targetCharacterKeys, affectedGuildIds, fullRefresh);
		int num2 = 0;
		int num3 = 0;
		foreach (ZDOID item in hashSet)
		{
			ZDOMan instance = ZDOMan.instance;
			ZDO val = ((instance != null) ? instance.GetZDO(item) : null);
			if (val == null || !WardOwnership.IsManagedWardZdo(val))
			{
				ManagedWardRegistry.RemoveEntry(item);
				continue;
			}
			long @long = val.GetLong(ZDOVars.s_creator, 0L);
			num3++;
			string wardSteamAccountId = WardOwnership.NormalizeAccountIdValue(WardOwnership.ResolveWardSteamAccountId(val, @long, WardOwnership.GetWardSteamAccountId(val)));
			if (ManagedWardMetadataMutationService.RefreshProjectedMetadata(val, @long, wardSteamAccountId, ManagedWardMapMutationKind.IndexOnly, "guild projection refreshed").ProjectionResult.AnyChanged)
			{
				num2++;
			}
		}
		if (num2 > 0 && liveDisplayRefresh)
		{
			NotifyGuildProjectionRefreshApplied(liveDisplayReason, fullRefresh, targetPlayerIds, targetCharacterKeys, affectedGuildIds);
		}
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Refresh", string.Format("Scanned {0} managed ward(s) for guild projection refresh out of {1} registry candidate(s) and {2} indexed ward(s){3}{4}{5}{6}, changed={7}.", num3, num, ManagedWardRegistry.GetIndexedCount(), fullRefresh ? " on full refresh" : string.Empty, (!fullRefresh && targetPlayerIds != null && targetPlayerIds.Count > 0) ? $" targeting {targetPlayerIds.Count} playerId(s)" : string.Empty, (!fullRefresh && targetCharacterKeys != null && targetCharacterKeys.Count > 0) ? $" and {targetCharacterKeys.Count} account/name identities" : string.Empty, (!fullRefresh && affectedGuildIds != null && affectedGuildIds.Count > 0) ? $" across {affectedGuildIds.Count} affected guild(s)" : string.Empty, num2));
		return num2 > 0;
	}

	private static List<WardGuildCharacterIdentity> CollectGuildMemberCharacterIdentities(object? guild, out bool hadUnresolvedMembers)
	{
		Dictionary<string, WardGuildCharacterIdentity> dictionary = new Dictionary<string, WardGuildCharacterIdentity>(StringComparer.Ordinal);
		hadUnresolvedMembers = false;
		if (guild == null || GuildMembersField == null)
		{
			return new List<WardGuildCharacterIdentity>();
		}
		object value = GuildMembersField.GetValue(guild);
		if (value is IDictionary dictionary2)
		{
			foreach (object key in dictionary2.Keys)
			{
				if (!TryCreateCharacterIdentityFromPlayerReference(key, out var identity))
				{
					hadUnresolvedMembers = true;
				}
				else
				{
					dictionary[BuildCharacterIdentityKey(identity.AccountId, identity.PlayerName)] = identity;
				}
			}
			return new List<WardGuildCharacterIdentity>(dictionary.Values);
		}
		if (!(value is IEnumerable enumerable))
		{
			return new List<WardGuildCharacterIdentity>();
		}
		foreach (object item in enumerable)
		{
			if (item != null)
			{
				if (!TryCreateCharacterIdentityFromPlayerReference(AccessTools.Property(item.GetType(), "Key")?.GetValue(item, null), out var identity2))
				{
					hadUnresolvedMembers = true;
				}
				else
				{
					dictionary[BuildCharacterIdentityKey(identity2.AccountId, identity2.PlayerName)] = identity2;
				}
			}
		}
		return new List<WardGuildCharacterIdentity>(dictionary.Values);
	}

	private static bool TryCreateCharacterIdentityFromPlayerReference(object? playerReference, out WardGuildCharacterIdentity identity)
	{
		identity = default(WardGuildCharacterIdentity);
		if (playerReference == null || PlayerReferenceIdField == null)
		{
			return false;
		}
		try
		{
			string text = WardOwnership.NormalizeAccountIdValue(PlayerReferenceIdField.GetValue(playerReference)?.ToString());
			string playerNameFromPlayerReference = GetPlayerNameFromPlayerReference(playerReference);
			if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(playerNameFromPlayerReference))
			{
				return false;
			}
			identity = new WardGuildCharacterIdentity(0L, text, playerNameFromPlayerReference);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string GetPlayerNameFromPlayerReference(object playerReference)
	{
		if (PlayerReferenceNameField != null)
		{
			string text = PlayerReferenceNameField.GetValue(playerReference)?.ToString()?.Trim() ?? string.Empty;
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		string text2 = playerReference.ToString()?.Trim() ?? string.Empty;
		int num = text2.IndexOf(':');
		if (num < 0 || num >= text2.Length - 1)
		{
			return string.Empty;
		}
		return text2.Substring(num + 1).Trim();
	}

	private static void InvalidateGuildCacheForAccountId(string accountId)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		string text = WardOwnership.NormalizeAccountIdValue(accountId);
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		HashSet<long> hashSet = new HashSet<long>();
		foreach (KeyValuePair<long, CachedPlayerPlatformIdentity> item in PlayerPlatformIdCache)
		{
			if (item.Value.HasPlatformId && string.Equals(item.Value.PlatformId, text, StringComparison.Ordinal))
			{
				hashSet.Add(item.Key);
			}
		}
		List<PlayerInfo> list = ZNet.instance?.m_players;
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				PlayerInfo val = list[i];
				if (string.Equals(WardOwnership.NormalizeAccountIdValue(((object)(PlatformUserID)(ref val.m_userInfo.m_id)).ToString()), text, StringComparison.Ordinal))
				{
					ZDOMan instance = ZDOMan.instance;
					ZDO obj = ((instance != null) ? instance.GetZDO(val.m_characterID) : null);
					long num = ((obj != null) ? obj.GetLong(ZDOVars.s_playerID, 0L) : 0);
					if (num != 0L)
					{
						hashSet.Add(num);
					}
				}
			}
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null && string.Equals(WardOwnership.NormalizeAccountIdValue(WardOwnership.GetPlayerAccountId(localPlayer)), text, StringComparison.Ordinal))
		{
			hashSet.Add(localPlayer.GetPlayerID());
		}
		foreach (long item2 in hashSet)
		{
			PlayerGuildCache.Remove(item2);
			PlayerPlatformIdCache.Remove(item2);
		}
	}

	private static void InvalidateAllGuildCaches()
	{
		PlayerGuildCache.Clear();
		PlayerPlatformIdCache.Clear();
	}

	internal static string DescribeGuildObject(object? guild)
	{
		if (guild == null)
		{
			return "null";
		}
		try
		{
			string arg = (GuildNameField?.GetValue(guild) as string) ?? string.Empty;
			int num = ((GuildGeneralField != null && GuildGeneralIdField != null) ? Convert.ToInt32(GuildGeneralIdField.GetValue(GuildGeneralField.GetValue(guild))) : 0);
			return $"type={guild.GetType().FullName}, id={num}, name='{arg}'";
		}
		catch
		{
			return "type=" + guild.GetType().FullName;
		}
	}

	internal static string GetPlayerPlatformId(long playerId)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (playerId == 0L)
		{
			return string.Empty;
		}
		if (TryGetCachedPlatformId(playerId, out string platformId))
		{
			return platformId;
		}
		string playerAccountId = WardOwnership.GetPlayerAccountId(playerId);
		if (!string.IsNullOrWhiteSpace(playerAccountId))
		{
			CachePlatformId(playerId, playerAccountId);
			return playerAccountId;
		}
		PlayerInfo? val = FindPlayerInfo(playerId);
		if (!val.HasValue || PlayerInfoUserInfoField == null || UserInfoIdField == null)
		{
			CachePlatformId(playerId, string.Empty);
			return string.Empty;
		}
		try
		{
			object obj = val.Value;
			object value = PlayerInfoUserInfoField.GetValue(obj);
			if (value == null)
			{
				CachePlatformId(playerId, string.Empty);
				return string.Empty;
			}
			string text = UserInfoIdField.GetValue(value)?.ToString() ?? string.Empty;
			CachePlatformId(playerId, text);
			return text;
		}
		catch
		{
			CachePlatformId(playerId, string.Empty);
			return string.Empty;
		}
	}

	private static bool TryGetGuild(Player? player, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		long playerID = player.GetPlayerID();
		if (TryGetCachedGuild(playerID, out guild))
		{
			return true;
		}
		if (IsCachedNoGuild(playerID))
		{
			return false;
		}
		if (IsAvailable() && GetPlayerGuildByPlayerMethod != null)
		{
			try
			{
				if (TryParseGuild(GetPlayerGuildByPlayerMethod.Invoke(null, new object[1] { player }), out guild))
				{
					CacheGuildLookup(playerID, hasGuild: true, guild);
					return true;
				}
			}
			catch
			{
			}
		}
		string playerAccountId = WardOwnership.GetPlayerAccountId(player);
		string playerName = player.GetPlayerName();
		if (!string.IsNullOrWhiteSpace(playerAccountId) && !string.IsNullOrWhiteSpace(playerName) && TryGetGuildByAccountAndName(playerAccountId, playerName, out guild))
		{
			CacheGuildLookup(playerID, hasGuild: true, guild);
			return true;
		}
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Lookup", $"Failed local guild lookup. playerId={playerID}, playerName='{playerName}', accountId='{playerAccountId}', apiAvailable={IsAvailable()}, hasPlayerLookup={GetPlayerGuildByPlayerMethod != null}, hasReferenceLookup={GetPlayerGuildByReferenceMethod != null && PlayerReferenceFromStringMethod != null}");
		CacheGuildLookup(playerID, hasGuild: false, default(WardGuildIdentity));
		return false;
	}

	private static bool TryGetGuild(long playerId, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if (playerId == 0L)
		{
			return false;
		}
		string playerAccountId = WardOwnership.GetPlayerAccountId(playerId);
		string playerName = WardOwnership.GetPlayerName(playerId);
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer() && TryGetSyncedGuildIdentity(playerId, playerAccountId, playerName, out guild))
		{
			CacheGuildLookup(playerId, guild.Id != 0, guild);
			return guild.Id != 0;
		}
		if (TryGetCachedGuild(playerId, out guild))
		{
			return true;
		}
		if (IsCachedNoGuild(playerId))
		{
			return false;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null && localPlayer.GetPlayerID() == playerId)
		{
			return TryGetGuild(localPlayer, out guild);
		}
		if (!IsAvailable() || GetPlayerGuildByReferenceMethod == null || PlayerReferenceFromStringMethod == null)
		{
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Lookup", $"Failed remote guild lookup because Guilds API reference lookup is unavailable. playerId={playerId}, playerName='{WardOwnership.GetPlayerName(playerId)}', accountId='{WardOwnership.GetPlayerAccountId(playerId)}', apiAvailable={IsAvailable()}, hasReferenceLookup={GetPlayerGuildByReferenceMethod != null && PlayerReferenceFromStringMethod != null}");
			return false;
		}
		string playerPlatformId = GetPlayerPlatformId(playerId);
		if (string.IsNullOrWhiteSpace(playerPlatformId))
		{
			string playerAccountId2 = WardOwnership.GetPlayerAccountId(playerId);
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Lookup", $"Failed remote guild lookup because live player platform id is unavailable. playerId={playerId}, playerName='{WardOwnership.GetPlayerName(playerId)}', fallbackAccountId='{playerAccountId2}'");
			CacheGuildLookup(playerId, hasGuild: false, default(WardGuildIdentity));
			return false;
		}
		if (string.IsNullOrWhiteSpace(playerName))
		{
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Lookup", $"Failed remote guild lookup because player name is unavailable. playerId={playerId}, accountId='{playerPlatformId}'");
			CacheGuildLookup(playerId, hasGuild: false, default(WardGuildIdentity));
			return false;
		}
		if (!string.IsNullOrWhiteSpace(playerName) && TryGetGuildByAccountAndName(playerPlatformId, playerName, out guild))
		{
			CacheGuildLookup(playerId, hasGuild: true, guild);
			return true;
		}
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Lookup", $"Failed remote guild lookup after account/name lookup. playerId={playerId}, playerName='{playerName}', accountId='{playerPlatformId}'");
		CacheGuildLookup(playerId, hasGuild: false, default(WardGuildIdentity));
		return false;
	}

	private static bool TryGetGuildByAccountAndName(string accountId, string playerName, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		string text = WardOwnership.NormalizeAccountIdValue(accountId);
		string text2 = playerName?.Trim() ?? string.Empty;
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer() && TryGetSyncedGuildIdentity(text, text2, out guild))
		{
			return guild.Id != 0;
		}
		if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(text2) || GetPlayerGuildByReferenceMethod == null || PlayerReferenceFromStringMethod == null || !IsAvailable())
		{
			return false;
		}
		try
		{
			object obj = PlayerReferenceFromStringMethod.Invoke(null, new object[1] { text + ":" + text2 });
			return TryParseGuild(GetPlayerGuildByReferenceMethod.Invoke(null, new object[1] { obj }), out guild);
		}
		catch
		{
			return false;
		}
	}

	private static bool TryGetGuildById(int guildId, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if (guildId == 0 || GetGuildByIdMethod == null || !IsAvailable())
		{
			return false;
		}
		try
		{
			return TryParseGuild(GetGuildByIdMethod.Invoke(null, new object[1] { guildId }), out guild);
		}
		catch
		{
			return false;
		}
	}

	private static bool TryGetCachedGuild(long playerId, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if (!PlayerGuildCache.TryGetValue(playerId, out var value))
		{
			return false;
		}
		if (value.ExpiresAtUtc <= DateTime.UtcNow)
		{
			PlayerGuildCache.Remove(playerId);
			return false;
		}
		if (!value.HasGuild || value.GuildId == 0)
		{
			return false;
		}
		guild = new WardGuildIdentity(value.GuildId, value.GuildName);
		return true;
	}

	private static bool IsCachedNoGuild(long playerId)
	{
		if (!PlayerGuildCache.TryGetValue(playerId, out var value))
		{
			return false;
		}
		if (value.ExpiresAtUtc <= DateTime.UtcNow)
		{
			PlayerGuildCache.Remove(playerId);
			return false;
		}
		return !value.HasGuild;
	}

	private static void CacheGuildLookup(long playerId, bool hasGuild, WardGuildIdentity guild)
	{
		if (playerId != 0L)
		{
			PlayerGuildCache[playerId] = new CachedWardGuildIdentity(hasGuild && guild.Id != 0, guild.Id, guild.Name ?? string.Empty, DateTime.UtcNow + GuildLookupCacheDuration);
		}
	}

	private static bool TryGetCachedPlatformId(long playerId, out string platformId)
	{
		platformId = string.Empty;
		if (!PlayerPlatformIdCache.TryGetValue(playerId, out var value))
		{
			return false;
		}
		if (value.ExpiresAtUtc <= DateTime.UtcNow)
		{
			PlayerPlatformIdCache.Remove(playerId);
			return false;
		}
		if (!value.HasPlatformId)
		{
			return true;
		}
		platformId = value.PlatformId;
		return true;
	}

	private static void CachePlatformId(long playerId, string platformId)
	{
		if (playerId != 0L)
		{
			bool flag = !string.IsNullOrWhiteSpace(platformId);
			PlayerPlatformIdCache[playerId] = new CachedPlayerPlatformIdentity(flag, flag ? platformId : string.Empty, DateTime.UtcNow + GuildLookupCacheDuration);
		}
	}

	private static PlayerInfo? FindPlayerInfo(long playerId)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerInfo> list = ZNet.instance?.m_players;
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			PlayerInfo val = list[i];
			ZDOMan instance = ZDOMan.instance;
			ZDO obj = ((instance != null) ? instance.GetZDO(val.m_characterID) : null);
			if (((obj != null) ? obj.GetLong(ZDOVars.s_playerID, 0L) : 0) == playerId)
			{
				return val;
			}
		}
		return null;
	}

	private static bool TryParseGuild(object? guildObject, out WardGuildIdentity guild)
	{
		guild = default(WardGuildIdentity);
		if (guildObject == null || GuildNameField == null || GuildGeneralField == null || GuildGeneralIdField == null)
		{
			return false;
		}
		try
		{
			object value = GuildGeneralField.GetValue(guildObject);
			int num = ((value != null) ? Convert.ToInt32(GuildGeneralIdField.GetValue(value)) : 0);
			if (num == 0)
			{
				return false;
			}
			string name = (GuildNameField.GetValue(guildObject) as string) ?? string.Empty;
			guild = new WardGuildIdentity(num, name);
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static void TryPatch(Harmony harmony)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		EnsureHooksRegistered();
		if (_saveGuildPatched || harmony == null || SaveGuildMethod == null)
		{
			if (SaveGuildMethod == null)
			{
				Plugin.LogWardDiagnosticFailure("GuildsCompat.Patch", "Skipped SaveGuild postfix patch because SaveGuildMethod could not be resolved.");
			}
			return;
		}
		MethodInfo methodInfo = AccessTools.DeclaredMethod(typeof(GuildsSaveGuildPatch), "Postfix", (Type[])null, (Type[])null);
		if (!(methodInfo == null))
		{
			harmony.Patch((MethodBase)SaveGuildMethod, (HarmonyMethod)null, new HarmonyMethod(methodInfo), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			_saveGuildPatched = true;
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Patch", "Patched Guilds.API.SaveGuild postfix for ward guild projection refresh.");
		}
	}

	private static void EnsureHooksRegistered()
	{
		if (_guildHooksRegistered)
		{
			_guildHooksActive = true;
			return;
		}
		if (ApiType == null)
		{
			Plugin.LogWardDiagnosticFailure("GuildsCompat.Patch", "Skipped guild event hook registration because Guilds API type could not be resolved.");
			return;
		}
		RegisterGuildHook(RegisterOnGuildJoinedMethod, GuildJoinedDelegateType, "HandleGuildJoinedEvent");
		RegisterGuildHook(RegisterOnGuildLeftMethod, GuildLeftDelegateType, "HandleGuildLeftEvent");
		RegisterGuildHook(RegisterOnGuildCreatedMethod, GuildCreatedDelegateType, "HandleGuildCreatedEvent");
		RegisterGuildHook(RegisterOnGuildDeletedMethod, GuildDeletedDelegateType, "HandleGuildDeletedEvent");
		_guildHooksRegistered = true;
		_guildHooksActive = true;
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Patch", "Registered Guilds API event hooks for joined/left/created/deleted.");
	}

	internal static void TryShutdownHooks()
	{
		_guildHooksActive = false;
		_saveGuildPatched = false;
	}

	internal static bool IsGuildHooksActive()
	{
		return _guildHooksActive;
	}

	private static void RegisterGuildHook(MethodInfo? registerMethod, Type? delegateType, string handlerName)
	{
		if (registerMethod == null || delegateType == null)
		{
			Plugin.LogWardDiagnosticFailure("GuildsCompat.Patch", "Skipped guild hook registration for handler '" + handlerName + "' because the register method or delegate type was unresolved.");
			return;
		}
		Delegate @delegate = CreateGuildCallback(delegateType, handlerName);
		if ((object)@delegate == null)
		{
			Plugin.LogWardDiagnosticFailure("GuildsCompat.Patch", "Failed to create guild callback delegate for handler '" + handlerName + "'.");
			return;
		}
		registerMethod.Invoke(null, new object[1] { @delegate });
		Plugin.LogWardDiagnosticVerbose("GuildsCompat.Patch", "Registered guild hook '" + registerMethod.Name + "' -> '" + handlerName + "'.");
	}

	private static Delegate? CreateGuildCallback(Type delegateType, string handlerName)
	{
		MethodInfo methodInfo = AccessTools.DeclaredMethod(typeof(GuildsCompat), handlerName, (Type[])null, (Type[])null);
		MethodInfo method = delegateType.GetMethod("Invoke");
		if (methodInfo == null || method == null)
		{
			return null;
		}
		ParameterInfo[] parameters = method.GetParameters();
		ParameterExpression[] array = new ParameterExpression[parameters.Length];
		Expression[] array2 = new Expression[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			array2[i] = Expression.Convert(array[i] = Expression.Parameter(parameters[i].ParameterType, parameters[i].Name), typeof(object));
		}
		MethodCallExpression body = Expression.Call(methodInfo, array2);
		return Expression.Lambda(delegateType, body, array).Compile();
	}

	private static void HandleGuildJoinedEvent(object guild, object playerReference)
	{
		if (_guildHooksActive)
		{
			if (!TryCreateCharacterIdentityFromPlayerReference(playerReference, out var identity))
			{
				Plugin.LogWardDiagnosticFailure("GuildsCompat.Event", "Received GuildJoined event with an unresolved player reference. Falling back to full ward guild refresh, guild=" + DescribeGuildObject(guild) + ".");
				RefreshAllWardGuildProjections(liveDisplayRefresh: true);
				return;
			}
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Event", "Received GuildJoined event for accountId='" + identity.AccountId + "', playerName='" + identity.PlayerName + "', guild=" + DescribeGuildObject(guild) + ".");
			RefreshWardGuildProjectionForCharacter(identity, liveDisplayRefresh: true, TryParseGuild(guild, out var guild2) ? guild2.Id : 0);
		}
	}

	private static void HandleGuildLeftEvent(object guild, object playerReference)
	{
		if (_guildHooksActive)
		{
			if (!TryCreateCharacterIdentityFromPlayerReference(playerReference, out var identity))
			{
				Plugin.LogWardDiagnosticFailure("GuildsCompat.Event", "Received GuildLeft event with an unresolved player reference. Falling back to full ward guild refresh, guild=" + DescribeGuildObject(guild) + ".");
				RefreshAllWardGuildProjections(liveDisplayRefresh: true);
				return;
			}
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Event", "Received GuildLeft event for accountId='" + identity.AccountId + "', playerName='" + identity.PlayerName + "', guild=" + DescribeGuildObject(guild) + ".");
			RefreshWardGuildProjectionForCharacter(identity, liveDisplayRefresh: true, TryParseGuild(guild, out var guild2) ? guild2.Id : 0);
		}
	}

	private static void HandleGuildCreatedEvent(object guild)
	{
		if (_guildHooksActive)
		{
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Event", "Received GuildCreated event, guild=" + DescribeGuildObject(guild) + ". Refreshing all ward guild projections.");
			RefreshAllWardGuildProjections(liveDisplayRefresh: true);
		}
	}

	private static void HandleGuildDeletedEvent(object guild)
	{
		if (_guildHooksActive)
		{
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Event", "Received GuildDeleted event, guild=" + DescribeGuildObject(guild) + ". Refreshing all ward guild projections.");
			RefreshAllWardGuildProjections(liveDisplayRefresh: true);
		}
	}
}
