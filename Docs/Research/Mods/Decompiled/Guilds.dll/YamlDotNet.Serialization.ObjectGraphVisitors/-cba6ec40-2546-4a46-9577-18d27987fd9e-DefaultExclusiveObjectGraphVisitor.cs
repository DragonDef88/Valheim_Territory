using System;
using System.ComponentModel;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultExclusiveObjectGraphVisitor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EChainedObjectGraphVisitor
{
	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultExclusiveObjectGraphVisitor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter> nextVisitor)
		: base(nextVisitor)
	{
	}

	private static object? GetDefault(Type type)
	{
		if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.IsValueType(type))
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}

	public override bool EnterMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter context, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		if (!object.Equals(value.Value, GetDefault(value.Type)))
		{
			return base.EnterMapping(key, value, context, serializer);
		}
		return false;
	}

	public override bool EnterMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter context, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		DefaultValueAttribute customAttribute = key.GetCustomAttribute<DefaultValueAttribute>();
		object objB = ((customAttribute != null) ? customAttribute.Value : GetDefault(key.Type));
		if (!object.Equals(value.Value, objB))
		{
			return base.EnterMapping(key, value, context, serializer);
		}
		return false;
	}
}
