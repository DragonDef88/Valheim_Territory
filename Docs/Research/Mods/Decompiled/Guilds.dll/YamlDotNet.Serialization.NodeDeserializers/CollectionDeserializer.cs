using System;
using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal abstract class CollectionDeserializer
{
	protected static void DeserializeHelper(Type tItem, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, IList result, bool canUpdate, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory)
	{
		IList result2 = result;
		parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart>();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd>(out @event))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent current = parser.Current;
			object obj = nestedObjectDeserializer(parser, tItem);
			if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise)
			{
				if (!canUpdate)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = current?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = current?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EForwardAnchorNotSupportedException(in start, in end, "Forward alias references are not allowed because this type does not implement IList<>");
				}
				int index = result2.Add(objectFactory.CreatePrimitive(tItem));
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise.ValueAvailable += delegate(object? v)
				{
					result2[index] = v;
				};
			}
			else
			{
				result2.Add(obj);
			}
		}
	}
}
