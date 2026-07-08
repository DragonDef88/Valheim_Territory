using System;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NamingConventions;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECamelCaseNamingConvention : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention
{
	public static readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention Instance = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECamelCaseNamingConvention();

	[Obsolete("Use the Instance static field instead of creating new instances")]
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECamelCaseNamingConvention()
	{
	}

	public string Apply(string value)
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringExtensions.ToCamelCase(value);
	}

	public string Reverse(string value)
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringExtensions.ToPascalCase(value);
	}
}
