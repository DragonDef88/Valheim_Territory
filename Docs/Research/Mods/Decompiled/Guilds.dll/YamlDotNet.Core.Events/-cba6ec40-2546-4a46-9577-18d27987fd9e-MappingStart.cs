namespace YamlDotNet.Core.Events;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent
{
	public override int NestingIncrease => 1;

	internal override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType Type => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.MappingStart;

	public bool IsImplicit { get; }

	public override bool IsCanonical => !IsImplicit;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle Style { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag, bool isImplicit, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle style, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(anchor, tag, start, end)
	{
		IsImplicit = isImplicit;
		Style = style;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag, bool isImplicit, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle style)
		: this(anchor, tag, isImplicit, style, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart()
		: this(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName.Empty, isImplicit: true, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle.Any, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public override string ToString()
	{
		return $"Mapping start [anchor = {base.Anchor}, tag = {base.Tag}, isImplicit = {IsImplicit}, style = {Style}]";
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
