using System.Collections.Generic;
using UnityEngine;

public class Petable : MonoBehaviour, Hoverable, Interactable
{
	public string m_name = "";

	public Transform m_effectLocation;

	public List<string> m_randomPetTexts = new List<string>();

	public EffectList m_petEffect = new EffectList();

	private float m_lastPetTime;

	public string GetHoverText()
	{
		return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_name);
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time - m_lastPetTime > 1f)
		{
			m_lastPetTime = Time.time;
			m_petEffect.Create(Object.op_Implicit((Object)(object)m_effectLocation) ? m_effectLocation.position : ((Component)this).transform.position, Object.op_Implicit((Object)(object)m_effectLocation) ? m_effectLocation.rotation : ((Component)this).transform.rotation);
			user.Message(MessageHud.MessageType.Center, m_name + " " + m_randomPetTexts[Random.Range(0, m_randomPetTexts.Count)]);
			return true;
		}
		return false;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}
}
