using BepInEx.Configuration;
using JetBrains.Annotations;

namespace ServerSync;

[PublicAPI]
public abstract class OwnConfigEntryBase
{
	public object? LocalBaseValue;

	public bool SynchronizedConfig = true;

	public abstract ConfigEntryBase BaseConfig { get; }
}
