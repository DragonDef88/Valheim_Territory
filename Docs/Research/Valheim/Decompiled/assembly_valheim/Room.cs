using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
	public enum Theme
	{
		None = 0,
		Crypt = 1,
		SunkenCrypt = 2,
		Cave = 4,
		ForestCrypt = 8,
		GoblinCamp = 0x10,
		MeadowsVillage = 0x20,
		MeadowsFarm = 0x40,
		DvergerTown = 0x80,
		DvergerBoss = 0x100,
		ForestCryptHildir = 0x200,
		CaveHildir = 0x400,
		PlainsFortHildir = 0x800,
		AshlandRuins = 0x1000,
		FortressRuins = 0x2000
	}

	private static List<RoomConnection> tempConnections = new List<RoomConnection>();

	public Vector3Int m_size = new Vector3Int(8, 4, 8);

	[BitMask(typeof(Theme))]
	public Theme m_theme = Theme.Crypt;

	public bool m_enabled = true;

	public bool m_entrance;

	public bool m_endCap;

	public bool m_divider;

	public int m_endCapPrio;

	public int m_minPlaceOrder;

	public float m_weight = 1f;

	public bool m_faceCenter;

	public bool m_perimeter;

	[NonSerialized]
	public int m_placeOrder;

	[NonSerialized]
	public int m_seed;

	public MusicVolume m_musicPrefab;

	private RoomConnection[] m_roomConnections;

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)m_musicPrefab))
		{
			Object.Instantiate<MusicVolume>(m_musicPrefab, ((Component)this).transform).m_sizeFromRoom = this;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position, ((Component)this).transform.rotation, new Vector3(1f, 1f, 1f));
		Gizmos.DrawWireCube(Vector3.zero, new Vector3((float)((Vector3Int)(ref m_size)).x, (float)((Vector3Int)(ref m_size)).y, (float)((Vector3Int)(ref m_size)).z));
		Gizmos.matrix = Matrix4x4.identity;
	}

	public int GetHash()
	{
		return StringExtensionMethods.GetStableHashCode(Utils.GetPrefabName(((Component)this).gameObject));
	}

	private void OnEnable()
	{
		m_roomConnections = null;
	}

	public RoomConnection[] GetConnections()
	{
		if (m_roomConnections == null)
		{
			m_roomConnections = ((Component)this).GetComponentsInChildren<RoomConnection>(false);
		}
		return m_roomConnections;
	}

	public RoomConnection GetConnection(RoomConnection other)
	{
		RoomConnection[] connections = GetConnections();
		tempConnections.Clear();
		RoomConnection[] array = connections;
		foreach (RoomConnection roomConnection in array)
		{
			if (roomConnection.m_type == other.m_type)
			{
				tempConnections.Add(roomConnection);
			}
		}
		if (tempConnections.Count == 0)
		{
			return null;
		}
		return tempConnections[Random.Range(0, tempConnections.Count)];
	}

	public RoomConnection GetEntrance()
	{
		RoomConnection[] connections = GetConnections();
		ZLog.Log((object)("Connections " + connections.Length));
		RoomConnection[] array = connections;
		foreach (RoomConnection roomConnection in array)
		{
			if (roomConnection.m_entrance)
			{
				return roomConnection;
			}
		}
		return null;
	}

	public bool HaveConnection(RoomConnection other)
	{
		RoomConnection[] connections = GetConnections();
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].m_type == other.m_type)
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format("{0}, Enabled: {1}, {2}, {3}", ((Object)this).name, m_enabled, m_theme, m_entrance ? "Entrance" : (m_endCap ? "EndCap" : "Room"));
	}
}
