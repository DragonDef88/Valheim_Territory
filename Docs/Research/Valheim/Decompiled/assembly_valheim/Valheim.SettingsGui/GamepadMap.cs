using TMPro;
using UnityEngine;

namespace Valheim.SettingsGui;

public class GamepadMap : MonoBehaviour
{
	[Header("Face Buttons")]
	[SerializeField]
	private GamepadMapLabel joyButton0;

	[SerializeField]
	private GamepadMapLabel joyButton1;

	[SerializeField]
	private GamepadMapLabel joyButton2;

	[SerializeField]
	private GamepadMapLabel joyButton3;

	[Header("Bumpers")]
	[SerializeField]
	private GamepadMapLabel joyButton4;

	[SerializeField]
	private GamepadMapLabel joyButton5;

	[Header("Center")]
	[SerializeField]
	private GamepadMapLabel joyButton6;

	[SerializeField]
	private GamepadMapLabel joyButton7;

	[Header("Triggers")]
	[SerializeField]
	private GamepadMapLabel joyAxis9;

	[SerializeField]
	private GamepadMapLabel joyAxis10;

	[SerializeField]
	private GamepadMapLabel joyAxis9And10;

	[Header("Sticks")]
	[SerializeField]
	private GamepadMapLabel joyButton8;

	[SerializeField]
	private GamepadMapLabel joyButton9;

	[SerializeField]
	private GamepadMapLabel joyAxis1And2;

	[SerializeField]
	private GamepadMapLabel joyAxis4And5;

	[Header("Dpad")]
	[SerializeField]
	private GamepadMapLabel joyAxis6And7;

	[SerializeField]
	private GamepadMapLabel joyAxis6Left;

	[SerializeField]
	private GamepadMapLabel joyAxis6Right;

	[SerializeField]
	private GamepadMapLabel joyAxis6LeftRight;

	[SerializeField]
	private GamepadMapLabel joyAxis7Up;

	[SerializeField]
	private GamepadMapLabel joyAxis7Down;

	[SerializeField]
	private TextMeshProUGUI alternateButtonLabel;

	public void UpdateMap(InputLayout layout)
	{
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Invalid comparison between Unknown and I4
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Invalid comparison between Unknown and I4
		((TMP_Text)joyButton0.Label).text = GetText((GamepadInput)5);
		((TMP_Text)joyButton1.Label).text = GetText((GamepadInput)6);
		((TMP_Text)joyButton2.Label).text = GetText((GamepadInput)7);
		((TMP_Text)joyButton3.Label).text = GetText((GamepadInput)8);
		((TMP_Text)joyButton4.Label).text = GetText((GamepadInput)15);
		((TMP_Text)joyButton5.Label).text = GetText((GamepadInput)16);
		((TMP_Text)joyButton6.Label).text = GetText((GamepadInput)19);
		((TMP_Text)joyButton7.Label).text = GetText((GamepadInput)20);
		((TMP_Text)joyAxis9.Label).text = GetText((GamepadInput)17);
		((TMP_Text)joyAxis10.Label).text = GetText((GamepadInput)18);
		((Component)joyAxis9And10).gameObject.SetActive((int)layout == 1 || (int)layout == 2);
		((TMP_Text)joyAxis9And10.Label).text = Localization.instance.Localize("$settings_gp");
		((TMP_Text)joyAxis1And2.Label).text = Localization.instance.Localize("$settings_move");
		((TMP_Text)joyAxis4And5.Label).text = Localization.instance.Localize("$settings_look");
		((TMP_Text)joyButton8.Label).text = GetText((GamepadInput)11);
		((TMP_Text)joyButton9.Label).text = GetText((GamepadInput)14);
		((TMP_Text)joyAxis6LeftRight.Label).text = GetText((GamepadInput)2);
		((TMP_Text)joyAxis7Up.Label).text = GetText((GamepadInput)4);
		((TMP_Text)joyAxis7Down.Label).text = GetText((GamepadInput)3);
		((TMP_Text)alternateButtonLabel).text = Localization.instance.Localize("$alternate_key_label ") + ZInput.instance.GetBoundKeyString("JoyAltKeys", false);
	}

	private static string GetText(GamepadInput gamepadInput, FloatRange? floatRange = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		string boundActionString = ZInput.instance.GetBoundActionString(gamepadInput, floatRange);
		return Localization.instance.Localize(boundActionString);
	}

	private static string GetText(KeyCode keyboardKey)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		string boundActionString = ZInput.instance.GetBoundActionString(keyboardKey);
		return Localization.instance.Localize(boundActionString);
	}
}
