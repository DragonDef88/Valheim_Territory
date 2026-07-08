using System;
using System.Globalization;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.Converters;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDateTimeConverter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter
{
	private readonly DateTimeKind kind;

	private readonly IFormatProvider provider;

	private readonly bool doubleQuotes;

	private readonly string[] formats;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDateTimeConverter(DateTimeKind kind = DateTimeKind.Utc, IFormatProvider? provider = null, bool doubleQuotes = false, params string[] formats)
	{
		this.kind = ((kind == DateTimeKind.Unspecified) ? DateTimeKind.Utc : kind);
		this.provider = provider ?? CultureInfo.InvariantCulture;
		this.doubleQuotes = doubleQuotes;
		this.formats = formats.DefaultIfEmpty("G").ToArray();
	}

	public bool Accepts(Type type)
	{
		return type == typeof(DateTime);
	}

	public object ReadYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		string value = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar>().Value;
		DateTimeStyles style = ((kind == DateTimeKind.Local) ? DateTimeStyles.AssumeLocal : DateTimeStyles.AssumeUniversal);
		DateTime dt = DateTime.ParseExact(value, formats, provider, style);
		dt = EnsureDateTimeKind(dt, kind);
		return dt;
	}

	public void WriteYaml(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, object? value, Type type, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		DateTime dateTime = (DateTime)value;
		string value2 = ((kind == DateTimeKind.Local) ? dateTime.ToLocalTime() : dateTime.ToUniversalTime()).ToString(formats.First(), provider);
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName.Empty, value2, doubleQuotes ? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any, isPlainImplicit: true, isQuotedImplicit: false));
	}

	private static DateTime EnsureDateTimeKind(DateTime dt, DateTimeKind kind)
	{
		if (dt.Kind == DateTimeKind.Local && kind == DateTimeKind.Utc)
		{
			return dt.ToUniversalTime();
		}
		if (dt.Kind == DateTimeKind.Utc && kind == DateTimeKind.Local)
		{
			return dt.ToLocalTime();
		}
		return dt;
	}
}
