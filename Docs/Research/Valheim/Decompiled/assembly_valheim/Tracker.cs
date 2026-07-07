using UnityEngine;

public class Tracker : MonoBehaviour
{
	private bool m_active;

	private void Awake()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		ZNetView component = ((Component)this).GetComponent<ZNetView>();
		if (Object.op_Implicit((Object)(object)component) && component.IsOwner())
		{
			m_active = true;
			ZNet.instance.SetReferencePosition(((Component)this).transform.position);
		}
	}

	public void SetActive(bool active)
	{
		m_active = active;
	}

	private void OnDestroy()
	{
		m_active = false;
	}

	private void FixedUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (m_active)
		{
			ZNet.instance.SetReferencePosition(((Component)this).transform.position);
		}
	}
}
