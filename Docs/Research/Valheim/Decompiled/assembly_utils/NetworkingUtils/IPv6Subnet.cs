using System;

namespace NetworkingUtils;

public struct IPv6Subnet
{
	public readonly IPv6Address m_prefix;

	public readonly ulong m_subnetMaskV0;

	public readonly ulong m_subnetMaskV1;

	public IPv6Subnet(IPv6Address prefix, byte prefixLength)
	{
		if (prefixLength > 128)
		{
			throw new ArgumentException($"Prefix length was {prefixLength} but it can't be larger than 128!");
		}
		m_prefix = prefix;
		if (prefixLength <= 64)
		{
			m_subnetMaskV1 = (ulong)(((1L << 64 - prefixLength) - 1) ^ -1);
			m_subnetMaskV0 = 0uL;
		}
		else
		{
			m_subnetMaskV1 = ulong.MaxValue;
			m_subnetMaskV0 = (ulong)(((1L << 64 - (prefixLength - 64)) - 1) ^ -1);
		}
		if ((m_prefix.V0 & m_subnetMaskV0) != m_prefix.V0 || (m_prefix.V1 & m_subnetMaskV1) != m_prefix.V1)
		{
			throw new ArgumentException($"Subnet prefix {m_prefix} contained bits that should've been zero based on the prefix length of {prefixLength}!");
		}
	}

	public bool Contains(IPv6Address address)
	{
		if ((address.V0 & m_subnetMaskV0) == m_prefix.V0)
		{
			return (address.V1 & m_subnetMaskV1) == m_prefix.V1;
		}
		return false;
	}
}
