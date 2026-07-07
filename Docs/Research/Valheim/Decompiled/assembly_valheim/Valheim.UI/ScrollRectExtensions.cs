using UnityEngine;
using UnityEngine.UI;

namespace Valheim.UI;

public static class ScrollRectExtensions
{
	public static void SnapToChild(this ScrollRect scrollRect, RectTransform child)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Component)scrollRect.viewport).transform.InverseTransformPoint(((Transform)child).position));
		Rect rect = scrollRect.viewport.rect;
		float height = ((Rect)(ref rect)).height;
		bool num = val.y > 0f;
		float num2 = 0f - val.y;
		rect = child.rect;
		bool flag = num2 + ((Rect)(ref rect)).height > height;
		float num3;
		if (!num)
		{
			if (!flag)
			{
				num3 = 0f;
			}
			else
			{
				float num4 = 0f - val.y;
				rect = child.rect;
				num3 = num4 + ((Rect)(ref rect)).height - height;
			}
		}
		else
		{
			num3 = 0f - val.y;
		}
		float num5 = num3;
		scrollRect.content.anchoredPosition = new Vector2(0f, scrollRect.content.anchoredPosition.y + num5);
	}

	public static bool IsVisible(this ScrollRect scrollRect, RectTransform child)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = scrollRect.viewport.rect;
		float height = ((Rect)(ref rect)).height;
		Vector2 val = Vector2.op_Implicit(((Component)scrollRect.viewport).transform.InverseTransformPoint(((Transform)child).position));
		if (val.y < 0f)
		{
			float num = 0f - val.y;
			rect = child.rect;
			return num + ((Rect)(ref rect)).height < height;
		}
		return false;
	}
}
