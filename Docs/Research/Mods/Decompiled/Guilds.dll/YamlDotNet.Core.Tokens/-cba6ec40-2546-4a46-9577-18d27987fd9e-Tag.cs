using System;

namespace YamlDotNet.Core.Tokens;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken
{
	public string Handle { get; }

	public string Suffix { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag(string handle, string suffix)
		: this(handle, suffix, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag(string handle, string suffix, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		Handle = handle ?? throw new ArgumentNullException("handle");
		Suffix = suffix ?? throw new ArgumentNullException("suffix");
	}
}
