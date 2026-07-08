namespace YamlDotNet.Core;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor cursor;

	public bool IsPossible { get; private set; }

	public bool IsRequired { get; }

	public int TokenNumber { get; }

	public long Index => cursor.Index;

	public long Line => cursor.Line;

	public long LineOffset => cursor.LineOffset;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark Mark => cursor.Mark();

	public void MarkAsImpossible()
	{
		IsPossible = false;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey()
	{
		cursor = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor();
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey(bool isRequired, int tokenNumber, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor cursor)
	{
		IsPossible = true;
		IsRequired = isRequired;
		TokenNumber = tokenNumber;
		this.cursor = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor(cursor);
	}
}
