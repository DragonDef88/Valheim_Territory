using System;
using YamlDotNet.Serialization.ObjectFactories;

namespace YamlDotNet.Serialization;

internal abstract class StaticContext
{
	public virtual bool IsKnownType(Type type)
	{
		throw new NotImplementedException();
	}

	public virtual _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver GetTypeResolver()
	{
		throw new NotImplementedException();
	}

	public virtual StaticObjectFactory GetFactory()
	{
		throw new NotImplementedException();
	}

	public virtual _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector GetTypeInspector()
	{
		throw new NotImplementedException();
	}
}
