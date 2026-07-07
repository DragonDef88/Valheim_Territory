using System;
using UnityEngine;

public class PickableItem : MonoBehaviour, Hoverable, Interactable
{
	[Serializable]
	public struct RandomItem
	{
		public ItemDrop m_itemPrefab;

		public int m_stackMin;

		public int m_stackMax;
	}

	public ItemDrop m_itemPrefab;

	public int m_stack;

	public RandomItem[] m_randomItemPrefabs = Array.Empty<RandomItem>();

	public EffectList m_pickEffector = new EffectList();

	private ZNetView m_nview;

	private GameObject m_instance;

	private bool m_picked;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.GetZDO() != null)
		{
			SetupRandomPrefab();
			m_nview.Register("Pick", RPC_Pick);
			SetupItem(enabled: true);
		}
	}

	private void SetupRandomPrefab()
	{
		if (!((Object)(object)m_itemPrefab == (Object)null) || m_randomItemPrefabs.Length == 0)
		{
			return;
		}
		int @int = m_nview.GetZDO().GetInt(ZDOVars.s_itemPrefab);
		if (@int == 0)
		{
			if (m_nview.IsOwner())
			{
				RandomItem randomItem = m_randomItemPrefabs[Random.Range(0, m_randomItemPrefabs.Length)];
				m_itemPrefab = randomItem.m_itemPrefab;
				m_stack = Game.instance.ScaleDrops(randomItem.m_itemPrefab.m_itemData, randomItem.m_stackMin, randomItem.m_stackMax + 1);
				int prefabHash = ObjectDB.instance.GetPrefabHash(((Component)m_itemPrefab).gameObject);
				m_nview.GetZDO().Set(ZDOVars.s_itemPrefab, prefabHash);
				m_nview.GetZDO().Set(ZDOVars.s_itemStack, m_stack);
			}
		}
		else
		{
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(@int);
			if ((Object)(object)itemPrefab == (Object)null)
			{
				ZLog.LogError((object)("Failed to find saved prefab " + @int + " in PickableItem " + ((Object)((Component)this).gameObject).name));
				return;
			}
			m_itemPrefab = itemPrefab.GetComponent<ItemDrop>();
			m_stack = m_nview.GetZDO().GetInt(ZDOVars.s_itemStack);
		}
	}

	public string GetHoverText()
	{
		if (m_picked)
		{
			return "";
		}
		return Localization.instance.Localize(GetHoverName() + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	public string GetHoverName()
	{
		if (Object.op_Implicit((Object)(object)m_itemPrefab))
		{
			int stackSize = GetStackSize();
			if (stackSize > 1)
			{
				return m_itemPrefab.m_itemData.m_shared.m_name + " x " + stackSize;
			}
			return m_itemPrefab.m_itemData.m_shared.m_name;
		}
		return "None";
	}

	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		m_nview.InvokeRPC("Pick");
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void RPC_Pick(long sender)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && !m_picked)
		{
			m_picked = true;
			m_pickEffector.Create(((Component)this).transform.position, Quaternion.identity);
			Drop();
			m_nview.Destroy();
		}
	}

	private void Drop()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position + Vector3.up * 0.2f;
		GameObject obj = Object.Instantiate<GameObject>(((Component)m_itemPrefab).gameObject, val, ((Component)this).transform.rotation);
		ItemDrop component = obj.GetComponent<ItemDrop>();
		if (component != null)
		{
			component.m_itemData.m_stack = GetStackSize();
			ItemDrop.OnCreateNew(component);
		}
		obj.GetComponent<Rigidbody>().linearVelocity = Vector3.up * 4f;
	}

	private int GetStackSize()
	{
		return Mathf.Clamp((m_stack > 0) ? m_stack : m_itemPrefab.m_itemData.m_stack, 1, (int)Math.Round((float)m_itemPrefab.m_itemData.m_shared.m_maxStackSize * Game.m_resourceRate));
	}

	private GameObject GetAttachPrefab()
	{
		Transform val = ((Component)m_itemPrefab).transform.Find("attach");
		if (Object.op_Implicit((Object)(object)val))
		{
			return ((Component)val).gameObject;
		}
		return null;
	}

	private void SetupItem(bool enabled)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (!enabled)
		{
			if (Object.op_Implicit((Object)(object)m_instance))
			{
				Object.Destroy((Object)(object)m_instance);
				m_instance = null;
			}
		}
		else if (!Object.op_Implicit((Object)(object)m_instance) && !((Object)(object)m_itemPrefab == (Object)null))
		{
			GameObject attachPrefab = GetAttachPrefab();
			if ((Object)(object)attachPrefab == (Object)null)
			{
				ZLog.LogWarning((object)("Failed to get attach prefab for item " + ((Object)m_itemPrefab).name));
				return;
			}
			m_instance = Object.Instantiate<GameObject>(attachPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
			m_instance.transform.localPosition = attachPrefab.transform.localPosition;
			m_instance.transform.localRotation = attachPrefab.transform.localRotation;
		}
	}

	private bool DrawPrefabMesh(ItemDrop prefab)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)prefab == (Object)null)
		{
			return false;
		}
		bool result = false;
		Gizmos.color = Color.yellow;
		MeshFilter[] componentsInChildren = ((Component)prefab).gameObject.GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter val in componentsInChildren)
		{
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val.sharedMesh))
			{
				Vector3 position = ((Component)prefab).transform.position;
				Quaternion val2 = Quaternion.Inverse(((Component)prefab).transform.rotation);
				Vector3 val3 = ((Component)val).transform.position - position;
				Vector3 val4 = ((Component)this).transform.position + ((Component)this).transform.rotation * val3;
				Quaternion val5 = val2 * ((Component)val).transform.rotation;
				Quaternion val6 = ((Component)this).transform.rotation * val5;
				Gizmos.DrawMesh(val.sharedMesh, val4, val6, ((Component)val).transform.lossyScale);
				result = true;
			}
		}
		return result;
	}
}
