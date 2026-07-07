using System;

namespace NetworkingUtils;

public struct IPEndPoint : IEquatable<IPEndPoint>
{
	public readonly IPv6Address m_address;

	public readonly ushort m_port;

	public bool IsPublic => m_address.IsPublic();

	public IPEndPoint(IPv6Address address, ushort port)
	{
		m_address = address;
		m_port = port;
	}

	public override string ToString()
	{
		if (m_address.AddressRange == IPv6AddressRange.IPv4Mapped)
		{
			return $"{m_address.IPv4.ToString()}:{m_port}";
		}
		return $"[{m_address.ToString()}]:{m_port}";
	}

	public string ToIPv6String()
	{
		return $"[{m_address.ToIPv6String()}]:{m_port}";
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is IPEndPoint other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(IPEndPoint other)
	{
		if (m_address == other.m_address)
		{
			return m_port == other.m_port;
		}
		return false;
	}

	public static bool operator ==(IPEndPoint lhs, IPEndPoint rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(IPEndPoint lhs, IPEndPoint rhs)
	{
		return !(lhs == rhs);
	}
}
