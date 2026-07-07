using System;

namespace NetworkingUtils;

public struct IPv4Address : IEquatable<IPv4Address>
{
	private const int c_MaxCharsPerSequence = 3;

	private const int c_SequenceCount = 4;

	private const char c_SequenceSeparator = '.';

	public readonly uint m_value;

	public IPv4AddressRange AddressRange
	{
		get
		{
			if (new IPv4Subnet(new IPv4Address(0, 0, 0, 0), 8).Contains(this))
			{
				return IPv4AddressRange.LocalNetwork;
			}
			if (new IPv4Subnet(new IPv4Address(10, 0, 0, 0), 8).Contains(this))
			{
				return IPv4AddressRange.Private;
			}
			if (new IPv4Subnet(new IPv4Address(100, 64, 0, 0), 10).Contains(this))
			{
				return IPv4AddressRange.GCNat;
			}
			if (new IPv4Subnet(new IPv4Address(127, 0, 0, 0), 8).Contains(this))
			{
				return IPv4AddressRange.LocalHostLoopback;
			}
			if (new IPv4Subnet(new IPv4Address(169, 254, 0, 0), 16).Contains(this))
			{
				return IPv4AddressRange.LinkLocal;
			}
			if (new IPv4Subnet(new IPv4Address(172, 16, 0, 0), 12).Contains(this))
			{
				return IPv4AddressRange.Private;
			}
			if (new IPv4Subnet(new IPv4Address(192, 0, 0, 0), 24).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(192, 0, 2, 0), 24).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(192, 88, 99, 0), 24).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(192, 168, 0, 0), 16).Contains(this))
			{
				return IPv4AddressRange.Private;
			}
			if (new IPv4Subnet(new IPv4Address(198, 18, 0, 0), 15).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(198, 51, 100, 0), 24).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(203, 0, 113, 0), 24).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(224, 0, 0, 0), 4).Contains(this))
			{
				return IPv4AddressRange.Multicast;
			}
			if (new IPv4Subnet(new IPv4Address(233, 252, 0, 0), 24).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(240, 0, 0, 0), 4).Contains(this))
			{
				return IPv4AddressRange.Reserved;
			}
			if (new IPv4Subnet(new IPv4Address(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 32).Contains(this))
			{
				return IPv4AddressRange.Broadcast;
			}
			return IPv4AddressRange.Public;
		}
	}

	public IPv4Address(uint value)
	{
		m_value = value;
	}

	public IPv4Address(byte v3, byte v2, byte v1, byte v0)
	{
		m_value = (uint)((v3 << 24) | (v2 << 16) | (v1 << 8) | v0);
	}

	public IPv4Address(Span<byte> value)
	{
		if (value.Length != 4)
		{
			throw new ArgumentException($"The byte array had {value.Length} entries but must have exactly 4.");
		}
		m_value = (uint)((value[0] << 24) | (value[1] << 16) | (value[2] << 8) | value[3]);
	}

	public static bool TryParse(ReadOnlySpan<char> stringRepresentation, out IPv4Address result)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		uint num = 0u;
		byte b = default(byte);
		for (int i = 0; i < 4; i++)
		{
			int num2 = ((i < 3) ? MemoryExtensions.IndexOf<char>(stringRepresentation, '.') : stringRepresentation.Length);
			if (num2 < 0)
			{
				result = default(IPv4Address);
				return false;
			}
			if (!byte.TryParse(stringRepresentation.Slice(0, num2), ref b))
			{
				result = default(IPv4Address);
				return false;
			}
			num <<= 8;
			num += b;
			if (i < 3)
			{
				stringRepresentation = stringRepresentation.Slice(num2 + 1);
			}
		}
		result = new IPv4Address(num);
		return true;
	}

	public unsafe override string ToString()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Span<char> val = new Span<char>((void*)stackalloc byte[30], 15);
		int num = 0;
		int num4 = default(int);
		for (int num2 = 3; num2 >= 0; num2--)
		{
			int num3 = num2 * 8;
			byte b = (byte)(((255 << num3) & m_value) >> num3);
			if (!b.TryFormat(val.Slice(num), ref num4, default(ReadOnlySpan<char>), (IFormatProvider)null) || num4 > 3)
			{
				throw new InvalidOperationException($"Failed to format! Span is {((object)val).ToString()}, length is {num}, value to format was {b}");
			}
			num += num4;
			if (num2 > 0)
			{
				val[num++] = '.';
			}
		}
		return new string(Span<char>.op_Implicit(val.Slice(0, num)));
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is IPv4Address other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(IPv4Address other)
	{
		return m_value == other.m_value;
	}

	public override int GetHashCode()
	{
		uint value = m_value;
		return value.GetHashCode();
	}

	public static bool operator ==(IPv4Address lhs, IPv4Address rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(IPv4Address lhs, IPv4Address rhs)
	{
		return !(lhs == rhs);
	}
}
