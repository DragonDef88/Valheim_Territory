using System;
using System.Collections.Generic;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.ObjectGraphTraversalStrategies;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERoundtripObjectGraphTraversalStrategy : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFullObjectGraphTraversalStrategy
{
	private readonly TypeConverterCache converters;

	private readonly Settings settings;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERoundtripObjectGraphTraversalStrategy(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> converters, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeDescriptor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver, int maxRecursion, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention namingConvention, Settings settings, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory factory)
		: base(typeDescriptor, typeResolver, maxRecursion, namingConvention, factory)
	{
		this.converters = new TypeConverterCache(converters);
		this.settings = settings;
	}

	protected override void TraverseProperties<TContext>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectGraphVisitor<TContext> visitor, TContext context, Stack<ObjectPathSegment> path, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		if (!value.Type.HasDefaultConstructor(settings.AllowPrivateConstructors) && !converters.TryGetConverterForType(value.Type, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter _))
		{
			throw new InvalidOperationException($"Type '{value.Type}' cannot be deserialized because it does not have a default constructor or a type converter.");
		}
		base.TraverseProperties(value, visitor, context, path, serializer);
	}
}
