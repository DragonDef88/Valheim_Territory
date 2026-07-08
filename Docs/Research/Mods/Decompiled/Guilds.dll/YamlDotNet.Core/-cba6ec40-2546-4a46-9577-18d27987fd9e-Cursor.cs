using System.Diagnostics;

namespace YamlDotNet.Core;

[DebuggerStepThrough]
internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor
{
	public long Index { get; private set; }

	public long Line { get; private set; }

	public long LineOffset { get; private set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor()
	{
		Line = 1L;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor cursor)
	{
		Index = cursor.Index;
		Line = cursor.Line;
		LineOffset = cursor.LineOffset;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark Mark()
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark(Index, Line, LineOffset + 1);
	}

	public void Skip()
	{
		Index++;
		LineOffset++;
	}

	public void SkipLineByOffset(int offset)
	{
		Index += offset;
		Line++;
		LineOffset = 0L;
	}

	public void ForceSkipLineAfterNonBreak()
	{
		if (LineOffset != 0L)
		{
			Line++;
			LineOffset = 0L;
		}
	}
}
