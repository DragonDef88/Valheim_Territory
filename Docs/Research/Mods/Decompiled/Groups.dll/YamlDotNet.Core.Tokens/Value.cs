namespace YamlDotNet.Core.Tokens;

internal sealed class Value : Token
{
	public Value()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public Value(Mark start, Mark end)
		: base(start, end)
	{
	}
}
