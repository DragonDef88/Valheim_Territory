using System.Collections.Generic;
using UnityEngine;

public class StationExtension : MonoBehaviour, Hoverable
{
	public CraftingStation m_craftingStation;

	public float m_maxStationDistance = 5f;

	public bool m_stack;

	public GameObject m_connectionPrefab;

	public Vector3 m_connectionOffset = new Vector3(0f, 0f, 0f);

	public bool m_continousConnection;

	private GameObject m_connection;

	private Piece m_piece;

	private static List<StationExtension> m_allExtensions = new List<StationExtension>();

	private void Awake()
	{
		if (((Component)this).GetComponent<ZNetView>().GetZDO() != null)
		{
			m_piece = ((Component)this).GetComponent<Piece>();
			m_allExtensions.Add(this);
			if (m_continousConnection)
			{
				((MonoBehaviour)this).InvokeRepeating("UpdateConnection", 1f, 4f);
			}
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)m_connection))
		{
			Object.Destroy((Object)(object)m_connection);
			m_connection = null;
		}
		m_allExtensions.Remove(this);
	}

	public string GetHoverText()
	{
		if (!m_continousConnection)
		{
			PokeEffect();
		}
		return Localization.instance.Localize(m_piece.m_name);
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_piece.m_name);
	}

	private string GetExtensionName()
	{
		return m_piece.m_name;
	}

	public static void FindExtensions(CraftingStation station, Vector3 pos, List<StationExtension> extensions)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (StationExtension allExtension in m_allExtensions)
		{
			if (Vector3.Distance(((Component)allExtension).transform.position, pos) < allExtension.m_maxStationDistance && allExtension.m_craftingStation.m_name == station.m_name && (allExtension.m_stack || !ExtensionInList(extensions, allExtension)))
			{
				extensions.Add(allExtension);
			}
		}
	}

	private static bool ExtensionInList(List<StationExtension> extensions, StationExtension extension)
	{
		foreach (StationExtension extension2 in extensions)
		{
			if (extension2.GetExtensionName() == extension.GetExtensionName())
			{
				return true;
			}
		}
		return false;
	}

	public bool OtherExtensionInRange(float radius)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		foreach (StationExtension allExtension in m_allExtensions)
		{
			if (!((Object)(object)allExtension == (Object)(object)this) && Vector3.Distance(((Component)allExtension).transform.position, ((Component)this).transform.position) < radius)
			{
				return true;
			}
		}
		return false;
	}

	public List<CraftingStation> FindStationsInRange(Vector3 center)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		List<CraftingStation> list = new List<CraftingStation>();
		CraftingStation.FindStationsInRange(m_craftingStation.m_name, center, m_maxStationDistance, list);
		return list;
	}

	public CraftingStation FindClosestStationInRange(Vector3 center)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return CraftingStation.FindClosestStationInRange(m_craftingStation.m_name, center, m_maxStationDistance);
	}

	private void UpdateConnection()
	{
		PokeEffect(5f);
	}

	private void PokeEffect(float timeout = 1f)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		CraftingStation craftingStation = FindClosestStationInRange(((Component)this).transform.position);
		if (Object.op_Implicit((Object)(object)craftingStation))
		{
			StartConnectionEffect(craftingStation, timeout);
		}
	}

	public void StartConnectionEffect(CraftingStation station, float timeout = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		StartConnectionEffect(station.GetConnectionEffectPoint(), timeout);
	}

	public void StartConnectionEffect(Vector3 targetPos, float timeout = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 connectionPoint = GetConnectionPoint();
		if ((Object)(object)m_connection == (Object)null)
		{
			m_connection = Object.Instantiate<GameObject>(m_connectionPrefab, connectionPoint, Quaternion.identity);
		}
		Vector3 val = targetPos - connectionPoint;
		Quaternion rotation = Quaternion.LookRotation(((Vector3)(ref val)).normalized);
		m_connection.transform.position = connectionPoint;
		m_connection.transform.rotation = rotation;
		m_connection.transform.localScale = new Vector3(1f, 1f, ((Vector3)(ref val)).magnitude);
		((MonoBehaviour)this).CancelInvoke("StopConnectionEffect");
		((MonoBehaviour)this).Invoke("StopConnectionEffect", timeout);
	}

	public void StopConnectionEffect()
	{
		if (Object.op_Implicit((Object)(object)m_connection))
		{
			Object.Destroy((Object)(object)m_connection);
			m_connection = null;
		}
	}

	private Vector3 GetConnectionPoint()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.TransformPoint(m_connectionOffset);
	}

	private void OnDrawGizmos()
	{
	}
}
