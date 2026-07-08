namespace YamlDotNet.Core.Events;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
{
	public override int NestingIncrease => -1;

	internal override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType Type => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.StreamEnd;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd()
		: this(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public override string ToString()
	{
		return "Stream end";
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
