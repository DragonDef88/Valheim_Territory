using System.ComponentModel;
using System.Globalization;

namespace Guilds;

public class PlayerReferenceTypeConverter : TypeConverter
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object? value)
	{
		return PlayerReference.fromString(value?.ToString() ?? ":");
	}
}
