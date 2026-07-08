namespace YamlDotNet.Core.Events;

internal sealed class AnchorAlias : ParsingEvent
{
	internal override EventType Type => EventType.Alias;

	public AnchorName Value { get; }

	public AnchorAlias(AnchorName value, Mark start, Mark end)
		: base(start, end)
	{
		if (value.IsEmpty)
		{
			throw new YamlException(start, end, "Anchor value must not be empty.");
		}
		Value = value;
	}

	public AnchorAlias(AnchorName value)
		: this(value, Mark.Empty, Mark.Empty)
	{
	}

	public override string ToString()
	{
		return $"Alias [value = {Value}]";
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
