using System;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.EventEmitters;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonEventEmitter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EChainedEventEmitter
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlFormatter formatter;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonEventEmitter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEventEmitter nextEmitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlFormatter formatter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector)
		: base(nextEmitter)
	{
		this.formatter = formatter;
		this.enumNamingConvention = enumNamingConvention;
		this.typeInspector = typeInspector;
	}

	public override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		eventInfo.NeedsExpansion = true;
	}

	public override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		eventInfo.IsPlainImplicit = true;
		eventInfo.Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain;
		object value = eventInfo.Source.Value;
		if (value == null)
		{
			eventInfo.RenderedValue = "null";
		}
		else
		{
			TypeCode typeCode = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.GetTypeCode(eventInfo.Source.Type);
			switch (typeCode)
			{
			case TypeCode.Boolean:
				eventInfo.RenderedValue = formatter.FormatBoolean(value);
				break;
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
				if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.IsEnum(eventInfo.Source.Type))
				{
					eventInfo.RenderedValue = formatter.FormatEnum(value, typeInspector, enumNamingConvention);
					eventInfo.Style = ((!formatter.PotentiallyQuoteEnums(value)) ? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted);
				}
				else
				{
					eventInfo.RenderedValue = formatter.FormatNumber(value);
				}
				break;
			case TypeCode.Single:
			{
				float f = (float)value;
				eventInfo.RenderedValue = f.ToString("G", CultureInfo.InvariantCulture);
				if (float.IsNaN(f) || float.IsInfinity(f))
				{
					eventInfo.Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted;
				}
				break;
			}
			case TypeCode.Double:
			{
				double d = (double)value;
				eventInfo.RenderedValue = d.ToString("G", CultureInfo.InvariantCulture);
				if (double.IsNaN(d) || double.IsInfinity(d))
				{
					eventInfo.Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted;
				}
				break;
			}
			case TypeCode.Decimal:
				eventInfo.RenderedValue = ((decimal)value).ToString(CultureInfo.InvariantCulture);
				break;
			case TypeCode.Char:
			case TypeCode.String:
				eventInfo.RenderedValue = value.ToString();
				eventInfo.Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted;
				break;
			case TypeCode.DateTime:
				eventInfo.RenderedValue = formatter.FormatDateTime(value);
				break;
			case TypeCode.Empty:
				eventInfo.RenderedValue = "null";
				break;
			default:
				if (eventInfo.Source.Type == typeof(TimeSpan))
				{
					eventInfo.RenderedValue = formatter.FormatTimeSpan(value);
					break;
				}
				throw new NotSupportedException($"TypeCode.{typeCode} is not supported.");
			}
		}
		base.Emit(eventInfo, emitter);
	}

	public override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStartEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		eventInfo.Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle.Flow;
		base.Emit(eventInfo, emitter);
	}

	public override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStartEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		eventInfo.Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStyle.Flow;
		base.Emit(eventInfo, emitter);
	}
}
