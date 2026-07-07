using System;
using System.Collections.Generic;
using UnityEngine;

public class ZNetScene : MonoBehaviour
{
	private static ZNetScene s_instance;

	private const int m_maxCreatedPerFrame = 10;

	private const float m_createDestroyFps = 30f;

	public List<GameObject> m_prefabs = new List<GameObject>();

	public List<GameObject> m_nonNetViewPrefabs = new List<GameObject>();

	private readonly Dictionary<int, GameObject> m_namedPrefabs = new Dictionary<int, GameObject>();

	private readonly Dictionary<ZDO, ZNetView> m_instances = new Dictionary<ZDO, ZNetView>();

	private readonly List<ZDO> m_tempCurrentObjects = new List<ZDO>();

	private readonly List<ZDO> m_tempCurrentObjects2 = new List<ZDO>();

	private readonly List<ZDO> m_tempCurrentDistantObjects = new List<ZDO>();

	private readonly List<ZNetView> m_tempRemoved = new List<ZNetView>();

	private float m_createDestroyTimer;

	public static ZNetScene instance => s_instance;

	private void Awake()
	{
		s_instance = this;
		foreach (GameObject prefab in m_prefabs)
		{
			m_namedPrefabs.Add(StringExtensionMethods.GetStableHashCode(((Object)prefab).name), prefab);
		}
		foreach (GameObject nonNetViewPrefab in m_nonNetViewPrefabs)
		{
			m_namedPrefabs.Add(StringExtensionMethods.GetStableHashCode(((Object)nonNetViewPrefab).name), nonNetViewPrefab);
		}
		ZDOMan zDOMan = ZDOMan.instance;
		zDOMan.m_onZDODestroyed = (Action<ZDO>)Delegate.Combine(zDOMan.m_onZDODestroyed, new Action<ZDO>(OnZDODestroyed));
		ZRoutedRpc.instance.Register<Vector3, Quaternion, int>("SpawnObject", RPC_SpawnObject);
	}

	private void OnDestroy()
	{
		ZLog.Log((object)"Net scene destroyed");
		if ((Object)(object)s_instance == (Object)(object)this)
		{
			s_instance = null;
		}
	}

	public void Shutdown()
	{
		foreach (KeyValuePair<ZDO, ZNetView> instance in m_instances)
		{
			if (Object.op_Implicit((Object)(object)instance.Value))
			{
				instance.Value.ResetZDO();
				Object.Destroy((Object)(object)((Component)instance.Value).gameObject);
			}
		}
		m_instances.Clear();
		((Behaviour)this).enabled = false;
	}

	public void AddInstance(ZDO zdo, ZNetView nview)
	{
		zdo.Created = true;
		m_instances[zdo] = nview;
	}

	private bool IsPrefabZDOValid(ZDO zdo)
	{
		int prefab = zdo.GetPrefab();
		if (prefab == 0)
		{
			return false;
		}
		return (Object)(object)GetPrefab(prefab) != (Object)null;
	}

	private GameObject CreateObject(ZDO zdo)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		int prefab = zdo.GetPrefab();
		if (prefab == 0)
		{
			return null;
		}
		GameObject prefab2 = GetPrefab(prefab);
		if ((Object)(object)prefab2 == (Object)null)
		{
			return null;
		}
		Vector3 position = zdo.GetPosition();
		Quaternion rotation = zdo.GetRotation();
		ZNetView.m_useInitZDO = true;
		ZNetView.m_initZDO = zdo;
		GameObject result = Object.Instantiate<GameObject>(prefab2, position, rotation);
		if (ZNetView.m_initZDO != null)
		{
			ZDOID uid = zdo.m_uid;
			ZLog.LogWarning((object)("ZDO " + uid.ToString() + " not used when creating object " + ((Object)prefab2).name));
			ZNetView.m_initZDO = null;
		}
		ZNetView.m_useInitZDO = false;
		return result;
	}

	public void Destroy(GameObject go)
	{
		ZNetView component = go.GetComponent<ZNetView>();
		if (Object.op_Implicit((Object)(object)component) && component.GetZDO() != null)
		{
			ZDO zDO = component.GetZDO();
			component.ResetZDO();
			m_instances.Remove(zDO);
			if (zDO.IsOwner())
			{
				ZDOMan.instance.DestroyZDO(zDO);
			}
		}
		Object.Destroy((Object)(object)go);
	}

	public bool HasPrefab(int hash)
	{
		return m_namedPrefabs.ContainsKey(hash);
	}

	public GameObject GetPrefab(int hash)
	{
		if (m_namedPrefabs.TryGetValue(hash, out var value))
		{
			return value;
		}
		return null;
	}

	public GameObject GetPrefab(string name)
	{
		return GetPrefab(StringExtensionMethods.GetStableHashCode(name));
	}

	public int GetPrefabHash(GameObject go)
	{
		return StringExtensionMethods.GetStableHashCode(((Object)go).name);
	}

	public bool IsAreaReady(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = ZoneSystem.GetZone(point);
		if (!ZoneSystem.instance.IsZoneLoaded(zone))
		{
			return false;
		}
		m_tempCurrentObjects.Clear();
		ZDOMan.instance.FindSectorObjects(zone, 1, 0, m_tempCurrentObjects);
		foreach (ZDO tempCurrentObject in m_tempCurrentObjects)
		{
			if (IsPrefabZDOValid(tempCurrentObject) && !Object.op_Implicit((Object)(object)FindInstance(tempCurrentObject)))
			{
				return false;
			}
		}
		return true;
	}

	private bool InLoadingScreen()
	{
		if (!((Object)(object)Player.m_localPlayer == (Object)null))
		{
			return Player.m_localPlayer.IsTeleporting();
		}
		return true;
	}

	private void CreateObjects(List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
	{
		int maxCreatedPerFrame = 10;
		if (InLoadingScreen())
		{
			maxCreatedPerFrame = 100;
		}
		int created = 0;
		CreateObjectsSorted(currentNearObjects, maxCreatedPerFrame, ref created);
		CreateDistantObjects(currentDistantObjects, maxCreatedPerFrame, ref created);
	}

	private void CreateObjectsSorted(List<ZDO> currentNearObjects, int maxCreatedPerFrame, ref int created)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!ZoneSystem.instance.IsActiveAreaLoaded())
		{
			return;
		}
		m_tempCurrentObjects2.Clear();
		Vector3 referencePosition = ZNet.instance.GetReferencePosition();
		foreach (ZDO currentNearObject in currentNearObjects)
		{
			if (!currentNearObject.Created)
			{
				currentNearObject.m_tempSortValue = Utils.DistanceSqr(referencePosition, currentNearObject.GetPosition());
				m_tempCurrentObjects2.Add(currentNearObject);
			}
		}
		int num = Mathf.Max(m_tempCurrentObjects2.Count / 100, maxCreatedPerFrame);
		m_tempCurrentObjects2.Sort(ZDOCompare);
		foreach (ZDO item in m_tempCurrentObjects2)
		{
			if (!ZoneSystem.instance.IsZoneReadyForType(item.GetSector(), item.Type))
			{
				continue;
			}
			if ((Object)(object)CreateObject(item) != (Object)null)
			{
				created++;
				if (created > num)
				{
					break;
				}
			}
			else if (ZNet.instance.IsServer())
			{
				item.SetOwner(ZDOMan.GetSessionID());
				ZDOID uid = item.m_uid;
				ZLog.Log((object)("Destroyed invalid predab ZDO:" + uid.ToString()));
				ZDOMan.instance.DestroyZDO(item);
			}
		}
	}

	private static int ZDOCompare(ZDO x, ZDO y)
	{
		if (x.Type == y.Type)
		{
			return Utils.CompareFloats(x.m_tempSortValue, y.m_tempSortValue);
		}
		return ((int)y.Type).CompareTo((int)x.Type);
	}

	private void CreateDistantObjects(List<ZDO> objects, int maxCreatedPerFrame, ref int created)
	{
		if (created > maxCreatedPerFrame)
		{
			return;
		}
		foreach (ZDO @object in objects)
		{
			if (@object.Created)
			{
				continue;
			}
			if ((Object)(object)CreateObject(@object) != (Object)null)
			{
				created++;
				if (created > maxCreatedPerFrame)
				{
					break;
				}
			}
			else if (ZNet.instance.IsServer())
			{
				@object.SetOwner(ZDOMan.GetSessionID());
				ZDOID uid = @object.m_uid;
				ZLog.Log((object)("Destroyed invalid predab ZDO:" + uid.ToString() + "  prefab hash:" + @object.GetPrefab()));
				ZDOMan.instance.DestroyZDO(@object);
			}
		}
	}

	private void OnZDODestroyed(ZDO zdo)
	{
		if (m_instances.TryGetValue(zdo, out var value))
		{
			value.ResetZDO();
			Object.Destroy((Object)(object)((Component)value).gameObject);
			m_instances.Remove(zdo);
		}
	}

	private void RemoveObjects(List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
	{
		byte b = (byte)((uint)Time.frameCount & 0xFFu);
		foreach (ZDO currentNearObject in currentNearObjects)
		{
			currentNearObject.TempRemoveEarmark = b;
		}
		foreach (ZDO currentDistantObject in currentDistantObjects)
		{
			currentDistantObject.TempRemoveEarmark = b;
		}
		m_tempRemoved.Clear();
		foreach (ZNetView value in m_instances.Values)
		{
			if (value.GetZDO().TempRemoveEarmark != b)
			{
				m_tempRemoved.Add(value);
			}
		}
		for (int i = 0; i < m_tempRemoved.Count; i++)
		{
			ZNetView zNetView = m_tempRemoved[i];
			ZDO zDO = zNetView.GetZDO();
			zNetView.ResetZDO();
			Object.Destroy((Object)(object)((Component)zNetView).gameObject);
			if (!zDO.Persistent && zDO.IsOwner())
			{
				ZDOMan.instance.DestroyZDO(zDO);
			}
			m_instances.Remove(zDO);
		}
	}

	public ZNetView FindInstance(ZDO zdo)
	{
		if (m_instances.TryGetValue(zdo, out var value))
		{
			return value;
		}
		return null;
	}

	public bool HaveInstance(ZDO zdo)
	{
		return m_instances.ContainsKey(zdo);
	}

	public GameObject FindInstance(ZDOID id)
	{
		ZDO zDO = ZDOMan.instance.GetZDO(id);
		if (zDO != null)
		{
			ZNetView zNetView = FindInstance(zDO);
			if (Object.op_Implicit((Object)(object)zNetView))
			{
				return ((Component)zNetView).gameObject;
			}
		}
		return null;
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		m_createDestroyTimer += deltaTime;
		if (m_createDestroyTimer >= 1f / 30f)
		{
			m_createDestroyTimer = 0f;
			CreateDestroyObjects();
		}
	}

	private void CreateDestroyObjects()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = ZoneSystem.GetZone(ZNet.instance.GetReferencePosition());
		m_tempCurrentObjects.Clear();
		m_tempCurrentDistantObjects.Clear();
		ZDOMan.instance.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea, ZoneSystem.instance.m_activeDistantArea, m_tempCurrentObjects, m_tempCurrentDistantObjects);
		CreateObjects(m_tempCurrentObjects, m_tempCurrentDistantObjects);
		RemoveObjects(m_tempCurrentObjects, m_tempCurrentDistantObjects);
	}

	public static bool InActiveArea(Vector2i zone, Vector3 refPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone2 = ZoneSystem.GetZone(refPoint);
		return InActiveArea(zone, zone2);
	}

	public static bool InActiveArea(Vector2i zone, Vector2i refCenterZone)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		int num = ZoneSystem.instance.m_activeArea - 1;
		if (zone.x >= refCenterZone.x - num && zone.x <= refCenterZone.x + num && zone.y <= refCenterZone.y + num)
		{
			return zone.y >= refCenterZone.y - num;
		}
		return false;
	}

	public static bool InActiveArea(Vector2i zone, Vector2i refCenterZone, int activatedArea)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (zone.x >= refCenterZone.x - activatedArea && zone.x <= refCenterZone.x + activatedArea && zone.y <= refCenterZone.y + activatedArea)
		{
			return zone.y >= refCenterZone.y - activatedArea;
		}
		return false;
	}

	public bool OutsideActiveArea(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return OutsideActiveArea(point, ZNet.instance.GetReferencePosition());
	}

	private static bool OutsideActiveArea(Vector3 point, Vector3 refPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = ZoneSystem.GetZone(refPoint);
		Vector2i zone2 = ZoneSystem.GetZone(point);
		if (zone2.x > zone.x - ZoneSystem.instance.m_activeArea && zone2.x < zone.x + ZoneSystem.instance.m_activeArea && zone2.y < zone.y + ZoneSystem.instance.m_activeArea)
		{
			return zone2.y <= zone.y - ZoneSystem.instance.m_activeArea;
		}
		return true;
	}

	public static bool OutsideActiveArea(Vector3 point, Vector2i centerZone, int activeArea)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = ZoneSystem.GetZone(point);
		if (zone.x > centerZone.x - activeArea && zone.x < centerZone.x + activeArea && zone.y < centerZone.y + activeArea)
		{
			return zone.y <= centerZone.y - activeArea;
		}
		return true;
	}

	public bool HaveInstanceInSector(Vector2i sector)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<ZDO, ZNetView> instance in m_instances)
		{
			if (Object.op_Implicit((Object)(object)instance.Value) && !instance.Value.m_distant && ZoneSystem.GetZone(((Component)instance.Value).transform.position) == sector)
			{
				return true;
			}
		}
		return false;
	}

	public int NrOfInstances()
	{
		return m_instances.Count;
	}

	public void SpawnObject(Vector3 pos, Quaternion rot, GameObject prefab)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int prefabHash = GetPrefabHash(prefab);
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SpawnObject", pos, rot, prefabHash);
	}

	public List<string> GetPrefabNames()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<int, GameObject> namedPrefab in m_namedPrefabs)
		{
			list.Add(((Object)namedPrefab.Value).name);
		}
		return list;
	}

	private void RPC_SpawnObject(long spawner, Vector3 pos, Quaternion rot, int prefabHash)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		GameObject prefab = GetPrefab(prefabHash);
		if ((Object)(object)prefab == (Object)null)
		{
			ZLog.Log((object)("Missing prefab " + prefabHash));
		}
		else
		{
			Object.Instantiate<GameObject>(prefab, pos, rot);
		}
	}
}
