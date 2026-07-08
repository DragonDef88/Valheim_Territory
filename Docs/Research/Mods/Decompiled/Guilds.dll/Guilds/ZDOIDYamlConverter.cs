using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Guilds;

public class ZDOIDYamlConverter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter
{
	public bool Accepts(Type type)
	{
		return type == typeof(ZDOID);
	}

	public object ReadYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string[] array = ((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)parser.Current).Value.Split(new char[1] { ':' });
		ZDOID val = new ZDOID(long.Parse(array[0]), uint.Parse(array[1]));
		parser.MoveNext();
		return (object)val;
	}

	public void WriteYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? value, Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ZDOID val = (ZDOID)value;
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar($"{((ZDOID)(ref val)).UserID}:{((ZDOID)(ref val)).ID}"));
	}
}
