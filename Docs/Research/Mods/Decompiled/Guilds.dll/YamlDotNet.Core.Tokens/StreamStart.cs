namespace YamlDotNet.Core.Tokens;

internal sealed class StreamStart : Token
{
	public StreamStart()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public StreamStart(Mark start, Mark end)
		: base(start, end)
	{
	}
}
