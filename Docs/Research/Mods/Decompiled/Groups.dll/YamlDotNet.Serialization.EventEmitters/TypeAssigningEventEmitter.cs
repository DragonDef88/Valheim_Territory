using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Schemas;

namespace YamlDotNet.Serialization.EventEmitters;

internal sealed class TypeAssigningEventEmitter : ChainedEventEmitter
{
	private readonly bool requireTagWhenStaticAndActualTypesAreDifferent;

	private readonly IDictionary<Type, TagName> tagMappings;

	public TypeAssigningEventEmitter(IEventEmitter nextEmitter, bool requireTagWhenStaticAndActualTypesAreDifferent, IDictionary<Type, TagName> tagMappings)
		: base(nextEmitter)
	{
		this.requireTagWhenStaticAndActualTypesAreDifferent = requireTagWhenStaticAndActualTypesAreDifferent;
		this.tagMappings = tagMappings ?? throw new ArgumentNullException("tagMappings");
	}

	public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
	{
		ScalarStyle style = ScalarStyle.Plain;
		object value = eventInfo.Source.Value;
		if (value == null)
		{
			eventInfo.Tag = JsonSchema.Tags.Null;
			eventInfo.RenderedValue = "";
		}
		else
		{
			TypeCode typeCode = eventInfo.Source.Type.GetTypeCode();
			switch (typeCode)
			{
			case TypeCode.Boolean:
				eventInfo.Tag = JsonSchema.Tags.Bool;
				eventInfo.RenderedValue = YamlFormatter.FormatBoolean(value);
				break;
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
				eventInfo.Tag = JsonSchema.Tags.Int;
				eventInfo.RenderedValue = YamlFormatter.FormatNumber(value);
				break;
			case TypeCode.Single:
				eventInfo.Tag = JsonSchema.Tags.Float;
				eventInfo.RenderedValue = YamlFormatter.FormatNumber((float)value);
				break;
			case TypeCode.Double:
				eventInfo.Tag = JsonSchema.Tags.Float;
				eventInfo.RenderedValue = YamlFormatter.FormatNumber((double)value);
				break;
			case TypeCode.Decimal:
				eventInfo.Tag = JsonSchema.Tags.Float;
				eventInfo.RenderedValue = YamlFormatter.FormatNumber(value);
				break;
			case TypeCode.Char:
			case TypeCode.String:
				eventInfo.Tag = FailsafeSchema.Tags.Str;
				eventInfo.RenderedValue = value.ToString();
				style = ScalarStyle.Any;
				break;
			case TypeCode.DateTime:
				eventInfo.Tag = DefaultSchema.Tags.Timestamp;
				eventInfo.RenderedValue = YamlFormatter.FormatDateTime(value);
				break;
			case TypeCode.Empty:
				eventInfo.Tag = JsonSchema.Tags.Null;
				eventInfo.RenderedValue = "";
				break;
			default:
				if (eventInfo.Source.Type == typeof(TimeSpan))
				{
					eventInfo.RenderedValue = YamlFormatter.FormatTimeSpan(value);
					break;
				}
				throw new NotSupportedException($"TypeCode.{typeCode} is not supported.");
			}
		}
		eventInfo.IsPlainImplicit = true;
		if (eventInfo.Style == ScalarStyle.Any)
		{
			eventInfo.Style = style;
		}
		base.Emit(eventInfo, emitter);
	}

	public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
	{
		AssignTypeIfNeeded(eventInfo);
		base.Emit(eventInfo, emitter);
	}

	public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
	{
		AssignTypeIfNeeded(eventInfo);
		base.Emit(eventInfo, emitter);
	}

	private void AssignTypeIfNeeded(ObjectEventInfo eventInfo)
	{
		if (tagMappings.TryGetValue(eventInfo.Source.Type, out var value))
		{
			eventInfo.Tag = value;
		}
		else if (requireTagWhenStaticAndActualTypesAreDifferent && eventInfo.Source.Value != null && eventInfo.Source.Type != eventInfo.Source.StaticType)
		{
			throw new YamlException("Cannot serialize type '" + eventInfo.Source.Type.FullName + "' where a '" + eventInfo.Source.StaticType.FullName + "' was expected because no tag mapping has been registered for '" + eventInfo.Source.Type.FullName + "', which means that it won't be possible to deserialize the document.\nRegister a tag mapping using the SerializerBuilder.WithTagMapping method.\n\nE.g: builder.WithTagMapping(\"!" + eventInfo.Source.Type.Name + "\", typeof(" + eventInfo.Source.Type.FullName + "));");
		}
	}
}
