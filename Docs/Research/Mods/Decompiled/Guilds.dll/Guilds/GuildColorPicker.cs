using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

public class GuildColorPicker : MonoBehaviour
{
	private delegate void ColorEvent(Color c);

	private sealed class HSV
	{
		public double H;

		public double S = 1.0;

		public double V = 1.0;

		private const byte A = byte.MaxValue;

		public HSV()
		{
		}

		public HSV(double h, double s, double v)
		{
			H = h;
			S = s;
			V = v;
		}

		public HSV(Color color)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
			float num2 = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
			float num3 = (float)H;
			if (num2 != num)
			{
				num3 = ((num == color.r) ? ((color.g - color.b) / (num - num2)) : ((num != color.g) ? (4f + (color.r - color.g) / (num - num2)) : (2f + (color.b - color.r) / (num - num2)))) * 60f;
				if (num3 < 0f)
				{
					num3 += 360f;
				}
			}
			H = num3;
			S = ((num == 0f) ? 0.0 : (1.0 - (double)num2 / (double)num));
			V = num;
		}

		public Color32 ToColor()
		{
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			int num = Convert.ToInt32(Math.Floor(H / 60.0)) % 6;
			double num2 = H / 60.0 - Math.Floor(H / 60.0);
			double num3 = V * 255.0;
			byte b = (byte)Convert.ToInt32(num3);
			byte b2 = (byte)Convert.ToInt32(num3 * (1.0 - S));
			byte b3 = (byte)Convert.ToInt32(num3 * (1.0 - num2 * S));
			byte b4 = (byte)Convert.ToInt32(num3 * (1.0 - (1.0 - num2) * S));
			return (Color32)(num switch
			{
				0 => new Color32(b, b4, b2, byte.MaxValue), 
				1 => new Color32(b3, b, b2, byte.MaxValue), 
				2 => new Color32(b2, b, b4, byte.MaxValue), 
				3 => new Color32(b2, b3, b, byte.MaxValue), 
				4 => new Color32(b4, b2, b, byte.MaxValue), 
				5 => new Color32(b, b2, b3, byte.MaxValue), 
				_ => default(Color32), 
			});
		}
	}

	private static GuildColorPicker? self;

	private static bool done = true;

	private static ColorEvent? onCC;

	private static ColorEvent? onCS;

	private static Color32 originalColor;

	private static Color32 modifiedColor;

	private static HSV modifiedHsv = null;

	public string chosenColor = "#000000";

	private bool interact;

	public RectTransform positionIndicator;

	public Slider mainComponent;

	public Slider rComponent;

	public Slider gComponent;

	public Slider bComponent;

	public TMP_InputField hexComponent;

	public RawImage colorComponent;

	public Image chosenColorPreview;

	private void Awake()
	{
		self = this;
	}

	public void OnColorButtonClicked()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Color original = default(Color);
		ColorUtility.TryParseHtmlString(chosenColor, ref original);
		Create(original, SetColor, ColorFinished);
	}

	private void SetColor(Color currentColor)
	{
	}

	private void ColorFinished(Color finishedColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)chosenColorPreview).color = finishedColor;
		chosenColor = "#" + ColorUtility.ToHtmlStringRGBA(finishedColor);
	}

	private static void Create(Color original, ColorEvent onColorChanged, ColorEvent onColorSelected)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (self != null)
		{
			if (done)
			{
				done = false;
				originalColor = Color32.op_Implicit(original);
				modifiedColor = Color32.op_Implicit(original);
				onCC = onColorChanged;
				onCS = onColorSelected;
				((Component)self).gameObject.SetActive(true);
				self.RecalculateMenu(recalculateHSV: true);
				((TMP_Text)((Component)self.hexComponent.placeholder).GetComponent<TextMeshProUGUI>()).text = "RRGGBB";
			}
			else
			{
				Done();
			}
		}
	}

	private void RecalculateMenu(bool recalculateHSV)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		interact = false;
		if (recalculateHSV)
		{
			modifiedHsv = new HSV(Color32.op_Implicit(modifiedColor));
		}
		else
		{
			modifiedColor = modifiedHsv.ToColor();
		}
		rComponent.value = (int)modifiedColor.r;
		((Component)((Component)rComponent).transform.GetChild(3)).GetComponent<TMP_InputField>().text = modifiedColor.r.ToString();
		gComponent.value = (int)modifiedColor.g;
		((Component)((Component)gComponent).transform.GetChild(3)).GetComponent<TMP_InputField>().text = modifiedColor.g.ToString();
		bComponent.value = (int)modifiedColor.b;
		((Component)((Component)bComponent).transform.GetChild(3)).GetComponent<TMP_InputField>().text = modifiedColor.b.ToString();
		mainComponent.value = (float)modifiedHsv.H;
		((Graphic)((Component)((Component)rComponent).transform.GetChild(0)).GetComponent<RawImage>()).color = Color32.op_Implicit(new Color32(byte.MaxValue, modifiedColor.g, modifiedColor.b, byte.MaxValue));
		((Graphic)((Component)((Component)rComponent).transform.GetChild(0).GetChild(0)).GetComponent<RawImage>()).color = Color32.op_Implicit(new Color32((byte)0, modifiedColor.g, modifiedColor.b, byte.MaxValue));
		((Graphic)((Component)((Component)gComponent).transform.GetChild(0)).GetComponent<RawImage>()).color = Color32.op_Implicit(new Color32(modifiedColor.r, byte.MaxValue, modifiedColor.b, byte.MaxValue));
		((Graphic)((Component)((Component)gComponent).transform.GetChild(0).GetChild(0)).GetComponent<RawImage>()).color = Color32.op_Implicit(new Color32(modifiedColor.r, (byte)0, modifiedColor.b, byte.MaxValue));
		((Graphic)((Component)((Component)bComponent).transform.GetChild(0)).GetComponent<RawImage>()).color = Color32.op_Implicit(new Color32(modifiedColor.r, modifiedColor.g, byte.MaxValue, byte.MaxValue));
		((Graphic)((Component)((Component)bComponent).transform.GetChild(0).GetChild(0)).GetComponent<RawImage>()).color = Color32.op_Implicit(new Color32(modifiedColor.r, modifiedColor.g, (byte)0, byte.MaxValue));
		((Graphic)((Component)((Transform)positionIndicator).parent.GetChild(0)).GetComponent<RawImage>()).color = Color32.op_Implicit(new HSV(modifiedHsv.H, 1.0, 1.0).ToColor());
		positionIndicator.anchorMin = new Vector2((float)modifiedHsv.S, (float)modifiedHsv.V);
		positionIndicator.anchorMax = positionIndicator.anchorMin;
		hexComponent.text = ColorUtility.ToHtmlStringRGB(Color32.op_Implicit(modifiedColor));
		((Graphic)colorComponent).color = Color32.op_Implicit(modifiedColor);
		onCC?.Invoke(Color32.op_Implicit(modifiedColor));
		interact = true;
	}

	public void SetChooser()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Transform)positionIndicator).parent;
		Vector2 val = default(Vector2);
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)((parent is RectTransform) ? parent : null), Vector2.op_Implicit(Input.mousePosition), ((Component)this).GetComponentInParent<Canvas>().worldCamera, ref val);
		val = Rect.PointToNormalized(((RectTransform)((parent is RectTransform) ? parent : null)).rect, val);
		if (positionIndicator.anchorMin != val)
		{
			positionIndicator.anchorMin = val;
			positionIndicator.anchorMax = val;
			modifiedHsv.S = val.x;
			modifiedHsv.V = val.y;
			RecalculateMenu(recalculateHSV: false);
		}
	}

	public void SetMain(float value)
	{
		if (interact)
		{
			modifiedHsv.H = value;
			RecalculateMenu(recalculateHSV: false);
		}
	}

	public void SetR(float value)
	{
		if (interact)
		{
			modifiedColor.r = (byte)value;
			RecalculateMenu(recalculateHSV: true);
		}
	}

	public void SetR(string value)
	{
		if (interact)
		{
			modifiedColor.r = (byte)Mathf.Clamp(int.Parse(value), 0, 255);
			RecalculateMenu(recalculateHSV: true);
		}
	}

	public void SetG(float value)
	{
		if (interact)
		{
			modifiedColor.g = (byte)value;
			RecalculateMenu(recalculateHSV: true);
		}
	}

	public void SetG(string value)
	{
		if (interact)
		{
			modifiedColor.g = (byte)Mathf.Clamp(int.Parse(value), 0, 255);
			RecalculateMenu(recalculateHSV: true);
		}
	}

	public void SetB(float value)
	{
		if (interact)
		{
			modifiedColor.b = (byte)value;
			RecalculateMenu(recalculateHSV: true);
		}
	}

	public void SetB(string value)
	{
		if (interact)
		{
			modifiedColor.b = (byte)Mathf.Clamp(int.Parse(value), 0, 255);
			RecalculateMenu(recalculateHSV: true);
		}
	}

	public void SetHexa(string value)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (interact)
		{
			Color val = default(Color);
			if (ColorUtility.TryParseHtmlString("#" + value, ref val))
			{
				val.a = 1f;
				modifiedColor = Color32.op_Implicit(val);
				RecalculateMenu(recalculateHSV: true);
			}
			else
			{
				hexComponent.text = ColorUtility.ToHtmlStringRGB(Color32.op_Implicit(modifiedColor));
			}
		}
	}

	public void CCancel()
	{
		Cancel();
	}

	private static void Cancel()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		modifiedColor = originalColor;
		Done();
	}

	public void CDone()
	{
		Done();
	}

	private static void Done()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		done = true;
		onCC?.Invoke(Color32.op_Implicit(modifiedColor));
		onCS?.Invoke(Color32.op_Implicit(modifiedColor));
		((Component)((Component)self).transform).gameObject.SetActive(false);
	}
}
