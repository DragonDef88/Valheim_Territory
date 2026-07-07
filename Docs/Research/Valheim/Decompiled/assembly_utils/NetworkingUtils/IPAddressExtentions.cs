namespace NetworkingUtils;

public static class IPAddressExtentions
{
	public static bool IsIPv4(this IPv6Address address)
	{
		return address.AddressRange == IPv6AddressRange.IPv4Mapped;
	}

	public static bool IsPublic(this IPv6Address address)
	{
		if (address.AddressRange != IPv6AddressRange.Public)
		{
			if (address.IsIPv4())
			{
				return address.IPv4.IsPublic();
			}
			return false;
		}
		return true;
	}

	public static bool IsPublic(this IPv4Address address)
	{
		return address.AddressRange == IPv4AddressRange.Public;
	}
}
