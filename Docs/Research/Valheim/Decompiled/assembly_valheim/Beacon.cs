using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
	public float m_range = 20f;

	private static List<Beacon> m_instances = new List<Beacon>();

	private void Awake()
	{
		m_instances.Add(this);
	}

	private void OnDestroy()
	{
		m_instances.Remove(this);
	}

	public static Beacon FindClosestBeaconInRange(Vector3 point)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Beacon beacon = null;
		float num = 999999f;
		foreach (Beacon instance in m_instances)
		{
			float num2 = Vector3.Distance(point, ((Component)instance).transform.position);
			if (num2 < instance.m_range && ((Object)(object)beacon == (Object)null || num2 < num))
			{
				beacon = instance;
				num = num2;
			}
		}
		return beacon;
	}

	public static void FindBeaconsInRange(Vector3 point, List<Beacon> becons)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		foreach (Beacon instance in m_instances)
		{
			if (Vector3.Distance(point, ((Component)instance).transform.position) < instance.m_range)
			{
				becons.Add(instance);
			}
		}
	}
}
