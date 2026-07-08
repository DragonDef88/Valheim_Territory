using System;

namespace YamlDotNet.Core;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel
{
	private int current;

	public int Maximum { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel(int maximum)
	{
		Maximum = maximum;
	}

	public void Increment()
	{
		if (!TryIncrement())
		{
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaximumRecursionLevelReachedException("Maximum level of recursion reached");
		}
	}

	public bool TryIncrement()
	{
		if (current < Maximum)
		{
			current++;
			return true;
		}
		return false;
	}

	public void Decrement()
	{
		if (current == 0)
		{
			throw new InvalidOperationException("Attempted to decrement RecursionLevel to a negative value");
		}
		current--;
	}
}
