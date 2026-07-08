using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.TypeInspectors;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECachedTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeInspectorSkeleton
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor;

	private readonly ConcurrentDictionary<Type, List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor>> cache = new ConcurrentDictionary<Type, List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor>>();

	private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> enumNameCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>();

	private readonly ConcurrentDictionary<object, string> enumValueCache = new ConcurrentDictionary<object, string>();

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECachedTypeInspector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor)
	{
		this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
	}

	public override string GetEnumName(Type enumType, string name)
	{
		ConcurrentDictionary<string, string> orAdd = enumNameCache.GetOrAdd(enumType, (Type _) => new ConcurrentDictionary<string, string>());
		return DictionaryExtensions.GetOrAdd(orAdd, name, delegate(string n, (Type enumType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor) context)
		{
			var (enumType2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector) = context;
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector.GetEnumName(enumType2, n);
		}, (enumType, innerTypeDescriptor));
	}

	public override string GetEnumValue(object enumValue)
	{
		return DictionaryExtensions.GetOrAdd(enumValueCache, enumValue, delegate(object _, (object enumValue, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor) context)
		{
			var (enumValue2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector) = context;
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector.GetEnumValue(enumValue2);
		}, (enumValue, innerTypeDescriptor));
	}

	public override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return DictionaryExtensions.GetOrAdd(cache, type, delegate(Type t, (object container, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector innerTypeDescriptor) context)
		{
			var (container2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector) = context;
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector.GetProperties(t, container2).ToList();
		}, (container, innerTypeDescriptor));
	}
}
