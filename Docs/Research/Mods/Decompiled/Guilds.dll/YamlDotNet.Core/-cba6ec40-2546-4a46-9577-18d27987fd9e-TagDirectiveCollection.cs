using System.Collections.Generic;
using System.Collections.ObjectModel;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection : KeyedCollection<string, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective>
{
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection()
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective> tagDirectives)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective tagDirective in tagDirectives)
		{
			Add(tagDirective);
		}
	}

	protected override string GetKeyForItem(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective item)
	{
		return item.Handle;
	}

	public new bool Contains(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective directive)
	{
		return Contains(GetKeyForItem(directive));
	}
}
