namespace YamlDotNet.Core.Tokens;

internal sealed class FlowEntry : Token
{
	public FlowEntry()
		: this(Mark.Empty, Mark.Empty)
	{
	}

	public FlowEntry(Mark start, Mark end)
		: base(start, end)
	{
	}
}
