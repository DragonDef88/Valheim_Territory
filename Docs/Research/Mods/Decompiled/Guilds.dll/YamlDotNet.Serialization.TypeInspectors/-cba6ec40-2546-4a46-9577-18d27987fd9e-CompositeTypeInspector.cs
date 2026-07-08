using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization.TypeInspectors;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECompositeTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeInspectorSkeleton
{
	private readonly IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector> typeInspectors;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECompositeTypeInspector(params _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector[] typeInspectors)
		: this((IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector>)typeInspectors)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECompositeTypeInspector(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector> typeInspectors)
	{
		this.typeInspectors = typeInspectors?.ToList() ?? throw new ArgumentNullException("typeInspectors");
	}

	public override string GetEnumName(Type enumType, string name)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector in typeInspectors)
		{
			try
			{
				return typeInspector.GetEnumName(enumType, name);
			}
			catch
			{
			}
		}
		throw new ArgumentOutOfRangeException("enumType,name", "Name not found on enum type");
	}

	public override string GetEnumValue(object enumValue)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector in typeInspectors)
		{
			try
			{
				return typeInspector.GetEnumValue(enumValue);
			}
			catch
			{
			}
		}
		throw new ArgumentOutOfRangeException("enumValue", $"Value not found for ({enumValue})");
	}

	public override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container)
	{
		Type type2 = type;
		object container2 = container;
		return typeInspectors.SelectMany((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector i) => i.GetProperties(type2, container2));
	}
}
