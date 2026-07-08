using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Helpers;

namespace YamlDotNet.Serialization.ObjectGraphTraversalStrategies;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFullObjectGraphTraversalStrategy : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphTraversalStrategy
{
	protected readonly struct ObjectPathSegment
	{
		public readonly object Name;

		public readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor Value;

		public ObjectPathSegment(object name, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value)
		{
			Name = name;
			Value = value;
		}
	}

	private readonly int maxRecursion;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeDescriptor;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention namingConvention;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFullObjectGraphTraversalStrategy(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver, int maxRecursion, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention namingConvention, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory)
	{
		if (maxRecursion <= 0)
		{
			throw new ArgumentOutOfRangeException("maxRecursion", maxRecursion, "maxRecursion must be greater than 1");
		}
		this.typeDescriptor = typeDescriptor ?? throw new ArgumentNullException("typeDescriptor");
		this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
		this.maxRecursion = maxRecursion;
		this.namingConvention = namingConvention ?? throw new ArgumentNullException("namingConvention");
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphTraversalStrategy.Traverse<TContext>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor graph, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<TContext> visitor, TContext context, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		Traverse(null, "<root>", graph, visitor, context, new Stack<ObjectPathSegment>(maxRecursion), serializer);
	}

	protected virtual void Traverse<TContext>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor? propertyDescriptor, object name, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		if (path.Count >= maxRecursion)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Too much recursion when traversing the object graph.");
			stringBuilder.AppendLine("The path to reach this recursion was:");
			Stack<KeyValuePair<string, string>> stack = new Stack<KeyValuePair<string, string>>(path.Count);
			int num = 0;
			foreach (ObjectPathSegment item in path)
			{
				string text = item.Name?.ToString() ?? string.Empty;
				num = Math.Max(num, text.Length);
				stack.Push(new KeyValuePair<string, string>(text, item.Value.Type.FullName));
			}
			foreach (KeyValuePair<string, string> item2 in stack)
			{
				stringBuilder.Append(" -> ").Append(item2.Key.PadRight(num)).Append("  [")
					.Append(item2.Value)
					.AppendLine("]");
			}
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMaximumRecursionLevelReachedException(stringBuilder.ToString());
		}
		if (!visitor.Enter(propertyDescriptor, value, context, serializer))
		{
			return;
		}
		path.Push(new ObjectPathSegment(name, value));
		try
		{
			TypeCode typeCode = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.GetTypeCode(value.Type);
			switch (typeCode)
			{
			case TypeCode.Boolean:
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
			case TypeCode.DateTime:
			case TypeCode.String:
				visitor.VisitScalar(value, context, serializer);
				return;
			case TypeCode.Empty:
				throw new NotSupportedException($"TypeCode.{typeCode} is not supported.");
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.IsDbNull(value))
			{
				visitor.VisitScalar(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(null, typeof(object), typeof(object)), context, serializer);
			}
			if (value.Value == null || value.Type == typeof(TimeSpan))
			{
				visitor.VisitScalar(value, context, serializer);
				return;
			}
			Type underlyingType = Nullable.GetUnderlyingType(value.Type);
			Type type = underlyingType ?? FsharpHelper.GetOptionUnderlyingType(value.Type);
			object obj = ((type != null) ? FsharpHelper.GetValue(value) : null);
			if (underlyingType != null)
			{
				Traverse(propertyDescriptor, "Value", new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(value.Value, underlyingType, value.Type, value.ScalarStyle), visitor, context, path, serializer);
			}
			else if (type != null && obj != null)
			{
				Traverse(propertyDescriptor, "Value", new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(FsharpHelper.GetValue(value), type, value.Type, value.ScalarStyle), visitor, context, path, serializer);
			}
			else
			{
				TraverseObject(propertyDescriptor, value, visitor, context, path, serializer);
			}
		}
		finally
		{
			path.Pop();
		}
	}

	protected virtual void TraverseObject<TContext>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor? propertyDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		IDictionary dictionary;
		Type[] genericArguments;
		if (typeof(IDictionary).IsAssignableFrom(value.Type))
		{
			TraverseDictionary(propertyDescriptor, value, visitor, typeof(object), typeof(object), context, path, serializer);
		}
		else if (objectFactory.GetDictionary(value, out dictionary, out genericArguments))
		{
			TraverseDictionary(propertyDescriptor, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(dictionary, value.Type, value.StaticType, value.ScalarStyle), visitor, genericArguments[0], genericArguments[1], context, path, serializer);
		}
		else if (typeof(IEnumerable).IsAssignableFrom(value.Type))
		{
			TraverseList(propertyDescriptor, value, visitor, context, path, serializer);
		}
		else
		{
			TraverseProperties(value, visitor, context, path, serializer);
		}
	}

	protected virtual void TraverseDictionary<TContext>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor? propertyDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor dictionary, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<TContext> visitor, Type keyType, Type valueType, TContext context, Stack<ObjectPathSegment> path, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		visitor.VisitMappingStart(dictionary, keyType, valueType, context, serializer);
		bool flag = dictionary.Type.FullName.Equals("System.Dynamic.ExpandoObject");
		foreach (DictionaryEntry? item in (IDictionary)dictionary.NonNullValue())
		{
			DictionaryEntry value = item.Value;
			object obj = (flag ? namingConvention.Apply(value.Key.ToString()) : value.Key);
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor objectDescriptor = GetObjectDescriptor(obj, keyType);
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor objectDescriptor2 = GetObjectDescriptor(value.Value, valueType);
			if (visitor.EnterMapping(objectDescriptor, objectDescriptor2, context, serializer))
			{
				Traverse(propertyDescriptor, obj, objectDescriptor, visitor, context, path, serializer);
				Traverse(propertyDescriptor, obj, objectDescriptor2, visitor, context, path, serializer);
			}
		}
		visitor.VisitMappingEnd(dictionary, context, serializer);
	}

	private void TraverseList<TContext>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor propertyDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		Type valueType = objectFactory.GetValueType(value.Type);
		visitor.VisitSequenceStart(value, valueType, context, serializer);
		int num = 0;
		foreach (object item in (IEnumerable)value.NonNullValue())
		{
			Traverse(propertyDescriptor, num, GetObjectDescriptor(item, valueType), visitor, context, path, serializer);
			num++;
		}
		visitor.VisitSequenceEnd(value, context, serializer);
	}

	protected virtual void TraverseProperties<TContext>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		if (context.GetType() != typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing))
		{
			objectFactory.ExecuteOnSerializing(value.Value);
		}
		visitor.VisitMappingStart(value, typeof(string), typeof(object), context, serializer);
		object obj = value.NonNullValue();
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor property in typeDescriptor.GetProperties(value.Type, obj))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value2 = property.Read(obj);
			if (visitor.EnterMapping(property, value2, context, serializer))
			{
				Traverse(null, property.Name, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(property.Name, typeof(string), typeof(string), _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain), visitor, context, path, serializer);
				Traverse(property, property.Name, value2, visitor, context, path, serializer);
			}
		}
		visitor.VisitMappingEnd(value, context, serializer);
		if (context.GetType() != typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing))
		{
			objectFactory.ExecuteOnSerialized(value.Value);
		}
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor GetObjectDescriptor(object? value, Type staticType)
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDescriptor(value, typeResolver.Resolve(staticType, value), staticType);
	}
}
