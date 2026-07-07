using UnityEngine;

public class Cover
{
	private const float m_coverRayDistance = 30f;

	private static int m_coverRayMask = 0;

	private static Vector3[] m_coverRays = null;

	private static RaycastHit[] m_raycastHits = (RaycastHit[])(object)new RaycastHit[128];

	public static void GetCoverForPoint(Vector3 startPos, out float coverPercentage, out bool underRoof, float minDistance = 0.5f)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Setup();
		float num = 0f;
		underRoof = IsUnderRoof(startPos);
		Vector3[] coverRays = m_coverRays;
		RaycastHit val2 = default(RaycastHit);
		foreach (Vector3 val in coverRays)
		{
			if (Physics.Raycast(startPos + val * minDistance, val, ref val2, 30f - minDistance, m_coverRayMask))
			{
				num += 1f;
			}
		}
		coverPercentage = num / (float)m_coverRays.Length;
	}

	public static bool IsUnderRoof(Vector3 startPos)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Setup();
		int num = Physics.SphereCastNonAlloc(startPos, 0.1f, Vector3.up, m_raycastHits, 100f, m_coverRayMask);
		for (int i = 0; i < num; i++)
		{
			RaycastHit val = m_raycastHits[i];
			if (!((Component)((RaycastHit)(ref val)).collider).gameObject.CompareTag("leaky"))
			{
				return true;
			}
		}
		return false;
	}

	private static void Setup()
	{
		if (m_coverRays == null)
		{
			m_coverRayMask = LayerMask.GetMask(new string[6] { "Default", "static_solid", "Default_small", "piece", "terrain", "vehicle" });
			CreateCoverRays();
		}
	}

	private static void CreateCoverRays()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		m_coverRays = (Vector3[])(object)new Vector3[17]
		{
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0f, 0f, -1f),
			new Vector3(-1f, 0f, -1f),
			new Vector3(1f, 0f, -1f),
			new Vector3(-1f, 0f, 1f),
			new Vector3(1f, 0f, 1f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0f, 1f, -1f),
			new Vector3(-1f, 1f, -1f),
			new Vector3(1f, 1f, -1f),
			new Vector3(-1f, 1f, 1f),
			new Vector3(1f, 1f, 1f)
		};
		Vector3[] coverRays = m_coverRays;
		for (int i = 0; i < coverRays.Length; i++)
		{
			Vector3 val = coverRays[i];
			((Vector3)(ref val)).Normalize();
		}
	}
}
