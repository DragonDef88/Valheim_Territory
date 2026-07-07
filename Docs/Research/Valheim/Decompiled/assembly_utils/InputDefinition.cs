using System;
using UnityEngine;

public struct InputDefinition
{
	private readonly KeyCode m_keyCode;

	private readonly string m_axisName;

	private readonly FloatRange m_axisRange;

	private readonly FloatRange m_outputRange;

	private readonly AdvancedInputMap m_advancedMap;

	private PlatformGamepadDefinition m_parentGamepad;

	private bool IsAdvanced => m_advancedMap != null;

	public bool IsDigital
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			if (IsAdvanced)
			{
				return m_advancedMap.IsDigital;
			}
			return (int)m_keyCode > 0;
		}
	}

	public bool IsAnalogue
	{
		get
		{
			if (IsAdvanced)
			{
				return m_advancedMap.IsAnalogue;
			}
			return m_axisName != null;
		}
	}

	private bool IsGamepadConnected
	{
		get
		{
			if (m_parentGamepad != null)
			{
				return m_parentGamepad.IsGamepadConnected;
			}
			return true;
		}
	}

	public InputDefinition(AdvancedInputMap advancedMap, PlatformGamepadDefinition gamepad)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (advancedMap == null)
		{
			throw new ArgumentException("Advanced map can't be null!");
		}
		m_keyCode = (KeyCode)0;
		m_axisName = null;
		m_axisRange = new FloatRange(0f, 1f);
		m_outputRange = new FloatRange(0f, 1f);
		m_advancedMap = advancedMap;
		m_parentGamepad = gamepad;
	}

	public InputDefinition(AdvancedInputMap advancedMap, FloatRange outputRange, PlatformGamepadDefinition gamepad)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (advancedMap == null)
		{
			throw new ArgumentException("Advanced map can't be null!");
		}
		m_keyCode = (KeyCode)0;
		m_axisName = null;
		m_axisRange = new FloatRange(0f, 1f);
		m_outputRange = outputRange;
		m_advancedMap = advancedMap;
		m_parentGamepad = gamepad;
	}

	public InputDefinition(string axis, PlatformGamepadDefinition gamepad)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(axis))
		{
			throw new ArgumentException("Axis name can't be empty!");
		}
		m_keyCode = (KeyCode)0;
		m_axisName = axis;
		m_axisRange = new FloatRange(0f, 1f);
		m_outputRange = new FloatRange(0f, 1f);
		m_advancedMap = null;
		m_parentGamepad = gamepad;
	}

	public InputDefinition(string axis, FloatRange axisRange, FloatRange outputRange, PlatformGamepadDefinition gamepad)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(axis))
		{
			throw new ArgumentException("Axis name can't be empty!");
		}
		m_keyCode = (KeyCode)0;
		m_axisName = axis;
		m_axisRange = axisRange;
		m_outputRange = outputRange;
		m_advancedMap = null;
		m_parentGamepad = gamepad;
	}

	public InputDefinition(KeyCode button, PlatformGamepadDefinition gamepad)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)button == 0)
		{
			throw new ArgumentException($"Keycode can't be {(object)(KeyCode)0}");
		}
		m_keyCode = button;
		m_axisName = null;
		m_axisRange = default(FloatRange);
		m_outputRange = new FloatRange(0f, 1f);
		m_advancedMap = null;
		m_parentGamepad = gamepad;
	}

	public InputDefinition(KeyCode button, FloatRange outputRange, PlatformGamepadDefinition gamepad)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)button == 0)
		{
			throw new ArgumentException($"Keycode can't be {(object)(KeyCode)0}");
		}
		m_keyCode = button;
		m_axisName = null;
		m_axisRange = default(FloatRange);
		m_outputRange = outputRange;
		m_advancedMap = null;
		m_parentGamepad = gamepad;
	}

	public float ReadAsAnalogue()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsGamepadConnected)
		{
			return 0f;
		}
		if (IsAdvanced)
		{
			return m_advancedMap.ReadAsAnalogue(m_outputRange);
		}
		if (IsAnalogue)
		{
			return FloatRange.Remap(Input.GetAxis(m_axisName), m_axisRange, m_outputRange, clamp: true);
		}
		if (!Input.GetKey(m_keyCode))
		{
			return 0f;
		}
		return m_outputRange.m_max;
	}

	public bool ReadAsDigital(float deadzone = 0.5f, FloatRange? mappedRange = null)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsGamepadConnected)
		{
			return false;
		}
		if (IsAdvanced)
		{
			return m_advancedMap.ReadAsDigital(m_outputRange, deadzone, mappedRange);
		}
		if (!IsAnalogue)
		{
			return Input.GetKey(m_keyCode);
		}
		float num = FloatRange.Remap(Input.GetAxis(m_axisName), m_axisRange, m_outputRange);
		if (mappedRange.HasValue)
		{
			return mappedRange.Value.RangeToNormalized(num) > deadzone;
		}
		return num > deadzone;
	}
}
