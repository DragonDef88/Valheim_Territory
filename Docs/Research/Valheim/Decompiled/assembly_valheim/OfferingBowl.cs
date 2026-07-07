using System.Collections.Generic;
using UnityEngine;

public class OfferingBowl : MonoBehaviour, Hoverable, Interactable
{
	[Header("Tokens")]
	public string m_name = "$piece_offerbowl";

	public string m_useItemText = "$piece_offerbowl_offeritem";

	public string m_usedAltarText = "$msg_offerdone";

	public string m_cantOfferText = "$msg_cantoffer";

	public string m_wrongOfferText = "$msg_offerwrong";

	public string m_incompleteOfferText = "$msg_incompleteoffering";

	[Header("Settings")]
	public ItemDrop m_bossItem;

	public int m_bossItems = 1;

	public GameObject m_bossPrefab;

	public ItemDrop m_itemPrefab;

	public Transform m_itemSpawnPoint;

	public string m_setGlobalKey = "";

	public bool m_renderSpawnAreaGizmos;

	public bool m_alertOnSpawn;

	[Header("Boss")]
	public float m_spawnBossDelay = 5f;

	public float m_spawnBossMaxDistance = 40f;

	public float m_spawnBossMinDistance;

	public float m_spawnBossMaxYDistance = 9999f;

	public int m_getSolidHeightMargin = 1000;

	public bool m_enableSolidHeightCheck = true;

	public float m_spawnPointClearingRadius;

	public float m_spawnYOffset = 1f;

	public Vector3 m_spawnAreaOffset;

	public List<GameObject> m_spawnPoints = new List<GameObject>();

	[Header("Use itemstands")]
	public bool m_useItemStands;

	public string m_itemStandPrefix = "";

	public float m_itemstandMaxRange = 20f;

	[Header("Effects")]
	public EffectList m_fuelAddedEffects = new EffectList();

	public EffectList m_spawnBossStartEffects = new EffectList();

	public EffectList m_spawnBossDoneffects = new EffectList();

	private Vector3 m_bossSpawnPoint;

	private int m_solidRayMask;

	private static readonly Collider[] s_tempColliders = (Collider[])(object)new Collider[1];

	private ZNetView m_nview;

	private Humanoid m_interactUser;

	private ItemDrop.ItemData m_usedSpawnItem;

	private void Awake()
	{
		m_solidRayMask = LayerMask.GetMask(new string[4] { "Default", "static_solid", "Default_small", "piece" });
	}

	private void Start()
	{
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			m_nview.Register<Vector3, bool>("RPC_SpawnBoss", RPC_SpawnBoss);
			m_nview.Register("RPC_BossSpawnInitiated", RPC_BossSpawnInitiated);
			m_nview.Register("RPC_RemoveBossSpawnInventoryItems", RPC_RemoveBossSpawnInventoryItems);
		}
	}

	public string GetHoverText()
	{
		if (m_useItemStands)
		{
			return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " + m_useItemText);
		}
		return Localization.instance.Localize(m_name + "\n[<color=yellow><b>1-8</b></color>] " + m_useItemText);
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (hold || IsBossSpawnQueued() || !m_useItemStands)
		{
			return false;
		}
		foreach (ItemStand item in FindItemStands())
		{
			if (!item.HaveAttachment())
			{
				user.Message(MessageHud.MessageType.Center, m_incompleteOfferText);
				return false;
			}
		}
		m_interactUser = user;
		InitiateSpawnBoss(GetSpawnPosition(), removeItemsFromInventory: false);
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		if (m_useItemStands)
		{
			return false;
		}
		if (IsBossSpawnQueued())
		{
			return true;
		}
		if ((Object)(object)m_bossItem != (Object)null)
		{
			if (item.m_shared.m_name == m_bossItem.m_itemData.m_shared.m_name)
			{
				int num = user.GetInventory().CountItems(m_bossItem.m_itemData.m_shared.m_name);
				if (num < m_bossItems)
				{
					if (num == 0 && Game.m_worldLevel > 0 && user.GetInventory().CountItems(m_bossItem.m_itemData.m_shared.m_name, -1, matchWorldLevel: false) > 0)
					{
						user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_ng_the_x") + item.m_shared.m_name + Localization.instance.Localize("$msg_ng_x_is_too_low"));
					}
					else
					{
						user.Message(MessageHud.MessageType.Center, $"{m_incompleteOfferText}: {m_bossItem.m_itemData.m_shared.m_name} {num.ToString()} / {m_bossItems}");
					}
					return true;
				}
				if ((Object)(object)m_bossPrefab != (Object)null)
				{
					m_usedSpawnItem = item;
					m_interactUser = user;
					InitiateSpawnBoss(GetSpawnPosition(), removeItemsFromInventory: true);
				}
				else if ((Object)(object)m_itemPrefab != (Object)null && SpawnItem(m_itemPrefab, user as Player))
				{
					user.GetInventory().RemoveItem(item.m_shared.m_name, m_bossItems);
					user.ShowRemovedMessage(m_bossItem.m_itemData, m_bossItems);
					user.Message(MessageHud.MessageType.Center, m_usedAltarText);
					m_fuelAddedEffects.Create(m_itemSpawnPoint.position, ((Component)this).transform.rotation);
				}
				if (!string.IsNullOrEmpty(m_setGlobalKey))
				{
					ZoneSystem.instance.SetGlobalKey(m_setGlobalKey);
				}
				return true;
			}
			user.Message(MessageHud.MessageType.Center, m_wrongOfferText);
			return true;
		}
		return false;
	}

	private bool SpawnItem(ItemDrop item, Player player)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (item.m_itemData.m_shared.m_questItem && player.HaveUniqueKey(item.m_itemData.m_shared.m_name))
		{
			player.Message(MessageHud.MessageType.Center, m_cantOfferText);
			return false;
		}
		Object.Instantiate<ItemDrop>(item, m_itemSpawnPoint.position, Quaternion.identity);
		return true;
	}

	private Vector3 GetSpawnPosition()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (m_spawnPoints.Count > 0)
		{
			return m_spawnPoints[Random.Range(0, m_spawnPoints.Count)].transform.position;
		}
		Vector3 val = Vector4.op_Implicit(((Component)this).transform.localToWorldMatrix * Vector4.op_Implicit(m_spawnAreaOffset));
		return ((Component)this).transform.position + val;
	}

	private void InitiateSpawnBoss(Vector3 point, bool removeItemsFromInventory)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		m_nview.InvokeRPC("RPC_SpawnBoss", point, removeItemsFromInventory);
	}

	private void RPC_SpawnBoss(long senderId, Vector3 point, bool removeItemsFromInventory)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && !IsBossSpawnQueued() && CanSpawnBoss(point, out var spawnPoint) && (Object.op_Implicit((Object)(object)m_nview) || !m_nview.IsValid()))
		{
			SpawnBoss(spawnPoint);
			m_nview.InvokeRPC(senderId, "RPC_BossSpawnInitiated");
			if (removeItemsFromInventory)
			{
				m_nview.InvokeRPC(senderId, "RPC_RemoveBossSpawnInventoryItems");
			}
			else
			{
				RemoveAltarItems();
			}
		}
	}

	private void SpawnBoss(Vector3 spawnPoint)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).Invoke("DelayedSpawnBoss", m_spawnBossDelay);
		m_spawnBossStartEffects.Create(spawnPoint, Quaternion.identity);
		m_bossSpawnPoint = spawnPoint;
	}

	private void RPC_RemoveBossSpawnInventoryItems(long senderId)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		m_interactUser.GetInventory().RemoveItem(m_usedSpawnItem.m_shared.m_name, m_bossItems);
		m_interactUser.ShowRemovedMessage(m_bossItem.m_itemData, m_bossItems);
		m_interactUser.Message(MessageHud.MessageType.Center, m_usedAltarText);
		if (Object.op_Implicit((Object)(object)m_itemSpawnPoint))
		{
			m_fuelAddedEffects.Create(m_itemSpawnPoint.position, ((Component)this).transform.rotation);
		}
	}

	private void RemoveAltarItems()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		foreach (ItemStand item in FindItemStands())
		{
			item.DestroyAttachment();
		}
		if (Object.op_Implicit((Object)(object)m_itemSpawnPoint))
		{
			m_fuelAddedEffects.Create(m_itemSpawnPoint.position, ((Component)this).transform.rotation);
		}
	}

	private void RPC_BossSpawnInitiated(long senderId)
	{
		m_interactUser.Message(MessageHud.MessageType.Center, m_usedAltarText);
	}

	private bool CanSpawnBoss(Vector3 point, out Vector3 spawnPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		spawnPoint = Vector3.zero;
		for (int i = 0; i < 100; i++)
		{
			Vector2 val = Random.insideUnitCircle * m_spawnBossMaxDistance;
			spawnPoint = point + new Vector3(val.x, 0f, val.y);
			if (m_enableSolidHeightCheck)
			{
				ZoneSystem.instance.GetSolidHeight(spawnPoint, out var height, m_getSolidHeightMargin);
				if (height < 0f || Mathf.Abs(height - ((Component)this).transform.position.y) > m_spawnBossMaxYDistance || Vector3.Distance(spawnPoint, point) < m_spawnBossMinDistance)
				{
					continue;
				}
				if (m_spawnPointClearingRadius > 0f)
				{
					spawnPoint.y = height + m_spawnYOffset;
					int num = Physics.OverlapSphereNonAlloc(spawnPoint, m_spawnPointClearingRadius, (Collider[])null, m_solidRayMask);
					if (num > 0)
					{
						ZLog.Log((object)num);
						continue;
					}
				}
				spawnPoint.y = height + m_spawnYOffset;
			}
			return true;
		}
		return false;
	}

	private bool IsBossSpawnQueued()
	{
		return ((MonoBehaviour)this).IsInvoking("DelayedSpawnBoss");
	}

	private void DelayedSpawnBoss()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(m_bossPrefab, m_bossSpawnPoint, Quaternion.identity);
		BaseAI component = val.GetComponent<BaseAI>();
		if ((Object)(object)component != (Object)null)
		{
			component.SetPatrolPoint();
			if (m_alertOnSpawn)
			{
				component.Alert();
			}
		}
		GameObject[] array = m_spawnBossDoneffects.Create(m_bossSpawnPoint, Quaternion.identity);
		for (int i = 0; i < array.Length; i++)
		{
			IProjectile[] componentsInChildren = array[i].GetComponentsInChildren<IProjectile>();
			if (componentsInChildren.Length != 0)
			{
				IProjectile[] array2 = componentsInChildren;
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].Setup(val.GetComponent<Character>(), Vector3.zero, -1f, null, null, null);
				}
			}
		}
	}

	private List<ItemStand> FindItemStands()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		List<ItemStand> list = new List<ItemStand>();
		ItemStand[] array = Object.FindObjectsOfType<ItemStand>();
		foreach (ItemStand itemStand in array)
		{
			if (!(Vector3.Distance(((Component)this).transform.position, ((Component)itemStand).transform.position) > m_itemstandMaxRange) && Utils.CustomStartsWith(((Object)((Component)itemStand).gameObject).name, m_itemStandPrefix))
			{
				list.Add(itemStand);
			}
		}
		return list;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (m_renderSpawnAreaGizmos)
		{
			Gizmos.color = Color.green;
			Utils.DrawGizmoCylinder(GetSpawnPosition(), m_spawnBossMaxDistance, m_spawnBossMaxYDistance, 32);
			Gizmos.color = Color.red;
			if (m_spawnBossMinDistance > 0f)
			{
				Utils.DrawGizmoCylinder(GetSpawnPosition(), m_spawnBossMinDistance, m_spawnBossMaxYDistance, 32);
			}
		}
	}
}
