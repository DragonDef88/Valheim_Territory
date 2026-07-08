using System;

namespace YamlDotNet.Core;

internal sealed class MaximumRecursionLevelReachedException : YamlException
{
	public MaximumRecursionLevelReachedException(string message)
		: base(message)
	{
	}

	public MaximumRecursionLevelReachedException(Mark start, Mark end, string message)
		: base(start, end, message)
	{
	}

	public MaximumRecursionLevelReachedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
