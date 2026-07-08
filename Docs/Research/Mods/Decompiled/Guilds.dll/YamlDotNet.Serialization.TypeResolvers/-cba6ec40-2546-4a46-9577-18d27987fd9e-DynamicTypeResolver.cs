using System;

namespace YamlDotNet.Serialization.TypeResolvers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDynamicTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver
{
	public Type Resolve(Type staticType, object? actualValue)
	{
		if (actualValue == null)
		{
			return staticType;
		}
		return actualValue.GetType();
	}
}
