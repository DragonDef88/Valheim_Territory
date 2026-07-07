using UnityEngine;
using UnityEngine.InputSystem;

public class Vector2MultiTap : IInputInteraction<Vector2>, IInputInteraction
{
	private enum TapPhase
	{
		None,
		WaitingForNextRelease,
		WaitingForNextPress
	}

	public float tapTime;

	public float tapDelay;

	public int tapCount;

	public bool requireReleaseForFinal;

	internal bool m_hasPressed;

	private TapPhase m_CurrentTapPhase;

	private int m_CurrentTapCount;

	private double m_CurrentTapStartTime;

	private double m_LastTapReleaseTime;

	internal float TapTimeOrDefault
	{
		get
		{
			if (tapTime == 0f)
			{
				return 0.5f;
			}
			return tapTime;
		}
	}

	internal float TapDelayOrDefault
	{
		get
		{
			if (tapDelay == 0f)
			{
				return 0.5f;
			}
			return tapDelay;
		}
	}

	internal int TapCountOrDefault
	{
		get
		{
			if (tapCount == 0)
			{
				return 2;
			}
			return tapCount;
		}
	}

	public void Process(ref InputInteractionContext context)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (((InputInteractionContext)(ref context)).timerHasExpired)
		{
			((InputInteractionContext)(ref context)).Canceled();
			return;
		}
		Vector2 val;
		switch (m_CurrentTapPhase)
		{
		case TapPhase.None:
			if (!m_hasPressed)
			{
				val = ((InputInteractionContext)(ref context)).ReadValue<Vector2>();
				if (((Vector2)(ref val)).magnitude >= 1f)
				{
					StartTapSequence(ref context);
					break;
				}
			}
			val = ((InputInteractionContext)(ref context)).ReadValue<Vector2>();
			if (((Vector2)(ref val)).magnitude <= 0f)
			{
				m_hasPressed = false;
			}
			break;
		case TapPhase.WaitingForNextRelease:
			val = ((InputInteractionContext)(ref context)).ReadValue<Vector2>();
			if (((Vector2)(ref val)).magnitude <= 0f)
			{
				HandleRelease(ref context);
			}
			break;
		case TapPhase.WaitingForNextPress:
			val = ((InputInteractionContext)(ref context)).ReadValue<Vector2>();
			if (((Vector2)(ref val)).magnitude >= 1f)
			{
				HandlePress(ref context);
			}
			break;
		}
	}

	private void HandlePress(ref InputInteractionContext context)
	{
		if (((InputInteractionContext)(ref context)).time - m_LastTapReleaseTime <= (double)TapDelayOrDefault)
		{
			if (!requireReleaseForFinal && m_CurrentTapCount + 1 >= TapCountOrDefault)
			{
				((InputInteractionContext)(ref context)).Performed();
				return;
			}
			m_CurrentTapPhase = TapPhase.WaitingForNextRelease;
			m_CurrentTapStartTime = ((InputInteractionContext)(ref context)).time;
			((InputInteractionContext)(ref context)).SetTimeout(TapTimeOrDefault);
		}
		else
		{
			((InputInteractionContext)(ref context)).Canceled();
		}
	}

	private void HandleRelease(ref InputInteractionContext context)
	{
		if (((InputInteractionContext)(ref context)).time - m_CurrentTapStartTime <= (double)TapTimeOrDefault)
		{
			m_CurrentTapCount++;
			if (m_CurrentTapCount >= TapCountOrDefault)
			{
				((InputInteractionContext)(ref context)).Performed();
				return;
			}
			m_CurrentTapPhase = TapPhase.WaitingForNextPress;
			m_LastTapReleaseTime = ((InputInteractionContext)(ref context)).time;
			((InputInteractionContext)(ref context)).SetTimeout(TapDelayOrDefault);
		}
		else
		{
			((InputInteractionContext)(ref context)).Canceled();
		}
	}

	private void StartTapSequence(ref InputInteractionContext context)
	{
		m_hasPressed = true;
		m_CurrentTapPhase = TapPhase.WaitingForNextRelease;
		m_CurrentTapStartTime = ((InputInteractionContext)(ref context)).time;
		((InputInteractionContext)(ref context)).Started();
		float tapTimeOrDefault = TapTimeOrDefault;
		float tapDelayOrDefault = TapDelayOrDefault;
		((InputInteractionContext)(ref context)).SetTimeout(tapTimeOrDefault);
		((InputInteractionContext)(ref context)).SetTotalTimeoutCompletionTime(tapTimeOrDefault * (float)TapCountOrDefault + (float)(TapCountOrDefault - 1) * tapDelayOrDefault);
	}

	public void Reset()
	{
		m_CurrentTapPhase = TapPhase.None;
		m_CurrentTapCount = 0;
		m_CurrentTapStartTime = 0.0;
		m_LastTapReleaseTime = 0.0;
	}
}
