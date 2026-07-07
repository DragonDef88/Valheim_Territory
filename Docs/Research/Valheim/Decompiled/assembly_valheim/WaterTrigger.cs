using System.Collections.Generic;
using UnityEngine;

public class WaterTrigger : MonoBehaviour, IMonoUpdater
{
	public EffectList m_effects = new EffectList();

	public float m_cooldownDelay = 2f;

	private float m_cooldownTimer;

	private WaterVolume m_previousAndOut;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Start()
	{
		m_cooldownTimer = Random.Range(0f, 2f);
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void CustomUpdate(float deltaTime, float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		m_cooldownTimer += deltaTime;
		if (!(m_cooldownTimer <= m_cooldownDelay))
		{
			Transform transform = ((Component)this).transform;
			Vector3 position = transform.position;
			if (Floating.IsUnderWater(position, ref m_previousAndOut))
			{
				m_effects.Create(position, transform.rotation, transform);
				m_cooldownTimer = 0f;
			}
		}
	}
}
