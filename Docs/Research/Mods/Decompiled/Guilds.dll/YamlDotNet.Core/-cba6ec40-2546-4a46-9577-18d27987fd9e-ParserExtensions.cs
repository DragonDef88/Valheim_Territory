using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserExtensions
{
	public static T Consume<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		T result = parser.Require<T>();
		parser.MoveNext();
		return result;
	}

	public static bool TryConsume<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNullWhen(false)] out T @event) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		if (parser.Accept<T>(out @event))
		{
			parser.MoveNext();
			return true;
		}
		return false;
	}

	public static T Require<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		if (!parser.Accept<T>(out var @event))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent current = parser.Current;
			if (current == null)
			{
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("Expected '" + typeof(T).Name + "', got nothing.");
			}
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = current.Start;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = current.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, $"Expected '{typeof(T).Name}', got '{current.GetType().Name}' (at {current.Start}).");
		}
		return @event;
	}

	public static bool Accept<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNullWhen(false)] out T @event) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		if (parser.Current == null && !parser.MoveNext())
		{
			throw new EndOfStreamException();
		}
		if (parser.Current is T val)
		{
			@event = val;
			return true;
		}
		@event = null;
		return false;
	}

	public static void SkipThisAndNestedEvents(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser)
	{
		int num = 0;
		do
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>();
			num += _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent.NestingIncrease;
		}
		while (num > 0);
	}

	[Obsolete("Please use Consume<T>() instead")]
	public static T Expect<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		return parser.Consume<T>();
	}

	[Obsolete("Please use TryConsume<T>(out var evt) instead")]
	[return: _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNull]
	public static T? Allow<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		if (!parser.TryConsume<T>(out var @event))
		{
			return null;
		}
		return @event;
	}

	[Obsolete("Please use Accept<T>(out var evt) instead")]
	[return: _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNull]
	public static T? Peek<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		if (!parser.Accept<T>(out var @event))
		{
			return null;
		}
		return @event;
	}

	[Obsolete("Please use TryConsume<T>(out var evt) or Accept<T>(out var evt) instead")]
	public static bool Accept<T>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser) where T : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		T @event;
		return parser.Accept<T>(out @event);
	}

	public static bool TryFindMappingEntry(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar, bool> selector, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNullWhen(false)] out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar? key, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaybeNullWhen(false)] out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent? value)
	{
		if (parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>(out var _))
		{
			while (parser.Current != null)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent current = parser.Current;
				if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar))
				{
					if (current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart || current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart)
					{
						parser.SkipThisAndNestedEvents();
					}
					else
					{
						parser.MoveNext();
					}
					continue;
				}
				bool flag = selector(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar);
				parser.MoveNext();
				if (flag)
				{
					value = parser.Current;
					key = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar;
					return true;
				}
				parser.SkipThisAndNestedEvents();
			}
		}
		key = null;
		value = null;
		return false;
	}
}
