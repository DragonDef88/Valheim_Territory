using UnityEngine;
using UnityEngine.InputSystem;

public class FloatDeadzone : InputProcessor<float>
{
	public float deadzone;

	internal float DeadzoneOrDefault
	{
		get
		{
			if (deadzone == 0f)
			{
				return InputSystem.settings.defaultDeadzoneMin;
			}
			return deadzone;
		}
	}

	public override float Process(float value, InputControl control)
	{
		float num = Mathf.Sign(value);
		value = Mathf.Abs(value);
		value = Mathf.Clamp01(value - DeadzoneOrDefault);
		value *= 1f / (1f - DeadzoneOrDefault);
		return value * num;
	}
}
