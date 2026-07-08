using System;
using System.Collections;
using System.ComponentModel;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesObjectGraphVisitor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EChainedObjectGraphVisitor
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling handling;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory factory;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesObjectGraphVisitor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling handling, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter> nextVisitor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory factory)
		: base(nextVisitor)
	{
		this.handling = handling;
		this.factory = factory;
	}

	private object? GetDefault(Type type)
	{
		return factory.CreatePrimitive(type);
	}

	public override bool EnterMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter context, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling defaultValuesHandling = handling;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute customAttribute = key.GetCustomAttribute<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute>();
		if (customAttribute != null && customAttribute.IsDefaultValuesHandlingSpecified)
		{
			defaultValuesHandling = customAttribute.DefaultValuesHandling;
		}
		if ((defaultValuesHandling & _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling.OmitNull) != 0 && value.Value == null)
		{
			return false;
		}
		if ((defaultValuesHandling & _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling.OmitEmptyCollections) != 0 && value.Value is IEnumerable enumerable)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			bool flag = enumerator.MoveNext();
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
			if (!flag)
			{
				return false;
			}
		}
		if ((defaultValuesHandling & _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling.OmitDefaults) != 0)
		{
			object objB = key.GetCustomAttribute<DefaultValueAttribute>()?.Value ?? GetDefault(key.Type);
			if (object.Equals(value.Value, objB))
			{
				return false;
			}
		}
		return base.EnterMapping(key, value, context, serializer);
	}
}
