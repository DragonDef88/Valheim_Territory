using System;
using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectFactories;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal class StaticDictionaryNodeDeserializer : DictionaryDeserializer, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	private readonly StaticObjectFactory objectFactory;

	public StaticDictionaryNodeDeserializer(StaticObjectFactory objectFactory, bool duplicateKeyChecking)
		: base(duplicateKeyChecking)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
	}

	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser reader, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		if (objectFactory.IsDictionary(expectedType))
		{
			if (!(objectFactory.Create(expectedType) is IDictionary dictionary))
			{
				value = null;
				return false;
			}
			Type keyType = objectFactory.GetKeyType(expectedType);
			Type valueType = objectFactory.GetValueType(expectedType);
			value = dictionary;
			base.Deserialize(keyType, valueType, reader, nestedObjectDeserializer, dictionary, rootDeserializer);
			return true;
		}
		value = null;
		return false;
	}
}
