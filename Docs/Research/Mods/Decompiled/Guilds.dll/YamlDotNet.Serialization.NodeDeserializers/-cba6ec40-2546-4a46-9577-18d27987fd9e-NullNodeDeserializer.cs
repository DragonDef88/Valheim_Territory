using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENullNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		value = null;
		if (parser.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent>(out var @event) && NodeIsNull(@event))
		{
			parser.SkipThisAndNestedEvents();
			return true;
		}
		return false;
	}

	private static bool NodeIsNull(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent nodeEvent)
	{
		if (nodeEvent.Tag == "tag:yaml.org,2002:null")
		{
			return true;
		}
		if (nodeEvent is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar { Style: _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain, IsKey: false } _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)
		{
			string value = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value;
			switch (value)
			{
			default:
				return value == "NULL";
			case "":
			case "~":
			case "null":
			case "Null":
				return true;
			}
		}
		return false;
	}
}
