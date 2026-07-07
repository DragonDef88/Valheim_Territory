using System.Collections.Generic;
using UnityEngine;

public class TerrainComp : MonoBehaviour
{
	private const int terrainCompVersion = 1;

	private static readonly List<TerrainComp> s_instances = new List<TerrainComp>();

	private bool m_initialized;

	private int m_width;

	private float m_size;

	private int m_operations;

	private bool[] m_modifiedHeight;

	private float[] m_levelDelta;

	private float[] m_smoothDelta;

	private bool[] m_modifiedPaint;

	private Color[] m_paintMask;

	private Heightmap m_hmap;

	private ZNetView m_nview;

	private uint m_lastDataRevision = uint.MaxValue;

	private Vector3 m_lastOpPoint;

	private float m_lastOpRadius;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_hmap = Heightmap.FindHeightmap(((Component)this).transform.position);
		if ((Object)(object)m_hmap == (Object)null)
		{
			ZLog.LogWarning((object)"Terrain compiler could not find hmap");
			return;
		}
		TerrainComp terrainComp = FindTerrainCompiler(((Component)this).transform.position);
		if (Object.op_Implicit((Object)(object)terrainComp))
		{
			ZLog.LogWarning((object)"Found another terrain compiler in this area, removing it");
			ZNetScene.instance.Destroy(((Component)terrainComp).gameObject);
		}
		s_instances.Add(this);
		m_nview.Register<ZPackage>("ApplyOperation", RPC_ApplyOperation);
		Initialize();
		CheckLoad();
	}

	private void OnDestroy()
	{
		s_instances.Remove(this);
	}

	private void Update()
	{
		if (m_nview.IsValid())
		{
			CheckLoad();
		}
	}

	private void Initialize()
	{
		m_initialized = true;
		m_width = m_hmap.m_width;
		m_size = (float)m_width * m_hmap.m_scale;
		int num = m_width + 1;
		m_modifiedHeight = new bool[num * num];
		m_levelDelta = new float[num * num];
		m_smoothDelta = new float[num * num];
		m_modifiedPaint = new bool[num * num];
		m_paintMask = (Color[])(object)new Color[num * num];
	}

	private void Save()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_initialized || !m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		ZPackage zPackage = new ZPackage();
		zPackage.Write(1);
		zPackage.Write(m_operations);
		zPackage.Write(m_lastOpPoint);
		zPackage.Write(m_lastOpRadius);
		zPackage.Write(m_modifiedHeight.Length);
		for (int i = 0; i < m_modifiedHeight.Length; i++)
		{
			zPackage.Write(m_modifiedHeight[i]);
			if (m_modifiedHeight[i])
			{
				zPackage.Write(m_levelDelta[i]);
				zPackage.Write(m_smoothDelta[i]);
			}
		}
		zPackage.Write(m_modifiedPaint.Length);
		for (int j = 0; j < m_modifiedPaint.Length; j++)
		{
			zPackage.Write(m_modifiedPaint[j]);
			if (m_modifiedPaint[j])
			{
				zPackage.Write(m_paintMask[j].r);
				zPackage.Write(m_paintMask[j].g);
				zPackage.Write(m_paintMask[j].b);
				zPackage.Write(m_paintMask[j].a);
			}
		}
		byte[] bytes = Utils.Compress(zPackage.GetArray());
		m_nview.GetZDO().Set(ZDOVars.s_TCData, bytes);
		m_lastDataRevision = m_nview.GetZDO().DataRevision;
	}

	private void CheckLoad()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.GetZDO().DataRevision == m_lastDataRevision)
		{
			return;
		}
		int operations = m_operations;
		if (!Load())
		{
			return;
		}
		m_hmap.Poke(delayed: false);
		if (Object.op_Implicit((Object)(object)ClutterSystem.instance))
		{
			if (m_operations == operations + 1)
			{
				ClutterSystem.instance.ResetGrass(m_lastOpPoint, m_lastOpRadius);
			}
			else
			{
				ClutterSystem.instance.ResetGrass(((Component)m_hmap).transform.position, (float)m_hmap.m_width * m_hmap.m_scale / 2f);
			}
		}
	}

	private bool Load()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		m_lastDataRevision = m_nview.GetZDO().DataRevision;
		byte[] byteArray = m_nview.GetZDO().GetByteArray(ZDOVars.s_TCData);
		if (byteArray == null)
		{
			return false;
		}
		ZPackage zPackage = new ZPackage(Utils.Decompress(byteArray));
		zPackage.ReadInt();
		m_operations = zPackage.ReadInt();
		m_lastOpPoint = zPackage.ReadVector3();
		m_lastOpRadius = zPackage.ReadSingle();
		int num = zPackage.ReadInt();
		if (num != m_modifiedHeight.Length)
		{
			ZLog.LogWarning((object)"Terrain data load error, height array missmatch");
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			m_modifiedHeight[i] = zPackage.ReadBool();
			if (m_modifiedHeight[i])
			{
				m_levelDelta[i] = zPackage.ReadSingle();
				m_smoothDelta[i] = zPackage.ReadSingle();
			}
			else
			{
				m_levelDelta[i] = 0f;
				m_smoothDelta[i] = 0f;
			}
		}
		int num2 = zPackage.ReadInt();
		for (int j = 0; j < num2; j++)
		{
			m_modifiedPaint[j] = zPackage.ReadBool();
			if (m_modifiedPaint[j])
			{
				Color val = default(Color);
				val.r = zPackage.ReadSingle();
				val.g = zPackage.ReadSingle();
				val.b = zPackage.ReadSingle();
				val.a = zPackage.ReadSingle();
				m_paintMask[j] = val;
			}
			else
			{
				m_paintMask[j] = Color.black;
			}
		}
		if (num2 == m_width * m_width)
		{
			Color[] array = (Color[])(object)new Color[m_paintMask.Length];
			m_paintMask.CopyTo(array, 0);
			bool[] array2 = new bool[m_modifiedPaint.Length];
			m_modifiedPaint.CopyTo(array2, 0);
			int num3 = m_width + 1;
			for (int k = 0; k < m_paintMask.Length; k++)
			{
				int num4 = k / num3;
				int num5 = (k + 1) / num3;
				int num6 = k - num4;
				if (num4 == m_width)
				{
					num6 -= m_width;
				}
				if (k > 0 && (k - num4) % m_width == 0 && (k + 1 - num5) % m_width == 0)
				{
					num6--;
				}
				m_paintMask[k] = array[num6];
				m_modifiedPaint[k] = array2[num6];
			}
		}
		return true;
	}

	public static TerrainComp FindTerrainCompiler(Vector3 pos)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		foreach (TerrainComp s_instance in s_instances)
		{
			float num = s_instance.m_size / 2f;
			Vector3 position = ((Component)s_instance).transform.position;
			if (pos.x >= position.x - num && pos.x <= position.x + num && pos.z >= position.z - num && pos.z <= position.z + num)
			{
				return s_instance;
			}
		}
		return null;
	}

	public void ApplyToHeightmap(Texture2D clearedMask, List<float> heights, float[] baseHeights, float[] levelOnlyHeights, Heightmap hm)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (!m_initialized)
		{
			return;
		}
		int num = m_width + 1;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num2 = i * num + j;
				float num3 = m_levelDelta[num2];
				float num4 = m_smoothDelta[num2];
				if (num3 != 0f || num4 != 0f)
				{
					float num5 = heights[num2];
					float num6 = baseHeights[num2];
					float num7 = num5 + num3 + num4;
					num7 = Mathf.Clamp(num7, num6 - 8f, num6 + 8f);
					heights[num2] = num7;
				}
			}
		}
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				int num8 = k * num + l;
				if (m_modifiedPaint[num8])
				{
					clearedMask.SetPixel(l, k, m_paintMask[num8]);
				}
			}
		}
	}

	public void ApplyOperation(TerrainOp modifier)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		ZPackage zPackage = new ZPackage();
		zPackage.Write(((Component)modifier).transform.position);
		modifier.m_settings.Serialize(zPackage);
		m_nview.InvokeRPC("ApplyOperation", zPackage);
	}

	private void RPC_ApplyOperation(long sender, ZPackage pkg)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			TerrainOp.Settings settings = new TerrainOp.Settings();
			Vector3 pos = pkg.ReadVector3();
			settings.Deserialize(pkg);
			DoOperation(pos, settings);
		}
	}

	private void DoOperation(Vector3 pos, TerrainOp.Settings modifier)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (m_initialized)
		{
			InternalDoOperation(pos, modifier);
			Save();
			m_hmap.Poke(delayed: false);
			if (Object.op_Implicit((Object)(object)ClutterSystem.instance))
			{
				ClutterSystem.instance.ResetGrass(pos, modifier.GetRadius());
			}
		}
	}

	private void InternalDoOperation(Vector3 pos, TerrainOp.Settings modifier)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (modifier.m_level)
		{
			LevelTerrain(pos + Vector3.up * modifier.m_levelOffset, modifier.m_levelRadius, modifier.m_square);
		}
		if (modifier.m_raise)
		{
			RaiseTerrain(pos, modifier.m_raiseRadius, modifier.m_raiseDelta, modifier.m_square, modifier.m_raisePower);
		}
		if (modifier.m_smooth)
		{
			SmoothTerrain(pos + Vector3.up * modifier.m_levelOffset, modifier.m_smoothRadius, modifier.m_square, modifier.m_smoothPower);
		}
		if (modifier.m_paintCleared)
		{
			PaintCleared(pos, modifier.m_paintRadius, modifier.m_paintType, modifier.m_paintHeightCheck, apply: false);
		}
		m_operations++;
		m_lastOpPoint = pos;
		m_lastOpRadius = modifier.GetRadius();
	}

	private void LevelTerrain(Vector3 worldPos, float radius, bool square)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		m_hmap.WorldToVertex(worldPos, out var x, out var y);
		Vector3 val = worldPos - ((Component)this).transform.position;
		float num = radius / m_hmap.m_scale;
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
					float height = m_hmap.GetHeight(j, i);
					float num4 = val.y - height;
					int num5 = i * num3 + j;
					num4 += m_smoothDelta[num5];
					m_smoothDelta[num5] = 0f;
					m_levelDelta[num5] += num4;
					m_levelDelta[num5] = Mathf.Clamp(m_levelDelta[num5], -8f, 8f);
					m_modifiedHeight[num5] = true;
				}
			}
		}
	}

	private void RaiseTerrain(Vector3 worldPos, float radius, float delta, bool square, float power)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		m_hmap.WorldToVertex(worldPos, out var x, out var y);
		Vector3 val = worldPos - ((Component)this).transform.position;
		float num = radius / m_hmap.m_scale;
		int num2 = Mathf.CeilToInt(num);
		int num3 = m_width + 1;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector((float)x, (float)y);
		for (int i = y - num2; i <= y + num2; i++)
		{
			for (int j = x - num2; j <= x + num2; j++)
			{
				if (j < 0 || i < 0 || j >= num3 || i >= num3)
				{
					continue;
				}
				float num4 = 1f;
				if (!square)
				{
					float num5 = Vector2.Distance(val2, new Vector2((float)j, (float)i));
					if (num5 > num)
					{
						continue;
					}
					if (power > 0f)
					{
						num4 = num5 / num;
						num4 = 1f - num4;
						if (power != 1f)
						{
							num4 = Mathf.Pow(num4, power);
						}
					}
				}
				float height = m_hmap.GetHeight(j, i);
				float num6 = delta * num4;
				float num7 = val.y + num6;
				if (delta < 0f && num7 > height)
				{
					continue;
				}
				if (delta > 0f)
				{
					if (num7 < height)
					{
						continue;
					}
					if (num7 > height + num6)
					{
						num7 = height + num6;
					}
				}
				int num8 = i * num3 + j;
				float num9 = num7 - height + m_smoothDelta[num8];
				m_smoothDelta[num8] = 0f;
				m_levelDelta[num8] += num9;
				m_levelDelta[num8] = Mathf.Clamp(m_levelDelta[num8], -8f, 8f);
				m_modifiedHeight[num8] = true;
			}
		}
	}

	private void SmoothTerrain(Vector3 worldPos, float radius, bool square, float power)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		m_hmap.WorldToVertex(worldPos, out var x, out var y);
		float num = worldPos.y - ((Component)this).transform.position.y;
		float num2 = radius / m_hmap.m_scale;
		int num3 = Mathf.CeilToInt(num2);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)x, (float)y);
		int num4 = m_width + 1;
		for (int i = y - num3; i <= y + num3; i++)
		{
			for (int j = x - num3; j <= x + num3; j++)
			{
				float num5 = Vector2.Distance(val, new Vector2((float)j, (float)i));
				if (!(num5 > num2) && j >= 0 && i >= 0 && j < num4 && i < num4)
				{
					float num6 = num5 / num2;
					num6 = ((power != 3f) ? Mathf.Pow(num6, power) : (num6 * num6 * num6));
					float height = m_hmap.GetHeight(j, i);
					float num7 = 1f - num6;
					float num8 = Mathf.Lerp(height, num, num7) - height;
					int num9 = i * num4 + j;
					m_smoothDelta[num9] += num8;
					m_smoothDelta[num9] = Mathf.Clamp(m_smoothDelta[num9], -1f, 1f);
					m_modifiedHeight[num9] = true;
				}
			}
		}
	}

	private void PaintCleared(Vector3 worldPos, float radius, TerrainModifier.PaintType paintType, bool heightCheck, bool apply)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		float num = worldPos.y - ((Component)this).transform.position.y;
		m_hmap.WorldToVertexMask(worldPos, out var x, out var y);
		float num2 = radius / m_hmap.m_scale;
		int num3 = Mathf.CeilToInt(num2);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)x, (float)y);
		for (int i = y - num3; i <= y + num3; i++)
		{
			for (int j = x - num3; j <= x + num3; j++)
			{
				float num4 = Vector2.Distance(val, new Vector2((float)j, (float)i));
				int num5 = m_width + 1;
				if (j >= 0 && i >= 0 && j < num5 && i < num5 && (!heightCheck || !(m_hmap.GetHeight(j, i) > num)))
				{
					float num6 = 1f - Mathf.Clamp01(num4 / num2);
					num6 = Mathf.Pow(num6, 0.1f);
					Color val2 = m_hmap.GetPaintMask(j, i);
					float a = val2.a;
					switch (paintType)
					{
					case TerrainModifier.PaintType.Dirt:
						val2 = Color.Lerp(val2, Heightmap.m_paintMaskDirt, num6);
						break;
					case TerrainModifier.PaintType.Cultivate:
						val2 = Color.Lerp(val2, Heightmap.m_paintMaskCultivated, num6);
						break;
					case TerrainModifier.PaintType.Paved:
						val2 = Color.Lerp(val2, Heightmap.m_paintMaskPaved, num6);
						break;
					case TerrainModifier.PaintType.Reset:
						val2 = Color.Lerp(val2, Heightmap.m_paintMaskNothing, num6);
						break;
					}
					val2.a = a;
					m_modifiedPaint[i * num5 + j] = true;
					m_paintMask[i * num5 + j] = val2;
				}
			}
		}
	}

	public bool IsOwner()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.IsOwner();
	}

	public void UpdatePaintMask(Heightmap hmap)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_width; i++)
		{
			for (int j = 0; j < m_width; j++)
			{
				int num = i * m_width + j;
				if (m_modifiedPaint[num])
				{
					Color val = m_paintMask[num];
					val.a = hmap.GetPaintMask(j, i).a;
					m_paintMask[num] = val;
				}
			}
		}
		Save();
		hmap.Poke(delayed: false);
	}

	public static void UpgradeTerrain()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return;
		}
		List<Heightmap> list = new List<Heightmap>();
		Heightmap.FindHeightmap(((Component)Player.m_localPlayer).transform.position, 150f, list);
		bool flag = false;
		foreach (Heightmap item in list)
		{
			if (UpgradeTerrain(item))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			Console.instance.Print("Nothing to optimize");
		}
		else
		{
			Console.instance.Print("Optimized terrain");
		}
	}

	public static bool UpgradeTerrain(Heightmap hmap)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		List<TerrainModifier> allInstances = TerrainModifier.GetAllInstances();
		int num = 0;
		List<TerrainModifier> list = new List<TerrainModifier>();
		foreach (TerrainModifier item in allInstances)
		{
			ZNetView component = ((Component)item).GetComponent<ZNetView>();
			if (!((Object)(object)component == (Object)null) && component.IsValid() && component.IsOwner() && item.m_playerModifiction)
			{
				if (!hmap.CheckTerrainModIsContained(item))
				{
					num++;
				}
				else
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		TerrainComp andCreateTerrainCompiler = hmap.GetAndCreateTerrainCompiler();
		if (!andCreateTerrainCompiler.IsOwner())
		{
			Console instance = Console.instance;
			Vector3 position = ((Component)hmap).transform.position;
			instance.Print("Skipping terrain at " + ((object)(Vector3)(ref position)).ToString() + " ( another player is currently the owner )");
			return false;
		}
		int num2 = andCreateTerrainCompiler.m_width + 1;
		float[] array = new float[andCreateTerrainCompiler.m_modifiedHeight.Length];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				array[i * num2 + j] = hmap.GetHeight(j, i);
			}
		}
		Color[] array2 = (Color[])(object)new Color[andCreateTerrainCompiler.m_paintMask.Length];
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				array2[k * num2 + l] = hmap.GetPaintMask(l, k);
			}
		}
		foreach (TerrainModifier item2 in list)
		{
			((Behaviour)item2).enabled = false;
			((Component)item2).GetComponent<ZNetView>().Destroy();
		}
		hmap.Poke(delayed: false);
		int num3 = 0;
		for (int m = 0; m < num2; m++)
		{
			for (int n = 0; n < num2; n++)
			{
				int num4 = m * num2 + n;
				float num5 = array[num4];
				float height = hmap.GetHeight(n, m);
				float num6 = num5 - height;
				if (!(Mathf.Abs(num6) < 0.001f))
				{
					andCreateTerrainCompiler.m_modifiedHeight[num4] = true;
					andCreateTerrainCompiler.m_levelDelta[num4] += num6;
					num3++;
				}
			}
		}
		int num7 = 0;
		for (int num8 = 0; num8 < num2; num8++)
		{
			for (int num9 = 0; num9 < num2; num9++)
			{
				int num10 = num8 * num2 + num9;
				Color val = array2[num10];
				Color paintMask = hmap.GetPaintMask(num9, num8);
				if (!(val == paintMask))
				{
					andCreateTerrainCompiler.m_modifiedPaint[num10] = true;
					andCreateTerrainCompiler.m_paintMask[num10] = val;
					num7++;
				}
			}
		}
		andCreateTerrainCompiler.Save();
		hmap.Poke(delayed: false);
		if (Object.op_Implicit((Object)(object)ClutterSystem.instance))
		{
			ClutterSystem.instance.ResetGrass(((Component)hmap).transform.position, (float)hmap.m_width * hmap.m_scale / 2f);
		}
		Console.instance.Print("Operations optimized:" + list.Count + "  height changes:" + num3 + "  paint changes:" + num7);
		return true;
	}
}
