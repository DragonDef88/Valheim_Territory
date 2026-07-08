using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ValueDeserializers;

internal sealed class NodeValueDeserializer : IValueDeserializer
{
	private readonly IList<INodeDeserializer> deserializers;

	private readonly IList<INodeTypeResolver> typeResolvers;

	public NodeValueDeserializer(IList<INodeDeserializer> deserializers, IList<INodeTypeResolver> typeResolvers)
	{
		this.deserializers = deserializers ?? throw new ArgumentNullException("deserializers");
		this.typeResolvers = typeResolvers ?? throw new ArgumentNullException("typeResolvers");
	}

	public object? DeserializeValue(IParser parser, Type expectedType, SerializerState state, IValueDeserializer nestedObjectDeserializer)
	{
		IValueDeserializer nestedObjectDeserializer2 = nestedObjectDeserializer;
		SerializerState state2 = state;
		parser.Accept<NodeEvent>(out var @event);
		Type typeFromEvent = GetTypeFromEvent(@event, expectedType);
		try
		{
			foreach (INodeDeserializer deserializer in deserializers)
			{
				if (deserializer.Deserialize(parser, typeFromEvent, (IParser r, Type t) => nestedObjectDeserializer2.DeserializeValue(r, t, state2, nestedObjectDeserializer2), out object value))
				{
					return TypeConverter.ChangeType(value, expectedType);
				}
			}
		}
		catch (YamlException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new YamlException(@event?.Start ?? Mark.Empty, @event?.End ?? Mark.Empty, "Exception during deserialization", innerException);
		}
		throw new YamlException(@event?.Start ?? Mark.Empty, @event?.End ?? Mark.Empty, "No node deserializer was able to deserialize the node into type " + expectedType.AssemblyQualifiedName);
	}

	private Type GetTypeFromEvent(NodeEvent? nodeEvent, Type currentType)
	{
		using (IEnumerator<INodeTypeResolver> enumerator = typeResolvers.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Resolve(nodeEvent, ref currentType))
			{
			}
		}
		return currentType;
	}
}
