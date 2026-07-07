using System.Collections.Generic;
using UnityEngine;

public class TerrainLod : MonoBehaviour
{
	private enum HeightmapState
	{
		NeedsRebuild,
		ReadyToRebuild,
		Done
	}

	private class HeightmapWithOffset
	{
		public Heightmap m_heightmap;

		public Vector3 m_offset;

		public HeightmapState m_state;

		public HeightmapWithOffset(Heightmap heightmap, Vector3 offset)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			m_heightmap = heightmap;
			m_offset = offset;
			m_state = HeightmapState.NeedsRebuild;
		}
	}

	[SerializeField]
	private float m_updateStepDistance = 256f;

	[SerializeField]
	private float m_terrainSize = 2400f;

	[SerializeField]
	private int m_regionsPerAxis = 3;

	[SerializeField]
	private float m_vertexDistance = 10f;

	[SerializeField]
	private Material m_material;

	private List<HeightmapWithOffset> m_heightmaps = new List<HeightmapWithOffset>();

	private Vector3 m_lastPoint = new Vector3(99999f, 0f, 99999f);

	private HeightmapState m_heightmapState = HeightmapState.Done;

	private void OnEnable()
	{
		CreateMeshes();
	}

	private void OnDisable()
	{
		ResetMeshes();
	}

	private void CreateMeshes()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		float num = m_terrainSize / (float)m_regionsPerAxis;
		float num2 = Mathf.Round(m_vertexDistance);
		int width = Mathf.RoundToInt(num / num2);
		Vector3 offset = default(Vector3);
		for (int i = 0; i < m_regionsPerAxis; i++)
		{
			for (int j = 0; j < m_regionsPerAxis; j++)
			{
				((Vector3)(ref offset))._002Ector(((float)i * 2f - (float)m_regionsPerAxis + 1f) * m_terrainSize * 0.5f / (float)m_regionsPerAxis, 0f, ((float)j * 2f - (float)m_regionsPerAxis + 1f) * m_terrainSize * 0.5f / (float)m_regionsPerAxis);
				CreateMesh(num2, width, offset);
			}
		}
	}

	private void CreateMesh(float scale, int width, Vector3 offset)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("lodMesh");
		val.transform.position = offset;
		val.transform.SetParent(((Component)this).transform);
		Heightmap heightmap = val.AddComponent<Heightmap>();
		m_heightmaps.Add(new HeightmapWithOffset(heightmap, offset));
		heightmap.m_scale = scale;
		heightmap.m_width = width;
		heightmap.m_material = m_material;
		heightmap.IsDistantLod = true;
		((Behaviour)heightmap).enabled = true;
	}

	private void ResetMeshes()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_heightmaps.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)m_heightmaps[i].m_heightmap).gameObject);
		}
		m_heightmaps.Clear();
		m_lastPoint = new Vector3(99999f, 0f, 99999f);
		m_heightmapState = HeightmapState.Done;
	}

	private void Update()
	{
		UpdateHeightmaps();
	}

	private void UpdateHeightmaps()
	{
		if (ZNet.GetConnectionStatus() == ZNet.ConnectionStatus.Connected && NeedsRebuild() && IsAllTerrainReady())
		{
			RebuildAllHeightmaps();
		}
	}

	private void RebuildAllHeightmaps()
	{
		for (int i = 0; i < m_heightmaps.Count; i++)
		{
			RebuildHeightmap(m_heightmaps[i]);
		}
		m_heightmapState = HeightmapState.Done;
	}

	private bool IsAllTerrainReady()
	{
		int num = 0;
		for (int i = 0; i < m_heightmaps.Count; i++)
		{
			if (IsTerrainReady(m_heightmaps[i]))
			{
				num++;
			}
		}
		return num == m_heightmaps.Count;
	}

	private bool IsTerrainReady(HeightmapWithOffset heightmapWithOffset)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Heightmap heightmap = heightmapWithOffset.m_heightmap;
		Vector3 offset = heightmapWithOffset.m_offset;
		if (heightmapWithOffset.m_state == HeightmapState.ReadyToRebuild)
		{
			return true;
		}
		if (HeightmapBuilder.instance.IsTerrainReady(m_lastPoint + offset, heightmap.m_width, heightmap.m_scale, heightmap.IsDistantLod, WorldGenerator.instance))
		{
			heightmapWithOffset.m_state = HeightmapState.ReadyToRebuild;
			return true;
		}
		return false;
	}

	private void RebuildHeightmap(HeightmapWithOffset heightmapWithOffset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Heightmap heightmap = heightmapWithOffset.m_heightmap;
		Vector3 offset = heightmapWithOffset.m_offset;
		((Component)heightmap).transform.position = m_lastPoint + offset;
		heightmap.Regenerate();
		heightmapWithOffset.m_state = HeightmapState.Done;
	}

	private bool NeedsRebuild()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (m_heightmapState == HeightmapState.NeedsRebuild)
		{
			return true;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if ((Object)(object)mainCamera == (Object)null)
		{
			return false;
		}
		Vector3 position = ((Component)mainCamera).transform.position;
		if (Utils.DistanceXZ(position, m_lastPoint) > m_updateStepDistance && m_heightmapState == HeightmapState.Done)
		{
			for (int i = 0; i < m_heightmaps.Count; i++)
			{
				m_heightmaps[i].m_state = HeightmapState.NeedsRebuild;
			}
			m_lastPoint = new Vector3(Mathf.Round(position.x / m_vertexDistance) * m_vertexDistance, 0f, Mathf.Round(position.z / m_vertexDistance) * m_vertexDistance);
			m_heightmapState = HeightmapState.NeedsRebuild;
			return true;
		}
		return false;
	}
}
