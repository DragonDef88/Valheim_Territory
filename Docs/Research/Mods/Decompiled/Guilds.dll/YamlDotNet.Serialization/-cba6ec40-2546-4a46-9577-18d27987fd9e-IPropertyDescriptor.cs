using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor
{
	string Name { get; }

	bool AllowNulls { get; }

	bool CanWrite { get; }

	Type Type { get; }

	Type? TypeOverride { get; set; }

	int Order { get; set; }

	_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle ScalarStyle { get; set; }

	bool Required { get; }

	Type? ConverterType { get; }

	T? GetCustomAttribute<T>() where T : Attribute;

	_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor Read(object target);

	void Write(object target, object? value);
}
