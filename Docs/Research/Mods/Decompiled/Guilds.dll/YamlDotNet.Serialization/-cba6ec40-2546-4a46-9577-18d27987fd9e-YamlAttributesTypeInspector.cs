using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributesTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeInspectorSkeleton
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributesTypeInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor)
	{
		this.innerTypeDescriptor = innerTypeDescriptor;
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
		return from p in (from p in innerTypeDescriptor.GetProperties(type, container)
				where p.GetCustomAttribute<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlIgnoreAttribute>() == null
				select p).Select((Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor>)delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor p)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor(p);
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute customAttribute = p.GetCustomAttribute<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute>();
				if (customAttribute != null)
				{
					if (customAttribute.SerializeAs != null)
					{
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor2.TypeOverride = customAttribute.SerializeAs;
					}
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor2.Order = customAttribute.Order;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor2.ScalarStyle = customAttribute.ScalarStyle;
					if (customAttribute.Alias != null)
					{
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor2.Name = customAttribute.Alias;
					}
				}
				return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyDescriptor2;
			})
			orderby p.Order
			select p;
	}
}
