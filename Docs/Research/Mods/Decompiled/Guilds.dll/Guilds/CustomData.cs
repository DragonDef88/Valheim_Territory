using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Guilds;

[PublicAPI]
public class CustomData
{
	internal Dictionary<Type, object> data = new Dictionary<Type, object>();

	internal Dictionary<string, object> unknown = new Dictionary<string, object>();
}
