using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NamingConventions;

internal sealed class LowerCaseNamingConvention : INamingConvention
{
	public static readonly INamingConvention Instance = new LowerCaseNamingConvention();

	private LowerCaseNamingConvention()
	{
	}

	public string Apply(string value)
	{
		return StringExtensions.ToCamelCase(value).ToLower();
	}
}
