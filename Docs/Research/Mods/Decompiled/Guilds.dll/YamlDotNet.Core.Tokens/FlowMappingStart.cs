namespace YamlDotNet.Core.Tokens;

internal sealed class FlowMappingStart : Token
{
	public FlowMappingStart()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public FlowMappingStart(Mark start, Mark end)
		: base(start, end)
	{
	}
}
