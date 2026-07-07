using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Valheim.UI;

public class Selector : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	public UnityEvent OnLeftButtonClickedEvent;

	public UnityEvent OnRightButtonClickedEvent;

	public void SetText(string text)
	{
		if ((Object)(object)label != (Object)null)
		{
			((TMP_Text)label).text = text;
		}
	}

	public void OnLeftButtonClicked()
	{
		UnityEvent onLeftButtonClickedEvent = OnLeftButtonClickedEvent;
		if (onLeftButtonClickedEvent != null)
		{
			onLeftButtonClickedEvent.Invoke();
		}
	}

	public void OnRightButtonClicked()
	{
		UnityEvent onRightButtonClickedEvent = OnRightButtonClickedEvent;
		if (onRightButtonClickedEvent != null)
		{
			onRightButtonClickedEvent.Invoke();
		}
	}
}
