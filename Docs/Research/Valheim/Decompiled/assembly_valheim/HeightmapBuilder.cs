using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class HeightmapBuilder
{
	public class HMBuildData
	{
		public Vector3 m_center;

		public int m_width;

		public float m_scale;

		public bool m_distantLod;

		public bool m_menu;

		public WorldGenerator m_worldGen;

		public Heightmap.Biome[] m_cornerBiomes;

		public List<float> m_baseHeights;

		public Color[] m_baseMask;

		public HMBuildData(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			m_center = center;
			m_width = width;
			m_scale = scale;
			m_distantLod = distantLod;
			m_worldGen = worldGen;
		}

		public bool IsEqual(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			if (m_center == center && m_width == width && m_scale == scale && m_distantLod == distantLod)
			{
				return m_worldGen == worldGen;
			}
			return false;
		}
	}

	private static bool hasBeenDisposed;

	private static HeightmapBuilder m_instance;

	private const int m_maxReadyQueue = 16;

	private List<HMBuildData> m_toBuild = new List<HMBuildData>();

	private List<HMBuildData> m_ready = new List<HMBuildData>();

	private Thread m_builder;

	private Mutex m_lock = new Mutex();

	private bool m_stop;

	public static HeightmapBuilder instance
	{
		get
		{
			if (hasBeenDisposed)
			{
				ZLog.LogWarning((object)"Tried to get instance of heightmap builder after heightmap builder has been disposed!");
				return null;
			}
			if (m_instance == null)
			{
				m_instance = new HeightmapBuilder();
			}
			return m_instance;
		}
	}

	private HeightmapBuilder()
	{
		m_instance = this;
		m_builder = new Thread(BuildThread);
		m_builder.Start();
	}

	public void Dispose()
	{
		if (!hasBeenDisposed)
		{
			hasBeenDisposed = true;
			if (m_builder != null)
			{
				ZLog.Log((object)"Stopping build thread");
				m_lock.WaitOne();
				m_stop = true;
				m_lock.ReleaseMutex();
				m_builder.Join();
				m_builder = null;
			}
			if (m_lock != null)
			{
				m_lock.Close();
				m_lock = null;
			}
		}
	}

	private void BuildThread()
	{
		ZLog.Log((object)"Builder started");
		bool flag = false;
		while (!flag)
		{
			m_lock.WaitOne();
			bool num = m_toBuild.Count > 0;
			m_lock.ReleaseMutex();
			if (num)
			{
				m_lock.WaitOne();
				HMBuildData hMBuildData = m_toBuild[0];
				m_lock.ReleaseMutex();
				new Stopwatch().Start();
				Build(hMBuildData);
				m_lock.WaitOne();
				m_toBuild.Remove(hMBuildData);
				m_ready.Add(hMBuildData);
				while (m_ready.Count > 16)
				{
					m_ready.RemoveAt(0);
				}
				m_lock.ReleaseMutex();
			}
			Thread.Sleep(10);
			m_lock.WaitOne();
			flag = m_stop;
			m_lock.ReleaseMutex();
		}
	}

	private void Build(HMBuildData data)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		int num = data.m_width + 1;
		int num2 = num * num;
		Vector3 val = data.m_center + new Vector3((float)data.m_width * data.m_scale * -0.5f, 0f, (float)data.m_width * data.m_scale * -0.5f);
		WorldGenerator worldGen = data.m_worldGen;
		data.m_cornerBiomes = new Heightmap.Biome[4];
		data.m_cornerBiomes[0] = worldGen.GetBiome(val.x, val.z);
		data.m_cornerBiomes[1] = worldGen.GetBiome((float)((double)val.x + (double)data.m_width * (double)data.m_scale), val.z);
		data.m_cornerBiomes[2] = worldGen.GetBiome(val.x, (float)((double)val.z + (double)data.m_width * (double)data.m_scale));
		data.m_cornerBiomes[3] = worldGen.GetBiome((float)((double)val.x + (double)data.m_width * (double)data.m_scale), (float)((double)val.z + (double)data.m_width * (double)data.m_scale));
		Heightmap.Biome biome = data.m_cornerBiomes[0];
		Heightmap.Biome biome2 = data.m_cornerBiomes[1];
		Heightmap.Biome biome3 = data.m_cornerBiomes[2];
		Heightmap.Biome biome4 = data.m_cornerBiomes[3];
		data.m_baseHeights = new List<float>(num * num);
		for (int i = 0; i < num2; i++)
		{
			data.m_baseHeights.Add(0f);
		}
		int num3 = num * num;
		data.m_baseMask = (Color[])(object)new Color[num3];
		for (int j = 0; j < num3; j++)
		{
			data.m_baseMask[j] = new Color(0f, 0f, 0f, 0f);
		}
		for (int k = 0; k < num; k++)
		{
			float wy = (float)((double)val.z + (double)k * (double)data.m_scale);
			float num4 = DUtils.SmoothStep(0f, 1f, (float)((double)k / (double)data.m_width));
			for (int l = 0; l < num; l++)
			{
				float wx = (float)((double)val.x + (double)l * (double)data.m_scale);
				float num5 = DUtils.SmoothStep(0f, 1f, (float)((double)l / (double)data.m_width));
				float num6 = 0f;
				Color mask = Color.black;
				if (data.m_distantLod)
				{
					Heightmap.Biome biome5 = worldGen.GetBiome(wx, wy);
					num6 = worldGen.GetBiomeHeight(biome5, wx, wy, out mask);
				}
				else if (biome3 == biome && biome2 == biome && biome4 == biome)
				{
					num6 = worldGen.GetBiomeHeight(biome, wx, wy, out mask);
				}
				else
				{
					Color[] array = (Color[])(object)new Color[4];
					float biomeHeight = worldGen.GetBiomeHeight(biome, wx, wy, out array[0]);
					float biomeHeight2 = worldGen.GetBiomeHeight(biome2, wx, wy, out array[1]);
					float biomeHeight3 = worldGen.GetBiomeHeight(biome3, wx, wy, out array[2]);
					float biomeHeight4 = worldGen.GetBiomeHeight(biome4, wx, wy, out array[3]);
					float num7 = DUtils.Lerp(biomeHeight, biomeHeight2, num5);
					float num8 = DUtils.Lerp(biomeHeight3, biomeHeight4, num5);
					num6 = DUtils.Lerp(num7, num8, num4);
					Color val2 = Color.Lerp(array[0], array[1], num5);
					Color val3 = Color.Lerp(array[2], array[3], num5);
					mask = Color.Lerp(val2, val3, num4);
				}
				data.m_baseHeights[k * num + l] = num6;
				data.m_baseMask[k * num + l] = mask;
			}
		}
		if (!data.m_distantLod)
		{
			return;
		}
		for (int m = 0; m < 4; m++)
		{
			List<float> list = new List<float>(data.m_baseHeights);
			for (int n = 1; n < num - 1; n++)
			{
				for (int num9 = 1; num9 < num - 1; num9++)
				{
					float num10 = list[n * num + num9];
					float num11 = list[(n - 1) * num + num9];
					float num12 = list[(n + 1) * num + num9];
					float num13 = list[n * num + num9 - 1];
					float num14 = list[n * num + num9 + 1];
					if (Mathf.Abs(num10 - num11) > 10f)
					{
						num10 = (num10 + num11) * 0.5f;
					}
					if (Mathf.Abs(num10 - num12) > 10f)
					{
						num10 = (num10 + num12) * 0.5f;
					}
					if (Mathf.Abs(num10 - num13) > 10f)
					{
						num10 = (num10 + num13) * 0.5f;
					}
					if (Mathf.Abs(num10 - num14) > 10f)
					{
						num10 = (num10 + num14) * 0.5f;
					}
					data.m_baseHeights[n * num + num9] = num10;
				}
			}
		}
	}

	public HMBuildData RequestTerrainSync(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		HMBuildData hMBuildData;
		do
		{
			hMBuildData = RequestTerrain(center, width, scale, distantLod, worldGen);
		}
		while (hMBuildData == null);
		return hMBuildData;
	}

	private HMBuildData RequestTerrain(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		m_lock.WaitOne();
		for (int i = 0; i < m_ready.Count; i++)
		{
			HMBuildData hMBuildData = m_ready[i];
			if (hMBuildData.IsEqual(center, width, scale, distantLod, worldGen))
			{
				m_ready.RemoveAt(i);
				m_lock.ReleaseMutex();
				return hMBuildData;
			}
		}
		for (int j = 0; j < m_toBuild.Count; j++)
		{
			if (m_toBuild[j].IsEqual(center, width, scale, distantLod, worldGen))
			{
				m_lock.ReleaseMutex();
				return null;
			}
		}
		m_toBuild.Add(new HMBuildData(center, width, scale, distantLod, worldGen));
		m_lock.ReleaseMutex();
		return null;
	}

	public bool IsTerrainReady(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		m_lock.WaitOne();
		for (int i = 0; i < m_ready.Count; i++)
		{
			if (m_ready[i].IsEqual(center, width, scale, distantLod, worldGen))
			{
				m_lock.ReleaseMutex();
				return true;
			}
		}
		for (int j = 0; j < m_toBuild.Count; j++)
		{
			if (m_toBuild[j].IsEqual(center, width, scale, distantLod, worldGen))
			{
				m_lock.ReleaseMutex();
				return false;
			}
		}
		m_toBuild.Add(new HMBuildData(center, width, scale, distantLod, worldGen));
		m_lock.ReleaseMutex();
		return false;
	}
}
