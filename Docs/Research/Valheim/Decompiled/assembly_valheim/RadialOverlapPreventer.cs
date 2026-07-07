using UnityEngine;
using Valheim.UI;

public class RadialOverlapPreventer : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_elementInfoElement;

	[SerializeField]
	private RectTransform m_tooltipElement;

	[SerializeField]
	private RectTransform m_parentElement;

	private void Start()
	{
		PreventOverlap();
	}

	private void OnEnable()
	{
		PreventOverlap();
	}

	private void PreventOverlap()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = m_parentElement.rect;
		float num = ((Rect)(ref rect)).width * 0.5f;
		float x = AnchoredPositionAt(m_tooltipElement.anchoredPosition, m_tooltipElement.anchorMin, new Vector2(0.5f, 0.5f)).x;
		rect = m_tooltipElement.rect;
		float num2 = x + ((Rect)(ref rect)).width * (1f - m_tooltipElement.pivot.x);
		float num3 = ((RadialData.SO.RadialSize == RadialSizeSetting.Big) ? RadialData.SO.RadialBigScale : RadialData.SO.RadialSmallScale);
		float num4 = RadialData.SO.RadialReferenceWidth * num3;
		float num5 = num - num2;
		int num6 = ((RadialData.SO.RadialSize == RadialSizeSetting.SmallEdge) ? 1 : 0);
		float num7 = Mathf.Clamp(Vector2.right.x * (float)num6 * num, num2, num);
		Vector2 val = AnchoredPositionAt(m_elementInfoElement.anchoredPosition, m_elementInfoElement.anchorMin, new Vector2(0.5f, 0.5f));
		rect = m_elementInfoElement.rect;
		Vector2 anchoredPosition = val + ((Rect)(ref rect)).size * (new Vector2(0.5f, 0.5f) - m_elementInfoElement.pivot);
		((Transform)m_elementInfoElement).localScale = Vector3.one * num3;
		if (num4 <= num5)
		{
			float num8 = num4 * 0.5f;
			float num9 = num2 + num8;
			float num10 = num - num8;
			anchoredPosition.x = Mathf.Clamp(num7, num9, num10);
		}
		else
		{
			float num11 = (num7 - num2) * 2f / RadialData.SO.RadialReferenceWidth;
			float num12 = (num - num7) * 2f / RadialData.SO.RadialReferenceWidth;
			float num13 = Mathf.Min(num11, num12);
			float num14 = Mathf.Min(num3, num13);
			((Transform)m_elementInfoElement).localScale = new Vector3(num14, num14, num14);
			float num15 = RadialData.SO.RadialReferenceWidth * num14 * 0.5f;
			anchoredPosition.x = Mathf.Clamp(num7, num2 + num15, num - num15);
		}
		m_elementInfoElement.anchoredPosition = anchoredPosition;
	}

	private Vector2 AnchoredPositionAt(Vector2 currentPosition, Vector2 currentAnchor, Vector2 targetAnchor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = m_parentElement.rect;
		Vector2 size = ((Rect)(ref rect)).size;
		Vector2 val = targetAnchor - currentAnchor;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(size.x * val.x, size.y * val.y);
		return currentPosition - val2;
	}
}
