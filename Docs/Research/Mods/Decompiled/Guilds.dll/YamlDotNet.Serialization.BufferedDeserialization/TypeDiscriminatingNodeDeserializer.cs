using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization;

internal class TypeDiscriminatingNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	private readonly IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> innerDeserializers;

	private readonly IList<ITypeDiscriminator> typeDiscriminators;

	private readonly int maxDepthToBuffer;

	private readonly int maxLengthToBuffer;

	public TypeDiscriminatingNodeDeserializer(IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> innerDeserializers, IList<ITypeDiscriminator> typeDiscriminators, int maxDepthToBuffer, int maxLengthToBuffer)
	{
		this.innerDeserializers = innerDeserializers;
		this.typeDiscriminators = typeDiscriminators;
		this.maxDepthToBuffer = maxDepthToBuffer;
		this.maxLengthToBuffer = maxLengthToBuffer;
	}

	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser reader, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		Type expectedType2 = expectedType;
		if (!reader.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>(out var _))
		{
			value = null;
			return false;
		}
		IEnumerable<ITypeDiscriminator> enumerable = typeDiscriminators.Where((ITypeDiscriminator t) => t.BaseType.IsAssignableFrom(expectedType2));
		if (!enumerable.Any())
		{
			value = null;
			return false;
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = reader.Current.Start;
		Type expectedType3 = expectedType2;
		ParserBuffer parserBuffer;
		try
		{
			parserBuffer = new ParserBuffer(reader, maxDepthToBuffer, maxLengthToBuffer);
		}
		catch (Exception innerException)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = reader.Current.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, "Failed to buffer yaml node", innerException);
		}
		try
		{
			foreach (ITypeDiscriminator item in enumerable)
			{
				parserBuffer.Reset();
				if (item.TryDiscriminate(parserBuffer, out Type suggestedType))
				{
					expectedType3 = suggestedType;
					break;
				}
			}
		}
		catch (Exception innerException2)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = reader.Current.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, "Failed to discriminate type", innerException2);
		}
		parserBuffer.Reset();
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer innerDeserializer in innerDeserializers)
		{
			if (innerDeserializer.Deserialize(parserBuffer, expectedType3, nestedObjectDeserializer, out value, rootDeserializer))
			{
				return true;
			}
		}
		value = null;
		return false;
	}
}
