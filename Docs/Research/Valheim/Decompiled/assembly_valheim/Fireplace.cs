using System;
using System.Collections.Generic;
using UnityEngine;

public class Fireplace : MonoBehaviour, Hoverable, Interactable, IHasHoverMenu
{
	[Serializable]
	public struct FireworkItem
	{
		public ItemDrop m_fireworkItem;

		public int m_fireworkItemCount;

		public EffectList m_fireworksEffects;
	}

	private ZNetView m_nview;

	private Piece m_piece;

	[Header("Fire")]
	public string m_name = "Fire";

	public float m_startFuel = 3f;

	public float m_maxFuel = 10f;

	public float m_secPerFuel = 3f;

	public bool m_infiniteFuel;

	public bool m_disableCoverCheck;

	public float m_checkTerrainOffset = 0.2f;

	public float m_coverCheckOffset = 0.5f;

	private const float m_minimumOpenSpace = 0.5f;

	public float m_holdRepeatInterval = 0.2f;

	public float m_halfThreshold = 0.5f;

	public bool m_canTurnOff;

	public bool m_canRefill = true;

	public bool m_lowWetOverHalf = true;

	public GameObject m_enabledObject;

	public GameObject m_enabledObjectLow;

	public GameObject m_enabledObjectHigh;

	public GameObject m_fullObject;

	public GameObject m_halfObject;

	public GameObject m_emptyObject;

	public GameObject m_playerBaseObject;

	public ItemDrop m_fuelItem;

	public SmokeSpawner m_smokeSpawner;

	public EffectList m_fuelAddedEffects = new EffectList();

	public EffectList m_toggleOnEffects = new EffectList();

	[Header("Fireworks")]
	[Range(0f, 60f)]
	public float m_fireworksMaxRandomAngle = 5f;

	public FireworkItem[] m_fireworkItemList;

	[Header("Ignite Pieces")]
	public float m_igniteInterval;

	public float m_igniteChance;

	public int m_igniteSpread = 4;

	public float m_igniteCapsuleRadius;

	public Vector3 m_igniteCapsuleStart;

	public Vector3 m_igniteCapsuleEnd;

	public GameObject m_firePrefab;

	private bool m_blocked;

	private bool m_wet;

	private Heightmap.Biome m_biome;

	private float m_lastUseTime;

	private bool m_checkWaterLevel;

	private WaterVolume m_previousWaterVolume;

	private static int m_solidRayMask = 0;

	private static Collider[] s_tempColliders = (Collider[])(object)new Collider[20];

	public void Awake()
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).gameObject.GetComponent<ZNetView>();
		m_piece = ((Component)this).gameObject.GetComponent<Piece>();
		if (m_nview.GetZDO() == null)
		{
			return;
		}
		if (m_solidRayMask == 0)
		{
			m_solidRayMask = LayerMask.GetMask(new string[5] { "Default", "static_solid", "Default_small", "piece", "terrain" });
		}
		if (m_nview.IsOwner() && m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, -1f) == -1f)
		{
			m_nview.GetZDO().Set(ZDOVars.s_fuel, m_startFuel);
			if (m_startFuel > 0f)
			{
				m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
		}
		Vector3 p = (Object.op_Implicit((Object)(object)m_enabledObject) ? m_enabledObject.transform.position : ((Component)this).transform.position);
		p.y -= 15f;
		m_checkWaterLevel = Floating.IsUnderWater(p, ref m_previousWaterVolume);
		m_nview.Register("RPC_AddFuel", RPC_AddFuel);
		m_nview.Register<float>("RPC_AddFuelAmount", RPC_AddFuelAmount);
		m_nview.Register<float>("RPC_SetFuelAmount", RPC_SetFuelAmount);
		m_nview.Register("RPC_ToggleOn", RPC_ToggleOn);
		((MonoBehaviour)this).InvokeRepeating("UpdateFireplace", 0f, 2f);
		((MonoBehaviour)this).InvokeRepeating("CheckEnv", 4f, 4f);
		if (m_igniteInterval > 0f && m_igniteCapsuleRadius > 0f)
		{
			((MonoBehaviour)this).InvokeRepeating("UpdateIgnite", m_igniteInterval, m_igniteInterval);
		}
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)m_playerBaseObject) && Object.op_Implicit((Object)(object)m_piece))
		{
			m_playerBaseObject.SetActive(m_piece.IsPlacedByPlayer());
		}
	}

	private double GetTimeSinceLastUpdate()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime dateTime = new DateTime(m_nview.GetZDO().GetLong(ZDOVars.s_lastTime, time.Ticks));
		TimeSpan timeSpan = time - dateTime;
		m_nview.GetZDO().Set(ZDOVars.s_lastTime, time.Ticks);
		double num = timeSpan.TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return num;
	}

	private void UpdateFireplace()
	{
		if (!m_nview.IsValid())
		{
			return;
		}
		if (m_nview.IsOwner() && m_secPerFuel > 0f)
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
			double timeSinceLastUpdate = GetTimeSinceLastUpdate();
			bool flag = m_nview.GetZDO().GetInt(ZDOVars.s_state, 1) == 1;
			if (IsBurning() && !m_infiniteFuel && flag)
			{
				float num = (float)(timeSinceLastUpdate / (double)m_secPerFuel);
				@float -= num;
				if (@float <= 0f)
				{
					@float = 0f;
				}
				m_nview.GetZDO().Set(ZDOVars.s_fuel, @float);
			}
		}
		UpdateState();
	}

	private void CheckEnv()
	{
		CheckUnderTerrain();
		if ((Object)(object)m_enabledObjectLow != (Object)null && (Object)(object)m_enabledObjectHigh != (Object)null)
		{
			CheckWet();
		}
	}

	private void CheckUnderTerrain()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		m_blocked = false;
		if (!m_disableCoverCheck)
		{
			RaycastHit val = default(RaycastHit);
			if (Heightmap.GetHeight(((Component)this).transform.position, out var height) && height > ((Component)this).transform.position.y + m_checkTerrainOffset)
			{
				m_blocked = true;
			}
			else if (Physics.Raycast(((Component)this).transform.position + Vector3.up * m_coverCheckOffset, Vector3.up, ref val, 0.5f, m_solidRayMask))
			{
				m_blocked = true;
			}
			else if (Object.op_Implicit((Object)(object)m_smokeSpawner) && m_smokeSpawner.IsBlocked())
			{
				m_blocked = true;
			}
		}
	}

	private void CheckWet()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		m_wet = false;
		bool flag = EnvMan.instance.GetWindIntensity() >= 0.8f;
		bool flag2 = EnvMan.IsWet();
		if (flag || flag2)
		{
			float num = default(float);
			bool flag3 = default(bool);
			Cover.GetCoverForPoint(((Component)this).transform.position + Vector3.up * m_coverCheckOffset, ref num, ref flag3, 0.5f);
			if (flag && num < 0.7f)
			{
				m_wet = true;
			}
			else if (flag2 && !flag3)
			{
				m_wet = true;
			}
		}
	}

	private void UpdateState()
	{
		float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
		bool flag = @float >= m_halfThreshold;
		bool flag2 = @float <= 0f;
		if (m_lowWetOverHalf)
		{
			_ = !m_wet;
		}
		else
			_ = 0;
		if (IsBurning())
		{
			if (Object.op_Implicit((Object)(object)m_enabledObject))
			{
				m_enabledObject.SetActive(true);
			}
			if (Object.op_Implicit((Object)(object)m_enabledObjectHigh) && Object.op_Implicit((Object)(object)m_enabledObjectLow))
			{
				if (m_enabledObjectHigh.activeSelf != !m_wet)
				{
					m_enabledObjectHigh.SetActive(!m_wet);
				}
				if (m_enabledObjectLow.activeSelf != m_wet)
				{
					m_enabledObjectLow.SetActive(m_wet);
				}
			}
			if (m_canTurnOff && m_wet && m_nview.IsOwner() && m_nview.GetZDO().GetInt(ZDOVars.s_state, 1) == 1)
			{
				m_nview.InvokeRPC("RPC_ToggleOn");
			}
		}
		else
		{
			if (Object.op_Implicit((Object)(object)m_enabledObject))
			{
				m_enabledObject.SetActive(false);
			}
			if (Object.op_Implicit((Object)(object)m_enabledObjectHigh) && Object.op_Implicit((Object)(object)m_enabledObjectLow))
			{
				if (m_enabledObjectLow.activeSelf)
				{
					m_enabledObjectLow.SetActive(false);
				}
				if (m_enabledObjectHigh.activeSelf)
				{
					m_enabledObjectHigh.SetActive(false);
				}
			}
		}
		if (Object.op_Implicit((Object)(object)m_fullObject) && Object.op_Implicit((Object)(object)m_halfObject))
		{
			m_fullObject.SetActive(flag);
			m_halfObject.SetActive(!flag);
		}
		if (!Object.op_Implicit((Object)(object)m_emptyObject))
		{
			return;
		}
		if (flag2)
		{
			if (Object.op_Implicit((Object)(object)m_fullObject) && m_fullObject.activeSelf)
			{
				m_fullObject.SetActive(false);
			}
			if (Object.op_Implicit((Object)(object)m_halfObject) && m_halfObject.activeSelf)
			{
				m_halfObject.SetActive(false);
			}
		}
		if (m_emptyObject.activeSelf != flag2)
		{
			m_emptyObject.SetActive(flag2);
		}
	}

	public string GetHoverText()
	{
		if (!m_nview.IsValid() || m_infiniteFuel)
		{
			return "";
		}
		float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
		string text = m_name;
		if (m_canRefill)
		{
			text += $"\n( $piece_fire_fuel {Mathf.Ceil(@float)}/{(int)m_maxFuel} )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use {m_fuelItem.m_itemData.m_shared.m_name}\n[<color=yellow><b>1-8</b></color>] $piece_useitem";
		}
		else if (m_canTurnOff && @float > 0f)
		{
			text += "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use";
		}
		return Localization.instance.Localize(text);
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public void AddFuel(float fuel)
	{
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid())
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
			if ((fuel < 0f && @float > 0f) || (fuel > 0f && @float < m_maxFuel))
			{
				m_nview.InvokeRPC("RPC_AddFuelAmount", fuel);
			}
		}
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (hold)
		{
			if (m_holdRepeatInterval <= 0f)
			{
				return false;
			}
			if (Time.time - m_lastUseTime < m_holdRepeatInterval)
			{
				return false;
			}
		}
		if (!m_nview.HasOwner())
		{
			m_nview.ClaimOwnership();
		}
		float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
		if (m_canTurnOff && !hold && !alt && @float > 0f)
		{
			m_nview.InvokeRPC("RPC_ToggleOn");
			return true;
		}
		if (m_canRefill)
		{
			Inventory inventory = user.GetInventory();
			if (inventory != null)
			{
				if (m_infiniteFuel)
				{
					return false;
				}
				if (inventory.HaveItem(m_fuelItem.m_itemData.m_shared.m_name))
				{
					if ((float)Mathf.CeilToInt(@float) >= m_maxFuel)
					{
						user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", new string[1] { m_fuelItem.m_itemData.m_shared.m_name }));
						return false;
					}
					user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[1] { m_fuelItem.m_itemData.m_shared.m_name }));
					inventory.RemoveItem(m_fuelItem.m_itemData.m_shared.m_name, 1);
					m_nview.InvokeRPC("RPC_AddFuel");
					return true;
				}
				user.Message(MessageHud.MessageType.Center, "$msg_outof " + m_fuelItem.m_itemData.m_shared.m_name);
				return false;
			}
		}
		return false;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		if (!m_canRefill)
		{
			return false;
		}
		if (item.m_shared.m_name == m_fuelItem.m_itemData.m_shared.m_name && !m_infiniteFuel)
		{
			if ((float)Mathf.CeilToInt(m_nview.GetZDO().GetFloat(ZDOVars.s_fuel)) >= m_maxFuel)
			{
				user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", new string[1] { item.m_shared.m_name }));
				return true;
			}
			Inventory inventory = user.GetInventory();
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[1] { item.m_shared.m_name }));
			inventory.RemoveItem(item, 1);
			m_nview.InvokeRPC("RPC_AddFuel");
			return true;
		}
		for (int i = 0; i < m_fireworkItemList.Length; i++)
		{
			if (item.m_shared.m_name == m_fireworkItemList[i].m_fireworkItem.m_itemData.m_shared.m_name)
			{
				if (!IsBurning())
				{
					user.Message(MessageHud.MessageType.Center, "$msg_firenotburning");
					return true;
				}
				if (user.GetInventory().CountItems(m_fireworkItemList[i].m_fireworkItem.m_itemData.m_shared.m_name) < m_fireworkItemList[i].m_fireworkItemCount)
				{
					user.Message(MessageHud.MessageType.Center, "$msg_toofew " + m_fireworkItemList[i].m_fireworkItem.m_itemData.m_shared.m_name);
					return true;
				}
				user.GetInventory().RemoveItem(item.m_shared.m_name, m_fireworkItemList[i].m_fireworkItemCount);
				user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_throwinfire", new string[1] { item.m_shared.m_name }));
				float num = Random.Range(0f - m_fireworksMaxRandomAngle, m_fireworksMaxRandomAngle);
				float num2 = Random.Range(0f - m_fireworksMaxRandomAngle, m_fireworksMaxRandomAngle);
				Quaternion baseRot = Quaternion.Euler(num, 0f, num2);
				m_fireworkItemList[i].m_fireworksEffects.Create(((Component)this).transform.position, baseRot);
				m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				return true;
			}
		}
		return false;
	}

	private void RPC_AddFuel(long sender)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
			if (!((float)Mathf.CeilToInt(@float) >= m_maxFuel))
			{
				@float = Mathf.Clamp(@float, 0f, m_maxFuel);
				@float += 1f;
				@float = Mathf.Clamp(@float, 0f, m_maxFuel);
				m_nview.GetZDO().Set(ZDOVars.s_fuel, @float);
				m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				UpdateState();
			}
		}
	}

	private void RPC_ToggleOn(long sender)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			bool flag = m_nview.GetZDO().GetInt(ZDOVars.s_state, 1) == 1;
			m_nview.GetZDO().Set(ZDOVars.s_state, (!flag) ? 1 : 2);
			m_toggleOnEffects.Create(((Component)this).transform.position, Quaternion.identity, null, 1f, (!flag) ? 1 : 2);
		}
		UpdateState();
	}

	private void RPC_AddFuelAmount(long sender, float amount)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
			@float = Mathf.Clamp(@float + amount, 0f, m_maxFuel);
			m_nview.GetZDO().Set(ZDOVars.s_fuel, @float);
			m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			UpdateState();
		}
	}

	public void SetFuel(float fuel)
	{
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid())
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
			fuel = Mathf.Clamp(fuel, 0f, m_maxFuel);
			if (fuel != @float)
			{
				m_nview.InvokeRPC("RPC_SetFuelAmount", fuel);
			}
		}
	}

	private void RPC_SetFuelAmount(long sender, float fuel)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_fuel, fuel);
			m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			UpdateState();
		}
	}

	public bool CanBeRemoved()
	{
		return !IsBurning();
	}

	public bool IsBurning()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (m_blocked)
		{
			return false;
		}
		if (m_nview.GetZDO().GetInt(ZDOVars.s_state, 1) != 1)
		{
			return false;
		}
		if (m_checkWaterLevel && Floating.IsUnderWater(Object.op_Implicit((Object)(object)m_enabledObject) ? m_enabledObject.transform.position : ((Component)this).transform.position, ref m_previousWaterVolume))
		{
			return false;
		}
		if (!(m_nview.GetZDO().GetFloat(ZDOVars.s_fuel) > 0f))
		{
			return m_infiniteFuel;
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(((Component)this).transform.position + Vector3.up * m_coverCheckOffset, 0.5f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(((Component)this).transform.position + Vector3.up * m_checkTerrainOffset, new Vector3(1f, 0.01f, 1f));
		Gizmos.color = Color.red;
		Utils.DrawGizmoCapsule(((Component)this).transform.position + m_igniteCapsuleStart, ((Component)this).transform.position + m_igniteCapsuleEnd, m_igniteCapsuleRadius);
	}

	private void UpdateIgnite()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner() || !Object.op_Implicit((Object)(object)m_firePrefab) || !CanIgnite() || !IsBurning())
		{
			return;
		}
		int num = Physics.OverlapCapsuleNonAlloc(((Component)this).transform.position + m_igniteCapsuleStart, ((Component)this).transform.position + m_igniteCapsuleEnd, m_igniteCapsuleRadius, s_tempColliders);
		for (int i = 0; i < num; i++)
		{
			Collider val = s_tempColliders[i];
			if (!((Object)(object)((Component)val).gameObject == (Object)(object)((Component)this).gameObject) && (!((Object)(object)((Component)val).transform.parent != (Object)null) || !((Object)(object)((Component)((Component)val).transform.parent).gameObject == (Object)(object)((Component)this).gameObject)) && !val.isTrigger && Random.Range(0f, 1f) <= m_igniteChance && Cinder.CanBurn(val, ((Component)val).transform.position, out var _))
			{
				Object.Instantiate<GameObject>(m_firePrefab, ((Component)val).transform.position + Utils.RandomVector3(-0.1f, 0.1f), Quaternion.identity).GetComponent<CinderSpawner>()?.Setup(m_igniteSpread, ((Component)val).gameObject);
			}
		}
	}

	public bool TryGetItems(Player player, out List<string> items)
	{
		items = new List<string>();
		if (m_infiniteFuel)
		{
			return false;
		}
		if (!CanUseItems(player))
		{
			return true;
		}
		items.Add(m_fuelItem.m_itemData.m_shared.m_name);
		return true;
	}

	public bool CanUseItems(Player player, bool sendErrorMessage = true)
	{
		if (m_infiniteFuel)
		{
			return false;
		}
		if (!player.GetInventory().HaveItem(m_fuelItem.m_itemData.m_shared.m_name))
		{
			if (sendErrorMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_outof " + m_fuelItem.m_itemData.m_shared.m_name);
			}
			return false;
		}
		if (!((float)Mathf.CeilToInt(m_nview.GetZDO().GetFloat(ZDOVars.s_fuel)) >= m_maxFuel))
		{
			return true;
		}
		if (sendErrorMessage)
		{
			player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", new string[1] { m_fuelItem.m_itemData.m_shared.m_name }));
		}
		return false;
	}

	public bool CanIgnite()
	{
		return CinderSpawner.CanSpawnCinder(((Component)this).transform, ref m_biome);
	}
}
