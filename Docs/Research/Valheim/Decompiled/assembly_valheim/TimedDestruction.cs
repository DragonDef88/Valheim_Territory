using UnityEngine;

public class TimedDestruction : MonoBehaviour
{
	public float m_timeout = 1f;

	public bool m_triggerOnAwake;

	[Tooltip("If there are objects that you always want to destroy, even if there is no owner, check this. For instance, fires in the ashlands may be created by cinder rain outside of ownership-zones, so they must be deleted even if no owner exists.")]
	public bool m_forceTakeOwnershipAndDestroy;

	private ZNetView m_nview;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_triggerOnAwake)
		{
			Trigger();
		}
	}

	public void Trigger()
	{
		((MonoBehaviour)this).InvokeRepeating("DestroyNow", m_timeout, 1f);
	}

	public void Trigger(float timeout)
	{
		((MonoBehaviour)this).InvokeRepeating("DestroyNow", timeout, 1f);
	}

	private void DestroyNow()
	{
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			if (m_nview.IsValid())
			{
				if (!m_nview.HasOwner() && m_forceTakeOwnershipAndDestroy)
				{
					m_nview.ClaimOwnership();
				}
				if (m_nview.IsOwner())
				{
					ZNetScene.instance.Destroy(((Component)this).gameObject);
				}
			}
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
