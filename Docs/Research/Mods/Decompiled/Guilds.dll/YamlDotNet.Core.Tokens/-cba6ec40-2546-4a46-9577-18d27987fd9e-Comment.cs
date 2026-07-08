using System;

namespace YamlDotNet.Core.Tokens;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken
{
	public string Value { get; }

	public bool IsInline { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment(string value, bool isInline)
		: this(value, isInline, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment(string value, bool isInline, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		Value = value ?? throw new ArgumentNullException("value");
		IsInline = isInline;
	}
}
