using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasEventInfo : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventInfo
{
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName Alias { get; }

	public bool NeedsExpansion { get; set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasEventInfo(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor source, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName alias)
		: base(source)
	{
		if (alias.IsEmpty)
		{
			throw new ArgumentNullException("alias");
		}
		Alias = alias;
	}
}
