using System.Globalization;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NamingConventions;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELowerCaseNamingConvention : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention
{
	public static readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention Instance = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELowerCaseNamingConvention();

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELowerCaseNamingConvention()
	{
	}

	public string Apply(string value)
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringExtensions.ToCamelCase(value).ToLower(CultureInfo.InvariantCulture);
	}

	public string Reverse(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		return char.ToUpperInvariant(value[0]) + value.Substring(1);
	}
}
