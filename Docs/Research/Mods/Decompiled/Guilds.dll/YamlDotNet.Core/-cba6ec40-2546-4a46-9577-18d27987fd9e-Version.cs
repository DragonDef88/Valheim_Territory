using System;

namespace YamlDotNet.Core;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion
{
	public int Major { get; }

	public int Minor { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion(int major, int minor)
	{
		if (major < 0)
		{
			throw new ArgumentOutOfRangeException("major", $"{major} should be >= 0");
		}
		Major = major;
		if (minor < 0)
		{
			throw new ArgumentOutOfRangeException("minor", $"{minor} should be >= 0");
		}
		Minor = minor;
	}

	public override bool Equals(object? obj)
	{
		if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion2 && Major == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion2.Major)
		{
			return Minor == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion2.Minor;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(Major.GetHashCode(), Minor.GetHashCode());
	}
}
