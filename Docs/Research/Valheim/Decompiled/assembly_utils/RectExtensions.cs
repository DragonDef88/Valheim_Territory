using UnityEngine;

internal static class RectExtensions
{
	public static Rect Transform(this Rect r, Transform transform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Rect result = default(Rect);
		((Rect)(ref result)).min = Vector2.op_Implicit(transform.TransformPoint(Vector2.op_Implicit(((Rect)(ref r)).min)));
		((Rect)(ref result)).max = Vector2.op_Implicit(transform.TransformPoint(Vector2.op_Implicit(((Rect)(ref r)).max)));
		return result;
	}

	public static Rect InverseTransform(this Rect r, Transform transform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Rect result = default(Rect);
		((Rect)(ref result)).min = Vector2.op_Implicit(transform.InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref r)).min)));
		((Rect)(ref result)).max = Vector2.op_Implicit(transform.InverseTransformPoint(Vector2.op_Implicit(((Rect)(ref r)).max)));
		return result;
	}
}
