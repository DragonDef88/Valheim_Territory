using UnityEngine;

public class Teleport : MonoBehaviour, Hoverable, Interactable
{
	public string m_hoverText = "$location_enter";

	public string m_enterText = "";

	public Teleport m_targetPoint;

	public string GetHoverText()
	{
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + m_hoverText);
	}

	public string GetHoverName()
	{
		return "";
	}

	private void OnTriggerEnter(Collider collider)
	{
		Player component = ((Component)collider).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)Player.m_localPlayer != (Object)(object)component))
		{
			Interact(component, hold: false, alt: false);
		}
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		if ((Object)(object)m_targetPoint == (Object)null)
		{
			return false;
		}
		if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoBossPortals) && character.InInterior() && Location.IsInsideActiveBossDungeon(((Component)character).transform.position))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_blockedbyboss");
			return false;
		}
		if (character.TeleportTo(m_targetPoint.GetTeleportPoint(), ((Component)m_targetPoint).transform.rotation, distantTeleport: false))
		{
			Game.instance.IncrementPlayerStat(character.InInterior() ? PlayerStatType.PortalDungeonOut : PlayerStatType.PortalDungeonIn);
			if (m_enterText.Length > 0)
			{
				MessageHud.instance.ShowBiomeFoundMsg(m_enterText, playStinger: false);
			}
			return true;
		}
		return false;
	}

	private Vector3 GetTeleportPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + ((Component)this).transform.forward - ((Component)this).transform.up;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void OnDrawGizmos()
	{
	}
}
