namespace STUWard;

internal readonly struct WardOverlapArea
{
	internal int Id { get; }

	internal float X { get; }

	internal float Z { get; }

	internal float Radius { get; }

	internal long OwnerPlayerId { get; }

	internal int GuildId { get; }

	internal WardOverlapArea(int id, float x, float z, float radius, long ownerPlayerId, int guildId)
	{
		Id = id;
		X = x;
		Z = z;
		Radius = radius;
		OwnerPlayerId = ownerPlayerId;
		GuildId = guildId;
	}
}
