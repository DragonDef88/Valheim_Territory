using System;
using System.Collections.Generic;
using CircularBuffer;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMan : MonoBehaviour
{
	[Serializable]
	public class BiomeAmbients
	{
		public string m_name = "";

		public float m_forceFadeout = 3f;

		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		public List<AudioClip> m_randomAmbientClips = new List<AudioClip>();

		public List<AudioClip> m_randomAmbientClipsDay = new List<AudioClip>();

		public List<AudioClip> m_randomAmbientClipsNight = new List<AudioClip>();
	}

	private enum Snapshot
	{
		Default,
		Menu,
		Indoor
	}

	private class SoundHash
	{
		public int hash;

		public float playTime;

		public Vector3 position;

		public SoundHash(int h, float pt, Vector3 pos)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			hash = h;
			playTime = pt;
			position = pos;
		}
	}

	private bool m_zoneSystemValid;

	private bool m_envManValid;

	private readonly List<ZSFX> m_loopingSfx = new List<ZSFX>();

	private readonly List<int> m_checkedHashes = new List<int>();

	private readonly List<ZSFX> m_tmpSameSfx = new List<ZSFX>();

	private readonly List<float> m_tmpSameSfxDistance = new List<float>();

	private AudioListener m_activeAudioListener;

	private static AudioMan m_instance;

	[Header("Mixers")]
	public AudioMixerGroup m_ambientMixer;

	public AudioMixerGroup m_guiMixer;

	public AudioMixer m_masterMixer;

	public float m_snapshotTransitionTime = 2f;

	[Header("Wind")]
	public AudioClip m_windAudio;

	public float m_windMinVol;

	public float m_windMaxVol = 1f;

	public float m_windMinPitch = 0.5f;

	public float m_windMaxPitch = 1.5f;

	public float m_windVariation = 0.2f;

	public float m_windIntensityPower = 1.5f;

	[Header("Ocean")]
	public AudioClip m_oceanAudio;

	public float m_oceanVolumeMax = 1f;

	public float m_oceanVolumeMin = 1f;

	public float m_oceanFadeSpeed = 0.1f;

	public float m_oceanMoveSpeed = 0.1f;

	public float m_oceanDepthTreshold = 10f;

	[Header("Random ambients")]
	public float m_ambientFadeTime = 2f;

	[Min(1f)]
	public float m_randomAmbientInterval = 5f;

	[Range(0f, 1f)]
	public float m_randomAmbientChance = 0.5f;

	public float m_randomMinDistance = 5f;

	public float m_randomMaxDistance = 20f;

	public List<BiomeAmbients> m_randomAmbients = new List<BiomeAmbients>();

	public GameObject m_randomAmbientPrefab;

	[Header("Lava Ambience")]
	[Min(10f)]
	public float m_lavaScanRadius = 40f;

	[Min(0f)]
	public float m_lavaNoiseMinDistance = 2f;

	[Min(10f)]
	public float m_lavaNoiseMaxDistance = 10f;

	[Min(1f)]
	public float m_lavaNoiseInterval = 2.5f;

	[Range(0f, 1f)]
	public float m_lavaNoiseChance = 0.25f;

	public List<AudioClip> m_randomLavaNoises;

	public GameObject m_lavaLoopPrefab;

	[Space(16f)]
	public int m_maxLavaLoops;

	public float m_minDistanceBetweenLavaLoops = 10f;

	public float m_maxLavaLoopDistance = 40f;

	[Header("Shield Dome Hum")]
	public bool m_enableShieldDomeHum = true;

	public GameObject m_shieldHumPrefab;

	[Header("ZSFX Settings")]
	[Min(0f)]
	[Tooltip("How soon a sound trying to play after the same one counts as concurrent")]
	public float m_concurrencyThreshold = 0.2f;

	[Min(0f)]
	[Tooltip("Automatically makes sure no looping sounds are playing more than this many at a time. ZSFX components that have a max concurrency value set will use that instead.")]
	public int m_forcedMaxConcurrentLoops = 5;

	private AudioSource m_oceanAmbientSource;

	private AudioSource m_ambientLoopSource;

	private AudioSource m_windLoopSource;

	private AudioSource m_shieldHumSource;

	private AudioClip m_queuedAmbientLoop;

	private float m_queuedAmbientVol;

	private float m_ambientVol;

	private float m_randomAmbientTimer;

	private bool m_stopAmbientLoop;

	private bool m_indoor;

	private float m_oceanUpdateTimer;

	private bool m_haveOcean;

	private Vector3 m_avgOceanPoint = Vector3.zero;

	private Vector3 m_listenerPos = Vector3.zero;

	private float m_lavaAmbientTimer;

	private CircularBuffer<Vector3> m_validLavaPositions = new CircularBuffer<Vector3>(128);

	private List<ZSFX> m_ambientLavaLoops = new List<ZSFX>();

	private Snapshot m_currentSnapshot;

	private readonly CircularBuffer<SoundHash> m_soundList = new CircularBuffer<SoundHash>(512);

	public static AudioMan instance => m_instance;

	private void Awake()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		if ((Object)(object)m_instance != (Object)null)
		{
			ZLog.Log((object)"Audioman already exist, destroying self");
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		m_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		GameObject val = new GameObject("ocean_ambient_loop");
		val.transform.SetParent(((Component)this).transform);
		m_oceanAmbientSource = val.AddComponent<AudioSource>();
		m_oceanAmbientSource.loop = true;
		m_oceanAmbientSource.spatialBlend = 0.75f;
		m_oceanAmbientSource.outputAudioMixerGroup = m_ambientMixer;
		m_oceanAmbientSource.maxDistance = 128f;
		m_oceanAmbientSource.minDistance = 40f;
		m_oceanAmbientSource.spread = 90f;
		m_oceanAmbientSource.rolloffMode = (AudioRolloffMode)1;
		m_oceanAmbientSource.clip = m_oceanAudio;
		m_oceanAmbientSource.bypassReverbZones = true;
		m_oceanAmbientSource.dopplerLevel = 0f;
		m_oceanAmbientSource.volume = 0f;
		m_oceanAmbientSource.priority = 0;
		m_oceanAmbientSource.Play();
		GameObject val2 = new GameObject("ambient_loop");
		val2.transform.SetParent(((Component)this).transform);
		m_ambientLoopSource = val2.AddComponent<AudioSource>();
		m_ambientLoopSource.loop = true;
		m_ambientLoopSource.spatialBlend = 0f;
		m_ambientLoopSource.outputAudioMixerGroup = m_ambientMixer;
		m_ambientLoopSource.bypassReverbZones = true;
		m_ambientLoopSource.priority = 0;
		m_ambientLoopSource.volume = 0f;
		GameObject val3 = new GameObject("wind_loop");
		val3.transform.SetParent(((Component)this).transform);
		m_windLoopSource = val3.AddComponent<AudioSource>();
		m_windLoopSource.loop = true;
		m_windLoopSource.spatialBlend = 0f;
		m_windLoopSource.outputAudioMixerGroup = m_ambientMixer;
		m_windLoopSource.bypassReverbZones = true;
		m_windLoopSource.clip = m_windAudio;
		m_windLoopSource.volume = 0f;
		m_windLoopSource.priority = 0;
		m_windLoopSource.Play();
		if (m_enableShieldDomeHum)
		{
			GameObject val4 = Object.Instantiate<GameObject>(m_shieldHumPrefab);
			val4.transform.SetParent(((Component)this).transform);
			m_shieldHumSource = val4.GetComponent<AudioSource>();
		}
		m_maxLavaLoops = GetLoopingMaxConcurrency(m_lavaLoopPrefab.GetComponent<ZSFX>());
	}

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)SystemInfo.graphicsDeviceType == 4)
		{
			AudioListener.volume = 0f;
			return;
		}
		AudioListener.volume = PlatformPrefs.GetFloat("MasterVolume", AudioListener.volume);
		SetSFXVolume(PlatformPrefs.GetFloat("SfxVolume", GetSFXVolume()));
	}

	private void OnApplicationQuit()
	{
		StopAllAudio();
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_instance == (Object)(object)this)
		{
			m_instance = null;
		}
	}

	private void StopAllAudio()
	{
		AudioSource[] array = Object.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}

	private void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		float deltaTime = Time.deltaTime;
		m_zoneSystemValid = (Object)(object)ZoneSystem.instance != (Object)null;
		m_envManValid = (Object)(object)EnvMan.instance != (Object)null;
		bool inMenu = InMenu();
		m_listenerPos = ((Component)GetActiveAudioListener()).transform.position;
		UpdateAmbientLoop(deltaTime);
		UpdateRandomAmbient(deltaTime, inMenu);
		UpdateLavaAmbient(deltaTime, inMenu);
		UpdateSnapshots(deltaTime, inMenu);
		UpdateLoopingConcurrency();
		UpdateShieldHum();
	}

	private void UpdateShieldHum()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!m_enableShieldDomeHum)
		{
			return;
		}
		if (ShieldGenerator.HasShields())
		{
			if (!m_shieldHumSource.isPlaying)
			{
				m_shieldHumSource.Play();
			}
			((Component)m_shieldHumSource).transform.position = ShieldGenerator.GetClosestShieldPoint(((Component)GetActiveAudioListener()).transform.position);
		}
		else
		{
			m_shieldHumSource.Stop();
		}
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		m_zoneSystemValid = (Object)(object)ZoneSystem.instance != (Object)null;
		m_envManValid = (Object)(object)EnvMan.instance != (Object)null;
		UpdateOceanAmbiance(fixedDeltaTime);
		UpdateWindAmbience(fixedDeltaTime);
	}

	public static float GetSFXVolume()
	{
		if ((Object)(object)m_instance == (Object)null)
		{
			return 1f;
		}
		float num = default(float);
		m_instance.m_masterMixer.GetFloat("SfxVol", ref num);
		if (!(num > -80f))
		{
			return 0f;
		}
		return Mathf.Pow(10f, num / 10f);
	}

	public static void SetSFXVolume(float vol)
	{
		if (!((Object)(object)m_instance == (Object)null))
		{
			float num = ((vol > 0f) ? (Mathf.Log10(Mathf.Clamp(vol, 0.001f, 1f)) * 10f) : (-80f));
			m_instance.m_masterMixer.SetFloat("SfxVol", num);
			m_instance.m_masterMixer.SetFloat("GuiVol", num);
		}
	}

	private void UpdateRandomAmbient(float dt, bool inMenu)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (inMenu)
		{
			return;
		}
		m_randomAmbientTimer += dt;
		if (!(m_randomAmbientTimer > m_randomAmbientInterval))
		{
			return;
		}
		m_randomAmbientTimer = 0f;
		if (!(Random.value <= m_randomAmbientChance))
		{
			return;
		}
		float fadeoutDuration = 0f;
		if (SelectRandomAmbientClip(out var clip, out fadeoutDuration))
		{
			Vector3 randomAmbiencePoint = GetRandomAmbiencePoint();
			GameObject val = Object.Instantiate<GameObject>(m_randomAmbientPrefab, randomAmbiencePoint, Quaternion.identity, ((Component)this).transform);
			ZSFX component = val.GetComponent<ZSFX>();
			component.m_audioClips = (AudioClip[])(object)new AudioClip[1] { clip };
			component.Play();
			TimedDestruction component2 = val.GetComponent<TimedDestruction>();
			if (fadeoutDuration > 0f)
			{
				component.m_fadeOutDelay = 0f;
				component.m_fadeOutDuration = fadeoutDuration;
				component.m_fadeOutOnAwake = true;
				component2.m_timeout = fadeoutDuration + 2f;
			}
			else
			{
				component.m_fadeOutDelay = clip.length - 1f;
				component.m_fadeOutDuration = 1f;
				component.m_fadeOutOnAwake = true;
				component2.m_timeout = clip.length * 1.5f;
			}
			component2.Trigger();
		}
	}

	private void UpdateLavaAmbient(float dt, bool inMenu)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		ScanForLava();
		UpdateLavaAmbientLoops();
		if (inMenu || m_validLavaPositions.Size == 0 || !m_envManValid || EnvMan.instance.GetCurrentBiome() != Heightmap.Biome.AshLands)
		{
			return;
		}
		m_lavaAmbientTimer += dt;
		if (m_lavaAmbientTimer < m_lavaNoiseInterval)
		{
			return;
		}
		m_lavaAmbientTimer = 0f;
		if (Random.value > m_lavaNoiseChance)
		{
			return;
		}
		int i = 0;
		Vector3 val = Vector3.zero;
		for (; i < 5; i++)
		{
			val = m_validLavaPositions[Random.Range(0, m_validLavaPositions.Size - 1)];
			float num = VectorExtensions.DistanceTo(val, ((Component)GetActiveAudioListener()).transform.position);
			if (num > m_lavaNoiseMinDistance && num < m_lavaNoiseMaxDistance)
			{
				break;
			}
		}
		if (i != 5)
		{
			GameObject val2 = Object.Instantiate<GameObject>(m_randomAmbientPrefab, val, Quaternion.identity, ((Component)this).transform);
			ZSFX component = val2.GetComponent<ZSFX>();
			AudioClip val3 = m_randomLavaNoises[Random.Range(0, m_randomLavaNoises.Count - 1)];
			component.m_audioClips = (AudioClip[])(object)new AudioClip[1] { val3 };
			component.Play();
			TimedDestruction component2 = val2.GetComponent<TimedDestruction>();
			component2.m_timeout = val3.length;
			component2.Trigger();
		}
	}

	private void UpdateLavaAmbientLoops()
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (Time.frameCount % 24 != 0)
		{
			return;
		}
		if (m_ambientLavaLoops.Count < m_maxLavaLoops && m_validLavaPositions.Size > 0 && m_envManValid && EnvMan.instance.GetCurrentBiome() == Heightmap.Biome.AshLands)
		{
			Vector3 val = m_validLavaPositions[Random.Range(0, m_validLavaPositions.Size - 1)];
			float num = float.PositiveInfinity;
			foreach (ZSFX ambientLavaLoop in m_ambientLavaLoops)
			{
				Vector3 position = ((Component)ambientLavaLoop).transform.position;
				float num2 = VectorExtensions.DistanceTo(position, val);
				if (num2 < num && ((Component)ambientLavaLoop).transform.position != position)
				{
					num = num2;
				}
			}
			if (num <= m_minDistanceBetweenLavaLoops)
			{
				return;
			}
			ZSFX component = Object.Instantiate<GameObject>(m_lavaLoopPrefab, val, Quaternion.identity).GetComponent<ZSFX>();
			component.OnDestroyingSfx += delegate(ZSFX zsfx)
			{
				if (m_ambientLavaLoops.Contains(zsfx))
				{
					m_ambientLavaLoops.Remove(zsfx);
				}
			};
			m_ambientLavaLoops.Add(component);
		}
		for (int num3 = m_ambientLavaLoops.Count - 1; num3 >= 0; num3--)
		{
			ZSFX zSFX = m_ambientLavaLoops[num3];
			if (!(VectorExtensions.DistanceTo(((Component)zSFX).gameObject.transform.position, m_listenerPos) < m_maxLavaLoopDistance))
			{
				((Component)zSFX).GetComponent<TimedDestruction>().Trigger();
				m_ambientLavaLoops.Remove(zSFX);
			}
		}
	}

	private void ScanForLava()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (Time.frameCount % 12 == 0 && m_envManValid && EnvMan.instance.GetCurrentBiome() == Heightmap.Biome.AshLands && m_zoneSystemValid)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector2 normalized = ((Vector2)(ref insideUnitCircle)).normalized;
			normalized *= Random.Range(2f, m_lavaScanRadius);
			Vector3 position = m_listenerPos + new Vector3(normalized.x, 0f, normalized.y);
			if (ZoneSystem.instance.IsLava(ref position))
			{
				m_validLavaPositions.PushFront(position);
			}
		}
	}

	private Vector3 GetRandomAmbiencePoint()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.value * (float)Math.PI * 2f;
		float num2 = Random.Range(m_randomMinDistance, m_randomMaxDistance);
		return m_listenerPos + new Vector3(Mathf.Sin(num) * num2, 0f, Mathf.Cos(num) * num2);
	}

	private bool SelectRandomAmbientClip(out AudioClip clip, out float fadeoutDuration)
	{
		fadeoutDuration = 0f;
		clip = null;
		if (!m_envManValid)
		{
			return false;
		}
		EnvSetup currentEnvironment = EnvMan.instance.GetCurrentEnvironment();
		BiomeAmbients biomeAmbients = null;
		biomeAmbients = ((currentEnvironment == null || string.IsNullOrEmpty(currentEnvironment.m_ambientList)) ? GetBiomeAmbients(EnvMan.instance.GetCurrentBiome()) : GetAmbients(currentEnvironment.m_ambientList));
		if (biomeAmbients == null)
		{
			return false;
		}
		fadeoutDuration = biomeAmbients.m_forceFadeout;
		List<AudioClip> list = new List<AudioClip>(biomeAmbients.m_randomAmbientClips);
		List<AudioClip> collection = (EnvMan.IsDaylight() ? biomeAmbients.m_randomAmbientClipsDay : biomeAmbients.m_randomAmbientClipsNight);
		list.AddRange(collection);
		if (list.Count == 0)
		{
			return false;
		}
		clip = list[Random.Range(0, list.Count)];
		return true;
	}

	private void UpdateAmbientLoop(float dt)
	{
		if (!m_envManValid)
		{
			m_ambientLoopSource.Stop();
		}
		else if (Object.op_Implicit((Object)(object)m_queuedAmbientLoop) || m_stopAmbientLoop)
		{
			if (!m_ambientLoopSource.isPlaying || m_ambientLoopSource.volume <= 0f)
			{
				m_ambientLoopSource.Stop();
				m_stopAmbientLoop = false;
				if (Object.op_Implicit((Object)(object)m_queuedAmbientLoop))
				{
					m_ambientLoopSource.clip = m_queuedAmbientLoop;
					m_ambientLoopSource.volume = 0f;
					m_ambientLoopSource.Play();
					m_ambientVol = m_queuedAmbientVol;
					m_queuedAmbientLoop = null;
				}
			}
			else
			{
				m_ambientLoopSource.volume = Mathf.MoveTowards(m_ambientLoopSource.volume, 0f, dt / m_ambientFadeTime);
			}
		}
		else if (m_ambientLoopSource.isPlaying)
		{
			m_ambientLoopSource.volume = Mathf.MoveTowards(m_ambientLoopSource.volume, m_ambientVol, dt / m_ambientFadeTime);
		}
	}

	public void SetIndoor(bool indoor)
	{
		m_indoor = indoor;
	}

	private bool InMenu()
	{
		if (!((Object)(object)FejdStartup.instance != (Object)null) && !Menu.IsVisible() && (!Object.op_Implicit((Object)(object)Game.instance) || !Game.instance.WaitingForRespawn()))
		{
			return TextViewer.IsShowingIntro();
		}
		return true;
	}

	private void UpdateSnapshots(float dt, bool inMenu)
	{
		if (inMenu)
		{
			SetSnapshot(Snapshot.Menu);
		}
		else if (m_indoor)
		{
			SetSnapshot(Snapshot.Indoor);
		}
		else
		{
			SetSnapshot(Snapshot.Default);
		}
	}

	private void SetSnapshot(Snapshot snapshot)
	{
		if (m_currentSnapshot != snapshot)
		{
			m_currentSnapshot = snapshot;
			switch (snapshot)
			{
			case Snapshot.Default:
				m_masterMixer.FindSnapshot("Default").TransitionTo(m_snapshotTransitionTime);
				break;
			case Snapshot.Indoor:
				m_masterMixer.FindSnapshot("Indoor").TransitionTo(m_snapshotTransitionTime);
				break;
			case Snapshot.Menu:
				m_masterMixer.FindSnapshot("Menu").TransitionTo(m_snapshotTransitionTime);
				break;
			}
		}
	}

	public void StopAmbientLoop()
	{
		m_queuedAmbientLoop = null;
		m_stopAmbientLoop = true;
	}

	public void QueueAmbientLoop(AudioClip clip, float vol)
	{
		if ((!((Object)(object)m_queuedAmbientLoop == (Object)(object)clip) || m_queuedAmbientVol != vol) && (!((Object)(object)m_queuedAmbientLoop == (Object)null) || !((Object)(object)m_ambientLoopSource.clip == (Object)(object)clip) || m_ambientVol != vol))
		{
			m_queuedAmbientLoop = clip;
			m_queuedAmbientVol = vol;
			m_stopAmbientLoop = false;
		}
	}

	private void UpdateWindAmbience(float dt)
	{
		if (!m_zoneSystemValid || !m_envManValid)
		{
			m_windLoopSource.volume = 0f;
			return;
		}
		float windIntensity = EnvMan.instance.GetWindIntensity();
		windIntensity = Mathf.Pow(windIntensity, m_windIntensityPower);
		windIntensity += windIntensity * Mathf.Sin(Time.time) * Mathf.Sin(Time.time * 1.54323f) * Mathf.Sin(Time.time * 2.31237f) * m_windVariation;
		m_windLoopSource.volume = Mathf.Lerp(m_windMinVol, m_windMaxVol, windIntensity);
		m_windLoopSource.pitch = Mathf.Lerp(m_windMinPitch, m_windMaxPitch, windIntensity);
	}

	private void UpdateOceanAmbiance(float dt)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (!m_zoneSystemValid || !m_envManValid)
		{
			m_oceanAmbientSource.volume = 0f;
			return;
		}
		m_oceanUpdateTimer += dt;
		if (m_oceanUpdateTimer > 2f)
		{
			m_oceanUpdateTimer = 0f;
			m_haveOcean = FindAverageOceanPoint(out m_avgOceanPoint);
		}
		if (m_haveOcean)
		{
			float windIntensity = EnvMan.instance.GetWindIntensity();
			float num = Mathf.Lerp(m_oceanVolumeMin, m_oceanVolumeMax, windIntensity);
			m_oceanAmbientSource.volume = Mathf.MoveTowards(m_oceanAmbientSource.volume, num, m_oceanFadeSpeed * dt);
			((Component)m_oceanAmbientSource).transform.position = Vector3.Lerp(((Component)m_oceanAmbientSource).transform.position, m_avgOceanPoint, m_oceanMoveSpeed);
		}
		else
		{
			m_oceanAmbientSource.volume = Mathf.MoveTowards(m_oceanAmbientSource.volume, 0f, m_oceanFadeSpeed * dt);
		}
	}

	private bool FindAverageOceanPoint(out Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		int num = 0;
		Vector2i zone = ZoneSystem.GetZone(m_listenerPos);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Vector2i id = zone;
				id.x += j;
				id.y += i;
				Vector3 zonePos = ZoneSystem.GetZonePos(id);
				if (IsOceanZone(zonePos))
				{
					num++;
					val += zonePos;
				}
			}
		}
		if (num > 0)
		{
			val /= (float)num;
			point = val;
			point.y = 30f;
			return true;
		}
		point = Vector3.zero;
		return false;
	}

	private bool IsOceanZone(Vector3 centerPos)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		float groundHeight = ZoneSystem.instance.GetGroundHeight(centerPos);
		if (30f - groundHeight > m_oceanDepthTreshold)
		{
			return true;
		}
		return false;
	}

	private BiomeAmbients GetAmbients(string name)
	{
		foreach (BiomeAmbients randomAmbient in m_randomAmbients)
		{
			if (randomAmbient.m_name == name)
			{
				return randomAmbient;
			}
		}
		return null;
	}

	private BiomeAmbients GetBiomeAmbients(Heightmap.Biome biome)
	{
		foreach (BiomeAmbients randomAmbient in m_randomAmbients)
		{
			if ((randomAmbient.m_biome & biome) != 0)
			{
				return randomAmbient;
			}
		}
		return null;
	}

	public bool RequestPlaySound(ZSFX sfx)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (sfx.IsLooping())
		{
			RegisterLoopingSound(sfx);
			return true;
		}
		if (sfx.m_maxConcurrentSources <= 0)
		{
			return true;
		}
		int hash = sfx.m_hash;
		float time = Time.time;
		Vector3 position = ((Component)sfx).gameObject.transform.position;
		int num = 0;
		foreach (SoundHash sound in m_soundList)
		{
			if (hash == sound.hash)
			{
				if (time - sound.playTime < m_concurrencyThreshold && Vector3.Distance(sound.position, position) < sfx.GetConcurrencyDistance())
				{
					num++;
				}
				if (num >= sfx.m_maxConcurrentSources)
				{
					return false;
				}
			}
		}
		m_soundList.PushFront(new SoundHash(hash, time, position));
		return true;
	}

	private void RegisterLoopingSound(ZSFX sfx)
	{
		if (GetLoopingMaxConcurrency(sfx) < 1)
		{
			return;
		}
		int num = 0;
		foreach (ZSFX item in m_loopingSfx)
		{
			if (item.m_hash == sfx.m_hash)
			{
				num++;
			}
		}
		if (num > sfx.m_maxConcurrentSources)
		{
			sfx.ConcurrencyDisable();
		}
		m_loopingSfx.Add(sfx);
		sfx.OnDestroyingSfx += delegate(ZSFX zsfx)
		{
			m_loopingSfx.Remove(zsfx);
		};
	}

	private int GetLoopingMaxConcurrency(ZSFX sfx)
	{
		if (sfx.m_maxConcurrentSources < 0)
		{
			return -1;
		}
		if (sfx.m_maxConcurrentSources != 0)
		{
			return sfx.m_maxConcurrentSources;
		}
		return m_forcedMaxConcurrentLoops;
	}

	private void UpdateLoopingConcurrency()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (Time.frameCount % 16 != 0)
		{
			return;
		}
		Vector3 position = ((Component)Utils.GetMainCamera()).transform.position;
		m_checkedHashes.Clear();
		foreach (ZSFX item in m_loopingSfx)
		{
			if (m_checkedHashes.Contains(item.m_hash))
			{
				continue;
			}
			m_checkedHashes.Add(item.m_hash);
			int maxConcurrentSources = item.m_maxConcurrentSources;
			m_tmpSameSfx.Clear();
			m_tmpSameSfxDistance.Clear();
			foreach (ZSFX item2 in m_loopingSfx)
			{
				if (item.m_hash == item2.m_hash)
				{
					float num = Vector3.Distance(((Component)item2).gameObject.transform.position, position);
					Utils.InsertSortNoAlloc<ZSFX>(m_tmpSameSfx, item2, m_tmpSameSfxDistance, num);
				}
			}
			for (int i = 0; i < m_tmpSameSfx.Count; i++)
			{
				if (i > maxConcurrentSources)
				{
					m_tmpSameSfx[i].ConcurrencyDisable();
				}
				else
				{
					m_tmpSameSfx[i].ConcurrencyEnable();
				}
			}
		}
	}

	public AudioListener GetActiveAudioListener()
	{
		if (Object.op_Implicit((Object)(object)m_activeAudioListener) && ((Behaviour)m_activeAudioListener).isActiveAndEnabled)
		{
			return m_activeAudioListener;
		}
		AudioListener[] array = Object.FindObjectsOfType<AudioListener>(false);
		m_activeAudioListener = Array.Find(array, (AudioListener l) => ((Behaviour)l).enabled);
		return m_activeAudioListener;
	}
}
