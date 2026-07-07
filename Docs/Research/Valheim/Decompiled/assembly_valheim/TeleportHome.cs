using UnityEngine;

public class TeleportHome : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		Player component = ((Component)collider).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)Player.m_localPlayer != (Object)(object)component))
		{
			Game.instance.RequestRespawn(0f);
		}
	}
}
