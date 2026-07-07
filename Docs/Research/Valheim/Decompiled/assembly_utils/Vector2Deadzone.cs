using UnityEngine;
using UnityEngine.InputSystem;

public class Vector2Deadzone : InputProcessor<Vector2>
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

	public override Vector2 Process(Vector2 value, InputControl control)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(ProcessAxis(value.x), ProcessAxis(value.y));
	}

	private float ProcessAxis(float value)
	{
		float num = Mathf.Sign(value);
		value = Mathf.Abs(value);
		value = Mathf.Clamp01(value - DeadzoneOrDefault);
		value *= 1f / (1f - DeadzoneOrDefault);
		return value * num;
	}
}
