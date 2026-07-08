using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Schemas;

namespace YamlDotNet.Serialization.EventEmitters;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeAssigningEventEmitter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EChainedEventEmitter
{
	private readonly IDictionary<Type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName> tagMappings;

	private readonly bool quoteNecessaryStrings;

	private readonly Regex? isSpecialStringValue_Regex;

	private static readonly string SpecialStrings_Pattern = "^(null|Null|NULL|\\~|true|True|TRUE|false|False|FALSE|[-+]?[0-9]+|0o[0-7]+|0x[0-9a-fA-F]+|[-+]?(\\.[0-9]+|[0-9]+(\\.[0-9]*)?)([eE][-+]?[0-9]+)?|[-+]?(\\.inf|\\.Inf|\\.INF)|\\.nan|\\.NaN|\\.NAN|\\s.*)$";

	private static readonly string CombinedYaml1_1SpecialStrings_Pattern = "^(null|Null|NULL|\\~|true|True|TRUE|false|False|FALSE|y|Y|yes|Yes|YES|n|N|no|No|NO|on|On|ON|off|Off|OFF|[-+]?0b[0-1_]+|[-+]?0o?[0-7_]+|[-+]?(0|[1-9][0-9_]*)|[-+]?0x[0-9a-fA-F_]+|[-+]?[1-9][0-9_]*(:[0-5]?[0-9])+|[-+]?([0-9][0-9_]*)?\\.[0-9_]*([eE][-+][0-9]+)?|[-+]?[0-9][0-9_]*(:[0-5]?[0-9])+\\.[0-9_]*|[-+]?\\.(inf|Inf|INF)|\\.(nan|NaN|NAN))$";

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle defaultScalarStyle;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlFormatter formatter;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeAssigningEventEmitter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEventEmitter nextEmitter, IDictionary<Type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName> tagMappings, bool quoteNecessaryStrings, bool quoteYaml1_1Strings, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle defaultScalarStyle, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlFormatter formatter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector)
		: base(nextEmitter)
	{
		this.defaultScalarStyle = defaultScalarStyle;
		this.formatter = formatter;
		this.tagMappings = tagMappings;
		this.quoteNecessaryStrings = quoteNecessaryStrings;
		isSpecialStringValue_Regex = new Regex(quoteYaml1_1Strings ? CombinedYaml1_1SpecialStrings_Pattern : SpecialStrings_Pattern, RegexOptions.Compiled);
		this.enumNamingConvention = enumNamingConvention;
		this.typeInspector = typeInspector;
	}

	public override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain;
		object value = eventInfo.Source.Value;
		if (value == null)
		{
			eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Null;
			eventInfo.RenderedValue = "";
		}
		else
		{
			TypeCode typeCode = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.GetTypeCode(eventInfo.Source.Type);
			switch (typeCode)
			{
			case TypeCode.Boolean:
				eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Bool;
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
				if (eventInfo.Source.Type.IsEnum)
				{
					eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFailsafeSchema.Tags.Str;
					eventInfo.RenderedValue = formatter.FormatEnum(value, typeInspector, enumNamingConvention);
					style = ((!quoteNecessaryStrings || !IsSpecialStringValue(eventInfo.RenderedValue) || !formatter.PotentiallyQuoteEnums(value)) ? defaultScalarStyle : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted);
				}
				else
				{
					eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Int;
					eventInfo.RenderedValue = formatter.FormatNumber(value);
				}
				break;
			case TypeCode.Single:
				eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Float;
				eventInfo.RenderedValue = formatter.FormatNumber((float)value);
				break;
			case TypeCode.Double:
				eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Float;
				eventInfo.RenderedValue = formatter.FormatNumber((double)value);
				break;
			case TypeCode.Decimal:
				eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Float;
				eventInfo.RenderedValue = formatter.FormatNumber(value);
				break;
			case TypeCode.Char:
			case TypeCode.String:
				eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFailsafeSchema.Tags.Str;
				eventInfo.RenderedValue = value.ToString();
				style = ((!quoteNecessaryStrings || !IsSpecialStringValue(eventInfo.RenderedValue)) ? defaultScalarStyle : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted);
				break;
			case TypeCode.DateTime:
				eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultSchema.Tags.Timestamp;
				eventInfo.RenderedValue = formatter.FormatDateTime(value);
				break;
			case TypeCode.Empty:
				eventInfo.Tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Null;
				eventInfo.RenderedValue = "";
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
		eventInfo.IsPlainImplicit = true;
		if (eventInfo.Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any)
		{
			eventInfo.Style = style;
		}
		base.Emit(eventInfo, emitter);
	}

	public override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStartEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		AssignTypeIfNeeded(eventInfo);
		base.Emit(eventInfo, emitter);
	}

	public override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStartEventInfo eventInfo, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter)
	{
		AssignTypeIfNeeded(eventInfo);
		base.Emit(eventInfo, emitter);
	}

	private void AssignTypeIfNeeded(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectEventInfo eventInfo)
	{
		if (tagMappings.TryGetValue(eventInfo.Source.Type, out var value))
		{
			eventInfo.Tag = value;
		}
	}

	private bool IsSpecialStringValue(string value)
	{
		if (value.Trim() == string.Empty)
		{
			return true;
		}
		return isSpecialStringValue_Regex?.IsMatch(value) ?? false;
	}
}
