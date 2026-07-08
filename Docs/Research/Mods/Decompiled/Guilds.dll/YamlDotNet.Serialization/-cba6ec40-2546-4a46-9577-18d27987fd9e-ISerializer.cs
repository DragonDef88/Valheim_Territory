using System;
using System.IO;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EISerializer
{
	string Serialize(object? graph);

	string Serialize(object? graph, Type type);

	void Serialize(TextWriter writer, object? graph);

	void Serialize(TextWriter writer, object? graph, Type type);

	void Serialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? graph);

	void Serialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? graph, Type type);
}
