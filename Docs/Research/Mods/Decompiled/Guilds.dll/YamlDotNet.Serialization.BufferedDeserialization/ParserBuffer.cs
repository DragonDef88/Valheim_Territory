using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.BufferedDeserialization;

internal class ParserBuffer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser
{
	private readonly LinkedList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> buffer;

	private LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>? current;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent? Current => current?.Value;

	public ParserBuffer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parserToBuffer, int maxDepth, int maxLength)
	{
		buffer = new LinkedList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>();
		buffer.AddLast(parserToBuffer.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>());
		int num = 0;
		do
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent = parserToBuffer.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>();
			num += _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent.NestingIncrease;
			buffer.AddLast(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent);
			if (maxDepth > -1 && num > maxDepth)
			{
				throw new ArgumentOutOfRangeException("parserToBuffer", "Parser buffer exceeded max depth");
			}
			if (maxLength > -1 && buffer.Count > maxLength)
			{
				throw new ArgumentOutOfRangeException("parserToBuffer", "Parser buffer exceeded max length");
			}
		}
		while (num >= 0);
		current = buffer.First;
	}

	public bool MoveNext()
	{
		current = current?.Next;
		return current != null;
	}

	public void Reset()
	{
		current = buffer.First;
	}
}
