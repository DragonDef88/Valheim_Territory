using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMist : MonoBehaviour
{
	private List<Heightmap> tempHeightmaps = new List<Heightmap>();

	private List<Demister> fields = new List<Demister>();

	private List<KeyValuePair<Demister, float>> sortList = new List<KeyValuePair<Demister, float>>();

	private static ParticleMist m_instance;

	private ParticleSystem m_ps;

	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome = Heightmap.Biome.Mistlands;

	public float m_localRange = 10f;

	public int m_localEmission = 50;

	public int m_localEmissionPerUnit = 50;

	public float m_maxMistAltitude = 50f;

	[Header("Misters")]
	public float m_distantMaxRange = 100f;

	public float m_distantMinSize = 5f;

	public float m_distantMaxSize = 20f;

	public float m_distantEmissionMax = 0.1f;

	public float m_distantEmissionMaxVel = 1f;

	public float m_distantThickness = 4f;

	[Header("Demisters")]
	public float m_minDistance = 10f;

	public float m_maxDistance = 50f;

	public float m_emissionMax = 0.2f;

	public float m_emissionPerUnit = 20f;

	public float m_minSize = 2f;

	public float m_maxSize = 10f;

	private float m_inMistAreaTimer;

	private float m_accumulator;

	private float m_combinedMovement;

	private Vector3 m_lastUpdatePos;

	private bool m_haveActiveMist;

	public static ParticleMist instance => m_instance;

	private void Awake()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		m_instance = this;
		m_ps = ((Component)this).GetComponent<ParticleSystem>();
		m_lastUpdatePos = ((Component)this).transform.position;
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_instance == (Object)(object)this)
		{
			m_instance = null;
		}
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		EmissionModule emission = m_ps.emission;
		if (!((EmissionModule)(ref emission)).enabled)
		{
			return;
		}
		m_accumulator += Time.fixedDeltaTime;
		if (m_accumulator < 0.1f)
		{
			return;
		}
		m_accumulator -= 0.1f;
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return;
		}
		List<Mister> demistersSorted = Mister.GetDemistersSorted(((Component)localPlayer).transform.position);
		if (demistersSorted.Count == 0)
		{
			return;
		}
		m_haveActiveMist = demistersSorted.Count > 0;
		GetAllForcefields(fields);
		m_inMistAreaTimer += 0.1f;
		float num = Vector3.Distance(((Component)this).transform.position, m_lastUpdatePos);
		m_combinedMovement += Mathf.Clamp(num, 0f, 10f);
		m_lastUpdatePos = ((Component)this).transform.position;
		FindMaxMistAlltitude(50f, out var minMistHeight, out var _);
		int num2 = (int)(m_combinedMovement * (float)m_localEmissionPerUnit);
		if (num2 > 0)
		{
			m_combinedMovement = Mathf.Max(0f, m_combinedMovement - (float)num2 / (float)m_localEmissionPerUnit);
		}
		int toEmit = (int)((float)m_localEmission * 0.1f) + num2;
		Emit(((Component)this).transform.position, 0f, m_localRange, toEmit, fields, null, minMistHeight);
		foreach (Demister field in fields)
		{
			float endRange = field.m_forceField.endRange;
			float num3 = Mathf.Max(0f, Vector3.Distance(((Component)field).transform.position, ((Component)this).transform.position) - endRange);
			if (!(num3 > m_maxDistance))
			{
				float num4 = (float)Math.PI * 4f * (endRange * endRange);
				float num5 = Mathf.Lerp(m_emissionMax, 0f, Utils.LerpStep(m_minDistance, m_maxDistance, num3));
				int num6 = (int)(num4 * num5 * 0.1f);
				float movedDistance = field.GetMovedDistance();
				num6 += (int)(movedDistance * m_emissionPerUnit);
				Emit(((Component)field).transform.position, endRange, 0f, num6, fields, field, minMistHeight);
			}
		}
		foreach (Mister item in demistersSorted)
		{
			if (!item.Inside(((Component)this).transform.position, 0f))
			{
				MisterEmit(item, demistersSorted, fields, minMistHeight, 0.1f);
			}
		}
	}

	private void Emit(Vector3 center, float radius, float thickness, int toEmit, List<Demister> fields, Demister pf, float minAlt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (!Mister.InsideMister(center, radius + thickness) || IsInsideOtherDemister(fields, center, radius + thickness, pf))
		{
			return;
		}
		EmitParams val = default(EmitParams);
		for (int i = 0; i < toEmit; i++)
		{
			Vector3 onUnitSphere = Random.onUnitSphere;
			Vector3 val2 = center + onUnitSphere * (radius + 0.1f + Random.Range(0f, thickness));
			if (!(val2.y < minAlt) && !IsInsideOtherDemister(fields, val2, 0f, pf) && Mister.InsideMister(val2))
			{
				float num = Vector3.Distance(((Component)this).transform.position, val2);
				if (!(num > m_maxDistance))
				{
					((EmitParams)(ref val)).startSize = Mathf.Lerp(m_minSize, m_maxSize, Utils.LerpStep(m_minDistance, m_maxDistance, num));
					((EmitParams)(ref val)).position = val2;
					m_ps.Emit(val, 1);
				}
			}
		}
	}

	private void MisterEmit(Mister mister, List<Mister> allMisters, List<Demister> fields, float minAlt, float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)mister).transform.position;
		float radius = mister.m_radius;
		float num = Mathf.Max(0f, Vector3.Distance(((Component)mister).transform.position, ((Component)this).transform.position) - radius);
		if (num > m_distantMaxRange || mister.IsCompletelyInsideOtherMister(m_distantThickness))
		{
			return;
		}
		float num2 = (float)Math.PI * 4f * (radius * radius);
		float num3 = Mathf.Lerp(m_distantEmissionMax, 0f, Utils.LerpStep(0f, m_distantMaxRange, num));
		int num4 = (int)(num2 * num3 * dt);
		float num5 = ((Component)mister).transform.position.y + mister.m_height;
		EmitParams val = default(EmitParams);
		for (int i = 0; i < num4; i++)
		{
			Vector3 onUnitSphere = Random.onUnitSphere;
			Vector3 val2 = position + onUnitSphere * (radius + 0.1f + Random.Range(0f, m_distantThickness));
			if (val2.y < minAlt)
			{
				continue;
			}
			if (val2.y > num5)
			{
				val2.y = num5;
			}
			if (!Mister.IsInsideOtherMister(val2, mister) && !IsInsideOtherDemister(fields, val2, 0f, null))
			{
				float num6 = Vector3.Distance(((Component)this).transform.position, val2);
				if (!(num6 > m_distantMaxRange))
				{
					((EmitParams)(ref val)).startSize = Mathf.Lerp(m_distantMinSize, m_distantMaxSize, Utils.LerpStep(0f, m_distantMaxRange, num6));
					((EmitParams)(ref val)).position = val2;
					Vector3 velocity = onUnitSphere * Random.Range(0f, m_distantEmissionMaxVel);
					velocity.y = 0f;
					((EmitParams)(ref val)).velocity = velocity;
					m_ps.Emit(val, 1);
				}
			}
		}
	}

	private bool IsInsideOtherDemister(List<Demister> fields, Vector3 p, float radius, Demister ignore)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		foreach (Demister field in fields)
		{
			if (!((Object)(object)field == (Object)(object)ignore) && Vector3.Distance(((Component)field).transform.position, p) + radius < field.m_forceField.endRange)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsInMist(Vector3 p0)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_instance == (Object)null)
		{
			return false;
		}
		if (!m_instance.m_haveActiveMist)
		{
			return false;
		}
		if (Mister.InsideMister(p0))
		{
			return !m_instance.InsideDemister(p0);
		}
		return false;
	}

	public static bool IsMistBlocked(Vector3 p0, Vector3 p1)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_instance == (Object)null)
		{
			return false;
		}
		return m_instance.IsMistBlocked_internal(p0, p1);
	}

	private bool IsMistBlocked_internal(Vector3 p0, Vector3 p1)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!m_haveActiveMist)
		{
			return false;
		}
		if (Vector3.Distance(p0, p1) < 10f)
		{
			return false;
		}
		Vector3 p2 = (p0 + p1) * 0.5f;
		if (Mister.InsideMister(p0) && !InsideDemister(p0))
		{
			return true;
		}
		if (Mister.InsideMister(p1) && !InsideDemister(p1))
		{
			return true;
		}
		if (Mister.InsideMister(p2) && !InsideDemister(p2))
		{
			return true;
		}
		return false;
	}

	private bool InsideDemister(Vector3 p)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Demister demister in Demister.GetDemisters())
		{
			if (Vector3.Distance(((Component)demister).transform.position, p) < demister.m_forceField.endRange)
			{
				return true;
			}
		}
		return false;
	}

	private void GetAllForcefields(List<Demister> fields)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		List<Demister> demisters = Demister.GetDemisters();
		sortList.Clear();
		foreach (Demister item in demisters)
		{
			sortList.Add(new KeyValuePair<Demister, float>(item, Vector3.Distance(((Component)this).transform.position, ((Component)item).transform.position)));
		}
		sortList.Sort((KeyValuePair<Demister, float> a, KeyValuePair<Demister, float> b) => a.Value.CompareTo(b.Value));
		fields.Clear();
		foreach (KeyValuePair<Demister, float> sort in sortList)
		{
			fields.Add(sort.Key);
		}
	}

	private void FindMaxMistAlltitude(float testRange, out float minMistHeight, out float maxMistHeight)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		float num = 0f;
		int num2 = 20;
		minMistHeight = 99999f;
		for (int i = 0; i < num2; i++)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector3 p = position + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * testRange;
			float groundHeight = ZoneSystem.instance.GetGroundHeight(p);
			num += groundHeight;
			if (groundHeight < minMistHeight)
			{
				minMistHeight = groundHeight;
			}
		}
		float num3 = num / (float)num2;
		maxMistHeight = num3 + m_maxMistAltitude;
	}
}
