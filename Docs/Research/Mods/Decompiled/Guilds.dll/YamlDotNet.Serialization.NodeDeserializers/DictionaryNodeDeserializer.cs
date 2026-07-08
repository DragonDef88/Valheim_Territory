using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class DictionaryNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	public DictionaryNodeDeserializer(IObjectFactory objectFactory)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
	}

	bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
	{
		Type implementedGenericInterface = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IDictionary<, >));
		Type type;
		Type type2;
		IDictionary dictionary;
		if (implementedGenericInterface != null)
		{
			Type[] genericArguments = implementedGenericInterface.GetGenericArguments();
			type = genericArguments[0];
			type2 = genericArguments[1];
			value = objectFactory.Create(expectedType);
			dictionary = value as IDictionary;
			if (dictionary == null)
			{
				dictionary = (IDictionary)Activator.CreateInstance(typeof(GenericDictionaryToNonGenericAdapter<, >).MakeGenericType(type, type2), value);
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
		DeserializeHelper(type, type2, parser, nestedObjectDeserializer, dictionary);
		return true;
	}

	private static void DeserializeHelper(Type tKey, Type tValue, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IDictionary result)
	{
		IDictionary result2 = result;
		parser.Consume<MappingStart>();
		MappingEnd @event;
		while (!parser.TryConsume<MappingEnd>(out @event))
		{
			object key = nestedObjectDeserializer(parser, tKey);
			object value = nestedObjectDeserializer(parser, tValue);
			IValuePromise valuePromise = value as IValuePromise;
			if (key is IValuePromise valuePromise2)
			{
				if (valuePromise == null)
				{
					valuePromise2.ValueAvailable += delegate(object? v)
					{
						result2[v] = value;
					};
					continue;
				}
				bool hasFirstPart = false;
				valuePromise2.ValueAvailable += delegate(object? v)
				{
					if (hasFirstPart)
					{
						result2[v] = value;
					}
					else
					{
						key = v;
						hasFirstPart = true;
					}
				};
				valuePromise.ValueAvailable += delegate(object? v)
				{
					if (hasFirstPart)
					{
						result2[key] = v;
					}
					else
					{
						value = v;
						hasFirstPart = true;
					}
				};
			}
			else if (valuePromise == null)
			{
				result2[key] = value;
			}
			else
			{
				valuePromise.ValueAvailable += delegate(object? v)
				{
					result2[key] = v;
				};
			}
		}
	}
}
