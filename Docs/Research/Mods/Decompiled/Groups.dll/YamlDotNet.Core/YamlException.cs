using System;

namespace YamlDotNet.Core;

internal class YamlException : Exception
{
	public Mark Start { get; }

	public Mark End { get; }

	public YamlException(string message)
		: this(Mark.Empty, Mark.Empty, message)
	{
	}

	public YamlException(Mark start, Mark end, string message)
		: this(start, end, message, null)
	{
	}

	public YamlException(Mark start, Mark end, string message, Exception? innerException)
		: base($"({start}) - ({end}): {message}", innerException)
	{
		Start = start;
		End = end;
	}

	public YamlException(string message, Exception inner)
		: this(Mark.Empty, Mark.Empty, message, inner)
	{
	}
}
