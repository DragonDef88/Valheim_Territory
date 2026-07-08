namespace YamlDotNet.Core.Events;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
{
	public string Value { get; }

	public bool IsInline { get; }

	internal override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType Type => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.Comment;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment(string value, bool isInline)
		: this(value, isInline, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment(string value, bool isInline, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		Value = value;
		IsInline = isInline;
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override string ToString()
	{
		return (IsInline ? "Inline" : "Block") + " Comment [" + Value + "]";
	}
}
