using System;
using System.Globalization;

namespace YamlDotNet;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECultureInfoAdapter : CultureInfo
{
	private readonly IFormatProvider provider;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECultureInfoAdapter(CultureInfo baseCulture, IFormatProvider provider)
		: base(baseCulture.Name)
	{
		this.provider = provider;
	}

	public override object? GetFormat(Type formatType)
	{
		return provider.GetFormat(formatType);
	}
}
