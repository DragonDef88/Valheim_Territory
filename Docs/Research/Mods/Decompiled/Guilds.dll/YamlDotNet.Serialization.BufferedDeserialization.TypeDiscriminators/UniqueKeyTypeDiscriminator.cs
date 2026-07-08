using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

internal class UniqueKeyTypeDiscriminator : ITypeDiscriminator
{
	private readonly IDictionary<string, Type> typeMapping;

	public Type BaseType { get; private set; }

	public UniqueKeyTypeDiscriminator(Type baseType, IDictionary<string, Type> typeMapping)
	{
		foreach (KeyValuePair<string, Type> item in typeMapping)
		{
			if (!baseType.IsAssignableFrom(item.Value))
			{
				throw new ArgumentOutOfRangeException("typeMapping", $"{item.Value} is not a assignable to {baseType}");
			}
		}
		BaseType = baseType;
		this.typeMapping = typeMapping;
	}

	public bool TryDiscriminate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, out Type? suggestedType)
	{
		if (parser.TryFindMappingEntry((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar scalar) => typeMapping.ContainsKey(scalar.Value), out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar key, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent _))
		{
			suggestedType = typeMapping[key.Value];
			return true;
		}
		suggestedType = null;
		return false;
	}
}
