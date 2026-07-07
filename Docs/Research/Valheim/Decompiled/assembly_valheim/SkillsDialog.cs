using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillsDialog : MonoBehaviour
{
	public RectTransform m_listRoot;

	[SerializeField]
	private ScrollRect skillListScrollRect;

	[SerializeField]
	private Scrollbar scrollbar;

	public RectTransform m_tooltipAnchor;

	public GameObject m_elementPrefab;

	public TMP_Text m_totalSkillText;

	public float m_spacing = 80f;

	public float m_inputDelay = 0.1f;

	private int m_selectionIndex;

	private float m_inputDelayTimer;

	private float m_baseListSize;

	private readonly List<GameObject> m_elements = new List<GameObject>();

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = m_listRoot.rect;
		m_baseListSize = ((Rect)(ref rect)).height;
	}

	private IEnumerator SelectFirstEntry()
	{
		yield return null;
		yield return null;
		if (m_elements.Count > 0)
		{
			m_selectionIndex = 0;
			EventSystem.current.SetSelectedGameObject(m_elements[m_selectionIndex]);
			_003F val = this;
			SkillsDialog skillsDialog = this;
			Transform transform = m_elements[m_selectionIndex].transform;
			((MonoBehaviour)val).StartCoroutine(skillsDialog.FocusOnCurrentLevel((RectTransform)(object)((transform is RectTransform) ? transform : null)));
			skillListScrollRect.verticalNormalizedPosition = 1f;
		}
		yield return null;
	}

	private IEnumerator FocusOnCurrentLevel(RectTransform element)
	{
		yield return null;
		yield return null;
		Canvas.ForceUpdateCanvases();
		SnapTo(element);
	}

	private void SnapTo(RectTransform target)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Canvas.ForceUpdateCanvases();
		m_listRoot.anchoredPosition = Vector2.op_Implicit(((Component)skillListScrollRect).transform.InverseTransformPoint(((Transform)m_listRoot).position)) - Vector2.op_Implicit(((Component)skillListScrollRect).transform.InverseTransformPoint(((Transform)target).position)) - new Vector2(target.sizeDelta.x / 2f, 0f);
	}

	private void Update()
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		if (m_inputDelayTimer > 0f)
		{
			m_inputDelayTimer -= Time.unscaledDeltaTime;
			return;
		}
		if (ZInput.IsGamepadActive() && m_elements.Count > 0)
		{
			float joyRightStickY = ZInput.GetJoyRightStickY(true);
			float joyLeftStickY = ZInput.GetJoyLeftStickY(true);
			bool buttonDown = ZInput.GetButtonDown("JoyDPadUp");
			bool flag = joyLeftStickY < -0.1f || joyRightStickY < -0.1f;
			bool buttonDown2 = ZInput.GetButtonDown("JoyDPadDown");
			bool flag2 = joyLeftStickY > 0.1f || joyRightStickY > 0.1f;
			if ((flag || buttonDown) && m_selectionIndex > 0)
			{
				m_selectionIndex--;
			}
			if ((buttonDown2 || flag2) && m_selectionIndex < m_elements.Count - 1)
			{
				m_selectionIndex++;
			}
			GameObject val = m_elements[m_selectionIndex];
			EventSystem.current.SetSelectedGameObject(val);
			_003F val2 = this;
			Transform transform = val.transform;
			((MonoBehaviour)val2).StartCoroutine(FocusOnCurrentLevel((RectTransform)(object)((transform is RectTransform) ? transform : null)));
			val.GetComponentInChildren<UITooltip>().OnHoverStart(val);
			if (flag || flag2)
			{
				m_inputDelayTimer = m_inputDelay;
			}
		}
		if (m_elements.Count > 0)
		{
			Transform transform2 = ((Component)skillListScrollRect).transform;
			RectTransform val3 = (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
			RectTransform listRoot = m_listRoot;
			Scrollbar obj = scrollbar;
			Rect rect = val3.rect;
			float height = ((Rect)(ref rect)).height;
			rect = listRoot.rect;
			obj.size = height / ((Rect)(ref rect)).height;
		}
	}

	public void Setup(Player player)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		List<Skills.Skill> skillList = player.GetSkills().GetSkillList();
		int num = skillList.Count - m_elements.Count;
		for (int i = 0; i < num; i++)
		{
			GameObject item = Object.Instantiate<GameObject>(m_elementPrefab, Vector3.zero, Quaternion.identity, (Transform)(object)m_listRoot);
			m_elements.Add(item);
		}
		for (int j = 0; j < skillList.Count; j++)
		{
			Skills.Skill skill = skillList[j];
			GameObject obj = m_elements[j];
			obj.SetActive(true);
			Transform transform = obj.transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			val.anchoredPosition = new Vector2(0f, (float)(-j) * m_spacing);
			obj.GetComponentInChildren<UITooltip>().Set("", skill.m_info.m_description, m_tooltipAnchor, new Vector2(0f, Math.Min(255f, ((Transform)val).localPosition.y + 10f)));
			((Component)Utils.FindChild(obj.transform, "icon", (IterativeSearchType)0)).GetComponent<Image>().sprite = skill.m_info.m_icon;
			((Component)Utils.FindChild(obj.transform, "name", (IterativeSearchType)0)).GetComponent<TMP_Text>().text = Localization.instance.Localize("$skill_" + skill.m_info.m_skill.ToString().ToLower());
			float skillLevel = player.GetSkills().GetSkillLevel(skill.m_info.m_skill);
			((Component)Utils.FindChild(obj.transform, "leveltext", (IterativeSearchType)0)).GetComponent<TMP_Text>().text = ((int)skill.m_level).ToString();
			TMP_Text component = ((Component)Utils.FindChild(obj.transform, "bonustext", (IterativeSearchType)0)).GetComponent<TMP_Text>();
			bool flag = skillLevel != Mathf.Floor(skill.m_level);
			((Component)component).gameObject.SetActive(flag);
			if (flag)
			{
				component.text = (skillLevel - skill.m_level).ToString("+0");
			}
			((Component)Utils.FindChild(obj.transform, "levelbar_total", (IterativeSearchType)0)).GetComponent<GuiBar>().SetValue(skillLevel / 100f);
			((Component)Utils.FindChild(obj.transform, "levelbar", (IterativeSearchType)0)).GetComponent<GuiBar>().SetValue(skill.m_level / 100f);
			((Component)Utils.FindChild(obj.transform, "currentlevel", (IterativeSearchType)0)).GetComponent<GuiBar>().SetValue(skill.GetLevelPercentage());
		}
		for (int k = skillList.Count; k < m_elements.Count; k++)
		{
			m_elements[k].SetActive(false);
		}
		float num2 = Mathf.Max(m_baseListSize, (float)skillList.Count * m_spacing);
		m_listRoot.SetSizeWithCurrentAnchors((Axis)1, num2);
		m_totalSkillText.text = "<color=orange>" + player.GetSkills().GetTotalSkill().ToString("0") + "</color><color=white> / </color><color=orange>" + player.GetSkills().GetTotalSkillCap().ToString("0") + "</color>";
		((MonoBehaviour)this).StartCoroutine(SelectFirstEntry());
	}

	public void OnClose()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void SkillClicked(GameObject selectedObject)
	{
		m_selectionIndex = m_elements.IndexOf(selectedObject);
	}
}
