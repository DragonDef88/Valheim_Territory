using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
	[Serializable]
	public class SpawnData
	{
		public string m_name = "";

		public bool m_enabled = true;

		public bool m_devDisabled;

		public GameObject m_prefab;

		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		[Header("Total nr of instances (if near player is set, only instances within the max spawn radius is counted)")]
		public int m_maxSpawned = 1;

		[Header("How often do we spawn")]
		public float m_spawnInterval = 4f;

		[Header("Chanse to spawn each spawn interval")]
		[Range(0f, 100f)]
		public float m_spawnChance = 100f;

		[Header("Minimum distance to another instance")]
		public float m_spawnDistance = 10f;

		[Header("Spawn range ( 0 = use global setting )")]
		public float m_spawnRadiusMin;

		public float m_spawnRadiusMax;

		[Header("Only spawn if this key is set")]
		public string m_requiredGlobalKey = "";

		[Header("Only spawn if this environment is active")]
		public List<string> m_requiredEnvironments = new List<string>();

		[Header("Group spawning")]
		public int m_groupSizeMin = 1;

		public int m_groupSizeMax = 1;

		public float m_groupRadius = 3f;

		[Header("Time of day & Environment")]
		public bool m_spawnAtNight = true;

		public bool m_spawnAtDay = true;

		[Header("Altitude")]
		public float m_minAltitude = -1000f;

		public float m_maxAltitude = 1000f;

		[Header("Terrain tilt")]
		public float m_minTilt;

		public float m_maxTilt = 35f;

		[Header("Areas")]
		public bool m_inForest = true;

		public bool m_outsideForest = true;

		public bool m_inLava;

		public bool m_outsideLava = true;

		public bool m_canSpawnCloseToPlayer;

		public bool m_insidePlayerBase;

		[Header("Ocean depth ")]
		public float m_minOceanDepth;

		public float m_maxOceanDepth;

		[Header("States")]
		public bool m_huntPlayer;

		public float m_groundOffset = 0.5f;

		public float m_groundOffsetRandom;

		[Header("Distance from center")]
		public float m_minDistanceFromCenter;

		public float m_maxDistanceFromCenter;

		[Header("Level")]
		public int m_maxLevel = 1;

		public int m_minLevel = 1;

		public float m_levelUpMinCenterDistance;

		public float m_overrideLevelupChance = -1f;

		[HideInInspector]
		public bool m_foldout;

		public SpawnData Clone()
		{
			SpawnData obj = MemberwiseClone() as SpawnData;
			obj.m_requiredEnvironments = new List<string>(m_requiredEnvironments);
			return obj;
		}
	}

	public static bool m_nospawn = false;

	private static List<SpawnSystem> m_instances = new List<SpawnSystem>();

	private const float m_spawnDistanceMin = 40f;

	private const float m_spawnDistanceMax = 80f;

	private const float m_levelupChance = 10f;

	public List<SpawnSystemList> m_spawnLists = new List<SpawnSystemList>();

	[HideInInspector]
	public List<Heightmap.Biome> m_biomeFolded = new List<Heightmap.Biome>();

	private static List<Player> m_tempNearPlayers = new List<Player>();

	private ZNetView m_nview;

	private Heightmap m_heightmap;

	private List<SE_Stats> m_pheromoneList = new List<SE_Stats>();

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		m_instances.Add(this);
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_heightmap = Heightmap.FindHeightmap(((Component)this).transform.position);
		((MonoBehaviour)this).InvokeRepeating("UpdateSpawning", 10f, 1f);
	}

	private void OnDestroy()
	{
		m_instances.Remove(this);
	}

	private void UpdateSpawning()
	{
		if (!m_nview.IsValid() || !m_nview.IsOwner() || (Object)(object)Player.m_localPlayer == (Object)null)
		{
			return;
		}
		m_tempNearPlayers.Clear();
		GetPlayersInZone(m_tempNearPlayers);
		if (m_tempNearPlayers.Count == 0)
		{
			return;
		}
		DateTime time = ZNet.instance.GetTime();
		foreach (SpawnSystemList spawnList in m_spawnLists)
		{
			UpdateSpawnList(spawnList.m_spawners, time, eventSpawners: false);
		}
		List<SpawnData> currentSpawners = RandEventSystem.instance.GetCurrentSpawners();
		if (currentSpawners != null)
		{
			UpdateSpawnList(currentSpawners, time, eventSpawners: true);
		}
	}

	private void UpdateSpawnList(List<SpawnData> spawners, DateTime currentTime, bool eventSpawners)
	{
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		string text = (eventSpawners ? "e_" : "b_");
		m_pheromoneList.Clear();
		foreach (Player allPlayer in Player.GetAllPlayers())
		{
			foreach (StatusEffect statusEffect in allPlayer.GetSEMan().GetStatusEffects())
			{
				if (statusEffect is SE_Stats sE_Stats && (Object)(object)sE_Stats.m_pheromoneTarget != (Object)null)
				{
					m_pheromoneList.Add(sE_Stats);
				}
			}
		}
		int num = 0;
		foreach (SpawnData spawner in spawners)
		{
			num++;
			if (!spawner.m_enabled || !m_heightmap.HaveBiome(spawner.m_biome))
			{
				continue;
			}
			int stableHashCode = StringExtensionMethods.GetStableHashCode(text + ((Object)spawner.m_prefab).name + num);
			DateTime dateTime = new DateTime(m_nview.GetZDO().GetLong(stableHashCode, 0L));
			TimeSpan timeSpan = currentTime - dateTime;
			int num2 = Mathf.Min((spawner.m_maxSpawned == 0) ? 1 : spawner.m_maxSpawned, (int)(timeSpan.TotalSeconds / (double)spawner.m_spawnInterval));
			if (num2 > 0)
			{
				m_nview.GetZDO().Set(stableHashCode, currentTime.Ticks);
			}
			for (int i = 0; i < num2; i++)
			{
				if (!FindBaseSpawnPoint(spawner, m_tempNearPlayers, out var spawnCenter, out var _))
				{
					continue;
				}
				int num3 = spawner.m_maxSpawned;
				float num4 = spawner.m_spawnChance;
				int minLevelOverride = -1;
				float num5 = 1f;
				foreach (SE_Stats pheromone in m_pheromoneList)
				{
					if ((Object)(object)pheromone.m_pheromoneTarget == (Object)(object)spawner.m_prefab && (Object)(object)pheromone.m_character != (Object)null && Vector3.Distance(spawnCenter, ((Component)pheromone.m_character).transform.position) < 100f)
					{
						if (pheromone.m_pheromoneSpawnChanceOverride > 0f)
						{
							num4 = pheromone.m_pheromoneSpawnChanceOverride;
						}
						if (pheromone.m_pheromoneMaxInstanceOverride > 0)
						{
							num3 = pheromone.m_pheromoneMaxInstanceOverride;
						}
						if (pheromone.m_pheromoneSpawnMinLevel > 0)
						{
							minLevelOverride = pheromone.m_pheromoneSpawnMinLevel;
						}
						if (pheromone.m_pheromoneLevelUpMultiplier != 1f)
						{
							num5 *= pheromone.m_pheromoneLevelUpMultiplier;
						}
					}
				}
				if (Random.Range(0f, 100f) > num4)
				{
					continue;
				}
				if ((!string.IsNullOrEmpty(spawner.m_requiredGlobalKey) && !ZoneSystem.instance.GetGlobalKey(spawner.m_requiredGlobalKey)) || (spawner.m_requiredEnvironments.Count > 0 && !EnvMan.instance.IsEnvironment(spawner.m_requiredEnvironments)) || (!spawner.m_spawnAtDay && EnvMan.IsDay()) || (!spawner.m_spawnAtNight && EnvMan.IsNight()))
				{
					break;
				}
				int num6 = 0;
				if (num3 > 0)
				{
					num6 = GetNrOfInstances(spawner.m_prefab, Vector3.zero, 0f, eventSpawners);
					if (num6 >= num3)
					{
						break;
					}
				}
				if (spawner.m_spawnDistance > 0f && HaveInstanceInRange(spawner.m_prefab, spawnCenter, spawner.m_spawnDistance))
				{
					continue;
				}
				int num7 = Mathf.Min(Random.Range(spawner.m_groupSizeMin, spawner.m_groupSizeMax + 1), (spawner.m_maxSpawned > 0) ? (spawner.m_maxSpawned - num6) : 100);
				float num8 = ((num7 > 1) ? spawner.m_groupRadius : 0f);
				int num9 = 0;
				for (int j = 0; j < num7 * 2; j++)
				{
					Vector2 insideUnitCircle = Random.insideUnitCircle;
					Vector3 spawnPoint = spawnCenter + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * num8;
					if (IsSpawnPointGood(spawner, ref spawnPoint))
					{
						Spawn(spawner, spawnPoint + Vector3.up * (spawner.m_groundOffset + Random.Range(0f, spawner.m_groundOffsetRandom)), eventSpawners, minLevelOverride, num5);
						num9++;
						if (num9 >= num7)
						{
							break;
						}
					}
				}
				ZLog.Log((object)("Spawned " + ((Object)spawner.m_prefab).name + " x " + num9));
			}
		}
	}

	private void Spawn(SpawnData critter, Vector3 spawnPoint, bool eventSpawner, int minLevelOverride = -1, float levelUpMultiplier = 1f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (m_nospawn)
		{
			return;
		}
		GameObject val = Object.Instantiate<GameObject>(critter.m_prefab, spawnPoint, Quaternion.identity);
		if (Terminal.m_showTests && Terminal.m_testList.ContainsKey("spawns"))
		{
			Terminal.Log($"Spawning {((Object)critter.m_prefab).name} at {spawnPoint}");
			Chat.instance.SendPing(spawnPoint);
		}
		BaseAI component = val.GetComponent<BaseAI>();
		if ((Object)(object)component != (Object)null && critter.m_huntPlayer && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs))
		{
			component.SetHuntPlayer(hunt: true);
		}
		if (critter.m_levelUpMinCenterDistance <= 0f || ((Vector3)(ref spawnPoint)).magnitude > critter.m_levelUpMinCenterDistance)
		{
			int i = critter.m_minLevel;
			float num = GetLevelUpChance(critter);
			if (minLevelOverride >= 0)
			{
				i = minLevelOverride;
			}
			if (levelUpMultiplier != 1f)
			{
				num *= levelUpMultiplier;
			}
			for (; i < critter.m_maxLevel; i++)
			{
				if (!(Random.Range(0f, 100f) <= num))
				{
					break;
				}
			}
			if (i > 1)
			{
				val.GetComponent<Character>()?.SetLevel(i);
				if (val.GetComponent<Fish>() != null)
				{
					val.GetComponent<ItemDrop>()?.SetQuality(i);
				}
			}
		}
		if (component is MonsterAI monsterAI)
		{
			if (!critter.m_spawnAtDay)
			{
				monsterAI.SetDespawnInDay(despawn: true);
			}
			if (eventSpawner)
			{
				monsterAI.SetEventCreature(despawn: true);
			}
		}
	}

	private bool IsSpawnPointGood(SpawnData spawn, ref Vector3 spawnPoint)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		ZoneSystem.instance.GetGroundData(ref spawnPoint, out var normal, out var biome, out var biomeArea, out var hmap);
		if ((spawn.m_biome & biome) == 0)
		{
			return false;
		}
		if ((spawn.m_biomeArea & biomeArea) == 0)
		{
			return false;
		}
		if (ZoneSystem.instance.IsBlocked(spawnPoint))
		{
			return false;
		}
		float num = spawnPoint.y - 30f;
		if (num < spawn.m_minAltitude || num > spawn.m_maxAltitude)
		{
			return false;
		}
		float num2 = Mathf.Cos((float)Math.PI / 180f * spawn.m_maxTilt);
		float num3 = Mathf.Cos((float)Math.PI / 180f * spawn.m_minTilt);
		if (normal.y < num2 || normal.y > num3)
		{
			return false;
		}
		if (spawn.m_minDistanceFromCenter > 0f || spawn.m_maxDistanceFromCenter > 0f)
		{
			float num4 = Utils.LengthXZ(spawnPoint);
			if (spawn.m_minDistanceFromCenter > 0f && num4 < spawn.m_minDistanceFromCenter)
			{
				return false;
			}
			if (spawn.m_maxDistanceFromCenter > 0f && num4 > spawn.m_maxDistanceFromCenter)
			{
				return false;
			}
		}
		float range = ((spawn.m_spawnRadiusMin > 0f) ? spawn.m_spawnRadiusMin : 40f);
		if (!spawn.m_canSpawnCloseToPlayer && Player.IsPlayerInRange(spawnPoint, range))
		{
			return false;
		}
		if (!spawn.m_insidePlayerBase && Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(spawnPoint, EffectArea.Type.PlayerBase)))
		{
			return false;
		}
		if (!spawn.m_inForest || !spawn.m_outsideForest)
		{
			bool flag = WorldGenerator.InForest(spawnPoint);
			if (!spawn.m_inForest && flag)
			{
				return false;
			}
			if (!spawn.m_outsideForest && !flag)
			{
				return false;
			}
		}
		if (!spawn.m_inLava || !spawn.m_outsideLava)
		{
			if (!spawn.m_inLava && ZoneSystem.instance.IsLava(spawnPoint, defaultTrue: true))
			{
				return false;
			}
			if (!spawn.m_outsideLava && !ZoneSystem.instance.IsLava(spawnPoint))
			{
				return false;
			}
		}
		if (spawn.m_minOceanDepth != spawn.m_maxOceanDepth && (Object)(object)hmap != (Object)null)
		{
			float oceanDepth = hmap.GetOceanDepth(spawnPoint);
			if (oceanDepth < spawn.m_minOceanDepth || oceanDepth > spawn.m_maxOceanDepth)
			{
				return false;
			}
		}
		return true;
	}

	private bool FindBaseSpawnPoint(SpawnData spawn, List<Player> allPlayers, out Vector3 spawnCenter, out Player targetPlayer)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		float num = ((spawn.m_spawnRadiusMin > 0f) ? spawn.m_spawnRadiusMin : 40f);
		float num2 = ((spawn.m_spawnRadiusMax > 0f) ? spawn.m_spawnRadiusMax : 80f);
		for (int i = 0; i < 20; i++)
		{
			Player player = allPlayers[Random.Range(0, allPlayers.Count)];
			Vector3 val = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward;
			Vector3 spawnPoint = ((Component)player).transform.position + val * Random.Range(num, num2);
			if (IsSpawnPointGood(spawn, ref spawnPoint))
			{
				spawnCenter = spawnPoint;
				targetPlayer = player;
				return true;
			}
		}
		spawnCenter = Vector3.zero;
		targetPlayer = null;
		return false;
	}

	private int GetNrOfInstances(string prefabName)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		List<Character> allCharacters = Character.GetAllCharacters();
		int num = 0;
		foreach (Character item in allCharacters)
		{
			if (Utils.CustomStartsWith(((Object)((Component)item).gameObject).name, prefabName) && InsideZone(((Component)item).transform.position))
			{
				num++;
			}
		}
		return num;
	}

	private void GetPlayersInZone(List<Player> players)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player allPlayer in Player.GetAllPlayers())
		{
			if (InsideZone(((Component)allPlayer).transform.position))
			{
				players.Add(allPlayer);
			}
		}
	}

	private void GetPlayersNearZone(List<Player> players, float marginDistance)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player allPlayer in Player.GetAllPlayers())
		{
			if (InsideZone(((Component)allPlayer).transform.position, marginDistance))
			{
				players.Add(allPlayer);
			}
		}
	}

	private bool IsPlayerTooClose(List<Player> players, Vector3 point, float minDistance)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player player in players)
		{
			if (Vector3.Distance(((Component)player).transform.position, point) < minDistance)
			{
				return true;
			}
		}
		return false;
	}

	private bool InPlayerRange(List<Player> players, Vector3 point, float minDistance, float maxDistance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		foreach (Player player in players)
		{
			float num = Utils.DistanceXZ(((Component)player).transform.position, point);
			if (num < minDistance)
			{
				return false;
			}
			if (num < maxDistance)
			{
				result = true;
			}
		}
		return result;
	}

	private static bool HaveInstanceInRange(GameObject prefab, Vector3 centerPoint, float minDistance)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		string name = ((Object)prefab).name;
		if ((Object)(object)prefab.GetComponent<BaseAI>() != (Object)null)
		{
			foreach (BaseAI baseAIInstance in BaseAI.BaseAIInstances)
			{
				if (Utils.CustomStartsWith(((Object)((Component)baseAIInstance).gameObject).name, name) && Utils.DistanceXZ(((Component)baseAIInstance).transform.position, centerPoint) < minDistance)
				{
					return true;
				}
			}
			return false;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("spawned");
		foreach (GameObject val in array)
		{
			if (Utils.CustomStartsWith(((Object)val.gameObject).name, name) && Utils.DistanceXZ(val.transform.position, centerPoint) < minDistance)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetNrOfInstances(GameObject prefab)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetNrOfInstances(prefab, Vector3.zero, 0f);
	}

	public static int GetNrOfInstances(GameObject prefab, Vector3 center, float maxRange, bool eventCreaturesOnly = false, bool procreationOnly = false)
	{
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		string text = ((Object)prefab).name + "(Clone)";
		if ((Object)(object)prefab.GetComponent<BaseAI>() != (Object)null)
		{
			List<BaseAI> baseAIInstances = BaseAI.BaseAIInstances;
			int num = 0;
			{
				foreach (BaseAI item in baseAIInstances)
				{
					if (((Object)((Component)item).gameObject).name != text || (maxRange > 0f && Vector3.Distance(center, ((Component)item).transform.position) > maxRange))
					{
						continue;
					}
					if (eventCreaturesOnly)
					{
						MonsterAI monsterAI = item as MonsterAI;
						if (Object.op_Implicit((Object)(object)monsterAI) && !monsterAI.IsEventCreature())
						{
							continue;
						}
					}
					if (procreationOnly)
					{
						Procreation component = ((Component)item).GetComponent<Procreation>();
						if (Object.op_Implicit((Object)(object)component) && !component.ReadyForProcreation())
						{
							continue;
						}
					}
					num++;
				}
				return num;
			}
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("spawned");
		int num2 = 0;
		GameObject[] array2 = array;
		foreach (GameObject val in array2)
		{
			if (Utils.CustomStartsWith(((Object)val).name, text) && (!(maxRange > 0f) || !(Vector3.Distance(center, val.transform.position) > maxRange)))
			{
				num2++;
			}
		}
		return num2;
	}

	private bool InsideZone(Vector3 point, float extra = 0f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		float num = 32f + extra;
		Vector3 position = ((Component)this).transform.position;
		if (point.x < position.x - num || point.x > position.x + num)
		{
			return false;
		}
		if (point.z < position.z - num || point.z > position.z + num)
		{
			return false;
		}
		return true;
	}

	private bool HaveGlobalKeys(SpawnData ev)
	{
		if (!string.IsNullOrEmpty(ev.m_requiredGlobalKey))
		{
			return ZoneSystem.instance.GetGlobalKey(ev.m_requiredGlobalKey);
		}
		return true;
	}

	public static float GetLevelUpChance(SpawnData creature)
	{
		return GetLevelUpChance((creature.m_overrideLevelupChance >= 0f) ? creature.m_overrideLevelupChance : 0f);
	}

	public static float GetLevelUpChance(float levelUpChanceOverride = 0f)
	{
		float num = ((levelUpChanceOverride > 0f) ? levelUpChanceOverride : 10f);
		if (Game.m_worldLevel > 0 && Game.instance.m_worldLevelEnemyLevelUpExponent > 0f)
		{
			return Mathf.Min(70f, Mathf.Pow(num, (float)Game.m_worldLevel * Game.instance.m_worldLevelEnemyLevelUpExponent));
		}
		return num * Game.m_enemyLevelUpRate;
	}
}
