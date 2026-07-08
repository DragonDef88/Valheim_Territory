using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides
{
	private readonly struct AttributeKey
	{
		public readonly Type AttributeType;

		public readonly string PropertyName;

		public AttributeKey(Type attributeType, string propertyName)
		{
			AttributeType = attributeType;
			PropertyName = propertyName;
		}

		public override bool Equals(object? obj)
		{
			if (obj is AttributeKey attributeKey && AttributeType.Equals(attributeKey.AttributeType))
			{
				return PropertyName.Equals(attributeKey.PropertyName);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(AttributeType.GetHashCode(), PropertyName.GetHashCode());
		}
	}

	private sealed class AttributeMapping
	{
		public readonly Type RegisteredType;

		public readonly Attribute Attribute;

		public AttributeMapping(Type registeredType, Attribute attribute)
		{
			RegisteredType = registeredType;
			Attribute = attribute;
		}

		public override bool Equals(object? obj)
		{
			if (obj is AttributeMapping attributeMapping && RegisteredType.Equals(attributeMapping.RegisteredType))
			{
				return Attribute.Equals(attributeMapping.Attribute);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(RegisteredType.GetHashCode(), Attribute.GetHashCode());
		}

		public int Matches(Type matchType)
		{
			int num = 0;
			Type type = matchType;
			while (type != null)
			{
				num++;
				if (type == RegisteredType)
				{
					return num;
				}
				type = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.BaseType(type);
			}
			if (matchType.GetInterfaces().Contains(RegisteredType))
			{
				return num;
			}
			return 0;
		}
	}

	private readonly Dictionary<AttributeKey, List<AttributeMapping>> overrides = new Dictionary<AttributeKey, List<AttributeMapping>>();

	[return: _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNull]
	public T GetAttribute<T>(Type type, string member) where T : Attribute
	{
		if (overrides.TryGetValue(new AttributeKey(typeof(T), member), out List<AttributeMapping> value))
		{
			int num = 0;
			AttributeMapping attributeMapping = null;
			foreach (AttributeMapping item in value)
			{
				int num2 = item.Matches(type);
				if (num2 > num)
				{
					num = num2;
					attributeMapping = item;
				}
			}
			if (num > 0)
			{
				return (T)attributeMapping.Attribute;
			}
		}
		return null;
	}

	public void Add(Type type, string member, Attribute attribute)
	{
		AttributeMapping item = new AttributeMapping(type, attribute);
		AttributeKey key = new AttributeKey(attribute.GetType(), member);
		if (!overrides.TryGetValue(key, out List<AttributeMapping> value))
		{
			value = new List<AttributeMapping>();
			overrides.Add(key, value);
		}
		else if (value.Contains(item))
		{
			throw new InvalidOperationException($"Attribute ({attribute}) already set for Type {type.FullName}, Member {member}");
		}
		value.Add(item);
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides Clone()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides();
		foreach (KeyValuePair<AttributeKey, List<AttributeMapping>> @override in overrides)
		{
			foreach (AttributeMapping item in @override.Value)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides2.Add(item.RegisteredType, @override.Key.PropertyName, item.Attribute);
			}
		}
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides2;
	}

	public void Add<TClass>(Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
	{
		PropertyInfo propertyInfo = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EExpressionExtensions.AsProperty(propertyAccessor);
		Add(typeof(TClass), propertyInfo.Name, attribute);
	}
}
