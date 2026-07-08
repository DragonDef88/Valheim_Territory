using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core.Events;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
{
	public override int NestingIncrease => 1;

	internal override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType Type => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.DocumentStart;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection? Tags { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective? Version { get; }

	public bool IsImplicit { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective? version, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection? tags, bool isImplicit, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: base(in start, in end)
	{
		Version = version;
		Tags = tags;
		IsImplicit = isImplicit;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective? version, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection? tags, bool isImplicit)
		: this(version, tags, isImplicit, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
		: this(null, null, isImplicit: true, start, end)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart()
		: this(null, null, isImplicit: true, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty)
	{
	}

	public override string ToString()
	{
		return $"Document start [isImplicit = {IsImplicit}]";
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
