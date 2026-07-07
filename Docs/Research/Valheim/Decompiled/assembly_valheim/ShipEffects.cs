using System.Collections.Generic;
using UnityEngine;

public class ShipEffects : MonoBehaviour, IMonoUpdater
{
	public Transform m_shadow;

	public float m_offset = 0.01f;

	public float m_minimumWakeVel = 5f;

	public GameObject m_speedWakeRoot;

	public GameObject m_wakeSoundRoot;

	public GameObject m_inWaterSoundRoot;

	public float m_audioFadeDuration = 2f;

	public AudioSource m_sailSound;

	public float m_sailFadeDuration = 1f;

	public GameObject m_splashEffects;

	private ParticleSystem[] m_wakeParticles;

	private float m_sailBaseVol = 1f;

	private readonly List<KeyValuePair<AudioSource, float>> m_wakeSounds = new List<KeyValuePair<AudioSource, float>>();

	private readonly List<KeyValuePair<AudioSource, float>> m_inWaterSounds = new List<KeyValuePair<AudioSource, float>>();

	private WaterVolume m_previousWaterVolume;

	private Rigidbody m_body;

	private Ship m_ship;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		ZNetView componentInParent = ((Component)this).GetComponentInParent<ZNetView>();
		if (Object.op_Implicit((Object)(object)componentInParent) && componentInParent.GetZDO() == null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		m_body = ((Component)this).GetComponentInParent<Rigidbody>();
		m_ship = ((Component)this).GetComponentInParent<Ship>();
		if (Object.op_Implicit((Object)(object)m_speedWakeRoot))
		{
			m_wakeParticles = m_speedWakeRoot.GetComponentsInChildren<ParticleSystem>();
		}
		if (Object.op_Implicit((Object)(object)m_wakeSoundRoot))
		{
			AudioSource[] componentsInChildren = m_wakeSoundRoot.GetComponentsInChildren<AudioSource>();
			foreach (AudioSource val in componentsInChildren)
			{
				val.pitch = Random.Range(0.9f, 1.1f);
				m_wakeSounds.Add(new KeyValuePair<AudioSource, float>(val, val.volume));
			}
		}
		if (Object.op_Implicit((Object)(object)m_inWaterSoundRoot))
		{
			AudioSource[] componentsInChildren = m_inWaterSoundRoot.GetComponentsInChildren<AudioSource>();
			foreach (AudioSource val2 in componentsInChildren)
			{
				val2.pitch = Random.Range(0.9f, 1.1f);
				m_inWaterSounds.Add(new KeyValuePair<AudioSource, float>(val2, val2.volume));
			}
		}
		if (Object.op_Implicit((Object)(object)m_sailSound))
		{
			m_sailBaseVol = m_sailSound.volume;
			m_sailSound.pitch = Random.Range(0.9f, 1.1f);
		}
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void CustomLateUpdate(float deltaTime)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!Floating.IsUnderWater(((Component)this).transform.position, ref m_previousWaterVolume))
		{
			((Component)m_shadow).gameObject.SetActive(false);
			SetWake(enabled: false, deltaTime);
			FadeSounds(m_inWaterSounds, enabled: false, deltaTime);
			return;
		}
		((Component)m_shadow).gameObject.SetActive(true);
		Vector3 linearVelocity = m_body.linearVelocity;
		bool enabled = ((Vector3)(ref linearVelocity)).magnitude > m_minimumWakeVel;
		FadeSounds(m_inWaterSounds, enabled: true, deltaTime);
		SetWake(enabled, deltaTime);
		if (Object.op_Implicit((Object)(object)m_sailSound))
		{
			float target = (m_ship.IsSailUp() ? m_sailBaseVol : 0f);
			FadeSound(m_sailSound, target, m_sailFadeDuration, deltaTime);
		}
		if ((Object)(object)m_splashEffects != (Object)null)
		{
			m_splashEffects.SetActive(m_ship.HasPlayerOnboard());
		}
	}

	private void SetWake(bool enabled, float dt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystem[] wakeParticles = m_wakeParticles;
		for (int i = 0; i < wakeParticles.Length; i++)
		{
			EmissionModule emission = wakeParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = enabled;
		}
		FadeSounds(m_wakeSounds, enabled, dt);
	}

	private void FadeSounds(List<KeyValuePair<AudioSource, float>> sources, bool enabled, float dt)
	{
		foreach (KeyValuePair<AudioSource, float> source in sources)
		{
			if (enabled)
			{
				FadeSound(source.Key, source.Value, m_audioFadeDuration, dt);
			}
			else
			{
				FadeSound(source.Key, 0f, m_audioFadeDuration, dt);
			}
		}
	}

	private static void FadeSound(AudioSource source, float target, float fadeDuration, float dt)
	{
		float num = dt / fadeDuration;
		if (target > 0f)
		{
			if (!source.isPlaying)
			{
				source.Play();
			}
			source.volume = Mathf.MoveTowards(source.volume, target, num);
		}
		else if (source.isPlaying)
		{
			source.volume = Mathf.MoveTowards(source.volume, 0f, num);
			if (source.volume <= 0f)
			{
				source.Stop();
			}
		}
	}
}
