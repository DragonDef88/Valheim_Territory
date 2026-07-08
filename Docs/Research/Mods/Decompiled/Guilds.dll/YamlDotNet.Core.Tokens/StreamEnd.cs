namespace YamlDotNet.Core.Tokens;

internal sealed class StreamEnd : Token
{
	public StreamEnd()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public StreamEnd(Mark start, Mark end)
		: base(start, end)
	{
	}
}
