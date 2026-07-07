using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectEnsureVisible : MonoBehaviour
{
	private RectTransform maskTransform;

	private ScrollRect mScrollRect;

	private RectTransform mScrollTransform;

	private RectTransform mContent;

	private bool mInitialized;

	private void Awake()
	{
		if (!mInitialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		mScrollRect = ((Component)this).GetComponent<ScrollRect>();
		ref RectTransform reference = ref mScrollTransform;
		Transform transform = ((Component)mScrollRect).transform;
		reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		mContent = mScrollRect.content;
		Reset();
		mInitialized = true;
	}

	public void CenterOnItem(RectTransform target)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		if (!mInitialized)
		{
			Initialize();
		}
		Vector3 worldPointInWidget = GetWorldPointInWidget(mScrollTransform, GetWidgetWorldPoint(target));
		Vector3 val = GetWorldPointInWidget(mScrollTransform, GetWidgetWorldPoint(maskTransform)) - worldPointInWidget;
		val.z = 0f;
		if (!mScrollRect.horizontal)
		{
			val.x = 0f;
		}
		if (!mScrollRect.vertical)
		{
			val.y = 0f;
		}
		float x = val.x;
		Rect rect = mContent.rect;
		float x2 = ((Rect)(ref rect)).size.x;
		rect = mScrollTransform.rect;
		float num = x / (x2 - ((Rect)(ref rect)).size.x);
		float y = val.y;
		rect = mContent.rect;
		float y2 = ((Rect)(ref rect)).size.y;
		rect = mScrollTransform.rect;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(num, y / (y2 - ((Rect)(ref rect)).size.y));
		Vector2 val3 = mScrollRect.normalizedPosition - val2;
		if ((int)mScrollRect.movementType != 0)
		{
			val3.x = Mathf.Clamp01(val3.x);
			val3.y = Mathf.Clamp01(val3.y);
		}
		mScrollRect.normalizedPosition = val3;
	}

	private void Reset()
	{
		if (!((Object)(object)maskTransform == (Object)null))
		{
			return;
		}
		Mask componentInChildren = ((Component)this).GetComponentInChildren<Mask>(true);
		if (Object.op_Implicit((Object)(object)componentInChildren))
		{
			maskTransform = componentInChildren.rectTransform;
		}
		if ((Object)(object)maskTransform == (Object)null)
		{
			RectMask2D componentInChildren2 = ((Component)this).GetComponentInChildren<RectMask2D>(true);
			if (Object.op_Implicit((Object)(object)componentInChildren2))
			{
				maskTransform = componentInChildren2.rectTransform;
			}
		}
	}

	private Vector3 GetWidgetWorldPoint(RectTransform target)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.5f - target.pivot.x;
		Rect rect = target.rect;
		float num2 = num * ((Rect)(ref rect)).size.x;
		float num3 = 0.5f - target.pivot.y;
		rect = target.rect;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num2, num3 * ((Rect)(ref rect)).size.y, 0f);
		Vector3 val2 = ((Transform)target).localPosition + val;
		return ((Transform)target).parent.TransformPoint(val2);
	}

	private Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return ((Transform)target).InverseTransformPoint(worldPoint);
	}
}
