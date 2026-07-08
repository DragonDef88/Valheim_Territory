using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.Utilities;

internal sealed class TypeConverterCache
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter[] typeConverters;

	private readonly ConcurrentDictionary<Type, (bool HasMatch, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter? TypeConverter)> cache = new ConcurrentDictionary<Type, (bool, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter)>();

	public TypeConverterCache(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter>? typeConverters)
		: this(typeConverters?.ToArray() ?? Array.Empty<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter>())
	{
	}

	public TypeConverterCache(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter[] typeConverters)
	{
		this.typeConverters = typeConverters;
	}

	public bool TryGetConverterForType(Type type, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENotNullWhen(true)] out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter? typeConverter)
	{
		(bool, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter) orAdd = DictionaryExtensions.GetOrAdd(cache, type, (Type t, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter[] tc) => LookupTypeConverter(t, tc), typeConverters);
		typeConverter = orAdd.Item2;
		return orAdd.Item1;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter GetConverterByType(Type converter)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter[] array = typeConverters;
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter in array)
		{
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter.GetType() == converter)
			{
				return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter;
			}
		}
		throw new ArgumentException("IYamlTypeConverter of type " + converter.FullName + " not found", "converter");
	}

	private static (bool HasMatch, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter? TypeConverter) LookupTypeConverter(Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter[] typeConverters)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter in typeConverters)
		{
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter.Accepts(type))
			{
				return (HasMatch: true, TypeConverter: _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter);
			}
		}
		return (HasMatch: false, TypeConverter: null);
	}
}
