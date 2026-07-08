using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace Guilds;

public class CustomDataConverter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter
{
	[CompilerGenerated]
	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer _003CvalueDeserializer_003EP;

	[CompilerGenerated]
	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueSerializer _003CvalueSerializer_003EP;

	public static readonly Dictionary<string, Type> RegisteredCustomTypes = new Dictionary<string, Type>();

	public CustomDataConverter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer valueDeserializer, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueSerializer valueSerializer)
	{
		_003CvalueDeserializer_003EP = valueDeserializer;
		_003CvalueSerializer_003EP = valueSerializer;
		base._002Ector();
	}

	public bool Accepts(Type type)
	{
		return type == typeof(CustomData);
	}

	public object ReadYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type _, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>();
		CustomData customData = new CustomData();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd>(out @event))
		{
			string key = (string)_003CvalueDeserializer_003EP.DeserializeValue(parser, typeof(string), new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState(), _003CvalueDeserializer_003EP);
			if (RegisteredCustomTypes.TryGetValue(key, out Type value))
			{
				object obj = _003CvalueDeserializer_003EP.DeserializeValue(parser, value, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState(), _003CvalueDeserializer_003EP);
				if (obj != null)
				{
					customData.data.Add(value, obj);
				}
			}
			else
			{
				object obj2 = _003CvalueDeserializer_003EP.DeserializeValue(parser, typeof(object), new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState(), _003CvalueDeserializer_003EP);
				if (obj2 != null)
				{
					customData.unknown.Add(key, obj2);
				}
			}
		}
		return customData;
	}

	public void WriteYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? value, Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		if (value == null)
		{
			return;
		}
		CustomData customData = (CustomData)value;
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart());
		foreach (KeyValuePair<Type, object> datum in customData.data)
		{
			_003CvalueSerializer_003EP.SerializeValue(emitter, datum.Key.FullName, typeof(string));
			_003CvalueSerializer_003EP.SerializeValue(emitter, datum.Value, datum.Key);
		}
		foreach (KeyValuePair<string, object> item in customData.unknown)
		{
			_003CvalueSerializer_003EP.SerializeValue(emitter, item.Key, typeof(string));
			_003CvalueSerializer_003EP.SerializeValue(emitter, item.Value, typeof(object));
		}
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd());
	}
}
