using UnityEngine;
using UnityEngine.InputSystem;

public class Vector2Tap : IInputInteraction<Vector2>, IInputInteraction
{
	public float duration;

	internal double m_TapStartTime;

	internal bool m_waitingOnRelease;

	internal float DurationOrDefault
	{
		get
		{
			if (duration == 0f)
			{
				return 0.1f;
			}
			return duration;
		}
	}

	public void Process(ref InputInteractionContext context)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (((InputInteractionContext)(ref context)).timerHasExpired)
		{
			((InputInteractionContext)(ref context)).Canceled();
			return;
		}
		Vector2 val;
		if (((InputInteractionContext)(ref context)).isWaiting && !((InputInteractionContext)(ref context)).isStarted)
		{
			if (!m_waitingOnRelease)
			{
				val = ((InputInteractionContext)(ref context)).ReadValue<Vector2>();
				if (((Vector2)(ref val)).magnitude >= 1f)
				{
					StartTap(ref context);
					return;
				}
			}
			val = ((InputInteractionContext)(ref context)).ReadValue<Vector2>();
			if (((Vector2)(ref val)).magnitude <= 0f)
			{
				m_waitingOnRelease = false;
				return;
			}
		}
		if (((InputInteractionContext)(ref context)).isStarted && m_waitingOnRelease)
		{
			val = ((InputInteractionContext)(ref context)).ReadValue<Vector2>();
			if (((Vector2)(ref val)).magnitude <= 0f)
			{
				CompleteTap(ref context);
			}
		}
	}

	private void CompleteTap(ref InputInteractionContext context)
	{
		if (((InputInteractionContext)(ref context)).time - m_TapStartTime <= (double)DurationOrDefault)
		{
			m_TapStartTime = 0.0;
			((InputInteractionContext)(ref context)).Performed();
		}
		else
		{
			((InputInteractionContext)(ref context)).Canceled();
		}
	}

	private void StartTap(ref InputInteractionContext context)
	{
		m_TapStartTime = ((InputInteractionContext)(ref context)).time;
		m_waitingOnRelease = true;
		((InputInteractionContext)(ref context)).Started();
		((InputInteractionContext)(ref context)).SetTimeout(DurationOrDefault + 1E-05f);
	}

	public void Reset()
	{
		m_TapStartTime = 0.0;
	}
}
