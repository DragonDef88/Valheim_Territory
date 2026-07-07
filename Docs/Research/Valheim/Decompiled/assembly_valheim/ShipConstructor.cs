using System;
using UnityEngine;

public class ShipConstructor : MonoBehaviour
{
	public GameObject m_shipPrefab;

	public GameObject m_hideWhenConstructed;

	public Transform m_spawnPoint;

	public long m_constructionTimeMinutes = 1L;

	private ZNetView m_nview;

	private void Start()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (!((Object)(object)m_nview == (Object)null) && m_nview.GetZDO() != null)
		{
			if (m_nview.IsOwner() && m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L) == 0L)
			{
				m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
			}
			((MonoBehaviour)this).InvokeRepeating("UpdateConstruction", 5f, 1f);
			if (IsBuilt())
			{
				m_hideWhenConstructed.SetActive(false);
			}
		}
	}

	private bool IsBuilt()
	{
		return m_nview.GetZDO().GetBool(ZDOVars.s_done);
	}

	private void UpdateConstruction()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			return;
		}
		if (IsBuilt())
		{
			m_hideWhenConstructed.SetActive(false);
			return;
		}
		DateTime time = ZNet.instance.GetTime();
		DateTime dateTime = new DateTime(m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L));
		if ((time - dateTime).TotalMinutes > (double)m_constructionTimeMinutes)
		{
			m_hideWhenConstructed.SetActive(false);
			Object.Instantiate<GameObject>(m_shipPrefab, m_spawnPoint.position, m_spawnPoint.rotation);
			m_nview.GetZDO().Set(ZDOVars.s_done, value: true);
		}
	}
}
