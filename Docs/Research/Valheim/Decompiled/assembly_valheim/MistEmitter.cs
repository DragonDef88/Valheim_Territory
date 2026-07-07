using System;
using UnityEngine;

public class MistEmitter : MonoBehaviour
{
	public float m_interval = 1f;

	public float m_totalRadius = 30f;

	public float m_testRadius = 5f;

	public int m_rays = 10;

	public float m_placeOffset = 1f;

	public ParticleSystem m_psystem;

	private float m_placeTimer;

	private bool m_emit = true;

	public void SetEmit(bool emit)
	{
		m_emit = emit;
	}

	private void Update()
	{
		if (m_emit)
		{
			m_placeTimer += Time.deltaTime;
			if (m_placeTimer > m_interval)
			{
				m_placeTimer = 0f;
				PlaceOne();
			}
		}
	}

	private void PlaceOne()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!GetRandomPoint(((Component)this).transform.position, m_totalRadius, out var p))
		{
			return;
		}
		int num = 0;
		float num2 = (float)Math.PI * 2f / (float)m_rays;
		for (int i = 0; i < m_rays; i++)
		{
			float angle = (float)i * num2;
			if ((double)GetPointOnEdge(p, angle, m_testRadius).y < (double)p.y - 0.1)
			{
				num++;
			}
		}
		if (num <= m_rays / 4 && !Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(p, EffectArea.Type.Fire, m_testRadius)))
		{
			EmitParams val = default(EmitParams);
			((EmitParams)(ref val)).position = p + Vector3.up * m_placeOffset;
			m_psystem.Emit(val, 1);
		}
	}

	private static bool GetRandomPoint(Vector3 center, float radius, out Vector3 p)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.value * (float)Math.PI * 2f;
		float num2 = Random.Range(0f, radius);
		p = center + new Vector3(Mathf.Sin(num) * num2, 0f, Mathf.Cos(num) * num2);
		if (ZoneSystem.instance.GetGroundHeight(p, out var height))
		{
			if (height < 30f)
			{
				return false;
			}
			float liquidLevel = Floating.GetLiquidLevel(p);
			if (height < liquidLevel)
			{
				return false;
			}
			p.y = height;
			return true;
		}
		return false;
	}

	private static Vector3 GetPointOnEdge(Vector3 center, float angle, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
		val.y = ZoneSystem.instance.GetGroundHeight(val);
		if (val.y < 30f)
		{
			val.y = 30f;
		}
		return val;
	}
}
