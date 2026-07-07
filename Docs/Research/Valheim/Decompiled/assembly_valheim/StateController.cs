using UnityEngine;

public class StateController : StateMachineBehaviour
{
	public string m_effectJoint = "";

	public EffectList m_enterEffect = new EffectList();

	public bool m_enterDisableChildren;

	public bool m_enterEnableChildren;

	public GameObject[] m_enterDisable = (GameObject[])(object)new GameObject[0];

	public GameObject[] m_enterEnable = (GameObject[])(object)new GameObject[0];

	private Transform m_effectJoinT;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (m_enterEffect.HasEffects())
		{
			m_enterEffect.Create(GetEffectPos(animator), ((Component)animator).transform.rotation);
		}
		if (m_enterDisableChildren)
		{
			for (int i = 0; i < ((Component)animator).transform.childCount; i++)
			{
				((Component)((Component)animator).transform.GetChild(i)).gameObject.SetActive(false);
			}
		}
		if (m_enterEnableChildren)
		{
			for (int j = 0; j < ((Component)animator).transform.childCount; j++)
			{
				((Component)((Component)animator).transform.GetChild(j)).gameObject.SetActive(true);
			}
		}
	}

	private Vector3 GetEffectPos(Animator animator)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (m_effectJoint.Length == 0)
		{
			return ((Component)animator).transform.position;
		}
		if ((Object)(object)m_effectJoinT == (Object)null)
		{
			m_effectJoinT = Utils.FindChild(((Component)animator).transform, m_effectJoint, (IterativeSearchType)0);
		}
		return m_effectJoinT.position;
	}
}
