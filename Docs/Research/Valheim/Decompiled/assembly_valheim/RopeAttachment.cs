using UnityEngine;

public class RopeAttachment : MonoBehaviour, Interactable, Hoverable
{
	public string m_name = "Rope";

	public string m_hoverText = "Pull";

	public float m_pullDistance = 5f;

	public float m_pullForce = 1f;

	public float m_maxPullVel = 1f;

	private Rigidbody m_boatBody;

	private Character m_puller;

	private void Awake()
	{
		m_boatBody = ((Component)this).GetComponentInParent<Rigidbody>();
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_puller))
		{
			m_puller = null;
			ZLog.Log((object)"Detached rope");
		}
		else
		{
			m_puller = character;
			ZLog.Log((object)"Attached rope");
		}
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	public string GetHoverText()
	{
		return m_hoverText;
	}

	public string GetHoverName()
	{
		return m_name;
	}

	private void FixedUpdate()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_puller) && Vector3.Distance(((Component)m_puller).transform.position, ((Component)this).transform.position) > m_pullDistance)
		{
			Vector3 val = ((Component)m_puller).transform.position - ((Component)this).transform.position;
			Vector3 val2 = (((Vector3)(ref val)).normalized * m_maxPullVel - m_boatBody.GetPointVelocity(((Component)this).transform.position)) * m_pullForce;
			m_boatBody.AddForceAtPosition(((Component)this).transform.position, val2);
		}
	}
}
