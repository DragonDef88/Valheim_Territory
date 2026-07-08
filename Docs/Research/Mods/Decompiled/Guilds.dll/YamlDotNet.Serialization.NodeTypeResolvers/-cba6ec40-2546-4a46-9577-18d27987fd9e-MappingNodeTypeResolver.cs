using System;
using System.Collections.Generic;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeTypeResolvers;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
{
	private readonly IDictionary<Type, Type> mappings;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingNodeTypeResolver(IDictionary<Type, Type> mappings)
	{
		if (mappings == null)
		{
			throw new ArgumentNullException("mappings");
		}
		foreach (KeyValuePair<Type, Type> mapping in mappings)
		{
			if (!mapping.Key.IsAssignableFrom(mapping.Value))
			{
				throw new InvalidOperationException($"Type '{mapping.Value}' does not implement type '{mapping.Key}'.");
			}
		}
		this.mappings = mappings;
	}

	public bool Resolve(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent? nodeEvent, ref Type currentType)
	{
		if (mappings.TryGetValue(currentType, out Type value))
		{
			currentType = value;
			return true;
		}
		return false;
	}
}
