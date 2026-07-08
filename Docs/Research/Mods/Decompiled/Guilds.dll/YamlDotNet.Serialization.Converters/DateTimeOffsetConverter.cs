using System;
using System.Globalization;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.Converters;

internal class DateTimeOffsetConverter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter
{
	private readonly IFormatProvider provider;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle style;

	private readonly DateTimeStyles dateStyle;

	private readonly string[] formats;

	public DateTimeOffsetConverter(IFormatProvider? provider = null, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any, DateTimeStyles dateStyle = DateTimeStyles.None, params string[] formats)
	{
		this.provider = provider ?? CultureInfo.InvariantCulture;
		this.style = style;
		this.dateStyle = dateStyle;
		this.formats = formats.DefaultIfEmpty("O").ToArray();
	}

	public bool Accepts(Type type)
	{
		return type == typeof(DateTimeOffset);
	}

	public object ReadYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		string value = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar>().Value;
		DateTimeOffset dateTimeOffset = DateTimeOffset.ParseExact(value, formats, provider, dateStyle);
		return dateTimeOffset;
	}

	public void WriteYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? value, Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		string value2 = ((DateTimeOffset)value).ToString(formats.First(), provider);
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName.Empty, value2, style, isPlainImplicit: true, isQuotedImplicit: false));
	}
}
