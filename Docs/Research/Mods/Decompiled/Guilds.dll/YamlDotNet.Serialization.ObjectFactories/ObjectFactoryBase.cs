using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.ObjectFactories;

internal abstract class ObjectFactoryBase : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory
{
	public abstract object Create(Type type);

	public virtual object? CreatePrimitive(Type type)
	{
		if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.IsValueType(type))
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}

	public virtual void ExecuteOnDeserialized(object value)
	{
	}

	public virtual void ExecuteOnDeserializing(object value)
	{
	}

	public virtual void ExecuteOnSerialized(object value)
	{
	}

	public virtual void ExecuteOnSerializing(object value)
	{
	}

	public virtual bool GetDictionary(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments)
	{
		Type implementationOfOpenGenericInterface = descriptor.Type.GetImplementationOfOpenGenericInterface(typeof(IDictionary<, >));
		if (implementationOfOpenGenericInterface != null)
		{
			genericArguments = implementationOfOpenGenericInterface.GetGenericArguments();
			object obj = Activator.CreateInstance(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EGenericDictionaryToNonGenericAdapter<, >).MakeGenericType(genericArguments), descriptor.Value);
			dictionary = obj as IDictionary;
			return true;
		}
		genericArguments = null;
		dictionary = null;
		return false;
	}

	public virtual Type GetValueType(Type type)
	{
		Type implementationOfOpenGenericInterface = type.GetImplementationOfOpenGenericInterface(typeof(IEnumerable<>));
		return (implementationOfOpenGenericInterface != null) ? implementationOfOpenGenericInterface.GetGenericArguments()[0] : typeof(object);
	}
}
