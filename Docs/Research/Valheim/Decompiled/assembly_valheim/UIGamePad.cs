using System.Collections.Generic;
using GUIFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGamePad : MonoBehaviour
{
	public KeyCode m_keyCode;

	public string m_zinputKey;

	public GameObject m_hint;

	[Tooltip("The hotkey won't activate if any of these gameobjects are visible")]
	public List<GameObject> m_blockingElements;

	private Button m_button;

	private ISubmitHandler m_submit;

	private Toggle m_toggle;

	private UIGroupHandler m_group;

	[SerializeField]
	private UIGroupHandler alternativeGroupHandler;

	private bool m_blockedByLastFrame;

	private bool m_blockNextFrame;

	private static int m_lastInteractFrame;

	private void Start()
	{
		m_group = ((Component)this).GetComponentInParent<UIGroupHandler>();
		m_button = ((Component)this).GetComponent<Button>();
		m_submit = ((Component)this).GetComponent<ISubmitHandler>();
		m_toggle = ((Component)this).GetComponent<Toggle>();
		if (Object.op_Implicit((Object)(object)m_hint))
		{
			m_hint.SetActive(false);
		}
	}

	private bool IsInteractive()
	{
		if ((Object)(object)m_button != (Object)null && !((Selectable)m_button).IsInteractable())
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_toggle))
		{
			if (!((Selectable)m_toggle).IsInteractable())
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)m_toggle.group) && !m_toggle.group.allowSwitchOff && m_toggle.isOn)
			{
				return false;
			}
		}
		if ((Object)(object)alternativeGroupHandler != (Object)null && alternativeGroupHandler.IsActive)
		{
			return true;
		}
		if (Object.op_Implicit((Object)(object)m_group) && !m_group.IsActive)
		{
			return false;
		}
		if (m_submit is GuiInputField && !((Selectable)/*isinst with value type is only supported in some contexts*/).interactable)
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		bool flag = IsInteractive();
		if (Object.op_Implicit((Object)(object)m_hint))
		{
			bool flag2 = ZInput.IsGamepadActive();
			bool flag3 = (Object)(object)alternativeGroupHandler != (Object)null && alternativeGroupHandler.IsActive;
			m_hint.SetActive(flag && (flag2 || flag3));
		}
		if (flag && Time.frameCount - m_lastInteractFrame >= 2 && ButtonPressed())
		{
			m_lastInteractFrame = Time.frameCount;
			ZLog.Log((object)("Button pressed " + ((Object)((Component)this).gameObject).name + "  frame:" + Time.frameCount));
			if ((Object)(object)m_button != (Object)null)
			{
				m_button.OnSubmit((BaseEventData)null);
			}
			else if (m_submit != null)
			{
				m_submit.OnSubmit((BaseEventData)null);
			}
			else if ((Object)(object)m_toggle != (Object)null)
			{
				m_toggle.OnSubmit((BaseEventData)null);
			}
		}
		m_blockedByLastFrame = m_blockNextFrame;
		m_blockNextFrame = false;
		if (m_blockingElements == null)
		{
			return;
		}
		foreach (GameObject blockingElement in m_blockingElements)
		{
			if (blockingElement.gameObject.activeInHierarchy)
			{
				m_blockNextFrame = true;
			}
		}
	}

	public bool ButtonPressed()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (IsBlocked())
		{
			return false;
		}
		if (m_blockingElements != null)
		{
			foreach (GameObject blockingElement in m_blockingElements)
			{
				if (blockingElement.gameObject.activeInHierarchy)
				{
					return false;
				}
			}
		}
		if (!string.IsNullOrEmpty(m_zinputKey) && ZInput.GetButtonDown(m_zinputKey))
		{
			return true;
		}
		if ((int)m_keyCode != 0 && ZInput.GetKeyDown(m_keyCode, true))
		{
			return true;
		}
		return false;
	}

	public bool IsBlocked()
	{
		if (!m_blockedByLastFrame && !m_blockNextFrame)
		{
			if (Object.op_Implicit((Object)(object)Console.instance))
			{
				return Console.IsVisible();
			}
			return false;
		}
		return true;
	}
}
