using System;

namespace YamlDotNet.Serialization.ObjectFactories;

internal sealed class LambdaObjectFactory : IObjectFactory
{
	private readonly Func<Type, object> factory;

	public LambdaObjectFactory(Func<Type, object> factory)
	{
		this.factory = factory ?? throw new ArgumentNullException("factory");
	}

	public object Create(Type type)
	{
		return factory(type);
	}
}
