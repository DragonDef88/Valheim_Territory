using System;
using System.Collections.Generic;
using System.Linq;
using Splatform;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XInput;
using Valheim.SettingsGui;

public class ZInput
{
	[Flags]
	public enum InputSource
	{
		None = 0,
		KeyboardMouse = 0xAA,
		Gamepad = 0xB4,
		AutomaticBlocking = 0xDF,
		AutomaticNonBlocking = 0xDE,
		KeyboardMouseOnly = 0xCB,
		GamepadOnly = 0xD5,
		BothKeyboardMouseHints = 0xCE,
		BothGamepadHints = 0xD6,
		BlockingBit = 1,
		AllowKBMInputBit = 2,
		AllowGamepadInputBit = 4,
		AllowKBMHintsBit = 8,
		AllowGamepadHintsBit = 0x10,
		InputSourceBit = 0x20,
		InputSwitchingModeBit = 0x40,
		ValidBit = 0x80,
		AllowedInputMask = 6,
		AllowedHintsMask = 0x18
	}

	public class ButtonDef
	{
		public string Name;

		private bool m_heldDynamic;

		private bool m_heldFixed;

		private bool m_pressedDynamic;

		private bool m_pressedFixed;

		private bool m_wasPressedDynamic;

		private bool m_wasPressedFixed;

		private bool m_releasedDynamic;

		private bool m_releasedFixed;

		private float m_repeatDelay;

		private float m_repeatInterval;

		public InputAction ButtonAction { get; }

		public InputSource Source { get; private set; }

		public bool Rebindable { get; private set; }

		public bool ShowHints { get; private set; }

		public bool AltKey { get; private set; }

		public bool Held
		{
			get
			{
				if (!(Time.inFixedTimeStep ? m_heldFixed : m_heldDynamic))
				{
					if (Pressed)
					{
						return !Released;
					}
					return false;
				}
				return true;
			}
		}

		public bool Pressed
		{
			get
			{
				if (!Time.inFixedTimeStep)
				{
					return m_pressedDynamic;
				}
				return m_pressedFixed;
			}
		}

		public bool Released
		{
			get
			{
				if (!Time.inFixedTimeStep)
				{
					return m_releasedDynamic;
				}
				return m_releasedFixed;
			}
		}

		public float PressedTimer { get; private set; }

		public float LastPressedTimer { get; private set; }

		public ButtonDef(string name, string path, bool altKey = false, bool showHints = true, bool rebindable = false, float repeatDelay = -1f, float repeatInterval = -1f, string pressPoint = "0.4")
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			Name = name;
			ButtonAction = new InputAction(name, (InputActionType)2, path, "press(pressPoint=" + pressPoint + ")", (string)null, (string)null);
			ButtonAction.Enable();
			Source = (path.Contains("Gamepad") ? InputSource.Gamepad : InputSource.KeyboardMouse);
			AltKey = altKey;
			ShowHints = showHints;
			m_repeatDelay = repeatDelay;
			m_repeatInterval = repeatInterval;
			Rebindable = rebindable;
		}

		public void Tick(float deltaTime)
		{
			if (Time.inFixedTimeStep)
			{
				UpdatePressStates(ref m_heldFixed, ref m_pressedFixed, ref m_wasPressedFixed, ref m_releasedFixed);
				return;
			}
			if (m_heldDynamic)
			{
				PressedTimer += deltaTime;
			}
			UpdatePressStates(ref m_heldDynamic, ref m_pressedDynamic, ref m_wasPressedDynamic, ref m_releasedDynamic);
			if (m_repeatDelay > 0f && !m_releasedDynamic && PressedTimer > m_repeatDelay)
			{
				RePress();
			}
		}

		private void UpdatePressStates(ref bool held, ref bool pressed, ref bool wasPressed, ref bool released)
		{
			pressed = !wasPressed & held;
			released = wasPressed && !held;
			wasPressed = held;
		}

		public void Press()
		{
			m_wasPressedDynamic = (m_wasPressedFixed = false);
			m_heldDynamic = (m_heldFixed = true);
			PressedTimer = 0f;
		}

		private void RePress()
		{
			if (!Released)
			{
				m_wasPressedDynamic = (m_wasPressedFixed = false);
				PressedTimer -= m_repeatInterval;
			}
		}

		public void Release()
		{
			m_heldDynamic = (m_heldFixed = false);
			LastPressedTimer = PressedTimer;
			PressedTimer = 0f;
		}

		public void ResetState()
		{
			m_wasPressedDynamic = (m_wasPressedFixed = false);
			m_releasedDynamic = (m_releasedFixed = false);
			m_pressedDynamic = (m_pressedFixed = false);
			m_heldDynamic = (m_heldFixed = false);
			float pressedTimer = (LastPressedTimer = 0f);
			PressedTimer = pressedTimer;
		}

		public void Rebind(string newPath)
		{
			ButtonAction.Disable();
			InputActionRebindingExtensions.RemoveAllBindingOverrides(ButtonAction);
			InputActionRebindingExtensions.ApplyBindingOverride(ButtonAction, newPath, (string)null, (string)null);
			ButtonAction.Enable();
		}

		public void ResetBinding()
		{
			ButtonAction.Disable();
			InputActionRebindingExtensions.RemoveAllBindingOverrides(ButtonAction);
			ButtonAction.Enable();
		}

		public string GetActionPath(bool effective = true)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			InputBinding val = ButtonAction.bindings[0];
			if (val == default(InputBinding))
			{
				return null;
			}
			if (!effective)
			{
				return ((InputBinding)(ref val)).path;
			}
			return ((InputBinding)(ref val)).effectivePath;
		}
	}

	public class ValueDef
	{
		public InputAction ValueAction;

		public string Name { get; private set; }

		public ValueDef(string name, string path, string interactions = null, string processors = null)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			Name = name;
			ValueAction = new InputAction(name, (InputActionType)2, path, interactions, processors, (string)null);
			ValueAction.Enable();
		}

		public T GetValue<T>() where T : struct
		{
			return ValueAction.ReadValue<T>();
		}

		public T GetValueRaw<T>() where T : struct
		{
			if (!(ValueAction.activeControl is InputControl<T> val))
			{
				return GetValue<T>();
			}
			return val.ReadUnprocessedValue();
		}
	}

	private static ZInput m_instance;

	private bool m_systemUpdated;

	private bool m_mouseInputThisFrame;

	private static bool _workaroundEnabled = false;

	private static bool m_virtualKeyboardOpen;

	public PlatformGamepadDefinition m_definition;

	public static InputLayout InputLayout = InputLayout.Default;

	public static GamepadGlyphs CurrentGlyph;

	public static Func<ZInput> GetZInput;

	public static bool SwapTriggers = false;

	public static bool ToggleRun = false;

	public static bool ToggleRunState = false;

	private const float m_stickDeadZone = 0.2f;

	private const float m_axisPressDeadZone = 0.4f;

	private const float m_gamepadInactiveTimeout = 60f;

	private const float c_repeatDelay = 0.3f;

	private const float c_repeatInterval = 0.1f;

	private const string m_invertedText = "_inverted";

	private const string ControllerLayout = "ControllerLayout";

	private DateTime m_inputTimer = DateTime.Now;

	private DateTime m_inputTimerGamepad = DateTime.Now;

	private DateTime m_inputTimerMouse = DateTime.Now;

	private InputAction m_mouseDelta;

	private InputAction m_mousePosition;

	private InputAction m_mouseScrollDeltaAction;

	private InputAction m_radialMultiTap;

	private InputAction m_radialTap;

	private static GamepadType m_latestConnectedGamepadType = GamepadType.None;

	private static InputSource s_inputSwitchingMode = InputSource.AutomaticNonBlocking;

	private static InputSource m_inputSource = InputSource.KeyboardMouse;

	private static bool? s_isInputSwitchingModeValid = null;

	private static float m_blockGamePadInput;

	private static int m_ignoreMouseInputFrames = 0;

	private Dictionary<string, ButtonDef> m_buttons = new Dictionary<string, ButtonDef>();

	private Dictionary<string, ValueDef> m_values = new Dictionary<string, ValueDef>();

	private static readonly List<Key> s_keyCodeValues = Enum.GetValues(typeof(Key)).OfType<Key>().ToList();

	private static readonly Dictionary<string, Dictionary<string, string>> s_gamepadSpriteMap = new Dictionary<string, Dictionary<string, string>>
	{
		{
			"buttonSouth",
			new Dictionary<string, string>
			{
				{ "xbox", "button_a" },
				{ "ps5", "button_cross" }
			}
		},
		{
			"buttonEast",
			new Dictionary<string, string>
			{
				{ "xbox", "button_b" },
				{ "ps5", "button_circle" }
			}
		},
		{
			"buttonWest",
			new Dictionary<string, string>
			{
				{ "xbox", "button_x" },
				{ "ps5", "button_square" }
			}
		},
		{
			"buttonNorth",
			new Dictionary<string, string>
			{
				{ "xbox", "button_y" },
				{ "ps5", "button_triangle" }
			}
		},
		{
			"leftShoulder",
			new Dictionary<string, string>
			{
				{ "xbox", "button_lb" },
				{ "ps5", "button_l1" }
			}
		},
		{
			"rightShoulder",
			new Dictionary<string, string>
			{
				{ "xbox", "button_rb" },
				{ "ps5", "button_r1" }
			}
		},
		{
			"select",
			new Dictionary<string, string>
			{
				{ "xbox", "button_back" },
				{ "ps5", "button_share" }
			}
		},
		{
			"start",
			new Dictionary<string, string>
			{
				{ "xbox", "button_start" },
				{ "ps5", "button_options" }
			}
		},
		{
			"leftStickPress",
			new Dictionary<string, string>
			{
				{ "xbox", "button_ls" },
				{ "ps5", "button_l3" }
			}
		},
		{
			"rightStickPress",
			new Dictionary<string, string>
			{
				{ "xbox", "button_rs" },
				{ "ps5", "button_r3" }
			}
		},
		{
			"leftStick_left",
			new Dictionary<string, string>
			{
				{ "xbox", "lstick_left" },
				{ "ps5", "lstick_left" }
			}
		},
		{
			"leftStick_right",
			new Dictionary<string, string>
			{
				{ "xbox", "lstick_right" },
				{ "ps5", "lstick_right" }
			}
		},
		{
			"leftStick_up",
			new Dictionary<string, string>
			{
				{ "xbox", "lstick_up" },
				{ "ps5", "lstick_up" }
			}
		},
		{
			"leftStick_down",
			new Dictionary<string, string>
			{
				{ "xbox", "lstick_down" },
				{ "ps5", "lstick_down" }
			}
		},
		{
			"leftTrigger",
			new Dictionary<string, string>
			{
				{ "xbox", "button_lt" },
				{ "ps5", "button_l2" }
			}
		},
		{
			"rightTrigger",
			new Dictionary<string, string>
			{
				{ "xbox", "button_rt" },
				{ "ps5", "button_r2" }
			}
		},
		{
			"dpad_left",
			new Dictionary<string, string>
			{
				{ "xbox", "dpad_left" },
				{ "ps5", "dpad_left" }
			}
		},
		{
			"dpad_right",
			new Dictionary<string, string>
			{
				{ "xbox", "dpad_right" },
				{ "ps5", "dpad_right" }
			}
		},
		{
			"dpad_down",
			new Dictionary<string, string>
			{
				{ "xbox", "dpad_down" },
				{ "ps5", "dpad_down" }
			}
		},
		{
			"dpad_up",
			new Dictionary<string, string>
			{
				{ "xbox", "dpad_up" },
				{ "ps5", "dpad_up" }
			}
		},
		{
			"dpad",
			new Dictionary<string, string>
			{
				{ "xbox", "dpad" },
				{ "ps5", "dpad" }
			}
		},
		{
			"leftStick",
			new Dictionary<string, string>
			{
				{ "xbox", "lstick" },
				{ "ps5", "lstick" }
			}
		},
		{
			"rightStick",
			new Dictionary<string, string>
			{
				{ "xbox", "rstick" },
				{ "ps5", "rstick" }
			}
		}
	};

	private static readonly Dictionary<string, string> s_keyLocalizationMap = new Dictionary<string, string>
	{
		{ "comma", "," },
		{ "period", "." },
		{ "leftButton", "$button_mouse0" },
		{ "rightButton", "$button_mouse1" },
		{ "middleButton", "$button_mouse2" },
		{ "space", "$button_space" },
		{ "shift", "$button_lshift" },
		{ "leftShift", "$button_lshift" },
		{ "rightShift", "$button_rshift" },
		{ "alt", "$button_lalt" },
		{ "leftAlt", "$button_lalt" },
		{ "rightAlt", "$button_ralt" },
		{ "ctrl", "$button_lctrl" },
		{ "leftCtrl", "$button_lctrl" },
		{ "rightCtrl", "$button_rctrl" },
		{ "enter", "$button_return" }
	};

	private static readonly Dictionary<GamepadInput, string> s_gamepadInputPathMap = new Dictionary<GamepadInput, string>
	{
		{
			GamepadInput.None,
			""
		},
		{
			GamepadInput.DPadLeft,
			"<Gamepad>/dpad/left"
		},
		{
			GamepadInput.DPadRight,
			"<Gamepad>/dpad/right"
		},
		{
			GamepadInput.DPadDown,
			"<Gamepad>/dpad/down"
		},
		{
			GamepadInput.DPadUp,
			"<Gamepad>/dpad/up"
		},
		{
			GamepadInput.FaceButtonA,
			"<Gamepad>/buttonSouth"
		},
		{
			GamepadInput.FaceButtonB,
			"<Gamepad>/buttonEast"
		},
		{
			GamepadInput.FaceButtonX,
			"<Gamepad>/buttonWest"
		},
		{
			GamepadInput.FaceButtonY,
			"<Gamepad>/buttonNorth"
		},
		{
			GamepadInput.StickLHorizontal,
			"<Gamepad>/leftStick/x"
		},
		{
			GamepadInput.StickLVertical,
			"<Gamepad>/leftStick/y"
		},
		{
			GamepadInput.StickLButton,
			"<Gamepad>/leftStickPress"
		},
		{
			GamepadInput.StickRHorizontal,
			"<Gamepad>/rightStick/x"
		},
		{
			GamepadInput.StickRVertical,
			"<Gamepad>/rightStick/y"
		},
		{
			GamepadInput.StickRButton,
			"<Gamepad>/rightStickPress"
		},
		{
			GamepadInput.BumperL,
			"<Gamepad>/leftShoulder"
		},
		{
			GamepadInput.BumperR,
			"<Gamepad>/rightShoulder"
		},
		{
			GamepadInput.TriggerL,
			"<Gamepad>/leftTrigger"
		},
		{
			GamepadInput.TriggerR,
			"<Gamepad>/rightTrigger"
		},
		{
			GamepadInput.Select,
			"<Gamepad>/select"
		},
		{
			GamepadInput.Start,
			"<Gamepad>/start"
		},
		{
			GamepadInput.DualShockTouchpad,
			"<DualShockGamepad>/touchpadButton"
		},
		{
			GamepadInput.StickRUp,
			"<Gamepad>/rightStick/up"
		},
		{
			GamepadInput.StickRDown,
			"<Gamepad>/rightStick/down"
		},
		{
			GamepadInput.StickRLeft,
			"<Gamepad>/rightStick/left"
		},
		{
			GamepadInput.StickRRight,
			"<Gamepad>/rightStick/right"
		},
		{
			GamepadInput.StickLUp,
			"<Gamepad>/leftStick/up"
		},
		{
			GamepadInput.StickLDown,
			"<Gamepad>/leftStick/down"
		},
		{
			GamepadInput.StickLRight,
			"<Gamepad>/leftStick/right"
		},
		{
			GamepadInput.StickLLeft,
			"<Gamepad>/leftStick/left"
		},
		{
			GamepadInput.StickR,
			"<Gamepad>/rightStick"
		},
		{
			GamepadInput.StickL,
			"<Gamepad>/leftStick"
		}
	};

	private static readonly Dictionary<KeyCode, MouseButton> s_keyCodeToMouseButtonMap = new Dictionary<KeyCode, MouseButton>
	{
		{
			(KeyCode)323,
			(MouseButton)0
		},
		{
			(KeyCode)324,
			(MouseButton)1
		},
		{
			(KeyCode)325,
			(MouseButton)2
		},
		{
			(KeyCode)326,
			(MouseButton)3
		},
		{
			(KeyCode)327,
			(MouseButton)4
		}
	};

	private static readonly Dictionary<KeyCode, GamepadInput> s_keyCodeToGamepadInputMap = new Dictionary<KeyCode, GamepadInput>
	{
		{
			(KeyCode)330,
			GamepadInput.FaceButtonA
		},
		{
			(KeyCode)346,
			GamepadInput.FaceButtonA
		},
		{
			(KeyCode)331,
			GamepadInput.FaceButtonB
		},
		{
			(KeyCode)347,
			GamepadInput.FaceButtonB
		},
		{
			(KeyCode)332,
			GamepadInput.FaceButtonX
		},
		{
			(KeyCode)348,
			GamepadInput.FaceButtonX
		},
		{
			(KeyCode)333,
			GamepadInput.FaceButtonY
		},
		{
			(KeyCode)349,
			GamepadInput.FaceButtonY
		},
		{
			(KeyCode)334,
			GamepadInput.BumperL
		},
		{
			(KeyCode)343,
			GamepadInput.BumperL
		},
		{
			(KeyCode)335,
			GamepadInput.BumperR
		},
		{
			(KeyCode)344,
			GamepadInput.BumperR
		},
		{
			(KeyCode)336,
			GamepadInput.Select
		},
		{
			(KeyCode)340,
			GamepadInput.Select
		},
		{
			(KeyCode)337,
			GamepadInput.Start
		},
		{
			(KeyCode)339,
			GamepadInput.StickRButton
		},
		{
			(KeyCode)338,
			GamepadInput.StickLButton
		},
		{
			(KeyCode)341,
			GamepadInput.StickLButton
		},
		{
			(KeyCode)342,
			GamepadInput.StickRButton
		}
	};

	private static readonly Dictionary<KeyCode, GamepadButton> s_keyCodeToGamepadButtonMap = new Dictionary<KeyCode, GamepadButton>
	{
		{
			(KeyCode)330,
			(GamepadButton)6
		},
		{
			(KeyCode)346,
			(GamepadButton)6
		},
		{
			(KeyCode)331,
			(GamepadButton)5
		},
		{
			(KeyCode)347,
			(GamepadButton)5
		},
		{
			(KeyCode)332,
			(GamepadButton)7
		},
		{
			(KeyCode)348,
			(GamepadButton)7
		},
		{
			(KeyCode)333,
			(GamepadButton)4
		},
		{
			(KeyCode)349,
			(GamepadButton)4
		},
		{
			(KeyCode)334,
			(GamepadButton)10
		},
		{
			(KeyCode)343,
			(GamepadButton)10
		},
		{
			(KeyCode)335,
			(GamepadButton)11
		},
		{
			(KeyCode)344,
			(GamepadButton)11
		},
		{
			(KeyCode)336,
			(GamepadButton)13
		},
		{
			(KeyCode)340,
			(GamepadButton)13
		},
		{
			(KeyCode)337,
			(GamepadButton)12
		},
		{
			(KeyCode)339,
			(GamepadButton)9
		},
		{
			(KeyCode)338,
			(GamepadButton)8
		},
		{
			(KeyCode)341,
			(GamepadButton)8
		},
		{
			(KeyCode)342,
			(GamepadButton)9
		}
	};

	private static readonly Dictionary<KeyCode, Key> s_keyCodeToKeyMap = new Dictionary<KeyCode, Key>
	{
		{
			(KeyCode)0,
			(Key)0
		},
		{
			(KeyCode)32,
			(Key)1
		},
		{
			(KeyCode)8,
			(Key)65
		},
		{
			(KeyCode)127,
			(Key)71
		},
		{
			(KeyCode)9,
			(Key)3
		},
		{
			(KeyCode)13,
			(Key)2
		},
		{
			(KeyCode)19,
			(Key)76
		},
		{
			(KeyCode)27,
			(Key)60
		},
		{
			(KeyCode)256,
			(Key)84
		},
		{
			(KeyCode)257,
			(Key)85
		},
		{
			(KeyCode)258,
			(Key)86
		},
		{
			(KeyCode)259,
			(Key)87
		},
		{
			(KeyCode)260,
			(Key)88
		},
		{
			(KeyCode)261,
			(Key)89
		},
		{
			(KeyCode)262,
			(Key)90
		},
		{
			(KeyCode)263,
			(Key)91
		},
		{
			(KeyCode)264,
			(Key)92
		},
		{
			(KeyCode)265,
			(Key)93
		},
		{
			(KeyCode)266,
			(Key)82
		},
		{
			(KeyCode)267,
			(Key)78
		},
		{
			(KeyCode)268,
			(Key)79
		},
		{
			(KeyCode)269,
			(Key)81
		},
		{
			(KeyCode)270,
			(Key)80
		},
		{
			(KeyCode)271,
			(Key)77
		},
		{
			(KeyCode)272,
			(Key)83
		},
		{
			(KeyCode)273,
			(Key)63
		},
		{
			(KeyCode)274,
			(Key)64
		},
		{
			(KeyCode)275,
			(Key)62
		},
		{
			(KeyCode)276,
			(Key)61
		},
		{
			(KeyCode)277,
			(Key)70
		},
		{
			(KeyCode)278,
			(Key)68
		},
		{
			(KeyCode)279,
			(Key)69
		},
		{
			(KeyCode)280,
			(Key)67
		},
		{
			(KeyCode)281,
			(Key)66
		},
		{
			(KeyCode)282,
			(Key)94
		},
		{
			(KeyCode)283,
			(Key)95
		},
		{
			(KeyCode)284,
			(Key)96
		},
		{
			(KeyCode)285,
			(Key)97
		},
		{
			(KeyCode)286,
			(Key)98
		},
		{
			(KeyCode)287,
			(Key)99
		},
		{
			(KeyCode)288,
			(Key)100
		},
		{
			(KeyCode)289,
			(Key)101
		},
		{
			(KeyCode)290,
			(Key)102
		},
		{
			(KeyCode)291,
			(Key)103
		},
		{
			(KeyCode)292,
			(Key)104
		},
		{
			(KeyCode)293,
			(Key)105
		},
		{
			(KeyCode)48,
			(Key)50
		},
		{
			(KeyCode)49,
			(Key)41
		},
		{
			(KeyCode)50,
			(Key)42
		},
		{
			(KeyCode)51,
			(Key)43
		},
		{
			(KeyCode)52,
			(Key)44
		},
		{
			(KeyCode)53,
			(Key)45
		},
		{
			(KeyCode)54,
			(Key)46
		},
		{
			(KeyCode)55,
			(Key)47
		},
		{
			(KeyCode)56,
			(Key)48
		},
		{
			(KeyCode)57,
			(Key)49
		},
		{
			(KeyCode)39,
			(Key)5
		},
		{
			(KeyCode)44,
			(Key)7
		},
		{
			(KeyCode)45,
			(Key)13
		},
		{
			(KeyCode)46,
			(Key)8
		},
		{
			(KeyCode)47,
			(Key)9
		},
		{
			(KeyCode)59,
			(Key)6
		},
		{
			(KeyCode)61,
			(Key)14
		},
		{
			(KeyCode)91,
			(Key)11
		},
		{
			(KeyCode)92,
			(Key)10
		},
		{
			(KeyCode)93,
			(Key)12
		},
		{
			(KeyCode)96,
			(Key)4
		},
		{
			(KeyCode)97,
			(Key)15
		},
		{
			(KeyCode)98,
			(Key)16
		},
		{
			(KeyCode)99,
			(Key)17
		},
		{
			(KeyCode)100,
			(Key)18
		},
		{
			(KeyCode)101,
			(Key)19
		},
		{
			(KeyCode)102,
			(Key)20
		},
		{
			(KeyCode)103,
			(Key)21
		},
		{
			(KeyCode)104,
			(Key)22
		},
		{
			(KeyCode)105,
			(Key)23
		},
		{
			(KeyCode)106,
			(Key)24
		},
		{
			(KeyCode)107,
			(Key)25
		},
		{
			(KeyCode)108,
			(Key)26
		},
		{
			(KeyCode)109,
			(Key)27
		},
		{
			(KeyCode)110,
			(Key)28
		},
		{
			(KeyCode)111,
			(Key)29
		},
		{
			(KeyCode)112,
			(Key)30
		},
		{
			(KeyCode)113,
			(Key)31
		},
		{
			(KeyCode)114,
			(Key)32
		},
		{
			(KeyCode)115,
			(Key)33
		},
		{
			(KeyCode)116,
			(Key)34
		},
		{
			(KeyCode)117,
			(Key)35
		},
		{
			(KeyCode)118,
			(Key)36
		},
		{
			(KeyCode)119,
			(Key)37
		},
		{
			(KeyCode)120,
			(Key)38
		},
		{
			(KeyCode)121,
			(Key)39
		},
		{
			(KeyCode)122,
			(Key)40
		},
		{
			(KeyCode)300,
			(Key)73
		},
		{
			(KeyCode)301,
			(Key)72
		},
		{
			(KeyCode)302,
			(Key)75
		},
		{
			(KeyCode)303,
			(Key)52
		},
		{
			(KeyCode)304,
			(Key)51
		},
		{
			(KeyCode)305,
			(Key)56
		},
		{
			(KeyCode)306,
			(Key)55
		},
		{
			(KeyCode)307,
			(Key)54
		},
		{
			(KeyCode)308,
			(Key)53
		},
		{
			(KeyCode)310,
			(Key)57
		},
		{
			(KeyCode)311,
			(Key)57
		},
		{
			(KeyCode)309,
			(Key)58
		},
		{
			(KeyCode)312,
			(Key)58
		},
		{
			(KeyCode)313,
			(Key)54
		},
		{
			(KeyCode)316,
			(Key)74
		},
		{
			(KeyCode)319,
			(Key)59
		}
	};

	public static ZInput instance => m_instance;

	public static Vector3 mousePosition => m_instance?.Input_GetMousePosition() ?? Vector3.zero;

	private static GamepadType ConnectedGamepadType
	{
		get
		{
			Gamepad current = Gamepad.current;
			if (current != null)
			{
				if (!(current is XInputController))
				{
					if (!(current is DualShock4GamepadHID))
					{
						if (!(current is DualShockGamepad))
						{
							if (current is SwitchProControllerHID)
							{
								return GamepadType.SwitchPro;
							}
							return GamepadType.Generic;
						}
						return GamepadType.DualSense;
					}
					return GamepadType.DualShock;
				}
				return GamepadType.XInput;
			}
			return GamepadType.None;
		}
	}

	public static bool s_IsRebindActive { get; private set; }

	public static bool GamepadActive => m_inputSource == InputSource.Gamepad;

	public static bool WorkaroundEnabled
	{
		get
		{
			return _workaroundEnabled;
		}
		set
		{
		}
	}

	public static bool SwapFaceButtons => false;

	public static bool VirtualKeyboardOpen
	{
		get
		{
			return m_virtualKeyboardOpen;
		}
		set
		{
			if (value)
			{
				ResetAllButtonStates();
			}
			if (m_virtualKeyboardOpen != value)
			{
				m_blockGamePadInput = 0.3f;
			}
			m_virtualKeyboardOpen = value;
		}
	}

	public static string CompositionString
	{
		get
		{
			BaseInput input = EventSystem.current.currentInputModule.input;
			if (!((Object)(object)input != (Object)null))
			{
				return Input.compositionString;
			}
			return input.compositionString;
		}
	}

	public static int CompositionLength => CompositionString.Length;

	public static event Action OnInputLayoutChanged;

	public static void Initialize()
	{
		if (PlatformPrefs.GetInt("ControllerLayout", -1) >= 0)
		{
			InputLayout = (InputLayout)PlatformPrefs.GetInt("ControllerLayout");
		}
		if (m_instance == null)
		{
			m_instance = new ZInput();
		}
	}

	public ZInput()
	{
		InputSystem.RegisterInteraction<Vector2MultiTap>((string)null);
		InputSystem.RegisterInteraction<Vector2Tap>((string)null);
		InputSystem.RegisterProcessor<FloatDeadzone>((string)null);
		InputSystem.RegisterProcessor<Vector2Deadzone>((string)null);
		GamepadType gamepadFromCLArgs = GetGamepadFromCLArgs();
		if (gamepadFromCLArgs != 0)
		{
			m_definition = PlatformGamepadDefinition.Get(gamepadFromCLArgs);
		}
		if (m_definition == null)
		{
			gamepadFromCLArgs = GamepadType.NewInputSystem;
			m_definition = PlatformGamepadDefinition.Get(gamepadFromCLArgs);
		}
		InputSystem.onDeviceChange += OnDeviceChanged;
		SetUpMouseAndStickHandlers();
		SetUpStickHandlers();
		Reset();
		Load();
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		InputDeviceMatcher val = default(InputDeviceMatcher);
		val = ((InputDeviceMatcher)(ref val)).WithInterface("HID", true);
		val = ((InputDeviceMatcher)(ref val)).WithManufacturer("Sony Interactive Entertainment", true);
		InputSystem.RegisterLayout<DualSenseGamepadHID>("DualSense Edge", (InputDeviceMatcher?)((InputDeviceMatcher)(ref val)).WithProduct("DualSense Edge Wireless Controller", true));
	}

	private GamepadType GetGamepadFromCLArgs()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (commandLineArgs[i] == "-gamepad" && Enum.TryParse<GamepadType>(commandLineArgs[i + 1], ignoreCase: true, out var result))
			{
				return result;
			}
		}
		return GamepadType.None;
	}

	public static void Update(float dt)
	{
		if (m_instance != null)
		{
			m_instance.InternalUpdate(dt);
		}
	}

	private void InternalUpdate(float dt)
	{
		if (WorkaroundEnabled && !m_systemUpdated)
		{
			InputSystem.Update();
		}
		UpdateMouseInput();
		if (m_blockGamePadInput > 0f)
		{
			m_blockGamePadInput -= dt;
		}
		foreach (ButtonDef value in m_buttons.Values)
		{
			value.Tick(dt);
		}
		m_systemUpdated = false;
	}

	public static void FixedUpdate(float dt)
	{
		if (m_instance != null)
		{
			m_instance.InternalUpdateFixed(dt);
		}
	}

	private void InternalUpdateFixed(float dt)
	{
		if (WorkaroundEnabled && !m_systemUpdated)
		{
			InputSystem.Update();
			m_systemUpdated = true;
		}
		foreach (ButtonDef value in m_buttons.Values)
		{
			value.Tick(dt);
		}
	}

	public static void OnGUI()
	{
		if (m_instance != null)
		{
			m_instance.OnGUIInternal();
		}
	}

	private void OnGUIInternal()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		if (Event.current.isKey && (int)Event.current.keyCode > 0 && (int)Event.current.keyCode < 330)
		{
			OnInput(InputSource.KeyboardMouse, allowSwitchInputSource: true);
		}
	}

	public void Reset()
	{
		ClearButtons();
		ResetKBMButtons();
		UpdateGamepadInputLayout(clearButtons: false);
	}

	public void Save()
	{
		PlatformPrefs.DeleteKey("gamepad_enabled");
		PlatformPrefs.SetInt("input_switching_mode", (int)s_inputSwitchingMode);
		foreach (ButtonDef item in m_buttons.Values.Where((ButtonDef b) => b.Rebindable).ToList())
		{
			PlatformPrefs.SetString("kbmBinding_" + item.Name, item.GetActionPath());
		}
		NukeLegacyPlayerPrefs();
	}

	public void Load()
	{
		SwapTriggers = PlatformPrefs.GetInt("SwapTriggers") == 1;
		ToggleRun = PlatformPrefs.GetInt("ToggleRun", IsGamepadActive() ? 1 : 0) == 1;
		Reset();
		if (PlatformPrefs.GetInt("LegacyNuked") != 1)
		{
			LoadLegacyPlayerPrefs();
		}
		InputSource @int = (InputSource)PlatformPrefs.GetInt("input_switching_mode");
		int int2 = PlatformPrefs.GetInt("gamepad_enabled", -1);
		if (Enum.IsDefined(typeof(InputSource), @int) && @int.HasFlag(InputSource.InputSwitchingModeBit | InputSource.ValidBit))
		{
			SetInputSwitchingMode(@int);
		}
		else if (int2 >= 0)
		{
			SetInputSwitchingMode((int2 == 1) ? InputSource.AutomaticNonBlocking : InputSource.KeyboardMouseOnly);
		}
		else
		{
			SetInputSwitchingMode(InputSource.AutomaticNonBlocking);
		}
		foreach (ButtonDef item in m_buttons.Values.Where((ButtonDef b) => b.Rebindable && PlatformPrefs.HasKey("kbmBinding_" + b.Name)).ToList())
		{
			item.Rebind(PlatformPrefs.GetString("kbmBinding_" + item.Name));
		}
	}

	private void LoadLegacyPlayerPrefs()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		foreach (ButtonDef item in m_buttons.Values.Where((ButtonDef b) => b.Rebindable).ToList())
		{
			string text = "kbmKey_" + item.Name;
			if (PlatformPrefs.HasKey(text))
			{
				item.Rebind((PlatformPrefs.GetInt(text + "_isMouseButton") != 0) ? MouseButtonToPath((MouseButton)PlatformPrefs.GetInt(text)) : KeyToPath((Key)PlatformPrefs.GetInt(text)));
				continue;
			}
			string name = "key_" + item.Name;
			if (PlatformPrefs.HasKey(name))
			{
				KeyCode keyCode = (KeyCode)PlatformPrefs.GetInt(name);
				MouseButton result2;
				if (TryKeyCodeToKey(keyCode, out var result))
				{
					item.Rebind(KeyToPath(result));
				}
				else if (TryKeyCodeToMouseButton(keyCode, out result2))
				{
					item.Rebind(MouseButtonToPath(result2));
				}
			}
		}
	}

	private void NukeLegacyPlayerPrefs()
	{
		foreach (ButtonDef value in m_buttons.Values)
		{
			DeleteLegacyPlayerPrefKey("kbmKey_" + value.Name);
			DeleteLegacyPlayerPrefKey("kbmKey_" + value.Name + "_isMouseButton");
			DeleteLegacyPlayerPrefKey("key_" + value.Name);
		}
		DeleteLegacyPlayerPrefKey("kbmKey_Forward");
		DeleteLegacyPlayerPrefKey("kbmKey_Forward_isMouseButton");
		DeleteLegacyPlayerPrefKey("kbmKey_Backward");
		DeleteLegacyPlayerPrefKey("kbmKey_Backward_isMouseButton");
		DeleteLegacyPlayerPrefKey("kbmKey_Left");
		DeleteLegacyPlayerPrefKey("kbmKey_Left_isMouseButton");
		DeleteLegacyPlayerPrefKey("kbmKey_Right");
		DeleteLegacyPlayerPrefKey("kbmKey_Right_isMouseButton");
		DeleteLegacyPlayerPrefKey("key_Forward");
		DeleteLegacyPlayerPrefKey("key_Backward");
		DeleteLegacyPlayerPrefKey("key_Left");
		DeleteLegacyPlayerPrefKey("key_Right");
		PlatformPrefs.SetInt("LegacyNuked", 1);
	}

	private void DeleteLegacyPlayerPrefKey(string key)
	{
		if (PlatformPrefs.HasKey(key))
		{
			PlatformPrefs.DeleteKey(key);
		}
	}

	private void UpdateMouseInput()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (GetMouseDelta() != Vector2.zero)
		{
			OnInput(InputSource.KeyboardMouse, allowSwitchInputSource: true);
			m_mouseInputThisFrame = true;
		}
		else
		{
			m_mouseInputThisFrame = false;
		}
	}

	private static void InvalidateInputSwitchingModeValid()
	{
		s_isInputSwitchingModeValid = null;
	}

	public static void SetGamepadEnabled(bool enabled)
	{
		SetInputSwitchingMode(enabled ? InputSource.AutomaticNonBlocking : InputSource.KeyboardMouseOnly);
	}

	public static void SetInputSwitchingMode(InputSource inputSwitchingMode)
	{
		if (inputSwitchingMode != 0)
		{
			s_inputSwitchingMode = inputSwitchingMode;
		}
		else
		{
			ZLog.LogError($"Can't set input switching mode to {InputSource.None}! Forcing value to {InputSource.AutomaticNonBlocking}");
			s_inputSwitchingMode = InputSource.AutomaticNonBlocking;
		}
		InvalidateInputSwitchingModeValid();
		InputSource inputSource = m_inputSource;
		switch (s_inputSwitchingMode)
		{
		case InputSource.KeyboardMouseOnly:
		case InputSource.BothKeyboardMouseHints:
			inputSource = InputSource.KeyboardMouse;
			break;
		case InputSource.GamepadOnly:
		case InputSource.BothGamepadHints:
			inputSource = InputSource.Gamepad;
			break;
		}
		bool flag = inputSource != m_inputSource;
		m_inputSource = inputSource;
		if (flag && instance != null)
		{
			ZInput.OnInputLayoutChanged?.Invoke();
		}
	}

	private void OnInput(InputSource inputSource, bool allowSwitchInputSource)
	{
		if (inputSource == InputSource.Gamepad && allowSwitchInputSource && PlatformManager.DistributionPlatform.InputDeviceManager != null)
		{
			PlatformManager.DistributionPlatform.InputDeviceManager.NotifyGamepadInput();
		}
		if (inputSource == InputSource.Gamepad && (m_virtualKeyboardOpen || m_blockGamePadInput > 0f || m_mouseInputThisFrame))
		{
			return;
		}
		if (allowSwitchInputSource && s_inputSwitchingMode.HasFlag(InputSource.AllowedInputMask))
		{
			bool num = m_inputSource != inputSource;
			m_inputSource = inputSource;
			if (num && instance != null)
			{
				ZInput.OnInputLayoutChanged?.Invoke();
			}
		}
		if (m_inputSource == inputSource)
		{
			m_inputTimer = DateTime.Now;
			switch (m_inputSource)
			{
			case InputSource.KeyboardMouse:
				m_inputTimerMouse = m_inputTimer;
				break;
			case InputSource.Gamepad:
				m_inputTimerGamepad = m_inputTimer;
				break;
			}
		}
	}

	private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if (device is Gamepad && ((int)change == 0 || (int)change == 3 || (int)change == 7))
		{
			ZInput.OnInputLayoutChanged?.Invoke();
		}
	}

	private void OnActionPerformed(CallbackContext ctx)
	{
		InputSource inputSource = ((((CallbackContext)(ref ctx)).control.device is Gamepad) ? InputSource.Gamepad : InputSource.KeyboardMouse);
		if (ShouldAcceptInputFromSource(inputSource) && ((CallbackContext)(ref ctx)).action.IsPressed() && m_buttons.TryGetValue(((CallbackContext)(ref ctx)).action.name, out var value))
		{
			value.Press();
			OnInput(inputSource, allowSwitchInputSource: true);
		}
	}

	private void OnActionCanceled(CallbackContext ctx)
	{
		if (ShouldAcceptInputFromSource((((CallbackContext)(ref ctx)).control.device is Gamepad) ? InputSource.Gamepad : InputSource.KeyboardMouse) && m_buttons.TryGetValue(((CallbackContext)(ref ctx)).action.name, out var value))
		{
			value.Release();
		}
	}

	private Vector3 Input_GetMousePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(m_mousePosition.ReadValue<Vector2>());
	}

	public static Vector2 GetMouseDelta()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return m_instance?.Internal_GetMouseDelta() ?? Vector2.zero;
	}

	private Vector2 Internal_GetMouseDelta()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Vector2 result = m_mouseDelta.ReadValue<Vector2>();
		if (!((Vector2)(ref result)).Equals(Vector2.zero))
		{
			m_instance.OnInput(InputSource.KeyboardMouse, allowSwitchInputSource: true);
		}
		if (ShouldAcceptInputFromSource(InputSource.KeyboardMouse))
		{
			return result;
		}
		return Vector2.zero;
	}

	public static float GetMouseScrollWheel()
	{
		return m_instance?.Internal_GetMouseScrollWheel() ?? 0f;
	}

	private float Internal_GetMouseScrollWheel()
	{
		float num = m_mouseScrollDeltaAction.ReadValue<float>() * GetScrollModifier();
		if (num != 0f)
		{
			OnInput(InputSource.KeyboardMouse, allowSwitchInputSource: true);
		}
		if (ShouldAcceptInputFromSource(InputSource.KeyboardMouse))
		{
			return num;
		}
		return 0f;
	}

	public static T GetValue<T>(string name) where T : struct
	{
		return ReadValueDef<T>(name);
	}

	public static float GetJoyLTrigger()
	{
		return ReadValueDef<float>("TriggerL");
	}

	public static float GetJoyRTrigger()
	{
		return ReadValueDef<float>("TriggerR");
	}

	public static float GetJoyRightStickX(bool smooth = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ReadValueDef<Vector2>("StickR", smooth).x;
	}

	public static float GetJoyRightStickY(bool smooth = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return 0f - ReadValueDef<Vector2>("StickR", smooth).y;
	}

	public static float GetJoyLeftStickX(bool smooth = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ReadValueDef<Vector2>("StickL", smooth).x;
	}

	public static float GetJoyLeftStickY(bool smooth = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return 0f - ReadValueDef<Vector2>("StickL", smooth).y;
	}

	private static T ReadValueDef<T>(string name, bool smooth = false) where T : struct
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (m_instance == null)
		{
			return default(T);
		}
		if (!m_instance.m_values.TryGetValue(name, out var value))
		{
			Debug.LogError((object)("No ValueDef with name " + name + " found."));
			return default(T);
		}
		T valueRaw = value.GetValueRaw<T>();
		if (!(valueRaw is float value2))
		{
			if (valueRaw is Vector2 value3)
			{
				return (T)(object)m_instance.ApplyDeadzoneVector(value3, smooth);
			}
			return valueRaw;
		}
		return (T)(object)m_instance.ApplyDeadzoneFloat(value2, smooth);
	}

	public static bool GetRadialTap()
	{
		ZInput zInput = m_instance;
		if (zInput == null)
		{
			return false;
		}
		return zInput.m_radialTap.WasPerformedThisFrame();
	}

	public static bool GetRadialMultiTap()
	{
		ZInput zInput = m_instance;
		if (zInput == null)
		{
			return false;
		}
		return zInput.m_radialMultiTap.WasPerformedThisFrame();
	}

	public static bool GetButtonDown(string name)
	{
		return m_instance?.TryGetButtonState(name, (ButtonDef b) => b.Pressed) ?? false;
	}

	public static bool GetButtonUp(string name)
	{
		return m_instance?.TryGetButtonState(name, (ButtonDef b) => b.Released) ?? false;
	}

	public static bool GetButton(string name)
	{
		return m_instance?.TryGetButtonState(name, (ButtonDef b) => b.Held) ?? false;
	}

	public static bool GetMouseButton(int button)
	{
		return m_instance?.TryGetButtonState(IntToMouseButtonString(button), (ButtonDef b) => b.Held) ?? false;
	}

	public static bool GetMouseButtonDown(int button)
	{
		return m_instance?.TryGetButtonState(IntToMouseButtonString(button), (ButtonDef b) => b.Pressed) ?? false;
	}

	public static bool GetMouseButtonUp(int button)
	{
		return m_instance?.TryGetButtonState(IntToMouseButtonString(button), (ButtonDef b) => b.Released) ?? false;
	}

	private bool TryGetButtonState(string name, Func<ButtonDef, bool> stateCheckProperty)
	{
		if (m_buttons.TryGetValue(name, out var value))
		{
			return stateCheckProperty(value);
		}
		return false;
	}

	public static float GetButtonPressedTimer(string name)
	{
		if (m_instance.m_buttons.TryGetValue(name, out var value))
		{
			return value.PressedTimer;
		}
		return 0f;
	}

	public static float GetButtonLastPressedTimer(string name)
	{
		if (m_instance.m_buttons.TryGetValue(name, out var value))
		{
			return value.LastPressedTimer;
		}
		return 0f;
	}

	public DateTime GetLastInputTimer()
	{
		return m_inputTimer;
	}

	public DateTime GetLastInputTimerGamepad()
	{
		return m_inputTimerGamepad;
	}

	public DateTime GetLastInputTimerMouse()
	{
		return m_inputTimerMouse;
	}

	public static bool GetKey(KeyCode key, bool logWarning = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return m_instance?.TryGetKeyStateLowLevel(key, (ButtonControl b) => b.isPressed, logWarning) ?? false;
	}

	public static bool GetKeyDown(KeyCode key, bool logWarning = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return m_instance?.TryGetKeyStateLowLevel(key, (ButtonControl b) => b.wasPressedThisFrame, logWarning) ?? false;
	}

	public static bool GetKeyUp(KeyCode key, bool logWarning = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return m_instance?.TryGetKeyStateLowLevel(key, (ButtonControl b) => b.wasReleasedThisFrame, logWarning) ?? false;
	}

	private bool TryGetKeyStateLowLevel(KeyCode keyCode, Func<ButtonControl, bool> stateCheckProperty, bool logWarning = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!IsKeyCodeValid(keyCode))
		{
			return false;
		}
		if ((int)keyCode >= 330)
		{
			if (Gamepad.current != null)
			{
				return stateCheckProperty(Gamepad.current[KeyCodeToGamepadButton(keyCode, logWarning)]);
			}
			return false;
		}
		if ((int)keyCode >= 323 && (int)keyCode <= 327)
		{
			if (Mouse.current != null)
			{
				return stateCheckProperty(KeyCodeToMouseButtonControl(keyCode, logWarning));
			}
			return false;
		}
		if (Keyboard.current != null)
		{
			return stateCheckProperty((ButtonControl)(object)Keyboard.current[KeyCodeToKey(keyCode, logWarning)]);
		}
		return false;
	}

	public ButtonDef GetButtonDef(string name)
	{
		return CollectionExtensions.GetValueOrDefault<string, ButtonDef>((IReadOnlyDictionary<string, ButtonDef>)m_buttons, name);
	}

	public string GetBoundKeyString(string name, bool emptyStringOnMissing = false)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (m_virtualKeyboardOpen)
		{
			return "";
		}
		if (!m_buttons.TryGetValue(name, out var value))
		{
			if (!emptyStringOnMissing)
			{
				return "MISSING BUTTON DEF \"" + name + "\"";
			}
			return "";
		}
		InputBinding val = ((IEnumerable<InputBinding>)(object)value.ButtonAction.bindings).FirstOrDefault();
		if (val == default(InputBinding))
		{
			if (!emptyStringOnMissing)
			{
				return "MISSING KEY BINDING \"" + name + "\"";
			}
			return "";
		}
		string key = MapKeyFromPath(((InputBinding)(ref val)).effectivePath);
		if (value.Source != InputSource.Gamepad)
		{
			if (!s_keyLocalizationMap.TryGetValue(key, out var value2))
			{
				return ((InputBinding)(ref val)).ToDisplayString((DisplayStringOptions)4, (InputControl)(object)Keyboard.current);
			}
			return value2;
		}
		if (m_blockGamePadInput > 0f)
		{
			return "";
		}
		string text;
		switch (CurrentGlyph)
		{
		case GamepadGlyphs.Auto:
		{
			string text2;
			switch (ConnectedGamepadType)
			{
			case GamepadType.XInput:
				text2 = "xbox";
				break;
			case GamepadType.DualSense:
			case GamepadType.DualShock:
				text2 = "ps5";
				break;
			default:
				text2 = "xbox";
				break;
			}
			text = text2;
			break;
		}
		case GamepadGlyphs.Xbox:
			text = "xbox";
			break;
		case GamepadGlyphs.Playstation:
			text = "ps5";
			break;
		default:
			text = "xbox";
			break;
		}
		string text3 = text;
		return "<sprite=\"" + text3 + "\" name=\"" + s_gamepadSpriteMap[key][text3] + "\">";
	}

	public string GetBoundActionString(KeyCode keycode)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		string str = "";
		foreach (ButtonDef item in m_buttons.Values.Where((ButtonDef b) => b.ShowHints && b.Source != InputSource.Gamepad))
		{
			if (((IEnumerable<InputBinding>)(object)item.ButtonAction.bindings).FirstOrDefault((Func<InputBinding, bool>)delegate(InputBinding e)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				string effectivePath = ((InputBinding)(ref e)).effectivePath;
				Key val = KeyCodeToKey(keycode);
				return effectivePath.Contains(((object)(Key)(ref val)).ToString());
			}) != default(InputBinding))
			{
				TryFormatAndAddString(ref str, item);
			}
		}
		return str;
	}

	public string GetBoundActionString(GamepadInput gamepadInput, FloatRange? mappedRange)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		string str = "";
		foreach (ButtonDef item in m_buttons.Values.Where((ButtonDef b) => b.ShowHints && b.Source == InputSource.Gamepad))
		{
			if (((IEnumerable<InputBinding>)(object)item.ButtonAction.bindings).FirstOrDefault((Func<InputBinding, bool>)((InputBinding e) => ((InputBinding)(ref e)).effectivePath == s_gamepadInputPathMap[gamepadInput])) != default(InputBinding))
			{
				TryFormatAndAddString(ref str, item);
			}
		}
		return str;
	}

	private void TryFormatAndAddString(ref string str, ButtonDef button)
	{
		string text = button.Name.ToLower();
		if (!text.Contains("bumper") && !text.Contains("start") && !(text == "joyaltkeys"))
		{
			if (str.Length > 0)
			{
				str += " / ";
			}
			if (text.Length > 3 && text.ToLower().StartsWith("joy"))
			{
				text = text.Substring(3);
			}
			if (text == "radialbuild")
			{
				text = "radial";
			}
			if (button.AltKey)
			{
				str = str + "<color=#AAAAAA>$settings_" + text + "</color>";
			}
			else if (IsNonClassicFunctionality() && (text == "rotate" || text == "rotateright"))
			{
				str += "$rotate_build_mode";
			}
			else
			{
				str = str + "$settings_" + text;
			}
		}
	}

	private void ClearGamepadButtons()
	{
		foreach (ButtonDef item in from button in m_buttons.Values.ToList()
			where button.Source == InputSource.Gamepad
			select button)
		{
			UnsubscribeButton(item);
			item.ButtonAction.Disable();
			m_buttons.Remove(item.Name);
		}
	}

	private void ClearButtons()
	{
		UnsubscribeButtons();
		m_buttons.Clear();
	}

	private void SubscribeButton(ButtonDef btn)
	{
		btn.ButtonAction.performed += OnActionPerformed;
		btn.ButtonAction.canceled += OnActionCanceled;
	}

	private void SubscribeButtons()
	{
		foreach (ButtonDef value in m_buttons.Values)
		{
			value.ButtonAction.performed += OnActionPerformed;
			value.ButtonAction.canceled += OnActionCanceled;
		}
	}

	private void UnsubscribeButtons(bool disable = true)
	{
		foreach (ButtonDef value in m_buttons.Values)
		{
			value.ButtonAction.performed -= OnActionPerformed;
			value.ButtonAction.canceled -= OnActionCanceled;
			if (disable)
			{
				value.ButtonAction.Disable();
			}
		}
	}

	private void UnsubscribeButton(ButtonDef btn)
	{
		btn.ButtonAction.performed -= OnActionPerformed;
		btn.ButtonAction.canceled -= OnActionCanceled;
	}

	public static void ResetButtonStatus(string name)
	{
		if (m_instance.m_buttons.TryGetValue(name, out var value))
		{
			value.ResetState();
		}
	}

	public static void ResetAllButtonStates()
	{
		if (m_instance == null)
		{
			return;
		}
		foreach (ButtonDef value in m_instance.m_buttons.Values)
		{
			value.ResetState();
		}
	}

	public void StartBindKey(string name)
	{
		if (!m_buttons.TryGetValue(name, out var value))
		{
			Debug.LogError((object)("No button with name: " + name + " found! Cannot start rebind."));
			return;
		}
		if (!value.Rebindable)
		{
			Debug.LogWarning((object)("Button with name " + name + " is not set to rebindable."));
			return;
		}
		InputAction action = value.ButtonAction;
		if (action == null)
		{
			Debug.LogError((object)("Button " + name + " does not have an assigned InputAction. Cannot start rebind."));
			return;
		}
		action.Disable();
		RebindingOperation val = InputActionRebindingExtensions.PerformInteractiveRebinding(action, -1).WithCancelingThrough("<Keyboard>/escape").WithCancelingThrough("<Gamepad>/*");
		if (value.Name.Contains("Tab"))
		{
			val.WithControlsExcluding("Mouse");
		}
		val.Start().OnComplete((Action<RebindingOperation>)delegate
		{
			OnRebindComplete(action);
		}).OnCancel((Action<RebindingOperation>)delegate
		{
			OnRebindComplete(action);
		});
		s_IsRebindActive = true;
	}

	private void OnRebindComplete(InputAction action)
	{
		action.Enable();
		s_IsRebindActive = false;
	}

	public void ResetToDefault(string name = "all")
	{
		if (name == "all")
		{
			foreach (ButtonDef value2 in m_buttons.Values)
			{
				value2.ResetBinding();
			}
			return;
		}
		if (!m_buttons.TryGetValue(name, out var value))
		{
			Debug.LogError((object)("No button with name: " + name + " found. Can't reset."));
		}
		else
		{
			value.ResetBinding();
		}
	}

	public static void UpdateRadialTapTime(float newDuration)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		ZInput zInput = m_instance;
		if (zInput != null)
		{
			InputActionRebindingExtensions.ApplyParameterOverride(zInput.m_radialTap, "vector2Tap:duration", PrimitiveValue.op_Implicit(newDuration), default(InputBinding));
		}
	}

	public static void UpdateRadialMultiTap(float newDuration, float newDelay, int newAmount, bool requireReleaseForFinal)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (m_instance != null)
		{
			InputActionRebindingExtensions.ApplyParameterOverride(m_instance.m_radialMultiTap, "vector2MultiTap:tapTime", PrimitiveValue.op_Implicit(newDuration), default(InputBinding));
			InputActionRebindingExtensions.ApplyParameterOverride(m_instance.m_radialMultiTap, "vector2MultiTap:tapDelay", PrimitiveValue.op_Implicit(newDelay), default(InputBinding));
			InputActionRebindingExtensions.ApplyParameterOverride(m_instance.m_radialMultiTap, "vector2MultiTap:tapCount", PrimitiveValue.op_Implicit(newAmount), default(InputBinding));
			InputActionRebindingExtensions.ApplyParameterOverride(m_instance.m_radialMultiTap, "vector2MultiTap:requireReleaseForFinal", PrimitiveValue.op_Implicit(requireReleaseForFinal), default(InputBinding));
		}
	}

	public void ChangeLayout(InputLayout inputLayout)
	{
		InputLayout = inputLayout;
		UpdateGamepadInputLayout();
		PlatformPrefs.SetInt("ControllerLayout", (int)InputLayout);
		ZInput.OnInputLayoutChanged?.Invoke();
	}

	private void UpdateGamepadInputLayout(bool clearButtons = true)
	{
		if (clearButtons)
		{
			ClearGamepadButtons();
		}
		ResetGamepadButtonsGeneric();
		switch (InputLayout)
		{
		case InputLayout.Default:
			ResetGamepadToClassic();
			break;
		case InputLayout.Alternative1:
			ResetGamepadToAlt1();
			break;
		case InputLayout.Alternative2:
			ResetGamepadToAlt2();
			break;
		}
		if (SwapTriggers || SwapFaceButtons)
		{
			foreach (ButtonDef item in m_buttons.Values.Where((ButtonDef b) => b.Name.Contains("Joy")).ToList())
			{
				if (SwapTriggers)
				{
					SwapBindingPaths(item.ButtonAction, "<Gamepad>/leftTrigger", "<Gamepad>/rightTrigger");
				}
				if (SwapFaceButtons)
				{
					SwapBindingPaths(item.ButtonAction, "<Gamepad>/buttonSouth", "<Gamepad>/buttonEast");
					SwapBindingPaths(item.ButtonAction, "<Gamepad>/buttonWest", "<Gamepad>/buttonNorth");
				}
			}
		}
		if (!SwapTriggers)
		{
			return;
		}
		foreach (ValueDef item2 in m_values.Values.Where((ValueDef v) => v.Name.Contains("Trigger")).ToList())
		{
			SwapBindingPaths(item2.ValueAction, "<Gamepad>/leftTrigger", "<Gamepad>/rightTrigger");
		}
	}

	private void SwapBindingPaths(InputAction action, string bindingPath1, string bindingPath2)
	{
		InputActionRebindingExtensions.ApplyBindingOverride(action, bindingPath1, (string)null, bindingPath2);
		InputActionRebindingExtensions.ApplyBindingOverride(action, bindingPath2, (string)null, bindingPath1);
	}

	public static void SetMousePosition(Vector2 newPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Cursor.lockState == 1)
		{
			ZLog.LogError("Can't set cursor position when cursor is locked!");
		}
		Mouse current = Mouse.current;
		if (current != null)
		{
			current.WarpCursorPosition(newPos);
		}
	}

	public static bool IsKeyCodeValid(KeyCode keyCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if ((int)keyCode != 0 && (int)keyCode <= 349 && (int)keyCode != 328)
		{
			return (int)keyCode != 329;
		}
		return false;
	}

	public static GamepadGlyphs ConnectedGamepadTypeGlyphs()
	{
		return ConnectedGamepadType switch
		{
			GamepadType.XInput => GamepadGlyphs.Xbox, 
			GamepadType.DualSense => GamepadGlyphs.Playstation, 
			GamepadType.DualShock => GamepadGlyphs.Playstation, 
			_ => GamepadGlyphs.Xbox, 
		};
	}

	public List<string> GetDuplicateBindings()
	{
		return (from b in m_buttons
			group b by b.Value.GetActionPath() into g
			where g.Count() > 1
			select g).SelectMany((IGrouping<string, KeyValuePair<string, ButtonDef>> g) => g.Select((KeyValuePair<string, ButtonDef> b) => b.Key)).ToList();
	}

	private float ApplyDeadzoneFloat(float value, bool smooth)
	{
		float num = Mathf.Sign(value);
		value = Mathf.Abs(value);
		value = Mathf.Clamp01(value - 0.2f);
		value *= 1.25f;
		if (smooth)
		{
			value *= value;
		}
		return value * num;
	}

	private Vector2 ApplyDeadzoneVector(Vector2 value, bool smooth)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(ApplyDeadzoneFloat(value.x, smooth), ApplyDeadzoneFloat(value.y, smooth));
	}

	private static float GetScrollModifier()
	{
		return 1f;
	}

	public static void IgnoreMouseInputForFrames(int frames)
	{
		m_ignoreMouseInputFrames = frames;
	}

	private string MapKeyFromPath(string path)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		string[] array = path.Split("/").Skip(1).ToArray();
		string text = array[^1];
		return string.Join("_", (text == "x" || text == "y") ? array[Range.EndAt(new Index(1, true))] : array);
	}

	private static string IntToMouseButtonString(int button, bool logWarning = true)
	{
		if (TryIntToMouseButtonString(button, out var result))
		{
			return result;
		}
		if (logWarning)
		{
			Debug.LogError((object)$"{button} is not a valid Mouse Button. Returning \"MouseLeft\"");
		}
		return result;
	}

	private static bool TryIntToMouseButtonString(int button, out string result)
	{
		switch (button)
		{
		case 0:
			result = "MouseLeft";
			return true;
		case 1:
			result = "MouseRight";
			return true;
		case 2:
			result = "MouseMiddle";
			return true;
		case 3:
			result = "MouseForward";
			return true;
		case 4:
			result = "MouseBack";
			return true;
		default:
			result = "MouseLeft";
			return false;
		}
	}

	private static Key KeyCodeToKey(KeyCode keyCode, bool logWarning = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (TryKeyCodeToKey(keyCode, out var result))
		{
			return result;
		}
		if (logWarning)
		{
			Debug.LogWarning((object)$"The KeyCode: {keyCode}, lacks a proper counterpart in the Key enum. Returning \"Key.None\".");
		}
		return result;
	}

	private static bool TryKeyCodeToKey(KeyCode keyCode, out Key result)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		result = (Key)0;
		return s_keyCodeToKeyMap.TryGetValue(keyCode, out result);
	}

	private static ButtonControl KeyCodeToMouseButtonControl(KeyCode keyCode, bool logWarning = true)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected I4, but got Unknown
		if (Mouse.current == null)
		{
			return null;
		}
		MouseButton val = KeyCodeToMouseButton(keyCode, logWarning);
		return (ButtonControl)((int)val switch
		{
			0 => Mouse.current.leftButton, 
			1 => Mouse.current.rightButton, 
			2 => Mouse.current.middleButton, 
			3 => Mouse.current.forwardButton, 
			4 => Mouse.current.backButton, 
			_ => null, 
		});
	}

	private static MouseButton KeyCodeToMouseButton(KeyCode keyCode, bool logWarning = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (TryKeyCodeToMouseButton(keyCode, out var result))
		{
			return result;
		}
		if (logWarning)
		{
			Debug.LogWarning((object)$"The KeyCode: {keyCode}, lacks a proper counterpart in the Key enum. Returning Left Button.");
		}
		return result;
	}

	private static bool TryKeyCodeToMouseButton(KeyCode keyCode, out MouseButton result)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		result = (MouseButton)0;
		return s_keyCodeToMouseButtonMap.TryGetValue(keyCode, out result);
	}

	private static GamepadButton KeyCodeToGamepadButton(KeyCode keyCode, bool logWarning = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (TryKeyCodeToGamepadButton(keyCode, out var result))
		{
			return result;
		}
		if (logWarning)
		{
			Debug.LogWarning((object)$"The KeyCode: {keyCode}, lacks a proper counterpart in the GamepadButton enum. Returning South Button.");
		}
		return result;
	}

	private static bool TryKeyCodeToGamepadButton(KeyCode keyCode, out GamepadButton result)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected I4, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		result = (GamepadButton)6;
		if ((int)keyCode < 330 || (int)keyCode > 349)
		{
			return false;
		}
		if ((int)Application.platform != 1 && (int)Application.platform != 45 && (int)Application.platform != 0)
		{
			return s_keyCodeToGamepadButtonMap.TryGetValue(keyCode, out result);
		}
		switch (keyCode - 335)
		{
		case 0:
			result = (GamepadButton)0;
			return true;
		case 1:
			result = (GamepadButton)1;
			return true;
		case 2:
			result = (GamepadButton)2;
			return true;
		case 3:
			result = (GamepadButton)3;
			return true;
		case 4:
			result = (GamepadButton)12;
			return true;
		default:
			return false;
		}
	}

	private static GamepadInput KeyCodeToGamepadInput(KeyCode keyCode, bool logWarning = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (TryKeyCodeToGamepadInput(keyCode, out var result))
		{
			return result;
		}
		if (logWarning)
		{
			Debug.LogWarning((object)$"The KeyCode: {keyCode}, lacks a proper counterpart in the GamepadButton enum. Returning South Button.");
		}
		return result;
	}

	private static bool TryKeyCodeToGamepadInput(KeyCode keyCode, out GamepadInput result)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected I4, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		result = GamepadInput.FaceButtonA;
		if ((int)keyCode < 330 || (int)keyCode > 349)
		{
			return false;
		}
		if ((int)Application.platform == 1 || (int)Application.platform == 45 || (int)Application.platform == 0)
		{
			switch (keyCode - 335)
			{
			case 0:
				result = GamepadInput.DPadUp;
				return true;
			case 1:
				result = GamepadInput.DPadDown;
				return true;
			case 2:
				result = GamepadInput.DPadLeft;
				return true;
			case 3:
				result = GamepadInput.DPadRight;
				return true;
			case 4:
				result = GamepadInput.Start;
				return true;
			}
		}
		return s_keyCodeToGamepadInputMap.TryGetValue(keyCode, out result);
	}

	public static string KeyCodeToDisplayName(KeyCode keyCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		ButtonControl val = (ButtonControl)InputSystem.FindControl(KeyCodeToPath(keyCode));
		return (((int)val != 0) ? ((InputControl)val).displayName : null) ?? ("$KeyCode " + ((object)(KeyCode)(ref keyCode)).ToString() + " did not have corresponding ButtonControl");
	}

	private static string KeyCodeToPath(KeyCode keyCode, bool logWarning = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (TryKeyCodeToGamepadInput(keyCode, out var result))
		{
			return CollectionExtensions.GetValueOrDefault<GamepadInput, string>((IReadOnlyDictionary<GamepadInput, string>)s_gamepadInputPathMap, result, "<Gamepad>/None");
		}
		if (TryKeyCodeToMouseButton(keyCode, out var result2))
		{
			return MouseButtonToPath(result2);
		}
		if (TryKeyCodeToKey(keyCode, out var result3))
		{
			return KeyToPath(result3);
		}
		if (logWarning)
		{
			Debug.LogError((object)("No corresponding path for KeyCode \"" + ((object)(KeyCode)(ref keyCode)).ToString() + "\" found!"));
		}
		return KeyToPath((Key)0);
	}

	private static string MouseButtonToPath(MouseButton btn, bool full = true)
	{
		if (!full)
		{
			return ((object)(MouseButton)(ref btn)).ToString().ToLower() + "Button";
		}
		return "<Mouse>/" + ((object)(MouseButton)(ref btn)).ToString().ToLower() + "Button";
	}

	private static string KeyToPath(Key key)
	{
		string text = ((object)(Key)(ref key)).ToString();
		if (text != null)
		{
			if (text.Contains("Digit"))
			{
				return $"<Keyboard>/{text[text.Length - 1]}";
			}
			if (text.Contains("Apple"))
			{
				return "<Keyboard>/" + text.Substring(0, text.IndexOf("A", StringComparison.Ordinal)) + "Meta";
			}
			if (text.Contains("Windows"))
			{
				return "<Keyboard>/" + text.Substring(0, text.IndexOf("W", StringComparison.Ordinal)) + "Meta";
			}
			if (text.Contains("Command"))
			{
				return "<Keyboard>/" + text.Substring(0, text.IndexOf("C", StringComparison.Ordinal)) + "Meta";
			}
		}
		return "<Keyboard>/" + text;
	}

	private void AddValue(string name, string path, string interactions = null, string processors = null)
	{
		ValueDef value = new ValueDef(name, path, interactions, processors);
		m_values.Add(name, value);
	}

	private void AddButton(string name, string path, bool altKey = false, bool showHints = true, bool rebindable = false, float repeatDelay = 0f, float repeatInterval = 0f)
	{
		ButtonDef buttonDef = new ButtonDef(name, path, altKey, showHints, rebindable, repeatDelay, repeatInterval, 0.4f.ToGlobalInvariantString());
		m_buttons.Add(name, buttonDef);
		SubscribeButton(buttonDef);
	}

	private void SetUpMouseAndStickHandlers()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		m_mouseDelta = new InputAction("MouseDelta", (InputActionType)0, "<Mouse>/delta", (string)null, "scaleVector2(x=0.05,y=0.05)", (string)null);
		m_mouseDelta.Enable();
		m_mousePosition = new InputAction("MousePosition", (InputActionType)0, "<Mouse>/position", (string)null, (string)null, (string)null);
		m_mousePosition.Enable();
		m_mouseScrollDeltaAction = new InputAction("MouseScrollDelta", (InputActionType)0, "<Mouse>/scroll/y", (string)null, "scale(factor=0.15)", (string)null);
		m_mouseScrollDeltaAction.Enable();
		m_radialTap = new InputAction("RadialTap", (InputActionType)0, "<Gamepad>/rightStick", "vector2Tap", "stickDeadzone", (string)null);
		m_radialTap.Enable();
		m_radialMultiTap = new InputAction("RadialMultiTap", (InputActionType)0, "<Gamepad>/rightStick", "vector2MultiTap", "stickDeadzone", (string)null);
		m_radialMultiTap.Enable();
	}

	private void SetUpStickHandlers()
	{
		AddValue("StickRHorizontal", s_gamepadInputPathMap[GamepadInput.StickRHorizontal]);
		AddValue("StickRVertical", s_gamepadInputPathMap[GamepadInput.StickRVertical]);
		AddValue("StickLHorizontal", s_gamepadInputPathMap[GamepadInput.StickLHorizontal]);
		AddValue("StickLVertical", s_gamepadInputPathMap[GamepadInput.StickLVertical]);
		AddValue("StickL", s_gamepadInputPathMap[GamepadInput.StickL]);
		AddValue("StickR", s_gamepadInputPathMap[GamepadInput.StickR]);
		AddValue("RadialStick", s_gamepadInputPathMap[GamepadInput.StickR]);
		AddValue("TriggerL", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddValue("TriggerR", s_gamepadInputPathMap[GamepadInput.TriggerR]);
	}

	private void ResetKBMButtons()
	{
		AddButton("MouseLeft", MouseButtonToPath((MouseButton)0));
		AddButton("MouseRight", MouseButtonToPath((MouseButton)1));
		AddButton("MouseMiddle", MouseButtonToPath((MouseButton)2));
		AddButton("MouseForward", MouseButtonToPath((MouseButton)3));
		AddButton("MouseBack", MouseButtonToPath((MouseButton)4));
		AddButton("Attack", MouseButtonToPath((MouseButton)0), altKey: false, showHints: true, rebindable: true);
		AddButton("SecondaryAttack", MouseButtonToPath((MouseButton)2), altKey: false, showHints: true, rebindable: true);
		AddButton("Block", MouseButtonToPath((MouseButton)1), altKey: false, showHints: true, rebindable: true);
		AddButton("Use", KeyToPath((Key)19), altKey: false, showHints: true, rebindable: true);
		AddButton("Hide", KeyToPath((Key)32), altKey: false, showHints: true, rebindable: true);
		AddButton("Jump", KeyToPath((Key)1), altKey: false, showHints: true, rebindable: true);
		AddButton("Crouch", KeyToPath((Key)55), altKey: false, showHints: true, rebindable: true);
		AddButton("Run", KeyToPath((Key)51), altKey: false, showHints: true, rebindable: true);
		AddButton("ToggleWalk", KeyToPath((Key)17), altKey: false, showHints: true, rebindable: true);
		AddButton("AutoRun", KeyToPath((Key)31), altKey: false, showHints: true, rebindable: true);
		AddButton("Sit", KeyToPath((Key)38), altKey: false, showHints: true, rebindable: true);
		AddButton("GP", KeyToPath((Key)20), altKey: false, showHints: true, rebindable: true);
		AddButton("AltPlace", KeyToPath((Key)51), altKey: false, showHints: true, rebindable: true);
		AddButton("CamZoomIn", KeyToPath((Key)0));
		AddButton("CamZoomOut", KeyToPath((Key)0));
		AddButton("Forward", KeyToPath((Key)37), altKey: false, showHints: true, rebindable: true, 0.3f, 0.1f);
		AddButton("Left", KeyToPath((Key)15), altKey: false, showHints: true, rebindable: true, 0.3f, 0.1f);
		AddButton("Backward", KeyToPath((Key)33), altKey: false, showHints: true, rebindable: true, 0.3f, 0.1f);
		AddButton("Right", KeyToPath((Key)18), altKey: false, showHints: true, rebindable: true, 0.3f, 0.1f);
		AddButton("Inventory", KeyToPath((Key)3), altKey: false, showHints: true, rebindable: true);
		AddButton("Map", KeyToPath((Key)27), altKey: false, showHints: true, rebindable: true);
		AddButton("MapZoomOut", KeyToPath((Key)7), altKey: false, showHints: true, rebindable: true);
		AddButton("MapZoomIn", KeyToPath((Key)8), altKey: false, showHints: true, rebindable: true);
		AddButton("Escape", KeyToPath((Key)60));
		AddButton("TabLeft", KeyToPath((Key)31), altKey: false, showHints: true, rebindable: true);
		AddButton("TabRight", KeyToPath((Key)19), altKey: false, showHints: true, rebindable: true);
		AddButton("BuildMenu", MouseButtonToPath((MouseButton)1), altKey: false, showHints: true, rebindable: true);
		AddButton("Remove", MouseButtonToPath((MouseButton)2), altKey: false, showHints: true, rebindable: true);
		AddButton("AutoPickup", KeyToPath((Key)36), altKey: false, showHints: true, rebindable: true);
		AddButton("ScrollChatUp", KeyToPath((Key)67), altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("ScrollChatDown", KeyToPath((Key)66), altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("ChatUp", KeyToPath((Key)63), altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("ChatDown", KeyToPath((Key)64), altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("Console", KeyToPath((Key)98), altKey: false, showHints: true, rebindable: true);
		AddButton("OpenEmote", KeyToPath((Key)34), altKey: false, showHints: true, rebindable: true);
		AddButton("RadialSecondaryInteract", MouseButtonToPath((MouseButton)1));
		AddButton("Chat", KeyToPath((Key)2));
		AddButton("Hotbar1", KeyToPath((Key)41));
		AddButton("Hotbar2", KeyToPath((Key)42));
		AddButton("Hotbar3", KeyToPath((Key)43));
		AddButton("Hotbar4", KeyToPath((Key)44));
		AddButton("Hotbar5", KeyToPath((Key)45));
		AddButton("Hotbar6", KeyToPath((Key)46));
		AddButton("Hotbar7", KeyToPath((Key)47));
		AddButton("Hotbar8", KeyToPath((Key)48));
		AddButton("Tab", KeyToPath((Key)3));
		AddButton("LShift", KeyToPath((Key)51));
	}

	private void ResetGamepadButtonsGeneric()
	{
		AddButton("JoyButtonA", s_gamepadInputPathMap[GamepadInput.FaceButtonA], altKey: false, showHints: false);
		AddButton("JoyButtonB", s_gamepadInputPathMap[GamepadInput.FaceButtonB], altKey: false, showHints: false);
		AddButton("JoyButtonX", s_gamepadInputPathMap[GamepadInput.FaceButtonX], altKey: false, showHints: false);
		AddButton("JoyButtonY", s_gamepadInputPathMap[GamepadInput.FaceButtonY], altKey: false, showHints: false);
		AddButton("JoyBack", s_gamepadInputPathMap[GamepadInput.Select], altKey: false, showHints: false);
		AddButton("JoyStart", s_gamepadInputPathMap[GamepadInput.Start]);
		AddButton("JoyLStick", s_gamepadInputPathMap[GamepadInput.StickLButton], altKey: false, showHints: false);
		AddButton("JoyRStick", s_gamepadInputPathMap[GamepadInput.StickRButton], altKey: false, showHints: false);
		AddButton("JoyLStickLeft", s_gamepadInputPathMap[GamepadInput.StickLLeft], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyLStickRight", s_gamepadInputPathMap[GamepadInput.StickLRight], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyLStickUp", s_gamepadInputPathMap[GamepadInput.StickLUp], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyLStickDown", s_gamepadInputPathMap[GamepadInput.StickLDown], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyRStickLeft", s_gamepadInputPathMap[GamepadInput.StickRLeft], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyRStickRight", s_gamepadInputPathMap[GamepadInput.StickRRight], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyRStickUp", s_gamepadInputPathMap[GamepadInput.StickRUp], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyRStickDown", s_gamepadInputPathMap[GamepadInput.StickRDown], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyDPadLeft", s_gamepadInputPathMap[GamepadInput.DPadLeft], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyDPadRight", s_gamepadInputPathMap[GamepadInput.DPadRight], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyDPadUp", s_gamepadInputPathMap[GamepadInput.DPadUp], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyDPadDown", s_gamepadInputPathMap[GamepadInput.DPadDown], altKey: false, showHints: false, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyLBumper", s_gamepadInputPathMap[GamepadInput.BumperL], altKey: false, showHints: false);
		AddButton("JoyRBumper", s_gamepadInputPathMap[GamepadInput.BumperR], altKey: false, showHints: false);
		AddButton("JoyLTrigger", s_gamepadInputPathMap[GamepadInput.TriggerL], altKey: false, showHints: false);
		AddButton("JoyRTrigger", s_gamepadInputPathMap[GamepadInput.TriggerR], altKey: false, showHints: false);
		AddButton("JoyMap", s_gamepadInputPathMap[GamepadInput.Select]);
		AddButton("JoyChat", s_gamepadInputPathMap[GamepadInput.Select], altKey: true);
		AddButton("JoyMenu", s_gamepadInputPathMap[GamepadInput.Start]);
		AddButton("JoyToggleHUD", s_gamepadInputPathMap[GamepadInput.Start], altKey: true);
	}

	private void ResetGamepadToClassic()
	{
		AddButton("JoyBuildMenu", s_gamepadInputPathMap[GamepadInput.FaceButtonA]);
		AddButton("JoyUse", s_gamepadInputPathMap[GamepadInput.FaceButtonA]);
		AddButton("JoyHide", s_gamepadInputPathMap[GamepadInput.StickRButton]);
		AddButton("JoyJump", s_gamepadInputPathMap[GamepadInput.FaceButtonB]);
		AddButton("JoyDodge", s_gamepadInputPathMap[GamepadInput.FaceButtonB], altKey: true);
		AddButton("JoySit", s_gamepadInputPathMap[GamepadInput.FaceButtonX]);
		AddButton("JoyGP", s_gamepadInputPathMap[GamepadInput.DPadDown]);
		AddButton("JoyInventory", s_gamepadInputPathMap[GamepadInput.FaceButtonY]);
		AddButton("JoyRun", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyCrouch", s_gamepadInputPathMap[GamepadInput.StickLButton]);
		AddButton("JoyHotbarUse", s_gamepadInputPathMap[GamepadInput.DPadUp]);
		AddButton("JoyCamZoomIn", s_gamepadInputPathMap[GamepadInput.DPadUp], altKey: true);
		AddButton("JoyCamZoomOut", s_gamepadInputPathMap[GamepadInput.DPadDown], altKey: true);
		AddButton("JoyMiniMapZoomIn", s_gamepadInputPathMap[GamepadInput.DPadRight], altKey: true);
		AddButton("JoyMiniMapZoomOut", s_gamepadInputPathMap[GamepadInput.DPadLeft], altKey: true);
		AddButton("JoyHotbarLeft", s_gamepadInputPathMap[GamepadInput.DPadLeft], altKey: false, showHints: true, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyHotbarRight", s_gamepadInputPathMap[GamepadInput.DPadRight], altKey: false, showHints: true, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyAutoPickup", s_gamepadInputPathMap[GamepadInput.StickLButton], altKey: true);
		AddButton("JoyBlock", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyAttack", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoySecondaryAttack", s_gamepadInputPathMap[GamepadInput.BumperR]);
		AddButton("JoyRadial", s_gamepadInputPathMap[GamepadInput.StickRButton]);
		AddButton("JoyRadialInteract", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoyRadialBack", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyRadialClose", s_gamepadInputPathMap[GamepadInput.FaceButtonB]);
		AddButton("JoyAltPlace", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyRotate", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyPlace", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoyRemove", s_gamepadInputPathMap[GamepadInput.BumperR]);
		AddButton("JoyTabLeft", s_gamepadInputPathMap[GamepadInput.BumperL], altKey: false, showHints: false);
		AddButton("JoyTabRight", s_gamepadInputPathMap[GamepadInput.BumperR], altKey: false, showHints: false);
		AddButton("JoyNextSnap", s_gamepadInputPathMap[GamepadInput.StickRButton]);
		AddButton("JoyPrevSnap", s_gamepadInputPathMap[GamepadInput.StickLButton]);
		AddButton("JoyAltKeys", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyScrollChatUp", s_gamepadInputPathMap[GamepadInput.StickRUp], altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("JoyScrollChatDown", s_gamepadInputPathMap[GamepadInput.StickRDown], altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
	}

	private void ResetGamepadToAlt1()
	{
		AddButton("JoyJump", s_gamepadInputPathMap[GamepadInput.FaceButtonA]);
		AddButton("JoyUse", s_gamepadInputPathMap[GamepadInput.FaceButtonX]);
		AddButton("JoyInventory", s_gamepadInputPathMap[GamepadInput.FaceButtonY]);
		AddButton("JoyBlock", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyAttack", s_gamepadInputPathMap[GamepadInput.BumperR]);
		AddButton("JoyHide", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoySecondaryAttack", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoyRun", s_gamepadInputPathMap[GamepadInput.StickLButton]);
		AddButton("JoyCrouch", s_gamepadInputPathMap[GamepadInput.StickRButton]);
		AddButton("JoyHotbarLeft", s_gamepadInputPathMap[GamepadInput.DPadLeft], altKey: false, showHints: true, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyHotbarRight", s_gamepadInputPathMap[GamepadInput.DPadRight], altKey: false, showHints: true, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyHotbarUse", s_gamepadInputPathMap[GamepadInput.DPadUp]);
		AddButton("JoySit", s_gamepadInputPathMap[GamepadInput.DPadDown]);
		AddButton("JoyRadial", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyRadialInteract", s_gamepadInputPathMap[GamepadInput.BumperR]);
		AddButton("JoyRadialBack", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyRadialClose", s_gamepadInputPathMap[GamepadInput.FaceButtonB]);
		AddButton("JoyAltKeys", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyCamZoomIn", s_gamepadInputPathMap[GamepadInput.DPadUp], altKey: true);
		AddButton("JoyCamZoomOut", s_gamepadInputPathMap[GamepadInput.DPadDown], altKey: true);
		AddButton("JoyMiniMapZoomIn", s_gamepadInputPathMap[GamepadInput.DPadRight], altKey: true);
		AddButton("JoyMiniMapZoomOut", s_gamepadInputPathMap[GamepadInput.DPadLeft], altKey: true);
		AddButton("JoyBuildMenu", s_gamepadInputPathMap[GamepadInput.FaceButtonB]);
		AddButton("JoyPlace", s_gamepadInputPathMap[GamepadInput.BumperR]);
		AddButton("JoyRemove", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyRotate", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyRotateRight", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoyNextSnap", s_gamepadInputPathMap[GamepadInput.StickRButton]);
		AddButton("JoyPrevSnap", s_gamepadInputPathMap[GamepadInput.StickLButton]);
		AddButton("JoyAltPlace", s_gamepadInputPathMap[GamepadInput.StickRButton], altKey: true);
		AddButton("JoyDodge", s_gamepadInputPathMap[GamepadInput.FaceButtonB], altKey: true);
		AddButton("JoyTabLeft", s_gamepadInputPathMap[GamepadInput.BumperL], altKey: false, showHints: false);
		AddButton("JoyTabRight", s_gamepadInputPathMap[GamepadInput.BumperR], altKey: false, showHints: false);
		AddButton("JoyScrollChatUp", s_gamepadInputPathMap[GamepadInput.StickRUp], altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("JoyScrollChatDown", s_gamepadInputPathMap[GamepadInput.StickRDown], altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("JoyAutoPickup", s_gamepadInputPathMap[GamepadInput.StickLButton], altKey: true);
	}

	private void ResetGamepadToAlt2()
	{
		AddButton("JoyJump", s_gamepadInputPathMap[GamepadInput.FaceButtonA]);
		AddButton("JoyDodge", s_gamepadInputPathMap[GamepadInput.FaceButtonB], altKey: true);
		AddButton("JoyUse", s_gamepadInputPathMap[GamepadInput.FaceButtonX]);
		AddButton("JoyInventory", s_gamepadInputPathMap[GamepadInput.FaceButtonY]);
		AddButton("JoyHide", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoySecondaryAttack", s_gamepadInputPathMap[GamepadInput.BumperR]);
		AddButton("JoyBlock", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyAttack", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoyRun", s_gamepadInputPathMap[GamepadInput.StickLButton]);
		AddButton("JoyCrouch", s_gamepadInputPathMap[GamepadInput.StickRButton]);
		AddButton("JoyHotbarLeft", s_gamepadInputPathMap[GamepadInput.DPadLeft], altKey: false, showHints: true, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyHotbarRight", s_gamepadInputPathMap[GamepadInput.DPadRight], altKey: false, showHints: true, rebindable: false, 0.3f, 0.1f);
		AddButton("JoyHotbarUse", s_gamepadInputPathMap[GamepadInput.DPadUp]);
		AddButton("JoySit", s_gamepadInputPathMap[GamepadInput.DPadDown]);
		AddButton("JoyRadial", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyRadialInteract", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoyRadialBack", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyRadialClose", s_gamepadInputPathMap[GamepadInput.FaceButtonB]);
		AddButton("JoyAltKeys", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyCamZoomIn", s_gamepadInputPathMap[GamepadInput.DPadUp], altKey: true);
		AddButton("JoyCamZoomOut", s_gamepadInputPathMap[GamepadInput.DPadDown], altKey: true);
		AddButton("JoyMiniMapZoomIn", s_gamepadInputPathMap[GamepadInput.DPadRight], altKey: true);
		AddButton("JoyMiniMapZoomOut", s_gamepadInputPathMap[GamepadInput.DPadLeft], altKey: true);
		AddButton("JoyBuildMenu", s_gamepadInputPathMap[GamepadInput.FaceButtonB]);
		AddButton("JoyPlace", s_gamepadInputPathMap[GamepadInput.BumperR]);
		AddButton("JoyRemove", s_gamepadInputPathMap[GamepadInput.BumperL]);
		AddButton("JoyRotate", s_gamepadInputPathMap[GamepadInput.TriggerL]);
		AddButton("JoyRotateRight", s_gamepadInputPathMap[GamepadInput.TriggerR]);
		AddButton("JoyNextSnap", s_gamepadInputPathMap[GamepadInput.StickRButton]);
		AddButton("JoyPrevSnap", s_gamepadInputPathMap[GamepadInput.StickLButton]);
		AddButton("JoyAltPlace", s_gamepadInputPathMap[GamepadInput.StickRButton], altKey: true);
		AddButton("JoyTabLeft", s_gamepadInputPathMap[GamepadInput.BumperL], altKey: false, showHints: false);
		AddButton("JoyTabRight", s_gamepadInputPathMap[GamepadInput.BumperR], altKey: false, showHints: false);
		AddButton("JoyScrollChatUp", s_gamepadInputPathMap[GamepadInput.StickRUp], altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("JoyScrollChatDown", s_gamepadInputPathMap[GamepadInput.StickRDown], altKey: false, showHints: true, rebindable: false, 0.5f, 0.05f);
		AddButton("JoyAutoPickup", s_gamepadInputPathMap[GamepadInput.StickLButton], altKey: true);
	}

	private static bool ShouldAcceptInputFromSource(InputSource inputSource)
	{
		if (inputSource == InputSource.Gamepad && (m_blockGamePadInput > 0f || VirtualKeyboardOpen))
		{
			return false;
		}
		if (!IsInputSwitchingModeValid())
		{
			ZLog.LogWarning($"Input switching mode {s_inputSwitchingMode} invalid! Accepting all input!");
			return true;
		}
		if (!s_inputSwitchingMode.HasFlag(InputSource.BlockingBit))
		{
			return true;
		}
		return s_inputSwitchingMode.HasFlag(inputSource & InputSource.AllowedInputMask);
	}

	private static bool IsInputSwitchingModeValid()
	{
		if (!s_isInputSwitchingModeValid.HasValue)
		{
			s_isInputSwitchingModeValid = Enum.IsDefined(typeof(InputSource), s_inputSwitchingMode) && s_inputSwitchingMode.HasFlag(InputSource.InputSwitchingModeBit | InputSource.ValidBit);
		}
		return s_isInputSwitchingModeValid.Value;
	}

	public static bool IsNonClassicFunctionality()
	{
		InputLayout inputLayout = InputLayout;
		return inputLayout == InputLayout.Alternative1 || inputLayout == InputLayout.Alternative2;
	}

	public static bool IsGamepadEnabled()
	{
		return s_inputSwitchingMode.HasFlag(InputSource.AllowGamepadInputBit);
	}

	public static bool IsGamepadActive()
	{
		if (m_virtualKeyboardOpen)
		{
			return false;
		}
		if (m_instance == null || !IsGamepadEnabled())
		{
			return false;
		}
		return m_inputSource == InputSource.Gamepad;
	}

	public static bool IsKeyboardAvailable()
	{
		if (Keyboard.current != null)
		{
			return !m_virtualKeyboardOpen;
		}
		return false;
	}

	public static void CheckKeyboardMouseConnected(out bool keyboardConnected, out bool mouseConnected)
	{
		bool flag = m_inputSource == InputSource.KeyboardMouse;
		keyboardConnected = flag || Keyboard.current != null;
		mouseConnected = flag || Mouse.current != null;
	}

	public static bool IsMouseActive()
	{
		return m_instance.Internal_IsMouseActive();
	}

	private bool Internal_IsMouseActive()
	{
		if (Mouse.current != null)
		{
			return m_inputSource == InputSource.KeyboardMouse;
		}
		return false;
	}
}
