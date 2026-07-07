using Splatform;

internal struct ServerJoinDataAndHostUser
{
	public ServerJoinData m_joinData;

	public PlatformUserID m_hostUser;

	public ServerJoinDataAndHostUser(ServerJoinData joinData, PlatformUserID hostUser)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		m_joinData = joinData;
		m_hostUser = hostUser;
	}
}
