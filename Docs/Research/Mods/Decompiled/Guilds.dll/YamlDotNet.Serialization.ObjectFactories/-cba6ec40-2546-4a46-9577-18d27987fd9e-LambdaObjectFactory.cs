using System;

namespace YamlDotNet.Serialization.ObjectFactories;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELambdaObjectFactory : ObjectFactoryBase
{
	private readonly Func<Type, object> factory;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELambdaObjectFactory(Func<Type, object> factory)
	{
		this.factory = factory ?? throw new ArgumentNullException("factory");
	}

	public override object Create(Type type)
	{
		return factory(type);
	}
}
