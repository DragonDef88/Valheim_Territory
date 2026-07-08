using System.Reflection;

namespace YamlDotNet;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyInfoExtensions
{
	public static object? ReadValue(this PropertyInfo property, object target)
	{
		return property.GetValue(target, null);
	}
}
