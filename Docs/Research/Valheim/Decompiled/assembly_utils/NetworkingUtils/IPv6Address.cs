using System;
using System.Globalization;

namespace NetworkingUtils;

public struct IPv6Address : IEquatable<IPv6Address>
{
	private const int c_MaxCharsPerSequence = 4;

	private const int c_SequenceCount = 8;

	private const char c_SequenceSeparator = ':';

	public readonly ushort v0;

	public readonly ushort v1;

	public readonly ushort v2;

	public readonly ushort v3;

	public readonly ushort v4;

	public readonly ushort v5;

	public readonly ushort v6;

	public readonly ushort v7;

	public ulong V0 => ((ulong)v3 << 48) | ((ulong)v2 << 32) | ((ulong)v1 << 16) | v0;

	public ulong V1 => ((ulong)v7 << 48) | ((ulong)v6 << 32) | ((ulong)v5 << 16) | v4;

	public IPv4Address IPv4
	{
		get
		{
			if (AddressRange != IPv6AddressRange.IPv4Mapped)
			{
				throw new InvalidOperationException("The IP address " + ToString() + " is not an IPv4 address!");
			}
			return new IPv4Address((uint)((v1 << 16) | v0));
		}
	}

	public IPv6AddressRange AddressRange
	{
		get
		{
			if (new IPv6Subnet(new IPv6Address(0, 0, 0, 0, 0, 0, 0, 0), 128).Contains(this))
			{
				return IPv6AddressRange.Unspecified;
			}
			if (new IPv6Subnet(new IPv6Address(0, 0, 0, 0, 0, 0, 0, 1), 128).Contains(this))
			{
				return IPv6AddressRange.LocalHostLoopback;
			}
			if (new IPv6Subnet(new IPv6Address(0, 0, 0, 0, 0, ushort.MaxValue, 0, 0), 96).Contains(this))
			{
				return IPv6AddressRange.IPv4Mapped;
			}
			if (new IPv6Subnet(new IPv6Address(0, 0, 0, 0, ushort.MaxValue, 0, 0, 0), 96).Contains(this))
			{
				return IPv6AddressRange.IPv4Translated;
			}
			if (new IPv6Subnet(new IPv6Address(100, 65435, 0, 0, 0, 0, 0, 0), 96).Contains(this))
			{
				return IPv6AddressRange.IPv4IPv6Translation;
			}
			if (new IPv6Subnet(new IPv6Address(100, 65435, 1, 0, 0, 0, 0, 0), 48).Contains(this))
			{
				return IPv6AddressRange.IPv4IPv6Translation;
			}
			if (new IPv6Subnet(new IPv6Address(256, 0, 0, 0, 0, 0, 0, 0), 64).Contains(this))
			{
				return IPv6AddressRange.DiscardPrefix;
			}
			if (new IPv6Subnet(new IPv6Address(8193, 0, 0, 0, 0, 0, 0, 0), 32).Contains(this))
			{
				return IPv6AddressRange.TeredoTunneling;
			}
			if (new IPv6Subnet(new IPv6Address(8193, 32, 0, 0, 0, 0, 0, 0), 28).Contains(this))
			{
				return IPv6AddressRange.ORCHIDv2;
			}
			if (new IPv6Subnet(new IPv6Address(8193, 3512, 0, 0, 0, 0, 0, 0), 32).Contains(this))
			{
				return IPv6AddressRange.Reserved;
			}
			if (new IPv6Subnet(new IPv6Address(8194, 0, 0, 0, 0, 0, 0, 0), 16).Contains(this))
			{
				return IPv6AddressRange.SixToFour;
			}
			if (new IPv6Subnet(new IPv6Address(16383, 0, 0, 0, 0, 0, 0, 0), 20).Contains(this))
			{
				return IPv6AddressRange.Reserved;
			}
			if (new IPv6Subnet(new IPv6Address(24320, 0, 0, 0, 0, 0, 0, 0), 16).Contains(this))
			{
				return IPv6AddressRange.SegmentRouting;
			}
			if (new IPv6Subnet(new IPv6Address(64512, 0, 0, 0, 0, 0, 0, 0), 7).Contains(this))
			{
				return IPv6AddressRange.UniqueLocal;
			}
			if (new IPv6Subnet(new IPv6Address(65152, 0, 0, 0, 0, 0, 0, 0), 64).Contains(this))
			{
				return IPv6AddressRange.LinkLocal;
			}
			if (new IPv6Subnet(new IPv6Address(65152, 0, 0, 0, 0, 0, 0, 0), 10).Contains(this))
			{
				return IPv6AddressRange.Reserved;
			}
			if (new IPv6Subnet(new IPv6Address(65280, 0, 0, 0, 0, 0, 0, 0), 8).Contains(this))
			{
				return IPv6AddressRange.Multicast;
			}
			return IPv6AddressRange.Public;
		}
	}

	public IPv6Address(IPv4Address address)
	{
		v7 = 0;
		v6 = 0;
		v5 = 0;
		v4 = 0;
		v3 = 0;
		v2 = ushort.MaxValue;
		v1 = (ushort)((address.m_value >> 16) & 0xFFFFu);
		v0 = (ushort)(address.m_value & 0xFFFFu);
	}

	public IPv6Address(ushort v7, ushort v6, ushort v5, ushort v4, ushort v3, ushort v2, ushort v1, ushort v0)
	{
		this.v0 = v0;
		this.v1 = v1;
		this.v2 = v2;
		this.v3 = v3;
		this.v4 = v4;
		this.v5 = v5;
		this.v6 = v6;
		this.v7 = v7;
	}

	public IPv6Address(Span<byte> value)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (value.Length != 16)
		{
			throw new ArgumentException($"The byte array had {value.Length} entries but must have exactly 16.");
		}
		v7 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(0, 2)));
		v6 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(2, 2)));
		v5 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(4, 2)));
		v4 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(6, 2)));
		v3 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(8, 2)));
		v2 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(10, 2)));
		v1 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(12, 2)));
		v0 = BitConverter.ToUInt16(Span<byte>.op_Implicit(value.Slice(14, 2)));
	}

	public unsafe static bool TryParse(ReadOnlySpan<char> stringRepresentation, out IPv6Address result, bool allowIPv4 = true)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		if (allowIPv4 && IPv4Address.TryParse(stringRepresentation, out var result2))
		{
			result = result2;
			return true;
		}
		Span<ushort> val = new Span<ushort>((void*)stackalloc byte[16], 8);
		int num = 0;
		sbyte b = sbyte.MinValue;
		bool flag = false;
		sbyte b2 = 0;
		ushort num4 = default(ushort);
		while (b2 < val.Length && num < stringRepresentation.Length)
		{
			if (flag)
			{
				if (*(ushort*)stringRepresentation[num] != 58)
				{
					result = default(IPv6Address);
					return false;
				}
				flag = false;
				num++;
				if (num >= stringRepresentation.Length)
				{
					result = default(IPv6Address);
					return false;
				}
			}
			if (*(ushort*)stringRepresentation[num] == 58)
			{
				if (b2 == 0)
				{
					if (stringRepresentation.Length <= num + 1 || *(ushort*)stringRepresentation[num + 1] != 58)
					{
						result = default(IPv6Address);
						return false;
					}
					num++;
				}
				else if (b >= 0)
				{
					result = default(IPv6Address);
					return false;
				}
				num++;
				b = b2;
			}
			else
			{
				int num3;
				if (b2 < val.Length - 1)
				{
					int num2 = MemoryExtensions.IndexOf<char>(stringRepresentation.Slice(num), ':');
					if (num2 < 0)
					{
						if (b < 0)
						{
							result = default(IPv6Address);
							return false;
						}
						num3 = stringRepresentation.Length;
					}
					else
					{
						num3 = num + num2;
					}
				}
				else
				{
					num3 = stringRepresentation.Length;
					if (num3 - num <= 0)
					{
						result = default(IPv6Address);
						return false;
					}
				}
				if (!ushort.TryParse(stringRepresentation.Slice(num, num3 - num), NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture, ref num4))
				{
					result = default(IPv6Address);
					return false;
				}
				val[(int)b2] = num4;
				num = num3;
				flag = true;
			}
			b2++;
		}
		if (b >= 0)
		{
			int num5 = val.Length - b2;
			if (num5 > 0)
			{
				for (int num6 = val.Length - num5 - 1; num6 >= b + 1; num6--)
				{
					val[num6 + num5] = val[num6];
					val[num6] = 0;
				}
			}
		}
		result = new IPv6Address(val[0], val[1], val[2], val[3], val[4], val[5], val[6], val[7]);
		return true;
	}

	public override string ToString()
	{
		if (AddressRange == IPv6AddressRange.IPv4Mapped)
		{
			return IPv4.ToString();
		}
		return ToIPv6String();
	}

	public unsafe string ToIPv6String()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		Span<ushort> val = new Span<ushort>((void*)stackalloc byte[16], 8);
		val[7] = v0;
		val[6] = v1;
		val[5] = v2;
		val[4] = v3;
		val[3] = v4;
		val[2] = v5;
		val[1] = v6;
		val[0] = v7;
		sbyte b = -1;
		byte b2 = 0;
		bool flag = false;
		sbyte b3 = -1;
		for (byte b4 = 0; b4 < val.Length; b4++)
		{
			bool flag2 = val[(int)b4] == 0;
			if (flag2 != flag)
			{
				if (flag2)
				{
					b3 = (sbyte)b4;
				}
				else
				{
					byte b5 = (byte)(b4 - b3);
					if (b5 > b2)
					{
						b2 = b5;
						b = b3;
					}
				}
				flag = flag2;
			}
		}
		if (flag)
		{
			byte b6 = (byte)(val.Length - b3);
			if (b6 > b2)
			{
				b2 = b6;
				b = b3;
			}
		}
		Span<char> val2 = new Span<char>((void*)stackalloc byte[78], 39);
		int num = 0;
		int num2 = default(int);
		for (byte b7 = 0; b7 < val.Length; b7++)
		{
			if (b7 != 0)
			{
				val2[num++] = ':';
			}
			if (b >= 0 && b7 == b)
			{
				if (b7 == 0)
				{
					val2[num++] = ':';
				}
				b7 += (byte)(b2 - 1);
				if (b7 == val.Length - 1)
				{
					val2[num++] = ':';
				}
			}
			else
			{
				if (!val[(int)b7].TryFormat(val2.Slice(num), ref num2, string.op_Implicit("x"), (IFormatProvider)null) || num2 > 4)
				{
					throw new InvalidOperationException(string.Format("Failed to format! Span is {0}, length is {1}, value to format was {2}", ((object)val2).ToString(), num, val[(int)b7].ToString("x")));
				}
				num += num2;
			}
		}
		return new string(Span<char>.op_Implicit(val2.Slice(0, num)));
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is IPv6Address other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(IPv6Address other)
	{
		if (V0 == other.V0)
		{
			return V1 == other.V1;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<ulong, ulong>(V0, V1);
	}

	public static bool operator ==(IPv6Address lhs, IPv6Address rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(IPv6Address lhs, IPv6Address rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator IPv6Address(IPv4Address address)
	{
		return new IPv6Address(address);
	}
}
