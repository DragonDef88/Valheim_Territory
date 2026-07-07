using UnityEngine;

namespace STUWard;

internal static class WardDiagnosticInfo
{
	internal static string DescribeWard(PrivateArea? area)
	{
		if ((Object)(object)area == (Object)null)
		{
			return "ward=null";
		}
		Piece obj = (((Object)(object)area.m_piece != (Object)null) ? area.m_piece : ((Component)area).GetComponent<Piece>());
		ZNetView nView = WardPrivateAreaSafeAccess.GetNView(area);
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		string text = ((zdo != null) ? ((object)(ZDOID)(ref zdo.m_uid)).ToString() : "none");
		long num = ((zdo != null) ? zdo.GetLong(ZDOVars.s_creator, 0L) : 0);
		long num2 = ((obj != null) ? obj.GetCreator() : 0);
		string wardSteamAccountId = WardOwnership.GetWardSteamAccountId(area);
		int wardGuildId = GuildsCompat.GetWardGuildId(zdo);
		string wardGuildName = GuildsCompat.GetWardGuildName(zdo);
		bool flag = (Object)(object)nView != (Object)null && nView.IsValid();
		bool flag2 = (Object)(object)nView != (Object)null && nView.IsOwner();
		return $"wardZdo={text}, nviewValid={flag}, nviewOwner={flag2}, enabled={area.IsEnabled()}, pieceCreator={num2}, zdoCreator={num}, steamAccountId='{wardSteamAccountId}', guildId={wardGuildId}, guildName='{wardGuildName}'";
	}

	internal static string DescribeInteractionState(PrivateArea? area, long requesterId = 0L)
	{
		if ((Object)(object)area == (Object)null)
		{
			return "interaction=ward=null";
		}
		bool flag = requesterId != 0L && WardAccess.CanControlManagedWard(area, requesterId);
		bool flag2 = requesterId != 0L && WardPrivateAreaSafeAccess.IsPlayerPermitted(area, requesterId);
		return $"requesterId={requesterId}, canControl={flag}, isPermitted={flag2}, {DescribeWard(area)}";
	}

	internal static string DescribeLocalPlayer(Player? player)
	{
		Game instance = Game.instance;
		long? obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			PlayerProfile playerProfile = instance.GetPlayerProfile();
			obj = ((playerProfile != null) ? new long?(playerProfile.GetPlayerID()) : null);
		}
		long? num = obj;
		long valueOrDefault = num.GetValueOrDefault();
		if ((Object)(object)player == (Object)null)
		{
			return $"player=null, profilePlayerId={valueOrDefault}";
		}
		return $"playerId={player.GetPlayerID()}, profilePlayerId={valueOrDefault}, ownerSession={((Character)player).GetOwner()}, accountId='{WardOwnership.GetPlayerAccountId(player)}'";
	}
}
