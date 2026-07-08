using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDictionaryNodeDeserializer : DictionaryDeserializer, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDictionaryNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory, bool duplicateKeyChecking)
		: base(duplicateKeyChecking)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
	}

	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		Type implementationOfOpenGenericInterface = expectedType.GetImplementationOfOpenGenericInterface(typeof(IDictionary<, >));
		Type type;
		Type type2;
		IDictionary dictionary;
		if (implementationOfOpenGenericInterface != null)
		{
			Type[] genericArguments = implementationOfOpenGenericInterface.GetGenericArguments();
			type = genericArguments[0];
			type2 = genericArguments[1];
			value = objectFactory.Create(expectedType);
			dictionary = value as IDictionary;
			if (dictionary == null)
			{
				dictionary = (IDictionary)Activator.CreateInstance(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EGenericDictionaryToNonGenericAdapter<, >).MakeGenericType(type, type2), value);
			}
		}
		else
		{
			if (!typeof(IDictionary).IsAssignableFrom(expectedType))
			{
				value = null;
				return false;
			}
			type = typeof(object);
			type2 = typeof(object);
			value = objectFactory.Create(expectedType);
			dictionary = (IDictionary)value;
		}
		Deserialize(type, type2, parser, nestedObjectDeserializer, dictionary, rootDeserializer);
		return true;
	}
}
