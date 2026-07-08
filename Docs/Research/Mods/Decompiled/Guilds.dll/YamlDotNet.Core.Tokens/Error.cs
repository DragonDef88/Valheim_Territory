namespace YamlDotNet.Core.Tokens;

internal class Error : Token
{
	internal string Value { get; }

	internal Error(string value, Mark start, Mark end)
		: base(start, end)
	{
		Value = value;
	}
}
