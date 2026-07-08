using System;

namespace YamlDotNet.Core.Tokens;

internal abstract class Token
{
	public Mark Start { get; }

	public Mark End { get; }

	protected Token(Mark start, Mark end)
	{
		Start = start ?? throw new ArgumentNullException("start");
		End = end ?? throw new ArgumentNullException("end");
	}
}
