using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YamlDotNet.RepresentationModel;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeIdentityEqualityComparer : IEqualityComparer<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>
{
	public bool Equals([_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAllowNull] _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode x, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAllowNull] _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode y)
	{
		return x == y;
	}

	public int GetHashCode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode obj)
	{
		return obj.GetHashCode();
	}
}
