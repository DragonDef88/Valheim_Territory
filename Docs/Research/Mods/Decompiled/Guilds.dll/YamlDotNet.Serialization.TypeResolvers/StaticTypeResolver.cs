using System;

namespace YamlDotNet.Serialization.TypeResolvers;

internal sealed class StaticTypeResolver : ITypeResolver
{
	public Type Resolve(Type staticType, object? actualValue)
	{
		return staticType;
	}
}
