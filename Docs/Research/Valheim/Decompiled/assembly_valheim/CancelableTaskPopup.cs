using System.Collections;
using TMPro;

public class CancelableTaskPopup : LivePopupBase
{
	public readonly PopupButtonCallback cancelCallback;

	public override PopupType Type => PopupType.CancelableTask;

	public CancelableTaskPopup(RetrieveFromStringSource headerRetrievalFunc, RetrieveFromStringSource textRetrievalFunc, RetrieveFromBoolSource shouldCloseRetrievalFunc, PopupButtonCallback cancelCallback)
		: base(headerRetrievalFunc, textRetrievalFunc, shouldCloseRetrievalFunc)
	{
		SetUpdateRoutine(UpdateRoutine());
		this.cancelCallback = cancelCallback;
	}

	private IEnumerator UpdateRoutine()
	{
		while (!shouldCloseRetrievalFunc())
		{
			((TMP_Text)headerText).text = headerRetrievalFunc();
			((TMP_Text)bodyText).text = textRetrievalFunc();
			yield return null;
		}
		base.ShouldClose = true;
	}
}
