using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class ScalarNodeDeserializer : INodeDeserializer
{
	private const string BooleanTruePattern = "^(true|y|yes|on)$";

	private const string BooleanFalsePattern = "^(false|n|no|off)$";

	bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
	{
		if (!parser.TryConsume<Scalar>(out var @event))
		{
			value = null;
			return false;
		}
		Type type = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
		if (type.IsEnum())
		{
			value = Enum.Parse(type, @event.Value, ignoreCase: true);
			return true;
		}
		TypeCode typeCode = type.GetTypeCode();
		switch (typeCode)
		{
		case TypeCode.Boolean:
			value = DeserializeBooleanHelper(@event.Value);
			break;
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
			value = DeserializeIntegerHelper(typeCode, @event.Value);
			break;
		case TypeCode.Single:
			value = float.Parse(@event.Value, YamlFormatter.NumberFormat);
			break;
		case TypeCode.Double:
			value = double.Parse(@event.Value, YamlFormatter.NumberFormat);
			break;
		case TypeCode.Decimal:
			value = decimal.Parse(@event.Value, YamlFormatter.NumberFormat);
			break;
		case TypeCode.String:
			value = @event.Value;
			break;
		case TypeCode.Char:
			value = @event.Value[0];
			break;
		case TypeCode.DateTime:
			value = DateTime.Parse(@event.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
			break;
		default:
			if (expectedType == typeof(object))
			{
				value = @event.Value;
			}
			else
			{
				value = TypeConverter.ChangeType(@event.Value, expectedType);
			}
			break;
		}
		return true;
	}

	private object DeserializeBooleanHelper(string value)
	{
		bool flag;
		if (Regex.IsMatch(value, "^(true|y|yes|on)$", RegexOptions.IgnoreCase))
		{
			flag = true;
		}
		else
		{
			if (!Regex.IsMatch(value, "^(false|n|no|off)$", RegexOptions.IgnoreCase))
			{
				throw new FormatException("The value \"" + value + "\" is not a valid YAML Boolean");
			}
			flag = false;
		}
		return flag;
	}

	private object DeserializeIntegerHelper(TypeCode typeCode, string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		bool flag = false;
		ulong num = 0uL;
		if (value[0] == '-')
		{
			i++;
			flag = true;
		}
		else if (value[0] == '+')
		{
			i++;
		}
		if (value[i] == '0')
		{
			int num2;
			if (i == value.Length - 1)
			{
				num2 = 10;
				num = 0uL;
			}
			else
			{
				i++;
				if (value[i] == 'b')
				{
					num2 = 2;
					i++;
				}
				else if (value[i] == 'x')
				{
					num2 = 16;
					i++;
				}
				else
				{
					num2 = 8;
				}
			}
			for (; i < value.Length; i++)
			{
				if (value[i] != '_')
				{
					stringBuilder.Append(value[i]);
				}
			}
			switch (num2)
			{
			case 2:
			case 8:
				num = Convert.ToUInt64(stringBuilder.ToString(), num2);
				break;
			case 16:
				num = ulong.Parse(stringBuilder.ToString(), NumberStyles.HexNumber, YamlFormatter.NumberFormat);
				break;
			}
		}
		else
		{
			string[] array = value.Substring(i).Split(new char[1] { ':' });
			num = 0uL;
			for (int j = 0; j < array.Length; j++)
			{
				num *= 60;
				num += ulong.Parse(array[j].Replace("_", ""));
			}
		}
		if (flag)
		{
			return CastInteger(checked(-(long)num), typeCode);
		}
		return CastInteger(num, typeCode);
	}

	private static object CastInteger(long number, TypeCode typeCode)
	{
		return checked(typeCode switch
		{
			TypeCode.Byte => (byte)number, 
			TypeCode.Int16 => (short)number, 
			TypeCode.Int32 => (int)number, 
			TypeCode.Int64 => number, 
			TypeCode.SByte => (sbyte)number, 
			TypeCode.UInt16 => (ushort)number, 
			TypeCode.UInt32 => (uint)number, 
			TypeCode.UInt64 => (ulong)number, 
			_ => number, 
		});
	}

	private static object CastInteger(ulong number, TypeCode typeCode)
	{
		return checked(typeCode switch
		{
			TypeCode.Byte => (byte)number, 
			TypeCode.Int16 => (short)number, 
			TypeCode.Int32 => (int)number, 
			TypeCode.Int64 => (long)number, 
			TypeCode.SByte => (sbyte)number, 
			TypeCode.UInt16 => (ushort)number, 
			TypeCode.UInt32 => (uint)number, 
			TypeCode.UInt64 => number, 
			_ => number, 
		});
	}
}
