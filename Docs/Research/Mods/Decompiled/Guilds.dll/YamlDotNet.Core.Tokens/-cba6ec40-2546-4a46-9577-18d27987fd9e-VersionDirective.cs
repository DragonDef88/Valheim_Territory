namespace YamlDotNet.Core.Tokens;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken
{
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion Version { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion version)
		: this(version, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion version, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		Version = version;
	}

	public override bool Equals(object? obj)
	{
		if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective2)
		{
			return Version.Equals(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective2.Version);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Version.GetHashCode();
	}
}
