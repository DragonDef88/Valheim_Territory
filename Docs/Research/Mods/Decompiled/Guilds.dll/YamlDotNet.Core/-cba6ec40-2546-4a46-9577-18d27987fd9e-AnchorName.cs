using System;
using System.Text.RegularExpressions;

namespace YamlDotNet.Core;

public readonly struct _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName : IEquatable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName>
{
	public static readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName Empty;

	private static readonly Regex AnchorPattern = new Regex("^[^\\[\\]\\{\\},]+$", RegexOptions.Compiled);

	private readonly string? value;

	public string Value => value ?? throw new InvalidOperationException("Cannot read the Value of an empty anchor");

	public bool IsEmpty => value == null;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName(string value)
	{
		this.value = value ?? throw new ArgumentNullException("value");
		if (!AnchorPattern.IsMatch(value))
		{
			throw new ArgumentException("Anchor cannot be empty or contain disallowed characters: []{},\nThe value was '" + value + "'.", "value");
		}
	}

	public override string ToString()
	{
		return value ?? "[empty]";
	}

	public bool Equals(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName other)
	{
		return object.Equals(value, other.value);
	}

	public override bool Equals(object? obj)
	{
		if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value?.GetHashCode() ?? 0;
	}

	public static bool operator ==(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName right)
	{
		return !(left == right);
	}

	public static implicit operator _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName(string? value)
	{
		if (value != null)
		{
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName(value);
		}
		return Empty;
	}
}
