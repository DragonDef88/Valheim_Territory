using System;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EISerializer
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueSerializer valueSerializer;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterSettings emitterSettings;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializer()
		: this(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerBuilder().BuildValueSerializer(), _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterSettings.Default)
	{
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueSerializer valueSerializer, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterSettings emitterSettings)
	{
		this.valueSerializer = valueSerializer ?? throw new ArgumentNullException("valueSerializer");
		this.emitterSettings = emitterSettings ?? throw new ArgumentNullException("emitterSettings");
	}

	public static _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializer FromValueSerializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueSerializer valueSerializer, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterSettings emitterSettings)
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializer(valueSerializer, emitterSettings);
	}

	public string Serialize(object? graph)
	{
		using StringWriter stringWriter = new StringWriter();
		Serialize(stringWriter, graph);
		return stringWriter.ToString();
	}

	public string Serialize(object? graph, Type type)
	{
		using StringWriter stringWriter = new StringWriter();
		Serialize(stringWriter, graph, type);
		return stringWriter.ToString();
	}

	public void Serialize(TextWriter writer, object? graph)
	{
		Serialize(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(writer, emitterSettings), graph);
	}

	public void Serialize(TextWriter writer, object? graph, Type type)
	{
		Serialize(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(writer, emitterSettings), graph, type);
	}

	public void Serialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? graph)
	{
		if (emitter == null)
		{
			throw new ArgumentNullException("emitter");
		}
		EmitDocument(emitter, graph, null);
	}

	public void Serialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? graph, Type type)
	{
		if (emitter == null)
		{
			throw new ArgumentNullException("emitter");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		EmitDocument(emitter, graph, type);
	}

	private void EmitDocument(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? graph, Type? type)
	{
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart());
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart());
		valueSerializer.SerializeValue(emitter, graph, type);
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd(isImplicit: true));
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd());
	}
}
