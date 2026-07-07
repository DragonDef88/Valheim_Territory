using UnityEngine;

public class EggGrow : MonoBehaviour, Hoverable
{
	public float m_growTime = 60f;

	public GameObject m_grownPrefab;

	public bool m_tamed;

	public float m_updateInterval = 5f;

	public bool m_requireNearbyFire = true;

	public bool m_requireUnderRoof = true;

	public float m_requireCoverPercentige = 0.7f;

	public EffectList m_hatchEffect;

	public GameObject m_growingObject;

	public GameObject m_notGrowingObject;

	private ZNetView m_nview;

	private ItemDrop m_item;

	private void Start()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_item = ((Component)this).GetComponent<ItemDrop>();
		((MonoBehaviour)this).InvokeRepeating("GrowUpdate", Random.Range(m_updateInterval, m_updateInterval * 2f), m_updateInterval);
		if (Object.op_Implicit((Object)(object)m_growingObject))
		{
			m_growingObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)m_notGrowingObject))
		{
			m_notGrowingObject.SetActive(true);
		}
	}

	private void GrowUpdate()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		float num = m_nview.GetZDO().GetFloat(ZDOVars.s_growStart);
		if (!m_nview.IsValid() || !m_nview.IsOwner() || m_item.m_itemData.m_stack > 1)
		{
			UpdateEffects(num);
			return;
		}
		if (CanGrow())
		{
			if (num == 0f)
			{
				num = (float)ZNet.instance.GetTimeSeconds();
			}
		}
		else
		{
			num = 0f;
		}
		m_nview.GetZDO().Set(ZDOVars.s_growStart, num);
		UpdateEffects(num);
		if (num > 0f && ZNet.instance.GetTimeSeconds() > (double)(num + m_growTime))
		{
			Character component = Object.Instantiate<GameObject>(m_grownPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation).GetComponent<Character>();
			m_hatchEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			if (Object.op_Implicit((Object)(object)component))
			{
				component.SetTamed(m_tamed);
				component.SetLevel(m_item.m_itemData.m_quality);
			}
			m_nview.Destroy();
		}
	}

	private bool CanGrow()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (m_item.m_itemData.m_stack > 1)
		{
			return false;
		}
		if (m_requireNearbyFire && !Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(((Component)this).transform.position, EffectArea.Type.Heat, 0.5f)))
		{
			return false;
		}
		if (m_requireUnderRoof)
		{
			float num = default(float);
			bool flag = default(bool);
			Cover.GetCoverForPoint(((Component)this).transform.position, ref num, ref flag, 0.1f);
			if (!flag || num < m_requireCoverPercentige)
			{
				return false;
			}
		}
		return true;
	}

	private void UpdateEffects(float grow)
	{
		if (Object.op_Implicit((Object)(object)m_growingObject))
		{
			m_growingObject.SetActive(grow > 0f);
		}
		if (Object.op_Implicit((Object)(object)m_notGrowingObject))
		{
			m_notGrowingObject.SetActive(grow == 0f);
		}
	}

	public string GetHoverText()
	{
		if (!Object.op_Implicit((Object)(object)m_item))
		{
			return "";
		}
		if (!Object.op_Implicit((Object)(object)m_nview) || !m_nview.IsValid())
		{
			return m_item.GetHoverText();
		}
		bool flag = m_nview.GetZDO().GetFloat(ZDOVars.s_growStart) > 0f;
		string text = ((m_item.m_itemData.m_stack > 1) ? "$item_chicken_egg_stacked" : (flag ? "$item_chicken_egg_warm" : "$item_chicken_egg_cold"));
		string hoverText = m_item.GetHoverText();
		int num = hoverText.IndexOf('\n');
		if (num > 0)
		{
			return hoverText.Substring(0, num) + " " + Localization.instance.Localize(text) + hoverText.Substring(num);
		}
		return m_item.GetHoverText();
	}

	public string GetHoverName()
	{
		return m_item.GetHoverName();
	}
}
