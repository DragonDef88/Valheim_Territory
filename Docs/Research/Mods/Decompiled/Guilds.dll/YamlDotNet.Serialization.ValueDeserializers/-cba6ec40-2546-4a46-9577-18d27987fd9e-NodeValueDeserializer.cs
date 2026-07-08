using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ValueDeserializers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeValueDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer
{
	private readonly IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> deserializers;

	private readonly IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver> typeResolvers;

	private readonly ITypeConverter typeConverter;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeValueDeserializer(IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> deserializers, IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver> typeResolvers, ITypeConverter typeConverter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector)
	{
		this.deserializers = deserializers ?? throw new ArgumentNullException("deserializers");
		this.typeResolvers = typeResolvers ?? throw new ArgumentNullException("typeResolvers");
		this.typeConverter = typeConverter ?? throw new ArgumentNullException("typeConverter");
		this.enumNamingConvention = enumNamingConvention ?? throw new ArgumentNullException("enumNamingConvention");
		this.typeInspector = typeInspector;
	}

	public object? DeserializeValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState state, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer nestedObjectDeserializer)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser2 = parser;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESerializerState state2 = state;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer nestedObjectDeserializer2 = nestedObjectDeserializer;
		parser2.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent>(out var @event);
		Type typeFromEvent = GetTypeFromEvent(@event, expectedType);
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer = (Type x) => DeserializeValue(parser2, x, state2, nestedObjectDeserializer2);
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		try
		{
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer deserializer in deserializers)
			{
				if (deserializer.Deserialize(parser2, typeFromEvent, (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser r, Type t) => nestedObjectDeserializer2.DeserializeValue(r, t, state2, nestedObjectDeserializer2), out object value, rootDeserializer))
				{
					return typeConverter.ChangeType(value, expectedType, enumNamingConvention, typeInspector);
				}
			}
		}
		catch (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			start = @event?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
			end = @event?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, "Exception during deserialization", innerException);
		}
		start = @event?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		end = @event?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, "No node deserializer was able to deserialize the node into type " + expectedType.AssemblyQualifiedName);
	}

	private Type GetTypeFromEvent(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent? nodeEvent, Type currentType)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver typeResolver in typeResolvers)
		{
			if (typeResolver.Resolve(nodeEvent, ref currentType))
			{
				break;
			}
		}
		return currentType;
	}
}
