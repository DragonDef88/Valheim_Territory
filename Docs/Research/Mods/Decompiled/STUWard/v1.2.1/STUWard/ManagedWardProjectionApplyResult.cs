namespace STUWard;

internal readonly struct ManagedWardProjectionApplyResult
{
	internal bool AccountChanged { get; }

	internal bool GuildChanged { get; }

	internal bool AnyChanged
	{
		get
		{
			if (!AccountChanged)
			{
				return GuildChanged;
			}
			return true;
		}
	}

	internal ManagedWardProjectionApplyResult(bool accountChanged, bool guildChanged)
	{
		AccountChanged = accountChanged;
		GuildChanged = guildChanged;
	}
}
