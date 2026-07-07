using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeyToggle : KeyUI
{
	private Toggle m_toggle;

	private TMP_Text m_text;

	public TMP_Text m_toolTipLabel;

	public string m_toolTip;

	public bool m_defaultOn;

	public string m_enabledKey;

	public void Awake()
	{
		m_toggle = ((Component)this).GetComponentInParent<Toggle>();
		m_toggle.isOn = m_defaultOn;
		((UnityEvent<bool>)(object)m_toggle.onValueChanged).AddListener((UnityAction<bool>)delegate
		{
			OnValueChanged();
		});
		m_text = ((Component)this).GetComponentInChildren<TMP_Text>();
	}

	public override void Update()
	{
		if (ZInput.IsGamepadActive() && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)m_toggle).gameObject)
		{
			SetToolTip();
		}
		base.Update();
	}

	protected override void SetToolTip()
	{
		if (Object.op_Implicit((Object)(object)m_toolTipLabel))
		{
			m_toolTipLabel.text = Localization.instance.Localize(m_toolTip);
		}
	}

	public override void SetKeys(World world)
	{
		if (m_toggle.isOn)
		{
			string text = m_enabledKey.ToLower();
			if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
			{
				ZoneSystem.instance.SetGlobalKey(text);
			}
			else if (!world.m_startingGlobalKeys.Contains(text))
			{
				world.m_startingGlobalKeys.Add(m_enabledKey.ToLower());
			}
		}
	}

	public override bool TryMatch(World world, bool checkAllKeys = false)
	{
		return m_toggle.isOn = world.m_startingGlobalKeys.Contains(m_enabledKey.ToLower());
	}

	public override bool TryMatch(IReadOnlyList<string> keys, out string label, bool setToggle = true)
	{
		m_toggle.isOn = false;
		if (StringUtils.ContainsString(keys, m_enabledKey, StringComparison.InvariantCultureIgnoreCase))
		{
			if (setToggle)
			{
				m_toggle.isOn = true;
			}
			TMP_Text text = m_text;
			label = ((text != null) ? text.text : m_enabledKey);
			return true;
		}
		label = null;
		return false;
	}
}
