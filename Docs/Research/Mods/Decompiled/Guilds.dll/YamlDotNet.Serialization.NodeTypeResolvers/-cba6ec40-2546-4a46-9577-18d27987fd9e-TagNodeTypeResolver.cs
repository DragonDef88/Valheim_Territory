using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeTypeResolvers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
{
	private readonly IDictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName, Type> tagMappings;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagNodeTypeResolver(IDictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName, Type> tagMappings)
	{
		this.tagMappings = tagMappings ?? throw new ArgumentNullException("tagMappings");
	}

	bool _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver.Resolve(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent? nodeEvent, ref Type currentType)
	{
		if (nodeEvent != null && !nodeEvent.Tag.IsEmpty && tagMappings.TryGetValue(nodeEvent.Tag, out Type value))
		{
			currentType = value;
			return true;
		}
		return false;
	}
}
