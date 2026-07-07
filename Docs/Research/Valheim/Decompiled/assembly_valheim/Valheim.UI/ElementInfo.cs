using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.UI;

public class ElementInfo : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI m_title;

	[SerializeField]
	protected Image m_icon;

	[SerializeField]
	protected GuiBar m_durabilityBar;

	[SerializeField]
	protected TextMeshProUGUI m_subTitle;

	[SerializeField]
	protected RadialInventoryInfo m_inventoryInfo;

	protected Image m_background;

	private Image m_durabilityBarBG;

	private RectTransform m_rectTransform;

	protected Material m_cutoutMaterial;

	protected Material m_iconMaterial;

	public Image BackgroundImage
	{
		get
		{
			if ((Object)(object)m_background == (Object)null)
			{
				m_background = ((Component)this).gameObject.GetComponent<Image>();
			}
			return m_background;
		}
	}

	public Image DurabilityBarBG
	{
		get
		{
			if ((Object)(object)m_durabilityBarBG == (Object)null)
			{
				m_durabilityBarBG = ((Component)m_durabilityBar).gameObject.GetComponent<Image>();
			}
			return m_durabilityBarBG;
		}
	}

	public RectTransform InfoTransform
	{
		get
		{
			if ((Object)(object)m_rectTransform == (Object)null)
			{
				ref RectTransform rectTransform = ref m_rectTransform;
				Transform transform = ((Component)this).transform;
				rectTransform = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			}
			return m_rectTransform;
		}
	}

	public float Radius
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return InfoTransform.sizeDelta.x;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			InfoTransform.sizeDelta = Vector2.one * value;
		}
	}

	public float Alpha
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return CutoutMaterial.GetColor("_Color").a;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			BGAlpha = value;
			Color color = IconMaterial.color;
			color.a = value;
			IconMaterial.color = color;
			((TMP_Text)m_title).alpha = value;
			((TMP_Text)m_subTitle).alpha = value;
			color = m_durabilityBar.GetColor();
			color.a = value;
			m_durabilityBar.SetColor(color);
			color = ((Graphic)DurabilityBarBG).color;
			color.a = Mathf.Max(value, 0.65f);
			((Graphic)DurabilityBarBG).color = color;
		}
	}

	public float BGAlpha
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return CutoutMaterial.GetColor("_Color").a;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Color color = CutoutMaterial.GetColor("_Color");
			color.a = Mathf.Clamp(value, 0f, 0.8f);
			CutoutMaterial.SetColor("_Color", color);
		}
	}

	public Material CutoutMaterial
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			if (Object.op_Implicit((Object)(object)m_cutoutMaterial))
			{
				return m_cutoutMaterial;
			}
			m_cutoutMaterial = new Material(((Graphic)BackgroundImage).material);
			((Graphic)BackgroundImage).material = m_cutoutMaterial;
			return m_cutoutMaterial;
		}
	}

	public Material IconMaterial
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			if (Object.op_Implicit((Object)(object)m_iconMaterial))
			{
				return m_iconMaterial;
			}
			m_iconMaterial = new Material(((Graphic)m_icon).material);
			((Graphic)m_icon).material = m_iconMaterial;
			return m_iconMaterial;
		}
		set
		{
			m_iconMaterial = value;
		}
	}

	public virtual void Clear()
	{
		((Component)m_subTitle).gameObject.SetActive(true);
		((Component)m_durabilityBar).gameObject.SetActive(false);
		((TMP_Text)m_title).text = "";
		((TMP_Text)m_subTitle).text = "";
		((Component)m_icon).gameObject.SetActive(false);
	}

	public void UpdateDurabilityAndWeightInfo(RadialMenuElement element)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)m_durabilityBar).gameObject.activeSelf && element is ItemElement itemElement)
		{
			m_durabilityBar.SetValue(itemElement.m_durability.GetSmoothValue());
			m_durabilityBar.SetColor(itemElement.m_durability.GetColor());
		}
		if (((Component)m_inventoryInfo).gameObject.activeSelf)
		{
			if (element is ThrowElement element2)
			{
				m_inventoryInfo.RefreshWeight(element2);
			}
			else
			{
				m_inventoryInfo.RefreshWeight();
			}
		}
	}

	internal virtual void Set(RadialMenuElement element, RadialMenuAnimationManager animator)
	{
		if (!Object.op_Implicit((Object)(object)element))
		{
			Clear();
			return;
		}
		((Component)m_subTitle).gameObject.SetActive(true);
		((Component)m_durabilityBar).gameObject.SetActive(false);
		bool flag = element is ItemElement || element is ThrowElement;
		((TMP_Text)m_title).text = (flag ? "" : element.Name);
		((TMP_Text)m_subTitle).text = element.SubTitle;
		((Component)m_icon).gameObject.SetActive(flag);
		if (flag)
		{
			m_icon.sprite = element.Icon.sprite;
		}
		if (!((Component)m_inventoryInfo).gameObject.activeSelf)
		{
			return;
		}
		m_inventoryInfo.RefreshInfo();
		if (!(element is ItemElement itemElement))
		{
			if (element is ThrowElement throwElement)
			{
				m_inventoryInfo.SetElement(throwElement, animator);
				SetDurabilityData(throwElement.m_data);
			}
			else
			{
				m_inventoryInfo.HideToolTip(animator);
			}
		}
		else
		{
			m_inventoryInfo.SetElement(itemElement, animator);
			SetDurabilityData(itemElement.m_data);
		}
		void SetDurabilityData(ItemDrop.ItemData data)
		{
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			bool flag2 = data.m_shared.m_useDurability && data.m_durability < data.GetMaxDurability();
			((Component)m_durabilityBar).gameObject.SetActive(flag2);
			((Component)m_subTitle).gameObject.SetActive(!flag2);
			if (flag2)
			{
				bool flag3 = data.m_durability <= 0f;
				m_durabilityBar.SetValue(flag3 ? 1f : data.GetDurabilityPercentage());
				if (flag3)
				{
					m_durabilityBar.SetColor((Color)((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : new Color(0f, 0f, 0f, 0f)));
				}
				else
				{
					m_durabilityBar.ResetColor();
				}
			}
		}
	}

	public virtual void Set(IRadialConfig config, bool updateAlpha = true)
	{
		Clear();
		if (config != null)
		{
			((TMP_Text)m_title).text = config.LocalizedName;
			((Component)m_inventoryInfo).gameObject.SetActive(config is ItemGroupConfig || config is ThrowGroupConfig);
			m_inventoryInfo.RefreshInfo();
			if (updateAlpha)
			{
				Alpha = 1f;
				m_inventoryInfo.SetAlpha(1f);
			}
		}
	}

	internal void OpenAnimation(RadialMenuAnimationManager manager, string id, float duration, float radius, float startOffset, EasingType alphaEasingType, EasingType positionEasingType)
	{
		Radius = radius + startOffset;
		Alpha = 0f;
		manager.StartTween(() => Alpha, delegate(float val)
		{
			Alpha = val;
		}, id, 0.8f, duration + 0.1f, alphaEasingType);
		manager.StartTween(m_inventoryInfo.SetAlpha, id, 0f, 1f, duration + 0.1f, alphaEasingType);
		manager.StartTween(() => Radius, delegate(float val)
		{
			Radius = val;
		}, id, radius, duration, positionEasingType);
	}

	internal void CloseAnimation(RadialMenuAnimationManager manager, string id, float duration, float radius, float startOffset, EasingType alphaEasingType, EasingType positionEasingType)
	{
		manager.StartTween(() => Alpha, delegate(float val)
		{
			Alpha = val;
		}, id, 0f, duration, alphaEasingType);
		manager.StartTween(m_inventoryInfo.SetAlpha, id, 1f, 0f, duration, alphaEasingType);
		manager.StartTween(() => Radius, delegate(float val)
		{
			Radius = val;
		}, id, radius + startOffset, duration + 0.1f, positionEasingType, delegate
		{
			Radius = radius;
		});
	}
}
