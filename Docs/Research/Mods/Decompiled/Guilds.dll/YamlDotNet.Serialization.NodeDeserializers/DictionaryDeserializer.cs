using System;
using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal abstract class DictionaryDeserializer
{
	private readonly bool duplicateKeyChecking;

	public DictionaryDeserializer(bool duplicateKeyChecking)
	{
		this.duplicateKeyChecking = duplicateKeyChecking;
	}

	private void TryAssign(IDictionary result, object key, object value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart propertyName)
	{
		if (duplicateKeyChecking && result.Contains(key))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = propertyName.Start;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = propertyName.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, $"Encountered duplicate key {key}");
		}
		result[key] = value;
	}

	protected virtual void Deserialize(Type tKey, Type tValue, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, IDictionary result, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		IDictionary result2 = result;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart property = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd>(out @event))
		{
			object key = nestedObjectDeserializer(parser, tKey);
			object value = nestedObjectDeserializer(parser, tValue);
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise = value as _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise;
			if (key is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise2)
			{
				if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise == null)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise2.ValueAvailable += delegate(object? v)
					{
						result2[v] = value;
					};
					continue;
				}
				bool hasFirstPart = false;
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise2.ValueAvailable += delegate(object? v)
				{
					if (hasFirstPart)
					{
						TryAssign(result2, v, value, property);
					}
					else
					{
						key = v;
						hasFirstPart = true;
					}
				};
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise.ValueAvailable += delegate(object? v)
				{
					if (hasFirstPart)
					{
						TryAssign(result2, key, v, property);
					}
					else
					{
						value = v;
						hasFirstPart = true;
					}
				};
				continue;
			}
			if (key == null)
			{
				throw new ArgumentException("Empty key names are not supported yet.", "tKey");
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise == null)
			{
				TryAssign(result2, key, value, property);
				continue;
			}
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise.ValueAvailable += delegate(object? v)
			{
				result2[key] = v;
			};
		}
	}
}
