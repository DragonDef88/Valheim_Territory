using System.Collections.Generic;
using UnityEngine;

namespace Valheim.UI;

[CreateAssetMenu(fileName = "ValheimRadialConfig", menuName = "Valheim/Radial/Group Config/Main Group Config")]
public class ValheimRadialConfig : ScriptableObject, IRadialConfig
{
	public string LocalizedName => "Main";

	public Sprite Sprite => null;

	public void InitRadialConfig(RadialBase radial)
	{
		List<RadialMenuElement> list = new List<RadialMenuElement>();
		AddHotbarGroup(radial, list);
		AddItemGroup(radial, list, "consumables");
		AddItemGroup(radial, list, "weaponstools");
		AddItemGroup(radial, list, "armor_utility");
		AddEmoteGroup(radial, list);
		AddItemGroup(radial, list, "allitems");
		if ((Object)(object)Player.m_localPlayer != (Object)null && Player.m_localPlayer.GetInventory().ContainsItemByName("$item_hammer"))
		{
			AddHammer(list);
		}
		else
		{
			AddEmpty(list);
		}
		if ((Object)(object)radial.LastUsed != (Object)null)
		{
			list.Add(radial.LastUsed);
			radial.LastUsed.Hovering = 0f;
		}
		else
		{
			AddEmpty(list);
		}
		radial.ConstructRadial(list);
	}

	private void AddEmoteGroup(RadialBase radial, List<RadialMenuElement> elements)
	{
		GroupElement groupElement = Object.Instantiate<GroupElement>(RadialData.SO.GroupElement);
		EmoteGroupConfig config = Object.Instantiate<EmoteGroupConfig>(RadialData.SO.EmoteGroupConfig);
		groupElement.Init(config, this, radial);
		elements.Add(groupElement);
	}

	private void AddHotbarGroup(RadialBase radial, List<RadialMenuElement> elements)
	{
		GroupElement groupElement = Object.Instantiate<GroupElement>(RadialData.SO.GroupElement);
		HotbarGroupConfig config = Object.Instantiate<HotbarGroupConfig>(RadialData.SO.HotbarGroupConfig);
		groupElement.Init(config, this, radial);
		elements.Add(groupElement);
	}

	private void AddItemGroup(RadialBase radial, List<RadialMenuElement> elements, string groupName)
	{
		GroupElement groupElement = Object.Instantiate<GroupElement>(RadialData.SO.GroupElement);
		ItemGroupConfig itemGroupConfig = Object.Instantiate<ItemGroupConfig>(RadialData.SO.ItemGroupConfig);
		itemGroupConfig.GroupName = groupName;
		groupElement.Init(itemGroupConfig, this, radial);
		elements.Add(groupElement);
	}

	private void AddEmpty(List<RadialMenuElement> elements)
	{
		EmptyElement emptyElement = Object.Instantiate<EmptyElement>(RadialData.SO.EmptyElement);
		emptyElement.Init();
		elements.Add(emptyElement);
	}

	private void AddHammer(List<RadialMenuElement> elements)
	{
		HammerItemElement hammerItemElement = Object.Instantiate<HammerItemElement>(RadialData.SO.HammerItemElement);
		hammerItemElement.Init();
		elements.Add(hammerItemElement);
	}
}
