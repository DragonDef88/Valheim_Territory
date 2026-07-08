using System;
using System.IO;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIDeserializer
{
	T Deserialize<T>(string input);

	T Deserialize<T>(TextReader input);

	T Deserialize<T>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser);

	object? Deserialize(string input);

	object? Deserialize(TextReader input);

	object? Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser);

	object? Deserialize(string input, Type type);

	object? Deserialize(TextReader input, Type type);

	object? Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type type);
}
