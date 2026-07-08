using System;

namespace YamlDotNet.Serialization;

internal interface IObjectFactory
{
	object Create(Type type);
}
