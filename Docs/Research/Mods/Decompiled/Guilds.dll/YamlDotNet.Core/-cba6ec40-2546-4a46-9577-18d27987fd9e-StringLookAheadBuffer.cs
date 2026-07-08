using System;
using YamlDotNet.Core.ObjectPool;

namespace YamlDotNet.Core;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EILookAheadBuffer, IResettable
{
	public string Value { get; set; } = string.Empty;


	public int Position { get; private set; }

	public int Length => Value.Length;

	public bool EndOfInput => IsOutside(Position);

	public char Peek(int offset)
	{
		int index = Position + offset;
		if (!IsOutside(index))
		{
			return Value[index];
		}
		return '\0';
	}

	private bool IsOutside(int index)
	{
		return index >= Value.Length;
	}

	public void Skip(int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "The length must be positive.");
		}
		Position += length;
	}

	public bool TryReset()
	{
		Position = 0;
		Value = string.Empty;
		return true;
	}
}
