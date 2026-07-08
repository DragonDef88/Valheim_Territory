namespace YamlDotNet.Core;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode
{
	public static int CombineHashCodes(int h1, int h2)
	{
		return ((h1 << 5) + h1) ^ h2;
	}

	public static int CombineHashCodes(int h1, object? o2)
	{
		return CombineHashCodes(h1, GetHashCode(o2));
	}

	private static int GetHashCode(object? obj)
	{
		return obj?.GetHashCode() ?? 0;
	}
}
