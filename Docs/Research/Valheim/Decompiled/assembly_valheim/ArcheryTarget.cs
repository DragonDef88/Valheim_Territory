using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ArcheryTarget : MonoBehaviour, IHitProjectile, Hoverable, Interactable
{
	public string m_name;

	public GameObject m_center;

	public float m_targetSize = 1f;

	public int m_points = 10;

	public int m_scoreListSize = 5;

	public float m_projectileStayTTL = 60f;

	public bool m_killProjectile;

	public float m_raiseSkillMultiplier = 1f;

	public GameObject m_returnPoint;

	public List<ItemDrop> m_returnAmmo = new List<ItemDrop>();

	[Header("Effects")]
	public List<ProjectileTypeEffect> m_projectileHitEffects = new List<ProjectileTypeEffect>();

	public EffectList m_bullsEyeEffect = new EffectList();

	public EffectList m_doubleBullsEyeEffect = new EffectList();

	public EffectList m_fullBullsEyeEffect = new EffectList();

	private Vector3 m_lastHitPos;

	private ZNetView m_nview;

	private WearNTear m_wnt;

	private byte[] m_lastScores;

	private static StringBuilder m_sb = new StringBuilder();

	private void Start()
	{
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		m_wnt = ((Component)this).GetComponentInParent<WearNTear>();
		if (Object.op_Implicit((Object)(object)m_wnt))
		{
			WearNTear wnt = m_wnt;
			wnt.m_onDestroyed = (Action)Delegate.Combine(wnt.m_onDestroyed, new Action(OnDestroyed));
		}
		m_lastScores = new byte[m_scoreListSize];
		m_nview.Register<int, int, Vector3>("RPC_ProjectileHit", RPC_ProjectileHit);
		m_nview.Register("RPC_DropArrows", RPC_DropArrows);
	}

	private void OnDrawGizmos()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_center))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(m_center.transform.position, m_targetSize);
			Gizmos.DrawWireSphere(m_lastHitPos, 0.02f);
			Gizmos.DrawWireSphere(m_lastHitPos, 0.25f);
		}
	}

	public bool OnProjectileHit(Character owner, ItemDrop.ItemData weapon, Projectile projectile, Collider collider, Vector3 hitPoint, bool water, Vector3 normal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		m_lastHitPos = hitPoint;
		if (m_projectileStayTTL >= 0f)
		{
			projectile.SetStayTTL(m_projectileStayTTL);
		}
		float num = Vector3.Distance(m_center.transform.position, m_lastHitPos) / m_targetSize;
		int num2 = Mathf.Max(0, Mathf.CeilToInt((1f - num) * (float)m_points));
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, num2.ToString());
		int num3 = FindAmmoIndex(projectile);
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			if (m_nview.IsOwner())
			{
				ProjectileHit(num2, num3, hitPoint);
			}
			else
			{
				m_nview.InvokeRPC("RPC_ProjectileHit", num2, num3, hitPoint);
			}
		}
		foreach (ProjectileTypeEffect projectileHitEffect in m_projectileHitEffects)
		{
			if (projectile.m_type.HasFlag(projectileHitEffect.m_type))
			{
				projectileHitEffect.m_effect.Create(hitPoint, ((Component)this).transform.rotation);
			}
		}
		if (m_raiseSkillMultiplier > 0f && (Object)(object)owner != (Object)null)
		{
			owner.RaiseSkill(projectile.m_skill, projectile.m_raiseSkillAmount * m_raiseSkillMultiplier * (1f - num));
		}
		return !m_killProjectile;
	}

	private void RPC_ProjectileHit(long sender, int points, int ammoIndex, Vector3 hitPoint)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		ProjectileHit(points, ammoIndex, hitPoint);
	}

	private void ProjectileHit(int points, int ammoIndex, Vector3 hitPoint)
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsOwner())
		{
			return;
		}
		ZDO zDO = m_nview.GetZDO();
		int @int = zDO.GetInt(ZDOVars.s_dataCount);
		zDO.Set(ZDOVars.s_dataCount, @int + points);
		int int2 = zDO.GetInt(ZDOVars.s_hitPoint);
		zDO.Set(ZDOVars.s_hitPoint, int2 + 1);
		if (m_scoreListSize > 0)
		{
			m_lastScores = zDO.GetByteArray(ZDOVars.s_data, m_lastScores);
			for (int num = m_lastScores.Length - 1; num >= 1; num--)
			{
				m_lastScores[num] = m_lastScores[num - 1];
			}
			m_lastScores[0] = (byte)points;
			zDO.Set(ZDOVars.s_data, m_lastScores);
		}
		zDO.Set(ZDOVars.s_ammoType + ammoIndex, zDO.GetInt(ZDOVars.s_ammoType + ammoIndex) + 1);
		if (points != m_points)
		{
			return;
		}
		bool flag = m_scoreListSize > 0;
		for (int i = 0; i < m_lastScores.Length; i++)
		{
			if (m_lastScores[i] != m_points)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			m_fullBullsEyeEffect.Create(hitPoint, ((Component)this).transform.rotation);
		}
		else if (m_scoreListSize > 0 && m_lastScores[1] == m_points)
		{
			m_doubleBullsEyeEffect.Create(hitPoint, ((Component)this).transform.rotation);
		}
		else
		{
			m_bullsEyeEffect.Create(hitPoint, ((Component)this).transform.rotation);
		}
	}

	public int FindAmmoIndex(Projectile projectile)
	{
		string prefabName = Utils.GetPrefabName(((Object)projectile).name);
		for (int i = 0; i < m_returnAmmo.Count; i++)
		{
			if (((Object)m_returnAmmo[i].m_itemData.m_shared.m_attack.m_attackProjectile).name == prefabName)
			{
				return i;
			}
		}
		return -1;
	}

	public string GetHoverText()
	{
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid())
		{
			return "";
		}
		ZDO zDO = m_nview.GetZDO();
		m_sb.Clear();
		m_sb.Append(GetHoverName());
		m_lastScores = zDO.GetByteArray(ZDOVars.s_data, m_lastScores);
		if (m_scoreListSize == 0 || m_lastScores[0] != 0)
		{
			if (m_scoreListSize > 0)
			{
				m_sb.Append("\n$piece_archerytarget_lastscores: ");
				for (int i = 0; i < m_lastScores.Length; i++)
				{
					m_sb.Append(m_lastScores[i]);
					if (i + 1 >= m_lastScores.Length || m_lastScores[i + 1] == 0)
					{
						break;
					}
					m_sb.Append(", ");
				}
			}
			int @int = zDO.GetInt(ZDOVars.s_hitPoint);
			if (@int > m_scoreListSize)
			{
				m_sb.Append("..");
			}
			m_sb.Append($"\n$piece_archerytarget_total: {zDO.GetInt(ZDOVars.s_dataCount)} ( {@int} $piece_archerytarget_hits )\n$piece_archerytarget_reset: [<color=yellow><b>$KEY_Use</b></color>]");
		}
		return Localization.instance.Localize(m_sb.ToString());
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (m_nview.GetZDO().GetInt(ZDOVars.s_dataCount) == 0)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsOwner())
		{
			DropArrows();
		}
		else
		{
			RemoveVisualArrows();
			m_nview.InvokeRPC("RPC_DropArrows");
		}
		return true;
	}

	public void RPC_DropArrows(long sender)
	{
		DropArrows();
	}

	public void RemoveVisualArrows()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Projectile[] array = Object.FindObjectsByType<Projectile>((FindObjectsSortMode)0);
		foreach (Projectile projectile in array)
		{
			if (Vector3.Distance(((Component)projectile).transform.position, m_center.transform.position) < m_targetSize)
			{
				projectile.SetStayTTL(0f);
			}
		}
	}

	public void DropArrows()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsOwner())
		{
			return;
		}
		RemoveVisualArrows();
		ZDO zDO = m_nview.GetZDO();
		for (int i = 0; i < m_returnAmmo.Count; i++)
		{
			int @int = zDO.GetInt(ZDOVars.s_ammoType + i);
			if (@int > 0)
			{
				for (int j = 0; j < @int; j++)
				{
					Object.Instantiate<ItemDrop>(m_returnAmmo[i], m_returnPoint.transform.position, Random.rotation);
				}
				zDO.Set(ZDOVars.s_ammoType + i, 0);
			}
		}
		zDO.Set(ZDOVars.s_dataCount, 0);
		zDO.Set(ZDOVars.s_hitPoint, 0);
		if (m_scoreListSize > 0)
		{
			for (int k = 0; k < m_lastScores.Length; k++)
			{
				m_lastScores[k] = 0;
			}
			zDO.Set(ZDOVars.s_data, m_lastScores);
		}
	}

	private void OnDestroyed()
	{
		DropArrows();
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}
}
