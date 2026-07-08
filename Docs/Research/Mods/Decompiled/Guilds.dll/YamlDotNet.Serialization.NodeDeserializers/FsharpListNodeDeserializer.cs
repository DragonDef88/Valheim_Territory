using System;
using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class FsharpListNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention;

	public FsharpListNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention)
	{
		this.typeInspector = typeInspector;
		this.enumNamingConvention = enumNamingConvention;
	}

	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		if (!FsharpHelper.IsFsharpListType(expectedType))
		{
			value = false;
			return false;
		}
		Type type = expectedType.GetGenericArguments()[0];
		Type t = expectedType.GetGenericTypeDefinition().MakeGenericType(type);
		ArrayList arrayList = new ArrayList();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECollectionNodeDeserializer.DeserializeHelper(type, parser, nestedObjectDeserializer, arrayList, canUpdate: true, enumNamingConvention, typeInspector);
		Array array = Array.CreateInstance(type, arrayList.Count);
		arrayList.CopyTo(array, 0);
		object obj = FsharpHelper.CreateFsharpListFromArray(t, type, array);
		value = obj;
		return true;
	}
}
