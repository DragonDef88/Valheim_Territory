using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ZDO : IEquatable<ZDO>
{
	[Flags]
	private enum DataFlags : byte
	{
		None = 0,
		Type = 3,
		Persistent = 4,
		Distant = 8,
		Created = 0x10,
		Owner = 0x20,
		Owned = 0x40,
		Valid = 0x80
	}

	public enum ObjectType : byte
	{
		Default,
		Prioritized,
		Solid,
		Terrain
	}

	public ZDOID m_uid = ZDOID.None;

	public float m_tempSortValue;

	private int m_prefab;

	private Vector2s m_sector = Vector2s.zero;

	private Vector3 m_rotation;

	private Vector3 m_position;

	private DataFlags m_dataFlags;

	private byte m_tempRemoveEarmark;

	public bool Persistent
	{
		get
		{
			return (m_dataFlags & DataFlags.Persistent) != 0;
		}
		set
		{
			if (value)
			{
				m_dataFlags |= DataFlags.Persistent;
			}
			else
			{
				m_dataFlags &= ~DataFlags.Persistent;
			}
		}
	}

	public bool Distant
	{
		get
		{
			return (m_dataFlags & DataFlags.Distant) != 0;
		}
		set
		{
			if (value)
			{
				m_dataFlags |= DataFlags.Distant;
			}
			else
			{
				m_dataFlags &= ~DataFlags.Distant;
			}
		}
	}

	private bool Owner
	{
		get
		{
			return (m_dataFlags & DataFlags.Owner) != 0;
		}
		set
		{
			if (value)
			{
				m_dataFlags |= DataFlags.Owner;
			}
			else
			{
				m_dataFlags &= ~DataFlags.Owner;
			}
		}
	}

	private bool Owned
	{
		get
		{
			return (m_dataFlags & DataFlags.Owned) != 0;
		}
		set
		{
			if (value)
			{
				m_dataFlags |= DataFlags.Owned;
			}
			else
			{
				m_dataFlags &= ~DataFlags.Owned;
			}
		}
	}

	private bool Valid
	{
		get
		{
			return (m_dataFlags & DataFlags.Valid) != 0;
		}
		set
		{
			if (value)
			{
				m_dataFlags |= DataFlags.Valid;
			}
			else
			{
				m_dataFlags &= ~DataFlags.Valid;
			}
		}
	}

	public bool Created
	{
		get
		{
			return (m_dataFlags & DataFlags.Created) != 0;
		}
		set
		{
			if (value)
			{
				m_dataFlags |= DataFlags.Created;
			}
			else
			{
				m_dataFlags &= ~DataFlags.Created;
			}
		}
	}

	public ObjectType Type
	{
		get
		{
			return (ObjectType)(m_dataFlags & DataFlags.Type);
		}
		set
		{
			m_dataFlags = (DataFlags)((uint)(m_dataFlags & ~DataFlags.Type) | (uint)(value & ObjectType.Terrain));
		}
	}

	private bool SaveClone
	{
		get
		{
			return m_tempSortValue < 0f;
		}
		set
		{
			if (value)
			{
				m_tempSortValue = -1f;
			}
			else
			{
				m_tempSortValue = 0f;
			}
		}
	}

	public byte TempRemoveEarmark
	{
		get
		{
			return m_tempRemoveEarmark;
		}
		set
		{
			m_tempRemoveEarmark = value;
		}
	}

	public ushort OwnerRevision { get; set; }

	public uint DataRevision { get; set; }

	public void Initialize(ZDOID id, Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		m_uid = id;
		m_position = position;
		Vector2i zone = ZoneSystem.GetZone(m_position);
		m_sector = Utils.ClampToShort(zone);
		ZDOMan.instance.AddToSector(this, zone);
		m_dataFlags = DataFlags.None;
		Valid = true;
	}

	public void Init()
	{
		m_dataFlags = DataFlags.None;
		Valid = true;
	}

	public override string ToString()
	{
		return m_uid.ToString();
	}

	public bool IsValid()
	{
		return Valid;
	}

	public override int GetHashCode()
	{
		return m_uid.GetHashCode();
	}

	public bool Equals(ZDO other)
	{
		return this == other;
	}

	public void Reset()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!SaveClone)
		{
			ZDOExtraData.Release(this, m_uid);
		}
		m_uid = ZDOID.None;
		m_dataFlags = DataFlags.None;
		OwnerRevision = 0;
		DataRevision = 0u;
		m_tempSortValue = 0f;
		m_prefab = 0;
		m_sector = Vector2s.zero;
		m_position = Vector3.zero;
		Quaternion identity = Quaternion.identity;
		m_rotation = ((Quaternion)(ref identity)).eulerAngles;
	}

	public ZDO Clone()
	{
		ZDO obj = MemberwiseClone() as ZDO;
		obj.SaveClone = true;
		return obj;
	}

	public void Set(string name, ZDOID id)
	{
		Set(GetHashZDOID(name), id);
	}

	public void Set(KeyValuePair<int, int> hashPair, ZDOID id)
	{
		Set(hashPair.Key, id.UserID);
		Set(hashPair.Value, id.ID);
	}

	public static KeyValuePair<int, int> GetHashZDOID(string name)
	{
		return new KeyValuePair<int, int>(StringExtensionMethods.GetStableHashCode(name + "_u"), StringExtensionMethods.GetStableHashCode(name + "_i"));
	}

	public ZDOID GetZDOID(string name)
	{
		return GetZDOID(GetHashZDOID(name));
	}

	public ZDOID GetZDOID(KeyValuePair<int, int> hashPair)
	{
		long @long = GetLong(hashPair.Key, 0L);
		uint num = (uint)GetLong(hashPair.Value, 0L);
		if (@long == 0L || num == 0)
		{
			return ZDOID.None;
		}
		return new ZDOID(@long, num);
	}

	public void Set(string name, float value)
	{
		Set(StringExtensionMethods.GetStableHashCode(name), value);
	}

	public void Set(int hash, float value)
	{
		if (ZDOExtraData.Set(m_uid, hash, value))
		{
			IncreaseDataRevision();
		}
	}

	public void Set(string name, Vector3 value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Set(StringExtensionMethods.GetStableHashCode(name), value);
	}

	public void Set(int hash, Vector3 value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (ZDOExtraData.Set(m_uid, hash, value))
		{
			IncreaseDataRevision();
		}
	}

	public void Update(int hash, Vector3 value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (ZDOExtraData.Update(m_uid, hash, value))
		{
			IncreaseDataRevision();
		}
	}

	public void Set(string name, Quaternion value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Set(StringExtensionMethods.GetStableHashCode(name), value);
	}

	public void Set(int hash, Quaternion value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (ZDOExtraData.Set(m_uid, hash, value))
		{
			IncreaseDataRevision();
		}
	}

	public void Set(string name, int value)
	{
		Set(StringExtensionMethods.GetStableHashCode(name), value);
	}

	public void Set(int hash, int value, bool okForNotOwner = false)
	{
		if (ZDOExtraData.Set(m_uid, hash, value))
		{
			IncreaseDataRevision();
		}
	}

	public void SetConnection(ZDOExtraData.ConnectionType connectionType, ZDOID zid)
	{
		if (ZDOExtraData.SetConnection(m_uid, connectionType, zid))
		{
			IncreaseDataRevision();
		}
	}

	public void UpdateConnection(ZDOExtraData.ConnectionType connectionType, ZDOID zid)
	{
		if (ZDOExtraData.UpdateConnection(m_uid, connectionType, zid))
		{
			IncreaseDataRevision();
		}
	}

	public void Set(string name, bool value)
	{
		Set(StringExtensionMethods.GetStableHashCode(name), value);
	}

	public void Set(int hash, bool value)
	{
		Set(hash, value ? 1 : 0);
	}

	public void Set(string name, long value)
	{
		Set(StringExtensionMethods.GetStableHashCode(name), value);
	}

	public void Set(int hash, long value)
	{
		if (ZDOExtraData.Set(m_uid, hash, value))
		{
			IncreaseDataRevision();
		}
	}

	public void Set(string name, byte[] bytes)
	{
		Set(StringExtensionMethods.GetStableHashCode(name), bytes);
	}

	public void Set(int hash, byte[] bytes)
	{
		if (ZDOExtraData.Set(m_uid, hash, bytes))
		{
			IncreaseDataRevision();
		}
	}

	public void Set(string name, string value)
	{
		Set(StringExtensionMethods.GetStableHashCode(name), value);
	}

	public void Set(int hash, string value)
	{
		if (ZDOExtraData.Set(m_uid, hash, value))
		{
			IncreaseDataRevision();
		}
	}

	public void SetPosition(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		InternalSetPosition(pos);
	}

	public void InternalSetPosition(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_position == pos))
		{
			m_position = pos;
			SetSector(ZoneSystem.GetZone(m_position));
			if (IsOwner())
			{
				IncreaseDataRevision();
			}
		}
	}

	public void InvalidateSector()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		SetSector(new Vector2i(int.MinValue, int.MinValue));
	}

	private void SetSector(Vector2i sector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_sector == sector))
		{
			ZDOMan.instance.RemoveFromSector(this, ((Vector2s)(ref m_sector)).ToVector2i());
			m_sector = Utils.ClampToShort(sector);
			ZDOMan.instance.AddToSector(this, sector);
			if (ZNet.instance.IsServer())
			{
				ZDOMan.instance.ZDOSectorInvalidated(this);
			}
		}
	}

	public Vector2i GetSector()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Vector2s)(ref m_sector)).ToVector2i();
	}

	public void SetRotation(Quaternion rot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_rotation == ((Quaternion)(ref rot)).eulerAngles))
		{
			m_rotation = ((Quaternion)(ref rot)).eulerAngles;
			IncreaseDataRevision();
		}
	}

	public void SetType(ObjectType type)
	{
		if (Type != type)
		{
			Type = type;
			IncreaseDataRevision();
		}
	}

	public void SetDistant(bool distant)
	{
		if (Distant != distant)
		{
			Distant = distant;
			IncreaseDataRevision();
		}
	}

	public void SetPrefab(int prefab)
	{
		if (m_prefab != prefab)
		{
			m_prefab = prefab;
			IncreaseDataRevision();
		}
	}

	public int GetPrefab()
	{
		return m_prefab;
	}

	public Vector3 GetPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return m_position;
	}

	public Quaternion GetRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(m_rotation);
	}

	private void IncreaseDataRevision()
	{
		DataRevision++;
		if (!ZNet.instance.IsServer())
		{
			ZDOMan.instance.ClientChanged(m_uid);
		}
	}

	private void IncreaseOwnerRevision()
	{
		OwnerRevision++;
		if (!ZNet.instance.IsServer())
		{
			ZDOMan.instance.ClientChanged(m_uid);
		}
	}

	public float GetFloat(string name, float defaultValue = 0f)
	{
		return GetFloat(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public float GetFloat(int hash, float defaultValue = 0f)
	{
		return ZDOExtraData.GetFloat(m_uid, hash, defaultValue);
	}

	public bool GetFloat(string name, out float value)
	{
		return GetFloat(StringExtensionMethods.GetStableHashCode(name), out value);
	}

	public bool GetFloat(int hash, out float value)
	{
		return ZDOExtraData.GetFloat(m_uid, hash, out value);
	}

	public Vector3 GetVec3(string name, Vector3 defaultValue)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return GetVec3(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public Vector3 GetVec3(int hash, Vector3 defaultValue)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ZDOExtraData.GetVec3(m_uid, hash, defaultValue);
	}

	public bool GetVec3(string name, out Vector3 value)
	{
		return GetVec3(StringExtensionMethods.GetStableHashCode(name), out value);
	}

	public bool GetVec3(int hash, out Vector3 value)
	{
		return ZDOExtraData.GetVec3(m_uid, hash, out value);
	}

	public Quaternion GetQuaternion(string name, Quaternion defaultValue)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return GetQuaternion(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public Quaternion GetQuaternion(int hash, Quaternion defaultValue)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ZDOExtraData.GetQuaternion(m_uid, hash, defaultValue);
	}

	public bool GetQuaternion(string name, out Quaternion value)
	{
		return GetQuaternion(StringExtensionMethods.GetStableHashCode(name), out value);
	}

	public bool GetQuaternion(int hash, out Quaternion value)
	{
		return ZDOExtraData.GetQuaternion(m_uid, hash, out value);
	}

	public int GetInt(string name, int defaultValue = 0)
	{
		return GetInt(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public int GetInt(int hash, int defaultValue = 0)
	{
		return ZDOExtraData.GetInt(m_uid, hash, defaultValue);
	}

	public bool GetInt(string name, out int value)
	{
		return GetInt(StringExtensionMethods.GetStableHashCode(name), out value);
	}

	public bool GetInt(int hash, out int value)
	{
		return ZDOExtraData.GetInt(m_uid, hash, out value);
	}

	public bool GetBool(string name, bool defaultValue = false)
	{
		return GetBool(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public bool GetBool(int hash, bool defaultValue = false)
	{
		return ZDOExtraData.GetInt(m_uid, hash, defaultValue ? 1 : 0) != 0;
	}

	public bool GetBool(string name, out bool value)
	{
		return GetBool(StringExtensionMethods.GetStableHashCode(name), out value);
	}

	public bool GetBool(int hash, out bool value)
	{
		return ZDOExtraData.GetBool(m_uid, hash, out value);
	}

	public long GetLong(string name, long defaultValue = 0L)
	{
		return GetLong(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public long GetLong(int hash, long defaultValue = 0L)
	{
		return ZDOExtraData.GetLong(m_uid, hash, defaultValue);
	}

	public string GetString(string name, string defaultValue = "")
	{
		return GetString(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public string GetString(int hash, string defaultValue = "")
	{
		return ZDOExtraData.GetString(m_uid, hash, defaultValue);
	}

	public bool GetString(string name, out string value)
	{
		return GetString(StringExtensionMethods.GetStableHashCode(name), out value);
	}

	public bool GetString(int hash, out string value)
	{
		return ZDOExtraData.GetString(m_uid, hash, out value);
	}

	public byte[] GetByteArray(string name, byte[] defaultValue = null)
	{
		return GetByteArray(StringExtensionMethods.GetStableHashCode(name), defaultValue);
	}

	public byte[] GetByteArray(int hash, byte[] defaultValue = null)
	{
		return ZDOExtraData.GetByteArray(m_uid, hash, defaultValue);
	}

	public bool GetByteArray(string name, out byte[] value)
	{
		return GetByteArray(StringExtensionMethods.GetStableHashCode(name), out value);
	}

	public bool GetByteArray(int hash, out byte[] value)
	{
		return ZDOExtraData.GetByteArray(m_uid, hash, out value);
	}

	public ZDOID GetConnectionZDOID(ZDOExtraData.ConnectionType type)
	{
		return ZDOExtraData.GetConnectionZDOID(m_uid, type);
	}

	public ZDOExtraData.ConnectionType GetConnectionType()
	{
		return ZDOExtraData.GetConnectionType(m_uid);
	}

	public ZDOConnection GetConnection()
	{
		return ZDOExtraData.GetConnection(m_uid);
	}

	public ZDOConnectionHashData GetConnectionHashData(ZDOExtraData.ConnectionType type)
	{
		return ZDOExtraData.GetConnectionHashData(m_uid, type);
	}

	public bool RemoveInt(string name)
	{
		return RemoveInt(StringExtensionMethods.GetStableHashCode(name));
	}

	public bool RemoveInt(int hash)
	{
		return ZDOExtraData.RemoveInt(m_uid, hash);
	}

	public bool RemoveLong(int hash)
	{
		return ZDOExtraData.RemoveLong(m_uid, hash);
	}

	public bool RemoveFloat(string name)
	{
		return RemoveFloat(StringExtensionMethods.GetStableHashCode(name));
	}

	public bool RemoveFloat(int hash)
	{
		return ZDOExtraData.RemoveFloat(m_uid, hash);
	}

	public bool RemoveVec3(string name)
	{
		return RemoveVec3(StringExtensionMethods.GetStableHashCode(name));
	}

	public bool RemoveVec3(int hash)
	{
		return ZDOExtraData.RemoveVec3(m_uid, hash);
	}

	public bool RemoveQuaternion(string name)
	{
		return RemoveQuaternion(StringExtensionMethods.GetStableHashCode(name));
	}

	public bool RemoveQuaternion(int hash)
	{
		return ZDOExtraData.RemoveQuaternion(m_uid, hash);
	}

	public void RemoveZDOID(string name)
	{
		KeyValuePair<int, int> hashZDOID = GetHashZDOID(name);
		ZDOExtraData.RemoveLong(m_uid, hashZDOID.Key);
		ZDOExtraData.RemoveLong(m_uid, hashZDOID.Value);
	}

	public void RemoveZDOID(KeyValuePair<int, int> hashes)
	{
		ZDOExtraData.RemoveLong(m_uid, hashes.Key);
		ZDOExtraData.RemoveLong(m_uid, hashes.Value);
	}

	public void Serialize(ZPackage pkg)
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		List<KeyValuePair<int, float>> floats = ZDOExtraData.GetFloats(m_uid);
		List<KeyValuePair<int, Vector3>> vec3s = ZDOExtraData.GetVec3s(m_uid);
		List<KeyValuePair<int, Quaternion>> quaternions = ZDOExtraData.GetQuaternions(m_uid);
		List<KeyValuePair<int, int>> ints = ZDOExtraData.GetInts(m_uid);
		List<KeyValuePair<int, long>> longs = ZDOExtraData.GetLongs(m_uid);
		List<KeyValuePair<int, string>> strings = ZDOExtraData.GetStrings(m_uid);
		List<KeyValuePair<int, byte[]>> byteArrays = ZDOExtraData.GetByteArrays(m_uid);
		ZDOConnection connection = ZDOExtraData.GetConnection(m_uid);
		ushort num = 0;
		if (connection != null && connection.m_type != 0)
		{
			num = (ushort)(num | 1u);
		}
		if (floats.Count > 0)
		{
			num = (ushort)(num | 2u);
		}
		if (vec3s.Count > 0)
		{
			num = (ushort)(num | 4u);
		}
		if (quaternions.Count > 0)
		{
			num = (ushort)(num | 8u);
		}
		if (ints.Count > 0)
		{
			num = (ushort)(num | 0x10u);
		}
		if (longs.Count > 0)
		{
			num = (ushort)(num | 0x20u);
		}
		if (strings.Count > 0)
		{
			num = (ushort)(num | 0x40u);
		}
		if (byteArrays.Count > 0)
		{
			num = (ushort)(num | 0x80u);
		}
		Vector3 rotation = m_rotation;
		Quaternion identity = Quaternion.identity;
		bool flag = rotation != ((Quaternion)(ref identity)).eulerAngles;
		num = (ushort)(num | (Persistent ? 256u : 0u));
		num = (ushort)(num | (Distant ? 512u : 0u));
		num |= (ushort)((uint)Type << 10);
		num = (ushort)(num | (flag ? 4096u : 0u));
		pkg.Write(num);
		pkg.Write(m_prefab);
		if (flag)
		{
			pkg.Write(m_rotation);
		}
		if ((num & 0xFF) == 0)
		{
			return;
		}
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			pkg.Write((byte)connection.m_type);
			pkg.Write(connection.m_target);
		}
		if (floats.Count > 0)
		{
			pkg.Write((byte)floats.Count);
			foreach (KeyValuePair<int, float> item in floats)
			{
				pkg.Write(item.Key);
				pkg.Write(item.Value);
			}
		}
		if (vec3s.Count > 0)
		{
			pkg.Write((byte)vec3s.Count);
			foreach (KeyValuePair<int, Vector3> item2 in vec3s)
			{
				pkg.Write(item2.Key);
				pkg.Write(item2.Value);
			}
		}
		if (quaternions.Count > 0)
		{
			pkg.Write((byte)quaternions.Count);
			foreach (KeyValuePair<int, Quaternion> item3 in quaternions)
			{
				pkg.Write(item3.Key);
				pkg.Write(item3.Value);
			}
		}
		if (ints.Count > 0)
		{
			pkg.Write((byte)ints.Count);
			foreach (KeyValuePair<int, int> item4 in ints)
			{
				pkg.Write(item4.Key);
				pkg.Write(item4.Value);
			}
		}
		if (longs.Count > 0)
		{
			pkg.Write((byte)longs.Count);
			foreach (KeyValuePair<int, long> item5 in longs)
			{
				pkg.Write(item5.Key);
				pkg.Write(item5.Value);
			}
		}
		if (strings.Count > 0)
		{
			pkg.Write((byte)strings.Count);
			foreach (KeyValuePair<int, string> item6 in strings)
			{
				pkg.Write(item6.Key);
				pkg.Write(item6.Value);
			}
		}
		if (byteArrays.Count <= 0)
		{
			return;
		}
		pkg.Write((byte)byteArrays.Count);
		foreach (KeyValuePair<int, byte[]> item7 in byteArrays)
		{
			pkg.Write(item7.Key);
			pkg.Write(item7.Value);
		}
	}

	public void Deserialize(ZPackage pkg)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		ushort num = pkg.ReadUShort();
		Persistent = (num & 0x100) != 0;
		Distant = (num & 0x200) != 0;
		Type = (ObjectType)((uint)(num >> 10) & 3u);
		m_prefab = pkg.ReadInt();
		if ((num & 0x1000u) != 0)
		{
			m_rotation = pkg.ReadVector3();
		}
		if ((num & 0xFF) == 0)
		{
			return;
		}
		bool num2 = (num & 1) != 0;
		bool flag = (num & 2) != 0;
		bool flag2 = (num & 4) != 0;
		bool flag3 = (num & 8) != 0;
		bool flag4 = (num & 0x10) != 0;
		bool flag5 = (num & 0x20) != 0;
		bool flag6 = (num & 0x40) != 0;
		bool flag7 = (num & 0x80) != 0;
		if (num2)
		{
			ZDOExtraData.ConnectionType connectionType = (ZDOExtraData.ConnectionType)pkg.ReadByte();
			ZDOID target = pkg.ReadZDOID();
			ZDOExtraData.SetConnection(m_uid, connectionType, target);
		}
		if (flag)
		{
			int num3 = pkg.ReadByte();
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Float, num3);
			for (int i = 0; i < num3; i++)
			{
				int hash = pkg.ReadInt();
				float value = pkg.ReadSingle();
				ZDOExtraData.Set(m_uid, hash, value);
			}
		}
		if (flag2)
		{
			int num4 = pkg.ReadByte();
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Vec3, num4);
			for (int j = 0; j < num4; j++)
			{
				int hash2 = pkg.ReadInt();
				Vector3 value2 = pkg.ReadVector3();
				ZDOExtraData.Set(m_uid, hash2, value2);
			}
		}
		if (flag3)
		{
			int num5 = pkg.ReadByte();
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Quat, num5);
			for (int k = 0; k < num5; k++)
			{
				int hash3 = pkg.ReadInt();
				Quaternion value3 = pkg.ReadQuaternion();
				ZDOExtraData.Set(m_uid, hash3, value3);
			}
		}
		if (flag4)
		{
			int num6 = pkg.ReadByte();
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Int, num6);
			for (int l = 0; l < num6; l++)
			{
				int hash4 = pkg.ReadInt();
				int value4 = pkg.ReadInt();
				ZDOExtraData.Set(m_uid, hash4, value4);
			}
		}
		if (flag5)
		{
			int num7 = pkg.ReadByte();
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Long, num7);
			for (int m = 0; m < num7; m++)
			{
				int hash5 = pkg.ReadInt();
				long value5 = pkg.ReadLong();
				ZDOExtraData.Set(m_uid, hash5, value5);
			}
		}
		if (flag6)
		{
			int num8 = pkg.ReadByte();
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.String, num8);
			for (int n = 0; n < num8; n++)
			{
				int hash6 = pkg.ReadInt();
				string value6 = pkg.ReadString();
				ZDOExtraData.Set(m_uid, hash6, value6);
			}
		}
		if (flag7)
		{
			int num9 = pkg.ReadByte();
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.ByteArray, num9);
			for (int num10 = 0; num10 < num9; num10++)
			{
				int hash7 = pkg.ReadInt();
				byte[] value7 = pkg.ReadByteArray();
				ZDOExtraData.Set(m_uid, hash7, value7);
			}
		}
	}

	public void Save(ZPackage pkg)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		List<KeyValuePair<int, float>> saveFloats = ZDOExtraData.GetSaveFloats(m_uid);
		List<KeyValuePair<int, Vector3>> saveVec3s = ZDOExtraData.GetSaveVec3s(m_uid);
		List<KeyValuePair<int, Quaternion>> saveQuaternions = ZDOExtraData.GetSaveQuaternions(m_uid);
		List<KeyValuePair<int, int>> saveInts = ZDOExtraData.GetSaveInts(m_uid);
		List<KeyValuePair<int, long>> saveLongs = ZDOExtraData.GetSaveLongs(m_uid);
		List<KeyValuePair<int, string>> saveStrings = ZDOExtraData.GetSaveStrings(m_uid);
		List<KeyValuePair<int, byte[]>> saveByteArrays = ZDOExtraData.GetSaveByteArrays(m_uid);
		ZDOConnectionHashData saveConnections = ZDOExtraData.GetSaveConnections(m_uid);
		ushort num = 0;
		if (saveConnections != null && saveConnections.m_type != 0)
		{
			num = (ushort)(num | 1u);
		}
		if (saveFloats.Count > 0)
		{
			num = (ushort)(num | 2u);
		}
		if (saveVec3s.Count > 0)
		{
			num = (ushort)(num | 4u);
		}
		if (saveQuaternions.Count > 0)
		{
			num = (ushort)(num | 8u);
		}
		if (saveInts.Count > 0)
		{
			num = (ushort)(num | 0x10u);
		}
		if (saveLongs.Count > 0)
		{
			num = (ushort)(num | 0x20u);
		}
		if (saveStrings.Count > 0)
		{
			num = (ushort)(num | 0x40u);
		}
		if (saveByteArrays.Count > 0)
		{
			num = (ushort)(num | 0x80u);
		}
		Vector3 rotation = m_rotation;
		Quaternion identity = Quaternion.identity;
		bool flag = rotation != ((Quaternion)(ref identity)).eulerAngles;
		num = (ushort)(num | (Persistent ? 256u : 0u));
		num = (ushort)(num | (Distant ? 512u : 0u));
		num |= (ushort)((uint)Type << 10);
		num = (ushort)(num | (flag ? 4096u : 0u));
		pkg.Write(num);
		pkg.Write(m_sector);
		pkg.Write(m_position);
		pkg.Write(m_prefab);
		if (flag)
		{
			pkg.Write(m_rotation);
		}
		if ((num & 0xFFu) != 0)
		{
			if (((uint)num & (true ? 1u : 0u)) != 0)
			{
				pkg.Write((byte)saveConnections.m_type);
				pkg.Write(saveConnections.m_hash);
			}
			ZDODataHelper.WriteData(pkg, saveFloats, delegate(float value)
			{
				pkg.Write(value);
			});
			ZDODataHelper.WriteData(pkg, saveVec3s, delegate(Vector3 value)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				pkg.Write(value);
			});
			ZDODataHelper.WriteData(pkg, saveQuaternions, delegate(Quaternion value)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				pkg.Write(value);
			});
			ZDODataHelper.WriteData(pkg, saveInts, delegate(int value)
			{
				pkg.Write(value);
			});
			ZDODataHelper.WriteData(pkg, saveLongs, delegate(long value)
			{
				pkg.Write(value);
			});
			ZDODataHelper.WriteData(pkg, saveStrings, delegate(string value)
			{
				pkg.Write(value);
			});
			ZDODataHelper.WriteData(pkg, saveByteArrays, delegate(byte[] value)
			{
				pkg.Write(value);
			});
		}
	}

	private static bool Strip(int key)
	{
		return ZDOHelper.s_stripOldData.Contains(key);
	}

	private static bool StripLong(int key)
	{
		return ZDOHelper.s_stripOldLongData.Contains(key);
	}

	private static bool Strip(int key, long data)
	{
		if (!StripLong(key))
		{
			return Strip(key);
		}
		return true;
	}

	private static bool Strip(int key, int data)
	{
		return Strip(key);
	}

	private static bool Strip(int key, Quaternion data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (!(data == Quaternion.identity))
		{
			return Strip(key);
		}
		return true;
	}

	private static bool Strip(int key, string data)
	{
		if (!string.IsNullOrEmpty(data))
		{
			return Strip(key);
		}
		return true;
	}

	private static bool Strip(int key, byte[] data)
	{
		if (data.Length != 0)
		{
			return ZDOHelper.s_stripOldDataByteArray.Contains(key);
		}
		return true;
	}

	private static bool StripConvert(ZDOID zid, int key, long data)
	{
		if (Strip(key))
		{
			return true;
		}
		if (key == ZDOVars.s_SpawnTime__DontUse || key == ZDOVars.s_spawn_time__DontUse)
		{
			ZDOExtraData.Set(zid, ZDOVars.s_spawnTime, data);
			return true;
		}
		return false;
	}

	private static bool StripConvert(ZDOID zid, int key, Vector3 data)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (Strip(key))
		{
			return true;
		}
		if (key == ZDOVars.s_SpawnPoint__DontUse)
		{
			ZDOExtraData.Set(zid, ZDOVars.s_spawnPoint, data);
			return true;
		}
		if (Mathf.Approximately(data.x, data.y) && Mathf.Approximately(data.x, data.z))
		{
			if (key == ZDOVars.s_scaleHash)
			{
				if (Mathf.Approximately(data.x, 1f))
				{
					return true;
				}
				ZDOExtraData.Set(zid, ZDOVars.s_scaleScalarHash, data.x);
				return true;
			}
			if (Mathf.Approximately(data.x, 0f))
			{
				return true;
			}
		}
		return false;
	}

	private static bool StripConvert(ZDOID zid, int key, float data)
	{
		if (Strip(key))
		{
			return true;
		}
		if (key == ZDOVars.s_scaleScalarHash && Mathf.Approximately(data, 1f))
		{
			return true;
		}
		return false;
	}

	public void LoadOldFormat(ZPackage pkg, int version)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		pkg.ReadUInt();
		pkg.ReadUInt();
		Persistent = pkg.ReadBool();
		pkg.ReadLong();
		long timeCreated = pkg.ReadLong();
		ZDOExtraData.SetTimeCreated(m_uid, timeCreated);
		pkg.ReadInt();
		if (version >= 16 && version < 24)
		{
			pkg.ReadInt();
		}
		if (version >= 23)
		{
			Type = (ObjectType)((uint)pkg.ReadSByte() & 3u);
		}
		if (version >= 22)
		{
			Distant = pkg.ReadBool();
		}
		if (version < 13)
		{
			pkg.ReadChar();
			pkg.ReadChar();
		}
		if (version >= 17)
		{
			m_prefab = pkg.ReadInt();
		}
		m_sector = Utils.ClampToShort(pkg.ReadVector2i());
		m_position = pkg.ReadVector3();
		Quaternion val = pkg.ReadQuaternion();
		m_rotation = ((Quaternion)(ref val)).eulerAngles;
		int num = pkg.ReadChar();
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				int num2 = pkg.ReadInt();
				float num3 = pkg.ReadSingle();
				if (!StripConvert(m_uid, num2, num3))
				{
					ZDOExtraData.Set(m_uid, num2, num3);
				}
			}
		}
		int num4 = pkg.ReadChar();
		if (num4 > 0)
		{
			for (int j = 0; j < num4; j++)
			{
				int num5 = pkg.ReadInt();
				Vector3 val2 = pkg.ReadVector3();
				if (!StripConvert(m_uid, num5, val2))
				{
					ZDOExtraData.Set(m_uid, num5, val2);
				}
			}
		}
		int num6 = pkg.ReadChar();
		if (num6 > 0)
		{
			for (int k = 0; k < num6; k++)
			{
				int num7 = pkg.ReadInt();
				Quaternion value = pkg.ReadQuaternion();
				if (!Strip(num7))
				{
					ZDOExtraData.Set(m_uid, num7, value);
				}
			}
		}
		int num8 = pkg.ReadChar();
		if (num8 > 0)
		{
			for (int l = 0; l < num8; l++)
			{
				int num9 = pkg.ReadInt();
				int value2 = pkg.ReadInt();
				if (!Strip(num9))
				{
					ZDOExtraData.Set(m_uid, num9, value2);
				}
			}
		}
		int num10 = pkg.ReadChar();
		if (num10 > 0)
		{
			for (int m = 0; m < num10; m++)
			{
				int num11 = pkg.ReadInt();
				long num12 = pkg.ReadLong();
				if (!StripConvert(m_uid, num11, num12))
				{
					ZDOExtraData.Set(m_uid, num11, num12);
				}
			}
		}
		int num13 = pkg.ReadChar();
		if (num13 > 0)
		{
			for (int n = 0; n < num13; n++)
			{
				int num14 = pkg.ReadInt();
				string value3 = pkg.ReadString();
				if (!Strip(num14))
				{
					ZDOExtraData.Set(m_uid, num14, value3);
				}
			}
		}
		if (version >= 27)
		{
			int num15 = pkg.ReadChar();
			if (num15 > 0)
			{
				for (int num16 = 0; num16 < num15; num16++)
				{
					int num17 = pkg.ReadInt();
					byte[] value4 = pkg.ReadByteArray();
					if (!Strip(num17))
					{
						ZDOExtraData.Set(m_uid, num17, value4);
					}
				}
			}
		}
		if (version < 17)
		{
			m_prefab = GetInt("prefab");
		}
	}

	private int ReadNumItems(ZPackage pkg, int version)
	{
		if (version < 33)
		{
			return pkg.ReadByte();
		}
		return pkg.ReadNumItems();
	}

	public void Load(ZPackage pkg, int version)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		m_uid.SetID(++ZDOID.m_loadID);
		ushort num = pkg.ReadUShort();
		Persistent = (num & 0x100) != 0;
		Distant = (num & 0x200) != 0;
		Type = (ObjectType)((uint)(num >> 10) & 3u);
		m_sector = pkg.ReadVector2s();
		m_position = pkg.ReadVector3();
		m_prefab = pkg.ReadInt();
		OwnerRevision = 0;
		DataRevision = 0u;
		Owned = false;
		Owner = false;
		Valid = true;
		SaveClone = false;
		if ((num & 0x1000u) != 0)
		{
			m_rotation = pkg.ReadVector3();
		}
		if ((num & 0xFF) == 0)
		{
			return;
		}
		bool num2 = (num & 1) != 0;
		bool flag = (num & 2) != 0;
		bool flag2 = (num & 4) != 0;
		bool flag3 = (num & 8) != 0;
		bool flag4 = (num & 0x10) != 0;
		bool flag5 = (num & 0x20) != 0;
		bool flag6 = (num & 0x40) != 0;
		bool flag7 = (num & 0x80) != 0;
		if (num2)
		{
			ZDOExtraData.ConnectionType connectionType = (ZDOExtraData.ConnectionType)pkg.ReadByte();
			int hash = pkg.ReadInt();
			ZDOExtraData.SetConnectionData(m_uid, connectionType, hash);
		}
		if (flag)
		{
			int num3 = ReadNumItems(pkg, version);
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Float, num3);
			for (int i = 0; i < num3; i++)
			{
				int num4 = pkg.ReadInt();
				float num5 = pkg.ReadSingle();
				if (!StripConvert(m_uid, num4, num5))
				{
					ZDOExtraData.Add(m_uid, num4, num5);
				}
			}
			ZDOExtraData.RemoveIfEmpty(m_uid, ZDOExtraData.Type.Float);
		}
		if (flag2)
		{
			int num6 = ReadNumItems(pkg, version);
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Vec3, num6);
			for (int j = 0; j < num6; j++)
			{
				int num7 = pkg.ReadInt();
				Vector3 val = pkg.ReadVector3();
				if (!StripConvert(m_uid, num7, val))
				{
					ZDOExtraData.Add(m_uid, num7, val);
				}
			}
			ZDOExtraData.RemoveIfEmpty(m_uid, ZDOExtraData.Type.Vec3);
		}
		if (flag3)
		{
			int num8 = ReadNumItems(pkg, version);
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Quat, num8);
			for (int k = 0; k < num8; k++)
			{
				int num9 = pkg.ReadInt();
				Quaternion val2 = pkg.ReadQuaternion();
				if (!Strip(num9, val2))
				{
					ZDOExtraData.Add(m_uid, num9, val2);
				}
			}
			ZDOExtraData.RemoveIfEmpty(m_uid, ZDOExtraData.Type.Quat);
		}
		if (flag4)
		{
			int num10 = ReadNumItems(pkg, version);
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Int, num10);
			for (int l = 0; l < num10; l++)
			{
				int num11 = pkg.ReadInt();
				int num12 = pkg.ReadInt();
				if (!Strip(num11, num12))
				{
					ZDOExtraData.Add(m_uid, num11, num12);
				}
			}
			ZDOExtraData.RemoveIfEmpty(m_uid, ZDOExtraData.Type.Int);
		}
		if (flag5)
		{
			int num13 = ReadNumItems(pkg, version);
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.Long, num13);
			for (int m = 0; m < num13; m++)
			{
				int num14 = pkg.ReadInt();
				long num15 = pkg.ReadLong();
				if (!Strip(num14, num15))
				{
					ZDOExtraData.Add(m_uid, num14, num15);
				}
			}
			ZDOExtraData.RemoveIfEmpty(m_uid, ZDOExtraData.Type.Long);
		}
		if (flag6)
		{
			int num16 = ReadNumItems(pkg, version);
			ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.String, num16);
			for (int n = 0; n < num16; n++)
			{
				int num17 = pkg.ReadInt();
				string text = pkg.ReadString();
				if (!Strip(num17, text))
				{
					ZDOExtraData.Add(m_uid, num17, text);
				}
			}
			ZDOExtraData.RemoveIfEmpty(m_uid, ZDOExtraData.Type.String);
		}
		if (!flag7)
		{
			return;
		}
		int num18 = ReadNumItems(pkg, version);
		ZDOExtraData.Reserve(m_uid, ZDOExtraData.Type.ByteArray, num18);
		for (int num19 = 0; num19 < num18; num19++)
		{
			int num20 = pkg.ReadInt();
			byte[] array = pkg.ReadByteArray();
			if (!Strip(num20, array))
			{
				ZDOExtraData.Add(m_uid, num20, array);
			}
		}
		ZDOExtraData.RemoveIfEmpty(m_uid, ZDOExtraData.Type.ByteArray);
	}

	public long GetOwner()
	{
		if (!Owned)
		{
			return 0L;
		}
		return ZDOExtraData.GetOwner(m_uid);
	}

	public bool IsOwner()
	{
		return Owner;
	}

	public bool HasOwner()
	{
		return Owned;
	}

	public void SetOwner(long uid)
	{
		if (ZDOExtraData.GetOwner(m_uid) != uid)
		{
			SetOwnerInternal(uid);
			IncreaseOwnerRevision();
		}
	}

	public void SetOwnerInternal(long uid)
	{
		if (uid == 0L)
		{
			ZDOExtraData.ReleaseOwner(m_uid);
			Owned = false;
			Owner = false;
		}
		else
		{
			ushort ownerKey = ZDOID.AddUser(uid);
			ZDOExtraData.SetOwner(m_uid, ownerKey);
			Owned = true;
			Owner = uid == ZDOMan.GetSessionID();
		}
	}

	public ZDO()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Quaternion identity = Quaternion.identity;
		m_rotation = ((Quaternion)(ref identity)).eulerAngles;
		m_position = Vector3.zero;
		m_tempRemoveEarmark = byte.MaxValue;
		base._002Ector();
	}
}
