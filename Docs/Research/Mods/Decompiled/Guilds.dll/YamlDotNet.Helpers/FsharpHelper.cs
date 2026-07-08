using System;
using YamlDotNet.Serialization;

namespace YamlDotNet.Helpers;

internal static class FsharpHelper
{
	public static IFsharpHelper? Instance { get; set; }

	public static bool IsOptionType(Type t)
	{
		return Instance?.IsOptionType(t) ?? false;
	}

	public static Type? GetOptionUnderlyingType(Type t)
	{
		return Instance?.GetOptionUnderlyingType(t);
	}

	public static object? GetValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor objectDescriptor)
	{
		return Instance?.GetValue(objectDescriptor);
	}

	public static bool IsFsharpListType(Type t)
	{
		return Instance?.IsFsharpListType(t) ?? false;
	}

	public static object? CreateFsharpListFromArray(Type t, Type itemsType, Array arr)
	{
		return Instance?.CreateFsharpListFromArray(t, itemsType, arr);
	}
}
