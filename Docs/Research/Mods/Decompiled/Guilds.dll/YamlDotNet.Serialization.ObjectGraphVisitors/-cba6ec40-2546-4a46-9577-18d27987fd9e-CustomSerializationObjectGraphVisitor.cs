using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECustomSerializationObjectGraphVisitor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EChainedObjectGraphVisitor
{
	private readonly TypeConverterCache typeConverters;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer nestedObjectSerializer;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECustomSerializationObjectGraphVisitor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter> nextVisitor, IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> typeConverters, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer nestedObjectSerializer)
		: base(nextVisitor)
	{
		this.typeConverters = new TypeConverterCache(typeConverters);
		this.nestedObjectSerializer = nestedObjectSerializer;
	}

	public override bool Enter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor? propertyDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter context, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		if (propertyDescriptor?.ConverterType != null)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter converterByType = typeConverters.GetConverterByType(propertyDescriptor.ConverterType);
			converterByType.WriteYaml(context, value.Value, value.Type, serializer);
			return false;
		}
		if (typeConverters.TryGetConverterForType(value.Type, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter typeConverter))
		{
			typeConverter.WriteYaml(context, value.Value, value.Type, serializer);
			return false;
		}
		if (value.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible.Write(context, nestedObjectSerializer);
			return false;
		}
		if (value.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlSerializable _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlSerializable)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlSerializable.WriteYaml(context);
			return false;
		}
		return base.Enter(propertyDescriptor, value, context, serializer);
	}
}
