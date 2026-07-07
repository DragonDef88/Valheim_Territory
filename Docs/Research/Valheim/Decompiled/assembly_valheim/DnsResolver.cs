using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NetworkingUtils;

public class DnsResolver
{
	private readonly Dictionary<string, IPv6Address?> m_dnsResolveCache = new Dictionary<string, IPv6Address?>();

	private readonly Dictionary<string, DnsResolveRequest> m_dnsResolveRequests = new Dictionary<string, DnsResolveRequest>();

	public bool ResolveDomainName(string domainName, out IPv6Address? address, DnsResolveFlags flags = DnsResolveFlags.None)
	{
		if (!flags.HasFlag(DnsResolveFlags.DontCheckCache) && m_dnsResolveCache.TryGetValue(domainName, out address))
		{
			return true;
		}
		if (flags.HasFlag(DnsResolveFlags.CacheOnly))
		{
			address = null;
			return false;
		}
		if (!URLToIP(domainName, out address))
		{
			m_dnsResolveCache.Add(domainName, null);
			return false;
		}
		SetCacheEntry(domainName, address);
		return true;
	}

	public void ResolveDomainNameAsync(string domainName, ResolveDomainCompletedHandler completedCallback, DnsResolveFlags flags = DnsResolveFlags.None)
	{
		DnsResolveRequest value2;
		if (!flags.HasFlag(DnsResolveFlags.DontCheckCache) && m_dnsResolveCache.TryGetValue(domainName, out var value))
		{
			completedCallback?.Invoke(succeeded: true, value);
		}
		else if (flags.HasFlag(DnsResolveFlags.CacheOnly))
		{
			completedCallback?.Invoke(succeeded: false, null);
		}
		else if (!m_dnsResolveRequests.TryGetValue(domainName, out value2))
		{
			value2 = new DnsResolveRequest(domainName, OnResolveDomainNameAsyncCompleted);
			m_dnsResolveRequests.Add(domainName, value2);
			value2.Completed += completedCallback;
			value2.RunAsync();
		}
		else
		{
			value2.Completed += completedCallback;
		}
	}

	public void ClearCache()
	{
		m_dnsResolveCache.Clear();
	}

	private void OnResolveDomainNameAsyncCompleted(DnsResolveRequest request)
	{
		m_dnsResolveRequests.Remove(request.m_domainName);
		SetCacheEntry(request.m_domainName, request.Address);
	}

	private void SetCacheEntry(string domainName, IPv6Address? address)
	{
		m_dnsResolveCache.Remove(domainName);
		m_dnsResolveCache.Add(domainName, address);
	}

	public static bool URLToIP(string url, out IPv6Address? ip)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		ip = null;
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(url);
			if (hostAddresses.Length == 0)
			{
				return false;
			}
			ZLog.Log((object)("Got dns entries: " + hostAddresses.Length));
			IPAddress[] array = hostAddresses;
			IPv6Address? val;
			foreach (IPAddress iPAddress in array)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					ip = IPv6Address.op_Implicit(new IPv4Address(Span<byte>.op_Implicit(iPAddress.GetAddressBytes())));
					return true;
				}
				val = ip;
				if (!val.HasValue && iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					ip = new IPv6Address(Span<byte>.op_Implicit(iPAddress.GetAddressBytes()));
				}
			}
			val = ip;
			return val.HasValue;
		}
		catch (Exception ex)
		{
			ZLog.Log((object)("Exception while finding ip:" + ex.ToString()));
			ip = null;
			return false;
		}
	}
}
