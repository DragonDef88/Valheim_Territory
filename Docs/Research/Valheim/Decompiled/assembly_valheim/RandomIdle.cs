using UnityEngine;

public class RandomIdle : StateMachineBehaviour
{
	public int m_animations = 4;

	public int m_animationsWhenTamed;

	public string m_valueName = "";

	public int m_alertedIdle = -1;

	private float m_last;

	private bool m_haveSetup;

	private BaseAI m_baseAI;

	private Character m_character;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int randomIdle = GetRandomIdle(animator);
		animator.SetFloat(m_valueName, (float)randomIdle);
		m_last = ((AnimatorStateInfo)(ref stateInfo)).normalizedTime % 1f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float num = ((AnimatorStateInfo)(ref stateInfo)).normalizedTime % 1f;
		if (num < m_last)
		{
			int randomIdle = GetRandomIdle(animator);
			animator.SetFloat(m_valueName, (float)randomIdle);
		}
		m_last = num;
	}

	private int GetRandomIdle(Animator animator)
	{
		if (!m_haveSetup)
		{
			m_haveSetup = true;
			m_baseAI = ((Component)animator).GetComponentInParent<BaseAI>();
			m_character = ((Component)animator).GetComponentInParent<Character>();
		}
		if (Object.op_Implicit((Object)(object)m_baseAI) && m_alertedIdle >= 0 && m_baseAI.IsAlerted())
		{
			return m_alertedIdle;
		}
		return Random.Range(0, (m_animationsWhenTamed > 0 && (Object)(object)m_character != (Object)null && m_character.IsTamed()) ? m_animationsWhenTamed : m_animations);
	}
}
