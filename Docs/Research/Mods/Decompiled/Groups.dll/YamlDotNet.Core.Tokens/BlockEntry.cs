namespace YamlDotNet.Core.Tokens;

internal sealed class BlockEntry : Token
{
	public BlockEntry()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public BlockEntry(Mark start, Mark end)
		: base(start, end)
	{
	}
}
