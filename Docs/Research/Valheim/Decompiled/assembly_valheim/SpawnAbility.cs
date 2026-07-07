using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAbility : MonoBehaviour, IProjectile
{
	public enum TargetType
	{
		ClosestEnemy,
		RandomEnemy,
		Caster,
		Position,
		RandomPathfindablePosition
	}

	[Serializable]
	public class LevelUpSettings
	{
		public Skills.SkillType m_skill;

		public int m_skillLevel;

		public int m_setLevel;

		public int m_maxSpawns;
	}

	[Header("Spawn")]
	public GameObject[] m_spawnPrefab;

	public string m_maxSummonReached = "$hud_maxsummonsreached";

	public bool m_spawnOnAwake;

	public bool m_alertSpawnedCreature = true;

	public bool m_passiveAggressive;

	public bool m_spawnAtTarget = true;

	public int m_minToSpawn = 1;

	public int m_maxToSpawn = 1;

	public int m_maxSpawned = 3;

	public float m_spawnRadius = 3f;

	public bool m_circleSpawn;

	public bool m_snapToTerrain = true;

	[Tooltip("Used to give random Y rotations to things like AOEs that aren't circular")]
	public bool m_randomYRotation;

	public float m_spawnGroundOffset;

	public int m_getSolidHeightMargin = 1000;

	public float m_initialSpawnDelay;

	public float m_spawnDelay;

	public float m_preSpawnDelay;

	public bool m_commandOnSpawn;

	public bool m_wakeUpAnimation;

	public Skills.SkillType m_copySkill;

	public float m_copySkillToRandomFactor;

	public bool m_setMaxInstancesFromWeaponLevel;

	public List<LevelUpSettings> m_levelUpSettings;

	public TargetType m_targetType;

	public Pathfinding.AgentType m_targetWhenPathfindingType = Pathfinding.AgentType.Humanoid;

	public float m_maxTargetRange = 40f;

	public EffectList m_spawnEffects = new EffectList();

	public EffectList m_preSpawnEffects = new EffectList();

	[Tooltip("Used for the troll summoning staff, to spawn an AOE that's friendly to the spawned creature.")]
	public GameObject m_aoePrefab;

	[Header("Projectile")]
	public float m_projectileVelocity = 10f;

	public float m_projectileVelocityMax;

	public float m_projectileAccuracy = 10f;

	public bool m_randomDirection;

	public float m_randomAngleMin;

	public float m_randomAngleMax;

	private Character m_owner;

	private ItemDrop.ItemData m_weapon;

	public void Awake()
	{
		if (m_spawnOnAwake)
		{
			((MonoBehaviour)this).StartCoroutine("Spawn");
		}
	}

	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		m_owner = owner;
		m_weapon = item;
		((MonoBehaviour)this).StartCoroutine("Spawn");
	}

	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	private IEnumerator Spawn()
	{
		if (m_initialSpawnDelay > 0f)
		{
			yield return (object)new WaitForSeconds(m_initialSpawnDelay);
		}
		int toSpawn = Random.Range(m_minToSpawn, m_maxToSpawn);
		Skills skills = (Object.op_Implicit((Object)(object)m_owner) ? m_owner.GetSkills() : null);
		int num3;
		for (int i = 0; i < toSpawn; num3 = i + 1, i = num3)
		{
			Vector3 targetPosition = ((Component)this).transform.position;
			bool foundSpawnPoint = false;
			int tries = ((m_targetType != TargetType.RandomPathfindablePosition) ? 1 : 5);
			for (int j = 0; j < tries; j++)
			{
				bool flag;
				foundSpawnPoint = (flag = FindTarget(out targetPosition, i, toSpawn));
				if (flag)
				{
					break;
				}
				if (m_targetType == TargetType.RandomPathfindablePosition)
				{
					if (j == tries - 1)
					{
						Terminal.LogWarning($"SpawnAbility failed to pathfindable target after {tries} tries, defaulting to transform position.");
						targetPosition = ((Component)this).transform.position;
						foundSpawnPoint = true;
					}
					else
					{
						Terminal.Log("SpawnAbility failed to pathfindable target, waiting before retry.");
						yield return (object)new WaitForSeconds(0.2f);
					}
				}
			}
			if (!foundSpawnPoint)
			{
				Terminal.LogWarning("SpawnAbility failed to find spawn point, aborting spawn.");
				continue;
			}
			Vector3 spawnPoint = targetPosition;
			if (m_targetType != TargetType.RandomPathfindablePosition)
			{
				Vector3 val = (m_spawnAtTarget ? targetPosition : ((Component)this).transform.position);
				Vector2 val2 = Random.insideUnitCircle * m_spawnRadius;
				if (m_circleSpawn)
				{
					val2 = GetCirclePoint(i, toSpawn) * m_spawnRadius;
				}
				spawnPoint = val + new Vector3(val2.x, 0f, val2.y);
				if (m_snapToTerrain)
				{
					ZoneSystem.instance.GetSolidHeight(spawnPoint, out var height, m_getSolidHeightMargin);
					spawnPoint.y = height;
				}
				spawnPoint.y += m_spawnGroundOffset;
				if (Mathf.Abs(spawnPoint.y - val.y) > 100f)
				{
					continue;
				}
			}
			GameObject prefab = m_spawnPrefab[Random.Range(0, m_spawnPrefab.Length)];
			if (m_maxSpawned > 0 && SpawnSystem.GetNrOfInstances(prefab) >= m_maxSpawned)
			{
				if (m_owner is Player player)
				{
					player.Message(MessageHud.MessageType.Center, m_maxSummonReached);
				}
				continue;
			}
			m_preSpawnEffects.Create(spawnPoint, Quaternion.identity);
			if (m_preSpawnDelay > 0f)
			{
				yield return (object)new WaitForSeconds(m_preSpawnDelay);
			}
			Terminal.Log("SpawnAbility spawning a " + ((Object)prefab).name);
			GameObject val3 = Object.Instantiate<GameObject>(prefab, spawnPoint, Quaternion.Euler(0f, Random.value * (float)Math.PI * 2f, 0f));
			ZNetView component = val3.GetComponent<ZNetView>();
			Projectile component2 = val3.GetComponent<Projectile>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				SetupProjectile(component2, targetPosition);
			}
			if (m_randomYRotation)
			{
				val3.transform.Rotate(Vector3.up, (float)Random.Range(-180, 180));
			}
			if (Object.op_Implicit((Object)(object)skills))
			{
				if (m_copySkill != 0 && m_copySkillToRandomFactor > 0f)
				{
					component.GetZDO().Set(ZDOVars.s_randomSkillFactor, 1f + skills.GetSkillLevel(m_copySkill) * m_copySkillToRandomFactor);
				}
				if (m_levelUpSettings.Count > 0)
				{
					Character component3 = val3.GetComponent<Character>();
					if (component3 != null)
					{
						for (int num = m_levelUpSettings.Count - 1; num >= 0; num--)
						{
							LevelUpSettings levelUpSettings = m_levelUpSettings[num];
							if (skills.GetSkillLevel(levelUpSettings.m_skill) >= (float)levelUpSettings.m_skillLevel)
							{
								component3.SetLevel(levelUpSettings.m_setLevel);
								int num2 = (m_setMaxInstancesFromWeaponLevel ? m_weapon.m_quality : levelUpSettings.m_maxSpawns);
								if (num2 > 0)
								{
									component.GetZDO().Set(ZDOVars.s_maxInstances, num2);
								}
								break;
							}
						}
					}
				}
			}
			if (m_commandOnSpawn)
			{
				Tameable component4 = val3.GetComponent<Tameable>();
				if (component4 != null && m_owner is Humanoid humanoid)
				{
					component4.Command(humanoid, message: false);
					if ((Object)(object)humanoid == (Object)(object)Player.m_localPlayer)
					{
						Game.instance.IncrementPlayerStat(PlayerStatType.SkeletonSummons);
					}
				}
			}
			if (m_wakeUpAnimation)
			{
				val3.GetComponent<ZSyncAnimation>()?.SetBool("wakeup", value: true);
			}
			BaseAI component5 = val3.GetComponent<BaseAI>();
			if ((Object)(object)component5 != (Object)null)
			{
				if (m_alertSpawnedCreature)
				{
					component5.Alert();
				}
				BaseAI baseAI = m_owner.GetBaseAI();
				if (component5.m_aggravatable && Object.op_Implicit((Object)(object)baseAI) && baseAI.m_aggravatable)
				{
					component5.SetAggravated(baseAI.IsAggravated(), BaseAI.AggravatedReason.Damage);
				}
				if (m_passiveAggressive)
				{
					component5.m_passiveAggresive = true;
				}
			}
			SetupAoe(val3.GetComponent<Character>(), spawnPoint);
			m_spawnEffects.Create(spawnPoint, Quaternion.identity);
			if (m_spawnDelay > 0f)
			{
				yield return (object)new WaitForSeconds(m_spawnDelay);
			}
		}
		if (!m_spawnOnAwake)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private Vector3 GetRandomConeDirection()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		int num = Random.Range(0, 360);
		float num2 = Random.Range(m_randomAngleMin, m_randomAngleMax);
		return Quaternion.AngleAxis((float)num, Vector3.up) * new Vector3(Mathf.Sin(num2), Mathf.Cos(num2), 0f);
	}

	private void SetupProjectile(Projectile projectile, Vector3 targetPoint)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val2;
		if (!m_randomDirection)
		{
			Vector3 val = targetPoint - ((Component)projectile).transform.position;
			val2 = ((Vector3)(ref val)).normalized;
		}
		else
		{
			val2 = GetRandomConeDirection();
		}
		Vector3 val3 = val2;
		Vector3 val4 = Vector3.Cross(val3, Vector3.up);
		Quaternion val5 = Quaternion.AngleAxis(Random.Range(0f - m_projectileAccuracy, m_projectileAccuracy), Vector3.up);
		val3 = Quaternion.AngleAxis(Random.Range(0f - m_projectileAccuracy, m_projectileAccuracy), val4) * val3;
		val3 = val5 * val3;
		float num = ((m_projectileVelocityMax > 0f) ? Random.Range(m_projectileVelocity, m_projectileVelocityMax) : m_projectileVelocity);
		projectile.Setup(m_owner, val3 * num, -1f, null, null, null);
	}

	private void SetupAoe(Character owner, Vector3 targetPoint)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_aoePrefab == (Object)null) && !((Object)(object)owner == (Object)null))
		{
			Aoe component = Object.Instantiate<GameObject>(m_aoePrefab, targetPoint, Quaternion.identity).GetComponent<Aoe>();
			if (!((Object)(object)component == (Object)null))
			{
				component.Setup(owner, Vector3.zero, -1f, null, null, null);
			}
		}
	}

	private bool FindTarget(out Vector3 point, int i, int spawnCount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		point = Vector3.zero;
		switch (m_targetType)
		{
		case TargetType.ClosestEnemy:
		{
			if ((Object)(object)m_owner == (Object)null)
			{
				return false;
			}
			Character character2 = BaseAI.FindClosestEnemy(m_owner, ((Component)this).transform.position, m_maxTargetRange);
			if ((Object)(object)character2 != (Object)null)
			{
				point = ((Component)character2).transform.position;
				return true;
			}
			return false;
		}
		case TargetType.RandomEnemy:
		{
			if ((Object)(object)m_owner == (Object)null)
			{
				return false;
			}
			Character character = BaseAI.FindRandomEnemy(m_owner, ((Component)this).transform.position, m_maxTargetRange);
			if ((Object)(object)character != (Object)null)
			{
				point = ((Component)character).transform.position;
				return true;
			}
			return false;
		}
		case TargetType.Position:
			point = ((Component)this).transform.position;
			return true;
		case TargetType.Caster:
			if ((Object)(object)m_owner == (Object)null)
			{
				return false;
			}
			point = ((Component)m_owner).transform.position;
			return true;
		case TargetType.RandomPathfindablePosition:
		{
			if ((Object)(object)m_owner == (Object)null)
			{
				return false;
			}
			List<Vector3> list = new List<Vector3>();
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector2 val = ((Vector2)(ref insideUnitCircle)).normalized * Random.Range(m_spawnRadius / 2f, m_spawnRadius);
			point = ((Component)this).transform.position + new Vector3(val.x, 2f, val.y);
			ZoneSystem.instance.GetSolidHeight(point, out var height, 2);
			point.y = height;
			if (Pathfinding.instance.GetPath(((Component)m_owner).transform.position, point, list, m_targetWhenPathfindingType, requireFullPath: true, cleanup: false, havePath: true))
			{
				Terminal.Log($"SpawnAbility found path target, distance: {Vector3.Distance(((Component)this).transform.position, list[0])}");
				point = list[list.Count - 1];
				return true;
			}
			return false;
		}
		default:
			return false;
		}
	}

	private Vector2 GetCirclePoint(int i, int spawnCount)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)i / (float)spawnCount;
		float num2 = Mathf.Sin(num * (float)Math.PI * 2f);
		float num3 = Mathf.Cos(num * (float)Math.PI * 2f);
		return new Vector2(num2, num3);
	}
}
