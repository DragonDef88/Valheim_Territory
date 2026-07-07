using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valheim.UI;

public class RadialBase : MonoBehaviour
{
	public const bool c_EmoteOnlyMode = true;

	[Header("References")]
	[SerializeField]
	private ElementInfo m_elementInfo;

	[SerializeField]
	private RectTransform m_elementContainer;

	[SerializeField]
	private RectTransform m_cursor;

	[SerializeField]
	private RectTransform m_highlighter;

	private const string c_TOGGLE_ID = "_tog";

	private const string c_HOVER_SUFFIX = "_hov";

	private Player m_localPlayerRef;

	private Stack<IRadialConfig> m_backConfigs = new Stack<IRadialConfig>();

	private RadialArray<RadialMenuElement> m_elements;

	private readonly RadialMenuAnimationManager m_animationManager = new RadialMenuAnimationManager();

	private (IRadialConfig openConfig, IRadialConfig backConfig) m_queuedOpenConfigs = (openConfig: null, backConfig: null);

	private Vector2 m_previousInput;

	private bool m_radialHasItemElements;

	private bool m_reFade;

	private bool m_cursorLocked;

	private bool m_isAnimating;

	private bool m_isClosing;

	private bool m_closeQueued;

	private int m_nrOfLayers;

	private int m_currentLayer;

	private int m_index;

	private int m_closestPoint;

	private int m_previousClosestPoint;

	private int m_inputDirection;

	private float m_cursorThreshold;

	private float m_cursorTarget;

	private float m_previousCursorTarget;

	private float m_fadeAnchor;

	private float m_layerFadeThreshold;

	private float m_layerFadeDistance;

	private float m_layerShowDistance;

	private float m_segmentSize;

	private float m_inputAngle;

	private string m_cursorID;

	private RadialMenuElement m_selected;

	private RadialMenuElement m_lastUsed;

	private bool m_shouldAnimateIn;

	public Action<float> OnInteractionDelay { get; set; }

	public Func<Vector2> GetControllerDirection { get; set; }

	public Func<Vector2> GetMouseDirection { get; set; }

	public Func<bool> GetThrow { get; set; }

	public Func<bool> GetConfirm { get; set; }

	public Func<bool> GetOpenThrowMenu { get; set; }

	public Func<bool> GetBack { get; set; }

	public Func<bool> GetClose { get; set; }

	public Func<bool> GetFlick { get; set; }

	public Func<bool> GetDoubleTap { get; set; }

	public Func<bool> GetReleaseToUse { get; set; }

	public GameObject HoverObject { get; set; }

	public IRadialConfig CurrentConfig { get; private set; }

	public RadialMenuElement Selected
	{
		get
		{
			return m_selected;
		}
		private set
		{
			if (!((Object)(object)value == (Object)(object)m_selected))
			{
				RadialMenuElement selected = m_selected;
				m_selected = value;
				OnSelectedUpdate(selected, m_selected);
			}
		}
	}

	public RadialMenuElement LastUsed
	{
		get
		{
			return m_lastUsed;
		}
		private set
		{
			if (Object.op_Implicit((Object)(object)m_lastUsed) && (Object)(object)m_lastUsed != (Object)(object)value)
			{
				if (m_elements.GetArray.Contains(m_lastUsed))
				{
					((Component)m_lastUsed).transform.SetParent((Transform)(object)m_elementContainer);
				}
				else
				{
					Object.Destroy((Object)(object)((Component)m_lastUsed).gameObject);
				}
			}
			if (value is ItemElement itemElement && !m_localPlayerRef.GetInventory().ContainsItem(itemElement.m_data))
			{
				m_lastUsed = null;
			}
			else
			{
				m_lastUsed = value;
			}
		}
	}

	public Vector2 InfoPosition => Vector2.op_Implicit(((Transform)m_elementInfo.InfoTransform).position);

	public bool InRadialBlockingPlaceMode
	{
		get
		{
			if (Object.op_Implicit((Object)(object)m_localPlayerRef) && m_localPlayerRef.InPlaceMode())
			{
				return !m_localPlayerRef.InRepairMode();
			}
			return false;
		}
	}

	public bool CanOpen { get; set; } = true;


	public bool ShouldAnimateIn
	{
		set
		{
			m_shouldAnimateIn = RadialData.SO.EnableToggleAnimation && value;
		}
	}

	public bool ShouldNudgeSelected
	{
		get
		{
			if (RadialData.SO.EnableToggleAnimation && RadialData.SO.NudgeSelectedElement)
			{
				if (RadialData.SO.SpiralEffectInsensity != 0)
				{
					if (m_nrOfLayers > 0)
					{
						return !(RadialData.SO.ElementScaleFactor > 0f);
					}
					return true;
				}
				return true;
			}
			return false;
		}
	}

	public bool CanThrow { get; set; }

	public bool IsBlockingInput { get; private set; }

	public bool IsRefresh { get; private set; }

	public bool IsThrowMenu => CurrentConfig is ThrowGroupConfig;

	public bool IsTopLevel => m_backConfigs.Count <= 0;

	public bool IsHoverMenu => (Object)(object)HoverObject != (Object)null;

	public bool ShowThrowHint
	{
		get
		{
			if (!(CurrentConfig is ValheimRadialConfig) || LastUsed is ItemElement)
			{
				return !(CurrentConfig is ThrowGroupConfig);
			}
			return false;
		}
	}

	public bool UseHoverSelect => RadialData.SO.HoverSelectSpeed > 0f;

	public bool CanDoubleClick
	{
		get
		{
			if (RadialData.SO.EnableDoubleClick && !RadialData.SO.EnableFlick)
			{
				return ZInput.IsGamepadActive();
			}
			return false;
		}
	}

	public bool CanFlick
	{
		get
		{
			if (RadialData.SO.EnableFlick && !RadialData.SO.EnableDoubleClick)
			{
				return ZInput.IsGamepadActive();
			}
			return false;
		}
	}

	public bool Active
	{
		get
		{
			return ((Component)this).gameObject.activeSelf;
		}
		private set
		{
			m_isClosing = false;
			m_elementInfo.Clear();
			((Component)m_cursor).gameObject.SetActive(false);
			((Component)this).gameObject.SetActive(value);
			GameCamera.instance?.UpdateMouseCapture();
		}
	}

	public int MaxElementsPerLayer { get; private set; }

	public int StartItemIndex { get; set; } = -1;


	public int StartOffset { get; set; }

	public int BackIndex { get; set; }

	private void Awake()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = default(RectTransform);
		if (((Component)((Component)m_cursor).transform.GetChild(0)).TryGetComponent<RectTransform>(ref val))
		{
			((Transform)val).localPosition = Vector3.up * RadialData.SO.CursorDistance;
		}
		m_cursorID = ((Object)((Component)m_cursor).gameObject).GetInstanceID().ToString();
		Active = false;
	}

	public void QueuedOpen(IRadialConfig config, IRadialConfig backConfig = null)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		m_queuedOpenConfigs = (openConfig: config, backConfig: backConfig);
		if (!IsRefresh && !(config is ThrowGroupConfig) && StartItemIndex < 0 && Object.op_Implicit((Object)(object)m_selected) && (!RadialData.SO.DefaultToBackButtonOnNewPage || !(m_previousInput == Vector2.zero)))
		{
			StartItemIndex = m_elements.IndexOf(m_selected) + StartOffset;
		}
	}

	public void Open(IRadialConfig config, IRadialConfig backConfig = null)
	{
		if (!CanOpen)
		{
			return;
		}
		ResetExclusiveVariables();
		m_localPlayerRef = Player.m_localPlayer;
		if (InRadialBlockingPlaceMode && !Active)
		{
			return;
		}
		CurrentConfig = config;
		m_queuedOpenConfigs = (openConfig: null, backConfig: null);
		Active = true;
		IsBlockingInput = true;
		if (config is OpenRadialConfig)
		{
			m_animationManager.EndAll();
			LoadSettings();
			ResetAllVariables();
			if (Object.op_Implicit((Object)(object)m_localPlayerRef))
			{
				Player localPlayerRef = m_localPlayerRef;
				localPlayerRef.m_onDeath = (Action)Delegate.Combine(localPlayerRef.m_onDeath, new Action(InstantClose));
			}
		}
		if (backConfig != null)
		{
			m_backConfigs.Push(backConfig);
		}
		if (config != null)
		{
			m_elementInfo.Set(config, !RadialData.SO.EnableToggleAnimation);
			config.InitRadialConfig(this);
		}
		if (m_closeQueued)
		{
			Close(instant: true);
			return;
		}
		IsRefresh = false;
		if (RadialData.SO.EnableToggleAnimation && m_shouldAnimateIn && !(config is OpenRadialConfig))
		{
			StartAnimatingIn();
		}
	}

	public void Refresh()
	{
		if (StartItemIndex <= 0 && Object.op_Implicit((Object)(object)m_selected))
		{
			StartItemIndex = m_elements.IndexOf(m_selected);
		}
		IsRefresh = true;
		Deselect();
		QueuedOpen(CurrentConfig);
	}

	private void LoadSettings()
	{
		this.SetXYControls();
		this.SetItemInteractionControls();
	}

	public void ConstructRadial(List<RadialMenuElement> elements)
	{
		ClearElements();
		List<RadialMenuElement> list = elements ?? new List<RadialMenuElement>();
		if (IsHoverMenu || (m_backConfigs.Count > 0 && list.All((RadialMenuElement e) => !(e is BackElement))))
		{
			InsertBackElement(list, 0, IsThrowMenu);
		}
		int count = list.Count;
		if (!IsThrowMenu && (RadialData.SO.ReSizeOnRefresh || !IsRefresh))
		{
			SetElementsPerLayer(count);
		}
		if (RadialData.SO.UsePersistantBackBtn)
		{
			AddPersistentBackButton(list, IsThrowMenu);
		}
		StartOffset = ((StartOffset != 0) ? ((int)UIMath.Mod(StartOffset, MaxElementsPerLayer)) : 0);
		if (count < MaxElementsPerLayer)
		{
			CenterBackButton(count, IsThrowMenu, list);
		}
		if (!IsRefresh && StartOffset < 0 && StartItemIndex >= 0 && !IsThrowMenu)
		{
			StartItemIndex = (int)UIMath.Mod(StartItemIndex + -StartOffset, MaxElementsPerLayer);
		}
		m_nrOfLayers = ((count > MaxElementsPerLayer) ? Mathf.FloorToInt((float)count / (float)MaxElementsPerLayer) : 0);
		m_radialHasItemElements = list.Any((RadialMenuElement e) => e is ItemElement);
		m_elements = new RadialArray<RadialMenuElement>(list, MaxElementsPerLayer);
		SetRadialLayout();
		SelectStartElement();
		UpdateItemElementVisuals();
	}

	private void SetElementsPerLayer(int count)
	{
		int num = RadialData.SO.MaxElementsRange[0];
		for (int i = 0; i < RadialData.SO.MaxElementsRange.Length - 1; i++)
		{
			if (count > RadialData.SO.MaxElementsRange[i])
			{
				num = RadialData.SO.MaxElementsRange[i + 1];
			}
		}
		if (num != MaxElementsPerLayer)
		{
			if (m_segmentSize == 0f)
			{
				m_segmentSize = Mathf.RoundToInt(360f / (float)((MaxElementsPerLayer > 0) ? MaxElementsPerLayer : num));
			}
			if (StartItemIndex > 0 && !IsHoverMenu)
			{
				StartItemIndex = Mathf.RoundToInt((float)StartItemIndex * m_segmentSize);
			}
			MaxElementsPerLayer = num;
			m_segmentSize = Mathf.RoundToInt(360f / (float)MaxElementsPerLayer);
			m_cursorThreshold = m_segmentSize * RadialData.SO.CursorSensitivity;
			m_layerFadeDistance = m_segmentSize * (float)RadialData.SO.LayerFadeCount;
			m_layerShowDistance = m_segmentSize * (float)RadialData.SO.LayerShowCount;
			m_layerFadeThreshold = (float)((RadialData.SO.LayerShowCount - 1 + RadialData.SO.LayerFadeCount) * 2) * m_segmentSize;
			if (StartItemIndex > 0 && !IsHoverMenu)
			{
				StartItemIndex = Mathf.RoundToInt((float)StartItemIndex / m_segmentSize);
			}
		}
	}

	private void SetRadialLayout()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_elements.Count; i++)
		{
			RadialMenuElement element = m_elements.GetElement(i);
			((Component)element).transform.SetParent((Transform)(object)m_elementContainer);
			((Component)element).transform.localScale = Vector3.one;
			((Component)element).gameObject.SetActive(true);
			Vector2 val = UIMath.AngleToDirection(UIMath.Mod((float)(i + StartOffset) * m_segmentSize, 360f));
			element.LocalPosition = Vector2.op_Implicit(val * RadialData.SO.ElementsDistance);
			element.Alpha = ((m_nrOfLayers > 0) ? 0f : 1f);
			element.SetSegment(MaxElementsPerLayer);
			if (element is BackElement)
			{
				Transform child = ((Transform)element.ElementTransform).GetChild(1);
				Vector3 localPosition = element.LocalPosition;
				child.localRotation = UIMath.DirectionToRotation(Vector2.op_Implicit(((Vector3)(ref localPosition)).normalized));
			}
		}
	}

	private void SelectStartElement()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		if (StartItemIndex > 0 && m_elements.ViableIndex(StartItemIndex) == -1)
		{
			StartItemIndex = m_elements.MaxIndex;
		}
		else if (!IsRefresh && StartItemIndex <= 0 && ((RadialData.SO.DefaultToBackButtonOnNewPage && m_previousInput == Vector2.zero) || IsThrowMenu))
		{
			StartItemIndex = m_elements.BackButtonIndex();
		}
		m_currentLayer = ((StartItemIndex > 0) ? Mathf.FloorToInt((float)StartItemIndex / (float)MaxElementsPerLayer) : 0);
		m_previousClosestPoint = (m_closestPoint = ((StartItemIndex > 0) ? ((int)UIMath.Mod(StartItemIndex, MaxElementsPerLayer)) : ((!(m_previousInput != Vector2.zero)) ? ((int)UIMath.Mod(UIMath.AngleToRadialPoint(m_inputAngle, m_segmentSize) - StartOffset, MaxElementsPerLayer)) : 0)));
		UpdateSelectionIndex();
		m_previousCursorTarget = (m_cursorTarget = ((m_index != -1) ? UIMath.Mod((float)(m_index + StartOffset) * m_segmentSize, 360f) : ((StartItemIndex > 0) ? (UIMath.Mod(StartItemIndex, MaxElementsPerLayer) * m_segmentSize) : ((float)m_closestPoint * m_segmentSize))));
		if (m_nrOfLayers > 0 && m_index != -1)
		{
			SetFadeAnchor((float)m_index * m_segmentSize, instant: true);
		}
		UpdateSelection();
		UpdateCursorPosition(instant: true);
		if (!ZInput.IsGamepadActive() && IsTopLevel)
		{
			ZInput.SetMousePosition(Vector2.op_Implicit(((Transform)m_selected.ElementTransform).position));
		}
		StartItemIndex = -1;
	}

	private void InsertBackElement(List<RadialMenuElement> elems, int index, bool shouldOffset)
	{
		BackElement backElement = Object.Instantiate<BackElement>(RadialData.SO.BackElement);
		backElement.Init(this);
		elems.Insert(shouldOffset ? (index + 1) : index, backElement);
	}

	private void AddPersistentBackButton(List<RadialMenuElement> elems, bool shouldOffset)
	{
		int num = Mathf.Max(Mathf.CeilToInt((float)elems.Count / (float)MaxElementsPerLayer), 1);
		if (num > 1)
		{
			int num2 = elems.Count + num - 1;
			while (Mathf.Max(Mathf.CeilToInt((float)num2 / (float)MaxElementsPerLayer), 1) != num)
			{
				num = Mathf.Max(Mathf.CeilToInt((float)num2 / (float)MaxElementsPerLayer), 1);
				num2 = elems.Count + num;
			}
			for (int i = 1; i < num; i++)
			{
				InsertBackElement(elems, i * MaxElementsPerLayer, shouldOffset);
			}
			if (m_backConfigs.Count <= 0 && num > 1 && StartItemIndex >= MaxElementsPerLayer)
			{
				StartItemIndex += num - 1;
			}
		}
	}

	private void CenterBackButton(int nrOfElements, bool isThrowConfig, List<RadialMenuElement> elems)
	{
		int num = ((nrOfElements % 2 == 0) ? (nrOfElements / 2) : Mathf.FloorToInt((float)nrOfElements / 2f));
		RadialMenuElement item = (isThrowConfig ? elems[1] : elems[0]);
		elems.Remove(item);
		elems.Insert(num, item);
		StartOffset -= num;
	}

	private void Update()
	{
		if (!Active)
		{
			return;
		}
		m_animationManager.Tick(Time.deltaTime);
		if (m_closeQueued)
		{
			Close();
		}
		else
		{
			if (m_isClosing)
			{
				return;
			}
			IsBlockingInput = true;
			RadialArray<RadialMenuElement> elements = m_elements;
			if (elements != null && elements.Count > 0)
			{
				HandleInput();
				HandleVisuals();
			}
			if (m_closeQueued)
			{
				Close();
				return;
			}
			var (radialConfig, radialConfig2) = m_queuedOpenConfigs;
			if (radialConfig != null || radialConfig2 != null)
			{
				Open(m_queuedOpenConfigs.openConfig, m_queuedOpenConfigs.backConfig);
			}
		}
	}

	private void HandleInput()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		if (TryClosure() || (CanThrow && TryThrow()) || TryInteract())
		{
			return;
		}
		m_previousClosestPoint = m_closestPoint;
		Vector2 val = ((!ZInput.IsGamepadActive()) ? (GetMouseDirection?.Invoke() ?? Vector2.zero) : (GetControllerDirection?.Invoke() ?? Vector2.zero));
		bool flag = val != Vector2.zero;
		m_inputDirection = (flag ? m_inputDirection : 0);
		if (flag)
		{
			if (((Vector2)(ref val)).normalized != ((Vector2)(ref m_previousInput)).normalized)
			{
				OnUpdateCursorInput(val);
			}
			if (UseHoverSelect && CanDoHoverSelect(m_selected) && !m_animationManager.IsTweenActiveWithEndAction(m_selected.ID + "_hov"))
			{
				m_selected.StartHoverSelect(m_animationManager, RadialData.SO.HoverSelectSpeed, RadialData.SO.HoverSelectEasingType, OnInteract);
			}
		}
		else if (UseHoverSelect && CanDoHoverSelect(m_selected) && m_animationManager.IsTweenActiveWithEndAction(m_selected.ID + "_hov"))
		{
			m_selected.ResetHoverSelect(m_animationManager, RadialData.SO.HoverSelectSpeed, RadialData.SO.HoverSelectEasingType);
		}
		if (m_reFade && (!Object.op_Implicit((Object)(object)m_selected) || !m_animationManager.IsTweenActive(m_selected.ID)))
		{
			m_reFade = false;
		}
		if (CanDoubleClick)
		{
			Func<bool> getDoubleTap = GetDoubleTap;
			if (getDoubleTap != null && getDoubleTap())
			{
				goto IL_01e4;
			}
		}
		if (CanFlick)
		{
			Func<bool> getFlick = GetFlick;
			if (getFlick != null && getFlick())
			{
				goto IL_01e4;
			}
		}
		m_previousInput = (flag ? val : Vector2.zero);
		return;
		IL_01e4:
		OnInteract();
	}

	private void HandleVisuals()
	{
		if (m_radialHasItemElements)
		{
			UpdateItemElementVisuals();
		}
	}

	private void OnUpdateCursorInput(Vector2 newInput)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		((Component)m_cursor).gameObject.SetActive(true);
		m_inputAngle = UIMath.DirectionToAngleDegrees(((Vector2)(ref newInput)).normalized);
		float num = Mathf.Floor(UIMath.RadialDelta(m_cursorTarget, m_inputAngle));
		if (!ZInput.IsGamepadActive() || !(num < ((m_index == -1) ? 5f : ((m_previousInput == Vector2.zero && MaxElementsPerLayer > 6) ? (m_segmentSize * 2f) : m_cursorThreshold))))
		{
			m_inputDirection = UIMath.RadialDirection(((Vector2)(ref m_previousInput)).normalized, ((Vector2)(ref newInput)).normalized);
			m_closestPoint = (int)UIMath.Mod(UIMath.AngleToRadialPoint(m_inputAngle, m_segmentSize) - StartOffset, MaxElementsPerLayer);
			if (m_closestPoint != m_previousClosestPoint)
			{
				UpdateSelectionIndex();
				UpdateSelection();
			}
			m_previousCursorTarget = m_cursorTarget;
			m_cursorTarget = ((m_index == -1) ? m_inputAngle : UIMath.ClosestSegment(m_inputAngle, m_segmentSize));
			if (!Mathf.Approximately(m_previousCursorTarget, m_cursorTarget))
			{
				UpdateCursorPosition();
			}
		}
	}

	private void OnInteract()
	{
		IsBlockingInput = !IsHoverMenu;
		if (IsBlockingInput)
		{
			OnInteractionDelay?.Invoke(RadialData.SO.InteractionDelay);
		}
		if (IsHoverMenu && m_selected is ItemElement)
		{
			ItemElement obj = m_selected as ItemElement;
			if (obj != null && obj.HoverMenuInteract?.Invoke(HoverObject) == true)
			{
				OnSuccessfulInteract();
				return;
			}
		}
		if (Object.op_Implicit((Object)(object)m_selected))
		{
			Func<bool> interact = m_selected.Interact;
			if (interact != null && interact())
			{
				OnSuccessfulInteract();
				return;
			}
		}
		QueuedClose();
		void OnSuccessfulInteract()
		{
			bool num = ((m_selected.AdvancedCloseOnInteract == null) ? (m_selected.CloseOnInteract?.Invoke() ?? false) : (m_selected.AdvancedCloseOnInteract?.Invoke(this, m_elements) ?? false));
			if (m_selected is ThrowElement throwElement && !m_localPlayerRef.GetInventory().ContainsItem(throwElement.m_data))
			{
				Back();
			}
			RadialMenuElement selected = m_selected;
			RadialMenuElement lastUsed;
			if (!(selected is GroupElement))
			{
				if (selected is ThrowElement)
				{
					lastUsed = ((!(LastUsed is ItemElement itemElement)) ? LastUsed : ((!m_localPlayerRef.GetInventory().ContainsItem(itemElement.m_data)) ? null : LastUsed));
				}
				else if (!(selected is ItemElement itemElement2))
				{
					lastUsed = ((selected != null) ? m_selected : null);
				}
				else if (((object)m_selected).Equals((object?)LastUsed))
				{
					lastUsed = ((!m_localPlayerRef.GetInventory().ContainsItem(itemElement2.m_data)) ? null : LastUsed);
				}
				else
				{
					ItemElement itemElement3 = itemElement2;
					lastUsed = ((itemElement3.m_data.m_shared.m_name.Contains("hammer") || !m_localPlayerRef.GetInventory().ContainsItem(itemElement3.m_data)) ? LastUsed : m_selected);
				}
			}
			else
			{
				lastUsed = LastUsed;
			}
			LastUsed = lastUsed;
			if (num)
			{
				QueuedClose();
			}
			else
			{
				var (radialConfig, radialConfig2) = m_queuedOpenConfigs;
				if (radialConfig == null && radialConfig2 == null)
				{
					Refresh();
				}
			}
		}
	}

	private bool TryInteract()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_selected))
		{
			return false;
		}
		Func<bool> getReleaseToUse = GetReleaseToUse;
		if (getReleaseToUse != null && getReleaseToUse())
		{
			if (m_previousInput != Vector2.zero)
			{
				OnInteract();
			}
			QueuedClose();
			return true;
		}
		Func<bool> getConfirm = GetConfirm;
		if (getConfirm == null || !getConfirm())
		{
			return false;
		}
		OnInteract();
		return true;
	}

	private bool TryClosure()
	{
		if (!Object.op_Implicit((Object)(object)m_localPlayerRef) || m_localPlayerRef.IsTeleporting() || m_localPlayerRef.IsSleeping())
		{
			Close();
			return true;
		}
		if (IsHoverMenu && Object.op_Implicit((Object)(object)m_localPlayerRef) && (Object)(object)m_localPlayerRef.GetHoverObject() != (Object)(object)HoverObject)
		{
			QueuedClose();
			return true;
		}
		Func<bool> getBack = GetBack;
		if (getBack != null && getBack())
		{
			Back();
			return true;
		}
		Func<bool> getClose = GetClose;
		if (getClose != null && getClose())
		{
			QueuedClose();
			return true;
		}
		return false;
	}

	private bool TryThrow()
	{
		if (!Object.op_Implicit((Object)(object)m_selected))
		{
			return false;
		}
		Func<bool> getOpenThrowMenu = GetOpenThrowMenu;
		if (getOpenThrowMenu != null && getOpenThrowMenu() && m_selected.TryOpenSubRadial != null)
		{
			return m_selected.TryOpenSubRadial(this, m_elements.IndexOf(m_selected));
		}
		Func<bool> getThrow = GetThrow;
		if (getThrow == null || !getThrow())
		{
			return false;
		}
		OnInteractionDelay?.Invoke(RadialData.SO.InteractionDelay);
		Func<bool> secondaryInteract = m_selected.SecondaryInteract;
		if (secondaryInteract != null && secondaryInteract())
		{
			if (LastUsed is ItemElement itemElement && !m_localPlayerRef.GetInventory().ContainsItem(itemElement.m_data))
			{
				LastUsed = null;
			}
			Refresh();
		}
		return true;
	}

	private void UpdateLayer()
	{
		if (RadialData.SO.ReFadeAtMidnight || !m_cursorLocked)
		{
			if (m_inputDirection > 0 && m_closestPoint < m_previousClosestPoint)
			{
				OnNextLayer();
			}
			else if (m_inputDirection < 0 && m_closestPoint > m_previousClosestPoint)
			{
				OnPreviousLayer();
			}
			else if (m_inputDirection == 0 && m_currentLayer == m_nrOfLayers && m_elements.ViableIndex((int)UIMath.Mod(m_closestPoint, MaxElementsPerLayer) + MaxElementsPerLayer * m_currentLayer) == -1)
			{
				OnPreviousLayer();
			}
		}
	}

	private void OnNextLayer()
	{
		if (m_currentLayer == m_nrOfLayers)
		{
			if (RadialData.SO.ReFadeAtMidnight)
			{
				m_reFade = true;
			}
			else
			{
				m_cursorLocked = true;
			}
		}
		m_currentLayer = Mathf.Min(m_currentLayer + 1, m_nrOfLayers);
	}

	private void OnPreviousLayer()
	{
		if (m_currentLayer == 0)
		{
			if (RadialData.SO.ReFadeAtMidnight)
			{
				m_reFade = true;
			}
			else
			{
				m_cursorLocked = true;
			}
		}
		m_currentLayer = Mathf.Max(m_currentLayer - 1, 0);
	}

	private void UpdateSelection()
	{
		bool flag = m_index == -1 || m_index > m_elements.MaxIndex;
		if (!flag && m_cursorLocked && !m_reFade && m_elements.GetElement(m_index).Alpha == 0f)
		{
			flag = true;
		}
		if ((flag || !((Object)(object)m_selected == (Object)(object)m_elements.GetElement(m_index))) && (!flag || Object.op_Implicit((Object)(object)m_selected)))
		{
			Selected = (flag ? null : m_elements.GetElement(m_index));
		}
	}

	private void Deselect()
	{
		if (Object.op_Implicit((Object)(object)m_selected))
		{
			Selected = null;
		}
	}

	private void OnSelectedUpdate(RadialMenuElement oldSelected, RadialMenuElement newSelected)
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		CanThrow = false;
		if (Object.op_Implicit((Object)(object)oldSelected))
		{
			oldSelected.Selected = false;
			((Transform)m_highlighter).SetParent(((Component)this).transform);
			if (oldSelected is GroupElement groupElement)
			{
				groupElement.ChangeToDeselectColor();
			}
			if (ShouldNudgeSelected)
			{
				oldSelected.ResetNudge(m_animationManager, RadialData.SO.ElementsDistance, RadialData.SO.NudgeDuration * 0.75f, RadialData.SO.NudgeEasingType);
			}
			if (UseHoverSelect && CanDoHoverSelect(oldSelected) && m_animationManager.IsTweenActiveWithEndAction(oldSelected.ID + "_hov"))
			{
				oldSelected.ResetHoverSelect(m_animationManager, RadialData.SO.HoverSelectSpeed, RadialData.SO.HoverSelectEasingType);
			}
		}
		if (Object.op_Implicit((Object)(object)newSelected))
		{
			newSelected.Selected = true;
			m_elementInfo.Set(newSelected, m_animationManager);
			((Transform)m_highlighter).SetParent(((Component)newSelected).transform);
			Vector3 localPosition = newSelected.LocalPosition;
			Vector3 normalized = ((Vector3)(ref localPosition)).normalized;
			float num = UIMath.DirectionToAngleDegrees(Vector2.op_Implicit(normalized));
			((Transform)m_highlighter).localRotation = Quaternion.Euler(0f, 0f, 0f - num);
			((Transform)m_highlighter).localPosition = normalized * RadialData.SO.OrnamentOffset;
			if (newSelected is GroupElement groupElement2)
			{
				groupElement2.ChangeToSelectColor();
			}
			if (UseHoverSelect && CanDoHoverSelect(newSelected))
			{
				newSelected.StartHoverSelect(m_animationManager, RadialData.SO.HoverSelectSpeed, RadialData.SO.HoverSelectEasingType, OnInteract);
			}
			if (ShouldNudgeSelected && !m_shouldAnimateIn && !m_isAnimating)
			{
				newSelected.StartNudge(m_animationManager, RadialData.SO.ElementsDistance + RadialData.SO.NudgeDistance, RadialData.SO.NudgeDuration, RadialData.SO.NudgeEasingType);
			}
			if (m_nrOfLayers > 0)
			{
				if (m_cursorLocked && !RadialData.SO.ReFadeAtMidnight)
				{
					m_reFade = true;
				}
				m_cursorLocked = false;
				SetFadeAnchor((float)m_index * m_segmentSize);
			}
		}
		if (Object.op_Implicit((Object)(object)newSelected))
		{
			m_elementInfo.Set(newSelected, m_animationManager);
		}
		else
		{
			m_elementInfo.Set(CurrentConfig, m_isAnimating);
		}
		((Component)m_highlighter).gameObject.SetActive(Object.op_Implicit((Object)(object)newSelected));
		((Component)m_cursor).gameObject.SetActive(m_previousInput != Vector2.zero || Object.op_Implicit((Object)(object)newSelected));
	}

	private void UpdateSelectionIndex()
	{
		if (m_nrOfLayers > 0)
		{
			int num = (int)UIMath.Mod(m_closestPoint, MaxElementsPerLayer) + MaxElementsPerLayer * m_currentLayer;
			bool cursorLocked = m_cursorLocked;
			UpdateLayer();
			if (!m_cursorLocked && m_currentLayer == m_nrOfLayers && !Object.op_Implicit((Object)(object)m_elements.GetElement(num)))
			{
				m_cursorLocked = UIMath.AngleToPos((float)m_closestPoint * m_segmentSize, m_currentLayer) > (float)m_elements.MaxIndex * m_segmentSize;
				if (m_cursorLocked && Mathf.Approximately((float)m_elements.MaxIndex * m_segmentSize, 360f * (float)m_nrOfLayers - m_segmentSize))
				{
					m_currentLayer--;
				}
			}
			if (!cursorLocked && m_cursorLocked)
			{
				SetFadeAnchor((m_currentLayer == 0) ? 0f : ((float)m_elements.MaxIndex * m_segmentSize));
			}
			if (cursorLocked)
			{
				RadialMenuElement element = m_elements.GetElement(num);
				if (!Object.op_Implicit((Object)(object)element) || element.Alpha == 0f)
				{
					element = m_elements.GetElement(num + MaxElementsPerLayer) ?? m_elements.GetElement(num - MaxElementsPerLayer);
					if (Object.op_Implicit((Object)(object)element) && m_elements.GetVisisbleElementsAt(m_fadeAnchor, RadialData.SO.LayerFadeCount, RadialData.SO.LayerShowCount).Contains(element))
					{
						if (m_elements.IndexOf(element) > num)
						{
							OnNextLayer();
						}
						else
						{
							OnPreviousLayer();
						}
					}
				}
				if (m_cursorLocked)
				{
					UpdateElementsScale();
				}
			}
		}
		m_index = m_elements.ViableIndex((int)UIMath.Mod(m_closestPoint, MaxElementsPerLayer) + MaxElementsPerLayer * m_currentLayer);
	}

	private bool CanDoHoverSelect(RadialMenuElement e)
	{
		if ((Object)(object)e != (Object)null)
		{
			return !(e is EmptyElement);
		}
		return false;
	}

	private void UpdateItemElementVisuals()
	{
		if (!Object.op_Implicit((Object)(object)m_localPlayerRef) || !m_radialHasItemElements)
		{
			return;
		}
		m_localPlayerRef.GetActionProgress(out var _, out var progress, out var data);
		int actionQueueCount = m_localPlayerRef.GetActionQueueCount();
		int num;
		if (data != null)
		{
			Player.MinorActionData.ActionType type = data.m_type;
			num = ((type == Player.MinorActionData.ActionType.Equip || type == Player.MinorActionData.ActionType.Unequip) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		RadialMenuElement[] getArray = m_elements.GetArray;
		foreach (RadialMenuElement radialMenuElement in getArray)
		{
			if (radialMenuElement is ItemElement itemElement)
			{
				itemElement.UpdateDurabilityAndAmount();
				if (flag && data.m_item == itemElement.m_data)
				{
					itemElement.UpdateQueueAndActivation(progress, data, actionQueueCount);
				}
				else
				{
					itemElement.UpdateQueueAndActivation(m_localPlayerRef.IsEquipActionQueued(itemElement.m_data));
				}
				if ((Object)(object)radialMenuElement == (Object)(object)m_selected)
				{
					m_elementInfo.UpdateDurabilityAndWeightInfo(radialMenuElement);
				}
			}
		}
	}

	public void OnAddItem(ItemDrop.ItemData newItemData)
	{
		if (CurrentConfig is ItemGroupConfig itemGroupConfig && itemGroupConfig.ShouldAddItem(newItemData))
		{
			itemGroupConfig.AddItem(this, newItemData, m_elements);
		}
	}

	private void UpdateElementsScale()
	{
		if (m_elements == null || m_isAnimating || m_shouldAnimateIn)
		{
			return;
		}
		RadialMenuElement[] getArray = m_elements.GetArray;
		foreach (RadialMenuElement e in getArray)
		{
			float scale = GetScale(e);
			if (!Mathf.Approximately(e.Scale, scale))
			{
				m_animationManager.StartUniqueTween(() => e.Scale, delegate(float val)
				{
					e.Scale = val;
				}, e.ID + "_scale", scale, m_reFade ? (RadialData.SO.ElementFadeDuration * RadialData.SO.ReFadeMultiplier) : RadialData.SO.ElementFadeDuration, RadialData.SO.ElementScaleEasingType);
			}
		}
	}

	private void UpdateElementsAlpha(bool instant = false)
	{
		if (m_elements == null || (!instant && (m_isAnimating || m_shouldAnimateIn)))
		{
			return;
		}
		List<RadialMenuElement> visisbleElementsAt = m_elements.GetVisisbleElementsAt(m_fadeAnchor, RadialData.SO.LayerFadeCount, RadialData.SO.LayerShowCount);
		RadialMenuElement[] getArray = m_elements.GetArray;
		foreach (RadialMenuElement e in getArray)
		{
			GetFadeAlphaAndScale(e, visisbleElementsAt, out var alpha, out var scale, out var shouldBeInstant);
			if (instant || shouldBeInstant)
			{
				m_animationManager.CancelTweens(e.ID);
				m_animationManager.CancelTweens(e.ID + "_scale");
				e.Alpha = alpha;
			}
			else
			{
				m_animationManager.StartUniqueTween(() => e.Alpha, delegate(float val)
				{
					e.Alpha = val;
				}, e.ID, alpha, m_reFade ? (RadialData.SO.ElementFadeDuration * RadialData.SO.ReFadeMultiplier) : RadialData.SO.ElementFadeDuration, RadialData.SO.ElementFadeEasingType);
			}
			if (Mathf.Approximately(e.Scale, scale))
			{
				continue;
			}
			if (instant)
			{
				e.Scale = scale;
			}
			else if ((Object)(object)e == (Object)(object)m_selected)
			{
				m_animationManager.StartUniqueTween(() => e.Scale, delegate(float val)
				{
					e.Scale = val;
				}, e.ID + "_scale", scale, m_reFade ? (RadialData.SO.ElementFadeDuration * RadialData.SO.ReFadeMultiplier) : RadialData.SO.ElementFadeDuration, RadialData.SO.ElementScaleEasingType, null, delegate
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					((Transform)m_highlighter).localScale = Vector3.one;
				});
			}
			else
			{
				m_animationManager.StartUniqueTween(() => e.Scale, delegate(float val)
				{
					e.Scale = val;
				}, e.ID + "_scale", scale, m_reFade ? (RadialData.SO.ElementFadeDuration * RadialData.SO.ReFadeMultiplier) : RadialData.SO.ElementFadeDuration, RadialData.SO.ElementScaleEasingType);
			}
		}
	}

	private void GetFadeAlphaAndScale(RadialMenuElement e, List<RadialMenuElement> visibleElements, out float alpha, out float scale, out bool shouldBeInstant)
	{
		alpha = 1f;
		scale = 1f;
		shouldBeInstant = (Object)(object)e == (Object)(object)m_selected;
		float num = (float)m_elements.IndexOf(e) * m_segmentSize;
		scale = GetScale(e, num);
		if (!shouldBeInstant)
		{
			float num2 = Mathf.Abs(num - m_fadeAnchor);
			if (!visibleElements.Contains(e))
			{
				shouldBeInstant = num2 >= m_layerFadeThreshold;
				alpha = 0f;
			}
			else if (!(num2 <= m_layerShowDistance))
			{
				float num3 = ((num > m_fadeAnchor) ? (m_fadeAnchor + m_layerShowDistance) : (m_fadeAnchor - m_layerShowDistance));
				alpha = 1f - Mathf.Clamp01(Mathf.Abs(num - num3) / (m_layerFadeDistance + m_segmentSize));
			}
		}
	}

	private float GetScale(RadialMenuElement e)
	{
		float ePos = (float)m_elements.IndexOf(e) * m_segmentSize;
		return GetScale(e, ePos);
	}

	private float GetScale(RadialMenuElement e, float ePos)
	{
		if (RadialData.SO.SpiralEffectInsensity == SpiralEffectIntensitySetting.Off || !RadialData.SO.EnableToggleAnimation)
		{
			return 1f;
		}
		if ((!Object.op_Implicit((Object)(object)m_selected) && !IsRefresh) || m_index == -1)
		{
			return e.Scale;
		}
		float num = (float)m_index * m_segmentSize;
		float num2 = Mathf.Abs(ePos - num);
		float num3 = ((ePos > num) ? RadialData.SO.ElementScaleFactor : (1f / RadialData.SO.ElementScaleFactor));
		float num4 = Mathf.Lerp(0f, 1f, Mathf.Clamp01(num2 / m_layerFadeThreshold));
		float num5 = ((ePos > num) ? RadialData.SO.ElementNudgeFactor : (1f / RadialData.SO.ElementNudgeFactor));
		float num6 = ((num5 >= 1f) ? (1f + (num5 - 1f) * num4) : (1f - (1f - num5) * num4));
		e.StartNudge(m_animationManager, RadialData.SO.ElementsDistance * num6, m_reFade ? (RadialData.SO.ElementFadeDuration * RadialData.SO.ReFadeMultiplier) : RadialData.SO.ElementFadeDuration, RadialData.SO.ElementScaleEasingType);
		if (!(num3 >= 1f))
		{
			return 1f - (1f - num3) * num4;
		}
		return 1f + (num3 - 1f) * num4;
	}

	private void SetFadeAnchor(float value, bool instant = false)
	{
		if (!Mathf.Approximately(m_fadeAnchor, value) || instant)
		{
			m_fadeAnchor = value;
			UpdateElementsAlpha(instant);
		}
	}

	private void UpdateCursorPosition(bool instant = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (instant)
		{
			((Transform)m_cursor).localRotation = UIMath.AngleToRotation(m_cursorTarget);
			return;
		}
		m_animationManager.StartUniqueAngleTween(delegate(float val)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			((Transform)m_cursor).localRotation = UIMath.AngleToRotation(val);
		}, m_cursorID, m_previousCursorTarget, m_cursorTarget, m_reFade ? (RadialData.SO.CursorSpeed * RadialData.SO.ReFadeMultiplier) : RadialData.SO.CursorSpeed, RadialData.SO.CursorEasingType);
	}

	private void StartAnimatingIn()
	{
		m_animationManager.CancelAll();
		m_isAnimating = true;
		m_elementInfo.OpenAnimation(m_animationManager, "_tog", RadialData.SO.ToggleAnimDuration, RadialData.SO.ElementInfoRadius, RadialData.SO.ToggleAnimDistance, RadialData.SO.ToggleAlphaEasingType, RadialData.SO.TogglePosEasingType);
		RadialMenuElement[] getArray = m_elements.GetArray;
		for (int i = 0; i < getArray.Length; i++)
		{
			getArray[i].OpenAnimation(m_animationManager, "_tog", RadialData.SO.ToggleAnimDuration, RadialData.SO.ElementsDistance, RadialData.SO.ToggleAnimDistance, RadialData.SO.ToggleAlphaEasingType, RadialData.SO.TogglePosEasingType);
		}
		m_animationManager.AddEnd("_tog", OnEnd);
		m_shouldAnimateIn = false;
		void OnEnd()
		{
			m_isAnimating = false;
			if (Object.op_Implicit((Object)(object)m_selected))
			{
				if (ShouldNudgeSelected)
				{
					m_selected.StartNudge(m_animationManager, RadialData.SO.ElementsDistance + RadialData.SO.NudgeDistance, RadialData.SO.NudgeDuration, RadialData.SO.NudgeEasingType);
				}
				else
				{
					UpdateElementsScale();
				}
			}
		}
	}

	private void StartAnimatingOut()
	{
		m_animationManager.CancelAll();
		m_elementInfo.CloseAnimation(m_animationManager, "_tog", RadialData.SO.ToggleAnimDuration, RadialData.SO.ElementInfoRadius, RadialData.SO.ToggleAnimDistance, RadialData.SO.ToggleAlphaEasingType, RadialData.SO.TogglePosEasingType);
		RadialMenuElement[] getArray = m_elements.GetArray;
		foreach (RadialMenuElement radialMenuElement in getArray)
		{
			if (UseHoverSelect && m_animationManager.IsTweenActiveWithEndAction(radialMenuElement.ID + "_hov"))
			{
				m_animationManager.CancelTweens(radialMenuElement.ID + "_hov");
			}
			radialMenuElement.CloseAnimation(m_animationManager, "_tog", RadialData.SO.ToggleAnimDuration, RadialData.SO.ElementsDistance, RadialData.SO.ToggleAnimDistance, RadialData.SO.ToggleAlphaEasingType, RadialData.SO.TogglePosEasingType);
		}
		m_animationManager.AddEnd("_tog", OnEnd);
		void OnEnd()
		{
			m_animationManager.EndAll();
			CanOpen = false;
			Active = false;
		}
	}

	private void ClearElements()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (((Transform)m_elementContainer).childCount <= 0)
		{
			return;
		}
		if ((Object)(object)LastUsed != (Object)null)
		{
			((Component)LastUsed).transform.SetParent(((Component)this).transform);
			LastUsed.Alpha = 0f;
		}
		((Transform)m_highlighter).SetParent(((Component)this).transform);
		foreach (Transform item in (Transform)m_elementContainer)
		{
			((Component)item).gameObject.SetActive(false);
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	private void ResetExclusiveVariables()
	{
		m_cursorLocked = false;
		m_closeQueued = false;
		m_reFade = false;
		m_fadeAnchor = 0f;
		StartOffset = 0;
	}

	private void ResetAllVariables()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		ResetExclusiveVariables();
		((Component)m_highlighter).gameObject.SetActive(false);
		((Component)m_cursor).gameObject.SetActive(false);
		m_backConfigs.Clear();
		HoverObject = null;
		CurrentConfig = null;
		m_selected = null;
		IsBlockingInput = false;
		IsRefresh = false;
		m_previousInput = Vector2.zero;
		StartItemIndex = -1;
		BackIndex = -1;
		m_index = -1;
		m_cursorTarget = 0f;
		m_inputAngle = 0f;
		UpdateCursorPosition(instant: true);
	}

	public void Back()
	{
		StartItemIndex = BackIndex;
		BackIndex = -1;
		if (m_backConfigs.Count < 1)
		{
			QueuedClose();
		}
		else
		{
			QueuedOpen(m_backConfigs.Pop());
		}
	}

	public void QueuedClose()
	{
		m_isClosing = true;
		m_closeQueued = true;
	}

	private void InstantClose()
	{
		Close(instant: true);
	}

	private void Close(bool instant = false)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_localPlayerRef))
		{
			Player localPlayerRef = m_localPlayerRef;
			localPlayerRef.m_onDeath = (Action)Delegate.Remove(localPlayerRef.m_onDeath, new Action(InstantClose));
		}
		if (Object.op_Implicit((Object)(object)m_selected))
		{
			m_selected.Selected = false;
		}
		Func<Vector2> getControllerDirection = GetControllerDirection;
		Vector2 val;
		if (getControllerDirection == null)
		{
			val = Vector2.zero;
		}
		else
		{
			Vector2 val2 = getControllerDirection();
			val = ((Vector2)(ref val2)).normalized;
		}
		Vector2 cameraDirectionLock = val;
		if (ZInput.IsGamepadActive() && !((Vector2)(ref cameraDirectionLock)).Equals(Vector2.zero))
		{
			PlayerController.cameraDirectionLock = cameraDirectionLock;
		}
		m_closeQueued = false;
		ResetAllVariables();
		if (RadialData.SO.EnableToggleAnimation && !instant)
		{
			m_isClosing = true;
			StartAnimatingOut();
		}
		else
		{
			m_animationManager.EndAll();
			CanOpen = false;
			Active = false;
		}
	}

	private void OnDisable()
	{
		m_animationManager.EndAll();
		if (Object.op_Implicit((Object)(object)m_localPlayerRef))
		{
			Player localPlayerRef = m_localPlayerRef;
			localPlayerRef.m_onDeath = (Action)Delegate.Remove(localPlayerRef.m_onDeath, new Action(InstantClose));
		}
	}
}
