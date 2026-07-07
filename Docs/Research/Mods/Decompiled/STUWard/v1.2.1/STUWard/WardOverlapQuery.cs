namespace STUWard;

internal readonly struct WardOverlapQuery
{
	internal float X { get; }

	internal float Z { get; }

	internal float Radius { get; }

	internal long OwnerPlayerId { get; }

	internal int GuildId { get; }

	internal int IgnoredAreaId { get; }

	internal WardOverlapQuery(float x, float z, float radius, long ownerPlayerId, int guildId, int ignoredAreaId = 0)
	{
		X = x;
		Z = z;
		Radius = radius;
		OwnerPlayerId = ownerPlayerId;
		GuildId = guildId;
		IgnoredAreaId = ignoredAreaId;
	}
}
