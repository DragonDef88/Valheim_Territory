using System;
using System.Collections.Generic;
using UnityEngine;

public class CircleProjector : MonoBehaviour
{
	public float m_radius = 5f;

	public int m_nrOfSegments = 20;

	public float m_speed = 0.1f;

	public float m_turns = 1f;

	public float m_start;

	public bool m_sliceLines;

	private float m_calcStart;

	private float m_calcTurns;

	public GameObject m_prefab;

	public LayerMask m_mask;

	private List<GameObject> m_segments = new List<GameObject>();

	private void Start()
	{
		CreateSegments();
	}

	private void Update()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		CreateSegments();
		bool flag = m_turns == 1f;
		float num = (float)Math.PI * 2f * m_turns / (float)(m_nrOfSegments - ((!flag) ? 1 : 0));
		float num2 = ((flag && !m_sliceLines) ? (Time.time * m_speed) : 0f);
		RaycastHit val2 = default(RaycastHit);
		for (int i = 0; i < m_nrOfSegments; i++)
		{
			float num3 = (float)Math.PI / 180f * m_start + (float)i * num + num2;
			Vector3 val = ((Component)this).transform.position + new Vector3(Mathf.Sin(num3) * m_radius, 0f, Mathf.Cos(num3) * m_radius);
			GameObject obj = m_segments[i];
			if (Physics.Raycast(val + Vector3.up * 500f, Vector3.down, ref val2, 1000f, ((LayerMask)(ref m_mask)).value))
			{
				val.y = ((RaycastHit)(ref val2)).point.y;
			}
			obj.transform.position = val;
		}
		for (int j = 0; j < m_nrOfSegments; j++)
		{
			GameObject val3 = m_segments[j];
			GameObject val4;
			GameObject val5;
			if (flag)
			{
				val4 = ((j == 0) ? m_segments[m_nrOfSegments - 1] : m_segments[j - 1]);
				val5 = ((j == m_nrOfSegments - 1) ? m_segments[0] : m_segments[j + 1]);
			}
			else
			{
				val4 = ((j == 0) ? val3 : m_segments[j - 1]);
				val5 = ((j == m_nrOfSegments - 1) ? val3 : m_segments[j + 1]);
			}
			Vector3 val6 = val5.transform.position - val4.transform.position;
			Vector3 normalized = ((Vector3)(ref val6)).normalized;
			val3.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		}
		RaycastHit val7 = default(RaycastHit);
		for (int k = m_nrOfSegments; k < m_segments.Count; k++)
		{
			Vector3 position = m_segments[k].transform.position;
			if (Physics.Raycast(position + Vector3.up * 500f, Vector3.down, ref val7, 1000f, ((LayerMask)(ref m_mask)).value))
			{
				position.y = ((RaycastHit)(ref val7)).point.y;
			}
			m_segments[k].transform.position = position;
		}
	}

	private void CreateSegments()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if ((!m_sliceLines && m_segments.Count == m_nrOfSegments) || (m_sliceLines && m_calcStart == m_start && m_calcTurns == m_turns))
		{
			return;
		}
		foreach (GameObject segment in m_segments)
		{
			Object.Destroy((Object)(object)segment);
		}
		m_segments.Clear();
		for (int i = 0; i < m_nrOfSegments; i++)
		{
			GameObject item = Object.Instantiate<GameObject>(m_prefab, ((Component)this).transform.position, Quaternion.identity, ((Component)this).transform);
			m_segments.Add(item);
		}
		m_calcStart = m_start;
		m_calcTurns = m_turns;
		if (m_sliceLines)
		{
			float start = m_start;
			float angle2 = m_start + (float)Math.PI * 2f * m_turns * 57.29578f;
			float num = 2f * m_radius * (float)Math.PI * m_turns / (float)m_nrOfSegments;
			int count2 = (int)(m_radius / num) - 2;
			placeSlices(start, count2);
			placeSlices(angle2, count2);
		}
		void placeSlices(float angle, int count)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			for (int j = 0; j < count; j++)
			{
				GameObject val = Object.Instantiate<GameObject>(m_prefab, ((Component)this).transform.position, Quaternion.Euler(0f, angle, 0f), ((Component)this).transform);
				Transform transform = val.transform;
				transform.position += val.transform.forward * m_radius * ((float)(j + 1) / (float)(count + 1));
				m_segments.Add(val);
			}
		}
	}
}
