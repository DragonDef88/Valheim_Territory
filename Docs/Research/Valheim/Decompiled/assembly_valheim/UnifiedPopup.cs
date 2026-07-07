using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnifiedPopup : MonoBehaviour
{
	public delegate void PopupEnabledHandler();

	private static UnifiedPopup instance;

	[Header("References")]
	[Tooltip("A reference to the parent object of the rest of the popup. This is what gets enabled and disabled to show and hide the popup.")]
	[SerializeField]
	private GameObject popupUIParent;

	[Tooltip("A reference to the left button of the popup, assigned to escape on keyboards and B on controllers. This usually gets assigned to \"back\", \"no\" or similar in dual-action popups. It's not necessary to assign buttons to any Unity Events - that is done automatically.")]
	[SerializeField]
	private Button buttonLeft;

	[Tooltip("A reference to the center button of the popup, assigned to enter on keyboards and A on controllers. This usually gets assigned to \"Ok\" or similar in single-action popups. It's not necessary to assign buttons to any Unity Events - that is done automatically.")]
	[SerializeField]
	private Button buttonCenter;

	[Tooltip("A reference to the right button of the popup, assigned to enter on keyboards and A on controllers. This usually gets assigned to \"yes\", \"accept\" or similar in dual-action popups. It's not necessary to assign buttons to any Unity Events - that is done automatically.")]
	[SerializeField]
	private Button buttonRight;

	[Tooltip("A reference to the header text of the popup.")]
	[SerializeField]
	private TextMeshProUGUI headerText;

	[Tooltip("A reference to the body text of the popup.")]
	[SerializeField]
	private TextMeshProUGUI bodyText;

	[Header("Button text")]
	[SerializeField]
	private string yesText = "$menu_yes";

	[SerializeField]
	private string noText = "$menu_no";

	[SerializeField]
	private string okText = "$menu_ok";

	[SerializeField]
	private string cancelText = "$menu_cancel";

	private TMP_Text buttonLeftText;

	private TMP_Text buttonCenterText;

	private TMP_Text buttonRightText;

	private bool wasClosedThisFrame;

	private Stack<PopupBase> popupStack = new Stack<PopupBase>();

	public static event PopupEnabledHandler OnPopupEnabled;

	private void Awake()
	{
		if ((Object)(object)buttonLeft != (Object)null)
		{
			buttonLeftText = ((Component)buttonLeft).GetComponentInChildren<TMP_Text>();
		}
		if ((Object)(object)buttonCenter != (Object)null)
		{
			buttonCenterText = ((Component)buttonCenter).GetComponentInChildren<TMP_Text>();
		}
		if ((Object)(object)buttonRight != (Object)null)
		{
			buttonRightText = ((Component)buttonRight).GetComponentInChildren<TMP_Text>();
		}
		Hide();
	}

	private void OnEnable()
	{
		if ((Object)(object)instance != (Object)null && (Object)(object)instance != (Object)(object)this)
		{
			ZLog.LogError((object)"Can't have more than one UnifiedPopup component enabled at the same time!");
			return;
		}
		instance = this;
		UnifiedPopup.OnPopupEnabled?.Invoke();
	}

	private void OnDisable()
	{
		if ((Object)(object)instance == (Object)null)
		{
			ZLog.LogError((object)"Instance of UnifiedPopup was already null! This may have happened because you had more than one UnifiedPopup component enabled at the same time, which isn't allowed!");
		}
		else
		{
			instance = null;
		}
	}

	private void LateUpdate()
	{
		while (popupStack.Count > 0 && popupStack.Peek() is LivePopupBase && (popupStack.Peek() as LivePopupBase).ShouldClose)
		{
			Pop();
		}
		if (!IsVisible())
		{
			wasClosedThisFrame = false;
		}
	}

	private static bool InstanceIsNullError()
	{
		if ((Object)(object)instance == (Object)null)
		{
			ZLog.LogError((object)"Can't show popup when there is no enabled UnifiedPopup component in the scene!");
			return true;
		}
		return false;
	}

	public static bool IsAvailable()
	{
		return (Object)(object)instance != (Object)null;
	}

	public static void Push(PopupBase popup)
	{
		if (!InstanceIsNullError())
		{
			instance.popupStack.Push(popup);
			instance.ShowTopmost();
		}
	}

	public static void Pop()
	{
		if (InstanceIsNullError())
		{
			return;
		}
		if (instance.popupStack.Count <= 0)
		{
			ZLog.LogError((object)"Push/pop mismatch! Tried to pop a popup element off the stack when it was empty!");
			return;
		}
		PopupBase popupBase = instance.popupStack.Pop();
		if (popupBase is LivePopupBase)
		{
			((MonoBehaviour)instance).StopCoroutine((popupBase as LivePopupBase).updateCoroutine);
		}
		if (instance.popupStack.Count <= 0)
		{
			instance.Hide();
		}
		else
		{
			instance.ShowTopmost();
		}
	}

	public static void SetFocus()
	{
		if ((Object)(object)instance.buttonCenter != (Object)null && ((Component)instance.buttonCenter).gameObject.activeInHierarchy)
		{
			((Selectable)instance.buttonCenter).Select();
		}
		else if ((Object)(object)instance.buttonRight != (Object)null && ((Component)instance.buttonRight).gameObject.activeInHierarchy)
		{
			((Selectable)instance.buttonRight).Select();
		}
		else if ((Object)(object)instance.buttonLeft != (Object)null && ((Component)instance.buttonLeft).gameObject.activeInHierarchy)
		{
			((Selectable)instance.buttonLeft).Select();
		}
	}

	public static bool IsVisible()
	{
		if (IsAvailable())
		{
			return instance.popupUIParent.activeInHierarchy;
		}
		return false;
	}

	public static bool WasVisibleThisFrame()
	{
		if (!IsVisible())
		{
			if (IsAvailable())
			{
				return instance.wasClosedThisFrame;
			}
			return false;
		}
		return true;
	}

	private void ShowTopmost()
	{
		Show(instance.popupStack.Peek());
	}

	private void Show(PopupBase popup)
	{
		ResetUI();
		switch (popup.Type)
		{
		case PopupType.YesNo:
			ShowYesNo(popup as YesNoPopup);
			break;
		case PopupType.Warning:
			ShowWarning(popup as WarningPopup);
			break;
		case PopupType.Task:
			ShowTask(popup as TaskPopup);
			break;
		case PopupType.CancelableTask:
			ShowCancelableTask(popup as CancelableTaskPopup);
			break;
		}
		popupUIParent.SetActive(true);
		popupUIParent.transform.parent.SetAsLastSibling();
	}

	private void ResetUI()
	{
		((UnityEventBase)buttonLeft.onClick).RemoveAllListeners();
		((UnityEventBase)buttonCenter.onClick).RemoveAllListeners();
		((UnityEventBase)buttonRight.onClick).RemoveAllListeners();
		((Component)buttonLeft).gameObject.SetActive(false);
		((Component)buttonCenter).gameObject.SetActive(false);
		((Component)buttonRight).gameObject.SetActive(false);
	}

	private void ShowYesNo(YesNoPopup popup)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		((TMP_Text)headerText).text = popup.header;
		((TMP_Text)bodyText).text = popup.text;
		buttonRightText.text = Localization.instance.Localize(yesText);
		((Component)buttonRight).gameObject.SetActive(true);
		((UnityEvent)buttonRight.onClick).AddListener((UnityAction)delegate
		{
			popup.yesCallback?.Invoke();
		});
		buttonLeftText.text = Localization.instance.Localize(noText);
		((Component)buttonLeft).gameObject.SetActive(true);
		((UnityEvent)buttonLeft.onClick).AddListener((UnityAction)delegate
		{
			popup.noCallback?.Invoke();
		});
	}

	private void ShowWarning(WarningPopup popup)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		((TMP_Text)headerText).text = popup.header;
		((TMP_Text)bodyText).text = popup.text;
		buttonCenterText.text = Localization.instance.Localize(okText);
		((Component)buttonCenter).gameObject.SetActive(true);
		((UnityEvent)buttonCenter.onClick).AddListener((UnityAction)delegate
		{
			popup.okCallback?.Invoke();
		});
	}

	private void ShowTask(TaskPopup popup)
	{
		((TMP_Text)headerText).text = popup.header;
		((TMP_Text)bodyText).text = popup.text;
	}

	private void ShowCancelableTask(CancelableTaskPopup popup)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		popup.SetTextReferences(headerText, bodyText);
		popup.SetUpdateCoroutineReference(((MonoBehaviour)this).StartCoroutine(popup.updateRoutine));
		buttonCenterText.text = Localization.instance.Localize(cancelText);
		((Component)buttonCenter).gameObject.SetActive(true);
		((UnityEvent)buttonCenter.onClick).AddListener((UnityAction)delegate
		{
			popup.cancelCallback?.Invoke();
			((MonoBehaviour)this).StopCoroutine(popup.updateCoroutine);
		});
	}

	private void Hide()
	{
		wasClosedThisFrame = true;
		popupUIParent.SetActive(false);
	}
}
