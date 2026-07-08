using System;
using System.Collections;

namespace YamlDotNet.Serialization;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory
{
	object Create(Type type);

	object? CreatePrimitive(Type type);

	bool GetDictionary(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments);

	Type GetValueType(Type type);

	void ExecuteOnDeserializing(object value);

	void ExecuteOnDeserialized(object value);

	void ExecuteOnSerializing(object value);

	void ExecuteOnSerialized(object value);
}
