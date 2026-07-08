using System;
using YamlDotNet.Serialization;

namespace YamlDotNet.Helpers;

internal class NullFsharpHelper : IFsharpHelper
{
	public object? CreateFsharpListFromArray(Type t, Type itemsType, Array arr)
	{
		return null;
	}

	public Type? GetOptionUnderlyingType(Type t)
	{
		return null;
	}

	public object? GetValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor objectDescriptor)
	{
		return null;
	}

	public bool IsFsharpListType(Type t)
	{
		return false;
	}

	public bool IsOptionType(Type t)
	{
		return false;
	}
}
