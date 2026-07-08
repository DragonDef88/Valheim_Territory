using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.Utilities;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectAnchorCollection
{
	private readonly Dictionary<string, object> objectsByAnchor = new Dictionary<string, object>();

	private readonly Dictionary<object, string> anchorsByObject = new Dictionary<object, string>();

	public object this[string anchor]
	{
		get
		{
			if (objectsByAnchor.TryGetValue(anchor, out object value))
			{
				return value;
			}
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorNotFoundException("The anchor '" + anchor + "' does not exists");
		}
	}

	public void Add(string anchor, object @object)
	{
		objectsByAnchor.Add(anchor, @object);
		if (@object != null)
		{
			anchorsByObject.Add(@object, anchor);
		}
	}

	public bool TryGetAnchor(object @object, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNullWhen(false)] out string? anchor)
	{
		return anchorsByObject.TryGetValue(@object, out anchor);
	}
}
