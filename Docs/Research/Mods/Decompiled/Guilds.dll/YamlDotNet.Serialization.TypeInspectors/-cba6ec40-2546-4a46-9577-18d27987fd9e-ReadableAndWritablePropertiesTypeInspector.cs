using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization.TypeInspectors;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadableAndWritablePropertiesTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeInspectorSkeleton
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadableAndWritablePropertiesTypeInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor)
	{
		this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
	}

	public override string GetEnumName(Type enumType, string name)
	{
		return innerTypeDescriptor.GetEnumName(enumType, name);
	}

	public override string GetEnumValue(object enumValue)
	{
		return innerTypeDescriptor.GetEnumValue(enumValue);
	}

	public override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return from p in innerTypeDescriptor.GetProperties(type, container)
			where p.CanWrite
			select p;
	}
}
