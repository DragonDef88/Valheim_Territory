using System;

namespace YamlDotNet.Core;

internal sealed class SyntaxErrorException : YamlException
{
	public SyntaxErrorException(string message)
		: base(message)
	{
	}

	public SyntaxErrorException(Mark start, Mark end, string message)
		: base(start, end, message)
	{
	}

	public SyntaxErrorException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
