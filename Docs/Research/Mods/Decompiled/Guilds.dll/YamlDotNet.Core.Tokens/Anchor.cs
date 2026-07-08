using System;

namespace YamlDotNet.Core.Tokens;

internal class Anchor : Token
{
	public AnchorName Value { get; }

	public Anchor(AnchorName value)
		: this(value, Mark.Empty, Mark.Empty)
	{
	}

	public Anchor(AnchorName value, Mark start, Mark end)
		: base(start, end)
	{
		if (value.IsEmpty)
		{
			throw new ArgumentNullException("value");
		}
		Value = value;
	}
}
