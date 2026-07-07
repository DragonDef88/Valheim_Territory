using System;
using System.Collections.Generic;
using System.Text;
using Splatform;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Chat : Terminal
{
	public delegate void SendChatMessageRPCHandler(long user, bool filterText);

	public class WorldTextInstance
	{
		public UserInfo m_userInfo;

		public long m_talkerID;

		public GameObject m_go;

		public Vector3 m_position;

		public float m_timer;

		public GameObject m_gui;

		public TextMeshProUGUI m_textMeshField;

		public Talker.Type m_type;

		public string m_text = "";

		public string m_name => m_userInfo.GetDisplayName();
	}

	public class NpcText
	{
		public string m_topic;

		public string m_text;

		public GameObject m_go;

		public Vector3 m_offset = Vector3.zero;

		public float m_cullDistance = 20f;

		public GameObject m_gui;

		public Animator m_animator;

		public TextMeshProUGUI m_textField;

		public TextMeshProUGUI m_topicField;

		public float m_ttl;

		public bool m_timeout;

		public void SetVisible(bool visible)
		{
			m_animator.SetBool("visible", visible);
		}

		public bool IsVisible()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("visible"))
			{
				return true;
			}
			return m_animator.GetBool("visible");
		}

		public void UpdateText()
		{
			if (m_topic.Length > 0)
			{
				((TMP_Text)m_textField).text = "<color=orange>" + Localization.instance.Localize(m_topic) + "</color>\n" + Localization.instance.Localize(m_text);
			}
			else
			{
				((TMP_Text)m_textField).text = Localization.instance.Localize(m_text);
			}
		}
	}

	private static Chat m_instance;

	public float m_hideDelay = 10f;

	public float m_worldTextTTL = 5f;

	public GameObject m_worldTextBase;

	public GameObject m_npcTextBase;

	public GameObject m_npcTextBaseLarge;

	[Tooltip("If true the player has to open chat twice to enter input mode.")]
	[SerializeField]
	protected bool m_doubleOpenForVirtualKeyboard = true;

	private List<WorldTextInstance> m_worldTexts = new List<WorldTextInstance>();

	private List<NpcText> m_npcTexts = new List<NpcText>();

	private float m_hideTimer = 9999f;

	public bool m_wasFocused;

	private bool m_socialRestrictionNotificationShown;

	public static Chat instance => m_instance;

	public List<WorldTextInstance> WorldTexts => m_worldTexts;

	protected override Terminal m_terminalInstance => m_instance;

	private void OnDestroy()
	{
		Localization.OnLanguageChange = (Action)Delegate.Remove(Localization.OnLanguageChange, new Action(OnLanguageChanged));
	}

	public override void Awake()
	{
		base.Awake();
		m_instance = this;
		ZRoutedRpc.instance.Register<Vector3, int, UserInfo, string>("ChatMessage", RPC_ChatMessage);
		ZRoutedRpc.instance.Register<Vector3, Quaternion, bool>("RPC_TeleportPlayer", RPC_TeleportPlayer);
		AddString(Localization.instance.Localize("/w [text] - $chat_whisper"));
		AddString(Localization.instance.Localize("/s [text] - $chat_shout"));
		AddString(Localization.instance.Localize("/die - $chat_kill"));
		AddString(Localization.instance.Localize("/resetspawn - $chat_resetspawn"));
		AddString(Localization.instance.Localize("/[emote]"));
		StringBuilder stringBuilder = new StringBuilder("Emotes: ");
		for (int i = 0; i < 25; i++)
		{
			Emotes emotes = (Emotes)i;
			stringBuilder.Append(emotes.ToString().ToLower());
			if (i + 1 < 25)
			{
				stringBuilder.Append(", ");
			}
		}
		AddString(Localization.instance.Localize(stringBuilder.ToString()));
		AddString("");
		((Component)m_input).gameObject.SetActive(false);
		m_worldTextBase.SetActive(false);
		m_tabPrefix = '/';
		m_maxVisibleBufferLength = 20;
		Terminal.m_bindList = new List<string>(PlatformPrefs.GetString("ConsoleBindings", "").Split('\n'));
		if (Terminal.m_bindList.Count == 0)
		{
			TryRunCommand("resetbinds");
		}
		Terminal.updateBinds();
		m_autoCompleteSecrets = true;
		Localization.OnLanguageChange = (Action)Delegate.Combine(Localization.OnLanguageChange, new Action(OnLanguageChanged));
	}

	private void OnLanguageChanged()
	{
		foreach (NpcText npcText in m_npcTexts)
		{
			npcText.UpdateText();
		}
	}

	public bool HasFocus()
	{
		if ((Object)(object)m_chatWindow != (Object)null && ((Component)m_chatWindow).gameObject.activeInHierarchy)
		{
			return ((TMP_InputField)m_input).isFocused;
		}
		return false;
	}

	public bool IsChatDialogWindowVisible()
	{
		return ((Component)m_chatWindow).gameObject.activeSelf;
	}

	public override void Update()
	{
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		m_focused = false;
		m_hideTimer += Time.deltaTime;
		((Component)m_chatWindow).gameObject.SetActive(m_hideTimer < m_hideDelay);
		if (!m_wasFocused)
		{
			if ((Object)(object)Player.m_localPlayer != (Object)null && !Console.IsVisible() && !TextInput.IsVisible() && !Minimap.InTextInput() && !Menu.IsVisible() && !InventoryGui.IsVisible())
			{
				bool flag = (int)ZInput.InputLayout == 1;
				bool button = ZInput.GetButton("JoyLBumper");
				bool button2 = ZInput.GetButton("JoyLTrigger");
				if (ZInput.GetButtonDown("Chat") || (ZInput.GetButtonDown("JoyChat") && ZInput.GetButton("JoyAltKeys") && !(flag && button2) && !(!flag && button)))
				{
					m_hideTimer = 0f;
					((Component)m_chatWindow).gameObject.SetActive(true);
					((Component)m_input).gameObject.SetActive(true);
					TryShowTextCommunicationRestrictedSystemPopup();
					if (m_doubleOpenForVirtualKeyboard && Application.isConsolePlatform)
					{
						((Selectable)m_input).Select();
					}
					else
					{
						m_input.ActivateInputField();
					}
				}
			}
		}
		else if (m_wasFocused)
		{
			m_hideTimer = 0f;
			m_focused = true;
			if (ZInput.GetKeyDown((KeyCode)323, true) || ZInput.GetKey((KeyCode)324, true) || ZInput.GetKeyDown((KeyCode)27, true) || ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("JoyLStickLeft") || ZInput.GetButtonDown("JoyLStickRight") || ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyLStickDown"))
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
				((Component)m_input).gameObject.SetActive(false);
				m_focused = false;
			}
		}
		m_wasFocused = ((TMP_InputField)m_input).isFocused;
		if (!((TMP_InputField)m_input).isFocused && ((Object)(object)Console.instance == (Object)null || !((Component)Console.instance.m_chatWindow).gameObject.activeInHierarchy))
		{
			foreach (KeyValuePair<KeyCode, List<string>> bind in Terminal.m_binds)
			{
				if (!ZInput.GetKeyDown(bind.Key, true))
				{
					continue;
				}
				foreach (string item in bind.Value)
				{
					TryRunCommand(item, silentFail: true, skipAllowedCheck: true);
				}
			}
		}
		base.Update();
	}

	private void TryShowTextCommunicationRestrictedSystemPopup()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (!PrivilegeResultExtentions.IsGranted(PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)3)))
		{
			if (!m_socialRestrictionNotificationShown)
			{
				PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege.Open((Privilege)3, (PrivilegeResult)64);
				m_socialRestrictionNotificationShown = true;
			}
		}
		else
		{
			m_socialRestrictionNotificationShown = false;
		}
	}

	public new void SendInput()
	{
		base.SendInput();
		((Component)m_input).gameObject.SetActive(false);
	}

	public void Hide()
	{
		m_hideTimer = m_hideDelay;
	}

	private void LateUpdate()
	{
		UpdateWorldTexts(Time.deltaTime);
		UpdateNpcTexts(Time.deltaTime);
	}

	public void OnNewChatMessage(GameObject go, long senderID, Vector3 pos, Talker.Type type, UserInfo sender, string text)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (senderID == ZNet.instance.LocalPlayerCharacterID.UserID)
		{
			OnCheckPermissionAsyncCompleted(RelationsManagerPermissionResult.Granted);
		}
		else
		{
			RelationsManager.CheckPermissionAsync(sender.UserId, (Permission)0, isSender: false, OnCheckPermissionAsyncCompleted);
		}
		void OnCheckPermissionAsyncCompleted(RelationsManagerPermissionResult result)
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			if (result.IsGranted())
			{
				if ((Object)(object)this == (Object)null)
				{
					Debug.LogError((object)"Chat has already been destroyed!");
				}
				else
				{
					text = text.Replace('<', ' ');
					text = text.Replace('>', ' ');
					if (result == RelationsManagerPermissionResult.GrantedRequiresFiltering)
					{
						CensorShittyWords.Filter(text, out text);
					}
					if (type != Talker.Type.Ping)
					{
						m_hideTimer = 0f;
						AddString(sender.UserId, text, type);
					}
					if (!Object.op_Implicit((Object)(object)Minimap.instance) || !Object.op_Implicit((Object)(object)Player.m_localPlayer) || Minimap.instance.m_mode != 0 || !(Vector3.Distance(((Component)Player.m_localPlayer).transform.position, pos) > Minimap.instance.m_nomapPingDistance))
					{
						AddInworldText(go, senderID, pos, type, sender, text);
					}
				}
			}
		}
	}

	private void UpdateWorldTexts(float dt)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		WorldTextInstance worldTextInstance = null;
		Camera mainCamera = Utils.GetMainCamera();
		if ((Object)(object)mainCamera == (Object)null)
		{
			return;
		}
		foreach (WorldTextInstance worldText in m_worldTexts)
		{
			worldText.m_timer += dt;
			if (worldText.m_timer > m_worldTextTTL && worldTextInstance == null)
			{
				worldTextInstance = worldText;
			}
			worldText.m_position.y += dt * 0.15f;
			Vector3 zero = Vector3.zero;
			if (Object.op_Implicit((Object)(object)worldText.m_go))
			{
				Character component = worldText.m_go.GetComponent<Character>();
				zero = ((!Object.op_Implicit((Object)(object)component)) ? (worldText.m_go.transform.position + Vector3.up * 0.3f) : (component.GetHeadPoint() + Vector3.up * 0.3f));
			}
			else
			{
				zero = worldText.m_position + Vector3.up * 0.3f;
			}
			Vector3 val = Utils.WorldToScreenPointScaled(mainCamera, zero);
			if (val.x < 0f || val.x > (float)Screen.width || val.y < 0f || val.y > (float)Screen.height || val.z < 0f)
			{
				Vector3 val2 = zero - ((Component)mainCamera).transform.position;
				bool flag = Vector3.Dot(((Component)mainCamera).transform.right, val2) < 0f;
				Vector3 val3 = val2;
				val3.y = 0f;
				float magnitude = ((Vector3)(ref val3)).magnitude;
				float y = val2.y;
				Vector3 val4 = ((Component)mainCamera).transform.forward;
				val4.y = 0f;
				((Vector3)(ref val4)).Normalize();
				val4 *= magnitude;
				Vector3 val5 = val4 + Vector3.up * y;
				val = Utils.WorldToScreenPointScaled(mainCamera, ((Component)mainCamera).transform.position + val5);
				val.x = ((!flag) ? Screen.width : 0);
			}
			Transform transform = worldText.m_gui.transform;
			RectTransform rt = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			val = ClampToScreenEdge(val, rt, isChatMessage: true);
			val.z = Mathf.Min(val.z, 100f);
			worldText.m_gui.transform.position = val;
		}
		if (worldTextInstance != null)
		{
			Object.Destroy((Object)(object)worldTextInstance.m_gui);
			m_worldTexts.Remove(worldTextInstance);
		}
	}

	private void AddInworldText(GameObject go, long senderID, Vector3 position, Talker.Type type, UserInfo user, string text)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		WorldTextInstance worldTextInstance = FindExistingWorldText(senderID);
		if (worldTextInstance == null)
		{
			worldTextInstance = new WorldTextInstance();
			worldTextInstance.m_talkerID = senderID;
			worldTextInstance.m_gui = Object.Instantiate<GameObject>(m_worldTextBase, ((Component)this).transform);
			worldTextInstance.m_gui.gameObject.SetActive(true);
			Transform val = worldTextInstance.m_gui.transform.Find("Text");
			worldTextInstance.m_textMeshField = ((Component)val).GetComponent<TextMeshProUGUI>();
			m_worldTexts.Add(worldTextInstance);
		}
		worldTextInstance.m_userInfo = user;
		worldTextInstance.m_type = type;
		worldTextInstance.m_go = go;
		worldTextInstance.m_position = position;
		Color color = default(Color);
		switch (type)
		{
		case Talker.Type.Shout:
			color = Color.yellow;
			text = text.ToUpper();
			break;
		case Talker.Type.Whisper:
			((Color)(ref color))._002Ector(1f, 1f, 1f, 0.75f);
			text = text.ToLowerInvariant();
			break;
		case Talker.Type.Ping:
			((Color)(ref color))._002Ector(0.6f, 0.7f, 1f, 1f);
			text = "PING";
			break;
		default:
			color = Color.white;
			break;
		}
		((Graphic)worldTextInstance.m_textMeshField).color = color;
		worldTextInstance.m_timer = 0f;
		worldTextInstance.m_text = text;
		UpdateWorldTextField(worldTextInstance);
	}

	private void UpdateWorldTextField(WorldTextInstance wt)
	{
		string text = "";
		if (wt.m_type == Talker.Type.Shout || wt.m_type == Talker.Type.Ping)
		{
			text = wt.m_name + ": ";
		}
		text += wt.m_text;
		((TMP_Text)wt.m_textMeshField).text = text;
	}

	private WorldTextInstance FindExistingWorldText(long senderID)
	{
		foreach (WorldTextInstance worldText in m_worldTexts)
		{
			if (worldText.m_talkerID == senderID)
			{
				return worldText;
			}
		}
		return null;
	}

	protected override bool isAllowedCommand(ConsoleCommand cmd)
	{
		if (cmd.IsCheat)
		{
			return false;
		}
		return base.isAllowedCommand(cmd);
	}

	protected override void InputText()
	{
		string text = ((TMP_InputField)m_input).text;
		if (text.Length != 0)
		{
			text = ((text[0] != '/') ? ("say " + text) : text.Substring(1));
			TryRunCommand(text, Object.op_Implicit((Object)(object)this));
		}
	}

	public void TeleportPlayer(long targetPeerID, Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ZRoutedRpc.instance.InvokeRoutedRPC(targetPeerID, "RPC_TeleportPlayer", pos, rot, distantTeleport);
	}

	private void RPC_TeleportPlayer(long sender, Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer != (Object)null)
		{
			Player.m_localPlayer.TeleportTo(pos, rot, distantTeleport);
		}
	}

	private void RPC_ChatMessage(long sender, Vector3 position, int type, UserInfo userInfo, string text)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		OnNewChatMessage(null, sender, position, (Talker.Type)type, userInfo, text);
	}

	public void SendText(Talker.Type type, string text)
	{
		Player localPlayer = Player.m_localPlayer;
		if (!Object.op_Implicit((Object)(object)localPlayer))
		{
			return;
		}
		if (type == Talker.Type.Shout)
		{
			CheckPermissionsAndSendChatMessageRPCsAsync(delegate(long user, bool filterText)
			{
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				GetChatMessageData(text, filterText, out var userInfoToSend, out var textToSend);
				ZRoutedRpc.instance.InvokeRoutedRPC(user, "ChatMessage", Player.m_localPlayer.GetHeadPoint(), 2, userInfoToSend, textToSend);
			});
		}
		else
		{
			((Component)localPlayer).GetComponent<Talker>().Say(type, text);
		}
	}

	public static void CheckPermissionsAndSendChatMessageRPCsAsync(SendChatMessageRPCHandler sendMessageHandler)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (!PrivilegeResultExtentions.IsGranted(PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)3)))
		{
			return;
		}
		if (PlatformManager.DistributionPlatform.RelationsProvider == null)
		{
			sendMessageHandler(0L, RelationsManager.PlatformRequiresTextFiltering());
			return;
		}
		sendMessageHandler?.Invoke(ZNet.instance.LocalPlayerCharacterID.UserID, filterText: false);
		List<ZNet.PlayerInfo> playerList = ZNet.instance.GetPlayerList();
		for (int i = 0; i < playerList.Count; i++)
		{
			ZNet.PlayerInfo playerInfo = playerList[i];
			if (playerInfo.m_characterID == ZDOID.None)
			{
				ZLog.LogWarning((object)$"Character ID for player {playerInfo} was {ZDOID.None}. Skipping.");
			}
			else
			{
				if (playerInfo.m_characterID == ZNet.instance.LocalPlayerCharacterID)
				{
					continue;
				}
				RelationsManager.CheckPermissionAsync(playerList[i].m_userInfo.m_id, (Permission)0, isSender: true, delegate(RelationsManagerPermissionResult result)
				{
					switch (result)
					{
					case RelationsManagerPermissionResult.Granted:
						sendMessageHandler(playerInfo.m_characterID.UserID, filterText: false);
						break;
					case RelationsManagerPermissionResult.GrantedRequiresFiltering:
						sendMessageHandler(playerInfo.m_characterID.UserID, filterText: true);
						break;
					case RelationsManagerPermissionResult.Denied:
						ZLog.Log((object)$"Withholding chat message for user {playerInfo} because the {(object)(Permission)0} permission was denied.");
						break;
					default:
						ZLog.LogError((object)$"Failed to send chat message to user {playerInfo}: {result}");
						break;
					}
				});
			}
		}
	}

	public static void GetChatMessageData(string text, bool filterText, out UserInfo userInfoToSend, out string textToSend)
	{
		userInfoToSend = UserInfo.GetLocalUser();
		if (filterText)
		{
			CensorShittyWords.Filter(userInfoToSend.Name, out userInfoToSend.Name);
			CensorShittyWords.Filter(text, out textToSend);
		}
		else
		{
			textToSend = text;
		}
	}

	public void SendPing(Vector3 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if (Object.op_Implicit((Object)(object)localPlayer))
		{
			Vector3 val = position;
			val.y = ((Component)localPlayer).transform.position.y;
			ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", val, 3, UserInfo.GetLocalUser(), "");
			if (Player.m_debugMode && (Object)(object)Console.instance != (Object)null && Console.instance.IsCheatsEnabled() && (Object)(object)Console.instance != (Object)null)
			{
				Console.instance.AddString($"Pinged at: {val.x}, {val.z}");
			}
		}
	}

	public void GetShoutWorldTexts(List<WorldTextInstance> texts)
	{
		foreach (WorldTextInstance worldText in m_worldTexts)
		{
			if (worldText.m_type == Talker.Type.Shout)
			{
				texts.Add(worldText);
			}
		}
	}

	public void GetPingWorldTexts(List<WorldTextInstance> texts)
	{
		foreach (WorldTextInstance worldText in m_worldTexts)
		{
			if (worldText.m_type == Talker.Type.Ping)
			{
				texts.Add(worldText);
			}
		}
	}

	private void UpdateNpcTexts(float dt)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		NpcText npcText = null;
		Camera mainCamera = Utils.GetMainCamera();
		foreach (NpcText npcText2 in m_npcTexts)
		{
			if (!Object.op_Implicit((Object)(object)npcText2.m_go))
			{
				npcText2.m_gui.SetActive(false);
				if (npcText == null)
				{
					npcText = npcText2;
				}
				continue;
			}
			if (npcText2.m_timeout)
			{
				npcText2.m_ttl -= dt;
				if (npcText2.m_ttl <= 0f)
				{
					npcText2.SetVisible(visible: false);
					if (!npcText2.IsVisible())
					{
						npcText = npcText2;
					}
					continue;
				}
			}
			Vector3 val = npcText2.m_go.transform.position + npcText2.m_offset;
			Vector3 val2 = Utils.WorldToScreenPointScaled(mainCamera, val);
			if (val2.x < 0f || val2.x > (float)Screen.width || val2.y < 0f || val2.y > (float)Screen.height || val2.z < 0f)
			{
				npcText2.SetVisible(visible: false);
			}
			else
			{
				npcText2.SetVisible(visible: true);
				Transform transform = npcText2.m_gui.transform;
				RectTransform rt = (RectTransform)(object)((transform is RectTransform) ? transform : null);
				val2 = ClampToScreenEdge(val2, rt, isChatMessage: false);
				npcText2.m_gui.transform.position = val2;
			}
			if (Vector3.Distance(((Component)mainCamera).transform.position, val) > npcText2.m_cullDistance)
			{
				npcText2.SetVisible(visible: false);
				if (npcText == null && !npcText2.IsVisible())
				{
					npcText = npcText2;
				}
			}
		}
		if (npcText != null)
		{
			ClearNpcText(npcText);
		}
		if (Hud.instance.m_userHidden && m_npcTexts.Count > 0)
		{
			HideAllNpcTexts();
		}
	}

	public Vector3 ClampToScreenEdge(Vector3 screenPos, RectTransform rt, bool isChatMessage)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		CanvasScaler componentInParent = ((Component)this).GetComponentInParent<CanvasScaler>();
		float num = (Object.op_Implicit((Object)(object)componentInParent) ? componentInParent.scaleFactor : 1f);
		Rect rect = rt.rect;
		float num2 = ((Rect)(ref rect)).width * num;
		rect = rt.rect;
		float num3 = ((Rect)(ref rect)).height * num;
		int num4 = ((!isChatMessage) ? 1 : 2);
		screenPos.x = Mathf.Clamp(screenPos.x, num2 / 2f, (float)Screen.width - num2 / 2f);
		screenPos.y = Mathf.Clamp(screenPos.y, num3 / 2f, (float)Screen.height - num3 * (float)num4);
		return screenPos;
	}

	public void HideAllNpcTexts()
	{
		for (int num = m_npcTexts.Count - 1; num >= 0; num--)
		{
			m_npcTexts[num].SetVisible(visible: false);
			ClearNpcText(m_npcTexts[num]);
		}
	}

	public void SetNpcText(GameObject talker, Vector3 offset, float cullDistance, float ttl, string topic, string text, bool large)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!Hud.instance.m_userHidden)
		{
			NpcText npcText = FindNpcText(talker);
			if (npcText != null)
			{
				ClearNpcText(npcText);
			}
			npcText = new NpcText();
			npcText.m_topic = topic;
			npcText.m_text = text;
			npcText.m_go = talker;
			npcText.m_gui = Object.Instantiate<GameObject>(large ? m_npcTextBaseLarge : m_npcTextBase, ((Component)this).transform);
			npcText.m_gui.SetActive(true);
			npcText.m_animator = npcText.m_gui.GetComponent<Animator>();
			npcText.m_topicField = ((Component)npcText.m_gui.transform.Find("Topic")).GetComponent<TextMeshProUGUI>();
			npcText.m_textField = ((Component)npcText.m_gui.transform.Find("Text")).GetComponent<TextMeshProUGUI>();
			npcText.m_ttl = ttl;
			npcText.m_timeout = ttl > 0f;
			npcText.m_offset = offset;
			npcText.m_cullDistance = cullDistance;
			npcText.UpdateText();
			m_npcTexts.Add(npcText);
		}
	}

	public int CurrentNpcTexts()
	{
		return m_npcTexts.Count;
	}

	public bool IsDialogVisible(GameObject talker)
	{
		return FindNpcText(talker)?.IsVisible() ?? false;
	}

	public void ClearNpcText(GameObject talker)
	{
		NpcText npcText = FindNpcText(talker);
		if (npcText != null)
		{
			ClearNpcText(npcText);
		}
	}

	private void ClearNpcText(NpcText npcText)
	{
		Object.Destroy((Object)(object)npcText.m_gui);
		m_npcTexts.Remove(npcText);
	}

	private NpcText FindNpcText(GameObject go)
	{
		foreach (NpcText npcText in m_npcTexts)
		{
			if ((Object)(object)npcText.m_go == (Object)(object)go)
			{
				return npcText;
			}
		}
		return null;
	}
}
