using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.EventEmitters;

internal abstract class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EChainedEventEmitter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEventEmitter
{
	protected readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEventEmitter nextEmitter;

	protected _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EChainedEventEmitter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEventEmitter nextEmitter)
	{
		this.nextEmitter = nextEmitter ?? throw new ArgumentNullException("nextEmitter");
	}

	public virtual void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		nextEmitter.Emit(eventInfo, emitter);
	}

	public virtual void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		nextEmitter.Emit(eventInfo, emitter);
	}

	public virtual void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStartEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		nextEmitter.Emit(eventInfo, emitter);
	}

	public virtual void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEndEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		nextEmitter.Emit(eventInfo, emitter);
	}

	public virtual void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStartEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		nextEmitter.Emit(eventInfo, emitter);
	}

	public virtual void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEndEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		nextEmitter.Emit(eventInfo, emitter);
	}
}
