using System.Collections.Generic;
using UnityEngine;

public class CraftingStation : MonoBehaviour, Hoverable, Interactable, IMonoUpdater
{
	public string m_name = "";

	public Sprite m_icon;

	public float m_discoverRange = 4f;

	public float m_rangeBuild = 10f;

	public float m_extraRangePerLevel;

	public bool m_craftRequireRoof = true;

	public bool m_craftRequireFire = true;

	public Transform m_roofCheckPoint;

	public Transform m_connectionPoint;

	public bool m_showBasicRecipies;

	public float m_useDistance = 2f;

	public Collider m_effectAreaCollider;

	public int m_useAnimation;

	public Skills.SkillType m_craftingSkill = Skills.SkillType.Crafting;

	public GameObject m_areaMarker;

	public GameObject m_inUseObject;

	public GameObject m_haveFireObject;

	public EffectList m_craftItemEffects = new EffectList();

	public EffectList m_craftItemDoneEffects = new EffectList();

	public EffectList m_repairItemDoneEffects = new EffectList();

	private const float m_updateExtensionInterval = 2f;

	private float m_updateExtensionTimer;

	private bool m_initialized;

	private float m_useTimer = 10f;

	private bool m_haveFire;

	private float m_buildRange;

	private ZNetView m_nview;

	private List<StationExtension> m_attachedExtensions = new List<StationExtension>();

	private static List<CraftingStation> m_allStations = new List<CraftingStation>();

	private static int m_triggerMask = 0;

	private CircleProjector m_areaMarkerCircle;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Start()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (!Object.op_Implicit((Object)(object)m_nview) || m_nview.GetZDO() != null)
		{
			m_allStations.Add(this);
			if (Object.op_Implicit((Object)(object)m_areaMarker))
			{
				m_areaMarker.SetActive(false);
				m_areaMarkerCircle = m_areaMarker.GetComponent<CircleProjector>();
			}
			if (m_craftRequireFire)
			{
				((MonoBehaviour)this).InvokeRepeating("CheckFire", 1f, 1f);
			}
			m_updateExtensionTimer = 2f;
		}
	}

	protected virtual void OnEnable()
	{
		Instances.Add(this);
	}

	protected virtual void OnDisable()
	{
		Instances.Remove(this);
	}

	private void OnDestroy()
	{
		m_allStations.Remove(this);
	}

	public bool Interact(Humanoid user, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if ((Object)(object)user == (Object)(object)Player.m_localPlayer)
		{
			if (!InUseDistance(user))
			{
				return false;
			}
			Player player = user as Player;
			if (CheckUsable(player, showMessage: true))
			{
				player.SetCraftingStation(this);
				InventoryGui.instance.Show(null, 3);
				return false;
			}
		}
		return false;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	public bool CheckUsable(Player player, bool showMessage)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (m_craftRequireRoof && !player.NoCostCheat())
		{
			float num = default(float);
			bool flag = default(bool);
			Cover.GetCoverForPoint(m_roofCheckPoint.position, ref num, ref flag, 0.5f);
			if (!flag)
			{
				if (showMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_stationneedroof");
				}
				return false;
			}
			if (num < 0.7f)
			{
				if (showMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_stationtooexposed");
				}
				return false;
			}
		}
		if (m_craftRequireFire && !player.NoCostCheat() && !m_haveFire)
		{
			if (showMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_needfire");
			}
			return false;
		}
		return true;
	}

	public string GetHoverText()
	{
		if (!InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=#888888>$piece_toofar</color>");
		}
		return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use ");
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public void ShowAreaMarker()
	{
		if (Object.op_Implicit((Object)(object)m_areaMarker))
		{
			m_areaMarker.SetActive(true);
			((MonoBehaviour)this).CancelInvoke("HideMarker");
			((MonoBehaviour)this).Invoke("HideMarker", 0.5f);
			PokeInUse();
		}
	}

	private void HideMarker()
	{
		m_areaMarker.SetActive(false);
	}

	public static void UpdateKnownStationsInRange(Player player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)player).transform.position;
		foreach (CraftingStation allStation in m_allStations)
		{
			if (Vector3.Distance(((Component)allStation).transform.position, position) < allStation.m_discoverRange)
			{
				player.AddKnownStation(allStation);
			}
		}
	}

	public void CustomUpdate(float deltaTime, float time)
	{
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid())
		{
			return;
		}
		m_useTimer += deltaTime;
		m_updateExtensionTimer += deltaTime;
		if (Object.op_Implicit((Object)(object)m_inUseObject))
		{
			bool flag = m_useTimer < 1f;
			if (m_inUseObject.activeSelf != flag)
			{
				m_inUseObject.SetActive(flag);
			}
		}
	}

	private void CheckFire()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		m_haveFire = EffectArea.IsPointPlus025InsideBurningArea(((Component)this).transform.position);
		if (Object.op_Implicit((Object)(object)m_haveFireObject))
		{
			m_haveFireObject.SetActive(m_haveFire);
		}
	}

	public void PokeInUse()
	{
		m_useTimer = 0f;
		TriggerExtensionEffects();
	}

	public static CraftingStation GetCraftingStation(Vector3 point)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (m_triggerMask == 0)
		{
			m_triggerMask = LayerMask.GetMask(new string[1] { "character_trigger" });
		}
		Collider[] array = Physics.OverlapSphere(point, 0.1f, m_triggerMask, (QueryTriggerInteraction)2);
		foreach (Collider val in array)
		{
			if (((Component)val).gameObject.CompareTag("StationUseArea"))
			{
				CraftingStation componentInParent = ((Component)val).GetComponentInParent<CraftingStation>();
				if ((Object)(object)componentInParent != (Object)null)
				{
					return componentInParent;
				}
			}
		}
		return null;
	}

	public static CraftingStation HaveBuildStationInRange(string name, Vector3 point)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		foreach (CraftingStation allStation in m_allStations)
		{
			if (!(allStation.m_name != name))
			{
				float stationBuildRange = allStation.GetStationBuildRange();
				point.y = ((Component)allStation).transform.position.y;
				if (Vector3.Distance(((Component)allStation).transform.position, point) < stationBuildRange)
				{
					return allStation;
				}
			}
		}
		return null;
	}

	public static void FindStationsInRange(string name, Vector3 point, float range, List<CraftingStation> stations)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		foreach (CraftingStation allStation in m_allStations)
		{
			if (!(allStation.m_name != name) && Vector3.Distance(((Component)allStation).transform.position, point) < range)
			{
				stations.Add(allStation);
			}
		}
	}

	public static CraftingStation FindClosestStationInRange(string name, Vector3 point, float range)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		CraftingStation craftingStation = null;
		float num = 99999f;
		foreach (CraftingStation allStation in m_allStations)
		{
			if (!(allStation.m_name != name))
			{
				float num2 = Vector3.Distance(((Component)allStation).transform.position, point);
				if (num2 < range && (num2 < num || (Object)(object)craftingStation == (Object)null))
				{
					craftingStation = allStation;
					num = num2;
				}
			}
		}
		return craftingStation;
	}

	private List<StationExtension> GetExtensions()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (m_updateExtensionTimer >= 2f)
		{
			m_updateExtensionTimer = 0f;
			m_attachedExtensions.Clear();
			StationExtension.FindExtensions(this, ((Component)this).transform.position, m_attachedExtensions);
			m_buildRange = m_rangeBuild + (float)GetExtentionCount(checkExtensions: false) * m_extraRangePerLevel;
			if (Object.op_Implicit((Object)(object)m_areaMarker))
			{
				m_areaMarkerCircle.m_radius = m_buildRange;
			}
			if ((Object)(object)m_effectAreaCollider == (Object)null)
			{
				return m_attachedExtensions;
			}
			Collider effectAreaCollider = m_effectAreaCollider;
			SphereCollider val = (SphereCollider)(object)((effectAreaCollider is SphereCollider) ? effectAreaCollider : null);
			if (val == null)
			{
				CapsuleCollider val2 = (CapsuleCollider)(object)((effectAreaCollider is CapsuleCollider) ? effectAreaCollider : null);
				if (val2 != null)
				{
					val2.radius = m_buildRange;
				}
			}
			else
			{
				val.radius = m_buildRange;
			}
		}
		return m_attachedExtensions;
	}

	private void TriggerExtensionEffects()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector3 connectionEffectPoint = GetConnectionEffectPoint();
		foreach (StationExtension extension in GetExtensions())
		{
			if (Object.op_Implicit((Object)(object)extension))
			{
				extension.StartConnectionEffect(connectionEffectPoint);
			}
		}
	}

	public Vector3 GetConnectionEffectPoint()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_connectionPoint))
		{
			return m_connectionPoint.position;
		}
		return ((Component)this).transform.position;
	}

	public int GetLevel(bool checkExtensions = true)
	{
		return 1 + GetExtentionCount(checkExtensions);
	}

	public int GetExtentionCount(bool checkExtensions = true)
	{
		if (checkExtensions)
		{
			GetExtensions();
		}
		return m_attachedExtensions.Count;
	}

	public float GetStationBuildRange()
	{
		GetExtensions();
		return m_buildRange;
	}

	public bool InUseDistance(Humanoid human)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)human).transform.position, ((Component)this).transform.position) < m_useDistance;
	}
}
