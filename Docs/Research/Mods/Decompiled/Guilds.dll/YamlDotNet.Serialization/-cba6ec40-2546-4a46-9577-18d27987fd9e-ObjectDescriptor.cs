using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor
{
	public object? Value { get; private set; }

	public Type Type { get; private set; }

	public Type StaticType { get; private set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle ScalarStyle { get; private set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(object? value, Type type, Type staticType)
		: this(value, type, staticType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(object? value, Type type, Type staticType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle scalarStyle)
	{
		Value = value;
		Type = type ?? throw new ArgumentNullException("type");
		StaticType = staticType ?? throw new ArgumentNullException("staticType");
		ScalarStyle = scalarStyle;
	}
}
