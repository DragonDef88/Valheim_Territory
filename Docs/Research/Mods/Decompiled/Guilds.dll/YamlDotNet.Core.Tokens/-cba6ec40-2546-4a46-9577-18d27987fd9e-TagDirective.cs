using System;
using System.Text.RegularExpressions;

namespace YamlDotNet.Core.Tokens;

public class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken
{
	private static readonly Regex TagHandlePattern = new Regex("^!([0-9A-Za-z_\\-]*!)?$", RegexOptions.Compiled);

	public string Handle { get; }

	public string Prefix { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective(string handle, string prefix)
		: this(handle, prefix, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective(string handle, string prefix, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		if (string.IsNullOrEmpty(handle))
		{
			throw new ArgumentNullException("handle", "Tag handle must not be empty.");
		}
		if (!TagHandlePattern.IsMatch(handle))
		{
			throw new ArgumentException("Tag handle must start and end with '!' and contain alphanumerical characters only.", "handle");
		}
		Handle = handle;
		if (string.IsNullOrEmpty(prefix))
		{
			throw new ArgumentNullException("prefix", "Tag prefix must not be empty.");
		}
		Prefix = prefix;
	}

	public override bool Equals(object? obj)
	{
		if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective2 && Handle.Equals(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective2.Handle))
		{
			return Prefix.Equals(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective2.Prefix);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode() ^ Prefix.GetHashCode();
	}

	public override string ToString()
	{
		return Handle + " => " + Prefix;
	}
}
