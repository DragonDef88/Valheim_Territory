using System;
using UnityEngine;

namespace Valheim.SettingsGui;

public class GamepadMapController : MonoBehaviour
{
	[SerializeField]
	private GamepadMap xboxMapPrefab;

	[SerializeField]
	private GamepadMap psMapPrefab;

	[SerializeField]
	private GamepadMap steamDeckXboxMapPrefab;

	[SerializeField]
	private GamepadMap steamDeckPSMapPrefab;

	[SerializeField]
	private RectTransform root;

	private GamepadMap xboxMapInstance;

	private GamepadMap psMapInstance;

	private GamepadMap steamDeckXboxMapInstance;

	private GamepadMap steamDeckPSMapInstance;

	private GamepadMapType visibleType;

	private InputLayout visibleLayout;

	public InputLayout VisibleLayout => visibleLayout;

	private void Start()
	{
		Localization.OnLanguageChange = (Action)Delegate.Combine(Localization.OnLanguageChange, new Action(OnLanguageChange));
	}

	private void OnDestroy()
	{
		Localization.OnLanguageChange = (Action)Delegate.Remove(Localization.OnLanguageChange, new Action(OnLanguageChange));
	}

	public void Show(InputLayout layout, GamepadMapType type = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		visibleType = type;
		visibleLayout = layout;
		switch ((int)type)
		{
		case 1:
			if ((Object)(object)psMapInstance == (Object)null)
			{
				psMapInstance = Object.Instantiate<GamepadMap>(psMapPrefab, (Transform)(object)root);
			}
			break;
		case 2:
			if ((Object)(object)steamDeckXboxMapInstance == (Object)null)
			{
				steamDeckXboxMapInstance = Object.Instantiate<GamepadMap>(steamDeckXboxMapPrefab, (Transform)(object)root);
			}
			break;
		case 3:
			if ((Object)(object)steamDeckPSMapInstance == (Object)null)
			{
				steamDeckPSMapInstance = Object.Instantiate<GamepadMap>(steamDeckPSMapPrefab, (Transform)(object)root);
			}
			break;
		default:
			if ((Object)(object)xboxMapInstance == (Object)null)
			{
				xboxMapInstance = Object.Instantiate<GamepadMap>(xboxMapPrefab, (Transform)(object)root);
			}
			break;
		}
		UpdateGamepadMap();
	}

	private void OnLanguageChange()
	{
		UpdateGamepadMap();
	}

	private void UpdateGamepadMap()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Invalid comparison between Unknown and I4
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Invalid comparison between Unknown and I4
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Invalid comparison between Unknown and I4
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)psMapInstance != (Object)null)
		{
			((Component)psMapInstance).gameObject.SetActive((int)visibleType == 1);
			if ((int)visibleType == 1)
			{
				psMapInstance.UpdateMap(visibleLayout);
			}
		}
		if ((Object)(object)steamDeckXboxMapInstance != (Object)null)
		{
			((Component)steamDeckXboxMapInstance).gameObject.SetActive((int)visibleType == 2);
			if ((int)visibleType == 2)
			{
				steamDeckXboxMapInstance.UpdateMap(visibleLayout);
			}
		}
		if ((Object)(object)steamDeckPSMapInstance != (Object)null)
		{
			((Component)steamDeckPSMapInstance).gameObject.SetActive((int)visibleType == 3);
			if ((int)visibleType == 3)
			{
				steamDeckPSMapInstance.UpdateMap(visibleLayout);
			}
		}
		if ((Object)(object)xboxMapInstance != (Object)null)
		{
			((Component)xboxMapInstance).gameObject.SetActive((int)visibleType == 0);
			if ((int)visibleType == 0)
			{
				xboxMapInstance.UpdateMap(visibleLayout);
			}
		}
	}

	public static string GetLayoutStringId(InputLayout layout)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		return (int)layout switch
		{
			0 => "$settings_controller_classic", 
			2 => "$settings_controller_default 2", 
			_ => "$settings_controller_default", 
		};
	}

	public static InputLayout NextLayout(InputLayout mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (mode + 1 < 3)
		{
			return (InputLayout)(mode + 1);
		}
		return (InputLayout)0;
	}

	public static InputLayout PrevLayout(InputLayout mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (mode - 1 >= 0)
		{
			return (InputLayout)(mode - 1);
		}
		return (InputLayout)2;
	}

	public static GamepadMapType GetType(GamepadGlyphs currentGlyphs = 0, bool steamDeck = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)currentGlyphs == 0)
		{
			currentGlyphs = ZInput.ConnectedGamepadTypeGlyphs();
		}
		GamepadGlyphs val = currentGlyphs;
		if ((int)val != 1)
		{
			if ((int)val == 2)
			{
				if (steamDeck)
				{
					return (GamepadMapType)3;
				}
				return (GamepadMapType)1;
			}
			return (GamepadMapType)0;
		}
		if (steamDeck)
		{
			return (GamepadMapType)2;
		}
		return (GamepadMapType)0;
	}
}
