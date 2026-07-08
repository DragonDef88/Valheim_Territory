using System;
using System.Collections.Generic;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeTypeResolvers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultContainersNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
{
	bool _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver.Resolve(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent? nodeEvent, ref Type currentType)
	{
		if (currentType == typeof(object))
		{
			if (nodeEvent is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart)
			{
				currentType = typeof(List<object>);
				return true;
			}
			if (nodeEvent is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart)
			{
				currentType = typeof(Dictionary<object, object>);
				return true;
			}
		}
		return false;
	}
}
