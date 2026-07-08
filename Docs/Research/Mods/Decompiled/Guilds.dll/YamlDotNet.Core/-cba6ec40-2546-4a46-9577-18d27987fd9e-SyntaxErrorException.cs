using System;

namespace YamlDotNet.Core;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException
{
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(string message)
		: base(message)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end, string message)
		: base(in start, in end, message)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
