using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ValueDeserializers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasValueDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer
{
	private sealed class AliasState : Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName, ValuePromise>, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPostDeserializationCallback
	{
		public void OnDeserialization()
		{
			foreach (ValuePromise value in base.Values)
			{
				if (!value.HasValue)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias alias = value.Alias;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = alias.Start;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = alias.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorNotFoundException(in start, in end, $"Anchor '{alias.Value}' not found");
				}
			}
		}
	}

	private sealed class ValuePromise : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise
	{
		private object? value;

		public readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias? Alias;

		public bool HasValue { get; private set; }

		public object? Value
		{
			get
			{
				if (!HasValue)
				{
					throw new InvalidOperationException("Value not set");
				}
				return value;
			}
			set
			{
				if (HasValue)
				{
					throw new InvalidOperationException("Value already set");
				}
				HasValue = true;
				this.value = value;
				this.ValueAvailable?.Invoke(value);
			}
		}

		public event Action<object?>? ValueAvailable;

		public ValuePromise(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias alias)
		{
			Alias = alias;
		}

		public ValuePromise(object? value)
		{
			HasValue = true;
			this.value = value;
		}
	}

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer innerDeserializer;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasValueDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer innerDeserializer)
	{
		this.innerDeserializer = innerDeserializer ?? throw new ArgumentNullException("innerDeserializer");
	}

	public object? DeserializeValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState state, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer nestedObjectDeserializer)
	{
		if (parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias>(out var @event))
		{
			AliasState aliasState = state.Get<AliasState>();
			if (!aliasState.TryGetValue(@event.Value, out ValuePromise value))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = @event.Start;
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = @event.End;
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorNotFoundException(in start, in end, $"Alias ${@event.Value} cannot precede anchor declaration");
			}
			if (!value.HasValue)
			{
				return value;
			}
			return value.Value;
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty;
		if (parser.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent>(out var event2) && !event2.Anchor.IsEmpty)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName = event2.Anchor;
			AliasState aliasState2 = state.Get<AliasState>();
			if (!aliasState2.ContainsKey(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName))
			{
				aliasState2[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName] = new ValuePromise(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName));
			}
		}
		object obj = innerDeserializer.DeserializeValue(parser, expectedType, state, nestedObjectDeserializer);
		if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.IsEmpty)
		{
			AliasState aliasState3 = state.Get<AliasState>();
			if (!aliasState3.TryGetValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName, out ValuePromise value2))
			{
				aliasState3.Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName, new ValuePromise(obj));
			}
			else if (!value2.HasValue)
			{
				value2.Value = obj;
			}
			else
			{
				aliasState3[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName] = new ValuePromise(obj);
			}
		}
		return obj;
	}
}
