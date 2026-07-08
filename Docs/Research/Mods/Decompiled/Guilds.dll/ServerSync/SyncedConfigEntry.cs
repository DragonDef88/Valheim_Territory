using BepInEx.Configuration;
using JetBrains.Annotations;

namespace ServerSync;

[PublicAPI]
public class SyncedConfigEntry<T> : OwnConfigEntryBase
{
	public readonly ConfigEntry<T> SourceConfig;

	public override ConfigEntryBase BaseConfig => (ConfigEntryBase)(object)SourceConfig;

	public T Value
	{
		get
		{
			return SourceConfig.Value;
		}
		set
		{
			SourceConfig.Value = value;
		}
	}

	public SyncedConfigEntry(ConfigEntry<T> sourceConfig)
	{
		SourceConfig = sourceConfig;
		base._002Ector();
	}

	public void AssignLocalValue(T value)
	{
		if (LocalBaseValue == null)
		{
			Value = value;
		}
		else
		{
			LocalBaseValue = value;
		}
	}
}
