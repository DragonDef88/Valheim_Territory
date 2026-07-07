using System;
using UnityEngine;

public class MineRock : MonoBehaviour, IDestructible, Hoverable
{
	public string m_name = "";

	public float m_health = 2f;

	public bool m_removeWhenDestroyed = true;

	public HitData.DamageModifiers m_damageModifiers;

	public int m_minToolTier;

	public GameObject m_areaRoot;

	public GameObject m_baseModel;

	public EffectList m_destroyedEffect = new EffectList();

	public EffectList m_hitEffect = new EffectList();

	public DropTable m_dropItems;

	public Action m_onHit;

	private Collider[] m_hitAreas;

	private MeshRenderer[][] m_areaMeshes;

	private ZNetView m_nview;

	private void Start()
	{
		m_hitAreas = (((Object)(object)m_areaRoot != (Object)null) ? m_areaRoot.GetComponentsInChildren<Collider>() : ((Component)this).gameObject.GetComponentsInChildren<Collider>());
		if (Object.op_Implicit((Object)(object)m_baseModel))
		{
			m_areaMeshes = new MeshRenderer[m_hitAreas.Length][];
			for (int i = 0; i < m_hitAreas.Length; i++)
			{
				m_areaMeshes[i] = ((Component)m_hitAreas[i]).GetComponents<MeshRenderer>();
			}
		}
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.GetZDO() != null)
		{
			m_nview.Register<HitData, int>("Hit", RPC_Hit);
			m_nview.Register<int>("Hide", RPC_Hide);
		}
		((MonoBehaviour)this).InvokeRepeating("UpdateVisability", Random.Range(1f, 2f), 10f);
	}

	public string GetHoverText()
	{
		return Localization.instance.Localize(m_name);
	}

	public string GetHoverName()
	{
		return m_name;
	}

	private void UpdateVisability()
	{
		bool flag = false;
		for (int i = 0; i < m_hitAreas.Length; i++)
		{
			Collider val = m_hitAreas[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				string name = "Health" + i;
				bool flag2 = m_nview.GetZDO().GetFloat(name, GetHealth()) > 0f;
				((Component)val).gameObject.SetActive(flag2);
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if (!Object.op_Implicit((Object)(object)m_baseModel))
		{
			return;
		}
		m_baseModel.SetActive(!flag);
		MeshRenderer[][] areaMeshes = m_areaMeshes;
		foreach (MeshRenderer[] array in areaMeshes)
		{
			for (int k = 0; k < array.Length; k++)
			{
				((Renderer)array[k]).enabled = flag;
			}
		}
	}

	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	public void Damage(HitData hit)
	{
		if ((Object)(object)hit.m_hitCollider == (Object)null)
		{
			ZLog.Log((object)"Minerock hit has no collider");
			return;
		}
		int areaIndex = GetAreaIndex(hit.m_hitCollider);
		if (areaIndex == -1)
		{
			ZLog.Log((object)("Invalid hit area on " + ((Object)((Component)this).gameObject).name));
			return;
		}
		ZLog.Log((object)("Hit mine rock area " + areaIndex));
		m_nview.InvokeRPC("Hit", hit, areaIndex);
	}

	private void RPC_Hit(long sender, HitData hit, int hitAreaIndex)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			return;
		}
		Collider hitArea = GetHitArea(hitAreaIndex);
		if ((Object)(object)hitArea == (Object)null)
		{
			ZLog.Log((object)("Missing hit area " + hitAreaIndex));
			return;
		}
		string name = "Health" + hitAreaIndex;
		float @float = m_nview.GetZDO().GetFloat(name, GetHealth());
		if (@float <= 0f)
		{
			ZLog.Log((object)"Already destroyed");
			return;
		}
		hit.ApplyResistance(m_damageModifiers, out var significantModifier);
		float totalDamage = hit.GetTotalDamage();
		if (!hit.CheckToolTier(m_minToolTier))
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f);
			return;
		}
		DamageText.instance.ShowText(significantModifier, hit.m_point, totalDamage);
		if (totalDamage <= 0f)
		{
			return;
		}
		@float -= totalDamage;
		m_nview.GetZDO().Set(name, @float);
		m_hitEffect.Create(hit.m_point, Quaternion.identity);
		if (hit.m_hitType != HitData.HitType.CinderFire)
		{
			Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
			if (Object.op_Implicit((Object)(object)closestPlayer))
			{
				closestPlayer.AddNoise(100f);
			}
		}
		if (m_onHit != null)
		{
			m_onHit();
		}
		if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
		{
			Game.instance.IncrementPlayerStat(PlayerStatType.MineHits);
		}
		if (!(@float <= 0f))
		{
			return;
		}
		EffectList destroyedEffect = m_destroyedEffect;
		Bounds bounds = hitArea.bounds;
		destroyedEffect.Create(((Bounds)(ref bounds)).center, Quaternion.identity);
		m_nview.InvokeRPC(ZNetView.Everybody, "Hide", hitAreaIndex);
		foreach (GameObject drop in m_dropItems.GetDropList())
		{
			Vector3 val = hit.m_point - hit.m_dir * 0.2f + Random.insideUnitSphere * 0.3f;
			Object.Instantiate<GameObject>(drop, val, Quaternion.identity);
			ItemDrop.OnCreateNew(drop);
		}
		if (m_removeWhenDestroyed && AllDestroyed())
		{
			m_nview.Destroy();
		}
		if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
		{
			Game.instance.IncrementPlayerStat(PlayerStatType.Mines);
			switch (m_minToolTier)
			{
			case 0:
				Game.instance.IncrementPlayerStat(PlayerStatType.MineTier0);
				break;
			case 1:
				Game.instance.IncrementPlayerStat(PlayerStatType.MineTier1);
				break;
			case 2:
				Game.instance.IncrementPlayerStat(PlayerStatType.MineTier2);
				break;
			case 3:
				Game.instance.IncrementPlayerStat(PlayerStatType.MineTier3);
				break;
			case 4:
				Game.instance.IncrementPlayerStat(PlayerStatType.MineTier4);
				break;
			case 5:
				Game.instance.IncrementPlayerStat(PlayerStatType.MineTier5);
				break;
			default:
				ZLog.LogWarning((object)("No stat for mine tier: " + m_minToolTier));
				break;
			}
		}
	}

	private bool AllDestroyed()
	{
		for (int i = 0; i < m_hitAreas.Length; i++)
		{
			string name = "Health" + i;
			if (m_nview.GetZDO().GetFloat(name, GetHealth()) > 0f)
			{
				return false;
			}
		}
		return true;
	}

	private void RPC_Hide(long sender, int index)
	{
		Collider hitArea = GetHitArea(index);
		if (Object.op_Implicit((Object)(object)hitArea))
		{
			((Component)hitArea).gameObject.SetActive(false);
		}
		if (!Object.op_Implicit((Object)(object)m_baseModel) || !m_baseModel.activeSelf)
		{
			return;
		}
		m_baseModel.SetActive(false);
		MeshRenderer[][] areaMeshes = m_areaMeshes;
		foreach (MeshRenderer[] array in areaMeshes)
		{
			for (int j = 0; j < array.Length; j++)
			{
				((Renderer)array[j]).enabled = true;
			}
		}
	}

	private int GetAreaIndex(Collider area)
	{
		for (int i = 0; i < m_hitAreas.Length; i++)
		{
			if ((Object)(object)m_hitAreas[i] == (Object)(object)area)
			{
				return i;
			}
		}
		return -1;
	}

	private Collider GetHitArea(int index)
	{
		if (index < 0 || index >= m_hitAreas.Length)
		{
			return null;
		}
		return m_hitAreas[index];
	}

	public float GetHealth()
	{
		return m_health + (float)Game.m_worldLevel * m_health * Game.instance.m_worldLevelMineHPMultiplier;
	}
}
