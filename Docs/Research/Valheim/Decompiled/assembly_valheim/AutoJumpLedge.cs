using UnityEngine;

public class AutoJumpLedge : MonoBehaviour
{
	public bool m_forwardOnly = true;

	public float m_upVel = 1f;

	public float m_forwardVel = 1f;

	private void OnTriggerStay(Collider collider)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Character component = ((Component)collider).GetComponent<Character>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.OnAutoJump(((Component)this).transform.forward, m_upVel, m_forwardVel);
		}
	}
}
