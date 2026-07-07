using System;
using System.Collections.Generic;
using UnityEngine;

public class ClutterSystem : MonoBehaviour
{
	[Serializable]
	public class Clutter
	{
		public string m_name = "";

		public bool m_enabled = true;

		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		public bool m_instanced;

		public GameObject m_prefab;

		public int m_amount = 80;

		public bool m_onUncleared = true;

		public bool m_onCleared;

		public float m_minVegetation;

		public float m_maxVegetation;

		public float m_scaleMin = 1f;

		public float m_scaleMax = 1f;

		public float m_maxTilt = 18f;

		public float m_minTilt;

		public float m_maxAlt = 1000f;

		public float m_minAlt = 27f;

		public bool m_snapToWater;

		public bool m_terrainTilt;

		public float m_randomOffset;

		[Header("Ocean depth ")]
		public float m_minOceanDepth;

		public float m_maxOceanDepth;

		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		public float m_forestTresholdMin;

		public float m_forestTresholdMax = 1f;

		[Header("Fractal placement (m_fractalScale > 0 == enabled) ")]
		public float m_fractalScale;

		public float m_fractalOffset;

		public float m_fractalTresholdMin = 0.5f;

		public float m_fractalTresholdMax = 1f;
	}

	private class PatchData
	{
		public Vector3 center;

		public List<GameObject> m_objects = new List<GameObject>();

		public float m_timer;

		public bool m_reset;
	}

	public enum Quality
	{
		Off,
		Low,
		Med,
		High
	}

	private static ClutterSystem m_instance;

	private int m_placeRayMask;

	public List<Clutter> m_clutter = new List<Clutter>();

	public float m_grassPatchSize = 8f;

	public float m_distance = 40f;

	public float m_waterLevel = 27f;

	public float m_playerPushFade = 0.05f;

	public float m_amountScale = 1f;

	public bool m_menuHack;

	private Dictionary<Vector2Int, PatchData> m_patches = new Dictionary<Vector2Int, PatchData>();

	private Stack<PatchData> m_freePatches = new Stack<PatchData>();

	private GameObject m_grassRoot;

	private Vector3 m_oldPlayerPos = Vector3.zero;

	private List<Vector2Int> m_tempToRemove = new List<Vector2Int>();

	private List<KeyValuePair<Vector2Int, PatchData>> m_tempToRemovePair = new List<KeyValuePair<Vector2Int, PatchData>>();

	private Quality m_quality = Quality.High;

	private bool m_forceRebuild;

	public static ClutterSystem instance => m_instance;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		m_instance = this;
		if ((int)SystemInfo.graphicsDeviceType != 4)
		{
			GraphicsSettingsManager.GraphicsSettingsChanged += ApplySettings;
			ApplySettings();
			m_placeRayMask = LayerMask.GetMask(new string[1] { "terrain" });
			m_grassRoot = new GameObject("grassroot");
			m_grassRoot.transform.SetParent(((Component)this).transform);
		}
	}

	private void OnDestroy()
	{
		GraphicsSettingsManager.GraphicsSettingsChanged -= ApplySettings;
	}

	private void ApplySettings()
	{
		Quality vegetation = GraphicsSettingsManager.Instance.GetCurrentSettingsWithCurrentPresetApplied(includeBackground: true).m_vegetation;
		if (m_quality != vegetation)
		{
			m_quality = vegetation;
			ClearAll();
		}
	}

	private void LateUpdate()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!RenderGroupSystem.IsGroupActive(RenderGroup.Overworld))
		{
			ClearAll();
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if ((Object)(object)mainCamera == (Object)null)
		{
			return;
		}
		Vector3 center = ((!GameCamera.InFreeFly() && Object.op_Implicit((Object)(object)Player.m_localPlayer)) ? ((Component)Player.m_localPlayer).transform.position : ((Component)mainCamera).transform.position);
		if (m_forceRebuild)
		{
			if (IsHeightmapReady())
			{
				m_forceRebuild = false;
				UpdateGrass(Time.deltaTime, rebuildAll: true, center);
			}
		}
		else if (IsHeightmapReady())
		{
			UpdateGrass(Time.deltaTime, rebuildAll: false, center);
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null)
		{
			m_oldPlayerPos = Vector3.Lerp(m_oldPlayerPos, ((Component)localPlayer).transform.position, m_playerPushFade);
			Shader.SetGlobalVector("_PlayerPosition", Vector4.op_Implicit(((Component)localPlayer).transform.position));
			Shader.SetGlobalVector("_PlayerOldPosition", Vector4.op_Implicit(m_oldPlayerPos));
		}
		else
		{
			Shader.SetGlobalVector("_PlayerPosition", Vector4.op_Implicit(new Vector3(999999f, 999999f, 999999f)));
			Shader.SetGlobalVector("_PlayerOldPosition", Vector4.op_Implicit(new Vector3(999999f, 999999f, 999999f)));
		}
	}

	public Vector2Int GetVegPatch(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt((point.x + m_grassPatchSize / 2f) / m_grassPatchSize);
		int num2 = Mathf.FloorToInt((point.z + m_grassPatchSize / 2f) / m_grassPatchSize);
		return new Vector2Int(num, num2);
	}

	public Vector3 GetVegPatchCenter(Vector2Int p)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)((Vector2Int)(ref p)).x * m_grassPatchSize, 0f, (float)((Vector2Int)(ref p)).y * m_grassPatchSize);
	}

	private bool IsHeightmapReady()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (!Object.op_Implicit((Object)(object)mainCamera))
		{
			return false;
		}
		if (Heightmap.HaveQueuedRebuild(((Component)mainCamera).transform.position, m_distance))
		{
			return false;
		}
		return true;
	}

	private void UpdateGrass(float dt, bool rebuildAll, Vector3 center)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (m_quality != 0)
		{
			GeneratePatches(rebuildAll, center);
			TimeoutPatches(dt);
		}
	}

	private void GeneratePatches(bool rebuildAll, Vector3 center)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		bool generated = false;
		Vector2Int vegPatch = GetVegPatch(center);
		GeneratePatch(center, vegPatch, ref generated, rebuildAll);
		int num = Mathf.CeilToInt((m_distance - m_grassPatchSize / 2f) / m_grassPatchSize);
		for (int i = 1; i <= num; i++)
		{
			for (int j = ((Vector2Int)(ref vegPatch)).x - i; j <= ((Vector2Int)(ref vegPatch)).x + i; j++)
			{
				GeneratePatch(center, new Vector2Int(j, ((Vector2Int)(ref vegPatch)).y - i), ref generated, rebuildAll);
				GeneratePatch(center, new Vector2Int(j, ((Vector2Int)(ref vegPatch)).y + i), ref generated, rebuildAll);
			}
			for (int k = ((Vector2Int)(ref vegPatch)).y - i + 1; k <= ((Vector2Int)(ref vegPatch)).y + i - 1; k++)
			{
				GeneratePatch(center, new Vector2Int(((Vector2Int)(ref vegPatch)).x - i, k), ref generated, rebuildAll);
				GeneratePatch(center, new Vector2Int(((Vector2Int)(ref vegPatch)).x + i, k), ref generated, rebuildAll);
			}
		}
	}

	private void GeneratePatch(Vector3 camPos, Vector2Int p, ref bool generated, bool rebuildAll)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (Utils.DistanceXZ(GetVegPatchCenter(p), camPos) > m_distance)
		{
			return;
		}
		if (m_patches.TryGetValue(p, out var value) && !value.m_reset)
		{
			value.m_timer = 0f;
		}
		else
		{
			if (!rebuildAll && generated && !m_menuHack)
			{
				return;
			}
			PatchData patchData = GenerateVegPatch(p, m_grassPatchSize);
			if (patchData == null)
			{
				return;
			}
			if (m_patches.TryGetValue(p, out var value2))
			{
				foreach (GameObject @object in value2.m_objects)
				{
					Object.Destroy((Object)(object)@object);
				}
				FreePatch(value2);
				m_patches.Remove(p);
			}
			m_patches.Add(p, patchData);
			generated = true;
		}
	}

	private void TimeoutPatches(float dt)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		m_tempToRemovePair.Clear();
		foreach (KeyValuePair<Vector2Int, PatchData> patch in m_patches)
		{
			patch.Value.m_timer += dt;
			if (patch.Value.m_timer >= 2f)
			{
				m_tempToRemovePair.Add(patch);
			}
		}
		foreach (KeyValuePair<Vector2Int, PatchData> item in m_tempToRemovePair)
		{
			foreach (GameObject @object in item.Value.m_objects)
			{
				Object.Destroy((Object)(object)@object);
			}
			m_patches.Remove(item.Key);
			FreePatch(item.Value);
		}
	}

	public void ClearAll()
	{
		foreach (KeyValuePair<Vector2Int, PatchData> patch in m_patches)
		{
			foreach (GameObject @object in patch.Value.m_objects)
			{
				Object.Destroy((Object)(object)@object);
			}
			FreePatch(patch.Value);
		}
		m_patches.Clear();
		m_forceRebuild = true;
	}

	public void ResetGrass(Vector3 center, float radius)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		float num = m_grassPatchSize / 2f;
		foreach (KeyValuePair<Vector2Int, PatchData> patch in m_patches)
		{
			Vector3 center2 = patch.Value.center;
			if (!(center2.x + num < center.x - radius) && !(center2.x - num > center.x + radius) && !(center2.z + num < center.z - radius) && !(center2.z - num > center.z + radius))
			{
				patch.Value.m_reset = true;
				m_forceRebuild = true;
			}
		}
	}

	public bool GetGroundInfo(Vector3 p, out Vector3 point, out Vector3 normal, out Heightmap hmap, out Heightmap.Biome biome)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p + Vector3.up * 500f, Vector3.down, ref val, 1000f, m_placeRayMask))
		{
			point = ((RaycastHit)(ref val)).point;
			normal = ((RaycastHit)(ref val)).normal;
			hmap = ((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>();
			biome = hmap.GetBiome(point);
			return true;
		}
		point = p;
		normal = Vector3.up;
		hmap = null;
		biome = Heightmap.Biome.Meadows;
		return false;
	}

	private Heightmap.Biome GetPatchBiomes(Vector3 center, float halfSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Heightmap.Biome biome = Heightmap.FindBiomeClutter(new Vector3(center.x - halfSize, 0f, center.z - halfSize));
		Heightmap.Biome biome2 = Heightmap.FindBiomeClutter(new Vector3(center.x + halfSize, 0f, center.z - halfSize));
		Heightmap.Biome biome3 = Heightmap.FindBiomeClutter(new Vector3(center.x - halfSize, 0f, center.z + halfSize));
		Heightmap.Biome biome4 = Heightmap.FindBiomeClutter(new Vector3(center.x + halfSize, 0f, center.z + halfSize));
		if (biome == Heightmap.Biome.None || biome2 == Heightmap.Biome.None || biome3 == Heightmap.Biome.None || biome4 == Heightmap.Biome.None)
		{
			return Heightmap.Biome.None;
		}
		return biome | biome2 | biome3 | biome4;
	}

	private PatchData GenerateVegPatch(Vector2Int patchID, float size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vegPatchCenter = GetVegPatchCenter(patchID);
		float num = size / 2f;
		Heightmap.Biome patchBiomes = GetPatchBiomes(vegPatchCenter, num);
		if (patchBiomes == Heightmap.Biome.None)
		{
			return null;
		}
		State state = Random.state;
		PatchData patchData = AllocatePatch();
		patchData.center = vegPatchCenter;
		Vector3 val = default(Vector3);
		Vector3 val2 = default(Vector3);
		for (int i = 0; i < m_clutter.Count; i++)
		{
			Clutter clutter = m_clutter[i];
			if (!clutter.m_enabled || (patchBiomes & clutter.m_biome) == 0)
			{
				continue;
			}
			InstanceRenderer instanceRenderer = null;
			Random.InitState(((Vector2Int)(ref patchID)).x * (((Vector2Int)(ref patchID)).y * 1374) + i * 9321);
			((Vector3)(ref val))._002Ector(clutter.m_fractalOffset, 0f, 0f);
			float num2 = Mathf.Cos((float)Math.PI / 180f * clutter.m_maxTilt);
			float num3 = Mathf.Cos((float)Math.PI / 180f * clutter.m_minTilt);
			int num4 = (int)((float)(m_quality switch
			{
				Quality.Low => clutter.m_amount / 4, 
				Quality.Med => clutter.m_amount / 2, 
				_ => clutter.m_amount, 
			}) * m_amountScale);
			for (int j = 0; j < num4; j++)
			{
				((Vector3)(ref val2))._002Ector(Random.Range(vegPatchCenter.x - num, vegPatchCenter.x + num), 0f, Random.Range(vegPatchCenter.z - num, vegPatchCenter.z + num));
				float num5 = Random.Range(0, 360);
				if (clutter.m_inForest)
				{
					float forestFactor = WorldGenerator.GetForestFactor(val2);
					if (forestFactor < clutter.m_forestTresholdMin || forestFactor > clutter.m_forestTresholdMax)
					{
						continue;
					}
				}
				if (clutter.m_fractalScale > 0f)
				{
					float num6 = Utils.Fbm(val2 * 0.01f * clutter.m_fractalScale + val, 3, 1.6f, 0.7f);
					if (num6 < clutter.m_fractalTresholdMin || num6 > clutter.m_fractalTresholdMax)
					{
						continue;
					}
				}
				if (!GetGroundInfo(val2, out var point, out var normal, out var hmap, out var biome) || (clutter.m_biome & biome) == 0)
				{
					continue;
				}
				float num7 = point.y - m_waterLevel;
				if (num7 < clutter.m_minAlt || num7 > clutter.m_maxAlt || normal.y < num2 || normal.y > num3)
				{
					continue;
				}
				if (clutter.m_minOceanDepth != clutter.m_maxOceanDepth)
				{
					float oceanDepth = hmap.GetOceanDepth(val2);
					if (oceanDepth < clutter.m_minOceanDepth || oceanDepth > clutter.m_maxOceanDepth)
					{
						continue;
					}
				}
				if (clutter.m_minVegetation != clutter.m_maxVegetation)
				{
					float vegetationMask = hmap.GetVegetationMask(point);
					if (vegetationMask > clutter.m_maxVegetation || vegetationMask < clutter.m_minVegetation)
					{
						continue;
					}
				}
				if (!clutter.m_onCleared || !clutter.m_onUncleared)
				{
					bool flag = hmap.IsCleared(point);
					if ((clutter.m_onCleared && !flag) || (clutter.m_onUncleared && flag))
					{
						continue;
					}
				}
				val2 = point;
				if (clutter.m_snapToWater)
				{
					val2.y = m_waterLevel;
				}
				if (clutter.m_randomOffset != 0f)
				{
					val2.y += Random.Range(0f - clutter.m_randomOffset, clutter.m_randomOffset);
				}
				Quaternion identity = Quaternion.identity;
				identity = ((!clutter.m_terrainTilt) ? Quaternion.Euler(0f, num5, 0f) : Quaternion.AngleAxis(num5, normal));
				if (clutter.m_instanced)
				{
					if ((Object)(object)instanceRenderer == (Object)null)
					{
						GameObject val3 = Object.Instantiate<GameObject>(clutter.m_prefab, vegPatchCenter, Quaternion.identity, m_grassRoot.transform);
						instanceRenderer = val3.GetComponent<InstanceRenderer>();
						if (instanceRenderer.m_lodMaxDistance > m_distance - m_grassPatchSize / 2f)
						{
							instanceRenderer.m_lodMaxDistance = m_distance - m_grassPatchSize / 2f;
						}
						patchData.m_objects.Add(val3);
					}
					float scale = Random.Range(clutter.m_scaleMin, clutter.m_scaleMax);
					instanceRenderer.AddInstance(val2, identity, scale);
				}
				else
				{
					GameObject item = Object.Instantiate<GameObject>(clutter.m_prefab, val2, identity, m_grassRoot.transform);
					patchData.m_objects.Add(item);
				}
			}
		}
		Random.state = state;
		return patchData;
	}

	private PatchData AllocatePatch()
	{
		if (m_freePatches.Count > 0)
		{
			return m_freePatches.Pop();
		}
		return new PatchData();
	}

	private void FreePatch(PatchData patch)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		patch.center = Vector3.zero;
		patch.m_objects.Clear();
		patch.m_timer = 0f;
		patch.m_reset = false;
		m_freePatches.Push(patch);
	}
}
