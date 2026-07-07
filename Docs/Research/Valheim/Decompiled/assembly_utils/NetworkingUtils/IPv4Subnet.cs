using System;

namespace NetworkingUtils;

public struct IPv4Subnet
{
	public readonly IPv4Address m_prefix;

	public readonly uint m_subnetMask;

	public IPv4Subnet(IPv4Address prefix, byte prefixLength)
	{
		if (prefixLength > 32)
		{
			throw new ArgumentException($"Prefix length was {prefixLength} but it can't be larger than 32!");
		}
		m_prefix = prefix;
		m_subnetMask = (uint)((1 << 32 - prefixLength) - 1) ^ 0xFFFFFFFFu;
		if ((m_prefix.m_value & m_subnetMask) != m_prefix.m_value)
		{
			throw new ArgumentException($"Subnet prefix {m_prefix} contained bits that should've been zero based on the prefix length of {prefixLength}!");
		}
	}

	public bool Contains(IPv4Address address)
	{
		return (address.m_value & m_subnetMask) == m_prefix.m_value;
	}
}
