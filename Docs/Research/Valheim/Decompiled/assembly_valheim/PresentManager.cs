using System;
using System.Collections.Generic;
using Splatform;
using UnityEngine;

public class PresentManager
{
	public const int m_backgroundFPS = 30;

	public const int m_menuFPS = 60;

	public const int m_minimumFPSLimit = 30;

	public const int m_maximumFPSLimit = 360;

	private const float c_FrameRateMarginPercent = 0.002f;

	private int? m_actualFrameRateLimit;

	private int? m_actualVSyncCount;

	private int m_setTargetFrameRate = -1;

	private bool m_setVSyncEnabled;

	private bool m_isXbox;

	private int m_framesForVulkanCrashWorkaround = 1;

	private Resolution m_currentResolution;

	public event Action ResolutionOrRefreshRateChanged;

	public void SetTargetFrameRate(int value)
	{
		m_setTargetFrameRate = ((value < 30 || value > 360) ? (-1) : value);
	}

	public void SetVSyncEnabled(bool value)
	{
		m_setVSyncEnabled = value;
	}

	public bool IsTargetFrameRateEvenlyDivisibleByASupportedFrameRate(uint targetFrameRate)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		foreach (RefreshRate supportedRefreshRate in GetSupportedRefreshRates())
		{
			ZLog.Log((object)$"Checking refresh rate {supportedRefreshRate}");
			if (IsEvenlyDivisible(supportedRefreshRate, targetFrameRate, 0.002f))
			{
				ZLog.Log((object)$"Target frame rate {targetFrameRate} was determined to evenly divisible by refresh rate {supportedRefreshRate} (margin: {0.002f})");
				return true;
			}
		}
		return false;
	}

	public void Update()
	{
		InvokeEventIfResolutionChanged();
		UpdatePresentSettings();
		UpdateTerminalTest();
	}

	public static Resolution GetCurrentPresentResolution()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Resolution currentResolution = Screen.currentResolution;
		((Resolution)(ref currentResolution)).width = Screen.width;
		((Resolution)(ref currentResolution)).height = Screen.height;
		return currentResolution;
	}

	private void InvokeEventIfResolutionChanged()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Resolution currentPresentResolution = GetCurrentPresentResolution();
		if (!((object)(Resolution)(ref m_currentResolution)).Equals((object?)currentPresentResolution))
		{
			m_currentResolution = currentPresentResolution;
			this.ResolutionOrRefreshRateChanged?.Invoke();
		}
	}

	private void UpdatePresentSettings()
	{
		if (m_isXbox)
		{
			UpdatePresentSettingsForXbox();
		}
		else
		{
			UpdatePresentSettingsWithVsyncCount();
		}
	}

	private IReadOnlyCollection<RefreshRate> GetSupportedRefreshRates()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		HashSet<RefreshRate> hashSet = new HashSet<RefreshRate>();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			hashSet.Add(((Resolution)(ref resolutions[i])).refreshRateRatio);
		}
		return hashSet;
	}

	private int GetCurrentFrameRateTarget()
	{
		int num = ((m_setTargetFrameRate < 0) ? int.MaxValue : m_setTargetFrameRate);
		if (Settings.ReduceBackgroundUsage && !Application.isFocused)
		{
			num = Mathf.Min(num, 30);
		}
		if (!Object.op_Implicit((Object)(object)Game.instance) || Game.IsPaused())
		{
			num = Mathf.Min(num, 60);
		}
		if (num >= int.MaxValue)
		{
			num = -1;
		}
		return num;
	}

	private void UpdatePresentSettingsForXbox()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		int currentFrameRateTarget = GetCurrentFrameRateTarget();
		if (m_actualFrameRateLimit != currentFrameRateTarget)
		{
			m_actualFrameRateLimit = currentFrameRateTarget;
			RefreshRate val = default(RefreshRate);
			val.numerator = (uint)m_actualFrameRateLimit.Value;
			val.denominator = 1u;
			RefreshRate val2 = val;
			Resolution currentResolution = Screen.currentResolution;
			int width = ((Resolution)(ref currentResolution)).width;
			currentResolution = Screen.currentResolution;
			Screen.SetResolution(width, ((Resolution)(ref currentResolution)).height, Screen.fullScreenMode, val2);
		}
	}

	private void UpdatePresentSettingsWithVsyncCount()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (m_framesForVulkanCrashWorkaround > 0)
		{
			m_framesForVulkanCrashWorkaround--;
			return;
		}
		int currentFrameRateTarget = GetCurrentFrameRateTarget();
		if (m_setVSyncEnabled)
		{
			if (m_actualFrameRateLimit != -1)
			{
				m_actualFrameRateLimit = -1;
				Application.targetFrameRate = -1;
			}
			Resolution currentResolution = Screen.currentResolution;
			RefreshRate refreshRateRatio = ((Resolution)(ref currentResolution)).refreshRateRatio;
			int num = Mathf.RoundToInt((float)((RefreshRate)(ref refreshRateRatio)).value);
			int num2 = Mathf.Max(1, num / currentFrameRateTarget);
			if (m_actualVSyncCount != num2)
			{
				m_actualVSyncCount = num2;
				QualitySettings.vSyncCount = num2;
			}
		}
		else
		{
			if (m_actualFrameRateLimit != currentFrameRateTarget)
			{
				m_actualFrameRateLimit = currentFrameRateTarget;
				Application.targetFrameRate = currentFrameRateTarget;
			}
			if (m_actualVSyncCount != 0)
			{
				m_actualVSyncCount = 0;
				QualitySettings.vSyncCount = 0;
			}
		}
	}

	private bool IsEvenlyDivisible(int numerator, int denominator)
	{
		return numerator / denominator * denominator == numerator;
	}

	private bool IsEvenlyDivisible(RefreshRate numerator, uint denominator, float percentMargin)
	{
		float num = (float)((RefreshRate)(ref numerator)).value / (float)denominator;
		float num2 = Mathf.Round(num);
		float num3 = num / num2 - 1f;
		if (num3 <= percentMargin)
		{
			return num3 >= 0f - percentMargin;
		}
		return false;
	}

	private void UpdateTerminalTest()
	{
		if (Terminal.m_showTests)
		{
			Terminal.m_testList["fps limit"] = Application.targetFrameRate.ToString();
		}
	}

	internal void Initialize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		m_currentResolution = GetCurrentPresentResolution();
		EnsureCurrentResolutionIsValid();
		if (PlatformManager.DistributionPlatform != null && PlatformManager.DistributionPlatform.HardwareInfoProvider != null)
		{
			HardwareInfo hardwareInfo = PlatformManager.DistributionPlatform.HardwareInfoProvider.HardwareInfo;
			m_isXbox = (int)hardwareInfo.m_category == 2 && hardwareInfo.m_brand == "Xbox";
		}
	}

	private void EnsureCurrentResolutionIsValid()
	{
		if (Settings.IsSteamRunningOnSteamDeck())
		{
			ResetToHighestResolutionIfCurrentIsInvalid();
		}
	}

	private void ResetToHighestResolutionIfCurrentIsInvalid()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions.Length == 0)
		{
			return;
		}
		RefreshRate refreshRateRatio;
		for (int i = 0; i < resolutions.Length; i++)
		{
			ref Resolution reference = ref resolutions[i];
			if (((Resolution)(ref m_currentResolution)).width == ((Resolution)(ref reference)).width && ((Resolution)(ref m_currentResolution)).height == ((Resolution)(ref reference)).height)
			{
				refreshRateRatio = ((Resolution)(ref m_currentResolution)).refreshRateRatio;
				double value = ((RefreshRate)(ref refreshRateRatio)).value;
				refreshRateRatio = ((Resolution)(ref reference)).refreshRateRatio;
				if (value == ((RefreshRate)(ref refreshRateRatio)).value)
				{
					return;
				}
			}
		}
		Resolution val = resolutions[0];
		for (int j = 1; j < resolutions.Length; j++)
		{
			ref Resolution reference2 = ref resolutions[j];
			int num = ((Resolution)(ref val)).width * ((Resolution)(ref val)).height;
			int num2 = ((Resolution)(ref reference2)).width * ((Resolution)(ref reference2)).height;
			if (num >= num2)
			{
				if (num > num2)
				{
					continue;
				}
				refreshRateRatio = ((Resolution)(ref val)).refreshRateRatio;
				double value2 = ((RefreshRate)(ref refreshRateRatio)).value;
				refreshRateRatio = ((Resolution)(ref reference2)).refreshRateRatio;
				double value3 = ((RefreshRate)(ref refreshRateRatio)).value;
				if (value2 >= value3)
				{
					continue;
				}
			}
			val = reference2;
		}
		Screen.SetResolution(((Resolution)(ref val)).width, ((Resolution)(ref val)).height, Screen.fullScreenMode, ((Resolution)(ref val)).refreshRateRatio);
	}
}
