using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeySlider : KeyUI
{
	[Serializable]
	public class SliderSetting
	{
		public string m_name;

		public string m_toolTip;

		public WorldModifierOption m_modifierValue;

		public List<string> m_keys = new List<string>();
	}

	private Slider m_slider;

	public TMP_Text m_nameLabel;

	public TMP_Text m_toolTipLabel;

	public string m_toolTip;

	public int m_defaultIndex;

	public WorldModifiers m_modifier;

	public List<SliderSetting> m_settings;

	[HideInInspector]
	public bool m_manualSet;

	private float m_lastToolTipUpdateValue = -1f;

	public static KeySlider m_lastActiveSlider;

	public void Awake()
	{
		m_slider = ((Component)this).GetComponentInParent<Slider>();
		m_slider.maxValue = m_settings.Count - 1;
		m_slider.value = m_defaultIndex;
		m_slider.wholeNumbers = true;
		foreach (SliderSetting setting in m_settings)
		{
			for (int i = 0; i < setting.m_keys.Count; i++)
			{
				setting.m_keys[i] = setting.m_keys[i].ToLower();
			}
		}
		((UnityEvent<float>)(object)m_slider.onValueChanged).AddListener((UnityAction<float>)OnValueChanged);
	}

	public override void Update()
	{
		if (Object.op_Implicit((Object)(object)m_nameLabel))
		{
			m_nameLabel.text = Localization.instance.Localize(Selected().m_name);
		}
		if ((Object)(object)m_lastActiveSlider == (Object)(object)this && (Object)(object)KeyUI.m_lastKeyUI == (Object)(object)this)
		{
			SetToolTip();
		}
		if (ZInput.IsGamepadActive() && (Object)(object)EventSystem.current.currentSelectedGameObject != (Object)(object)((Component)this).gameObject && (Object)(object)m_lastActiveSlider == (Object)(object)this)
		{
			m_lastActiveSlider = null;
		}
		base.Update();
	}

	public void OnValueChanged(float f)
	{
		OnValueChanged();
		m_manualSet = true;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if ((Object)(object)m_lastActiveSlider != (Object)(object)this || (Object)(object)KeyUI.m_lastKeyUI != (Object)(object)this)
		{
			m_lastActiveSlider = this;
			m_lastToolTipUpdateValue = -1f;
		}
		base.OnPointerEnter(eventData);
	}

	public void SetValue(WorldModifierOption value)
	{
		for (int i = 0; i < m_settings.Count; i++)
		{
			if (m_settings[i].m_modifierValue == value)
			{
				m_slider.value = i;
				OnValueChanged();
				return;
			}
		}
		Terminal.LogError($"Slider {m_modifier} missing value to set: {value}");
	}

	public WorldModifierOption GetValue()
	{
		return m_settings[(int)m_slider.value].m_modifierValue;
	}

	protected override void SetToolTip()
	{
		if (Object.op_Implicit((Object)(object)m_toolTipLabel) && !((Object)(object)m_lastActiveSlider != (Object)(object)this) && m_slider.value != m_lastToolTipUpdateValue)
		{
			m_lastToolTipUpdateValue = m_slider.value;
			string text = "";
			if (m_toolTip.Length > 0)
			{
				text = text + m_toolTip + "\n\n";
			}
			if (Selected().m_name.Length > 0 && Selected().m_toolTip.Length > 0)
			{
				text = text + "<color=orange>" + Selected().m_name + "</color>\n";
			}
			text += Selected().m_toolTip;
			if (text.Length > 0)
			{
				m_toolTipLabel.text = Localization.instance.Localize(text);
				((Component)m_toolTipLabel).gameObject.SetActive(true);
			}
		}
	}

	private SliderSetting Selected()
	{
		return m_settings[(int)m_slider.value];
	}

	public override void SetKeys(World world)
	{
		foreach (string key in m_settings[(int)m_slider.value].m_keys)
		{
			string text = key.ToLower();
			if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
			{
				ZoneSystem.instance.SetGlobalKey(text);
			}
			else
			{
				if (world.m_startingGlobalKeys.Contains(text))
				{
					continue;
				}
				string text2 = text.Split(' ')[0].ToLower();
				for (int num = world.m_startingGlobalKeys.Count - 1; num >= 0; num--)
				{
					if (world.m_startingGlobalKeys[num].Split(' ')[0].ToLower() == text2)
					{
						world.m_startingGlobalKeys.RemoveAt(num);
					}
				}
				world.m_startingGlobalKeys.Add(text);
			}
		}
	}

	public override bool TryMatch(World world, bool checkAllKeys = false)
	{
		int num = 0;
		for (int i = 0; i < m_settings.Count; i++)
		{
			bool flag = false;
			SliderSetting sliderSetting = m_settings[i];
			if (sliderSetting.m_keys.Count == 0)
			{
				m_slider.value = (num = i);
				flag = true;
				if (world.m_startingGlobalKeys.Count == 0)
				{
					return true;
				}
				continue;
			}
			foreach (string key in sliderSetting.m_keys)
			{
				if (!StringUtils.ContainsString((IReadOnlyList<string>)world.m_startingGlobalKeys, key, StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
					break;
				}
			}
			if (checkAllKeys)
			{
				foreach (string startingGlobalKey in world.m_startingGlobalKeys)
				{
					if (Enum.TryParse<GlobalKeys>(ZoneSystem.GetKeyValue(startingGlobalKey, out var _, out var _), ignoreCase: true, out var result) && result < GlobalKeys.NonServerOption && !StringUtils.ContainsString((IReadOnlyList<string>)sliderSetting.m_keys, startingGlobalKey, StringComparison.InvariantCultureIgnoreCase))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				m_slider.value = i;
				return true;
			}
		}
		return false;
	}

	public override bool TryMatch(IReadOnlyList<string> keys, out string label, bool setSlider = true)
	{
		int num = 0;
		int num2 = -1;
		int num3 = 0;
		for (int i = 0; i < m_settings.Count; i++)
		{
			bool flag = false;
			SliderSetting sliderSetting = m_settings[i];
			if (sliderSetting.m_keys.Count == 0)
			{
				num = i;
				flag = true;
			}
			foreach (string key in sliderSetting.m_keys)
			{
				if (!StringUtils.ContainsString(keys, key, StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
					break;
				}
			}
			if (!flag && sliderSetting.m_keys.Count >= num3)
			{
				num2 = i;
				num3 = sliderSetting.m_keys.Count;
			}
		}
		if (num2 >= 0)
		{
			if (setSlider)
			{
				m_slider.value = num2;
			}
			label = m_modifier.GetDisplayString() + ": " + m_settings[num2].m_name;
			return true;
		}
		if (setSlider)
		{
			m_slider.value = num;
		}
		label = null;
		return false;
	}
}
