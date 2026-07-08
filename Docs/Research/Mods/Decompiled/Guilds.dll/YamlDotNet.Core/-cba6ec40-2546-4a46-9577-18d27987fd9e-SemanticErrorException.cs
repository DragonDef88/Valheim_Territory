using System;

namespace YamlDotNet.Core;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException
{
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(string message)
		: base(message)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end, string message)
		: base(in start, in end, message)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
