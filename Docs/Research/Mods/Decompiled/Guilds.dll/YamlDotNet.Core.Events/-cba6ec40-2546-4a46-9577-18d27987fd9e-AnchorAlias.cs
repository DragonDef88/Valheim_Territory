namespace YamlDotNet.Core.Events;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
{
	internal override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType Type => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.Alias;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName Value { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		if (value.IsEmpty)
		{
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, "Anchor value must not be empty.");
		}
		Value = value;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName value)
		: this(value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public override string ToString()
	{
		return $"Alias [value = {Value}]";
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
