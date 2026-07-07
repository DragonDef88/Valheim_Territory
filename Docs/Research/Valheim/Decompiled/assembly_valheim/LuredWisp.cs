using System.Collections.Generic;
using UnityEngine;

public class LuredWisp : MonoBehaviour
{
	public bool m_despawnInDaylight = true;

	public float m_maxLureDistance = 20f;

	public float m_acceleration = 6f;

	public float m_noiseDistance = 1.5f;

	public float m_noiseDistanceYScale = 0.2f;

	public float m_noiseSpeed = 0.5f;

	public float m_maxSpeed = 40f;

	public float m_friction = 0.03f;

	public EffectList m_despawnEffects = new EffectList();

	private static List<LuredWisp> m_wisps = new List<LuredWisp>();

	private Vector3 m_ballVel = Vector3.zero;

	private ZNetView m_nview;

	private Vector3 m_targetPoint;

	private float m_despawnTimer;

	private float m_time;

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_wisps.Add(this);
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_targetPoint = ((Component)this).transform.position;
		m_time = Random.Range(0, 1000);
		((MonoBehaviour)this).InvokeRepeating("UpdateTarget", Random.Range(0f, 2f), 2f);
	}

	private void OnDestroy()
	{
		m_wisps.Remove(this);
	}

	private void UpdateTarget()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner() && !(m_despawnTimer > 0f))
		{
			WispSpawner bestSpawner = WispSpawner.GetBestSpawner(((Component)this).transform.position, m_maxLureDistance);
			if ((Object)(object)bestSpawner == (Object)null || (m_despawnInDaylight && EnvMan.IsDaylight()))
			{
				m_despawnTimer = 3f;
				m_targetPoint = ((Component)this).transform.position + Quaternion.Euler(-20f, (float)Random.Range(0, 360), 0f) * Vector3.forward * 100f;
			}
			else
			{
				m_despawnTimer = 0f;
				m_targetPoint = bestSpawner.m_spawnPoint.position;
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			UpdateMovement(m_targetPoint, Time.fixedDeltaTime);
		}
	}

	private void UpdateMovement(Vector3 targetPos, float dt)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (m_despawnTimer > 0f)
		{
			m_despawnTimer -= dt;
			if (m_despawnTimer <= 0f)
			{
				m_despawnEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				m_nview.Destroy();
				return;
			}
		}
		m_time += dt;
		float num = m_time * m_noiseSpeed;
		targetPos += new Vector3(Mathf.Sin(num * 4f), Mathf.Sin(num * 2f) * m_noiseDistanceYScale, Mathf.Cos(num * 5f)) * m_noiseDistance;
		Vector3 val = targetPos - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		m_ballVel += normalized * m_acceleration * dt;
		if (((Vector3)(ref m_ballVel)).magnitude > m_maxSpeed)
		{
			m_ballVel = ((Vector3)(ref m_ballVel)).normalized * m_maxSpeed;
		}
		m_ballVel -= m_ballVel * m_friction;
		((Component)this).transform.position = ((Component)this).transform.position + m_ballVel * dt;
	}

	public static int GetWispsInArea(Vector3 p, float r)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		float num = r * r;
		int num2 = 0;
		foreach (LuredWisp wisp in m_wisps)
		{
			if (Utils.DistanceSqr(p, ((Component)wisp).transform.position) < num)
			{
				num2++;
			}
		}
		return num2;
	}
}
