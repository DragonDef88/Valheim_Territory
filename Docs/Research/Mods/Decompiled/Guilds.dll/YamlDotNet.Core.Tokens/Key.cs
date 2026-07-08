namespace YamlDotNet.Core.Tokens;

internal sealed class Key : Token
{
	public Key()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public Key(Mark start, Mark end)
		: base(start, end)
	{
	}
}
