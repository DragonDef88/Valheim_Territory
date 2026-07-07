using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.UI;

public class RadialInventoryInfo : MonoBehaviour
{
	[SerializeField]
	private Image m_tooltipBG;

	[SerializeField]
	private Image m_armorBG;

	[SerializeField]
	private Image m_armorIcon;

	[SerializeField]
	private Image m_weightBG;

	[SerializeField]
	private Image m_weightIcon;

	[SerializeField]
	protected TextMeshProUGUI m_armorText;

	[SerializeField]
	protected TextMeshProUGUI m_inventoryWeightText;

	[SerializeField]
	protected float m_toolTipMinHeight = 75f;

	[SerializeField]
	protected float m_toolTipMaxHeight = 700f;

	[SerializeField]
	protected float m_toolTipReSizeSpeed = 10f;

	[SerializeField]
	private EasingType m_reSizeEasingType;

	[SerializeField]
	protected RectTransform m_itemTooltip;

	[SerializeField]
	protected TextMeshProUGUI m_itemTitleText;

	[SerializeField]
	protected TextMeshProUGUI m_itemTooltipText;

	private float m_currentTooltipHeight;

	internal void SetElement(ItemElement element, RadialMenuAnimationManager animator)
	{
		OverwriteWeightString(MakeInventoryWeightString(Player.m_localPlayer));
		SetTooltip(element.Name, element.Description, element.m_data, animator);
	}

	internal void SetElement(ThrowElement element, RadialMenuAnimationManager animator)
	{
		OverwriteWeightString(element.TotalWeightString);
		SetTooltip(element.Name, element.Description, element.m_data, animator);
	}

	private void SetTooltip(string name, string description, ItemDrop.ItemData data, RadialMenuAnimationManager animator)
	{
		SetArmorString(data);
		if (m_currentTooltipHeight < m_toolTipMinHeight)
		{
			ResizeTooltipHeight(m_toolTipMinHeight);
		}
		((TMP_Text)m_itemTitleText).text = name;
		if (description != null)
		{
			((TMP_Text)m_itemTooltipText).text = Localization.instance.Localize(description);
			StartResize(animator);
		}
	}

	private void SetArmorString(ItemDrop.ItemData data)
	{
		if (((Component)m_armorText).gameObject.activeSelf)
		{
			if (data.TryGetArmorDifference(out var difference))
			{
				string text = Player.m_localPlayer.GetBodyArmor().ToString();
				string text2 = ((difference > 0f) ? "<color=green>+" : ((difference == 0f) ? "<color=orange>+" : "<color=red>")) + difference + "</color>";
				((TMP_Text)m_armorText).text = text + " " + text2;
			}
			else
			{
				((TMP_Text)m_armorText).text = Player.m_localPlayer.GetBodyArmor().ToString();
			}
		}
	}

	internal void HideToolTip(RadialMenuAnimationManager animator)
	{
		animator.StartUniqueTween(() => m_currentTooltipHeight, delegate(float newHeight)
		{
			ResizeTooltipHeight(newHeight);
			if (MinHeightCheck())
			{
				animator.CancelTweens("RadialInfoResize");
			}
		}, "RadialInfoResize", 0f, m_toolTipReSizeSpeed, m_reSizeEasingType);
	}

	public void OverwriteWeightString(string newWeightString)
	{
		if (((Component)m_inventoryWeightText).gameObject.activeSelf)
		{
			((TMP_Text)m_inventoryWeightText).text = newWeightString;
		}
	}

	public string MakeInventoryWeightString(Player localPlayer)
	{
		int num = Mathf.CeilToInt(localPlayer.GetInventory().GetTotalWeight());
		int num2 = Mathf.CeilToInt(localPlayer.GetMaxCarryWeight());
		if (num > num2 && Mathf.Sin(Time.time * 10f) > 0f)
		{
			return $"<color=red>{num}</color> / {num2}";
		}
		return $"{num} / {num2}";
	}

	private void StartResize(RadialMenuAnimationManager animator)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float num = (((Component)m_itemTooltipText).gameObject.activeSelf ? (((TMP_Text)m_itemTooltipText).GetPreferredValues().y + m_toolTipMinHeight + 10f) : m_toolTipMinHeight);
		num = Mathf.Clamp(num, m_toolTipMinHeight, m_toolTipMaxHeight);
		if (num < m_toolTipMinHeight)
		{
			HideToolTip(animator);
			return;
		}
		animator.StartUniqueTween(() => m_currentTooltipHeight, ResizeTooltipHeight, "RadialInfoResize", num, m_toolTipReSizeSpeed, m_reSizeEasingType);
	}

	protected bool MinHeightCheck()
	{
		if (!(m_currentTooltipHeight < m_toolTipMinHeight))
		{
			return false;
		}
		ResizeTooltipHeight(0f);
		((TMP_Text)m_itemTitleText).text = "";
		return true;
	}

	protected void ResizeTooltipHeight(float newHeight)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		m_itemTooltip.sizeDelta = new Vector2(m_itemTooltip.sizeDelta.x, newHeight);
		m_currentTooltipHeight = newHeight;
	}

	public void RefreshInfo()
	{
		Player localPlayer = Player.m_localPlayer;
		bool active = (Object)(object)localPlayer != (Object)null;
		((Component)m_armorText).gameObject.SetActive(active);
		((Component)m_inventoryWeightText).gameObject.SetActive(active);
		((Component)m_itemTooltip).gameObject.SetActive(active);
		if (((Component)m_armorText).gameObject.activeSelf)
		{
			((TMP_Text)m_armorText).text = localPlayer.GetBodyArmor().ToString();
		}
		RefreshWeight();
	}

	public void RefreshWeight()
	{
		if ((Object)(object)Player.m_localPlayer != (Object)null && ((Component)m_inventoryWeightText).gameObject.activeSelf)
		{
			((TMP_Text)m_inventoryWeightText).text = MakeInventoryWeightString(Player.m_localPlayer);
		}
	}

	public void RefreshWeight(ThrowElement element)
	{
		OverwriteWeightString(element.TotalWeightString);
		((TMP_Text)m_itemTooltipText).text = Localization.instance.Localize(element.Description);
	}

	public void SetAlpha(float newAlpha)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Color black = Color.black;
		black.a = Mathf.Min(newAlpha, 0.8f);
		((Graphic)m_armorBG).color = black;
		((Graphic)m_weightBG).color = black;
		Color white = Color.white;
		white.a = newAlpha;
		((Graphic)m_armorIcon).color = white;
		((Graphic)m_weightIcon).color = white;
		((TMP_Text)m_armorText).alpha = newAlpha;
		((TMP_Text)m_inventoryWeightText).alpha = newAlpha;
		((Graphic)m_tooltipBG).color = black;
		((TMP_Text)m_itemTooltipText).alpha = newAlpha;
		((TMP_Text)m_itemTitleText).alpha = newAlpha;
	}

	private void OnEnable()
	{
		RefreshInfo();
		ResizeTooltipHeight(0f);
	}
}
