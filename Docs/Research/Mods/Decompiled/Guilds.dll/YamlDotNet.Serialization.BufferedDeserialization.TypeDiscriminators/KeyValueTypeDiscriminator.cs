using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

internal class KeyValueTypeDiscriminator : ITypeDiscriminator
{
	private readonly string targetKey;

	private readonly IDictionary<string, Type> typeMapping;

	public Type BaseType { get; private set; }

	public KeyValueTypeDiscriminator(Type baseType, string targetKey, IDictionary<string, Type> typeMapping)
	{
		foreach (KeyValuePair<string, Type> item in typeMapping)
		{
			if (!baseType.IsAssignableFrom(item.Value))
			{
				throw new ArgumentOutOfRangeException("typeMapping", $"{item.Value} is not a assignable to {baseType}");
			}
		}
		BaseType = baseType;
		this.targetKey = targetKey;
		this.typeMapping = typeMapping;
	}

	public bool TryDiscriminate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, out Type? suggestedType)
	{
		if (parser.TryFindMappingEntry((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar scalar) => targetKey == scalar.Value, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent value) && value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar && typeMapping.TryGetValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value, out Type value2))
		{
			suggestedType = value2;
			return true;
		}
		suggestedType = null;
		return false;
	}
}
