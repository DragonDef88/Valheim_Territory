using UnityEngine;

public class Ladder : MonoBehaviour, Interactable, Hoverable
{
	public Transform m_targetPos;

	public string m_name = "Ladder";

	public float m_useDistance = 2f;

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		if (!InUseDistance(character))
		{
			return false;
		}
		((Component)character).transform.position = m_targetPos.position;
		((Component)character).transform.rotation = m_targetPos.rotation;
		character.SetLookDir(m_targetPos.forward);
		Physics.SyncTransforms();
		return false;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	public string GetHoverText()
	{
		if (!InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=#888888>$piece_toofar</color>");
		}
		return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	public string GetHoverName()
	{
		return m_name;
	}

	private bool InUseDistance(Humanoid human)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)human).transform.position, ((Component)this).transform.position) < m_useDistance;
	}
}
