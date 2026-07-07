using System.Collections.Generic;
using UnityEngine;

public class Mister : MonoBehaviour
{
	public float m_radius = 50f;

	public float m_height = 10f;

	private float m_tempDistance;

	private static List<Mister> m_instances = new List<Mister>();

	private void Awake()
	{
	}

	private void OnEnable()
	{
		m_instances.Add(this);
	}

	private void OnDisable()
	{
		m_instances.Remove(this);
	}

	public static List<Mister> GetMisters()
	{
		return m_instances;
	}

	public static List<Mister> GetDemistersSorted(Vector3 refPoint)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (Mister instance in m_instances)
		{
			instance.m_tempDistance = Vector3.Distance(((Component)instance).transform.position, refPoint);
		}
		m_instances.Sort((Mister a, Mister b) => a.m_tempDistance.CompareTo(b.m_tempDistance));
		return m_instances;
	}

	public static Mister FindMister(Vector3 p)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Mister instance in m_instances)
		{
			if (Vector3.Distance(((Component)instance).transform.position, p) < instance.m_radius)
			{
				return instance;
			}
		}
		return null;
	}

	public static bool InsideMister(Vector3 p, float radius = 0f)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Mister instance in m_instances)
		{
			if (Vector3.Distance(((Component)instance).transform.position, p) < instance.m_radius + radius && p.y - radius < ((Component)instance).transform.position.y + instance.m_height)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCompletelyInsideOtherMister(float thickness)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		foreach (Mister instance in m_instances)
		{
			if (!((Object)(object)instance == (Object)(object)this) && Vector3.Distance(position, ((Component)instance).transform.position) + m_radius + thickness < instance.m_radius && position.y + m_height < ((Component)instance).transform.position.y + instance.m_height)
			{
				return true;
			}
		}
		return false;
	}

	public bool Inside(Vector3 p, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(p, ((Component)this).transform.position) < radius)
		{
			return p.y - radius < ((Component)this).transform.position.y + m_height;
		}
		return false;
	}

	public static bool IsInsideOtherMister(Vector3 p, Mister ignore)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		foreach (Mister instance in m_instances)
		{
			if (!((Object)(object)instance == (Object)(object)ignore) && Vector3.Distance(p, ((Component)instance).transform.position) < instance.m_radius && p.y < ((Component)instance).transform.position.y + instance.m_height)
			{
				return true;
			}
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
	}
}
