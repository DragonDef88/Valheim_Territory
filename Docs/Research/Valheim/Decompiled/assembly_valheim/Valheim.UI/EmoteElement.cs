using UnityEngine;

namespace Valheim.UI;

public class EmoteElement : RadialMenuElement
{
	public void Init(EmoteDataMapping mapping)
	{
		if (mapping.Emote == Emotes.Count)
		{
			base.Name = "";
			base.Interact = null;
		}
		else
		{
			base.Name = ((!string.IsNullOrEmpty(mapping.LocaString)) ? Localization.instance.Localize("$" + mapping.LocaString) : mapping.Emote.ToString());
			base.Interact = delegate
			{
				Emote.DoEmote(mapping.Emote);
				return true;
			};
		}
		base.CloseOnInteract = () => true;
		((Component)m_icon).gameObject.SetActive((Object)(object)mapping.Sprite != (Object)null);
		m_icon.sprite = mapping.Sprite;
	}
}
