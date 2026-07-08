using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIScanner
{
	_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark CurrentPosition { get; }

	_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken? Current { get; }

	bool MoveNext();

	bool MoveNextWithoutConsuming();

	void ConsumeCurrent();
}
