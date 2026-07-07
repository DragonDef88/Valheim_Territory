using System.Text;
using UnityEngine;

public class StartupMessages : MonoBehaviour
{
	private static StartupMessages s_instance;

	private uint m_shownMessages;

	public static StartupMessages Instance => s_instance;

	public bool StartupMessageDisplayed => m_shownMessages != 0;

	private void Awake()
	{
		if (s_instance != null)
		{
			ZLog.LogError((object)"StartupMessages already had instance!");
			Object.DestroyImmediate((Object)(object)this);
		}
		else
		{
			s_instance = this;
		}
	}

	private void OnDestroy()
	{
		if (s_instance == null)
		{
			ZLog.LogWarning((object)"StartupMessages had no instance!");
		}
		else if ((Object)(object)s_instance != (Object)(object)this)
		{
			ZLog.LogWarning((object)"StartupMessages had a different instance!");
		}
		else
		{
			s_instance = null;
		}
	}

	public void DisplayStartupMessages()
	{
		PrintGPUInfo();
		DisplayWindowsVulkanAMDCrashMessage();
	}

	private void DisplayWindowsVulkanAMDCrashMessage()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if (GetGPUVendor() == GPUVendor.AMD && (int)SystemInfo.operatingSystemFamily == 2 && (int)SystemInfo.graphicsDeviceType == 21)
		{
			m_shownMessages++;
			UnifiedPopup.Push(new WarningPopup("$menu_vulkancrashwarning_header", "$menu_vulkancrashwarning_text", delegate
			{
				UnifiedPopup.Pop();
				m_shownMessages--;
			}));
		}
	}

	private GPUVendor GetGPUVendor()
	{
		switch (SystemInfo.graphicsDeviceVendorID)
		{
		case 4318:
			return GPUVendor.NVIDIA;
		case 4098:
		case 4130:
			return GPUVendor.AMD;
		case 32902:
			return GPUVendor.Intel;
		default:
			return GPUVendor.Unknown;
		}
	}

	private void PrintGPUInfo()
	{
		string text = null;
		string text2;
		switch (SystemInfo.graphicsDeviceVendorID)
		{
		case 4318:
			text2 = "NVIDIA";
			break;
		case 4098:
		case 4130:
			switch (SystemInfo.graphicsDeviceID)
			{
			case 30032:
			case 30033:
			case 30096:
				text = "RDNA4";
				break;
			case 28639:
				text = "GCN4";
				break;
			}
			text2 = "AMD";
			break;
		case 32902:
			text2 = "Intel";
			break;
		default:
			text2 = "Unknown";
			break;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("GPU Device: " + SystemInfo.graphicsDeviceVendorID.ToString("X4") + ":" + SystemInfo.graphicsDeviceID.ToString("X4") + " (" + text2);
		if (text != null)
		{
			stringBuilder.Append(", " + text);
		}
		stringBuilder.Append(")");
		ZLog.Log((object)stringBuilder.ToString());
	}
}
