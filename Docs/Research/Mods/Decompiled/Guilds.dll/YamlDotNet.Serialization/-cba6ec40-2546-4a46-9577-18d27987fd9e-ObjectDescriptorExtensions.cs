using System;

namespace YamlDotNet.Serialization;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptorExtensions
{
	public static object NonNullValue(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor objectDescriptor)
	{
		return objectDescriptor.Value ?? throw new InvalidOperationException("Attempted to use a IObjectDescriptor of type '" + objectDescriptor.Type.FullName + "' whose Value is null at a point whete it is invalid to do so. This may indicate a bug in YamlDotNet.");
	}
}
