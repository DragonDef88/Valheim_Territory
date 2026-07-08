using System;
using YamlDotNet.Helpers;

namespace YamlDotNet.Core;

public readonly struct _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark : IEquatable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark>, IComparable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark>, IComparable
{
	public static readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark Empty = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark(0L, 1L, 1L);

	public long Index { get; }

	public long Line { get; }

	public long Column { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark(long index, long line, long column)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException("index", "Index must be greater than or equal to zero.");
		}
		if (line < 1)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException("line", "Line must be greater than or equal to 1.");
		}
		if (column < 1)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException("column", "Column must be greater than or equal to 1.");
		}
		Index = index;
		Line = line;
		Column = column;
	}

	public override string ToString()
	{
		return $"Line: {Line}, Col: {Column}, Idx: {Index}";
	}

	public override bool Equals(object? obj)
	{
		return Equals((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark)(obj ?? ((object)Empty)));
	}

	public bool Equals(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark other)
	{
		if (Index == other.Index && Line == other.Line)
		{
			return Column == other.Column;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(Index.GetHashCode(), _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(Line.GetHashCode(), Column.GetHashCode()));
	}

	public int CompareTo(object? obj)
	{
		return CompareTo((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark)(obj ?? ((object)Empty)));
	}

	public int CompareTo(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark other)
	{
		int num = Line.CompareTo(other.Line);
		if (num == 0)
		{
			num = Column.CompareTo(other.Column);
		}
		return num;
	}

	public static bool operator ==(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark right)
	{
		return !(left == right);
	}

	public static bool operator <(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator <=(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator >=(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark left, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark right)
	{
		return left.CompareTo(right) >= 0;
	}
}
