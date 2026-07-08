using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class ObjectNodeDeserializer : INodeDeserializer
{
	private readonly IObjectFactory objectFactory;

	private readonly ITypeInspector typeDescriptor;

	private readonly bool ignoreUnmatched;

	public ObjectNodeDeserializer(IObjectFactory objectFactory, ITypeInspector typeDescriptor, bool ignoreUnmatched)
	{
		this.objectFactory = objectFactory ?? throw new ArgumentNullException("objectFactory");
		this.typeDescriptor = typeDescriptor ?? throw new ArgumentNullException("typeDescriptor");
		this.ignoreUnmatched = ignoreUnmatched;
	}

	bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
	{
		if (!parser.TryConsume<MappingStart>(out var _))
		{
			value = null;
			return false;
		}
		Type type = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
		value = objectFactory.Create(type);
		MappingEnd event2;
		while (!parser.TryConsume<MappingEnd>(out event2))
		{
			Scalar scalar = parser.Consume<Scalar>();
			IPropertyDescriptor property = typeDescriptor.GetProperty(type, null, scalar.Value, ignoreUnmatched);
			if (property == null)
			{
				parser.SkipThisAndNestedEvents();
				continue;
			}
			object obj = nestedObjectDeserializer(parser, property.Type);
			if (obj is IValuePromise valuePromise)
			{
				object valueRef = value;
				valuePromise.ValueAvailable += delegate(object? v)
				{
					object value3 = TypeConverter.ChangeType(v, property.Type);
					property.Write(valueRef, value3);
				};
			}
			else
			{
				object value2 = TypeConverter.ChangeType(obj, property.Type);
				property.Write(value, value2);
			}
		}
		return true;
	}
}
