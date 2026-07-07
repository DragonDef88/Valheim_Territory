using Splatform;
using TMPro;
using UnityEngine;
using UserManagement;

public class Sign : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
	public TextMeshProUGUI m_textWidget;

	public string m_name = "Sign";

	public string m_defaultText = "Sign";

	public string m_writtenBy = "Written by";

	public int m_characterLimit = 50;

	private ZNetView m_nview;

	private bool m_isViewable = true;

	private string m_authorDisplayName = "";

	private PlatformUserID? m_author;

	private string m_currentText;

	private uint m_lastRevision = uint.MaxValue;

	private void Awake()
	{
		m_currentText = m_defaultText;
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.GetZDO() != null)
		{
			UpdateText();
			((MonoBehaviour)this).InvokeRepeating("UpdateText", 2f, 2f);
		}
	}

	public string GetHoverText()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		string text = (m_isViewable ? ("\"" + StringExtensionMethods.RemoveRichTextTags(GetText()) + "\"") : ((!m_author.HasValue || !MuteList.Contains(m_author.Value)) ? ("[" + Localization.instance.Localize("$text_hidden_notification_ugc_settings") + "]") : ("[" + Localization.instance.Localize("$text_hidden_notification_muted") + "]")));
		if (!PrivateArea.CheckAccess(((Component)this).transform.position, 0f, flash: false))
		{
			return text;
		}
		string text2 = "";
		return text + text2 + "\n" + Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(((Component)this).transform.position))
		{
			return false;
		}
		PrivilegeResult val = PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)2);
		if (!PrivilegeResultExtentions.IsGranted(val))
		{
			if (PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege != null)
			{
				PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege.Open((Privilege)2, (PrivilegeResult)64);
				if (!((UIController)PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege).IsOpen)
				{
					ZLog.LogError((object)string.Format("{0} can't resolve the {1} privilege on this platform, which was denied with result {2}. Modifying sign text was blocked without meaningful feedback to the user!", "ResolvePrivilegeUI", (object)(Privilege)2, val));
				}
			}
			else
			{
				ZLog.LogError((object)string.Format("{0} is not available on this platform to resolve the {1} privilege, which was denied with result {2}. Modifying sign text was blocked without meaningful feedback to the user!", "ResolvePrivilegeUI", (object)(Privilege)2, val));
			}
			return false;
		}
		TextInput.instance.RequestText(this, "$piece_sign_input", m_characterLimit);
		return true;
	}

	private void UpdateText()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		uint dataRevision = m_nview.GetZDO().DataRevision;
		if (m_lastRevision == dataRevision)
		{
			UpdateViewPermission();
			return;
		}
		m_lastRevision = dataRevision;
		string @string = m_nview.GetZDO().GetString(ZDOVars.s_text, m_defaultText);
		m_currentText = @string;
		m_authorDisplayName = m_nview.GetZDO().GetString(ZDOVars.s_authorDisplayName);
		string resolvedAuthor = m_nview.GetZDO().GetString(ZDOVars.s_author);
		if (m_nview.IsOwner() && RelationsManager.UpdateAuthorIfHost(resolvedAuthor, ref resolvedAuthor))
		{
			m_nview.GetZDO().Set(ZDOVars.s_author, resolvedAuthor);
		}
		if (string.IsNullOrEmpty(resolvedAuthor))
		{
			m_author = PlatformUserID.None;
		}
		else if (resolvedAuthor == "host")
		{
			m_author = null;
		}
		else
		{
			m_author = new PlatformUserID(resolvedAuthor);
		}
		UpdateViewPermission();
	}

	private void UpdateViewPermission()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_author.HasValue)
		{
			OnCheckPermissionCompleted(RelationsManagerPermissionResult.Denied);
		}
		PlatformUserID value = m_author.Value;
		if (((PlatformUserID)(ref value)).IsValid)
		{
			RelationsManager.CheckPermissionAsync(m_author.Value, (Permission)2, isSender: false, OnCheckPermissionCompleted);
		}
		else
		{
			OnCheckPermissionCompleted(RelationsManagerPermissionResult.Granted);
		}
	}

	private void OnCheckPermissionCompleted(RelationsManagerPermissionResult result)
	{
		if (result.IsGranted())
		{
			m_isViewable = true;
			if (result == RelationsManagerPermissionResult.GrantedRequiresFiltering)
			{
				CensorShittyWords.Filter(m_currentText, out var output);
				((TMP_Text)m_textWidget).text = output;
			}
			else
			{
				((TMP_Text)m_textWidget).text = m_currentText;
			}
		}
		else
		{
			m_isViewable = false;
			((TMP_Text)m_textWidget).text = "ᚬᛏᛁᛚᛚᚴᛅᚾᚴᛚᛁᚴ";
		}
	}

	public string GetText()
	{
		return ((TMP_Text)m_textWidget).text;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	public void SetText(string text)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (PrivateArea.CheckAccess(((Component)this).transform.position))
		{
			m_nview.ClaimOwnership();
			m_nview.GetZDO().Set(ZDOVars.s_text, text);
			PlatformUserID platformUserID = ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID;
			m_nview.GetZDO().Set(ZDOVars.s_author, PlatformManager.DistributionPlatform.LocalUser.IsSignedIn ? ((object)(PlatformUserID)(ref platformUserID)).ToString() : "host");
			if (ZNet.TryGetServerAssignedDisplayName(platformUserID, out var displayName))
			{
				m_nview.GetZDO().Set(ZDOVars.s_authorDisplayName, displayName);
			}
			UpdateText();
		}
	}
}
