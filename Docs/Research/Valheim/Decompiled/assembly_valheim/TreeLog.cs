using System;
using System.Collections.Generic;
using UnityEngine;

public class TreeLog : MonoBehaviour, IDestructible
{
	public float m_health = 60f;

	public HitData.DamageModifiers m_damages;

	public int m_minToolTier;

	public EffectList m_destroyedEffect = new EffectList();

	public EffectList m_hitEffect = new EffectList();

	public DropTable m_dropWhenDestroyed = new DropTable();

	public GameObject m_subLogPrefab;

	public Transform[] m_subLogPoints = Array.Empty<Transform>();

	public bool m_useSubLogPointRotation;

	public float m_spawnDistance = 2f;

	public float m_hitNoise = 100f;

	private Rigidbody m_body;

	private ZNetView m_nview;

	private bool m_firstFrame = true;

	private void Awake()
	{
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_body.maxDepenetrationVelocity = 1f;
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_nview.Register<HitData>("RPC_Damage", RPC_Damage);
		if (m_nview.IsOwner())
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_health, -1f);
			if (@float == -1f)
			{
				m_nview.GetZDO().Set(ZDOVars.s_health, m_health + (float)Game.m_worldLevel * m_health * Game.instance.m_worldLevelMineHPMultiplier);
			}
			else if (@float <= 0f)
			{
				m_nview.Destroy();
			}
		}
		((MonoBehaviour)this).Invoke("EnableDamage", 0.2f);
	}

	private void EnableDamage()
	{
		m_firstFrame = false;
	}

	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Tree;
	}

	public void Damage(HitData hit)
	{
		if (!m_firstFrame && m_nview.IsValid())
		{
			m_nview.InvokeRPC("RPC_Damage", hit);
		}
	}

	private void RPC_Damage(long sender, HitData hit)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			return;
		}
		float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_health);
		if (@float <= 0f)
		{
			return;
		}
		HitData hitData = hit.Clone();
		hit.ApplyResistance(m_damages, out var significantModifier);
		float totalDamage = hit.GetTotalDamage();
		if (!hit.CheckToolTier(m_minToolTier, alwaysAllowTierZero: true))
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f);
			return;
		}
		if (Object.op_Implicit((Object)(object)m_body))
		{
			m_body.AddForceAtPosition(hit.m_dir * hit.m_pushForce * 2f, hit.m_point, (ForceMode)1);
		}
		DamageText.instance.ShowText(significantModifier, hit.m_point, totalDamage);
		if (totalDamage <= 0f)
		{
			return;
		}
		@float -= totalDamage;
		if (@float < 0f)
		{
			@float = 0f;
		}
		m_nview.GetZDO().Set(ZDOVars.s_health, @float);
		if (hit.m_hitType != HitData.HitType.CinderFire)
		{
			m_hitEffect.Create(hit.m_point, Quaternion.identity, ((Component)this).transform);
			if (m_hitNoise > 0f)
			{
				Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 10f);
				if (Object.op_Implicit((Object)(object)closestPlayer))
				{
					closestPlayer.AddNoise(m_hitNoise);
				}
			}
		}
		if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
		{
			Game.instance.IncrementPlayerStat(PlayerStatType.LogChops);
		}
		if (@float <= 0f)
		{
			Destroy(hitData);
			if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
			{
				Game.instance.IncrementPlayerStat(PlayerStatType.Logs);
			}
		}
	}

	private void Destroy(HitData hitData = null)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		ZNetScene.instance.Destroy(((Component)this).gameObject);
		m_destroyedEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		List<GameObject> dropList = m_dropWhenDestroyed.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector3 val = ((Component)this).transform.position + ((Component)this).transform.up * Random.Range(0f - m_spawnDistance, m_spawnDistance) + Vector3.up * 0.3f * (float)i;
			Quaternion val2 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
			GameObject val3 = dropList[i];
			ItemDrop component = val3.GetComponent<ItemDrop>();
			int dropCount = 1;
			if (Object.op_Implicit((Object)(object)component))
			{
				val3 = Game.instance.CheckDropConversion(hitData, component, val3, ref dropCount);
			}
			for (int j = 0; j < dropCount; j++)
			{
				ItemDrop.OnCreateNew(Object.Instantiate<GameObject>(val3, val, val2));
			}
		}
		if ((Object)(object)m_subLogPrefab != (Object)null)
		{
			Transform[] subLogPoints = m_subLogPoints;
			foreach (Transform val4 in subLogPoints)
			{
				Quaternion val5 = (m_useSubLogPointRotation ? val4.rotation : ((Component)this).transform.rotation);
				Object.Instantiate<GameObject>(m_subLogPrefab, val4.position, val5).GetComponent<ZNetView>().SetLocalScale(((Component)this).transform.localScale);
			}
		}
	}
}
