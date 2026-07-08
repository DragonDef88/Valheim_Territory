using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector;

	private readonly bool ignoreUnmatched;

	private readonly bool duplicateKeyChecking;

	private readonly ITypeConverter typeConverter;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention;

	private readonly bool enforceNullability;

	private readonly bool caseInsensitivePropertyMatching;

	private readonly bool enforceRequiredProperties;

	private readonly TypeConverterCache typeConverters;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector, bool ignoreUnmatched, bool duplicateKeyChecking, ITypeConverter typeConverter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention, bool enforceNullability, bool caseInsensitivePropertyMatching, bool enforceRequiredProperties, IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> typeConverters)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
		this.typeInspector = typeInspector ?? throw new ArgumentNullException("typeInspector");
		this.ignoreUnmatched = ignoreUnmatched;
		this.duplicateKeyChecking = duplicateKeyChecking;
		this.typeConverter = typeConverter ?? throw new ArgumentNullException("typeConverter");
		this.enumNamingConvention = enumNamingConvention ?? throw new ArgumentNullException("enumNamingConvention");
		this.enforceNullability = enforceNullability;
		this.caseInsensitivePropertyMatching = caseInsensitivePropertyMatching;
		this.enforceRequiredProperties = enforceRequiredProperties;
		this.typeConverters = new TypeConverterCache(typeConverters);
	}

	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		if (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>(out var _))
		{
			value = null;
			return false;
		}
		Type type = Nullable.GetUnderlyingType(expectedType) ?? FsharpHelper.GetOptionUnderlyingType(expectedType) ?? expectedType;
		value = objectFactory.Create(type);
		objectFactory.ExecuteOnDeserializing(value);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.Ordinal);
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd event2;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd>(out event2))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar propertyName = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar>();
			if (duplicateKeyChecking && !hashSet.Add(propertyName.Value))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = propertyName.Start;
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = propertyName.End;
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start2, in end, "Encountered duplicate key " + propertyName.Value);
			}
			try
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor property = typeInspector.GetProperty(type, null, propertyName.Value, ignoreUnmatched, caseInsensitivePropertyMatching);
				if (property == null)
				{
					parser.SkipThisAndNestedEvents();
					continue;
				}
				hashSet2.Add(property.Name);
				object obj;
				if (property.ConverterType != null)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter converterByType = typeConverters.GetConverterByType(property.ConverterType);
					obj = converterByType.ReadYaml(parser, property.Type, rootDeserializer);
				}
				else
				{
					obj = nestedObjectDeserializer(parser, property.Type);
				}
				if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise)
				{
					object valueRef = value;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise.ValueAvailable += delegate(object? v)
					{
						object value3 = typeConverter.ChangeType(v, property.Type, enumNamingConvention, typeInspector);
						NullCheck(value3, property, propertyName);
						property.Write(valueRef, value3);
					};
				}
				else
				{
					object value2 = typeConverter.ChangeType(obj, property.Type, enumNamingConvention, typeInspector);
					NullCheck(value2, property, propertyName);
					property.Write(value, value2);
				}
			}
			catch (SerializationException ex)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = propertyName.Start;
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = propertyName.End;
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start2, in end, ex.Message);
			}
			catch (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = propertyName.Start;
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = propertyName.End;
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start2, in end, "Exception during deserialization", innerException);
			}
			start = propertyName.End;
		}
		if (enforceRequiredProperties)
		{
			IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> properties = typeInspector.GetProperties(type, value);
			List<string> list = new List<string>();
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor item in properties)
			{
				if (item.Required && !hashSet2.Contains(item.Name))
				{
					list.Add(item.Name);
				}
			}
			if (list.Count > 0)
			{
				string text = string.Join(",", list);
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in start, "Missing properties, '" + text + "' in source yaml.");
			}
		}
		objectFactory.ExecuteOnDeserialized(value);
		return true;
	}

	public void NullCheck(object value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor property, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar propertyName)
	{
		if (enforceNullability && value == null && !property.AllowNulls)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = propertyName.Start;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = propertyName.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, "Strict nullability enforcement error.", new NullReferenceException("Yaml value is null when target property requires non null values."));
		}
	}
}
