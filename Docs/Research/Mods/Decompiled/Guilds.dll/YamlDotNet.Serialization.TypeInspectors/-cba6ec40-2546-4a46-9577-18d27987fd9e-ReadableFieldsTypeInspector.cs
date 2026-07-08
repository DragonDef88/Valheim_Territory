using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.TypeInspectors;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadableFieldsTypeInspector : ReflectionTypeInspector
{
	protected class ReflectionFieldDescriptor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor
	{
		private readonly FieldInfo fieldInfo;

		private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver;

		public string Name => fieldInfo.Name;

		public bool Required => fieldInfo.IsRequired();

		public Type Type => fieldInfo.FieldType;

		public Type? ConverterType { get; }

		public Type? TypeOverride { get; set; }

		public bool AllowNulls => fieldInfo.AcceptsNull();

		public int Order { get; set; }

		public bool CanWrite => !fieldInfo.IsInitOnly;

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle ScalarStyle { get; set; }

		public ReflectionFieldDescriptor(FieldInfo fieldInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver)
		{
			this.fieldInfo = fieldInfo;
			this.typeResolver = typeResolver;
			YamlConverterAttribute customAttribute = fieldInfo.GetCustomAttribute<YamlConverterAttribute>();
			if (customAttribute != null)
			{
				ConverterType = customAttribute.ConverterType;
			}
			ScalarStyle = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any;
		}

		public void Write(object target, object? value)
		{
			fieldInfo.SetValue(target, value);
		}

		public T? GetCustomAttribute<T>() where T : Attribute
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(T), inherit: true);
			return (T)customAttributes.FirstOrDefault();
		}

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor Read(object target)
		{
			object value = fieldInfo.GetValue(target);
			Type type = TypeOverride ?? typeResolver.Resolve(Type, value);
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(value, type, Type, ScalarStyle);
		}
	}

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadableFieldsTypeInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver)
	{
		this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
	}

	public override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.GetPublicFields(type).Select((Func<FieldInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor>)((FieldInfo p) => new ReflectionFieldDescriptor(p, typeResolver)));
	}
}
