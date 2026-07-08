namespace YamlDotNet.Core.Tokens;

internal sealed class BlockMappingStart : Token
{
	public BlockMappingStart()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public BlockMappingStart(Mark start, Mark end)
		: base(start, end)
	{
	}
}
