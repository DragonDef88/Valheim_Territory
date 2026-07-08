using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Splatform;
using UnityEngine;

namespace Guilds;

[PublicAPI]
[TypeConverter(typeof(PlayerReferenceTypeConverter))]
public struct PlayerReference
{
	public string id;

	public string name;

	public static PlayerReference fromPlayerInfo(PlayerInfo playerInfo)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		PlayerReference result = default(PlayerReference);
		result.id = ((object)(PlatformUserID)(ref playerInfo.m_userInfo.m_id)).ToString();
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
		return forOwnPlayer();
	}

	public static PlayerReference forOwnPlayer()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		PlayerReference result = default(PlayerReference);
		IDistributionPlatform distributionPlatform = PlatformManager.DistributionPlatform;
		object obj;
		if (distributionPlatform == null)
		{
			obj = null;
		}
		else
		{
			PlatformUserID platformUserID = ((IUser)distributionPlatform.LocalUser).PlatformUserID;
			obj = ((object)(PlatformUserID)(ref platformUserID)).ToString();
		}
		if (obj == null)
		{
			obj = "";
		}
		result.id = (string)obj;
		result.name = Game.instance.GetPlayerProfile().GetName();
		return result;
	}

	public static PlayerReference fromRPC(ZRpc? rpc)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		ZRpc rpc2 = rpc;
		if (rpc2 != null)
		{
			return fromPlayerInfo(ZNet.instance.m_players.First((PlayerInfo p) => ((object)(PlatformUserID)(ref p.m_userInfo.m_id)).ToString().EndsWith(rpc2.m_socket.GetHostName())));
		}
		return forOwnPlayer();
	}

	public static bool operator !=(PlayerReference a, PlayerReference b)
	{
		return !(a == b);
	}

	public static bool operator ==(PlayerReference a, PlayerReference b)
	{
		if (a.id == b.id)
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
		return (id.GetHashCode() * 397) ^ name.GetHashCode();
	}

	public override string ToString()
	{
		return id + ":" + name;
	}

	public static PlayerReference fromString(string str)
	{
		string[] array = str.Split(new char[1] { ':' });
		PlayerReference result = default(PlayerReference);
		result.id = array[0];
		result.name = array[1];
		return result;
	}
}
