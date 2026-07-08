using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECollectionNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECollectionNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
		this.enumNamingConvention = enumNamingConvention;
		this.typeInspector = typeInspector;
	}

	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		bool canUpdate = true;
		Type implementationOfOpenGenericInterface = expectedType.GetImplementationOfOpenGenericInterface(typeof(ICollection<>));
		Type type;
		IList list;
		if (implementationOfOpenGenericInterface != null)
		{
			Type[] genericArguments = implementationOfOpenGenericInterface.GetGenericArguments();
			type = genericArguments[0];
			value = objectFactory.Create(expectedType);
			list = value as IList;
			if (list == null)
			{
				Type implementationOfOpenGenericInterface2 = expectedType.GetImplementationOfOpenGenericInterface(typeof(IList<>));
				canUpdate = implementationOfOpenGenericInterface2 != null;
				list = (IList)Activator.CreateInstance(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EGenericCollectionToNonGenericAdapter<>).MakeGenericType(type), value);
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
		DeserializeHelper(type, parser, nestedObjectDeserializer, list, canUpdate, enumNamingConvention, typeInspector);
		return true;
	}

	internal static void DeserializeHelper(Type tItem, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, IList result, bool canUpdate, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector, Action<int, object?>? promiseResolvedHandler = null)
	{
		Action<int, object?> promiseResolvedHandler2 = promiseResolvedHandler;
		IList result2 = result;
		Type tItem2 = tItem;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention2 = enumNamingConvention;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector2 = typeInspector;
		parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart>();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd>(out @event))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent current = parser.Current;
			object obj = nestedObjectDeserializer(parser, tItem2);
			if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise)
			{
				if (!canUpdate)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = current?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = current?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EForwardAnchorNotSupportedException(in start, in end, "Forward alias references are not allowed because this type does not implement IList<>");
				}
				int index = result2.Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.IsValueType(tItem2) ? Activator.CreateInstance(tItem2) : null);
				if (promiseResolvedHandler2 != null)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise.ValueAvailable += delegate(object? v)
					{
						promiseResolvedHandler2(index, v);
					};
				}
				else
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValuePromise.ValueAvailable += delegate(object? v)
					{
						result2[index] = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeConverter.ChangeType(v, tItem2, enumNamingConvention2, typeInspector2);
					};
				}
			}
			else
			{
				result2.Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeConverter.ChangeType(obj, tItem2, enumNamingConvention2, typeInspector2));
			}
		}
	}
}
