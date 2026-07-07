using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 To(this Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return b - a;
	}

	public static Vector3 DirTo(this Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Normalize(b - a);
	}

	public static float DistanceTo(this Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(a, b);
	}

	public static Vector3 Horizontal(this Vector3 a)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		a.y = 0f;
		return a;
	}

	public static Vector3 Vertical(this Vector3 a)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		a.x = 0f;
		a.z = 0f;
		return a;
	}

	public static Color ToColor(this Vector3 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Color(c.x, c.y, c.z);
	}
}
