using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class GuildIconElement : MonoBehaviour
{
	public Button guildIconButton;

	public RectTransform guildIconButtonRect;

	public Image guildIconBkg;

	public RectTransform guildIconBkgRect;

	public Image guildIcon;

	public RectTransform guildIconRect;

	public GuildIconUI guildIconUI;

	public int guildIconId;

	public void OnGuildIconElement_Clicked()
	{
		guildIconUI.selectedGuildIcon(guildIconId);
		((Component)guildIconUI).gameObject.SetActive(false);
	}
}
