using System;
using System.Collections.Generic;
using UnityEngine;

public class SmokeRenderer : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _particleSystemPrefab;

	[SerializeField]
	private Color m_smokeColor;

	[SerializeField]
	private float m_smokeBallSize = 4f;

	[Header("Chunking")]
	[SerializeField]
	private float m_chunkSize = 10f;

	private Dictionary<Vector3Int, ParticleSystem> m_chunkedParticleSystems = new Dictionary<Vector3Int, ParticleSystem>();

	private Dictionary<Vector3Int, List<Smoke>> m_chunkedSmoke = new Dictionary<Vector3Int, List<Smoke>>();

	private Dictionary<Vector3Int, Particle[]> m_chunkedParticles = new Dictionary<Vector3Int, Particle[]>();

	private List<Tuple<Vector3Int, Vector3Int, Smoke>> m_chunkedSmokeToMove = new List<Tuple<Vector3Int, Vector3Int, Smoke>>();

	public static SmokeRenderer Instance;

	private const int c_MaxChunkParticleCount = 100;

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public void RegisterSmoke(Smoke smoke)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		AddSmokeToChunk(PositionToChunk(((Component)smoke).transform.position), smoke);
	}

	public void UnregisterSmoke(Smoke smoke)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		RemoveSmokeFromChunk(smoke.RenderChunk, smoke);
	}

	private Vector3Int PositionToChunk(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(pos.x / m_chunkSize);
		int num2 = Mathf.FloorToInt(pos.y / m_chunkSize);
		int num3 = Mathf.FloorToInt(pos.z / m_chunkSize);
		return new Vector3Int(num, num2, num3);
	}

	private Vector3 ChunkToWorld(Vector3Int chunk)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)((Vector3Int)(ref chunk)).x * m_chunkSize, (float)((Vector3Int)(ref chunk)).y * m_chunkSize, (float)((Vector3Int)(ref chunk)).z * m_chunkSize);
	}

	private void AddSmokeToChunk(Vector3Int chunk, Smoke smoke)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_chunkedSmoke.ContainsKey(chunk))
		{
			m_chunkedSmoke.Add(chunk, new List<Smoke>());
			m_chunkedParticleSystems.Add(chunk, Object.Instantiate<ParticleSystem>(_particleSystemPrefab, ChunkToWorld(chunk), Quaternion.identity));
			m_chunkedParticles.Add(chunk, (Particle[])(object)new Particle[100]);
		}
		if (!m_chunkedSmoke[chunk].Contains(smoke))
		{
			m_chunkedSmoke[chunk].Add(smoke);
		}
		smoke.RenderChunk = chunk;
	}

	private void RemoveSmokeFromChunk(Vector3Int chunk, Smoke smoke)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (m_chunkedSmoke.ContainsKey(chunk))
		{
			m_chunkedSmoke[chunk].Remove(smoke);
			if (m_chunkedSmoke[chunk].Count == 0)
			{
				CleanupChunk(chunk);
			}
		}
	}

	private void CleanupChunk(Vector3Int chunk)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_chunkedParticles.Remove(chunk);
		m_chunkedSmoke.Remove(chunk);
		m_chunkedParticleSystems.Remove(chunk, out var value);
		if ((Object)(object)value != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)value).gameObject);
		}
	}

	private void TransferSmokeBetweenChunks()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		m_chunkedSmokeToMove.Clear();
		foreach (Vector3Int key in m_chunkedSmoke.Keys)
		{
			foreach (Smoke item in m_chunkedSmoke[key])
			{
				Vector3Int val = PositionToChunk(((Component)item).transform.position);
				if (val != key)
				{
					m_chunkedSmokeToMove.Add(new Tuple<Vector3Int, Vector3Int, Smoke>(key, val, item));
				}
			}
		}
		foreach (Tuple<Vector3Int, Vector3Int, Smoke> item2 in m_chunkedSmokeToMove)
		{
			RemoveSmokeFromChunk(item2.Item1, item2.Item3);
			AddSmokeToChunk(item2.Item2, item2.Item3);
		}
	}

	private void LateUpdate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		TransferSmokeBetweenChunks();
		foreach (Vector3Int key in m_chunkedParticleSystems.Keys)
		{
			ParticleSystem val = m_chunkedParticleSystems[key];
			List<Smoke> list = m_chunkedSmoke[key];
			Particle[] array = m_chunkedParticles[key];
			if (list.Count > val.particleCount)
			{
				val.Emit(list.Count - val.particleCount);
			}
			for (int i = 0; i < list.Count; i++)
			{
				Smoke smoke = list[i];
				array[i] = smoke.GetParticleValues();
				((Particle)(ref array[i])).startColor = Color32.op_Implicit(m_smokeColor * new Color(1f, 1f, 1f, smoke.GetAlpha()));
				((Particle)(ref array[i])).startSize = m_smokeBallSize;
			}
			for (int j = list.Count; j < val.particleCount; j++)
			{
				((Particle)(ref array[j])).remainingLifetime = -1f;
			}
			val.SetParticles(array, val.particleCount);
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		foreach (Vector3Int key in m_chunkedSmoke.Keys)
		{
			Vector3 val = ChunkToWorld(key);
			Color val2 = new Color(0.43f, 1f, 0f, 0.26f);
			Color red = Color.red;
			Gizmos.color = Color.Lerp(val2, red, (float)m_chunkedSmoke[key].Count / 100f * 0.33f);
			Gizmos.DrawWireCube(val + Vector3.one * m_chunkSize * 0.5f, m_chunkSize * Vector3.one);
		}
	}
}
