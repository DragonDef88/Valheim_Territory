using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization.Utilities;

public sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState : IDisposable
{
	private readonly Dictionary<Type, object> items = new Dictionary<Type, object>();

	public T Get<T>() where T : class, new()
	{
		if (!items.TryGetValue(typeof(T), out object value))
		{
			value = new T();
			items.Add(typeof(T), value);
		}
		return (T)value;
	}

	public void OnDeserialization()
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPostDeserializationCallback item in items.Values.OfType<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPostDeserializationCallback>())
		{
			item.OnDeserialization();
		}
	}

	public void Dispose()
	{
		foreach (IDisposable item in items.Values.OfType<IDisposable>())
		{
			item.Dispose();
		}
	}
}
