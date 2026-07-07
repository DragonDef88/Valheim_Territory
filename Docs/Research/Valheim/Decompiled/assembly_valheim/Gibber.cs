using System;
using UnityEngine;

public class Gibber : MonoBehaviour
{
	[Serializable]
	public class GibbData
	{
		public GameObject m_object;

		public float m_chanceToSpawn = 1f;
	}

	public EffectList m_punchEffector = new EffectList();

	public GameObject m_gibHitEffect;

	public GameObject m_gibDestroyEffect;

	public float m_gibHitDestroyChance;

	public GibbData[] m_gibbs = new GibbData[0];

	public float m_minVel = 10f;

	public float m_maxVel = 20f;

	public float m_maxRotVel = 20f;

	public float m_impactDirectionMix = 0.5f;

	public float m_timeout = 5f;

	public float m_delay;

	[Range(0f, 1f)]
	public float m_chanceToRemoveGib;

	private ZNetView m_nview;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
	}

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position;
		Vector3 val2 = Vector3.zero;
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid())
		{
			val = m_nview.GetZDO().GetVec3(ZDOVars.s_hitPoint, val);
			val2 = m_nview.GetZDO().GetVec3(ZDOVars.s_hitDir, val2);
		}
		if (m_delay > 0f)
		{
			((MonoBehaviour)this).Invoke("Explode", m_delay);
		}
		else
		{
			Explode(val, val2);
		}
	}

	public void Setup(Vector3 hitPoint, Vector3 hitDir)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid())
		{
			m_nview.GetZDO().Set(ZDOVars.s_hitPoint, hitPoint);
			m_nview.GetZDO().Set(ZDOVars.s_hitDir, hitDir);
		}
	}

	private void DestroyAll()
	{
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			if (!m_nview.GetZDO().HasOwner())
			{
				m_nview.ClaimOwnership();
			}
			if (m_nview.IsOwner())
			{
				ZNetScene.instance.Destroy(((Component)this).gameObject);
			}
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void CreateBodies()
	{
		MeshRenderer[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject = ((Component)componentsInChildren[i]).gameObject;
			if (m_chanceToRemoveGib > 0f && Random.value < m_chanceToRemoveGib)
			{
				Object.Destroy((Object)(object)gameObject);
			}
			else if (!Object.op_Implicit((Object)(object)gameObject.GetComponent<Rigidbody>()))
			{
				gameObject.AddComponent<BoxCollider>();
				gameObject.AddComponent<Rigidbody>().maxDepenetrationVelocity = 2f;
				TimedDestruction timedDestruction = gameObject.AddComponent<TimedDestruction>();
				timedDestruction.m_timeout = Random.Range(m_timeout / 2f, m_timeout);
				timedDestruction.Trigger();
			}
		}
	}

	private void Explode()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Explode(Vector3.zero, Vector3.zero);
	}

	private void Explode(Vector3 hitPoint, Vector3 hitDir)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).InvokeRepeating("DestroyAll", m_timeout, 1f);
		float num = (((double)((Vector3)(ref hitDir)).magnitude > 0.01) ? m_impactDirectionMix : 0f);
		CreateBodies();
		Rigidbody[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<Rigidbody>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		Vector3 val = Vector3.zero;
		int num2 = 0;
		Rigidbody[] array = componentsInChildren;
		foreach (Rigidbody val2 in array)
		{
			val += val2.worldCenterOfMass;
			num2++;
		}
		val /= (float)num2;
		array = componentsInChildren;
		foreach (Rigidbody obj in array)
		{
			float num3 = Random.Range(m_minVel, m_maxVel);
			Vector3 val3 = Vector3.Lerp(Vector3.Normalize(obj.worldCenterOfMass - val), hitDir, num);
			obj.linearVelocity = val3 * num3;
			obj.angularVelocity = new Vector3(Random.Range(0f - m_maxRotVel, m_maxRotVel), Random.Range(0f - m_maxRotVel, m_maxRotVel), Random.Range(0f - m_maxRotVel, m_maxRotVel));
		}
		GibbData[] gibbs = m_gibbs;
		foreach (GibbData gibbData in gibbs)
		{
			if (Object.op_Implicit((Object)(object)gibbData.m_object) && gibbData.m_chanceToSpawn < 1f && Random.value > gibbData.m_chanceToSpawn)
			{
				Object.Destroy((Object)(object)gibbData.m_object);
			}
		}
		if ((double)((Vector3)(ref hitDir)).magnitude > 0.01)
		{
			Quaternion baseRot = Quaternion.LookRotation(hitDir);
			m_punchEffector.Create(hitPoint, baseRot);
		}
	}
}
