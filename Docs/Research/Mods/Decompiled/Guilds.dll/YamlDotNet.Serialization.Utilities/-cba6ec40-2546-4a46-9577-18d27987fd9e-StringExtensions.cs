using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YamlDotNet.Serialization.Utilities;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringExtensions
{
	private static string ToCamelOrPascalCase(string str, Func<char, char> firstLetterTransform)
	{
		string text = Regex.Replace(str, "([_\\-])(?<char>[a-z])", (Match match) => match.Groups["char"].Value.ToUpperInvariant(), RegexOptions.IgnoreCase);
		return firstLetterTransform(text[0]) + text.Substring(1);
	}

	public static string ToCamelCase(this string str)
	{
		return ToCamelOrPascalCase(str, char.ToLowerInvariant);
	}

	public static string ToPascalCase(this string str)
	{
		return ToCamelOrPascalCase(str, char.ToUpperInvariant);
	}

	public static string FromCamelCase(this string str, string separator)
	{
		string separator2 = separator;
		str = char.ToLower(str[0], CultureInfo.InvariantCulture) + str.Substring(1);
		str = Regex.Replace(ToCamelCase(str), "(?<char>[A-Z])", (Match match) => separator2 + match.Groups["char"].Value.ToLowerInvariant());
		return str;
	}
}
