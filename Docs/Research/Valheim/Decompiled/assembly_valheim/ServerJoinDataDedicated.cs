using System;
using NetworkingUtils;

public struct ServerJoinDataDedicated : IEquatable<ServerJoinDataDedicated>
{
	public const string c_TypeName = "Dedicated";

	public readonly ushort m_port;

	private IPv6Address? m_address;

	public string m_host { get; private set; }

	public bool IsURL { get; private set; }

	public ServerJoinDataDedicated(string address)
	{
		m_host = null;
		m_port = 0;
		m_address = null;
		IsURL = false;
		ushort foundPort = 0;
		ServerJoinDataUtils.GetAddressAndPortFromString(address, out var ipAddress, out foundPort);
		if (!string.IsNullOrEmpty(ipAddress))
		{
			SetHost(ipAddress);
			if (foundPort != 0)
			{
				m_port = foundPort;
			}
			else
			{
				m_port = 2456;
			}
		}
	}

	public ServerJoinDataDedicated(string host, ushort port)
	{
		m_host = null;
		m_address = null;
		IsURL = false;
		m_port = port;
		SetHost(host);
	}

	public ServerJoinDataDedicated(uint host, ushort port)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		m_address = IPv6Address.op_Implicit(new IPv4Address(host));
		IPv6Address value = m_address.Value;
		IPv4Address iPv = ((IPv6Address)(ref value)).IPv4;
		m_host = ((object)(IPv4Address)(ref iPv)).ToString();
		m_port = port;
		IsURL = false;
	}

	public ServerJoinDataDedicated(IPEndPoint endPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		m_address = endPoint.m_address;
		IPv6Address address = endPoint.m_address;
		IPv4Address iPv = ((IPv6Address)(ref address)).IPv4;
		m_host = ((object)(IPv4Address)(ref iPv)).ToString();
		m_port = endPoint.m_port;
		IsURL = false;
	}

	public string GetDataName()
	{
		return "Dedicated";
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is ServerJoinDataDedicated other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(ServerJoinDataDedicated other)
	{
		if (m_host == other.m_host)
		{
			return m_port == other.m_port;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = -468063053;
		if (!string.IsNullOrEmpty(m_host))
		{
			num = num * -1521134295 + m_host.GetHashCode();
		}
		else
		{
			ZLog.LogWarning((object)"m_host was null or empty when trying to get hash code!");
		}
		int num2 = num * -1521134295;
		ushort port = m_port;
		return num2 + port.GetHashCode();
	}

	public static bool operator ==(ServerJoinDataDedicated left, ServerJoinDataDedicated right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ServerJoinDataDedicated left, ServerJoinDataDedicated right)
	{
		return !(left == right);
	}

	private void SetHost(string host)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		IPv6Address value = default(IPv6Address);
		if (IPv6Address.TryParse(string.op_Implicit(host), ref value, true))
		{
			m_host = ((object)(IPv6Address)(ref value)).ToString();
			m_address = value;
			return;
		}
		string text = host;
		if (!host.StartsWith("http://") && !host.StartsWith("https://"))
		{
			text = "http://" + host;
		}
		if (!host.EndsWith("/"))
		{
			text += "/";
		}
		if (Uri.TryCreate(text, UriKind.Absolute, out Uri _))
		{
			m_host = host;
			IsURL = true;
		}
		else
		{
			m_host = host;
		}
	}

	public string GetHost()
	{
		return m_host;
	}

	public bool TryGetIPAddress(out IPv6Address address)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		address = (IPv6Address)(m_address.HasValue ? m_address.Value : default(IPv6Address));
		return m_address.HasValue;
	}

	public bool TryGetIPEndPoint(out IPEndPoint endPoint)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		endPoint = (m_address.HasValue ? new IPEndPoint(m_address.Value, m_port) : default(IPEndPoint));
		return m_address.HasValue;
	}

	public override string ToString()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		string host = GetHost();
		int port = m_port;
		if (m_address.HasValue)
		{
			IPv6Address value = m_address.Value;
			if ((int)((IPv6Address)(ref value)).AddressRange != 2)
			{
				return $"[{host}]:{port}";
			}
		}
		return $"{host}:{port}";
	}
}
