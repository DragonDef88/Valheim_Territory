using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.TypeInspectors;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadablePropertiesTypeInspector : ReflectionTypeInspector
{
	protected class ReflectionPropertyDescriptor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor
	{
		private readonly PropertyInfo propertyInfo;

		private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver;

		public string Name => propertyInfo.Name;

		public bool Required => propertyInfo.IsRequired();

		public Type Type => propertyInfo.PropertyType;

		public Type? TypeOverride { get; set; }

		public Type? ConverterType { get; set; }

		public bool AllowNulls => propertyInfo.AcceptsNull();

		public int Order { get; set; }

		public bool CanWrite => propertyInfo.CanWrite;

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle ScalarStyle { get; set; }

		public ReflectionPropertyDescriptor(PropertyInfo propertyInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver)
		{
			this.propertyInfo = propertyInfo ?? throw new ArgumentNullException("propertyInfo");
			this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
			ScalarStyle = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any;
			YamlConverterAttribute customAttribute = propertyInfo.GetCustomAttribute<YamlConverterAttribute>();
			if (customAttribute != null)
			{
				ConverterType = customAttribute.ConverterType;
			}
		}

		public void Write(object target, object? value)
		{
			propertyInfo.SetValue(target, value, null);
		}

		public T? GetCustomAttribute<T>() where T : Attribute
		{
			Attribute[] allCustomAttributes = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.GetAllCustomAttributes<T>(propertyInfo);
			return (T)allCustomAttributes.FirstOrDefault();
		}

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor Read(object target)
		{
			object obj = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPropertyInfoExtensions.ReadValue(propertyInfo, target);
			Type type = TypeOverride ?? typeResolver.Resolve(Type, obj);
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(obj, type, Type, ScalarStyle);
		}
	}

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver;

	private readonly bool includeNonPublicProperties;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadablePropertiesTypeInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver)
		: this(typeResolver, includeNonPublicProperties: false)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadablePropertiesTypeInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver, bool includeNonPublicProperties)
	{
		this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
		this.includeNonPublicProperties = includeNonPublicProperties;
	}

	private static bool IsValidProperty(PropertyInfo property)
	{
		if (property.CanRead)
		{
			return property.GetGetMethod(nonPublic: true).GetParameters().Length == 0;
		}
		return false;
	}

	public override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.GetProperties(type, includeNonPublicProperties).Where(IsValidProperty).Select((Func<PropertyInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor>)((PropertyInfo p) => new ReflectionPropertyDescriptor(p, typeResolver)));
	}
}
