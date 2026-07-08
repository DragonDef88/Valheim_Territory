namespace YamlDotNet.Core.Events;

internal abstract class NodeEvent : ParsingEvent
{
	public AnchorName Anchor { get; }

	public TagName Tag { get; }

	public abstract bool IsCanonical { get; }

	protected NodeEvent(AnchorName anchor, TagName tag, Mark start, Mark end)
		: base(start, end)
	{
		Anchor = anchor;
		Tag = tag;
	}

	protected NodeEvent(AnchorName anchor, TagName tag)
		: this(anchor, tag, Mark.Empty, Mark.Empty)
	{
	}
}
