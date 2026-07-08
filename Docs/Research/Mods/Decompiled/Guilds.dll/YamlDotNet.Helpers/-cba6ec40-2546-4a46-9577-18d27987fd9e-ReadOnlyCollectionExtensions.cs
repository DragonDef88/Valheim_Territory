using System.Collections.Generic;

namespace YamlDotNet.Helpers;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadOnlyCollectionExtensions
{
	public static IReadOnlyList<T> AsReadonlyList<T>(this List<T> list)
	{
		return list;
	}

	public static IReadOnlyDictionary<TKey, TValue> AsReadonlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		return dictionary;
	}
}
