using System;
using System.Collections.Generic;
using UnityEngine;

public class Demister : MonoBehaviour
{
	public float m_disableForcefieldDelay;

	[NonSerialized]
	public ParticleSystemForceField m_forceField;

	private Vector3 m_lastUpdatePosition;

	private static List<Demister> m_instances = new List<Demister>();

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		m_forceField = ((Component)this).GetComponent<ParticleSystemForceField>();
		m_lastUpdatePosition = ((Component)this).transform.position;
		if (m_disableForcefieldDelay > 0f)
		{
			((MonoBehaviour)this).Invoke("DisableForcefield", m_disableForcefieldDelay);
		}
	}

	private void OnEnable()
	{
		m_instances.Add(this);
	}

	private void OnDisable()
	{
		m_instances.Remove(this);
	}

	private void DisableForcefield()
	{
		((Behaviour)m_forceField).enabled = false;
	}

	public float GetMovedDistance()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		if (position == m_lastUpdatePosition)
		{
			return 0f;
		}
		float num = Vector3.Distance(position, m_lastUpdatePosition);
		m_lastUpdatePosition = position;
		return Mathf.Min(num, 10f);
	}

	public static List<Demister> GetDemisters()
	{
		return m_instances;
	}
}
