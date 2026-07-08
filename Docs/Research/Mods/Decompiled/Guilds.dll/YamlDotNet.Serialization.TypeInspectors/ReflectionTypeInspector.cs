using System;

namespace YamlDotNet.Serialization.TypeInspectors;

internal abstract class ReflectionTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeInspectorSkeleton
{
	public override string GetEnumName(Type enumType, string name)
	{
		return name;
	}

	public override string GetEnumValue(object enumValue)
	{
		if (enumValue == null)
		{
			return string.Empty;
		}
		return enumValue.ToString();
	}
}
