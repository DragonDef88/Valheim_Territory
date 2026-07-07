using System;
using System.Collections.Generic;
using UnityEngine;

public class RuneStone : MonoBehaviour, Hoverable, Interactable
{
	[Serializable]
	public class RandomRuneText
	{
		public string m_topic = "";

		public string m_label = "";

		public string m_text = "";
	}

	public string m_name = "Rune stone";

	public string m_topic = "";

	public string m_label = "";

	[TextArea]
	public string m_text = "";

	public List<RandomRuneText> m_randomTexts;

	public string m_locationName = "";

	public string m_pinName = "Pin";

	public Minimap.PinType m_pinType = Minimap.PinType.Boss;

	public bool m_showMap;

	public string GetHoverText()
	{
		return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_rune_read");
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		Player player = character as Player;
		if (!string.IsNullOrEmpty(m_locationName))
		{
			Game.instance.DiscoverClosestLocation(m_locationName, ((Component)this).transform.position, m_pinName, (int)m_pinType, m_showMap);
		}
		RandomRuneText randomText = GetRandomText();
		if (randomText != null)
		{
			if (randomText.m_label.Length > 0)
			{
				player.AddKnownText(randomText.m_label, randomText.m_text);
			}
			TextViewer.instance.ShowText(TextViewer.Style.Rune, randomText.m_topic, randomText.m_text, autoHide: true);
		}
		else
		{
			if (m_label.Length > 0)
			{
				player.AddKnownText(m_label, m_text);
			}
			TextViewer.instance.ShowText(TextViewer.Style.Rune, m_topic, m_text, autoHide: true);
		}
		return false;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private RandomRuneText GetRandomText()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (m_randomTexts.Count == 0)
		{
			return null;
		}
		Vector3 position = ((Component)this).transform.position;
		int num = (int)position.x * (int)position.z;
		State state = Random.state;
		Random.InitState(num);
		RandomRuneText result = m_randomTexts[Random.Range(0, m_randomTexts.Count)];
		Random.state = state;
		return result;
	}
}
