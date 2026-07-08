using System;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeTypeResolvers;

[Obsolete("The mechanism that this class uses to specify type names is non-standard. Register the tags explicitly instead of using this convention.")]
internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeNameInTagNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
{
	bool _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver.Resolve(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent? nodeEvent, ref Type currentType)
	{
		if (nodeEvent != null && !nodeEvent.Tag.IsEmpty)
		{
			Type type = Type.GetType(nodeEvent.Tag.Value.Substring(1), throwOnError: false);
			if (type != null)
			{
				currentType = type;
				return true;
			}
		}
		return false;
	}
}
