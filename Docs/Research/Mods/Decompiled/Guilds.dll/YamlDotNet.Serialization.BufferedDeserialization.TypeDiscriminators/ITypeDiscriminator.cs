using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

internal interface ITypeDiscriminator
{
	Type BaseType { get; }

	bool TryDiscriminate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser buffer, out Type? suggestedType);
}
