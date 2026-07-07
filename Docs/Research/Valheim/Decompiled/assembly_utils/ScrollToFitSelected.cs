using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollToFitSelected : MonoBehaviour
{
	[SerializeField]
	private RectTransform _viewport;

	[SerializeField]
	private RectTransform _content;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private float _spacing;

	[SerializeField]
	private float _scrollTime;

	private float _scrollVelocity;

	private float _targetY = 1f;

	private RectTransform _cachedSelected;

	private void LateUpdate()
	{
		if (ZInput.IsGamepadActive() && !Mathf.Approximately(_scrollRect.verticalNormalizedPosition, _targetY))
		{
			_scrollRect.verticalNormalizedPosition = Mathf.SmoothDamp(_scrollRect.verticalNormalizedPosition, _targetY, ref _scrollVelocity, _scrollTime, float.PositiveInfinity, Time.unscaledDeltaTime);
		}
	}

	private void OnGUI()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (!ZInput.IsGamepadActive())
		{
			return;
		}
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if ((Object)(object)currentSelectedGameObject == (Object)null || !currentSelectedGameObject.transform.IsChildOf((Transform)(object)_content))
		{
			return;
		}
		RectTransform component = currentSelectedGameObject.GetComponent<RectTransform>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)component == (Object)(object)_cachedSelected))
		{
			_cachedSelected = component;
			Rect rect = _viewport.rect;
			Rect val = _cachedSelected.rect.Transform((Transform)(object)_cachedSelected).InverseTransform((Transform)(object)_viewport);
			float num = ((Rect)(ref val)).yMax - ((Rect)(ref rect)).yMax + _spacing;
			float num2 = ((Rect)(ref rect)).yMin - ((Rect)(ref val)).yMin + _spacing;
			if (num < 0f)
			{
				num = 0f;
			}
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			float num3 = ((num > 0f) ? num : (0f - num2));
			if (num3 != 0f)
			{
				Rect val2 = _content.rect.Transform((Transform)(object)_content).InverseTransform((Transform)(object)_viewport);
				float num4 = ((Rect)(ref val2)).height - ((Rect)(ref rect)).height;
				float num5 = 1f / num4;
				_targetY = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + num3 * num5);
				_scrollVelocity = 0f;
			}
		}
	}

	private void OnEnable()
	{
		_scrollVelocity = 0f;
		_targetY = 1f;
	}
}
