using System;
using System.Text;
using SoftReferenceableAssets;
using Splatform;
using UnityEngine;

public class GraphicsSettingsManager : MonoBehaviour
{
	public const int c_customPresetId = 100;

	private const string c_tesselationShaderKeyword = "TESSELATION_ON";

	private const string c_SteamDeckPlatformName = "Steam Deck";

	private const string c_WindowsPlatformName = "Windows";

	private const string c_MacOSPlatformName = "MacOS";

	private const string c_LinuxPlatformName = "Linux";

	private const string c_UnknownPlatformName = "Unknown";

	public const bool c_debugCustomizableGraphicsSettings = false;

	private static GraphicsSettingsManager s_instance;

	private static readonly GraphicsSettingsState s_defaultGraphicsSettings = new GraphicsSettingsState
	{
		m_presentSettings = new PresentSettingsState
		{
			m_fpsLimit = -1,
			m_vsync = false
		},
		m_vegetation = ClutterSystem.Quality.High,
		m_lod = 2,
		m_lights = 2,
		m_shadowQuality = 2,
		m_pointLights = 3,
		m_pointLightShadows = 2,
		m_ssao = GraphicsSettingInt.SSAO.GetRange().m_maxValue,
		m_target3DResolutionVertical = int.MaxValue,
		m_upscalingAlgorithm = UpscalingAlgorithm.Bilinear,
		m_distantShadows = true,
		m_tesselation = true,
		m_bloom = true,
		m_depthOfField = true,
		m_motionBlur = true,
		m_chromaticAberration = true,
		m_sunShafts = true,
		m_softParticles = true,
		m_antiAliasing = true,
		m_anisotropicTextures = true
	};

	[SerializeField]
	private GlobalGraphicsConfiguration m_globalConfig;

	private PresentManager m_presentManager = new PresentManager();

	private SoftReference<GraphicsConfiguration> m_config;

	private GraphicsSettingsState m_currentSettings;

	private int m_currentPresetID = 100;

	private bool m_isInitialized;

	private bool m_isInBackground;

	private bool m_isInLimitedMode;

	public static GraphicsSettingsManager Instance => s_instance;

	private GraphicsConfiguration Config
	{
		get
		{
			if (!m_config.IsLoaded)
			{
				ZLog.LogError((object)"Can't return the graphics config when it's not loaded!");
				return null;
			}
			return m_config.Asset;
		}
	}

	public ref GraphicsSettingsState CurrentSettingsRaw => ref m_currentSettings;

	public int CurrentPresetID
	{
		get
		{
			GraphicsModeConfiguration currentGraphicsModeConfiguration = GetCurrentGraphicsModeConfiguration();
			GraphicsSettingsPreset preset;
			bool flag = currentGraphicsModeConfiguration.TryFindPresetByID(m_currentPresetID, out preset);
			if (currentGraphicsModeConfiguration.HasCustomPreset)
			{
				if (!flag)
				{
					return 100;
				}
				return m_currentPresetID;
			}
			if (!flag || !IsPresetSupported(preset))
			{
				return currentGraphicsModeConfiguration.DefaultPreset.m_type.ID;
			}
			return m_currentPresetID;
		}
	}

	public static event Action GraphicsSettingsChanged;

	private GraphicsMode GetCurrentGraphicsMode(bool includeBackground = true)
	{
		if (includeBackground && m_isInBackground)
		{
			return GraphicsMode.Background;
		}
		if (m_isInLimitedMode)
		{
			return GraphicsMode.Limited;
		}
		return GraphicsMode.Standard;
	}

	public GraphicsSettingsState GetCurrentSettingsWithCurrentPresetApplied(bool includeBackground)
	{
		GraphicsSettingsState state = m_currentSettings;
		int currentPresetID = CurrentPresetID;
		if (currentPresetID == 100)
		{
			return state;
		}
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GetCurrentGraphicsModeConfiguration(includeBackground);
		if (currentGraphicsModeConfiguration.TryFindPresetByID(currentPresetID, out var preset))
		{
			SetGraphicsSettingsFromPreset(currentGraphicsModeConfiguration, ref state, preset);
		}
		return state;
	}

	public bool IsPresetSupported(GraphicsSettingsPreset preset)
	{
		if (preset == null)
		{
			throw new ArgumentNullException("preset");
		}
		if (preset.TryGetQualitySetting(GraphicsSettingInt.FpsLimit, out int value) && value > 0)
		{
			return IsTargetFrameRateSupported((uint)value);
		}
		return true;
	}

	public GraphicsModeConfiguration GetCurrentGraphicsModeConfiguration(bool includeBackground = false)
	{
		return Config.GetGraphicsModeConfiguration(GetCurrentGraphicsMode(includeBackground));
	}

	public static bool CanChangePresentSettings()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		if (PlatformManager.DistributionPlatform == null)
		{
			return true;
		}
		if (PlatformManager.DistributionPlatform.HardwareInfoProvider == null)
		{
			return true;
		}
		return (int)PlatformManager.DistributionPlatform.HardwareInfoProvider.HardwareInfo.m_category != 2;
	}

	public bool IsAccessibilitySetting(GraphicsSettingBool setting)
	{
		return m_globalConfig.AccessibilitySettings.HasFlag(setting);
	}

	private void Awake()
	{
		if ((Object)(object)s_instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		s_instance = this;
		m_presentManager.Initialize();
		m_presentManager.SetTargetFrameRate(60);
		m_presentManager.ResolutionOrRefreshRateChanged += OnGraphicsModeChanged;
		if (PlatformInitializer.PreferencesInitialized)
		{
			Initialize();
		}
	}

	private void OnDestroy()
	{
		if (!((Object)(object)s_instance != (Object)(object)this))
		{
			m_presentManager.ResolutionOrRefreshRateChanged -= OnGraphicsModeChanged;
			if (m_config.IsValid)
			{
				m_config.Release();
			}
			s_instance = null;
		}
	}

	private void OnEnable()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (PlatformManager.DistributionPlatform != null)
		{
			if (PlatformManager.DistributionPlatform.HardwareInfoProvider != null)
			{
				PlatformManager.DistributionPlatform.HardwareInfoProvider.PerformanceCharacteristicsChanged += new PerformanceCharacteristicsChangedHandler(OnPerformanceCharacteristicsChanged);
			}
			if (PlatformManager.DistributionPlatform.PLMProvider != null)
			{
				PlatformManager.DistributionPlatform.PLMProvider.IsRunningInBackgroundChanged += new IsRunningInBackgroundChangedHandler(OnIsRunningInBackgroundChanged);
			}
		}
	}

	private void OnDisable()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (PlatformManager.DistributionPlatform != null)
		{
			if (PlatformManager.DistributionPlatform.HardwareInfoProvider != null)
			{
				PlatformManager.DistributionPlatform.HardwareInfoProvider.PerformanceCharacteristicsChanged -= new PerformanceCharacteristicsChangedHandler(OnPerformanceCharacteristicsChanged);
			}
			if (PlatformManager.DistributionPlatform.PLMProvider != null)
			{
				PlatformManager.DistributionPlatform.PLMProvider.IsRunningInBackgroundChanged -= new IsRunningInBackgroundChangedHandler(OnIsRunningInBackgroundChanged);
			}
		}
	}

	private void Update()
	{
		if (!m_isInitialized && PlatformInitializer.PreferencesInitialized)
		{
			Initialize();
		}
		m_presentManager.Update();
	}

	private void Initialize()
	{
		QualitySettings.maxQueuedFrames = 2;
		m_isInitialized = true;
		if (LoadGraphicsConfigurationForCurrentPlatform())
		{
			ApplyStartupSettings();
		}
	}

	private bool LoadGraphicsConfigurationForCurrentPlatform()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		string currentPlatform = GetCurrentPlatform();
		m_config = m_globalConfig.GetConfigurationForPlatform(currentPlatform);
		if (!m_config.IsValid)
		{
			ZLog.LogError((object)"No configuration was available for this platform!");
			return false;
		}
		if ((int)m_config.Load() != 0)
		{
			ZLog.LogError((object)"Failed to load configuration for this platform!");
			return false;
		}
		return true;
	}

	public void SetGraphicsSettingsFromPreset(GraphicsModeConfiguration config, ref GraphicsSettingsState state, GraphicsSettingsPreset preset)
	{
		SetGraphicsSettingsFromPreset(config, ref state, preset, !config.HasCustomPreset);
	}

	public void SetGraphicsSettingsFromPreset(GraphicsModeConfiguration config, ref GraphicsSettingsState state, GraphicsSettingsPreset preset, bool unchangeableOnly)
	{
		for (int i = 0; i < 32; i++)
		{
			GraphicsSettingInt graphicsSettingInt = (GraphicsSettingInt)(1 << i);
			if (Enum.IsDefined(typeof(GraphicsSettingInt), graphicsSettingInt) && (!unchangeableOnly || !config.CanCustomizeGraphicsSetting(graphicsSettingInt)) && preset.TryGetQualitySetting(graphicsSettingInt, out int value))
			{
				state.SetValue(graphicsSettingInt, value);
			}
		}
		for (int j = 0; j < 32; j++)
		{
			GraphicsSettingBool graphicsSettingBool = (GraphicsSettingBool)(1 << j);
			if (Enum.IsDefined(typeof(GraphicsSettingBool), graphicsSettingBool) && (!unchangeableOnly || !config.CanCustomizeGraphicsSetting(graphicsSettingBool)) && preset.TryGetQualitySetting(graphicsSettingBool, out var value2))
			{
				if (m_globalConfig.AccessibilitySettings.HasFlag(graphicsSettingBool))
				{
					state.SetValue(graphicsSettingBool, value2 && state.GetValue(graphicsSettingBool));
				}
				else
				{
					state.SetValue(graphicsSettingBool, value2);
				}
			}
		}
	}

	private void ApplyStartupSettings()
	{
		LoadGraphicsSettingsFromPlayerPrefs();
		ApplyGraphicsSettingsToCurrentSession();
	}

	public void SaveAndApplyGraphicsSettingsCustom(ref GraphicsSettingsState settings)
	{
		SaveGraphicsSettingsToPlayerPrefs(ref settings, null);
		ApplyGraphicsSettingsToCurrentSession();
	}

	public void SaveAndApplyGraphicsSettingsWithPreset(ref GraphicsSettingsState settings, GraphicsSettingsPresetType presetType)
	{
		if (!GetCurrentGraphicsModeConfiguration().TryFindPresetByID(presetType.ID, out var _))
		{
			throw new ArgumentException($"Preset type {presetType} does not exist in the current graphics mode config!");
		}
		SaveGraphicsSettingsToPlayerPrefs(ref settings, presetType);
		ApplyGraphicsSettingsToCurrentSession();
	}

	public bool IsTargetFrameRateSupported(uint frameRate)
	{
		if (Config.PresetFrameRateTargetMustMatchDisplay)
		{
			return m_presentManager.IsTargetFrameRateEvenlyDivisibleByASupportedFrameRate(frameRate);
		}
		return true;
	}

	private void ApplyGraphicsSettingsToCurrentSession()
	{
		GraphicsSettingsState settings = GetCurrentSettingsWithCurrentPresetApplied(includeBackground: true);
		StringBuilder stringBuilder = new StringBuilder();
		m_presentManager.SetTargetFrameRate(settings.m_presentSettings.m_fpsLimit);
		m_presentManager.SetVSyncEnabled(settings.m_presentSettings.m_vsync);
		ApplyTargetResolutionSetting(settings.m_target3DResolutionVertical, settings.m_upscalingAlgorithm);
		ApplyShaderKeywords(settings);
		ApplyQualitySettings(ref settings);
		ApplyLightLod(settings);
		GraphicsSettingsManager.GraphicsSettingsChanged?.Invoke();
		ZLog.Log((object)stringBuilder.ToString());
	}

	private void SaveGraphicsSettingsToPlayerPrefs(ref GraphicsSettingsState settings, GraphicsSettingsPresetType presetType)
	{
		int num = presetType?.ID ?? 100;
		m_currentSettings = settings;
		m_currentPresetID = num;
		PlatformPrefs.SetInt("GraphicsQualityMode", num);
		PlatformPrefs.SetInt("FPSLimit", settings.m_presentSettings.m_fpsLimit);
		PlatformPrefs.SetBool("VSync", settings.m_presentSettings.m_vsync);
		PlatformPrefs.SetInt("ClutterQuality", (int)settings.m_vegetation);
		PlatformPrefs.SetInt("LodBias", settings.m_lod);
		PlatformPrefs.SetInt("Lights", settings.m_lights);
		PlatformPrefs.SetInt("ShadowQuality", settings.m_shadowQuality);
		PlatformPrefs.SetInt("PointLights", settings.m_pointLights);
		PlatformPrefs.SetInt("PointLightShadows", settings.m_pointLightShadows);
		PlatformPrefs.SetInt("Target3DResolutionVertical", settings.m_target3DResolutionVertical);
		PlatformPrefs.SetInt("UpscalingAlgorithm", (int)settings.m_upscalingAlgorithm);
		PlatformPrefs.SetInt("SSAO_2", settings.m_ssao);
		PlatformPrefs.SetBool("DistantShadows", settings.m_distantShadows);
		PlatformPrefs.SetBool("Tesselation", settings.m_tesselation);
		PlatformPrefs.SetBool("Bloom", settings.m_bloom);
		PlatformPrefs.SetBool("DOF", settings.m_depthOfField);
		PlatformPrefs.SetBool("MotionBlur", settings.m_motionBlur);
		PlatformPrefs.SetBool("ChromaticAberration", settings.m_chromaticAberration);
		PlatformPrefs.SetBool("SunShafts", settings.m_sunShafts);
		PlatformPrefs.SetBool("SoftPart", settings.m_softParticles);
		PlatformPrefs.SetBool("AntiAliasing", settings.m_antiAliasing);
	}

	private void LoadGraphicsSettingsFromPlayerPrefs()
	{
		int num = ((Config == null) ? 100 : Config.GetGraphicsModeConfiguration(GetCurrentGraphicsMode(includeBackground: false)).DefaultPreset.m_type.ID);
		m_currentPresetID = PlatformPrefs.GetInt("GraphicsQualityMode", num);
		int num2 = PlatformPrefs.GetInt("SSAO_2", -1);
		if (num2 < 0)
		{
			bool @bool = PlatformPrefs.GetBool("SSAO", false);
			num2 = ((@bool != PlatformPrefs.GetBool("SSAO", true)) ? s_defaultGraphicsSettings.m_ssao : (@bool ? GraphicsSettingInt.SSAO.GetRange().m_maxValue : GraphicsSettingInt.SSAO.GetRange().m_minValue));
		}
		m_currentSettings = new GraphicsSettingsState
		{
			m_presentSettings = new PresentSettingsState
			{
				m_fpsLimit = PlatformPrefs.GetInt("FPSLimit", s_defaultGraphicsSettings.m_presentSettings.m_fpsLimit),
				m_vsync = PlatformPrefs.GetBool("VSync", s_defaultGraphicsSettings.m_presentSettings.m_vsync)
			},
			m_vegetation = (ClutterSystem.Quality)PlatformPrefs.GetInt("ClutterQuality", (int)s_defaultGraphicsSettings.m_vegetation),
			m_lod = PlatformPrefs.GetInt("LodBias", s_defaultGraphicsSettings.m_lod),
			m_lights = PlatformPrefs.GetInt("Lights", s_defaultGraphicsSettings.m_lights),
			m_shadowQuality = PlatformPrefs.GetInt("ShadowQuality", s_defaultGraphicsSettings.m_shadowQuality),
			m_pointLights = PlatformPrefs.GetInt("PointLights", s_defaultGraphicsSettings.m_pointLights),
			m_pointLightShadows = PlatformPrefs.GetInt("PointLightShadows", s_defaultGraphicsSettings.m_pointLightShadows),
			m_ssao = num2,
			m_target3DResolutionVertical = GetTargetResolutionVertical(),
			m_upscalingAlgorithm = (UpscalingAlgorithm)PlatformPrefs.GetInt("UpscalingAlgorithm", (int)s_defaultGraphicsSettings.m_upscalingAlgorithm),
			m_distantShadows = PlatformPrefs.GetBool("DistantShadows", s_defaultGraphicsSettings.m_distantShadows),
			m_tesselation = PlatformPrefs.GetBool("Tesselation", s_defaultGraphicsSettings.m_tesselation),
			m_bloom = PlatformPrefs.GetBool("Bloom", s_defaultGraphicsSettings.m_bloom),
			m_depthOfField = PlatformPrefs.GetBool("DOF", s_defaultGraphicsSettings.m_depthOfField),
			m_motionBlur = PlatformPrefs.GetBool("MotionBlur", s_defaultGraphicsSettings.m_motionBlur),
			m_chromaticAberration = PlatformPrefs.GetBool("ChromaticAberration", s_defaultGraphicsSettings.m_chromaticAberration),
			m_sunShafts = PlatformPrefs.GetBool("SunShafts", s_defaultGraphicsSettings.m_sunShafts),
			m_softParticles = PlatformPrefs.GetBool("SoftPart", s_defaultGraphicsSettings.m_softParticles),
			m_antiAliasing = PlatformPrefs.GetBool("AntiAliasing", s_defaultGraphicsSettings.m_antiAliasing)
		};
	}

	private int GetTargetResolutionVertical()
	{
		int num = PlatformPrefs.GetInt("Target3DResolutionVertical", -1);
		if (num < 0)
		{
			float @float = PlatformPrefs.GetFloat("RenderScale", float.NaN);
			num = (float.IsNaN(@float) ? s_defaultGraphicsSettings.m_target3DResolutionVertical : ((@float != 1f) ? Mathf.RoundToInt((float)Screen.height * @float) : int.MaxValue));
		}
		return num;
	}

	private string GetCurrentPlatform()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Invalid comparison between Unknown and I4
		if (PlatformManager.DistributionPlatform != null && PlatformManager.DistributionPlatform.HardwareInfoProvider != null)
		{
			HardwareInfo hardwareInfo = PlatformManager.DistributionPlatform.HardwareInfoProvider.HardwareInfo;
			if ((int)hardwareInfo.m_category == 2)
			{
				return hardwareInfo.m_product;
			}
		}
		if (Settings.IsSteamRunningOnSteamDeck())
		{
			return "Steam Deck";
		}
		if ((int)Application.platform == 1 || (int)Application.platform == 0)
		{
			return "MacOS";
		}
		if ((int)Application.platform == 2 || (int)Application.platform == 7)
		{
			return "Windows";
		}
		if ((int)Application.platform == 13 || (int)Application.platform == 16)
		{
			return "Linux";
		}
		return "Unknown";
	}

	private void OnPerformanceCharacteristicsChanged()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		GraphicsMode currentGraphicsMode = GetCurrentGraphicsMode();
		m_isInLimitedMode = (int)PlatformManager.DistributionPlatform.HardwareInfoProvider.PerformanceCharacteristics.m_performancePriority == 1;
		if (currentGraphicsMode != GetCurrentGraphicsMode())
		{
			OnGraphicsModeChanged();
		}
	}

	private void OnIsRunningInBackgroundChanged(bool isInBackground)
	{
		GraphicsMode currentGraphicsMode = GetCurrentGraphicsMode();
		m_isInBackground = isInBackground;
		if (currentGraphicsMode != GetCurrentGraphicsMode())
		{
			OnGraphicsModeChanged();
		}
	}

	private void OnGraphicsModeChanged()
	{
		ZLog.Log((object)$"Graphics mode changed: {GetCurrentGraphicsMode()}");
		if (GetCurrentGraphicsModeConfiguration(includeBackground: true) == null)
		{
			ZLog.LogWarning((object)$"No preset for graphics mode {GetCurrentGraphicsMode()}! Doing nothing.");
		}
		else
		{
			ApplyGraphicsSettingsToCurrentSession();
		}
	}

	private void DebugPrintConfigInfo(string platform, GraphicsConfiguration config)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Loaded config for platform " + platform + ":");
		for (int i = 0; i < config.PresetSets.Count; i++)
		{
			GraphicsModeConfiguration graphicsModeConfiguration = config.PresetSets[i];
			stringBuilder.AppendLine($"Preset set type: {graphicsModeConfiguration.Mode}");
			stringBuilder.AppendLine($"\tPreset count: {graphicsModeConfiguration.Presets.Count}");
			stringBuilder.AppendLine($"\tDefault preset: {graphicsModeConfiguration.DefaultPresetIndex}");
		}
		ZLog.Log((object)stringBuilder.ToString());
	}

	private static float GetLodBias(int level)
	{
		return level switch
		{
			0 => 1f, 
			1 => 1.5f, 
			3 => 5f, 
			_ => 2f, 
		};
	}

	private static int GetLightLimit(int level)
	{
		return level switch
		{
			0 => 2, 
			1 => 4, 
			_ => 8, 
		};
	}

	public static int GetPointLightLimit(int level)
	{
		return level switch
		{
			0 => 4, 
			1 => 15, 
			3 => -1, 
			_ => 40, 
		};
	}

	public static int GetPointLightShadowLimit(int level)
	{
		return level switch
		{
			0 => 0, 
			1 => 1, 
			3 => -1, 
			_ => 3, 
		};
	}

	private static void ApplyTargetResolutionSetting(int resolution, UpscalingAlgorithm upscalingAlgorithm)
	{
		if (resolution <= 0)
		{
			UpscaledFrameBuffer.m_autoTargetResolution = true;
		}
		else
		{
			UpscaledFrameBuffer.m_autoTargetResolution = false;
			if (resolution == int.MaxValue)
			{
				UpscaledFrameBuffer.m_targetResolutionVertical = uint.MaxValue;
			}
			else
			{
				UpscaledFrameBuffer.m_targetResolutionVertical = (uint)resolution;
			}
		}
		UpscaledFrameBuffer.m_upscalingAlgorithm = upscalingAlgorithm;
	}

	private static void ApplyQualitySettings(ref GraphicsSettingsState settings)
	{
		QualitySettings.softParticles = settings.m_softParticles;
		QualitySettings.lodBias = GetLodBias(settings.m_lod);
		QualitySettings.pixelLightCount = GetLightLimit(settings.m_lights);
		switch (settings.m_shadowQuality)
		{
		case 0:
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 80f;
			QualitySettings.shadowResolution = (ShadowResolution)0;
			break;
		case 1:
			QualitySettings.shadowCascades = 3;
			QualitySettings.shadowDistance = 120f;
			QualitySettings.shadowResolution = (ShadowResolution)1;
			break;
		case 2:
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 150f;
			QualitySettings.shadowResolution = (ShadowResolution)2;
			break;
		}
	}

	private static void ApplyLightLod(GraphicsSettingsState settings)
	{
		LightLod.m_lightLimit = GetPointLightLimit(settings.m_pointLights);
		LightLod.m_shadowLimit = GetPointLightShadowLimit(settings.m_pointLightShadows);
	}

	private static void ApplyShaderKeywords(GraphicsSettingsState settings)
	{
		if (settings.m_tesselation)
		{
			Shader.EnableKeyword("TESSELATION_ON");
		}
		else
		{
			Shader.DisableKeyword("TESSELATION_ON");
		}
	}
}
