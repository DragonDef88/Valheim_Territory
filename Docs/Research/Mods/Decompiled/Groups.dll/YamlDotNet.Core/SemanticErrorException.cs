using System;

namespace YamlDotNet.Core;

internal class SemanticErrorException : YamlException
{
	public SemanticErrorException(string message)
		: base(message)
	{
	}

	public SemanticErrorException(Mark start, Mark end, string message)
		: base(start, end, message)
	{
	}

	public SemanticErrorException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
