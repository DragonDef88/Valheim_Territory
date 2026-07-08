using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Groups;

[PublicAPI]
public struct PlayerReference
{
	public long peerId;

	public string name;

	public static PlayerReference fromPlayerId(long id)
	{
		return ZNet.instance.m_players.Where((PlayerInfo p) => ((ZDOID)(ref p.m_characterID)).UserID == id).Select(fromPlayerInfo).FirstOrDefault();
	}

	public static PlayerReference fromPlayerInfo(PlayerInfo playerInfo)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		PlayerReference result = default(PlayerReference);
		result.peerId = ((ZDOID)(ref playerInfo.m_characterID)).UserID;
		result.name = playerInfo.m_name ?? "";
		return result;
	}

	public static PlayerReference fromPlayer(Player player)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Player player2 = player;
		if (!((Object)(object)player2 == (Object)(object)Player.m_localPlayer))
		{
			return fromPlayerInfo(((IEnumerable<PlayerInfo>)ZNet.instance.m_players).FirstOrDefault((Func<PlayerInfo, bool>)((PlayerInfo info) => info.m_characterID == ((Character)player2).GetZDOID())));
		}
		PlayerReference result = default(PlayerReference);
		result.peerId = ZDOMan.GetSessionID();
		result.name = Game.instance.GetPlayerProfile().GetName();
		return result;
	}

	public static bool operator !=(PlayerReference a, PlayerReference b)
	{
		return !(a == b);
	}

	public static bool operator ==(PlayerReference a, PlayerReference b)
	{
		if (a.peerId == b.peerId)
		{
			return a.name == b.name;
		}
		return false;
	}

	public bool Equals(PlayerReference other)
	{
		return this == other;
	}

	public override bool Equals(object? obj)
	{
		if (obj is PlayerReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (peerId.GetHashCode() * 397) ^ (name?.GetHashCode() ?? 0);
	}

	public override string ToString()
	{
		return $"{peerId}:{name}";
	}

	public static PlayerReference fromString(string str)
	{
		string[] array = str.Split(new char[1] { ':' });
		PlayerReference result = default(PlayerReference);
		result.peerId = long.Parse(array[0]);
		result.name = array[1];
		return result;
	}
}
