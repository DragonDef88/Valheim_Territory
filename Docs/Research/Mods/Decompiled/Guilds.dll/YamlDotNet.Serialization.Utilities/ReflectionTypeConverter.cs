using System;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Serialization.Utilities;

internal class ReflectionTypeConverter : ITypeConverter
{
	public object? ChangeType(object? value, Type expectedType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector)
	{
		return ChangeType(value, expectedType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENullNamingConvention.Instance, typeInspector);
	}

	public object? ChangeType(object? value, Type expectedType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector)
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeConverter.ChangeType(value, expectedType, enumNamingConvention, typeInspector);
	}
}
