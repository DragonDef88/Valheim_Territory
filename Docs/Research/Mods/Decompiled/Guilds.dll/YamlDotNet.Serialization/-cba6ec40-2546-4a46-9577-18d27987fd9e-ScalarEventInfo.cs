using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarEventInfo : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectEventInfo
{
	public string RenderedValue { get; set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle Style { get; set; }

	public bool IsPlainImplicit { get; set; }

	public bool IsQuotedImplicit { get; set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarEventInfo(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor source)
		: base(source)
	{
		Style = source.ScalarStyle;
		RenderedValue = string.Empty;
	}
}
