using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class Heightmap : MonoBehaviour, IMonoUpdater
{
	[Flags]
	public enum Biome
	{
		None = 0,
		Meadows = 1,
		Swamp = 2,
		Mountain = 4,
		BlackForest = 8,
		Plains = 0x10,
		AshLands = 0x20,
		DeepNorth = 0x40,
		Ocean = 0x100,
		Mistlands = 0x200,
		All = 0x37F
	}

	[Flags]
	public enum BiomeArea
	{
		Edge = 1,
		Median = 2,
		Everything = 3
	}

	private static readonly Dictionary<Biome, int> s_biomeToIndex = new Dictionary<Biome, int>
	{
		{
			Biome.None,
			0
		},
		{
			Biome.Meadows,
			1
		},
		{
			Biome.Swamp,
			2
		},
		{
			Biome.Mountain,
			3
		},
		{
			Biome.BlackForest,
			4
		},
		{
			Biome.Plains,
			5
		},
		{
			Biome.AshLands,
			6
		},
		{
			Biome.DeepNorth,
			7
		},
		{
			Biome.Ocean,
			8
		},
		{
			Biome.Mistlands,
			9
		}
	};

	private static readonly Biome[] s_indexToBiome = new Biome[10]
	{
		Biome.None,
		Biome.Meadows,
		Biome.Swamp,
		Biome.Mountain,
		Biome.BlackForest,
		Biome.Plains,
		Biome.AshLands,
		Biome.DeepNorth,
		Biome.Ocean,
		Biome.Mistlands
	};

	private static readonly float[] s_tempBiomeWeights = new float[Enum.GetValues(typeof(Biome)).Length];

	public GameObject m_terrainCompilerPrefab;

	public int m_width = 32;

	public float m_scale = 1f;

	public Material m_material;

	public const float c_LevelMaxDelta = 8f;

	public const float c_SmoothMaxDelta = 1f;

	[SerializeField]
	private bool m_isDistantLod;

	public bool m_distantLodEditorHax;

	private static readonly List<Heightmap> s_tempHmaps = new List<Heightmap>();

	private readonly List<float> m_heights = new List<float>();

	private HeightmapBuilder.HMBuildData m_buildData;

	private Texture2D m_paintMask;

	private Material m_materialInstance;

	private MeshCollider m_collider;

	private MeshFilter m_meshFilter;

	private MeshRenderer m_meshRenderer;

	private RenderGroupSubscriber m_renderGroupSubscriber;

	private readonly float[] m_oceanDepth = new float[4];

	private Biome[] m_cornerBiomes = new Biome[4]
	{
		Biome.Meadows,
		Biome.Meadows,
		Biome.Meadows,
		Biome.Meadows
	};

	private Bounds m_bounds;

	private BoundingSphere m_boundingSphere;

	private Mesh m_collisionMesh;

	private Mesh m_renderMesh;

	private bool m_doLateUpdate;

	private static readonly List<Heightmap> s_heightmaps = new List<Heightmap>();

	private static readonly List<Vector3> s_tempVertices = new List<Vector3>();

	private static readonly List<Vector2> s_tempUVs = new List<Vector2>();

	private static readonly List<int> s_tempIndices = new List<int>();

	private static readonly List<Color32> s_tempColors = new List<Color32>();

	public static Color m_paintMaskDirt = new Color(1f, 0f, 0f, 1f);

	public static Color m_paintMaskCultivated = new Color(0f, 1f, 0f, 1f);

	public static Color m_paintMaskPaved = new Color(0f, 0f, 1f, 1f);

	public static Color m_paintMaskNothing = new Color(0f, 0f, 0f, 1f);

	public static Color m_paintMaskClearVegetation = new Color(0f, 0f, 0f, 0f);

	private static int s_shaderPropertyClearedMaskTex = 0;

	public const RenderGroup c_RenderGroup = RenderGroup.Overworld;

	public bool IsDistantLod
	{
		get
		{
			return m_isDistantLod;
		}
		set
		{
			if (m_isDistantLod != value)
			{
				if (value)
				{
					s_heightmaps.Remove(this);
				}
				else
				{
					s_heightmaps.Add(this);
				}
				m_isDistantLod = value;
				UpdateShadowSettings();
			}
		}
	}

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	public event Action m_clearConnectedWearNTearCache;

	private void Awake()
	{
		if (!m_isDistantLod)
		{
			s_heightmaps.Add(this);
		}
		if (s_shaderPropertyClearedMaskTex == 0)
		{
			s_shaderPropertyClearedMaskTex = Shader.PropertyToID("_ClearedMaskTex");
		}
		m_collider = ((Component)this).GetComponent<MeshCollider>();
		m_meshFilter = ((Component)this).GetComponent<MeshFilter>();
		if (!Object.op_Implicit((Object)(object)m_meshFilter))
		{
			m_meshFilter = ((Component)this).gameObject.AddComponent<MeshFilter>();
		}
		m_meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		if (!Object.op_Implicit((Object)(object)m_meshRenderer))
		{
			m_meshRenderer = ((Component)this).gameObject.AddComponent<MeshRenderer>();
		}
		((Renderer)m_meshRenderer).motionVectorGenerationMode = (MotionVectorGenerationMode)0;
		m_renderGroupSubscriber = ((Component)this).GetComponent<RenderGroupSubscriber>();
		if (!Object.op_Implicit((Object)(object)m_renderGroupSubscriber))
		{
			m_renderGroupSubscriber = ((Component)this).gameObject.AddComponent<RenderGroupSubscriber>();
		}
		m_renderGroupSubscriber.Group = RenderGroup.Overworld;
		if ((Object)(object)m_material == (Object)null)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (!m_isDistantLod)
		{
			s_heightmaps.Remove(this);
		}
		if (Object.op_Implicit((Object)(object)m_materialInstance))
		{
			Object.DestroyImmediate((Object)(object)m_materialInstance);
		}
		if (Object.op_Implicit((Object)(object)m_collisionMesh))
		{
			Object.DestroyImmediate((Object)(object)m_collisionMesh);
		}
		if (Object.op_Implicit((Object)(object)m_renderMesh))
		{
			Object.DestroyImmediate((Object)(object)m_renderMesh);
		}
		if (Object.op_Implicit((Object)(object)m_paintMask))
		{
			Object.DestroyImmediate((Object)(object)m_paintMask);
		}
	}

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			Instances.Add(this);
			GraphicsSettingsManager.GraphicsSettingsChanged += UpdateShadowSettings;
			UpdateShadowSettings();
		}
		if (!m_isDistantLod || !Application.isPlaying || m_distantLodEditorHax)
		{
			Regenerate();
		}
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			GraphicsSettingsManager.GraphicsSettingsChanged -= UpdateShadowSettings;
			Instances.Remove(this);
		}
	}

	public void CustomLateUpdate(float deltaTime)
	{
		if (m_doLateUpdate)
		{
			m_doLateUpdate = false;
			Regenerate();
		}
	}

	private void UpdateShadowSettings()
	{
		bool distantShadows = GraphicsSettingsManager.Instance.GetCurrentSettingsWithCurrentPresetApplied(includeBackground: true).m_distantShadows;
		if (m_isDistantLod)
		{
			((Renderer)m_meshRenderer).shadowCastingMode = (ShadowCastingMode)(distantShadows ? 1 : 0);
			((Renderer)m_meshRenderer).receiveShadows = false;
		}
		else
		{
			((Renderer)m_meshRenderer).shadowCastingMode = (ShadowCastingMode)(distantShadows ? 1 : 2);
			((Renderer)m_meshRenderer).receiveShadows = true;
		}
	}

	public static void ForceGenerateAll()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		foreach (Heightmap s_heightmap in s_heightmaps)
		{
			if (s_heightmap.HaveQueuedRebuild())
			{
				Vector3 position = ((Component)s_heightmap).transform.position;
				ZLog.Log((object)("Force generating hmap " + ((object)(Vector3)(ref position)).ToString()));
				s_heightmap.Regenerate();
			}
		}
	}

	public void Poke(bool delayed)
	{
		if (delayed)
		{
			m_doLateUpdate = true;
		}
		else
		{
			Regenerate();
		}
	}

	public bool HaveQueuedRebuild()
	{
		return m_doLateUpdate;
	}

	public void Regenerate()
	{
		m_doLateUpdate = false;
		if (Generate())
		{
			RebuildCollisionMesh();
			UpdateCornerDepths();
			m_materialInstance.SetTexture(s_shaderPropertyClearedMaskTex, (Texture)(object)m_paintMask);
			RebuildRenderMesh();
			this.m_clearConnectedWearNTearCache?.Invoke();
		}
	}

	private void UpdateCornerDepths()
	{
		float num = 30f;
		m_oceanDepth[0] = GetHeight(0, m_width);
		m_oceanDepth[1] = GetHeight(m_width, m_width);
		m_oceanDepth[2] = GetHeight(m_width, 0);
		m_oceanDepth[3] = GetHeight(0, 0);
		m_oceanDepth[0] = Mathf.Max(0f, (float)((double)num - (double)m_oceanDepth[0]));
		m_oceanDepth[1] = Mathf.Max(0f, (float)((double)num - (double)m_oceanDepth[1]));
		m_oceanDepth[2] = Mathf.Max(0f, (float)((double)num - (double)m_oceanDepth[2]));
		m_oceanDepth[3] = Mathf.Max(0f, (float)((double)num - (double)m_oceanDepth[3]));
		m_materialInstance.SetFloatArray("_depth", m_oceanDepth);
	}

	public float[] GetOceanDepth()
	{
		return m_oceanDepth;
	}

	public float GetOceanDepth(Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		WorldToVertex(worldPos, out var x, out var y);
		float num = (float)((double)x / (double)(float)m_width);
		float num2 = (float)y / (float)m_width;
		float num3 = DUtils.Lerp(m_oceanDepth[3], m_oceanDepth[2], num);
		float num4 = DUtils.Lerp(m_oceanDepth[0], m_oceanDepth[1], num);
		return DUtils.Lerp(num3, num4, num2);
	}

	private void Initialize()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		int num = m_width + 1;
		int num2 = num * num;
		if (m_heights.Count != num2)
		{
			m_heights.Clear();
			for (int i = 0; i < num2; i++)
			{
				m_heights.Add(0f);
			}
			m_paintMask = new Texture2D(num, num);
			((Object)m_paintMask).name = "_Heightmap m_paintMask";
			((Texture)m_paintMask).wrapMode = (TextureWrapMode)1;
			m_materialInstance = new Material(m_material);
			m_materialInstance.SetTexture(s_shaderPropertyClearedMaskTex, (Texture)(object)m_paintMask);
			((Renderer)m_meshRenderer).sharedMaterial = m_materialInstance;
		}
	}

	private bool Generate()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (HeightmapBuilder.instance == null)
		{
			return false;
		}
		if (WorldGenerator.instance == null)
		{
			ZLog.LogError((object)"The WorldGenerator instance was null");
			throw new NullReferenceException("The WorldGenerator instance was null");
		}
		Initialize();
		int num = m_width + 1;
		int num2 = num * num;
		Vector3 position = ((Component)this).transform.position;
		if (m_buildData == null || m_buildData.m_baseHeights.Count != num2 || m_buildData.m_center != position || m_buildData.m_scale != m_scale || m_buildData.m_worldGen != WorldGenerator.instance)
		{
			m_buildData = HeightmapBuilder.instance.RequestTerrainSync(position, m_width, m_scale, m_isDistantLod, WorldGenerator.instance);
			m_cornerBiomes = m_buildData.m_cornerBiomes;
		}
		for (int i = 0; i < num2; i++)
		{
			m_heights[i] = m_buildData.m_baseHeights[i];
		}
		m_paintMask.SetPixels(m_buildData.m_baseMask);
		ApplyModifiers();
		return true;
	}

	private static float Distance(float x, float y, float rx, float ry)
	{
		float num = (float)((double)x - (double)rx);
		float num2 = (float)((double)y - (double)ry);
		float num3 = Mathf.Sqrt((float)((double)num * (double)num + (double)num2 * (double)num2));
		float num4 = (float)(1.4140000343322754 - (double)num3);
		return (float)((double)num4 * (double)num4 * (double)num4);
	}

	public bool HaveBiome(Biome biome)
	{
		if ((m_cornerBiomes[0] & biome) == 0 && (m_cornerBiomes[1] & biome) == 0 && (m_cornerBiomes[2] & biome) == 0)
		{
			return (m_cornerBiomes[3] & biome) != 0;
		}
		return true;
	}

	public Biome GetBiome(Vector3 point, float oceanLevel = 0.02f, bool waterAlwaysOcean = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (m_isDistantLod || waterAlwaysOcean)
		{
			return WorldGenerator.instance.GetBiome(point.x, point.z, oceanLevel, waterAlwaysOcean);
		}
		if (m_cornerBiomes[0] == m_cornerBiomes[1] && m_cornerBiomes[0] == m_cornerBiomes[2] && m_cornerBiomes[0] == m_cornerBiomes[3])
		{
			return m_cornerBiomes[0];
		}
		float x = point.x;
		float y = point.z;
		WorldToNormalizedHM(point, out x, out y);
		for (int i = 1; i < s_tempBiomeWeights.Length; i++)
		{
			s_tempBiomeWeights[i] = 0f;
		}
		s_tempBiomeWeights[s_biomeToIndex[m_cornerBiomes[0]]] += Distance(x, y, 0f, 0f);
		s_tempBiomeWeights[s_biomeToIndex[m_cornerBiomes[1]]] += Distance(x, y, 1f, 0f);
		s_tempBiomeWeights[s_biomeToIndex[m_cornerBiomes[2]]] += Distance(x, y, 0f, 1f);
		s_tempBiomeWeights[s_biomeToIndex[m_cornerBiomes[3]]] += Distance(x, y, 1f, 1f);
		int num = s_biomeToIndex[Biome.None];
		float num2 = -99999f;
		for (int j = 1; j < s_tempBiomeWeights.Length; j++)
		{
			if (s_tempBiomeWeights[j] > num2)
			{
				num = j;
				num2 = s_tempBiomeWeights[j];
			}
		}
		return s_indexToBiome[num];
	}

	public BiomeArea GetBiomeArea()
	{
		if (!IsBiomeEdge())
		{
			return BiomeArea.Median;
		}
		return BiomeArea.Edge;
	}

	public bool IsBiomeEdge()
	{
		if (m_cornerBiomes[0] == m_cornerBiomes[1] && m_cornerBiomes[0] == m_cornerBiomes[2])
		{
			return m_cornerBiomes[0] != m_cornerBiomes[3];
		}
		return true;
	}

	private void ApplyModifiers()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		List<TerrainModifier> allInstances = TerrainModifier.GetAllInstances();
		float[] array = null;
		float[] array2 = null;
		foreach (TerrainModifier item in allInstances)
		{
			if (((Behaviour)item).enabled && TerrainVSModifier(item))
			{
				if (item.m_playerModifiction && array == null)
				{
					array = m_heights.ToArray();
					array2 = m_heights.ToArray();
				}
				ApplyModifier(item, array, array2);
			}
		}
		TerrainComp terrainComp = (m_isDistantLod ? null : TerrainComp.FindTerrainCompiler(((Component)this).transform.position));
		if (Object.op_Implicit((Object)(object)terrainComp))
		{
			if (array == null)
			{
				array = m_heights.ToArray();
				array2 = m_heights.ToArray();
			}
			terrainComp.ApplyToHeightmap(m_paintMask, m_heights, array, array2, this);
		}
		m_paintMask.Apply();
	}

	private void ApplyModifier(TerrainModifier modifier, float[] baseHeights, float[] levelOnly)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (modifier.m_level)
		{
			LevelTerrain(((Component)modifier).transform.position + Vector3.up * modifier.m_levelOffset, modifier.m_levelRadius, modifier.m_square, baseHeights, levelOnly, modifier.m_playerModifiction);
		}
		if (modifier.m_smooth)
		{
			SmoothTerrain2(((Component)modifier).transform.position + Vector3.up * modifier.m_levelOffset, modifier.m_smoothRadius, modifier.m_square, levelOnly, modifier.m_smoothPower, modifier.m_playerModifiction);
		}
		if (modifier.m_paintCleared)
		{
			PaintCleared(((Component)modifier).transform.position, modifier.m_paintRadius, modifier.m_paintType, modifier.m_paintHeightCheck, apply: false);
		}
	}

	public bool CheckTerrainModIsContained(TerrainModifier modifier)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)modifier).transform.position;
		float num = modifier.GetRadius() + 0.1f;
		Vector3 position2 = ((Component)this).transform.position;
		float num2 = (float)m_width * m_scale * 0.5f;
		if (position.x + num > position2.x + num2)
		{
			return false;
		}
		if (position.x - num < position2.x - num2)
		{
			return false;
		}
		if (position.z + num > position2.z + num2)
		{
			return false;
		}
		if (position.z - num < position2.z - num2)
		{
			return false;
		}
		return true;
	}

	public bool TerrainVSModifier(TerrainModifier modifier)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)modifier).transform.position;
		float num = modifier.GetRadius() + 4f;
		Vector3 position2 = ((Component)this).transform.position;
		float num2 = (float)m_width * m_scale * 0.5f;
		if (position.x + num < position2.x - num2)
		{
			return false;
		}
		if (position.x - num > position2.x + num2)
		{
			return false;
		}
		if (position.z + num < position2.z - num2)
		{
			return false;
		}
		if (position.z - num > position2.z + num2)
		{
			return false;
		}
		return true;
	}

	private Vector3 CalcVertex(int x, int y)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		int num = m_width + 1;
		Vector3 val = new Vector3((float)((double)m_width * (double)m_scale * -0.5), 0f, (float)((double)m_width * (double)m_scale * -0.5));
		float num2 = m_heights[y * num + x];
		return val + new Vector3((float)((double)x * (double)m_scale), num2, (float)((double)y * (double)m_scale));
	}

	private Color GetBiomeColor(float ix, float iy)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (m_cornerBiomes[0] == m_cornerBiomes[1] && m_cornerBiomes[0] == m_cornerBiomes[2] && m_cornerBiomes[0] == m_cornerBiomes[3])
		{
			return Color32.op_Implicit(GetBiomeColor(m_cornerBiomes[0]));
		}
		Color32 biomeColor = GetBiomeColor(m_cornerBiomes[0]);
		Color32 biomeColor2 = GetBiomeColor(m_cornerBiomes[1]);
		Color32 biomeColor3 = GetBiomeColor(m_cornerBiomes[2]);
		Color32 biomeColor4 = GetBiomeColor(m_cornerBiomes[3]);
		Color32 val = Color32.Lerp(biomeColor, biomeColor2, ix);
		Color32 val2 = Color32.Lerp(biomeColor3, biomeColor4, ix);
		return Color32.op_Implicit(Color32.Lerp(val, val2, iy));
	}

	public static Color32 GetBiomeColor(Biome biome)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		return (Color32)(biome switch
		{
			Biome.Swamp => new Color32(byte.MaxValue, (byte)0, (byte)0, (byte)0), 
			Biome.Mountain => new Color32((byte)0, byte.MaxValue, (byte)0, (byte)0), 
			Biome.BlackForest => new Color32((byte)0, (byte)0, byte.MaxValue, (byte)0), 
			Biome.Plains => new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue), 
			Biome.AshLands => new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue), 
			Biome.DeepNorth => new Color32((byte)0, byte.MaxValue, (byte)0, (byte)0), 
			Biome.Mistlands => new Color32((byte)0, (byte)0, byte.MaxValue, byte.MaxValue), 
			_ => new Color32((byte)0, (byte)0, (byte)0, (byte)0), 
		});
	}

	private void RebuildCollisionMesh()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_collisionMesh == (Object)null)
		{
			m_collisionMesh = new Mesh();
			((Object)m_collisionMesh).name = "___Heightmap m_collisionMesh";
		}
		int num = m_width + 1;
		float num2 = -999999f;
		float num3 = 999999f;
		s_tempVertices.Clear();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 val = CalcVertex(j, i);
				s_tempVertices.Add(val);
				if (val.y > num2)
				{
					num2 = val.y;
				}
				if (val.y < num3)
				{
					num3 = val.y;
				}
			}
		}
		m_collisionMesh.SetVertices(s_tempVertices);
		int num4 = (num - 1) * (num - 1) * 6;
		if (m_collisionMesh.GetIndexCount(0) != num4)
		{
			s_tempIndices.Clear();
			for (int k = 0; k < num - 1; k++)
			{
				for (int l = 0; l < num - 1; l++)
				{
					int item = k * num + l;
					int item2 = k * num + l + 1;
					int item3 = (k + 1) * num + l + 1;
					int item4 = (k + 1) * num + l;
					s_tempIndices.Add(item);
					s_tempIndices.Add(item4);
					s_tempIndices.Add(item2);
					s_tempIndices.Add(item2);
					s_tempIndices.Add(item4);
					s_tempIndices.Add(item3);
				}
			}
			m_collisionMesh.SetIndices(s_tempIndices.ToArray(), (MeshTopology)0, 0);
		}
		if (Object.op_Implicit((Object)(object)m_collider))
		{
			m_collider.sharedMesh = m_collisionMesh;
		}
		float num5 = (float)m_width * m_scale * 0.5f;
		((Bounds)(ref m_bounds)).SetMinMax(((Component)this).transform.position + new Vector3(0f - num5, num3, 0f - num5), ((Component)this).transform.position + new Vector3(num5, num2, num5));
		m_boundingSphere.position = ((Bounds)(ref m_bounds)).center;
		m_boundingSphere.radius = Vector3.Distance(m_boundingSphere.position, ((Bounds)(ref m_bounds)).max);
	}

	private void RebuildRenderMesh()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_renderMesh == (Object)null)
		{
			m_renderMesh = new Mesh();
			((Object)m_renderMesh).name = "___Heightmap m_renderMesh";
		}
		WorldGenerator instance = WorldGenerator.instance;
		int num = m_width + 1;
		Vector3 val = ((Component)this).transform.position + new Vector3((float)((double)m_width * (double)m_scale * -0.5), 0f, (float)((double)m_width * (double)m_scale * -0.5));
		s_tempVertices.Clear();
		s_tempUVs.Clear();
		s_tempIndices.Clear();
		s_tempColors.Clear();
		for (int i = 0; i < num; i++)
		{
			float iy = DUtils.SmoothStep(0f, 1f, (float)((double)i / (double)m_width));
			for (int j = 0; j < num; j++)
			{
				float ix = DUtils.SmoothStep(0f, 1f, (float)((double)j / (double)m_width));
				s_tempUVs.Add(new Vector2((float)((double)j / (double)m_width), (float)((double)i / (double)m_width)));
				if (m_isDistantLod)
				{
					float wx = (float)((double)val.x + (double)j * (double)m_scale);
					float wy = (float)((double)val.z + (double)i * (double)m_scale);
					Biome biome = instance.GetBiome(wx, wy);
					s_tempColors.Add(GetBiomeColor(biome));
				}
				else
				{
					s_tempColors.Add(Color32.op_Implicit(GetBiomeColor(ix, iy)));
				}
			}
		}
		m_collisionMesh.GetVertices(s_tempVertices);
		m_collisionMesh.GetIndices(s_tempIndices, 0);
		m_renderMesh.Clear();
		m_renderMesh.SetVertices(s_tempVertices);
		m_renderMesh.SetColors(s_tempColors);
		m_renderMesh.SetUVs(0, s_tempUVs);
		m_renderMesh.SetIndices(s_tempIndices, (MeshTopology)0, 0, true, 0);
		m_renderMesh.RecalculateNormals();
		m_renderMesh.RecalculateTangents();
		m_renderMesh.RecalculateBounds();
		m_meshFilter.mesh = m_renderMesh;
	}

	private void SmoothTerrain2(Vector3 worldPos, float radius, bool square, float[] levelOnlyHeights, float power, bool playerModifiction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		WorldToVertex(worldPos, out var x, out var y);
		float num = (float)(double)(worldPos.y - ((Component)this).transform.position.y);
		float num2 = (float)(double)(radius / m_scale);
		int num3 = Mathf.CeilToInt(num2);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)x, (float)y);
		int num4 = m_width + 1;
		for (int i = y - num3; i <= y + num3; i++)
		{
			for (int j = x - num3; j <= x + num3; j++)
			{
				float num5 = Vector2.Distance(val, new Vector2((float)j, (float)i));
				if (num5 > num2)
				{
					continue;
				}
				float num6 = num5 / num2;
				if (j >= 0 && i >= 0 && j < num4 && i < num4)
				{
					num6 = ((power != 3f) ? Mathf.Pow(num6, power) : ((float)((double)num6 * (double)num6 * (double)num6)));
					float height = GetHeight(j, i);
					float num7 = (float)(1.0 - (double)num6);
					float num8 = DUtils.Lerp(height, num, num7);
					if (playerModifiction)
					{
						float num9 = levelOnlyHeights[i * num4 + j];
						num8 = Mathf.Clamp(num8, (float)((double)num9 - 1.0), (float)((double)num9 + 1.0));
					}
					SetHeight(j, i, num8);
				}
			}
		}
	}

	private bool AtMaxWorldLevelDepth(Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		GetWorldHeight(worldPos, out var height);
		GetWorldBaseHeight(worldPos, out var height2);
		return Mathf.Max(0f - (float)((double)height - (double)height2), 0f) >= 7.95f;
	}

	private bool GetWorldBaseHeight(Vector3 worldPos, out float height)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		WorldToVertex(worldPos, out var x, out var y);
		int num = m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			height = 0f;
			return false;
		}
		height = (float)((double)m_buildData.m_baseHeights[y * num + x] + (double)((Component)this).transform.position.y);
		return true;
	}

	private bool GetWorldHeight(Vector3 worldPos, out float height)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		WorldToVertex(worldPos, out var x, out var y);
		int num = m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			height = 0f;
			return false;
		}
		height = (float)((double)m_heights[y * num + x] + (double)((Component)this).transform.position.y);
		return true;
	}

	public static bool AtMaxLevelDepth(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Heightmap heightmap = FindHeightmap(worldPos);
		if (Object.op_Implicit((Object)(object)heightmap))
		{
			return heightmap.AtMaxWorldLevelDepth(worldPos);
		}
		return false;
	}

	public static bool GetHeight(Vector3 worldPos, out float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Heightmap heightmap = FindHeightmap(worldPos);
		if (Object.op_Implicit((Object)(object)heightmap) && heightmap.GetWorldHeight(worldPos, out height))
		{
			return true;
		}
		height = 0f;
		return false;
	}

	private void PaintCleared(Vector3 worldPos, float radius, TerrainModifier.PaintType paintType, bool heightCheck, bool apply)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		float num = worldPos.y - ((Component)this).transform.position.y;
		WorldToVertexMask(worldPos, out var x, out var y);
		float num2 = radius / m_scale;
		int num3 = Mathf.CeilToInt(num2);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)x, (float)y);
		for (int i = y - num3; i <= y + num3; i++)
		{
			for (int j = x - num3; j <= x + num3; j++)
			{
				if (j >= 0 && i >= 0 && j < ((Texture)m_paintMask).width + 1 && i < ((Texture)m_paintMask).height + 1 && (!heightCheck || !(GetHeight(j, i) > num)))
				{
					float num4 = Vector2.Distance(val, new Vector2((float)j, (float)i));
					float num5 = 1f - Mathf.Clamp01(num4 / num2);
					num5 = Mathf.Pow(num5, 0.1f);
					Color val2 = m_paintMask.GetPixel(j, i);
					float a = val2.a;
					switch (paintType)
					{
					case TerrainModifier.PaintType.Dirt:
						val2 = Color.Lerp(val2, m_paintMaskDirt, num5);
						break;
					case TerrainModifier.PaintType.Cultivate:
						val2 = Color.Lerp(val2, m_paintMaskCultivated, num5);
						break;
					case TerrainModifier.PaintType.Paved:
						val2 = Color.Lerp(val2, m_paintMaskPaved, num5);
						break;
					case TerrainModifier.PaintType.Reset:
						val2 = Color.Lerp(val2, m_paintMaskNothing, num5);
						break;
					case TerrainModifier.PaintType.ClearVegetation:
						val2 = Color.Lerp(val2, m_paintMaskClearVegetation, num5);
						break;
					}
					if (paintType != TerrainModifier.PaintType.ClearVegetation)
					{
						val2.a = a;
					}
					m_paintMask.SetPixel(j, i, val2);
				}
			}
		}
		if (apply)
		{
			m_paintMask.Apply();
		}
	}

	public float GetVegetationMask(Vector3 worldPos)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		WorldToVertexMask(worldPos, out var x, out var y);
		return m_paintMask.GetPixel(x, y).a;
	}

	public bool IsCleared(Vector3 worldPos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		worldPos.x = (float)((double)worldPos.x - 0.5);
		worldPos.z = (float)((double)worldPos.z - 0.5);
		WorldToVertexMask(worldPos, out var x, out var y);
		Color pixel = m_paintMask.GetPixel(x, y);
		if (!(pixel.r > 0.5f) && !(pixel.g > 0.5f))
		{
			return pixel.b > 0.5f;
		}
		return true;
	}

	public bool IsCultivated(Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		WorldToVertexMask(worldPos, out var x, out var y);
		return m_paintMask.GetPixel(x, y).g > 0.5f;
	}

	public bool IsLava(Vector3 worldPos, float lavaValue = 0.6f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (GetBiome(worldPos) != Biome.AshLands || IsBiomeEdge())
		{
			return false;
		}
		if (GetVegetationMask(worldPos) > lavaValue)
		{
			return true;
		}
		return false;
	}

	public float GetLava(Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (GetBiome(worldPos) != Biome.AshLands || IsBiomeEdge())
		{
			return 0f;
		}
		return GetVegetationMask(worldPos);
	}

	public float GetHeightOffset(Vector3 worldPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (GetBiome(worldPos) == Biome.AshLands)
		{
			if (IsBiomeEdge())
			{
				return GetGroundMaterialOffset(FootStep.GroundMaterial.Ashlands);
			}
			float vegetationMask = GetVegetationMask(worldPos);
			return Mathf.Lerp(GetGroundMaterialOffset(FootStep.GroundMaterial.Ashlands), GetGroundMaterialOffset(FootStep.GroundMaterial.Lava), vegetationMask);
		}
		return 0f;
	}

	public void WorldToVertex(Vector3 worldPos, out int x, out int y)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = worldPos - ((Component)this).transform.position;
		int num = m_width / 2;
		x = Mathf.FloorToInt(val.x / m_scale + 0.5f) + num;
		y = Mathf.FloorToInt(val.z / m_scale + 0.5f) + num;
	}

	public void WorldToVertexMask(Vector3 worldPos, out int x, out int y)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = worldPos - ((Component)this).transform.position;
		int num = (m_width + 1) / 2;
		x = Mathf.FloorToInt(val.x / m_scale + 0.5f) + num;
		y = Mathf.FloorToInt(val.z / m_scale + 0.5f) + num;
	}

	private void WorldToNormalizedHM(Vector3 worldPos, out float x, out float y)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)m_width * m_scale;
		Vector3 val = worldPos - ((Component)this).transform.position;
		x = val.x / num + 0.5f;
		y = val.z / num + 0.5f;
	}

	private void LevelTerrain(Vector3 worldPos, float radius, bool square, float[] baseHeights, float[] levelOnly, bool playerModifiction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		WorldToVertexMask(worldPos, out var x, out var y);
		Vector3 val = worldPos - ((Component)this).transform.position;
		float num = (float)((double)radius / (double)m_scale);
		int num2 = Mathf.CeilToInt(num);
		int num3 = m_width + 1;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector((float)x, (float)y);
		for (int i = y - num2; i <= y + num2; i++)
		{
			for (int j = x - num2; j <= x + num2; j++)
			{
				if ((square || !(Vector2.Distance(val2, new Vector2((float)j, (float)i)) > num)) && j >= 0 && i >= 0 && j < num3 && i < num3)
				{
					float num4 = val.y;
					if (playerModifiction)
					{
						float num5 = baseHeights[i * num3 + j];
						num4 = (levelOnly[i * num3 + j] = Mathf.Clamp(num4, (float)((double)num5 - 8.0), (float)((double)num5 + 8.0)));
					}
					SetHeight(j, i, num4);
				}
			}
		}
	}

	public Color GetPaintMask(int x, int y)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (x < 0 || y < 0 || x >= ((Texture)m_paintMask).width || y >= ((Texture)m_paintMask).height)
		{
			return Color.black;
		}
		return m_paintMask.GetPixel(x, y);
	}

	public Texture2D GetPaintMask()
	{
		return m_paintMask;
	}

	private void SetPaintMask(int x, int y, Color paint)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (x >= 0 && y >= 0 && x < m_width && y < m_width)
		{
			m_paintMask.SetPixel(x, y, paint);
		}
	}

	public float GetHeight(int x, int y)
	{
		int num = m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			return 0f;
		}
		return m_heights[y * num + x];
	}

	public void SetHeight(int x, int y, float h)
	{
		int num = m_width + 1;
		if (x >= 0 && y >= 0 && x < num && y < num)
		{
			m_heights[y * num + x] = h;
		}
	}

	public bool IsPointInside(Vector3 point, float radius = 0f)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)((double)m_width * (double)m_scale * 0.5);
		Vector3 position = ((Component)this).transform.position;
		if ((float)((double)point.x + (double)radius) >= (float)((double)position.x - (double)num) && (float)((double)point.x - (double)radius) <= (float)((double)position.x + (double)num) && (float)((double)point.z + (double)radius) >= (float)((double)position.z - (double)num) && (float)((double)point.z - (double)radius) <= (float)((double)position.z + (double)num))
		{
			return true;
		}
		return false;
	}

	public static List<Heightmap> GetAllHeightmaps()
	{
		return s_heightmaps;
	}

	public static Heightmap FindHeightmap(Vector3 point)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		foreach (Heightmap s_heightmap in s_heightmaps)
		{
			if (s_heightmap.IsPointInside(point))
			{
				return s_heightmap;
			}
		}
		return null;
	}

	public static void FindHeightmap(Vector3 point, float radius, List<Heightmap> heightmaps)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		foreach (Heightmap s_heightmap in s_heightmaps)
		{
			if (s_heightmap.IsPointInside(point, radius))
			{
				heightmaps.Add(s_heightmap);
			}
		}
	}

	public static Biome FindBiome(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Heightmap heightmap = FindHeightmap(point);
		if (!Object.op_Implicit((Object)(object)heightmap))
		{
			return Biome.None;
		}
		return heightmap.GetBiome(point);
	}

	public static bool HaveQueuedRebuild(Vector3 point, float radius)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		s_tempHmaps.Clear();
		FindHeightmap(point, radius, s_tempHmaps);
		foreach (Heightmap s_tempHmap in s_tempHmaps)
		{
			if (s_tempHmap.HaveQueuedRebuild())
			{
				return true;
			}
		}
		return false;
	}

	public static void UpdateTerrainAlpha()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return;
		}
		List<Heightmap> list = new List<Heightmap>();
		FindHeightmap(((Component)Player.m_localPlayer).transform.position, 150f, list);
		bool flag = false;
		foreach (Heightmap item in list)
		{
			if (UpdateTerrainAlpha(item))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			Console.instance.Print("Nothing to update");
		}
		else
		{
			Console.instance.Print("Updated terrain alpha");
		}
	}

	public static bool UpdateTerrainAlpha(Heightmap hmap)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		HeightmapBuilder.HMBuildData hMBuildData = HeightmapBuilder.instance.RequestTerrainSync(((Component)hmap).transform.position, hmap.m_width, hmap.m_scale, hmap.IsDistantLod, WorldGenerator.instance);
		int num = 0;
		for (int i = 0; i < hmap.m_width; i++)
		{
			for (int j = 0; j < hmap.m_width; j++)
			{
				int num2 = i * hmap.m_width + j;
				float a = hMBuildData.m_baseMask[num2].a;
				Color paintMask = hmap.GetPaintMask(j, i);
				if (a != paintMask.a)
				{
					paintMask.a = a;
					hmap.SetPaintMask(j, i, paintMask);
					num++;
				}
			}
		}
		if (num > 0)
		{
			hmap.GetAndCreateTerrainCompiler().UpdatePaintMask(hmap);
		}
		return num > 0;
	}

	public FootStep.GroundMaterial GetGroundMaterial(Vector3 groundNormal, Vector3 point, float lavaValue = 0.6f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Acos(Mathf.Clamp01(groundNormal.y)) * 57.29578f;
		switch (GetBiome(point))
		{
		case Biome.Mountain:
		case Biome.DeepNorth:
			if (num < 40f && !IsCleared(point))
			{
				return FootStep.GroundMaterial.Snow;
			}
			break;
		case Biome.Swamp:
			if (num < 40f)
			{
				return FootStep.GroundMaterial.Mud;
			}
			break;
		case Biome.Meadows:
		case Biome.BlackForest:
			if (num < 25f)
			{
				return FootStep.GroundMaterial.Grass;
			}
			break;
		case Biome.AshLands:
			if (IsLava(point, lavaValue))
			{
				return FootStep.GroundMaterial.Lava;
			}
			return FootStep.GroundMaterial.Ashlands;
		}
		return FootStep.GroundMaterial.GenericGround;
	}

	public static float GetGroundMaterialOffset(FootStep.GroundMaterial material)
	{
		return material switch
		{
			FootStep.GroundMaterial.Snow => 0.1f, 
			FootStep.GroundMaterial.Ashlands => 0.1f, 
			FootStep.GroundMaterial.Lava => 0.8f, 
			_ => 0f, 
		};
	}

	public static Biome FindBiomeClutter(Vector3 point)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ZoneSystem.instance) && !ZoneSystem.instance.IsZoneLoaded(point))
		{
			return Biome.None;
		}
		Heightmap heightmap = FindHeightmap(point);
		if (Object.op_Implicit((Object)(object)heightmap))
		{
			return heightmap.GetBiome(point);
		}
		return Biome.None;
	}

	public void Clear()
	{
		m_heights.Clear();
		m_paintMask = null;
		m_materialInstance = null;
		m_buildData = null;
		if (Object.op_Implicit((Object)(object)m_collisionMesh))
		{
			m_collisionMesh.Clear();
		}
		if (Object.op_Implicit((Object)(object)m_renderMesh))
		{
			m_renderMesh.Clear();
		}
		if (Object.op_Implicit((Object)(object)m_collider))
		{
			m_collider.sharedMesh = null;
		}
	}

	public TerrainComp GetAndCreateTerrainCompiler()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(((Component)this).transform.position);
		if (Object.op_Implicit((Object)(object)terrainComp))
		{
			return terrainComp;
		}
		return Object.Instantiate<GameObject>(m_terrainCompilerPrefab, ((Component)this).transform.position, Quaternion.identity).GetComponent<TerrainComp>();
	}
}
