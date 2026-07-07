using System;
using UnityEngine;

namespace STUWard;

internal static class WardMapRangeSprites
{
	private const int TextureSize = 512;

	private const int FixedDashCount = 24;

	private const int FixedStrokePixels = 12;

	private const float DashFillFraction = 0.55f;

	private const float RadialFeatherPixels = 1.25f;

	private static Sprite? _cachedSprite;

	private static Texture2D? _createdTexture;

	internal static void Reset()
	{
		if ((Object)(object)_cachedSprite != (Object)null)
		{
			Object.Destroy((Object)(object)_cachedSprite);
			_cachedSprite = null;
		}
		if ((Object)(object)_createdTexture != (Object)null)
		{
			Object.Destroy((Object)(object)_createdTexture);
			_createdTexture = null;
		}
	}

	internal static Sprite? GetRangeSprite(float radius)
	{
		if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
		{
			return null;
		}
		if ((Object)(object)_cachedSprite != (Object)null)
		{
			return _cachedSprite;
		}
		try
		{
			_cachedSprite = CreateSprite();
			return _cachedSprite;
		}
		catch (Exception ex)
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Range", $"Failed to build dashed ward range sprite. dashCount={24}, strokePixels={12}, error={ex.Message}");
			return null;
		}
	}

	private static Sprite CreateSprite()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		int num = 24;
		int num2 = 12;
		Texture2D val = new Texture2D(512, 512, (TextureFormat)5, false)
		{
			name = $"STUWard_RangeRing_Dashed_{num}_{num2}",
			wrapMode = (TextureWrapMode)1,
			filterMode = (FilterMode)1,
			hideFlags = (HideFlags)61
		};
		Color32[] array = (Color32[])(object)new Color32[262144];
		float num3 = 255.5f;
		float num4 = num3 - 2f;
		float innerRadius = Mathf.Max(0f, num4 - (float)num2);
		float num5 = Mathf.Max(1f, (float)Math.PI * 2f * num4);
		float dashFeatherFraction = Mathf.Clamp((float)num / num5, 0.0025f, 0.08f);
		for (int i = 0; i < 512; i++)
		{
			float num6 = (float)i - num3;
			for (int j = 0; j < 512; j++)
			{
				float num7 = (float)j - num3;
				float radialAlpha = GetRadialAlpha(Mathf.Sqrt(num7 * num7 + num6 * num6), innerRadius, num4);
				if (!(radialAlpha <= 0f))
				{
					float dashAlpha = GetDashAlpha(Mathf.Atan2(num6, num7), num, dashFeatherFraction);
					if (!(dashAlpha <= 0f))
					{
						float num8 = Mathf.Clamp01(radialAlpha * dashAlpha);
						array[i * 512 + j] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)Mathf.RoundToInt(num8 * 255f));
					}
				}
			}
		}
		val.SetPixels32(array);
		val.Apply(false, true);
		_createdTexture = val;
		Sprite obj = Sprite.Create(val, new Rect(0f, 0f, 512f, 512f), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0);
		((Object)obj).name = ((Object)val).name;
		((Object)obj).hideFlags = (HideFlags)61;
		return obj;
	}

	private static float GetRadialAlpha(float distance, float innerRadius, float outerRadius)
	{
		if (distance <= innerRadius - 1.25f || distance >= outerRadius + 1.25f)
		{
			return 0f;
		}
		if (distance < innerRadius + 1.25f)
		{
			return Mathf.Clamp01((distance - (innerRadius - 1.25f)) / 2.5f);
		}
		if (distance > outerRadius - 1.25f)
		{
			return Mathf.Clamp01((outerRadius + 1.25f - distance) / 2.5f);
		}
		return 1f;
	}

	private static float GetDashAlpha(float angle, int dashCount, float dashFeatherFraction)
	{
		if (dashCount <= 0)
		{
			return 1f;
		}
		float num = Mathf.Repeat(angle / ((float)Math.PI * 2f), 1f) * (float)dashCount;
		float num2 = num - Mathf.Floor(num);
		if (num2 >= 0.55f)
		{
			return 0f;
		}
		if (dashFeatherFraction <= 0f)
		{
			return 1f;
		}
		float num3 = Mathf.Clamp01(num2 / dashFeatherFraction);
		float num4 = Mathf.Clamp01((0.55f - num2) / dashFeatherFraction);
		return Mathf.Min(num3, num4);
	}
}
