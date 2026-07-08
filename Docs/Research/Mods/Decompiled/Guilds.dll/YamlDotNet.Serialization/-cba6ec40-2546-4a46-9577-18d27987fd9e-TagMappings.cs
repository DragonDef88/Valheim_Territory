using System;
using System.Collections.Generic;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagMappings
{
	private readonly Dictionary<string, Type> mappings;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagMappings()
	{
		mappings = new Dictionary<string, Type>();
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagMappings(IDictionary<string, Type> mappings)
	{
		this.mappings = new Dictionary<string, Type>(mappings);
	}

	public void Add(string tag, Type mapping)
	{
		mappings.Add(tag, mapping);
	}

	internal Type? GetMapping(string tag)
	{
		if (!mappings.TryGetValue(tag, out Type value))
		{
			return null;
		}
		return value;
	}
}
