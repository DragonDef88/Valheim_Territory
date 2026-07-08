using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization.TypeInspectors;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENamingConventionTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeInspectorSkeleton
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention namingConvention;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENamingConventionTypeInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention namingConvention)
	{
		this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
		this.namingConvention = namingConvention ?? throw new ArgumentNullException("namingConvention");
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
		return innerTypeDescriptor.GetProperties(type, container).Select(delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor p)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute customAttribute = p.GetCustomAttribute<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute>();
			return (customAttribute != null && !customAttribute.ApplyNamingConventions) ? p : new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor(p)
			{
				Name = namingConvention.Apply(p.Name)
			};
		});
	}
}
