using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverridesInspector : ReflectionTypeInspector
{
	public sealed class OverridePropertyDescriptor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor
	{
		private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor baseDescriptor;

		private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides overrides;

		private readonly Type classType;

		public string Name => baseDescriptor.Name;

		public bool Required => baseDescriptor.Required;

		public bool AllowNulls => baseDescriptor.AllowNulls;

		public bool CanWrite => baseDescriptor.CanWrite;

		public Type Type => baseDescriptor.Type;

		public Type? TypeOverride
		{
			get
			{
				return baseDescriptor.TypeOverride;
			}
			set
			{
				baseDescriptor.TypeOverride = value;
			}
		}

		public Type? ConverterType => GetCustomAttribute<YamlConverterAttribute>()?.ConverterType ?? baseDescriptor.ConverterType;

		public int Order
		{
			get
			{
				return baseDescriptor.Order;
			}
			set
			{
				baseDescriptor.Order = value;
			}
		}

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle ScalarStyle
		{
			get
			{
				return baseDescriptor.ScalarStyle;
			}
			set
			{
				baseDescriptor.ScalarStyle = value;
			}
		}

		public OverridePropertyDescriptor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor baseDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides overrides, Type classType)
		{
			this.baseDescriptor = baseDescriptor;
			this.overrides = overrides;
			this.classType = classType;
		}

		public void Write(object target, object? value)
		{
			baseDescriptor.Write(target, value);
		}

		public T? GetCustomAttribute<T>() where T : Attribute
		{
			T attribute = overrides.GetAttribute<T>(classType, Name);
			return attribute ?? baseDescriptor.GetCustomAttribute<T>();
		}

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor Read(object target)
		{
			return baseDescriptor.Read(target);
		}
	}

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides overrides;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverridesInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides overrides)
	{
		this.innerTypeDescriptor = innerTypeDescriptor;
		this.overrides = overrides;
	}

	public override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container)
	{
		Type type2 = type;
		IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> enumerable = innerTypeDescriptor.GetProperties(type2, container);
		if (overrides != null)
		{
			enumerable = enumerable.Select((Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor>)((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor p) => new OverridePropertyDescriptor(p, overrides, type2)));
		}
		return enumerable;
	}
}
