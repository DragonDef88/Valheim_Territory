using System;

namespace YamlDotNet.Core;

internal sealed class ForwardAnchorNotSupportedException : YamlException
{
	public ForwardAnchorNotSupportedException(string message)
		: base(message)
	{
	}

	public ForwardAnchorNotSupportedException(Mark start, Mark end, string message)
		: base(start, end, message)
	{
	}

	public ForwardAnchorNotSupportedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
