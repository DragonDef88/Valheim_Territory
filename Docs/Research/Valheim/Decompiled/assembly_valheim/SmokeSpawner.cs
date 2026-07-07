using System.Collections.Generic;
using UnityEngine;

public class SmokeSpawner : MonoBehaviour, IMonoUpdater
{
	private static Collider[] s_colliders = (Collider[])(object)new Collider[30];

	private const float m_minPlayerDistance = 64f;

	private const int m_maxGlobalSmoke = 100;

	private const float m_blockedMinTime = 4f;

	public GameObject m_smokePrefab;

	public float m_interval = 0.5f;

	public LayerMask m_testMask;

	public float m_testRadius = 0.5f;

	public float m_spawnRadius;

	public bool m_stopFireOnStart;

	private float m_lastSpawnTime;

	private float m_time;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		m_time = Random.Range(0f, m_interval);
		if (!m_stopFireOnStart)
		{
			return;
		}
		foreach (Fire s_fire in Fire.s_fires)
		{
			if (Object.op_Implicit((Object)(object)s_fire) && Vector3.Distance(((Component)s_fire).transform.position, ((Component)this).transform.position) < m_spawnRadius)
			{
				ZNetScene.instance.Destroy(((Component)s_fire).gameObject);
			}
		}
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void CustomUpdate(float deltaTime, float time)
	{
		m_time += deltaTime;
		if (m_time > m_interval)
		{
			m_time = 0f;
			Spawn(time);
		}
	}

	private void Spawn(float time)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null || Vector3.Distance(((Component)localPlayer).transform.position, ((Component)this).transform.position) > 64f)
		{
			m_lastSpawnTime = time;
		}
		else if (!TestBlocked())
		{
			if (Smoke.GetTotalSmoke() > 100)
			{
				Smoke.FadeOldest();
			}
			Vector3 val = ((Component)this).transform.position;
			if (m_spawnRadius > 0f)
			{
				Vector2 insideUnitCircle = Random.insideUnitCircle;
				Vector2 val2 = ((Vector2)(ref insideUnitCircle)).normalized * Random.Range(m_spawnRadius / 2f, m_spawnRadius);
				val += new Vector3(val2.x, 0f, val2.y);
			}
			Object.Instantiate<GameObject>(m_smokePrefab, val, Random.rotation);
			m_lastSpawnTime = time;
		}
	}

	private bool TestBlocked()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.CheckSphere(((Component)this).transform.position, m_testRadius, ((LayerMask)(ref m_testMask)).value))
		{
			return true;
		}
		return false;
	}

	public bool IsBlocked()
	{
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			return TestBlocked();
		}
		return Time.time - m_lastSpawnTime > 4f;
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Utils.DrawGizmoCircle(((Component)this).transform.position, m_spawnRadius, 16);
	}
}
