using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace YamlDotNet.Serialization.TypeInspectors;

internal abstract class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeInspectorSkeleton : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector
{
	public abstract string GetEnumName(Type enumType, string name);

	public abstract string GetEnumValue(object enumValue);

	public abstract IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container);

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor GetProperty(Type type, object? container, string name, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNullWhen(true)] bool ignoreUnmatched, bool caseInsensitivePropertyMatching)
	{
		string name2 = name;
		IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> enumerable = ((!caseInsensitivePropertyMatching) ? (from p in GetProperties(type, container)
			where p.Name == name2
			select p) : (from p in GetProperties(type, container)
			where p.Name.Equals(name2, StringComparison.OrdinalIgnoreCase)
			select p));
		using IEnumerator<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> enumerator = enumerable.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			if (ignoreUnmatched)
			{
				return null;
			}
			throw new SerializationException("Property '" + name2 + "' not found on type '" + type.FullName + "'.");
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor current = enumerator.Current;
		if (enumerator.MoveNext())
		{
			throw new SerializationException("Multiple properties with the name/alias '" + name2 + "' already exists on type '" + type.FullName + "', maybe you're misusing YamlAlias or maybe you are using the wrong naming convention? The matching properties are: " + string.Join(", ", enumerable.Select((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor p) => p.Name).ToArray()));
		}
		return current;
	}
}
