using System;

namespace YamlDotNet.Core.Tokens;

internal sealed class Scalar : Token
{
	public string Value { get; }

	public ScalarStyle Style { get; }

	public Scalar(string value)
		: this(value, ScalarStyle.Any)
	{
	}

	public Scalar(string value, ScalarStyle style)
		: this(value, style, Mark.Empty, Mark.Empty)
	{
	}

	public Scalar(string value, ScalarStyle style, Mark start, Mark end)
		: base(start, end)
	{
		Value = value ?? throw new ArgumentNullException("value");
		Style = style;
	}
}
