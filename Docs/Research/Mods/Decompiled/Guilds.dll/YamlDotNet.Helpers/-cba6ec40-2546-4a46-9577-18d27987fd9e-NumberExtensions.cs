namespace YamlDotNet.Helpers;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENumberExtensions
{
	public static bool IsPowerOfTwo(this int value)
	{
		return (value & (value - 1)) == 0;
	}
}
