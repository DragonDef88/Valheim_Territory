using UnityEngine;

public class TeleportWorldTrigger : MonoBehaviour
{
	private TeleportWorld m_teleportWorld;

	private void Awake()
	{
		m_teleportWorld = ((Component)this).GetComponentInParent<TeleportWorld>();
	}

	private void OnTriggerEnter(Collider colliderIn)
	{
		Player component = ((Component)colliderIn).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)Player.m_localPlayer != (Object)(object)component))
		{
			ZLog.Log((object)"Teleportation TRIGGER");
			m_teleportWorld.Teleport(component);
		}
	}
}
