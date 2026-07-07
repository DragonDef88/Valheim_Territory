using System;
using Steamworks;

public struct ServerJoinDataSteamUser : IEquatable<ServerJoinDataSteamUser>
{
	public const string c_TypeName = "Steam user";

	public readonly CSteamID m_joinUserID;

	public bool IsValid
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			CSteamID joinUserID = m_joinUserID;
			return ((CSteamID)(ref joinUserID)).IsValid();
		}
	}

	public ServerJoinDataSteamUser(ulong joinUserID)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		m_joinUserID = new CSteamID(joinUserID);
	}

	public ServerJoinDataSteamUser(CSteamID joinUserID)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		m_joinUserID = joinUserID;
	}

	public string GetDataName()
	{
		return "Steam user";
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is ServerJoinDataSteamUser other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(ServerJoinDataSteamUser other)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		CSteamID joinUserID = m_joinUserID;
		return ((CSteamID)(ref joinUserID)).Equals(other.m_joinUserID);
	}

	public override int GetHashCode()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		int num = -995281327 * -1521134295;
		CSteamID joinUserID = m_joinUserID;
		return num + ((object)(CSteamID)(ref joinUserID)).GetHashCode();
	}

	public static bool operator ==(ServerJoinDataSteamUser left, ServerJoinDataSteamUser right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ServerJoinDataSteamUser left, ServerJoinDataSteamUser right)
	{
		return !(left == right);
	}

	public override string ToString()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		CSteamID joinUserID = m_joinUserID;
		return ((object)(CSteamID)(ref joinUserID)).ToString();
	}
}
