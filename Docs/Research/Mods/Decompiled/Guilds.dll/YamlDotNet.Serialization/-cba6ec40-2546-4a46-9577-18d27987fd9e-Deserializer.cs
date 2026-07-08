using System;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIDeserializer
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer valueDeserializer;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializer()
		: this(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder().BuildValueDeserializer())
	{
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer valueDeserializer)
	{
		this.valueDeserializer = valueDeserializer ?? throw new ArgumentNullException("valueDeserializer");
	}

	public static _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializer FromValueDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer valueDeserializer)
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializer(valueDeserializer);
	}

	public T Deserialize<T>(string input)
	{
		using StringReader input2 = new StringReader(input);
		return Deserialize<T>(input2);
	}

	public T Deserialize<T>(TextReader input)
	{
		return Deserialize<T>(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParser(input));
	}

	public T Deserialize<T>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser)
	{
		return (T)Deserialize(parser, typeof(T));
	}

	public object? Deserialize(string input)
	{
		return Deserialize<object>(input);
	}

	public object? Deserialize(TextReader input)
	{
		return Deserialize<object>(input);
	}

	public object? Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser)
	{
		return Deserialize<object>(parser);
	}

	public object? Deserialize(string input, Type type)
	{
		using StringReader input2 = new StringReader(input);
		return Deserialize(input2, type);
	}

	public object? Deserialize(TextReader input, Type type)
	{
		return Deserialize(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParser(input), type);
	}

	public object? Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type type)
	{
		if (parser == null)
		{
			throw new ArgumentNullException("parser");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart @event;
		bool flag = parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart>(out @event);
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart event2;
		bool flag2 = parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart>(out event2);
		object result = null;
		if (!parser.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd>(out var _) && !parser.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd>(out var _))
		{
			using _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState();
			result = valueDeserializer.DeserializeValue(parser, type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState, valueDeserializer);
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState.OnDeserialization();
		}
		if (flag2)
		{
			parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd>();
		}
		if (flag)
		{
			parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd>();
		}
		return result;
	}
}
