using System;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.UI;

public class RadialMenuElement : MonoBehaviour
{
	protected const string c_HOVER_SUFFIX = "_hov";

	protected const string c_NUDGE_SUFFIX = "_nug";

	[SerializeField]
	protected Image m_icon;

	[SerializeField]
	protected Image m_background;

	[SerializeField]
	protected CanvasGroup m_canvasGroup;

	private Material m_backgroundMaterial;

	private RectTransform m_rectTransform;

	public Material BackgroundMaterial
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			if (Object.op_Implicit((Object)(object)m_backgroundMaterial))
			{
				return m_backgroundMaterial;
			}
			m_backgroundMaterial = new Material(((Graphic)Background).material);
			((Graphic)Background).material = m_backgroundMaterial;
			return m_backgroundMaterial;
		}
	}

	public RectTransform ElementTransform
	{
		get
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)m_rectTransform))
			{
				return m_rectTransform;
			}
			ref RectTransform rectTransform = ref m_rectTransform;
			Transform transform = ((Component)this).transform;
			rectTransform = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			RectTransform rectTransform2 = m_rectTransform;
			RectTransform rectTransform3 = m_rectTransform;
			RectTransform rectTransform4 = m_rectTransform;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(0.5f, 0.5f);
			rectTransform4.pivot = val;
			Vector2 anchorMin = (rectTransform3.anchorMax = val);
			rectTransform2.anchorMin = anchorMin;
			((Transform)m_rectTransform).GetChild(1).eulerAngles = Vector3.zero;
			return m_rectTransform;
		}
	}

	public Vector3 LocalPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Transform)ElementTransform).localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Transform)ElementTransform).localPosition = value;
		}
	}

	public Image Icon => m_icon;

	public Image Background => m_background;

	public string Name { get; protected set; }

	public string SubTitle { get; protected set; }

	public string Description { get; protected set; }

	public string ID => ((Object)((Component)this).gameObject).GetInstanceID().ToString();

	public Func<RadialBase, RadialArray<RadialMenuElement>, bool> AdvancedCloseOnInteract { get; set; }

	public Func<bool> CloseOnInteract { get; set; } = () => false;


	public Func<bool> Interact { get; set; }

	public Func<bool> SecondaryInteract { get; set; }

	public Func<RadialBase, int, bool> TryOpenSubRadial { get; set; }

	public float Scale
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Transform)ElementTransform).localScale.x;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			((Transform)ElementTransform).localScale = Vector3.one * value;
		}
	}

	public float Alpha
	{
		get
		{
			return m_canvasGroup.alpha;
		}
		set
		{
			m_canvasGroup.alpha = value;
			UnselectedColorAlpha = value;
			ActivatedColorAlpha = value;
			QueuedColorAlpha = value;
		}
	}

	public float UnselectedColorAlpha
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return BackgroundMaterial.GetColor("_UnselectedColor").a;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			Color color = BackgroundMaterial.GetColor("_UnselectedColor");
			float a = Mathf.Clamp(value, 0f, 0.8f);
			color.a = a;
			BackgroundMaterial.SetColor("_UnselectedColor", color);
		}
	}

	public float ActivatedColorAlpha
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return BackgroundMaterial.GetColor("_ActivatedColor").a;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			Color color = BackgroundMaterial.GetColor("_ActivatedColor");
			float a = Mathf.Clamp(value, 0f, 0.8f);
			color.a = a;
			BackgroundMaterial.SetColor("_ActivatedColor", color);
		}
	}

	public float QueuedColorAlpha
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return BackgroundMaterial.GetColor("_QueuedColor").a;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			Color color = BackgroundMaterial.GetColor("_QueuedColor");
			float a = Mathf.Clamp(value, 0f, 0.8f);
			color.a = a;
			BackgroundMaterial.SetColor("_QueuedColor", color);
		}
	}

	public bool Selected
	{
		get
		{
			return BackgroundMaterial.GetInt("_Selected") == 1;
		}
		set
		{
			BackgroundMaterial.SetInt("_Selected", value ? 1 : 0);
		}
	}

	public float Activated
	{
		get
		{
			return BackgroundMaterial.GetFloat("_Activated");
		}
		set
		{
			BackgroundMaterial.SetFloat("_Activated", value);
		}
	}

	public bool Queued
	{
		get
		{
			return BackgroundMaterial.GetInt("_Queued") == 1;
		}
		set
		{
			BackgroundMaterial.SetInt("_Queued", value ? 1 : 0);
		}
	}

	public float Hovering
	{
		get
		{
			return BackgroundMaterial.GetFloat("_Hovering");
		}
		set
		{
			m_backgroundMaterial.SetFloat("_Hovering", value);
		}
	}

	internal void OpenAnimation(RadialMenuAnimationManager manager, string id, float duration, float distance, float startOffset, EasingType alphaEasingType, EasingType positionEasingType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = LocalPosition;
		LocalPosition = ((Vector3)(ref localPosition)).normalized * (distance + startOffset);
		Func<Vector3> get = () => LocalPosition;
		Action<Vector3> set = delegate(Vector3 val)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			LocalPosition = val;
		};
		localPosition = LocalPosition;
		manager.StartTween<Vector3>(get, set, id, ((Vector3)(ref localPosition)).normalized * distance, duration, positionEasingType, (Action)null, (Action)null);
		float alpha = Alpha;
		Alpha = 0f;
		manager.StartTween(() => Alpha, delegate(float val)
		{
			Alpha = val;
		}, id, alpha, duration + 0.1f, alphaEasingType);
	}

	internal void CloseAnimation(RadialMenuAnimationManager manager, string id, float duration, float distance, float startOffset, EasingType alphaEasingType, EasingType positionEasingType)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Func<Vector3> get = () => LocalPosition;
		Action<Vector3> set = delegate(Vector3 val)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			LocalPosition = val;
		};
		Vector3 localPosition = LocalPosition;
		manager.StartTween<Vector3>(get, set, id, ((Vector3)(ref localPosition)).normalized * (distance + startOffset), duration + 0.1f, positionEasingType, (Action)null, (Action)null);
		manager.StartTween(() => Alpha, delegate(float val)
		{
			Alpha = val;
		}, id, 0f, duration + 0.1f, alphaEasingType);
	}

	internal void StartHoverSelect(RadialMenuAnimationManager manager, float duration, EasingType easingType, Action onEnd)
	{
		manager.StartUniqueTween(() => Hovering, delegate(float val)
		{
			Hovering = val;
		}, ID + "_hov", 1f, (Hovering > 0f) ? (duration - duration * Hovering) : duration, easingType, onEnd);
	}

	internal void ResetHoverSelect(RadialMenuAnimationManager manager, float duration, EasingType easingType)
	{
		manager.StartUniqueTween(() => Hovering, delegate(float val)
		{
			Hovering = val;
		}, ID + "_hov", 0f, Hovering * duration, easingType);
	}

	internal void StartNudge(RadialMenuAnimationManager manager, float distance, float duration, EasingType easingType)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Func<Vector3> get = () => LocalPosition;
		Action<Vector3> set = delegate(Vector3 val)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			LocalPosition = val;
		};
		string id = ID + "_nug";
		Vector3 localPosition = LocalPosition;
		manager.StartUniqueTween<Vector3>(get, set, id, ((Vector3)(ref localPosition)).normalized * distance, duration, easingType, (Action)null, (Action)null);
	}

	internal void ResetNudge(RadialMenuAnimationManager manager, float distance, float duration, EasingType easingType)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Func<Vector3> get = () => LocalPosition;
		Action<Vector3> set = delegate(Vector3 val)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			LocalPosition = val;
		};
		string id = ID + "_nug";
		Vector3 localPosition = LocalPosition;
		manager.StartUniqueTween<Vector3>(get, set, id, ((Vector3)(ref localPosition)).normalized * distance, duration, easingType, (Action)null, (Action)null);
	}

	internal void EndNudge(RadialMenuAnimationManager manager)
	{
		manager.EndTweens(ID + "_nug");
	}

	public void SetSegment(int segments)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = LocalPosition;
		SetSegment(UIMath.DirectionToAngleDegrees(Vector2.op_Implicit(((Vector3)(ref localPosition)).normalized)) / 360f, segments);
	}

	public void SetSegment(int index, int segments)
	{
		BackgroundMaterial.SetInt("_Segments", segments);
		BackgroundMaterial.SetFloat("_Offset", (float)index / (float)segments);
	}

	public void SetSegment(float offset, int segments)
	{
		offset = Math.Clamp(offset, 0f, 1f);
		BackgroundMaterial.SetInt("_Segments", segments);
		BackgroundMaterial.SetFloat("_Offset", offset);
	}
}
