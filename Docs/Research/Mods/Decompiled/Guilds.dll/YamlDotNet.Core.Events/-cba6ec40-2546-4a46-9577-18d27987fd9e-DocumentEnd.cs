namespace YamlDotNet.Core.Events;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
{
	public override int NestingIncrease => -1;

	internal override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType Type => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.DocumentEnd;

	public bool IsImplicit { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd(bool isImplicit, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		IsImplicit = isImplicit;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd(bool isImplicit)
		: this(isImplicit, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public override string ToString()
	{
		return $"Document end [isImplicit = {IsImplicit}]";
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
