using TMPro;
using UnityEngine;
using Valheim.UI;

public class KeyHintsRadial : MonoBehaviour
{
	public TextMeshProUGUI m_gamepadInteract;

	public TextMeshProUGUI m_gamepadBack;

	public TextMeshProUGUI m_gamepadDrop;

	public TextMeshProUGUI m_gamepadDropMulti;

	public TextMeshProUGUI m_gamepadClose;

	public TextMeshProUGUI m_gamepadCloseTopLevel;

	public GameObject m_kbInteract;

	public GameObject m_kbBack;

	public GameObject m_kbDrop;

	public GameObject m_kbDropMulti;

	public GameObject m_kbClose;

	public GameObject m_kbCloseTopLevel;

	public void UpdateGamepadHints()
	{
		if ((Object)(object)m_gamepadInteract != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_gamepadInteract);
			((TMP_Text)m_gamepadInteract).text = "$radial_interact  <mspace=0.6em>$KEY_RadialInteract</mspace>";
			Localization.instance.Localize(((TMP_Text)m_gamepadInteract).transform);
		}
		if ((Object)(object)m_gamepadBack != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_gamepadBack);
			((TMP_Text)m_gamepadBack).text = "$radial_back  <mspace=0.6em>$KEY_RadialClose</mspace>  /  <mspace=0.6em>$KEY_RadialBack</mspace>";
			Localization.instance.Localize(((TMP_Text)m_gamepadBack).transform);
		}
		if ((Object)(object)m_gamepadClose != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_gamepadClose);
			((TMP_Text)m_gamepadClose).text = "$radial_close  <mspace=0.6em>$KEY_Radial</mspace>";
			Localization.instance.Localize(((TMP_Text)m_gamepadClose).transform);
		}
		if ((Object)(object)m_gamepadCloseTopLevel != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_gamepadCloseTopLevel);
			((TMP_Text)m_gamepadCloseTopLevel).text = "$radial_close  <mspace=0.6em>$KEY_RadialClose</mspace>  /  <mspace=0.6em>$KEY_RadialBack</mspace>  /  <mspace=0.6em>$KEY_Radial</mspace>";
			Localization.instance.Localize(((TMP_Text)m_gamepadCloseTopLevel).transform);
		}
		if ((Object)(object)m_gamepadDrop != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_gamepadDrop);
			((TMP_Text)m_gamepadDrop).text = "$radial_drop  <mspace=0.6em>$KEY_RadialSecondaryInteract</mspace>";
			Localization.instance.Localize(((TMP_Text)m_gamepadDrop).transform);
		}
		if ((Object)(object)m_gamepadDropMulti != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_gamepadDropMulti);
			((TMP_Text)m_gamepadDropMulti).text = "$radial_drop_multiple  <mspace=0.6em>$KEY_RadialSecondaryInteract</mspace>  $radial_hold";
			Localization.instance.Localize(((TMP_Text)m_gamepadDropMulti).transform);
		}
	}

	public void UpdateRadialHints(RadialBase radial)
	{
		bool isTopLevel = radial.IsTopLevel;
		((Component)m_gamepadCloseTopLevel).gameObject.SetActive(isTopLevel);
		m_kbCloseTopLevel.SetActive(isTopLevel);
		((Component)m_gamepadClose).gameObject.SetActive(!isTopLevel);
		m_kbClose.gameObject.SetActive(!isTopLevel);
		((Component)m_gamepadBack).gameObject.SetActive(!isTopLevel);
		m_kbBack.SetActive(!isTopLevel);
		bool active = false;
		((Component)m_gamepadDrop).gameObject.SetActive(active);
		((Component)m_gamepadDropMulti).gameObject.SetActive(active);
		m_kbDrop.SetActive(active);
		m_kbDropMulti.SetActive(active);
	}
}
