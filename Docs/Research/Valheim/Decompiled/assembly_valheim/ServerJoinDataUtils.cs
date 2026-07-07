using System;
using NetworkingUtils;

public static class ServerJoinDataUtils
{
	public static void GetAddressAndPortFromString(string address, out string ipAddress, out ushort foundPort)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(address))
		{
			ipAddress = string.Empty;
			foundPort = 0;
			return;
		}
		int num = address.LastIndexOf(":");
		int num2 = ((num >= 0) ? num : address.Length);
		if (num < 0 || !ushort.TryParse(MemoryExtensions.AsSpan(address, num + 1), ref foundPort))
		{
			foundPort = 0;
			num2 = address.Length;
		}
		IPv6Address val = default(IPv6Address);
		IPv4Address val2 = default(IPv4Address);
		IPv6Address val3 = default(IPv6Address);
		if (address[0] == '[' && address[num2 - 1] == ']' && IPv6Address.TryParse(MemoryExtensions.AsSpan(address, 1, num2 - 2), ref val, false))
		{
			if ((int)((IPv6Address)(ref val)).AddressRange == 0)
			{
				ipAddress = string.Empty;
				foundPort = 0;
			}
			else
			{
				ipAddress = ((object)(IPv6Address)(ref val)).ToString();
			}
		}
		else if (IPv4Address.TryParse(MemoryExtensions.AsSpan(address, 0, num2), ref val2))
		{
			ipAddress = ((object)(IPv4Address)(ref val2)).ToString();
		}
		else if (IPv6Address.TryParse(string.op_Implicit(address), ref val3, false))
		{
			if ((int)((IPv6Address)(ref val3)).AddressRange == 0)
			{
				ipAddress = string.Empty;
			}
			else
			{
				ipAddress = ((object)(IPv6Address)(ref val3)).ToString();
			}
			foundPort = 0;
		}
		else
		{
			ipAddress = address.Substring(0, num2);
		}
	}
}
