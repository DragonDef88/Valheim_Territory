using System.Collections.Generic;
using UnityEngine;

public class StaticTarget : MonoBehaviour
{
	[Header("Static target")]
	public bool m_primaryTarget;

	public bool m_randomTarget = true;

	private List<Collider> m_colliders;

	private Vector3 m_localCenter;

	private bool m_haveCenter;

	public virtual bool IsPriorityTarget()
	{
		return m_primaryTarget;
	}

	public virtual bool IsRandomTarget()
	{
		return m_randomTarget;
	}

	public Vector3 GetCenter()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (!m_haveCenter)
		{
			List<Collider> allColliders = GetAllColliders();
			m_localCenter = Vector3.zero;
			foreach (Collider item in allColliders)
			{
				if (Object.op_Implicit((Object)(object)item))
				{
					Vector3 localCenter = m_localCenter;
					Bounds bounds = item.bounds;
					m_localCenter = localCenter + ((Bounds)(ref bounds)).center;
				}
			}
			m_localCenter /= (float)m_colliders.Count;
			m_localCenter = ((Component)this).transform.InverseTransformPoint(m_localCenter);
			m_haveCenter = true;
		}
		return ((Component)this).transform.TransformPoint(m_localCenter);
	}

	public List<Collider> GetAllColliders()
	{
		if (m_colliders == null)
		{
			Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>();
			m_colliders = new List<Collider>();
			m_colliders.Capacity = componentsInChildren.Length;
			Collider[] array = componentsInChildren;
			foreach (Collider val in array)
			{
				if (val.enabled && ((Component)val).gameObject.activeInHierarchy && !val.isTrigger)
				{
					m_colliders.Add(val);
				}
			}
		}
		return m_colliders;
	}

	public Vector3 FindClosestPoint(Vector3 point)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> allColliders = GetAllColliders();
		if (allColliders.Count == 0)
		{
			return ((Component)this).transform.position;
		}
		float num = 9999999f;
		Vector3 result = Vector3.zero;
		foreach (Collider item in allColliders)
		{
			if (Object.op_Implicit((Object)(object)item))
			{
				MeshCollider val = (MeshCollider)(object)((item is MeshCollider) ? item : null);
				Vector3 val2 = ((Object.op_Implicit((Object)(object)val) && !val.convex) ? item.ClosestPointOnBounds(point) : item.ClosestPoint(point));
				float num2 = Vector3.Distance(point, val2);
				if (num2 < num)
				{
					result = val2;
					num = num2;
				}
			}
		}
		return result;
	}
}
