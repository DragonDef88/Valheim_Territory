using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class CollectionNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	public CollectionNodeDeserializer(IObjectFactory objectFactory)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
	}

	bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
	{
		bool canUpdate = true;
		Type implementedGenericInterface = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(ICollection<>));
		Type type;
		IList list;
		if (implementedGenericInterface != null)
		{
			type = implementedGenericInterface.GetGenericArguments()[0];
			value = objectFactory.Create(expectedType);
			list = value as IList;
			if (list == null)
			{
				canUpdate = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IList<>)) != null;
				list = (IList)Activator.CreateInstance(typeof(GenericCollectionToNonGenericAdapter<>).MakeGenericType(type), value);
			}
		}
		else
		{
			if (!typeof(IList).IsAssignableFrom(expectedType))
			{
				value = null;
				return false;
			}
			type = typeof(object);
			value = objectFactory.Create(expectedType);
			list = (IList)value;
		}
		DeserializeHelper(type, parser, nestedObjectDeserializer, list, canUpdate);
		return true;
	}

	internal static void DeserializeHelper(Type tItem, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IList result, bool canUpdate)
	{
		IList result2 = result;
		Type tItem2 = tItem;
		parser.Consume<SequenceStart>();
		SequenceEnd @event;
		while (!parser.TryConsume<SequenceEnd>(out @event))
		{
			ParsingEvent current = parser.Current;
			object obj = nestedObjectDeserializer(parser, tItem2);
			if (obj is IValuePromise valuePromise)
			{
				if (!canUpdate)
				{
					throw new ForwardAnchorNotSupportedException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "Forward alias references are not allowed because this type does not implement IList<>");
				}
				int index = result2.Add(ReflectionExtensions.IsValueType(tItem2) ? Activator.CreateInstance(tItem2) : null);
				valuePromise.ValueAvailable += delegate(object? v)
				{
					result2[index] = TypeConverter.ChangeType(v, tItem2);
				};
			}
			else
			{
				result2.Add(TypeConverter.ChangeType(obj, tItem2));
			}
		}
	}
}
