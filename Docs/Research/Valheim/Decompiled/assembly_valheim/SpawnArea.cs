using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
	[Serializable]
	public class SpawnData
	{
		public GameObject m_prefab;

		public float m_weight;

		[Header("Level")]
		public int m_maxLevel = 1;

		public int m_minLevel = 1;
	}

	private const float dt = 2f;

	public List<SpawnData> m_prefabs = new List<SpawnData>();

	public float m_levelupChance = 15f;

	public float m_spawnIntervalSec = 30f;

	public float m_triggerDistance = 256f;

	public bool m_setPatrolSpawnPoint = true;

	public float m_spawnRadius = 2f;

	public float m_nearRadius = 10f;

	public float m_farRadius = 1000f;

	public int m_maxNear = 3;

	public int m_maxTotal = 20;

	public bool m_onGroundOnly;

	public EffectList m_spawnEffects = new EffectList();

	private ZNetView m_nview;

	private float m_spawnTimer;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		((MonoBehaviour)this).InvokeRepeating("UpdateSpawn", 2f, 2f);
	}

	private void UpdateSpawn()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && !ZNetScene.instance.OutsideActiveArea(((Component)this).transform.position) && Player.IsPlayerInRange(((Component)this).transform.position, m_triggerDistance))
		{
			m_spawnTimer += 2f;
			if (m_spawnTimer > m_spawnIntervalSec)
			{
				m_spawnTimer = 0f;
				SpawnOne();
			}
		}
	}

	private bool SpawnOne()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (SpawnSystem.m_nospawn)
		{
			return false;
		}
		GetInstances(out var near, out var total);
		if (near >= m_maxNear || total >= m_maxTotal)
		{
			return false;
		}
		SpawnData spawnData = SelectWeightedPrefab();
		if (spawnData == null)
		{
			return false;
		}
		if (!FindSpawnPoint(spawnData.m_prefab, out var point))
		{
			return false;
		}
		GameObject val = Object.Instantiate<GameObject>(spawnData.m_prefab, point, Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f));
		if (m_setPatrolSpawnPoint)
		{
			BaseAI component = val.GetComponent<BaseAI>();
			if ((Object)(object)component != (Object)null)
			{
				component.SetPatrolPoint();
			}
		}
		Character component2 = val.GetComponent<Character>();
		if (spawnData.m_maxLevel > 1)
		{
			int i;
			for (i = spawnData.m_minLevel; i < spawnData.m_maxLevel; i++)
			{
				if (!(Random.Range(0f, 100f) <= GetLevelUpChance()))
				{
					break;
				}
			}
			if (i > 1)
			{
				component2.SetLevel(i);
			}
		}
		Vector3 centerPoint = component2.GetCenterPoint();
		m_spawnEffects.Create(centerPoint, Quaternion.identity);
		return true;
	}

	private bool FindSpawnPoint(GameObject prefab, out Vector3 point)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		prefab.GetComponent<BaseAI>();
		for (int i = 0; i < 10; i++)
		{
			Vector3 val = ((Component)this).transform.position + Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward * Random.Range(0f, m_spawnRadius);
			if (ZoneSystem.instance.FindFloor(val, out var height) && (!m_onGroundOnly || !ZoneSystem.instance.IsBlocked(val)))
			{
				val.y = height + 0.1f;
				point = val;
				return true;
			}
		}
		point = Vector3.zero;
		return false;
	}

	private SpawnData SelectWeightedPrefab()
	{
		if (m_prefabs.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (SpawnData prefab in m_prefabs)
		{
			num += prefab.m_weight;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		foreach (SpawnData prefab2 in m_prefabs)
		{
			num3 += prefab2.m_weight;
			if (num2 <= num3)
			{
				return prefab2;
			}
		}
		return m_prefabs[m_prefabs.Count - 1];
	}

	private void GetInstances(out int near, out int total)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		near = 0;
		total = 0;
		Vector3 position = ((Component)this).transform.position;
		foreach (BaseAI baseAIInstance in BaseAI.BaseAIInstances)
		{
			if (IsSpawnPrefab(((Component)baseAIInstance).gameObject))
			{
				float num = Utils.DistanceXZ(((Component)baseAIInstance).transform.position, position);
				if (num < m_nearRadius)
				{
					near++;
				}
				if (num < m_farRadius)
				{
					total++;
				}
			}
		}
	}

	private bool IsSpawnPrefab(GameObject go)
	{
		string name = ((Object)go).name;
		Character component = go.GetComponent<Character>();
		foreach (SpawnData prefab in m_prefabs)
		{
			if (Utils.CustomStartsWith(name, ((Object)prefab.m_prefab).name) && (!Object.op_Implicit((Object)(object)component) || !component.IsTamed()))
			{
				return true;
			}
		}
		return false;
	}

	public float GetLevelUpChance()
	{
		if (Game.m_worldLevel > 0 && Game.instance.m_worldLevelEnemyLevelUpExponent > 0f)
		{
			return Mathf.Min(70f, Mathf.Pow(m_levelupChance, (float)Game.m_worldLevel * Game.instance.m_worldLevelEnemyLevelUpExponent));
		}
		return m_levelupChance * Game.m_enemyLevelUpRate;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(((Component)this).transform.position, m_spawnRadius);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(((Component)this).transform.position, m_nearRadius);
	}
}
