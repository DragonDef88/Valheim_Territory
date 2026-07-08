using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEnumerableNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		Type type;
		if (expectedType == typeof(IEnumerable))
		{
			type = typeof(object);
		}
		else
		{
			Type implementationOfOpenGenericInterface = expectedType.GetImplementationOfOpenGenericInterface(typeof(IEnumerable<>));
			if (implementationOfOpenGenericInterface != expectedType)
			{
				value = null;
				return false;
			}
			type = implementationOfOpenGenericInterface.GetGenericArguments()[0];
		}
		Type arg = typeof(List<>).MakeGenericType(type);
		value = nestedObjectDeserializer(parser, arg);
		return true;
	}
}
