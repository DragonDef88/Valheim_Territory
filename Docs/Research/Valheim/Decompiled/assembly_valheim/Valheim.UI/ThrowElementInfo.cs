using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.UI;

public class ThrowElementInfo : ElementInfo
{
	[SerializeField]
	protected Image m_itemIcon;

	[SerializeField]
	protected TextMeshProUGUI m_amountText;

	protected ItemDrop.ItemData m_data;

	protected string m_itemName;

	protected Material m_itemIconMaterial;

	public Sprite ItemIcon
	{
		get
		{
			return m_itemIcon.sprite;
		}
		set
		{
			m_itemIcon.sprite = value;
		}
	}

	public Material ItemIconMaterial
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			if ((Object)(object)m_iconMaterial == (Object)null)
			{
				m_itemIconMaterial = new Material(((Graphic)m_itemIcon).material);
				((Graphic)m_itemIcon).material = m_itemIconMaterial;
			}
			return m_itemIconMaterial;
		}
		set
		{
			m_itemIconMaterial = value;
		}
	}

	public void Init(ItemDrop.ItemData item)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (item != null)
		{
			ItemIcon = item.GetIcon();
			m_itemName = Localization.instance.Localize(item.m_shared.m_name);
			Color color = ItemIconMaterial.color;
			color.a = 1f;
			ItemIconMaterial.color = color;
			m_data = item;
			Clear();
		}
	}

	internal override void Set(RadialMenuElement element, RadialMenuAnimationManager animator)
	{
		if (element is ThrowElement throwElement)
		{
			((TMP_Text)m_title).text = m_itemName;
			((TMP_Text)m_subTitle).text = throwElement.SubTitle;
			((Component)m_icon).gameObject.SetActive(true);
			((Component)m_itemIcon).gameObject.SetActive(true);
			((Component)m_amountText).gameObject.SetActive(true);
			((TMP_Text)m_amountText).text = throwElement.m_inventoryAmountText;
		}
		else if (element is BackElement)
		{
			Clear();
			((TMP_Text)m_title).text = element.Name;
			((Component)m_amountText).gameObject.SetActive(false);
			((Component)m_icon).gameObject.SetActive(false);
		}
		else
		{
			Clear();
		}
	}

	public override void Clear()
	{
		((Component)m_amountText).gameObject.SetActive(true);
		((TMP_Text)m_title).text = m_itemName;
		((TMP_Text)m_subTitle).text = "";
		((Component)m_icon).gameObject.SetActive(true);
		if (m_data != null)
		{
			if (m_data.m_shared.m_maxStackSize > 1)
			{
				((TMP_Text)m_amountText).text = $"{m_data.m_stack} / {m_data.m_shared.m_maxStackSize}";
			}
			else
			{
				((TMP_Text)m_amountText).text = $"{m_data.m_stack}";
			}
		}
	}
}
