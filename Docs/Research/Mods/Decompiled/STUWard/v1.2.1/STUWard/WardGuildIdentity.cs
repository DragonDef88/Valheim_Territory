namespace STUWard;

internal readonly struct WardGuildIdentity
{
	internal int Id { get; }

	internal string Name { get; }

	internal WardGuildIdentity(int id, string name)
	{
		Id = id;
		Name = name;
	}
}
