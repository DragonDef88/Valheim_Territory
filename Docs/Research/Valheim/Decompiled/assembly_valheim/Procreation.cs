using System;
using UnityEngine;

public class Procreation : MonoBehaviour
{
	public float m_updateInterval = 10f;

	public float m_totalCheckRange = 10f;

	public int m_maxCreatures = 4;

	public float m_partnerCheckRange = 3f;

	public float m_pregnancyChance = 0.5f;

	public float m_pregnancyDuration = 10f;

	public int m_requiredLovePoints = 4;

	public GameObject m_offspring;

	public int m_minOffspringLevel;

	public float m_spawnOffset = 2f;

	public float m_spawnOffsetMax;

	public bool m_spawnRandomDirection;

	public GameObject m_seperatePartner;

	public GameObject m_noPartnerOffspring;

	public EffectList m_birthEffects = new EffectList();

	public EffectList m_loveEffects = new EffectList();

	private GameObject m_myPrefab;

	private GameObject m_offspringPrefab;

	private ZNetView m_nview;

	private BaseAI m_baseAI;

	private Character m_character;

	private Tameable m_tameable;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_baseAI = ((Component)this).GetComponent<BaseAI>();
		m_character = ((Component)this).GetComponent<Character>();
		m_tameable = ((Component)this).GetComponent<Tameable>();
		((MonoBehaviour)this).InvokeRepeating("Procreate", Random.Range(m_updateInterval, m_updateInterval + m_updateInterval * 0.5f), m_updateInterval);
	}

	private void Procreate()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner() || !m_tameable.IsTamed())
		{
			return;
		}
		if ((Object)(object)m_offspringPrefab == (Object)null)
		{
			string prefabName = Utils.GetPrefabName(m_offspring);
			m_offspringPrefab = ZNetScene.instance.GetPrefab(prefabName);
			int prefab = m_nview.GetZDO().GetPrefab();
			m_myPrefab = ZNetScene.instance.GetPrefab(prefab);
		}
		if (IsPregnant())
		{
			if (!IsDue())
			{
				return;
			}
			ResetPregnancy();
			GameObject val = m_offspringPrefab;
			if (Object.op_Implicit((Object)(object)m_noPartnerOffspring))
			{
				int nrOfInstances = SpawnSystem.GetNrOfInstances(Object.op_Implicit((Object)(object)m_seperatePartner) ? m_seperatePartner : m_myPrefab, ((Component)this).transform.position, m_partnerCheckRange, eventCreaturesOnly: false, procreationOnly: true);
				if ((!Object.op_Implicit((Object)(object)m_seperatePartner) && nrOfInstances < 2) || (Object.op_Implicit((Object)(object)m_seperatePartner) && nrOfInstances < 1))
				{
					val = m_noPartnerOffspring;
				}
			}
			Vector3 forward = ((Component)this).transform.forward;
			if (m_spawnRandomDirection)
			{
				float num = Random.Range(0f, (float)Math.PI * 2f);
				((Vector3)(ref forward))._002Ector(Mathf.Cos(num), 0f, Mathf.Sin(num));
			}
			float num2 = ((m_spawnOffsetMax > 0f) ? Random.Range(m_spawnOffset, m_spawnOffsetMax) : m_spawnOffset);
			GameObject val2 = Object.Instantiate<GameObject>(val, ((Component)this).transform.position - forward * num2, Quaternion.LookRotation(-((Component)this).transform.forward, Vector3.up));
			Character component = val2.GetComponent<Character>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.SetTamed(m_tameable.IsTamed());
				component.SetLevel(Mathf.Max(m_minOffspringLevel, Object.op_Implicit((Object)(object)m_character) ? m_character.GetLevel() : m_minOffspringLevel));
			}
			else
			{
				val2.GetComponent<ItemDrop>()?.SetQuality(Mathf.Max(m_minOffspringLevel, Object.op_Implicit((Object)(object)m_character) ? m_character.GetLevel() : m_minOffspringLevel));
			}
			m_birthEffects.Create(val2.transform.position, Quaternion.identity);
		}
		else
		{
			if (Random.value <= m_pregnancyChance || (Object.op_Implicit((Object)(object)m_baseAI) && m_baseAI.IsAlerted()) || m_tameable.IsHungry())
			{
				return;
			}
			int nrOfInstances2 = SpawnSystem.GetNrOfInstances(m_myPrefab, ((Component)this).transform.position, m_totalCheckRange);
			int nrOfInstances3 = SpawnSystem.GetNrOfInstances(m_offspringPrefab, ((Component)this).transform.position, m_totalCheckRange);
			if (nrOfInstances2 + nrOfInstances3 >= m_maxCreatures)
			{
				return;
			}
			int nrOfInstances4 = SpawnSystem.GetNrOfInstances(Object.op_Implicit((Object)(object)m_seperatePartner) ? m_seperatePartner : m_myPrefab, ((Component)this).transform.position, m_partnerCheckRange, eventCreaturesOnly: false, procreationOnly: true);
			if (Object.op_Implicit((Object)(object)m_noPartnerOffspring) || ((Object.op_Implicit((Object)(object)m_seperatePartner) || nrOfInstances4 >= 2) && (!Object.op_Implicit((Object)(object)m_seperatePartner) || nrOfInstances4 >= 1)))
			{
				if (nrOfInstances4 > 0)
				{
					m_loveEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				}
				int lovePoints = GetLovePoints();
				lovePoints++;
				m_nview.GetZDO().Set(ZDOVars.s_lovePoints, lovePoints);
				if (lovePoints >= m_requiredLovePoints)
				{
					m_nview.GetZDO().Set(ZDOVars.s_lovePoints, 0);
					MakePregnant();
				}
			}
		}
	}

	public int GetLovePoints()
	{
		return m_nview.GetZDO().GetInt(ZDOVars.s_lovePoints);
	}

	public bool ReadyForProcreation()
	{
		if (m_tameable.IsTamed() && !IsPregnant())
		{
			return !m_tameable.IsHungry();
		}
		return false;
	}

	private void MakePregnant()
	{
		m_nview.GetZDO().Set(ZDOVars.s_pregnant, ZNet.instance.GetTime().Ticks);
	}

	private void ResetPregnancy()
	{
		m_nview.GetZDO().Set(ZDOVars.s_pregnant, 0L);
	}

	private bool IsDue()
	{
		long @long = m_nview.GetZDO().GetLong(ZDOVars.s_pregnant, 0L);
		if (@long == 0L)
		{
			return false;
		}
		DateTime dateTime = new DateTime(@long);
		return (ZNet.instance.GetTime() - dateTime).TotalSeconds > (double)m_pregnancyDuration;
	}

	private bool IsPregnant()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.GetZDO().GetLong(ZDOVars.s_pregnant, 0L) != 0;
	}
}
