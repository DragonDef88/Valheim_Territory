using UnityEngine;

public class EventZone : MonoBehaviour
{
	public string m_event = "";

	private static EventZone m_triggered;

	private void OnTriggerStay(Collider collider)
	{
		Player component = ((Component)collider).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)Player.m_localPlayer != (Object)(object)component))
		{
			m_triggered = this;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!((Object)(object)m_triggered != (Object)(object)this))
		{
			Player component = ((Component)collider).GetComponent<Player>();
			if (!((Object)(object)component == (Object)null) && !((Object)(object)Player.m_localPlayer != (Object)(object)component))
			{
				m_triggered = null;
			}
		}
	}

	public static string GetEvent()
	{
		if (Object.op_Implicit((Object)(object)m_triggered) && m_triggered.m_event.Length > 0)
		{
			return m_triggered.m_event;
		}
		return null;
	}
}
