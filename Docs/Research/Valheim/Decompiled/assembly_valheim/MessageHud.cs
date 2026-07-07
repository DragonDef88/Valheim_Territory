using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageHud : MonoBehaviour
{
	public enum MessageType
	{
		TopLeft = 1,
		Center
	}

	private class UnlockMsg
	{
		public Sprite m_icon;

		public string m_topic;

		public string m_description;
	}

	private class MsgData
	{
		public Sprite m_icon;

		public string m_text;

		public int m_amount;
	}

	private class BiomeMessage
	{
		public string m_text;

		public bool m_playStinger;
	}

	private struct CrossFadeText
	{
		public TMP_Text text;

		public float alpha;

		public float time;
	}

	private MsgData currentMsg = new MsgData();

	private static MessageHud m_instance;

	public TMP_Text m_messageText;

	public Image m_messageIcon;

	public TMP_Text m_messageCenterText;

	public GameObject m_unlockMsgPrefab;

	public int m_maxUnlockMsgSpace = 110;

	public int m_maxUnlockMessages = 4;

	public int m_maxLogMessages = 50;

	public GameObject m_biomeFoundPrefab;

	public GameObject m_biomeFoundStinger;

	private Queue<BiomeMessage> m_biomeFoundQueue = new Queue<BiomeMessage>();

	private List<string> m_messageLog = new List<string>();

	private List<GameObject> m_unlockMessages = new List<GameObject>();

	private Queue<UnlockMsg> m_unlockMsgQueue = new Queue<UnlockMsg>();

	private Queue<MsgData> m_msgQeue = new Queue<MsgData>();

	private float m_msgQueueTimer = -1f;

	private int m_unlockMsgCount;

	private bool m_showDespiteHiddenHUD;

	private GameObject m_biomeMsgInstance;

	private List<CrossFadeText> _crossFadeTextBuffer = new List<CrossFadeText>();

	public static MessageHud instance => m_instance;

	private void Awake()
	{
		m_instance = this;
	}

	private void OnDestroy()
	{
		m_instance = null;
	}

	private void Start()
	{
		((Graphic)m_messageText).CrossFadeAlpha(0f, 0f, true);
		((Graphic)m_messageIcon).canvasRenderer.SetAlpha(0f);
		((Graphic)m_messageCenterText).CrossFadeAlpha(0f, 0f, true);
		for (int i = 0; i < m_maxUnlockMessages; i++)
		{
			m_unlockMessages.Add(null);
		}
		ZRoutedRpc.instance.Register<int, string>("ShowMessage", RPC_ShowMessage);
	}

	private void Update()
	{
		if (Hud.IsUserHidden() && !m_showDespiteHiddenHUD)
		{
			HideAll();
			return;
		}
		UpdateUnlockMsg(Time.deltaTime);
		UpdateMessage(Time.deltaTime);
		UpdateBiomeFound(Time.deltaTime);
	}

	private void HideAll()
	{
		for (int i = 0; i < m_maxUnlockMessages; i++)
		{
			if ((Object)(object)m_unlockMessages[i] != (Object)null)
			{
				Object.Destroy((Object)(object)m_unlockMessages[i]);
				m_unlockMessages[i] = null;
			}
		}
		((Graphic)m_messageText).CrossFadeAlpha(0f, 0f, true);
		((Graphic)m_messageIcon).canvasRenderer.SetAlpha(0f);
		((Graphic)m_messageCenterText).CrossFadeAlpha(0f, 0f, true);
		if (Object.op_Implicit((Object)(object)m_biomeMsgInstance))
		{
			Object.Destroy((Object)(object)m_biomeMsgInstance);
			m_biomeMsgInstance = null;
		}
	}

	public void MessageAll(MessageType type, string text)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ShowMessage", (int)type, text);
	}

	private void RPC_ShowMessage(long sender, int type, string text)
	{
		ShowMessage((MessageType)type, text);
	}

	public void ShowMessage(MessageType type, string text, int amount = 0, Sprite icon = null, bool showDespiteHiddenHUD = false)
	{
		m_showDespiteHiddenHUD = showDespiteHiddenHUD;
		if (!Hud.IsUserHidden() || showDespiteHiddenHUD)
		{
			text = Localization.instance.Localize(text);
			switch (type)
			{
			case MessageType.TopLeft:
			{
				MsgData msgData = new MsgData();
				msgData.m_icon = icon;
				msgData.m_text = text;
				msgData.m_amount = amount;
				m_msgQeue.Enqueue(msgData);
				AddLog(text);
				break;
			}
			case MessageType.Center:
				m_messageCenterText.text = text;
				_crossFadeTextBuffer.Add(new CrossFadeText
				{
					text = m_messageCenterText,
					alpha = 1f,
					time = 0f
				});
				_crossFadeTextBuffer.Add(new CrossFadeText
				{
					text = m_messageCenterText,
					alpha = 0f,
					time = 4f
				});
				break;
			}
		}
	}

	private void UpdateMessage(float dt)
	{
		if ((double)dt > 0.5)
		{
			return;
		}
		if (_crossFadeTextBuffer.Count > 0)
		{
			CrossFadeText crossFadeText = _crossFadeTextBuffer[0];
			_crossFadeTextBuffer.RemoveAt(0);
			((Graphic)crossFadeText.text).CrossFadeAlpha(crossFadeText.alpha, crossFadeText.time, true);
		}
		m_msgQueueTimer += dt;
		if (m_msgQeue.Count <= 0)
		{
			return;
		}
		MsgData msgData = m_msgQeue.Peek();
		bool flag = m_msgQueueTimer < 4f && msgData.m_text == currentMsg.m_text && (Object)(object)msgData.m_icon == (Object)(object)currentMsg.m_icon;
		if (m_msgQueueTimer >= 1f || flag)
		{
			MsgData msgData2 = m_msgQeue.Dequeue();
			m_messageText.text = msgData2.m_text;
			if (flag)
			{
				msgData2.m_amount += currentMsg.m_amount;
			}
			if (msgData2.m_amount > 1)
			{
				TMP_Text messageText = m_messageText;
				messageText.text = messageText.text + " x" + msgData2.m_amount;
			}
			_crossFadeTextBuffer.Add(new CrossFadeText
			{
				text = m_messageText,
				alpha = 1f,
				time = 0f
			});
			_crossFadeTextBuffer.Add(new CrossFadeText
			{
				text = m_messageText,
				alpha = 0f,
				time = 4f
			});
			if ((Object)(object)msgData2.m_icon != (Object)null)
			{
				m_messageIcon.sprite = msgData2.m_icon;
				((Graphic)m_messageIcon).canvasRenderer.SetAlpha(1f);
				((Graphic)m_messageIcon).CrossFadeAlpha(0f, 4f, true);
			}
			else
			{
				((Graphic)m_messageIcon).canvasRenderer.SetAlpha(0f);
			}
			currentMsg = msgData2;
			m_msgQueueTimer = 0f;
		}
	}

	private void UpdateBiomeFound(float dt)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_biomeMsgInstance != (Object)null)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_biomeMsgInstance.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("done"))
			{
				Object.Destroy((Object)(object)m_biomeMsgInstance);
				m_biomeMsgInstance = null;
			}
		}
		if (m_biomeFoundQueue.Count > 0 && (Object)(object)m_biomeMsgInstance == (Object)null && m_msgQeue.Count == 0 && m_msgQueueTimer > 2f)
		{
			BiomeMessage biomeMessage = m_biomeFoundQueue.Dequeue();
			m_biomeMsgInstance = Object.Instantiate<GameObject>(m_biomeFoundPrefab, ((Component)this).transform);
			TMP_Text component = ((Component)Utils.FindChild(m_biomeMsgInstance.transform, "Title", (IterativeSearchType)0)).GetComponent<TMP_Text>();
			string text = Localization.instance.Localize(biomeMessage.m_text);
			component.text = text;
			if (biomeMessage.m_playStinger && Object.op_Implicit((Object)(object)m_biomeFoundStinger))
			{
				Object.Instantiate<GameObject>(m_biomeFoundStinger);
			}
		}
	}

	public void ShowBiomeFoundMsg(string text, bool playStinger)
	{
		BiomeMessage biomeMessage = new BiomeMessage();
		biomeMessage.m_text = text;
		biomeMessage.m_playStinger = playStinger;
		m_biomeFoundQueue.Enqueue(biomeMessage);
	}

	public void QueueUnlockMsg(Sprite icon, string topic, string description)
	{
		UnlockMsg unlockMsg = new UnlockMsg();
		unlockMsg.m_icon = icon;
		unlockMsg.m_topic = Localization.instance.Localize(topic);
		unlockMsg.m_description = Localization.instance.Localize(description);
		m_unlockMsgQueue.Enqueue(unlockMsg);
		m_unlockMsgCount++;
		AddLog(topic + ": " + description);
		ZLog.Log((object)("Queue unlock msg:" + topic + ":" + description));
	}

	private int GetFreeUnlockMsgSlot()
	{
		for (int i = 0; i < m_unlockMessages.Count; i++)
		{
			if ((Object)(object)m_unlockMessages[i] == (Object)null)
			{
				return i;
			}
		}
		return -1;
	}

	private void UpdateUnlockMsg(float dt)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_unlockMessages.Count; i++)
		{
			GameObject val = m_unlockMessages[i];
			if (!((Object)(object)val == (Object)null))
			{
				AnimatorStateInfo currentAnimatorStateInfo = val.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0);
				if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("done"))
				{
					Object.Destroy((Object)(object)val);
					m_unlockMessages[i] = null;
					break;
				}
			}
		}
		if (m_unlockMsgQueue.Count > 0)
		{
			int freeUnlockMsgSlot = GetFreeUnlockMsgSlot();
			if (freeUnlockMsgSlot != -1)
			{
				Transform transform = ((Component)this).transform;
				GameObject val2 = Object.Instantiate<GameObject>(m_unlockMsgPrefab, transform);
				m_unlockMessages[freeUnlockMsgSlot] = val2;
				Transform transform2 = val2.transform;
				Transform obj = ((transform2 is RectTransform) ? transform2 : null);
				Vector3 val3 = Vector2.op_Implicit(((RectTransform)obj).anchoredPosition);
				val3.y -= m_maxUnlockMsgSpace * freeUnlockMsgSlot;
				((RectTransform)obj).anchoredPosition = Vector2.op_Implicit(val3);
				UnlockMsg unlockMsg = m_unlockMsgQueue.Dequeue();
				Image component = ((Component)obj.Find("UnlockMessage/icon_bkg/UnlockIcon")).GetComponent<Image>();
				TMP_Text component2 = ((Component)obj.Find("UnlockMessage/UnlockTitle")).GetComponent<TMP_Text>();
				TMP_Text component3 = ((Component)obj.Find("UnlockMessage/UnlockDescription")).GetComponent<TMP_Text>();
				component.sprite = unlockMsg.m_icon;
				component2.text = unlockMsg.m_topic;
				component3.text = unlockMsg.m_description;
			}
		}
		else if (m_unlockMsgCount > 0)
		{
			Player.m_localPlayer.Message(MessageType.TopLeft, $"{m_unlockMsgCount} $inventory_logs_new");
			m_unlockMsgCount = 0;
		}
	}

	private void AddLog(string logText)
	{
		m_messageLog.Add(logText);
		while (m_messageLog.Count > m_maxLogMessages)
		{
			m_messageLog.RemoveAt(0);
		}
	}

	public List<string> GetLog()
	{
		return m_messageLog;
	}
}
