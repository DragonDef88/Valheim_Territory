using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute : Attribute
{
	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling? defaultValuesHandling;

	public string? Description { get; set; }

	public Type? SerializeAs { get; set; }

	public int Order { get; set; }

	public string? Alias { get; set; }

	public bool ApplyNamingConventions { get; set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle ScalarStyle { get; set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultValuesHandling DefaultValuesHandling
	{
		get
		{
			return defaultValuesHandling.GetValueOrDefault();
		}
		set
		{
			defaultValuesHandling = value;
		}
	}

	public bool IsDefaultValuesHandlingSpecified => defaultValuesHandling.HasValue;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute()
	{
		ScalarStyle = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any;
		ApplyNamingConventions = true;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMemberAttribute(Type serializeAs)
		: this()
	{
		SerializeAs = serializeAs ?? throw new ArgumentNullException("serializeAs");
	}
}
