using System;
using System.Collections;
using System.ComponentModel;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class DefaultValuesObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private readonly DefaultValuesHandling handling;

	public DefaultValuesObjectGraphVisitor(DefaultValuesHandling handling, IObjectGraphVisitor<IEmitter> nextVisitor)
		: base(nextVisitor)
	{
		this.handling = handling;
	}

	private static object? GetDefault(Type type)
	{
		if (!type.IsValueType())
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}

	public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
	{
		DefaultValuesHandling defaultValuesHandling = handling;
		YamlMemberAttribute customAttribute = key.GetCustomAttribute<YamlMemberAttribute>();
		if (customAttribute != null && customAttribute.IsDefaultValuesHandlingSpecified)
		{
			defaultValuesHandling = customAttribute.DefaultValuesHandling;
		}
		if ((defaultValuesHandling & DefaultValuesHandling.OmitNull) != 0 && value.Value == null)
		{
			return false;
		}
		if ((defaultValuesHandling & DefaultValuesHandling.OmitEmptyCollections) != 0 && value.Value is IEnumerable enumerable)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			bool flag = enumerator.MoveNext();
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
			if (!flag)
			{
				return false;
			}
		}
		if ((defaultValuesHandling & DefaultValuesHandling.OmitDefaults) != 0)
		{
			object objB = key.GetCustomAttribute<DefaultValueAttribute>()?.Value ?? GetDefault(key.Type);
			if (object.Equals(value.Value, objB))
			{
				return false;
			}
		}
		return base.EnterMapping(key, value, context);
	}
}
