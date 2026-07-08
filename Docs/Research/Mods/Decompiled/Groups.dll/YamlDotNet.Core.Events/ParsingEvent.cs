using System;

namespace YamlDotNet.Core.Events;

internal abstract class ParsingEvent
{
	public virtual int NestingIncrease => 0;

	internal abstract EventType Type { get; }

	public Mark Start { get; }

	public Mark End { get; }

	public abstract void Accept(IParsingEventVisitor visitor);

	internal ParsingEvent(Mark start, Mark end)
	{
		Start = start ?? throw new ArgumentNullException("start");
		End = end ?? throw new ArgumentNullException("end");
	}
}
