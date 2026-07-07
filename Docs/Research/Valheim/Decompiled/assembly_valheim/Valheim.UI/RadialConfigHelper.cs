using UnityEngine;

namespace Valheim.UI;

public static class RadialConfigHelper
{
	public static void SetXYControls(this RadialBase radial)
	{
		radial.GetControllerDirection = () => ZInput.GetValue<Vector2>("RadialStick");
		radial.GetMouseDirection = () => GetMouseDirection(radial);
	}

	private static Vector2 GetMouseDirection(RadialBase radial)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(ZInput.mousePosition);
		Vector2 infoPosition = radial.InfoPosition;
		Vector2 val2 = val - infoPosition;
		return ((Vector2)(ref val2)).normalized;
	}

	public static void SetItemInteractionControls(this RadialBase radial)
	{
		radial.GetConfirm = () => (ZInput.GetButtonLastPressedTimer("JoyRadialInteract") < 0.33f && ZInput.GetButtonUp("JoyRadialInteract")) || ZInput.GetMouseButtonUp(0);
		radial.GetReleaseToUse = delegate
		{
			if (!RadialData.SO.EnableReleaseToUseMode)
			{
				return false;
			}
			return (ZInput.GetButtonLastPressedTimer("JoyRadial") > RadialData.SO.HoldCloseDelay && ZInput.GetButtonUp("JoyRadial")) || (ZInput.GetButtonLastPressedTimer("OpenRadial") > RadialData.SO.HoldCloseDelay && ZInput.GetButtonUp("OpenRadial")) || (ZInput.GetButtonLastPressedTimer("OpenEmote") > RadialData.SO.HoldCloseDelay && ZInput.GetButtonUp("OpenEmote"));
		};
		radial.GetBack = () => ZInput.GetButtonUp("JoyRadialBack") || (ZInput.GetButtonUp("JoyRadialClose") && !radial.IsTopLevel) || (!ZInput.GetKey((KeyCode)304, true) && ZInput.GetMouseButtonUp(1));
		radial.GetThrow = () => (ZInput.GetButtonLastPressedTimer("JoyRadialSecondaryInteract") < 0.33f && ZInput.GetButtonUp("JoyRadialSecondaryInteract")) || (ZInput.GetKey((KeyCode)304, true) && ZInput.GetButtonLastPressedTimer("RadialSecondaryInteract") < 0.33f && ZInput.GetButtonUp("RadialSecondaryInteract"));
		radial.GetOpenThrowMenu = () => (ZInput.GetButtonPressedTimer("JoyRadialSecondaryInteract") > 0.33f && ZInput.GetButton("JoyRadialSecondaryInteract")) || (ZInput.GetKey((KeyCode)304, true) && ZInput.GetButtonPressedTimer("RadialSecondaryInteract") > 0.33f && ZInput.GetButton("RadialSecondaryInteract"));
		radial.GetClose = () => ((ZInput.GetButtonUp("JoyRadialClose") && radial.IsTopLevel) || ZInput.GetButtonDown("JoyRadial") || ZInput.GetKeyDown((KeyCode)27, true) || ZInput.GetKeyDown((KeyCode)96, true) || ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyMap") || ZInput.GetButtonDown("Map") || ZInput.GetButtonDown("JoyChat") || ZInput.GetButtonDown("Chat") || ZInput.GetButtonDown("Console") || ZInput.GetButtonDown("OpenEmote")) ? true : false;
		radial.GetFlick = () => ZInput.GetRadialTap();
		radial.GetDoubleTap = () => ZInput.GetRadialMultiTap();
		ZInput.UpdateRadialMultiTap(RadialData.SO.DoubleClickTime, RadialData.SO.DoubleClickDelay, 2, RadialData.SO.RequireReleaseOnFinalClick);
		ZInput.UpdateRadialTapTime(RadialData.SO.FlickTime);
	}
}
