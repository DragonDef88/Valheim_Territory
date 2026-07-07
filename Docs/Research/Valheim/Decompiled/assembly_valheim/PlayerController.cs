using UnityEngine;
using Valheim.SettingsGui;

public class PlayerController : MonoBehaviour
{
	private bool m_run;

	private bool m_runToggled;

	private bool m_lastRunPressed;

	private float m_lastMagnitude;

	private bool m_runPressedWhileStamina = true;

	private static float takeInputDelay = 0f;

	private Player m_character;

	private ZNetView m_nview;

	public static Vector2 cameraDirectionLock = Vector2.zero;

	public static float m_mouseSens = 1f;

	public static float m_gamepadSens = 1f;

	public static bool m_invertMouse = false;

	public static bool m_invertCameraY = false;

	public static bool m_invertCameraX = false;

	public float m_minDodgeTime = 0.2f;

	private bool m_attackWasPressed;

	private bool m_secondAttackWasPressed;

	private bool m_blockWasPressed;

	private bool m_lastJump;

	private bool m_lastCrouch;

	public static bool HasInputDelay => takeInputDelay > 0f;

	private void Awake()
	{
		m_character = ((Component)this).GetComponent<Player>();
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.GetZDO() == null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		if (!PlatformPrefs.HasKey("MouseSensitivity"))
		{
			KeyboardMouseSettings.SetPlatformSpecificFirstTimeSettings();
		}
		m_mouseSens = PlatformPrefs.GetFloat("MouseSensitivity", m_mouseSens);
		m_gamepadSens = PlatformPrefs.GetFloat("GamepadSensitivity", m_gamepadSens);
		m_invertMouse = PlatformPrefs.GetInt("InvertMouse", 0) == 1;
		m_invertCameraY = PlatformPrefs.GetInt("InvertCameraY", m_invertMouse ? 1 : 0) == 1;
		m_invertCameraX = PlatformPrefs.GetInt("InvertCameraX", 0) == 1;
	}

	private void FixedUpdate()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		takeInputDelay = Mathf.Max(0f, takeInputDelay - Time.deltaTime);
		if (Object.op_Implicit((Object)(object)m_nview) && !m_nview.IsOwner())
		{
			return;
		}
		if (!TakeInput())
		{
			m_character.SetControls(Vector3.zero, attack: false, attackHold: false, secondaryAttack: false, secondaryAttackHold: false, block: false, blockHold: false, jump: false, crouch: false, run: false, autoRun: false);
			return;
		}
		bool flag = InInventoryEtc();
		bool flag2 = Hud.IsPieceSelectionVisible();
		bool flag3 = (ZInput.GetButton("SecondaryAttack") || ZInput.GetButton("JoySecondaryAttack")) && !flag && !Hud.InRadial();
		Vector3 zero = Vector3.zero;
		if (ZInput.GetButton("Forward"))
		{
			zero.z += 1f;
		}
		if (ZInput.GetButton("Backward"))
		{
			zero.z -= 1f;
		}
		if (ZInput.GetButton("Left"))
		{
			zero.x -= 1f;
		}
		if (ZInput.GetButton("Right"))
		{
			zero.x += 1f;
		}
		zero.x += ZInput.GetJoyLeftStickX(false);
		zero.z += 0f - ZInput.GetJoyLeftStickY(true);
		if (((Vector3)(ref zero)).magnitude > 1f)
		{
			((Vector3)(ref zero)).Normalize();
		}
		bool flag4 = (ZInput.GetButton("Attack") || ZInput.GetButton("JoyAttack")) && !flag && !Hud.InRadial();
		bool attackHold = flag4;
		bool attack = flag4 && !m_attackWasPressed;
		m_attackWasPressed = flag4;
		bool secondaryAttackHold = flag3;
		bool secondaryAttack = flag3 && !m_secondAttackWasPressed;
		m_secondAttackWasPressed = flag3;
		bool flag5 = (ZInput.GetButton("Block") || ZInput.GetButton("JoyBlock")) && !flag && !Hud.InRadial();
		bool blockHold = flag5;
		bool block = flag5 && !m_blockWasPressed;
		m_blockWasPressed = flag5;
		bool button = ZInput.GetButton("Jump");
		bool jump = (button && !m_lastJump) || (ZInput.GetButtonDown("JoyJump") && !flag2 && !flag && !Hud.InRadial());
		m_lastJump = button;
		bool dodge = ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive() && ZInput.GetButtonDown("JoyDodge") && !flag && !Hud.InRadial();
		bool flag6 = InventoryGui.IsVisible();
		bool flag7 = (ZInput.GetButton("Crouch") || ZInput.GetButton("JoyCrouch")) && !flag6 && !Hud.InRadial();
		bool crouch = flag7 && !m_lastCrouch;
		m_lastCrouch = flag7;
		bool flag8 = ZInput.GetButton("Run") || ZInput.GetButton("JoyRun");
		if (!m_lastRunPressed && flag8 && m_character.GetStamina() > 0f)
		{
			m_runPressedWhileStamina = true;
		}
		else if (m_character.GetStamina() <= 0f)
		{
			m_runPressedWhileStamina = false;
		}
		if (ZInput.ToggleRun)
		{
			if (!m_lastRunPressed && flag8)
			{
				m_run = !m_run;
			}
			if (m_character.GetStamina() <= 0f)
			{
				m_run = false;
			}
		}
		else
		{
			m_run = flag8 && m_runPressedWhileStamina;
		}
		float magnitude = ((Vector3)(ref zero)).magnitude;
		if (magnitude < 0.05f && m_lastMagnitude < 0.05f && !m_character.m_autoRun)
		{
			m_run = false;
		}
		m_lastRunPressed = flag8;
		m_lastMagnitude = magnitude;
		m_lastRunPressed = flag8;
		bool button2 = ZInput.GetButton("AutoRun");
		if (takeInputDelay > 0f)
		{
			m_character.SetControls(zero, attack: false, attackHold: false, secondaryAttack: false, secondaryAttackHold: false, block: false, blockHold: false, jump: false, crouch: false, m_run, button2);
		}
		else
		{
			m_character.SetControls(zero, attack, attackHold, secondaryAttack, secondaryAttackHold, block, blockHold, jump, crouch, m_run, button2, dodge);
		}
	}

	private static bool DetectTap(bool pressed, float dt, float minPressTime, bool run, ref float pressTimer, ref float releasedTimer, ref bool tapPressed)
	{
		bool result = false;
		if (pressed)
		{
			if ((releasedTimer > 0f && releasedTimer < minPressTime) & tapPressed)
			{
				tapPressed = false;
				result = true;
			}
			pressTimer += dt;
			releasedTimer = 0f;
		}
		else
		{
			if (pressTimer > 0f)
			{
				tapPressed = pressTimer < minPressTime;
				if (run & tapPressed)
				{
					tapPressed = false;
					result = true;
				}
			}
			releasedTimer += dt;
			pressTimer = 0f;
		}
		return result;
	}

	private bool TakeInput(bool look = false)
	{
		if (GameCamera.InFreeFly())
		{
			return false;
		}
		if ((!Object.op_Implicit((Object)(object)Chat.instance) || !Chat.instance.HasFocus()) && !Menu.IsVisible() && !Console.IsVisible() && !TextInput.IsVisible() && !Minimap.InTextInput() && (!ZInput.IsGamepadActive() || !Minimap.IsOpen()) && (!ZInput.IsGamepadActive() || !InventoryGui.IsVisible()) && (!ZInput.IsGamepadActive() || !StoreGui.IsVisible()) && (!ZInput.IsGamepadActive() || !Hud.IsPieceSelectionVisible()))
		{
			if (!PlayerCustomizaton.IsBarberGuiVisible() || look)
			{
				return !(PlayerCustomizaton.BarberBlocksLook() && look);
			}
			return false;
		}
		return false;
	}

	private bool InInventoryEtc()
	{
		if (!InventoryGui.IsVisible() && !Minimap.IsOpen() && !StoreGui.IsVisible())
		{
			return Hud.IsPieceSelectionVisible();
		}
		return true;
	}

	public static void SetTakeInputDelay(float delayInSeconds)
	{
		takeInputDelay = delayInSeconds;
	}

	private void LateUpdate()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if ((Hud.InRadial() && Hud.instance.m_radialMenu.IsBlockingInput) || (takeInputDelay > 0f && ZInput.IsGamepadActive()))
		{
			return;
		}
		if (!TakeInput(look: true) || InInventoryEtc())
		{
			m_character.SetMouseLook(Vector2.zero);
			return;
		}
		Vector2 mouseLook = Vector2.zero;
		if (ZInput.IsGamepadActive() && !Hud.InRadial())
		{
			if (!m_character.InPlaceMode() || !ZInput.GetButton("JoyRotate"))
			{
				if (!((Vector2)(ref cameraDirectionLock)).Equals(Vector2.zero))
				{
					Vector2 val = default(Vector2);
					((Vector2)(ref val))._002Ector(ZInput.GetJoyRightStickX(true), 0f - ZInput.GetJoyRightStickY(true));
					if (!((Vector2)(ref val)).Equals(cameraDirectionLock))
					{
						cameraDirectionLock = Vector2.zero;
						mouseLook.x += ZInput.GetJoyRightStickX(true) * 110f * Time.deltaTime * m_gamepadSens;
						mouseLook.y += (0f - ZInput.GetJoyRightStickY(true)) * 110f * Time.deltaTime * m_gamepadSens;
					}
				}
				else
				{
					mouseLook.x += ZInput.GetJoyRightStickX(true) * 110f * Time.deltaTime * m_gamepadSens;
					mouseLook.y += (0f - ZInput.GetJoyRightStickY(true)) * 110f * Time.deltaTime * m_gamepadSens;
				}
			}
			if (m_invertCameraX)
			{
				mouseLook.x *= -1f;
			}
			if (m_invertCameraY)
			{
				mouseLook.y *= -1f;
			}
		}
		else if (!Hud.InRadial())
		{
			mouseLook = ZInput.GetMouseDelta() * m_mouseSens;
			if (m_invertMouse)
			{
				mouseLook.y *= -1f;
			}
		}
		m_character.SetMouseLook(mouseLook);
	}
}
