using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Guilds;

public static class GuildSerialization
{
	private static readonly CustomDataConverter customDataConverter = new CustomDataConverter(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder().WithTypeConverter(new ZDOIDYamlConverter()).BuildValueDeserializer(), new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerBuilder().WithTypeConverter(new ZDOIDYamlConverter()).BuildValueSerializer());

	private static readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIDeserializer Deserializer = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder().WithNamingConvention(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECamelCaseNamingConvention.Instance).WithTypeConverter(new ZDOIDYamlConverter()).WithTypeConverter(customDataConverter)
		.Build();

	private static readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EISerializer Serializer = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerBuilder().WithNamingConvention(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECamelCaseNamingConvention.Instance).WithTypeConverter(new ZDOIDYamlConverter()).WithTypeConverter(customDataConverter)
		.Build();

	public static T Deserialize<T>(string yaml)
	{
		return Deserializer.Deserialize<T>(yaml);
	}

	public static object? Deserialize(string yaml, Type type)
	{
		return Deserializer.Deserialize(yaml, type);
	}

	public static string Serialize<T>(T guild) where T : notnull
	{
		return Serializer.Serialize(guild);
	}
}
