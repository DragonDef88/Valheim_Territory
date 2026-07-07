using UnityEngine;

public class WayStone : MonoBehaviour, Hoverable, Interactable
{
	[TextArea]
	public string m_activateMessage = "You touch the cold stone surface and you think of home.";

	public GameObject m_activeObject;

	public EffectList m_activeEffect;

	private void Awake()
	{
		m_activeObject.SetActive(false);
	}

	public string GetHoverText()
	{
		if (m_activeObject.activeSelf)
		{
			return "Activated waystone";
		}
		return Localization.instance.Localize("Waystone\n[<color=yellow><b>$KEY_Use</b></color>] Activate");
	}

	public string GetHoverName()
	{
		return "Waystone";
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		if (!m_activeObject.activeSelf)
		{
			character.Message(MessageHud.MessageType.Center, m_activateMessage);
			m_activeObject.SetActive(true);
			m_activeEffect.Create(((Component)this).gameObject.transform.position, ((Component)this).gameObject.transform.rotation);
		}
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void FixedUpdate()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (m_activeObject.activeSelf && (Object)(object)Game.instance != (Object)null)
		{
			Vector3 val = GetSpawnPoint() - ((Component)this).transform.position;
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			m_activeObject.transform.rotation = Quaternion.LookRotation(val);
		}
	}

	private Vector3 GetSpawnPoint()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		if (playerProfile.HaveCustomSpawnPoint())
		{
			return playerProfile.GetCustomSpawnPoint();
		}
		return playerProfile.GetHomePoint();
	}
}
