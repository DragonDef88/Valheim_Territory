using System;

namespace YamlDotNet.Serialization;

internal abstract class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventInfo
{
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor Source { get; }

	protected _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventInfo(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor source)
	{
		Source = source ?? throw new ArgumentNullException("source");
	}
}
