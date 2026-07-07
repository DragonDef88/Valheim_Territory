using System;
using UnityEngine;

namespace STUWard;

internal static class ManagedWardProjectionService
{
	private const string OwnerAccountIdKey = "stuw_owner_account_id";

	private const string GuildIdKey = "stuw_guild_id";

	private const string GuildNameKey = "stuw_guild_name";

	internal static ManagedWardProjection ResolveProjection(ZDO? zdo, long ownerPlayerId, string wardSteamAccountId)
	{
		if (zdo == null)
		{
			return default(ManagedWardProjection);
		}
		string text = ((ownerPlayerId != 0L) ? WardOwnership.GetPlayerAccountId(ownerPlayerId) : string.Empty);
		string text2 = ((!string.IsNullOrWhiteSpace(text)) ? WardOwnership.NormalizeAccountIdValue(text) : WardOwnership.ResolveWardSteamAccountId(zdo, ownerPlayerId, wardSteamAccountId));
		if (string.IsNullOrWhiteSpace(text2))
		{
			return new ManagedWardProjection(string.Empty, hasResolvedGuild: false, default(WardGuildIdentity));
		}
		string wardOwnerNameForProjection = GuildsCompat.GetWardOwnerNameForProjection(zdo);
		if (GuildsCompat.TryResolveProjectedGuildIdentity(ownerPlayerId, text2, wardOwnerNameForProjection, out var guild))
		{
			return new ManagedWardProjection(text2, hasResolvedGuild: true, guild);
		}
		return new ManagedWardProjection(text2, hasResolvedGuild: false, default(WardGuildIdentity));
	}

	internal static ManagedWardProjection ResolveExplicitProjection(long ownerPlayerId, string wardSteamAccountId, WardGuildIdentity guild)
	{
		return new ManagedWardProjection((ownerPlayerId != 0L) ? WardOwnership.NormalizeAccountIdValue(WardOwnership.GetPlayerAccountId(ownerPlayerId)) : WardOwnership.NormalizeAccountIdValue(wardSteamAccountId), hasResolvedGuild: true, guild);
	}

	internal static ManagedWardProjectionApplyResult RefreshProjection(ZDO? zdo, long ownerPlayerId, string wardSteamAccountId)
	{
		return ApplyProjection(zdo, ResolveProjection(zdo, ownerPlayerId, wardSteamAccountId));
	}

	internal static ManagedWardProjectionApplyResult ApplyProjection(ZDO? zdo, ManagedWardProjection projection, bool requireServer = true)
	{
		if (zdo == null || (requireServer && ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())))
		{
			return default(ManagedWardProjectionApplyResult);
		}
		bool accountChanged = false;
		if (!string.IsNullOrWhiteSpace(projection.AccountId) && !string.Equals(WardOwnership.GetWardSteamAccountId(zdo), projection.AccountId, StringComparison.Ordinal))
		{
			zdo.Set("stuw_owner_account_id", projection.AccountId);
			accountChanged = true;
		}
		bool guildChanged = false;
		if (projection.HasResolvedGuild)
		{
			guildChanged = ApplyProjectedGuildMetadata(zdo, projection.Guild);
		}
		return new ManagedWardProjectionApplyResult(accountChanged, guildChanged);
	}

	private static bool ApplyProjectedGuildMetadata(ZDO zdo, WardGuildIdentity guild)
	{
		bool result = false;
		if (zdo.GetInt("stuw_guild_id", 0) != guild.Id)
		{
			zdo.Set("stuw_guild_id", guild.Id);
			result = true;
		}
		string text = guild.Name ?? string.Empty;
		if (!string.Equals(zdo.GetString("stuw_guild_name", string.Empty), text, StringComparison.Ordinal))
		{
			zdo.Set("stuw_guild_name", text);
			result = true;
		}
		return result;
	}
}
