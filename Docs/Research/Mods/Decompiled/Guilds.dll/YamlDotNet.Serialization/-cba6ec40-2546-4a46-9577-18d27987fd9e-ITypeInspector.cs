using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YamlDotNet.Serialization;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector
{
	IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor> GetProperties(Type type, object? container);

	_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor GetProperty(Type type, object? container, string name, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNullWhen(true)] bool ignoreUnmatched, bool caseInsensitivePropertyMatching);

	string GetEnumName(Type enumType, string name);

	string GetEnumValue(object enumValue);
}
