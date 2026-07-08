using System;

namespace YamlDotNet.Core;

public readonly struct _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName : IEquatable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName>
{
	public static readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName Empty;

	private readonly string? value;

	public string Value => value ?? throw new InvalidOperationException("Cannot read the Value of a non-specific tag");

	public bool IsEmpty => value == null;

	public bool IsNonSpecific
	{
		get
		{
			if (!IsEmpty)
			{
				if (!(value == "!"))
				{
					return value == "?";
				}
				return true;
			}
			return false;
		}
	}

	public bool IsLocal
	{
		get
		{
			if (!IsEmpty)
			{
				return Value[0] == '!';
			}
			return false;
		}
	}

	public bool IsGlobal
	{
		get
		{
			if (!IsEmpty)
			{
				return !IsLocal;
			}
			return false;
		}
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName(string value)
	{
		this.value = value ?? throw new ArgumentNullException("value");
		if (value.Length == 0)
		{
			throw new ArgumentException("Tag value must not be empty.", "value");
		}
		if (IsGlobal && !Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
		{
			throw new ArgumentException("Global tags must be valid URIs.", "value");
		}
	}

	public override string ToString()
	{
		return value ?? "?";
	}

	public bool Equals(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName other)
	{
		return object.Equals(value, other.value);
	}

	public override bool Equals(object? obj)
	{
		if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value?.GetHashCode() ?? 0;
	}

	public static bool operator ==(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName right)
	{
		return !(left == right);
	}

	public static bool operator ==(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName left, string right)
	{
		return object.Equals(left.value, right);
	}

	public static bool operator !=(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName left, string right)
	{
		return !(left == right);
	}

	public static implicit operator _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName(string? value)
	{
		if (value != null)
		{
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName(value);
		}
		return Empty;
	}
}
