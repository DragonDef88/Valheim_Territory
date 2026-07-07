using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ManageSavesMenuElement : MonoBehaviour
{
	public delegate void BackupElementClickedHandler();

	private class BackupElement
	{
		public SaveFile File { get; private set; }

		public GameObject GuiInstance { get; private set; }

		public Button Button { get; private set; }

		public RectTransform rectTransform
		{
			get
			{
				Transform transform = GuiInstance.transform;
				return (RectTransform)(object)((transform is RectTransform) ? transform : null);
			}
		}

		public BackupElement(GameObject guiInstance, SaveFile backup, BackupElementClickedHandler clickedCallback)
		{
			GuiInstance = guiInstance;
			GuiInstance.SetActive(true);
			Button = GuiInstance.GetComponent<Button>();
			UpdateElement(backup, clickedCallback);
		}

		public void UpdateElement(SaveFile backup, BackupElementClickedHandler clickedCallback)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Invalid comparison between Unknown and I4
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Invalid comparison between Unknown and I4
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Invalid comparison between Unknown and I4
			File = backup;
			((UnityEventBase)Button.onClick).RemoveAllListeners();
			((UnityEvent)Button.onClick).AddListener((UnityAction)delegate
			{
				clickedCallback?.Invoke();
			});
			string text = backup.FileName;
			if (SaveSystem.IsCorrupt(backup))
			{
				text += " [CORRUPT]";
			}
			if (SaveSystem.IsWorldWithMissingMetaFile(backup))
			{
				text += " [MISSING META FILE]";
			}
			((Component)((Transform)rectTransform).Find("name")).GetComponent<TMP_Text>().text = text;
			((Component)((Transform)rectTransform).Find("size")).GetComponent<TMP_Text>().text = FileHelpers.BytesAsNumberString(backup.Size, 1u);
			((Component)((Transform)rectTransform).Find("date")).GetComponent<TMP_Text>().text = backup.LastModified.ToShortDateString() + " " + backup.LastModified.ToShortTimeString();
			Transform obj = ((Transform)rectTransform).Find("source");
			Transform obj2 = obj.Find("source_cloud");
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive((int)backup.m_source == 2);
			}
			Transform obj3 = obj.Find("source_local");
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive((int)backup.m_source == 1);
			}
			Transform obj4 = obj.Find("source_legacy");
			if (obj4 != null)
			{
				((Component)obj4).gameObject.SetActive((int)backup.m_source == 3);
			}
		}
	}

	public delegate void HeightChangedHandler();

	public delegate void ElementClickedHandler(ManageSavesMenuElement element, int backupElementIndex);

	public delegate void ElementExpandedChangedHandler(ManageSavesMenuElement element, bool isExpanded);

	[SerializeField]
	private Button primaryElement;

	[SerializeField]
	private Button backupElement;

	[SerializeField]
	private GameObject selectedBackground;

	[SerializeField]
	private Button arrow;

	[SerializeField]
	private TMP_Text nameText;

	[SerializeField]
	private TMP_Text sizeText;

	[SerializeField]
	private TMP_Text backupCountText;

	[SerializeField]
	private TMP_Text dateText;

	[SerializeField]
	private RectTransform sourceParent;

	private float elementHeight = 32f;

	private List<BackupElement> backupElements = new List<BackupElement>();

	private Coroutine arrowAnimationCoroutine;

	private Coroutine listAnimationCoroutine;

	public RectTransform rectTransform
	{
		get
		{
			Transform transform = ((Component)this).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	private RectTransform arrowRectTransform
	{
		get
		{
			Transform transform = ((Component)arrow).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	public bool IsExpanded { get; private set; }

	public int BackupCount => backupElements.Count;

	public SaveWithBackups Save { get; private set; }

	public event HeightChangedHandler HeightChanged;

	public event ElementClickedHandler ElementClicked;

	public event ElementExpandedChangedHandler ElementExpandedChanged;

	public void SetUp(SaveWithBackups save)
	{
		UpdatePrimaryElement();
		for (int i = 0; i < Save.BackupFiles.Length; i++)
		{
			BackupElement item = CreateBackupElement(Save.BackupFiles[i], i);
			backupElements.Add(item);
		}
		UpdateElementPositions();
	}

	public IEnumerator SetUpEnumerator(SaveWithBackups save)
	{
		Save = save;
		UpdatePrimaryElement();
		yield return null;
		for (int i = 0; i < Save.BackupFiles.Length; i++)
		{
			BackupElement item = CreateBackupElement(Save.BackupFiles[i], i);
			backupElements.Add(item);
			yield return null;
		}
		IEnumerator updateElementPositions = UpdateElementPositionsEnumerator();
		while (updateElementPositions.MoveNext())
		{
			yield return null;
		}
	}

	public void UpdateElement(SaveWithBackups save)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		Save = save;
		UpdatePrimaryElement();
		List<BackupElement> list = new List<BackupElement>();
		Dictionary<string, Dictionary<FileSource, BackupElement>> dictionary = new Dictionary<string, Dictionary<FileSource, BackupElement>>();
		for (int i = 0; i < backupElements.Count; i++)
		{
			if (!dictionary.ContainsKey(backupElements[i].File.FileName))
			{
				dictionary.Add(backupElements[i].File.FileName, new Dictionary<FileSource, BackupElement>());
			}
			dictionary[backupElements[i].File.FileName].Add(backupElements[i].File.m_source, backupElements[i]);
		}
		for (int j = 0; j < Save.BackupFiles.Length; j++)
		{
			SaveFile saveFile = Save.BackupFiles[j];
			if (dictionary.ContainsKey(saveFile.FileName) && dictionary[saveFile.FileName].ContainsKey(saveFile.m_source))
			{
				int currentIndex = j;
				dictionary[saveFile.FileName][saveFile.m_source].UpdateElement(saveFile, delegate
				{
					OnBackupElementClicked(currentIndex);
				});
				list.Add(dictionary[saveFile.FileName][saveFile.m_source]);
				dictionary[saveFile.FileName].Remove(saveFile.m_source);
				if (dictionary.Count <= 0)
				{
					dictionary.Remove(saveFile.FileName);
				}
			}
			else
			{
				BackupElement item = CreateBackupElement(saveFile, j);
				list.Add(item);
			}
		}
		foreach (KeyValuePair<string, Dictionary<FileSource, BackupElement>> item2 in dictionary)
		{
			foreach (KeyValuePair<FileSource, BackupElement> item3 in item2.Value)
			{
				Object.Destroy((Object)(object)item3.Value.GuiInstance);
			}
		}
		backupElements = list;
		float num = UpdateElementPositions();
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, IsExpanded ? num : elementHeight);
	}

	public IEnumerator UpdateElementEnumerator(SaveWithBackups save)
	{
		Save = save;
		UpdatePrimaryElement();
		List<BackupElement> newBackupElementsList = new List<BackupElement>();
		Dictionary<string, Dictionary<FileSource, BackupElement>> backupNameToElementMap = new Dictionary<string, Dictionary<FileSource, BackupElement>>();
		for (int j = 0; j < backupElements.Count; j++)
		{
			if (!backupNameToElementMap.ContainsKey(backupElements[j].File.FileName))
			{
				backupNameToElementMap.Add(backupElements[j].File.FileName, new Dictionary<FileSource, BackupElement>());
			}
			backupNameToElementMap[backupElements[j].File.FileName].Add(backupElements[j].File.m_source, backupElements[j]);
			yield return null;
		}
		for (int j = 0; j < Save.BackupFiles.Length; j++)
		{
			SaveFile saveFile = Save.BackupFiles[j];
			if (backupNameToElementMap.ContainsKey(saveFile.FileName) && backupNameToElementMap[saveFile.FileName].ContainsKey(saveFile.m_source))
			{
				int currentIndex = j;
				backupNameToElementMap[saveFile.FileName][saveFile.m_source].UpdateElement(saveFile, delegate
				{
					OnBackupElementClicked(currentIndex);
				});
				newBackupElementsList.Add(backupNameToElementMap[saveFile.FileName][saveFile.m_source]);
				backupNameToElementMap[saveFile.FileName].Remove(saveFile.m_source);
				if (backupNameToElementMap.Count <= 0)
				{
					backupNameToElementMap.Remove(saveFile.FileName);
				}
			}
			else
			{
				BackupElement item = CreateBackupElement(saveFile, j);
				newBackupElementsList.Add(item);
			}
			yield return null;
		}
		foreach (KeyValuePair<string, Dictionary<FileSource, BackupElement>> item2 in backupNameToElementMap)
		{
			foreach (KeyValuePair<FileSource, BackupElement> item3 in item2.Value)
			{
				Object.Destroy((Object)(object)item3.Value.GuiInstance);
				yield return null;
			}
		}
		backupElements = newBackupElementsList;
		float num = UpdateElementPositions();
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, IsExpanded ? num : elementHeight);
	}

	private BackupElement CreateBackupElement(SaveFile backup, int index)
	{
		return new BackupElement(Object.Instantiate<GameObject>(((Component)backupElement).gameObject, (Transform)(object)rectTransform), backup, delegate
		{
			OnBackupElementClicked(index);
		});
	}

	private float UpdateElementPositions()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float num = elementHeight;
		for (int i = 0; i < backupElements.Count; i++)
		{
			backupElements[i].rectTransform.anchoredPosition = new Vector2(backupElements[i].rectTransform.anchoredPosition.x, 0f - num);
			num += backupElements[i].rectTransform.sizeDelta.y;
		}
		return num;
	}

	private IEnumerator UpdateElementPositionsEnumerator()
	{
		float pos = elementHeight;
		for (int i = 0; i < backupElements.Count; i++)
		{
			backupElements[i].rectTransform.anchoredPosition = new Vector2(backupElements[i].rectTransform.anchoredPosition.x, 0f - pos);
			pos += backupElements[i].rectTransform.sizeDelta.y;
			yield return null;
		}
	}

	private void UpdatePrimaryElement()
	{
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Invalid comparison between Unknown and I4
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Invalid comparison between Unknown and I4
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Invalid comparison between Unknown and I4
		((Component)arrow).gameObject.SetActive(Save.BackupFiles.Length != 0);
		string text = Save.m_name;
		if (!Save.IsDeleted)
		{
			text = Save.PrimaryFile.FileName;
			if (SaveSystem.IsCorrupt(Save.PrimaryFile))
			{
				text += " [CORRUPT]";
			}
			if (SaveSystem.IsWorldWithMissingMetaFile(Save.PrimaryFile))
			{
				text += " [MISSING META]";
			}
		}
		nameText.text = text;
		sizeText.text = FileHelpers.BytesAsNumberString(Save.IsDeleted ? 0 : Save.PrimaryFile.Size, 1u) + "/" + FileHelpers.BytesAsNumberString(Save.SizeWithBackups, 1u);
		backupCountText.text = Localization.instance.Localize("$menu_backupcount", new string[1] { Save.BackupFiles.Length.ToString() });
		dateText.text = (Save.IsDeleted ? Localization.instance.Localize("$menu_deleted") : (Save.PrimaryFile.LastModified.ToShortDateString() + " " + Save.PrimaryFile.LastModified.ToShortTimeString()));
		Transform obj = ((Transform)sourceParent).Find("source_cloud");
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(!Save.IsDeleted && (int)Save.PrimaryFile.m_source == 2);
		}
		Transform obj2 = ((Transform)sourceParent).Find("source_local");
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(!Save.IsDeleted && (int)Save.PrimaryFile.m_source == 1);
		}
		Transform obj3 = ((Transform)sourceParent).Find("source_legacy");
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(!Save.IsDeleted && (int)Save.PrimaryFile.m_source == 3);
		}
		if (IsExpanded && Save.BackupFiles.Length == 0)
		{
			SetExpanded(value: false, animated: false);
		}
	}

	private void OnDestroy()
	{
		foreach (BackupElement backupElement in backupElements)
		{
			Object.Destroy((Object)(object)backupElement.GuiInstance);
		}
		backupElements.Clear();
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		elementHeight = rectTransform.sizeDelta.y;
	}

	private void OnEnable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		((UnityEvent)primaryElement.onClick).AddListener(new UnityAction(OnElementClicked));
		((UnityEvent)arrow.onClick).AddListener(new UnityAction(OnArrowClicked));
	}

	private void OnDisable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		((UnityEvent)primaryElement.onClick).RemoveListener(new UnityAction(OnElementClicked));
		((UnityEvent)arrow.onClick).RemoveListener(new UnityAction(OnArrowClicked));
	}

	private void OnElementClicked()
	{
		this.ElementClicked?.Invoke(this, -1);
	}

	private void OnBackupElementClicked(int index)
	{
		this.ElementClicked?.Invoke(this, index);
	}

	private void OnArrowClicked()
	{
		SetExpanded(!IsExpanded);
	}

	public void SetExpanded(bool value, bool animated = true)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (IsExpanded != value)
		{
			IsExpanded = value;
			this.ElementExpandedChanged?.Invoke(this, IsExpanded);
			if (arrowAnimationCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(arrowAnimationCoroutine);
			}
			if (listAnimationCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(listAnimationCoroutine);
			}
			if (animated)
			{
				arrowAnimationCoroutine = ((MonoBehaviour)this).StartCoroutine(AnimateArrow());
				listAnimationCoroutine = ((MonoBehaviour)this).StartCoroutine(AnimateList());
				return;
			}
			float num = ((!IsExpanded) ? 90 : 0);
			((Transform)arrowRectTransform).rotation = Quaternion.Euler(0f, 0f, num);
			float num2 = (IsExpanded ? (elementHeight * (float)(backupElements.Count + 1)) : elementHeight);
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, num2);
			this.HeightChanged?.Invoke();
		}
	}

	public void Select(ref int backupIndex)
	{
		if (backupIndex < 0 || BackupCount <= 0)
		{
			selectedBackground.gameObject.SetActive(true);
			backupIndex = -1;
		}
		else
		{
			backupIndex = Mathf.Clamp(backupIndex, 0, BackupCount - 1);
			((Component)((Transform)backupElements[backupIndex].rectTransform).Find("selected")).gameObject.SetActive(true);
		}
	}

	public void Deselect(int backupIndex = -1)
	{
		if (backupIndex < 0)
		{
			selectedBackground.gameObject.SetActive(false);
		}
		else if (backupIndex > backupElements.Count - 1)
		{
			ZLog.LogWarning((object)("Failed to deselect backup: Index " + backupIndex + " was outside of the valid range -1-" + (backupElements.Count - 1) + ". Ignoring."));
		}
		else
		{
			((Component)((Transform)backupElements[backupIndex].rectTransform).Find("selected")).gameObject.SetActive(false);
		}
	}

	public RectTransform GetTransform(int backupIndex = -1)
	{
		if (backupIndex < 0)
		{
			Transform transform = ((Component)primaryElement).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
		return backupElements[backupIndex].rectTransform;
	}

	private IEnumerator AnimateArrow()
	{
		Quaternion rotation = ((Transform)arrowRectTransform).rotation;
		float currentRotation = ((Quaternion)(ref rotation)).eulerAngles.z;
		float targetRotation = ((!IsExpanded) ? 90 : 0);
		float sign = Mathf.Sign(targetRotation - currentRotation);
		while (true)
		{
			currentRotation += sign * 90f * 10f * Time.deltaTime;
			if (currentRotation * sign > targetRotation * sign)
			{
				currentRotation = targetRotation;
			}
			((Transform)arrowRectTransform).rotation = Quaternion.Euler(0f, 0f, currentRotation);
			if (currentRotation == targetRotation)
			{
				break;
			}
			yield return null;
		}
		arrowAnimationCoroutine = null;
	}

	private IEnumerator AnimateList()
	{
		float currentSize = rectTransform.sizeDelta.y;
		float targetSize = (IsExpanded ? (elementHeight * (float)(backupElements.Count + 1)) : elementHeight);
		float sign = Mathf.Sign(targetSize - currentSize);
		float velocity = 0f;
		while (true)
		{
			currentSize = Mathf.SmoothDamp(currentSize, targetSize, ref velocity, 0.06f);
			if (currentSize * sign + 0.1f > targetSize * sign)
			{
				currentSize = targetSize;
			}
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, currentSize);
			this.HeightChanged?.Invoke();
			if (currentSize == targetSize)
			{
				break;
			}
			yield return null;
		}
		listAnimationCoroutine = null;
	}
}
