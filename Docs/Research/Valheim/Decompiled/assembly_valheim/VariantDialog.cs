using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VariantDialog : MonoBehaviour
{
	public Transform m_listRoot;

	public GameObject m_elementPrefab;

	public float m_spacing = 70f;

	public int m_gridWidth = 5;

	private List<GameObject> m_elements = new List<GameObject>();

	public Action<int> m_selected;

	public void Setup(ItemDrop.ItemData item)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		((Component)this).gameObject.SetActive(true);
		foreach (GameObject element in m_elements)
		{
			Object.Destroy((Object)(object)element);
		}
		m_elements.Clear();
		for (int i = 0; i < item.m_shared.m_variants; i++)
		{
			Sprite sprite = item.m_shared.m_icons[i];
			int num = i / m_gridWidth;
			int num2 = i % m_gridWidth;
			GameObject val = Object.Instantiate<GameObject>(m_elementPrefab, Vector3.zero, Quaternion.identity, m_listRoot);
			val.SetActive(true);
			Transform transform = val.transform;
			((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2((float)num2 * m_spacing, (float)(-num) * m_spacing);
			Button component = ((Component)val.transform.Find("Button")).GetComponent<Button>();
			int buttonIndex = i;
			((UnityEvent)component.onClick).AddListener((UnityAction)delegate
			{
				OnClicked(buttonIndex);
			});
			((Component)component).GetComponent<Image>().sprite = sprite;
			m_elements.Add(val);
		}
	}

	public void OnClose()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnClicked(int index)
	{
		ZLog.Log((object)("Clicked button " + index));
		((Component)this).gameObject.SetActive(false);
		m_selected(index);
	}
}
