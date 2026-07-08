using System;
using YamlDotNet.Serialization;

namespace YamlDotNet.Helpers;

internal interface IFsharpHelper
{
	bool IsOptionType(Type t);

	Type? GetOptionUnderlyingType(Type t);

	object? GetValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor objectDescriptor);

	bool IsFsharpListType(Type t);

	object? CreateFsharpListFromArray(Type t, Type itemsType, Array arr);
}
