using System;
using System.Collections.Generic;
using System.Text;

namespace HarmonyLib.Tools;

internal static class TypeNameHelper
{
	private static readonly Dictionary<Type, string> BuiltInTypeNames = new Dictionary<Type, string>
	{
		{
			typeof(void),
			"void"
		},
		{
			typeof(bool),
			"bool"
		},
		{
			typeof(byte),
			"byte"
		},
		{
			typeof(char),
			"char"
		},
		{
			typeof(decimal),
			"decimal"
		},
		{
			typeof(double),
			"double"
		},
		{
			typeof(float),
			"float"
		},
		{
			typeof(int),
			"int"
		},
		{
			typeof(long),
			"long"
		},
		{
			typeof(object),
			"object"
		},
		{
			typeof(sbyte),
			"sbyte"
		},
		{
			typeof(short),
			"short"
		},
		{
			typeof(string),
			"string"
		},
		{
			typeof(uint),
			"uint"
		},
		{
			typeof(ulong),
			"ulong"
		},
		{
			typeof(ushort),
			"ushort"
		}
	};

	private static readonly Dictionary<string, string> FSharpTypeNames = new Dictionary<string, string>
	{
		{ "Unit", "void" },
		{ "FSharpOption", "Option" },
		{ "FSharpAsync", "Async" },
		{ "FSharpOption`1", "Option" },
		{ "FSharpAsync`1", "Async" }
	};

	public static string GetTypeDisplayName(Type type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ProcessType(stringBuilder, type);
		return stringBuilder.ToString();
	}

	private static void ProcessType(StringBuilder builder, Type type)
	{
		string value;
		if (type.IsGenericType)
		{
			Type underlyingType = Nullable.GetUnderlyingType(type);
			if ((object)underlyingType != null)
			{
				ProcessType(builder, underlyingType);
				builder.Append('?');
			}
			else
			{
				Type[] genericArguments = type.GetGenericArguments();
				ProcessGenericType(builder, type, genericArguments, genericArguments.Length);
			}
		}
		else if (type.IsArray)
		{
			ProcessArrayType(builder, type);
		}
		else if (BuiltInTypeNames.TryGetValue(type, out value))
		{
			builder.Append(value);
		}
		else if (type.Namespace == "System")
		{
			builder.Append(type.Name);
		}
		else if (type.Assembly.ManifestModule.Name == "FSharp.Core.dll" && FSharpTypeNames.TryGetValue(type.Name, out value))
		{
			builder.Append(value);
		}
		else if (type.IsGenericParameter)
		{
			builder.Append(type.Name);
		}
		else
		{
			builder.Append(type.FullName ?? type.Name);
		}
	}

	private static void ProcessArrayType(StringBuilder builder, Type type)
	{
		Type type2 = type;
		while (type2.IsArray)
		{
			type2 = type2.GetElementType();
		}
		ProcessType(builder, type2);
		while (type.IsArray)
		{
			builder.Append('[');
			builder.Append(',', type.GetArrayRank() - 1);
			builder.Append(']');
			type = type.GetElementType();
		}
	}

	private static void ProcessGenericType(StringBuilder builder, Type type, Type[] genericArguments, int length)
	{
		int num = 0;
		if (type.IsNested)
		{
			num = type.DeclaringType.GetGenericArguments().Length;
		}
		if (type.IsNested)
		{
			ProcessGenericType(builder, type.DeclaringType, genericArguments, num);
			builder.Append('+');
		}
		else if (!string.IsNullOrEmpty(type.Namespace))
		{
			builder.Append(type.Namespace);
			builder.Append('.');
		}
		int num2 = type.Name.IndexOf('`');
		if (num2 <= 0)
		{
			builder.Append(type.Name);
			return;
		}
		if (type.Assembly.ManifestModule.Name == "FSharp.Core.dll" && FSharpTypeNames.TryGetValue(type.Name, out var value))
		{
			builder.Append(value);
		}
		else
		{
			builder.Append(type.Name, 0, num2);
		}
		builder.Append('<');
		for (int i = num; i < length; i++)
		{
			ProcessType(builder, genericArguments[i]);
			if (i + 1 != length)
			{
				builder.Append(',');
				if (!genericArguments[i + 1].IsGenericParameter)
				{
					builder.Append(' ');
				}
			}
		}
		builder.Append('>');
	}
}
