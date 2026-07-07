using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabHandler : MonoBehaviour
{
	[Serializable]
	public class Tab
	{
		public Button m_button;

		public RectTransform m_page;

		public bool m_default;

		public UnityEvent m_onClick;
	}

	public bool m_cycling = true;

	public bool m_tabKeyInput = true;

	public bool m_keybaordInput;

	public string m_keyboardNavigateLeft = "TabLeft";

	public string m_keyboardNavigateRight = "TabRight";

	public bool m_gamepadInput;

	public string m_gamepadNavigateLeft = "JoyTabLeft";

	public string m_gamepadNavigateRight = "JoyTabRight";

	private bool m_activeTabEverSet;

	public List<Tab> m_tabs = new List<Tab>();

	public List<GameObject> m_blockingElements = new List<GameObject>();

	[Header("Effects")]
	public EffectList m_setActiveTabEffects = new EffectList();

	private int m_selected;

	private UIGamePad gamePad;

	public event Action<int> ActiveTabChanged;

	private void Start()
	{
		Init(forceSelect: true);
	}

	public void Init(bool forceSelect = false)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		int num = -1;
		for (int i = 0; i < m_tabs.Count; i++)
		{
			Tab tab = m_tabs[i];
			if (!Object.op_Implicit((Object)(object)tab.m_button))
			{
				continue;
			}
			((UnityEvent)tab.m_button.onClick).AddListener((UnityAction)delegate
			{
				OnClick(tab.m_button);
			});
			Transform val = ((Component)tab.m_button).gameObject.transform.Find("Selected");
			if (Object.op_Implicit((Object)(object)val))
			{
				TMP_Text componentInChildren = ((Component)val).GetComponentInChildren<TMP_Text>();
				TMP_Text componentInChildren2 = ((Component)tab.m_button).GetComponentInChildren<TMP_Text>();
				string text = null;
				if ((Object)(object)componentInChildren2 != (Object)null)
				{
					text = componentInChildren2.text;
				}
				else
				{
					TextMeshProUGUI componentInChildren3 = ((Component)tab.m_button).GetComponentInChildren<TextMeshProUGUI>();
					if ((Object)(object)componentInChildren3 != (Object)null)
					{
						text = ((TMP_Text)componentInChildren3).text;
					}
				}
				if ((Object)(object)componentInChildren != (Object)null)
				{
					componentInChildren.text = text;
				}
				else
				{
					TextMeshProUGUI componentInChildren4 = ((Component)val).GetComponentInChildren<TextMeshProUGUI>();
					if ((Object)(object)componentInChildren4 != (Object)null)
					{
						((TMP_Text)componentInChildren4).text = text;
					}
				}
			}
			if (tab.m_default)
			{
				num = i;
			}
		}
		if (!m_activeTabEverSet && num >= 0)
		{
			SetActiveTab(num, forceSelect);
		}
		gamePad = ((Component)this).GetComponent<UIGamePad>();
	}

	private void Update()
	{
		if (UnifiedPopup.IsVisible())
		{
			return;
		}
		if (m_blockingElements.Count > 0)
		{
			foreach (GameObject blockingElement in m_blockingElements)
			{
				if (blockingElement.activeSelf)
				{
					return;
				}
			}
		}
		int num = 0;
		if (m_gamepadInput && ((Object)(object)gamePad == (Object)null || !gamePad.IsBlocked()))
		{
			if (!string.IsNullOrEmpty(m_gamepadNavigateLeft) && ZInput.GetButtonDown(m_gamepadNavigateLeft))
			{
				num = -1;
			}
			else if (!string.IsNullOrEmpty(m_gamepadNavigateRight) && ZInput.GetButtonDown(m_gamepadNavigateRight))
			{
				num = 1;
			}
		}
		if (m_keybaordInput)
		{
			if (!string.IsNullOrEmpty(m_keyboardNavigateLeft) && ZInput.GetButtonDown(m_keyboardNavigateLeft))
			{
				num = -1;
			}
			else if (!string.IsNullOrEmpty(m_keyboardNavigateRight) && ZInput.GetButtonDown(m_keyboardNavigateRight))
			{
				num = 1;
			}
		}
		if (m_tabKeyInput && ZInput.GetKeyDown((KeyCode)9, true))
		{
			num = 1;
		}
		if (num == 0)
		{
			return;
		}
		int num2 = m_selected + num;
		if (m_cycling)
		{
			if (num2 < 0)
			{
				num2 = m_tabs.Count - 1;
			}
			else if (num2 > m_tabs.Count - 1)
			{
				num2 = 0;
			}
			if (!Object.op_Implicit((Object)(object)m_tabs[num2].m_button))
			{
				for (int i = num2 + num; i <= m_tabs.Count && i != num2; i += num)
				{
					if (i >= m_tabs.Count)
					{
						i = 0;
					}
					if (Object.op_Implicit((Object)(object)m_tabs[i].m_button))
					{
						SetActiveTab(i);
						break;
					}
				}
			}
			else
			{
				SetActiveTab(num2);
			}
		}
		else
		{
			SetActiveTab(Math.Max(0, Math.Min(m_tabs.Count - 1, num2)));
		}
	}

	private void OnClick(Button button)
	{
		SetActiveTab(button);
	}

	private void SetActiveTab(Button button)
	{
		for (int i = 0; i < m_tabs.Count; i++)
		{
			if (!((Object)(object)m_tabs[i].m_button == (Object)null) && !((Object)(object)m_tabs[i].m_button != (Object)(object)button))
			{
				SetActiveTab(i);
				break;
			}
		}
	}

	public void SetActiveTab(int index, bool forceSelect = false, bool invokeOnClick = true)
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		m_activeTabEverSet = true;
		if (!forceSelect && m_selected == index)
		{
			return;
		}
		m_selected = (Object.op_Implicit((Object)(object)m_tabs[index].m_button) ? index : m_selected);
		for (int i = 0; i < m_tabs.Count; i++)
		{
			Tab tab = m_tabs[i];
			bool flag = i == index;
			if ((Object)(object)tab.m_page != (Object)null)
			{
				((Component)tab.m_page).gameObject.SetActive(flag);
			}
			if (!Object.op_Implicit((Object)(object)tab.m_button))
			{
				continue;
			}
			((Selectable)tab.m_button).interactable = !flag;
			Transform val = ((Component)tab.m_button).gameObject.transform.Find("Selected");
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(i == m_selected);
			}
			if (flag && invokeOnClick)
			{
				UnityEvent onClick = tab.m_onClick;
				if (onClick != null)
				{
					onClick.Invoke();
				}
			}
		}
		if (ZInput.IsGamepadActive())
		{
			m_setActiveTabEffects?.Create(((Object)(object)Player.m_localPlayer != (Object)null) ? ((Component)Player.m_localPlayer).transform.position : Vector3.zero, Quaternion.identity);
		}
		this.ActiveTabChanged?.Invoke(index);
	}

	public void SetActiveTabWithoutInvokingOnClick(int index)
	{
		m_selected = (Object.op_Implicit((Object)(object)m_tabs[index].m_button) ? index : m_selected);
		for (int i = 0; i < m_tabs.Count; i++)
		{
			Tab tab = m_tabs[i];
			bool flag = i == index;
			if ((Object)(object)tab.m_page != (Object)null)
			{
				((Component)tab.m_page).gameObject.SetActive(flag);
			}
			if (Object.op_Implicit((Object)(object)tab.m_button))
			{
				((Selectable)tab.m_button).interactable = !flag;
				Transform val = ((Component)tab.m_button).gameObject.transform.Find("Selected");
				if (Object.op_Implicit((Object)(object)val))
				{
					((Component)val).gameObject.SetActive(i == m_selected);
				}
			}
		}
		this.ActiveTabChanged?.Invoke(index);
	}

	public int GetActiveTab()
	{
		return m_selected;
	}
}
