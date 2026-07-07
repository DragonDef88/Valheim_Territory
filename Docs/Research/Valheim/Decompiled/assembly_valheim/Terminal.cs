using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GUIFramework;
using Splatform;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Terminal : MonoBehaviour
{
	public class ConsoleEventArgs
	{
		public string[] Args;

		public string ArgsAll;

		public string FullLine;

		public Terminal Context;

		public int Length => Args.Length;

		public string this[int i] => Args[i];

		public ConsoleEventArgs(string line, Terminal context)
		{
			Context = context;
			FullLine = line;
			int num = line.IndexOf(' ');
			ArgsAll = ((num > 0) ? line.Substring(num + 1) : "");
			Args = line.Split(' ');
		}

		public int TryParameterInt(int parameterIndex, int defaultValue = 1)
		{
			if (TryParameterInt(parameterIndex, out var value))
			{
				return value;
			}
			return defaultValue;
		}

		public bool TryParameterInt(int parameterIndex, out int value)
		{
			if (Args.Length <= parameterIndex || !int.TryParse(Args[parameterIndex], out value))
			{
				value = 0;
				return false;
			}
			return true;
		}

		public bool TryParameterLong(int parameterIndex, out long value)
		{
			if (Args.Length <= parameterIndex || !long.TryParse(Args[parameterIndex], out value))
			{
				value = 0L;
				return false;
			}
			return true;
		}

		public float TryParameterFloat(int parameterIndex, float defaultValue = 1f)
		{
			if (TryParameterFloat(parameterIndex, out var value))
			{
				return value;
			}
			return defaultValue;
		}

		public bool TryParameterFloat(int parameterIndex, out float value)
		{
			if (Args.Length <= parameterIndex || !float.TryParse(Args[parameterIndex].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			{
				value = 0f;
				return false;
			}
			return true;
		}

		public bool HasArgumentAnywhere(string value, int firstIndexToCheck = 0, bool toLower = true)
		{
			for (int i = firstIndexToCheck; i < Args.Length; i++)
			{
				if ((toLower && Args[i].ToLower() == value) || (!toLower && Args[i] == value))
				{
					return true;
				}
			}
			return false;
		}
	}

	public class ConsoleCommand
	{
		public string Command;

		public string Description;

		public bool IsCheat;

		public bool IsNetwork;

		public bool OnlyServer;

		public bool IsSecret;

		public bool AllowInDevBuild;

		public bool RemoteCommand;

		public bool OnlyAdmin;

		private ConsoleEventFailable actionFailable;

		private ConsoleEvent action;

		private ConsoleOptionsFetcher m_tabOptionsFetcher;

		private List<string> m_tabOptions;

		private bool m_alwaysRefreshTabOptions;

		public ConsoleCommand(string command, string description, ConsoleEventFailable action, bool isCheat = false, bool isNetwork = false, bool onlyServer = false, bool isSecret = false, bool allowInDevBuild = false, ConsoleOptionsFetcher optionsFetcher = null, bool alwaysRefreshTabOptions = false, bool remoteCommand = false, bool onlyAdmin = false)
		{
			commands[command.ToLower()] = this;
			Command = command;
			Description = description;
			actionFailable = action;
			IsCheat = isCheat;
			OnlyServer = onlyServer || onlyAdmin;
			IsSecret = isSecret;
			IsNetwork = isNetwork;
			AllowInDevBuild = allowInDevBuild;
			m_tabOptionsFetcher = optionsFetcher;
			m_alwaysRefreshTabOptions = alwaysRefreshTabOptions;
			RemoteCommand = remoteCommand;
			OnlyAdmin = onlyAdmin;
		}

		public ConsoleCommand(string command, string description, ConsoleEvent action, bool isCheat = false, bool isNetwork = false, bool onlyServer = false, bool isSecret = false, bool allowInDevBuild = false, ConsoleOptionsFetcher optionsFetcher = null, bool alwaysRefreshTabOptions = false, bool remoteCommand = false, bool onlyAdmin = false)
		{
			commands[command.ToLower()] = this;
			Command = command;
			Description = description;
			this.action = action;
			IsCheat = isCheat;
			OnlyServer = onlyServer;
			IsSecret = isSecret;
			IsNetwork = isNetwork;
			AllowInDevBuild = allowInDevBuild;
			m_tabOptionsFetcher = optionsFetcher;
			m_alwaysRefreshTabOptions = alwaysRefreshTabOptions;
			RemoteCommand = remoteCommand;
			OnlyAdmin = onlyAdmin;
		}

		public List<string> GetTabOptions()
		{
			if (m_tabOptionsFetcher != null && (m_tabOptions == null || m_alwaysRefreshTabOptions))
			{
				m_tabOptions = m_tabOptionsFetcher();
			}
			return m_tabOptions;
		}

		public void RunAction(ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				List<string> tabOptions = GetTabOptions();
				if (tabOptions != null)
				{
					foreach (string item in tabOptions)
					{
						if (item != null && args[1].ToLower() == item.ToLower())
						{
							args.Args[1] = item;
							break;
						}
					}
				}
			}
			if (action != null)
			{
				action(args);
			}
			else
			{
				object obj = actionFailable(args);
				if (obj is bool && !(bool)obj)
				{
					args.Context.AddString("<color=#8b0000>Error executing command. Check parameters and context.</color>\n   <color=#888888>" + Command + " - " + Description + "</color>");
				}
				if (obj is string text)
				{
					args.Context.AddString("<color=#8b0000>Error executing command: " + text + "</color>\n   <color=#888888>" + Command + " - " + Description + "</color>");
				}
			}
			if (Object.op_Implicit((Object)(object)Game.instance))
			{
				PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
				if (IsCheat)
				{
					playerProfile.m_usedCheats = true;
					playerProfile.IncrementStat(PlayerStatType.Cheats);
				}
				Utils.IncrementOrSet<string>(playerProfile.m_knownCommands, args[0].ToLower(), 1f);
			}
		}

		public bool ShowCommand(Terminal context)
		{
			if (!IsSecret)
			{
				if (!IsValid(context))
				{
					if (Object.op_Implicit((Object)(object)ZNet.instance) && !ZNet.instance.IsServer())
					{
						return RemoteCommand;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public bool IsValid(Terminal context, bool skipAllowedCheck = false)
		{
			if ((!IsCheat || context.IsCheatsEnabled()) && (context.isAllowedCommand(this) || skipAllowedCheck) && (!IsNetwork || Object.op_Implicit((Object)(object)ZNet.instance)))
			{
				if (OnlyServer)
				{
					if (Object.op_Implicit((Object)(object)ZNet.instance))
					{
						return ZNet.instance.IsServer();
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public delegate object ConsoleEventFailable(ConsoleEventArgs args);

	public delegate void ConsoleEvent(ConsoleEventArgs args);

	public delegate List<string> ConsoleOptionsFetcher();

	private static bool m_terminalInitialized;

	protected static List<string> m_bindList;

	public static Dictionary<string, string> m_testList = new Dictionary<string, string>();

	protected static Dictionary<KeyCode, List<string>> m_binds = new Dictionary<KeyCode, List<string>>();

	private static bool m_cheat = false;

	public static bool m_showTests;

	protected float m_lastDebugUpdate;

	protected static Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

	public static ConcurrentQueue<string> m_threadSafeMessages = new ConcurrentQueue<string>();

	public static ConcurrentQueue<string> m_threadSafeConsoleLog = new ConcurrentQueue<string>();

	protected char m_tabPrefix;

	protected bool m_autoCompleteSecrets;

	private List<string> m_history = new List<string>();

	protected string[] m_quickSelect = new string[4];

	private List<string> m_tabOptions = new List<string>();

	private int m_historyPosition;

	private int m_tabCaretPosition = -1;

	private int m_tabCaretPositionEnd;

	private int m_tabLength;

	private int m_tabIndex;

	private List<string> m_commandList = new List<string>();

	private List<Minimap.PinData> m_findPins = new List<Minimap.PinData>();

	protected bool m_focused;

	public RectTransform m_chatWindow;

	public TextMeshProUGUI m_output;

	public GuiInputField m_input;

	public TMP_Text m_search;

	private int m_lastSearchLength;

	private List<string> m_lastSearch = new List<string>();

	protected List<string> m_chatBuffer = new List<string>();

	protected const int m_maxBufferLength = 300;

	public int m_maxVisibleBufferLength = 30;

	private const int m_maxScrollHeight = 5;

	private int m_scrollHeight;

	protected abstract Terminal m_terminalInstance { get; }

	private static void InitTerminal()
	{
		if (m_terminalInitialized)
		{
			return;
		}
		m_terminalInitialized = true;
		AddConsoleCheatCommands();
		new ConsoleCommand("help", "Shows a list of console commands (optional: help 2 4 shows the second quarter)", delegate(ConsoleEventArgs args)
		{
			if (Object.op_Implicit((Object)(object)ZNet.instance) && ZNet.instance.IsServer())
			{
				Object.op_Implicit((Object)(object)Player.m_localPlayer);
			}
			else
				_ = 0;
			args.Context.IsCheatsEnabled();
			List<string> list17 = new List<string>();
			foreach (KeyValuePair<string, ConsoleCommand> command in commands)
			{
				if (command.Value.ShowCommand(args.Context))
				{
					list17.Add(command.Value.Command + " - " + command.Value.Description);
				}
			}
			list17.Sort();
			if ((Object)(object)args.Context != (Object)null)
			{
				int num40 = args.TryParameterInt(2, 5);
				if (!args.TryParameterInt(1, out var value21))
				{
					foreach (string item3 in list17)
					{
						args.Context.AddString(item3);
					}
					return;
				}
				int num41 = list17.Count / num40;
				for (int num42 = num41 * (value21 - 1); num42 < Mathf.Min(list17.Count, num41 * (value21 - 1) + num41); num42++)
				{
					args.Context.AddString(list17[num42]);
				}
			}
		});
		new ConsoleCommand("devcommands", "enables cheats", delegate(ConsoleEventArgs args)
		{
			if (Object.op_Implicit((Object)(object)ZNet.instance) && !ZNet.instance.IsServer())
			{
				ZNet.instance.RemoteCommand("devcommands");
			}
			m_cheat = !m_cheat;
			args.Context?.AddString("Dev commands: " + m_cheat);
			args.Context?.AddString("WARNING: using any dev commands is not recommended and is done at your own risk.");
			Gogan.LogEvent("Cheat", "CheatsEnabled", m_cheat.ToString(), 0L);
			args.Context.updateCommandList();
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: true);
		new ConsoleCommand("hidebetatext", "", delegate
		{
			if (Object.op_Implicit((Object)(object)Hud.instance))
			{
				Hud.instance.ToggleBetaTextVisible();
			}
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: true);
		new ConsoleCommand("ping", "ping server", delegate
		{
			if (Object.op_Implicit((Object)(object)Game.instance))
			{
				Game.instance.Ping();
			}
		});
		new ConsoleCommand("dpsdebug", "toggle dps debug print", delegate(ConsoleEventArgs args)
		{
			Character.SetDPSDebug(!Character.IsDPSDebugEnabled());
			args.Context?.AddString("DPS debug " + Character.IsDPSDebugEnabled());
		}, isCheat: true);
		new ConsoleCommand("lodbias", "set distance lod bias", delegate(ConsoleEventArgs args)
		{
			float value20;
			if (args.Length == 1)
			{
				args.Context.AddString("Lod bias:" + QualitySettings.lodBias);
			}
			else if (args.TryParameterFloat(1, out value20))
			{
				args.Context.AddString("Setting lod bias:" + value20);
				QualitySettings.lodBias = value20;
			}
		});
		new ConsoleCommand("info", "print system info", delegate(ConsoleEventArgs args)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			Terminal context3 = args.Context;
			RenderingThreadingMode renderingThreadingMode = SystemInfo.renderingThreadingMode;
			context3.AddString("Render threading mode:" + ((object)(RenderingThreadingMode)(ref renderingThreadingMode)).ToString());
			long totalMemory3 = GC.GetTotalMemory(forceFullCollection: false);
			args.Context.AddString("Total allocated mem: " + (totalMemory3 / 1048576).ToString("0") + "mb");
		});
		new ConsoleCommand("gc", "shows garbage collector information", delegate(ConsoleEventArgs args)
		{
			long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
			GC.Collect();
			long totalMemory2 = GC.GetTotalMemory(forceFullCollection: true);
			long num39 = totalMemory2 - totalMemory;
			args.Context.AddString("GC collect, Delta: " + (num39 / 1048576).ToString("0") + "mb   Total left:" + (totalMemory2 / 1048576).ToString("0") + "mb");
		}, isCheat: true);
		new ConsoleCommand("cr", "unloads unused assets", delegate(ConsoleEventArgs args)
		{
			args.Context.AddString("Unloading unused assets");
			Game.instance.CollectResources(displayMessage: true);
		}, isCheat: true);
		new ConsoleCommand("fov", "changes camera field of view", delegate(ConsoleEventArgs args)
		{
			Camera mainCamera = Utils.GetMainCamera();
			if (Object.op_Implicit((Object)(object)mainCamera))
			{
				float value19;
				if (args.Length == 1)
				{
					args.Context.AddString("Fov:" + mainCamera.fieldOfView);
				}
				else if (args.TryParameterFloat(1, out value19) && value19 > 5f)
				{
					args.Context.AddString("Setting fov to " + value19);
					Camera[] componentsInChildren2 = ((Component)mainCamera).GetComponentsInChildren<Camera>();
					for (int num38 = 0; num38 < componentsInChildren2.Length; num38++)
					{
						componentsInChildren2[num38].fieldOfView = value19;
					}
				}
			}
		});
		new ConsoleCommand("kick", "[name/ip/userID] - kick user", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string user3 = args[1];
			ZNet.instance.Kick(user3);
			return true;
		}, isCheat: false, isNetwork: true);
		new ConsoleCommand("ban", "[name/ip/userID] - ban user", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string user2 = args[1];
			ZNet.instance.Ban(user2);
			return true;
		}, isCheat: false, isNetwork: true);
		new ConsoleCommand("unban", "[ip/userID] - unban user", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string user = args[1];
			ZNet.instance.Unban(user);
			return true;
		}, isCheat: false, isNetwork: true);
		new ConsoleCommand("banned", "list banned users", delegate
		{
			ZNet.instance.PrintBanned();
		}, isCheat: false, isNetwork: true);
		new ConsoleCommand("save", "force saving of world and resets world save interval", delegate
		{
			ZNet.instance.SaveWorldAndPlayerProfiles();
		}, isCheat: false, isNetwork: true);
		new ConsoleCommand("optterrain", "optimize old terrain modifications", delegate
		{
			TerrainComp.UpgradeTerrain();
			Heightmap.UpdateTerrainAlpha();
		}, isCheat: false, isNetwork: true);
		new ConsoleCommand("genloc", "regenerate all locations.", delegate
		{
			ZoneSystem.instance.GenerateLocations();
		}, isCheat: false, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("players", "[nr] - force diffuculty scale ( 0 = reset)", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			if (args.TryParameterInt(1, out var value18))
			{
				Game.instance.SetForcePlayerDifficulty(value18);
				args.Context.AddString("Setting players to " + value18);
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("exclusivefullscreen", "changes window mode to exclusive fullscreen, or back to borderless", delegate
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Screen.fullScreenMode != 0)
			{
				Screen.fullScreenMode = (FullScreenMode)0;
			}
			else
			{
				Screen.fullScreenMode = (FullScreenMode)1;
			}
		});
		new ConsoleCommand("setkey", "[name]", delegate(ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				ZoneSystem.instance.SetGlobalKey(args.FullLine.Substring(args[0].Length + 1));
				args.Context.AddString("Setting global key " + args[1]);
			}
			else
			{
				args.Context.AddString("Syntax: setkey [key]");
			}
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list16 = Enum.GetNames(typeof(GlobalKeys)).ToList();
			list16.Remove(GlobalKeys.NonServerOption.ToString());
			return list16;
		}, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("removekey", "[name]", delegate(ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				ZoneSystem.instance.RemoveGlobalKey(args.FullLine.Substring(args[0].Length + 1));
				args.Context.AddString("Removing global key " + args[1]);
			}
			else
			{
				args.Context.AddString("Syntax: setkey [key]");
			}
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, () => (!Object.op_Implicit((Object)(object)ZoneSystem.instance)) ? null : ZoneSystem.instance.GetGlobalKeys(), alwaysRefreshTabOptions: true, remoteCommand: true);
		new ConsoleCommand("resetkeys", "[name]", delegate(ConsoleEventArgs args)
		{
			ZoneSystem.instance.ResetGlobalKeys();
			Player.m_localPlayer?.ResetUniqueKeys();
			args.Context.AddString("Global and player keys cleared");
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("resetworldkeys", "[name] Resets all world modifiers to default", delegate(ConsoleEventArgs args)
		{
			ZoneSystem.instance.ResetWorldKeys();
			args.Context.AddString("Server keys cleared");
		}, isCheat: false, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("setworldpreset", "[name] Resets all world modifiers to a named preset", delegate(ConsoleEventArgs args)
		{
			if (!Enum.TryParse<WorldPresets>(args[1], ignoreCase: true, out var result10))
			{
				return "Invalid preset";
			}
			ZoneSystem.instance.ResetWorldKeys();
			ServerOptionsGUI.m_instance.ReadKeys(ZNet.World);
			ServerOptionsGUI.m_instance.SetPreset(ZNet.World, result10);
			ServerOptionsGUI.m_instance.SetKeys(ZNet.World);
			return true;
		}, isCheat: false, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, () => Enum.GetNames(typeof(WorldPresets)).ToList(), alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("setworldmodifier", "[name] [value] Sets a world modifier value", delegate(ConsoleEventArgs args)
		{
			if (!Enum.TryParse<WorldModifiers>(args[1], ignoreCase: true, out var result8) || !Enum.TryParse<WorldModifierOption>(args[2], ignoreCase: true, out var result9))
			{
				return "Invalid input, possible valid values are: " + string.Join(", ", Enum.GetNames(typeof(WorldModifierOption)));
			}
			ServerOptionsGUI.m_instance.ReadKeys(ZNet.World);
			ServerOptionsGUI.m_instance.SetPreset(ZNet.World, result8, result9);
			ServerOptionsGUI.m_instance.SetKeys(ZNet.World);
			return true;
		}, isCheat: false, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, () => Enum.GetNames(typeof(WorldModifiers)).ToList(), alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("setkeyplayer", "[name]", delegate(ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				Player.m_localPlayer.AddUniqueKey(args.FullLine.Substring(args[0].Length + 1));
				args.Context.AddString("Setting player key " + args[1]);
			}
			else
			{
				args.Context.AddString("Syntax: setkey [key]");
			}
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, () => Enum.GetNames(typeof(PlayerKeys)).ToList());
		new ConsoleCommand("removekeyplayer", "[name]", delegate(ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				Player.m_localPlayer.RemoveUniqueKey(args.FullLine.Substring(args[0].Length + 1));
				args.Context.AddString("Removing player key " + args[1]);
			}
			else
			{
				args.Context.AddString("Syntax: setkey [key]");
			}
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, () => (!Object.op_Implicit((Object)(object)Player.m_localPlayer)) ? null : Player.m_localPlayer.GetUniqueKeys(), alwaysRefreshTabOptions: true);
		new ConsoleCommand("listkeys", "", delegate(ConsoleEventArgs args)
		{
			List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
			args.Context.AddString($"Current Keys: {globalKeys.Count}");
			foreach (string item4 in globalKeys)
			{
				args.Context.AddString("  " + item4);
			}
			args.Context.AddString($"Server Option Keys: {ZNet.World.m_startingGlobalKeys.Count}");
			foreach (string startingGlobalKey in ZNet.World.m_startingGlobalKeys)
			{
				args.Context.AddString("  " + startingGlobalKey);
			}
			if (args.Length > 2)
			{
				args.Context.AddString($"Current Keys Values: {globalKeys.Count}");
				foreach (KeyValuePair<string, string> globalKeysValue in ZoneSystem.instance.m_globalKeysValues)
				{
					args.Context.AddString("  " + globalKeysValue.Key + ": " + globalKeysValue.Value);
				}
				args.Context.AddString($"Current Keys Enums: {globalKeys.Count}");
				foreach (GlobalKeys globalKeysEnum in ZoneSystem.instance.m_globalKeysEnums)
				{
					args.Context.AddString($"  {globalKeysEnum}");
				}
			}
			if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
			{
				globalKeys = Player.m_localPlayer.GetUniqueKeys();
				args.Context.AddString($"Player Keys: {globalKeys.Count}");
				foreach (string item5 in globalKeys)
				{
					args.Context.AddString("  " + item5);
				}
			}
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("sortcraft", "[type] sorts crafting lists according to setting", delegate(ConsoleEventArgs args)
		{
			Player.m_localPlayer.RemoveUniqueKeyValue("sortcraft");
			if (args.Length >= 2 && args[1].Length > 0)
			{
				Player.m_localPlayer.AddUniqueKeyValue("sortcraft", args[1]);
				args.Context.AddString("List sorting set to: " + args[1]);
			}
			else
			{
				args.Context.AddString("List sorting reset");
			}
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, () => Enum.GetNames(typeof(InventoryGui.SortMethod)).ToList());
		new ConsoleCommand("debugmode", "fly mode", delegate(ConsoleEventArgs args)
		{
			Player.m_debugMode = !Player.m_debugMode;
			args.Context.AddString("Debugmode " + Player.m_debugMode);
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("fly", "fly mode", delegate(ConsoleEventArgs args)
		{
			Player.m_localPlayer.ToggleDebugFly();
			if (args.TryParameterInt(1, out var value17))
			{
				Character.m_debugFlySpeed = value17;
			}
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("nocost", "no build cost", delegate(ConsoleEventArgs args)
		{
			if (args.HasArgumentAnywhere("on"))
			{
				Player.m_localPlayer.SetNoPlacementCost(value: true);
			}
			else if (args.HasArgumentAnywhere("off"))
			{
				Player.m_localPlayer.SetNoPlacementCost(value: false);
			}
			else
			{
				Player.m_localPlayer.ToggleNoPlacementCost();
			}
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("raiseskill", "[skill] [amount]", delegate(ConsoleEventArgs args)
		{
			if (args.TryParameterInt(2, out var value16))
			{
				Player.m_localPlayer.GetSkills().CheatRaiseSkill(args[1], value16);
			}
			else
			{
				args.Context.AddString("Syntax: raiseskill [skill] [amount]");
			}
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list15 = Enum.GetNames(typeof(Skills.SkillType)).ToList();
			list15.Remove(Skills.SkillType.None.ToString());
			return list15;
		});
		new ConsoleCommand("resetskill", "[skill]", delegate(ConsoleEventArgs args)
		{
			if (args.Length > 1)
			{
				string name4 = args[1];
				Player.m_localPlayer.GetSkills().CheatResetSkill(name4);
			}
			else
			{
				args.Context.AddString("Syntax: resetskill [skill]");
			}
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list14 = Enum.GetNames(typeof(Skills.SkillType)).ToList();
			list14.Remove(Skills.SkillType.None.ToString());
			return list14;
		});
		new ConsoleCommand("sleep", "skips to next morning", delegate
		{
			EnvMan.instance.SkipToMorning();
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("stats", "shows player stats", delegate(ConsoleEventArgs args)
		{
			if (Object.op_Implicit((Object)(object)Game.instance))
			{
				PlayerProfile playerProfile2 = Game.instance.GetPlayerProfile();
				args.Context.AddString("Player stats");
				if (playerProfile2.m_usedCheats)
				{
					args.Context.AddString("Cheater!");
				}
				foreach (KeyValuePair<PlayerStatType, float> stat in playerProfile2.m_playerStats.m_stats)
				{
					if (PlayerProfile.m_statTypeDates.TryGetValue(stat.Key, out var value15))
					{
						args.Context.AddString("  " + value15);
					}
					args.Context.AddString($"    {stat.Key}: {stat.Value}");
				}
				args.Context.AddString("Known worlds:");
				foreach (KeyValuePair<string, float> knownWorld in playerProfile2.m_knownWorlds)
				{
					args.Context.AddString("  " + knownWorld.Key + ": " + TimeSpan.FromSeconds(knownWorld.Value).ToString("c"));
				}
				args.Context.AddString("Enemies:");
				foreach (KeyValuePair<string, float> enemyStat in playerProfile2.m_enemyStats)
				{
					args.Context.AddString($"  {Localization.instance.Localize(enemyStat.Key)}: {enemyStat.Value}");
				}
				args.Context.AddString("Items found:");
				foreach (KeyValuePair<string, float> itemPickupStat in playerProfile2.m_itemPickupStats)
				{
					args.Context.AddString($"  {Localization.instance.Localize(itemPickupStat.Key)}: {itemPickupStat.Value}");
				}
				args.Context.AddString("Crafts:");
				foreach (KeyValuePair<string, float> itemCraftStat in playerProfile2.m_itemCraftStats)
				{
					args.Context.AddString($"  {Localization.instance.Localize(itemCraftStat.Key)}: {itemCraftStat.Value}");
				}
				if (args.Length > 1)
				{
					args.Context.AddString("Known world keys:");
					foreach (KeyValuePair<string, float> knownWorldKey in playerProfile2.m_knownWorldKeys)
					{
						args.Context.AddString("  " + knownWorldKey.Key + ": " + TimeSpan.FromSeconds(knownWorldKey.Value).ToString("c"));
					}
					args.Context.AddString("Used commands:");
					foreach (KeyValuePair<string, float> knownCommand in playerProfile2.m_knownCommands)
					{
						args.Context.AddString($"  {knownCommand.Key}: {knownCommand.Value}");
					}
				}
			}
		}, isCheat: false, isNetwork: false, onlyServer: true);
		new ConsoleCommand("skiptime", "[gameseconds] skips head in seconds", delegate(ConsoleEventArgs args)
		{
			double timeSeconds2 = ZNet.instance.GetTimeSeconds();
			float num37 = args.TryParameterFloat(1, 240f);
			timeSeconds2 += (double)num37;
			ZNet.instance.SetNetTime(timeSeconds2);
			args.Context.AddString("Skipping " + num37.ToString("0") + "s , Day:" + EnvMan.instance.GetDay(timeSeconds2));
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("time", "shows current time", delegate(ConsoleEventArgs args)
		{
			double timeSeconds = ZNet.instance.GetTimeSeconds();
			bool flag2 = EnvMan.CanSleep();
			args.Context.AddString(string.Format("{0} sec, Day: {1} ({2}), {3}, Session start: {4}", timeSeconds.ToString("0.00"), EnvMan.instance.GetDay(timeSeconds), EnvMan.instance.GetDayFraction().ToString("0.00"), flag2 ? "Can sleep" : "Can NOT sleep", ZoneSystem.instance.TimeSinceStart()));
		}, isCheat: true);
		new ConsoleCommand("maxfps", "[FPS] sets fps limit", delegate(ConsoleEventArgs args)
		{
			if (args.TryParameterInt(1, out var value14))
			{
				GraphicsSettingsState settings = GraphicsSettingsManager.Instance.GetCurrentSettingsWithCurrentPresetApplied(includeBackground: true);
				settings.m_presentSettings.m_fpsLimit = value14;
				GraphicsSettingsManager.Instance.SaveAndApplyGraphicsSettingsCustom(ref settings);
				return true;
			}
			return false;
		});
		new ConsoleCommand("resetcharacter", "reset character data", delegate(ConsoleEventArgs args)
		{
			args.Context?.AddString("Reseting character");
			Player.m_localPlayer.ResetCharacter();
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("resetknownitems", "reset character known items & recipes", delegate(ConsoleEventArgs args)
		{
			args.Context?.AddString("Reseting known items for character");
			Player.m_localPlayer.ResetCharacterKnownItems();
		});
		new ConsoleCommand("tutorialreset", "reset tutorial data", delegate(ConsoleEventArgs args)
		{
			args.Context?.AddString("Reseting tutorials");
			Player.ResetSeenTutorials();
		});
		new ConsoleCommand("timescale", "[target] [fadetime, default: 1, max: 3] sets timescale", delegate(ConsoleEventArgs args)
		{
			if (args.TryParameterFloat(1, out var value13))
			{
				Game.FadeTimeScale(Mathf.Min(5f, value13), args.TryParameterFloat(2, 0f));
				return true;
			}
			return false;
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("randomevent", "start a random event", delegate
		{
			RandEventSystem.instance.StartRandomEvent();
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("event", "[name] - start event", delegate(ConsoleEventArgs args)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length < 2)
			{
				return false;
			}
			string text12 = args[1];
			if (!RandEventSystem.instance.HaveEvent(text12))
			{
				args.Context.AddString("Random event not found:" + text12);
				return true;
			}
			RandEventSystem.instance.SetRandomEventByName(text12, ((Component)Player.m_localPlayer).transform.position);
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list13 = new List<string>();
			foreach (RandomEvent @event in RandEventSystem.instance.m_events)
			{
				list13.Add(@event.m_name);
			}
			return list13;
		});
		new ConsoleCommand("stopevent", "stop current event", delegate
		{
			RandEventSystem.instance.ResetRandomEvent();
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("removedrops", "remove all item-drops in area", delegate
		{
			int num35 = 0;
			ItemDrop[] array19 = Object.FindObjectsOfType<ItemDrop>();
			foreach (ItemDrop itemDrop in array19)
			{
				Fish component16 = ((Component)itemDrop).gameObject.GetComponent<Fish>();
				if ((!Object.op_Implicit((Object)(object)component16) || component16.IsOutOfWater()) && !itemDrop.IsPiece())
				{
					ZNetView component17 = ((Component)itemDrop).GetComponent<ZNetView>();
					if (Object.op_Implicit((Object)(object)component17) && component17.IsValid() && component17.IsOwner())
					{
						component17.Destroy();
						num35++;
					}
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removed item drops: " + num35);
		}, isCheat: true);
		new ConsoleCommand("removefish", "remove all fish", delegate
		{
			int num33 = 0;
			Fish[] array18 = Object.FindObjectsOfType<Fish>();
			for (int num34 = 0; num34 < array18.Length; num34++)
			{
				ZNetView component15 = ((Component)array18[num34]).GetComponent<ZNetView>();
				if (Object.op_Implicit((Object)(object)component15) && component15.IsValid() && component15.IsOwner())
				{
					component15.Destroy();
					num33++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removed fish: " + num33);
		}, isCheat: true);
		new ConsoleCommand("printcreatures", "shows counts and levels of active creatures", delegate(ConsoleEventArgs args)
		{
			Dictionary<string, Dictionary<int, int>> counts2 = new Dictionary<string, Dictionary<int, int>>();
			GetInfo(Character.GetAllCharacters());
			GetInfo(Object.FindObjectsOfType<RandomFlyingBird>());
			GetInfo(Object.FindObjectsOfType<Fish>());
			foreach (KeyValuePair<string, Dictionary<int, int>> item6 in counts2)
			{
				string text11 = Localization.instance.Localize(item6.Key) + ": ";
				foreach (KeyValuePair<int, int> item7 in item6.Value)
				{
					text11 += $"Level {item7.Key}: {item7.Value}, ";
				}
				args.Context.AddString(text11);
			}
			void GetInfo(IEnumerable collection)
			{
				//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
				//IL_0118: Unknown result type (might be due to invalid IL or missing references)
				//IL_0127: Unknown result type (might be due to invalid IL or missing references)
				//IL_012c: Unknown result type (might be due to invalid IL or missing references)
				foreach (object item8 in collection)
				{
					if (item8 is Character character)
					{
						count(character.m_name, character.GetLevel());
					}
					else if (item8 is RandomFlyingBird)
					{
						count("Bird", 1);
					}
					else if (item8 is Fish fish)
					{
						ItemDrop component14 = ((Component)fish).GetComponent<ItemDrop>();
						if (component14 != null)
						{
							count(component14.m_itemData.m_shared.m_name, component14.m_itemData.m_quality, component14.m_itemData.m_stack);
						}
					}
				}
				foreach (object item9 in collection)
				{
					MonoBehaviour val9 = (MonoBehaviour)((item9 is MonoBehaviour) ? item9 : null);
					if (val9 != null)
					{
						args.Context.AddString(string.Format("   {0}, Dist: {1}, Offset: {2}", ((Object)val9).name, Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)val9).transform.position).ToString("0.0"), ((Component)val9).transform.position - ((Component)Player.m_localPlayer).transform.position));
					}
				}
			}
		}, isCheat: true);
		new ConsoleCommand("printnetobj", "[radius = 5] lists number of network objects by name surrounding the player", delegate(ConsoleEventArgs args)
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			float num30 = args.TryParameterFloat(1, 5f);
			ZNetView[] array16 = Object.FindObjectsOfType<ZNetView>();
			Dictionary<string, int> counts = new Dictionary<string, int>();
			int total = 0;
			ZNetView[] array17 = array16;
			foreach (ZNetView zNetView in array17)
			{
				Transform val8 = (((Object)(object)((Component)zNetView).transform.parent != (Object)null) ? ((Component)zNetView).transform.parent : ((Component)zNetView).transform);
				if (!(num30 > 0f) || !(Vector3.Distance(val8.position, ((Component)Player.m_localPlayer).transform.position) > num30))
				{
					string name3 = ((Object)val8).name;
					int num32 = name3.IndexOf('(');
					if (num32 > 0)
					{
						add(name3.Substring(0, num32));
					}
					else
					{
						add("Other");
					}
				}
			}
			args.Context.AddString($"Total network objects found: {total}");
			foreach (KeyValuePair<string, int> item10 in counts)
			{
				args.Context.AddString($"   {item10.Key}: {item10.Value}");
			}
			void add(string key)
			{
				total++;
				if (counts.TryGetValue(key, out var value12))
				{
					counts[key] = value12 + 1;
				}
				else
				{
					counts[key] = 1;
				}
			}
		}, isCheat: true);
		new ConsoleCommand("removebirds", "remove all birds", delegate
		{
			int num28 = 0;
			RandomFlyingBird[] array15 = Object.FindObjectsOfType<RandomFlyingBird>();
			for (int num29 = 0; num29 < array15.Length; num29++)
			{
				ZNetView component13 = ((Component)array15[num29]).GetComponent<ZNetView>();
				if (Object.op_Implicit((Object)(object)component13) && component13.IsValid() && component13.IsOwner())
				{
					component13.Destroy();
					num28++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removed birds: " + num28);
		}, isCheat: true);
		new ConsoleCommand("printlocations", "shows counts of loaded locations", delegate(ConsoleEventArgs args)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			new Dictionary<string, Dictionary<int, int>>();
			Location[] array14 = Object.FindObjectsOfType<Location>();
			foreach (Location location in array14)
			{
				args.Context.AddString(string.Format("   {0}, Dist: {1}, Offset: {2}", ((Object)location).name, Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)location).transform.position).ToString("0.0"), ((Component)location).transform.position - ((Component)Player.m_localPlayer).transform.position));
			}
		}, isCheat: true);
		new ConsoleCommand("find", "[text] [pingmax] searches loaded objects and location list matching name and pings them on the map. pingmax defaults to 1, if more will place pins on map instead", delegate(ConsoleEventArgs args)
		{
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length < 2)
			{
				return false;
			}
			string text10 = args[1].ToLower();
			List<Tuple<object, Vector3>> list12 = find(text10);
			list12.Sort((Tuple<object, Vector3> a, Tuple<object, Vector3> b) => Vector3.Distance(a.Item2, ((Component)Player.m_localPlayer).transform.position).CompareTo(Vector3.Distance(b.Item2, ((Component)Player.m_localPlayer).transform.position)));
			foreach (Tuple<object, Vector3> item11 in list12)
			{
				Terminal context2 = args.Context;
				object item2 = item11.Item1;
				GameObject val7 = (GameObject)((item2 is GameObject) ? item2 : null);
				context2.AddString(string.Format("   {0}, Dist: {1}, Pos: {2}", (val7 != null) ? ((Object)val7).name.ToString() : ((item11.Item1 is ZoneSystem.LocationInstance locationInstance) ? locationInstance.m_location.m_prefab.Name : "unknown"), Vector3.Distance(((Component)Player.m_localPlayer).transform.position, item11.Item2).ToString("0.0"), item11.Item2));
			}
			foreach (Minimap.PinData findPin in args.Context.m_findPins)
			{
				Minimap.instance.RemovePin(findPin);
			}
			args.Context.m_findPins.Clear();
			int num25 = Math.Min(list12.Count, args.TryParameterInt(2));
			if (num25 == 1)
			{
				Chat.instance.SendPing(list12[0].Item2);
			}
			else
			{
				for (int num26 = 0; num26 < num25; num26++)
				{
					args.Context.m_findPins.Add(Minimap.instance.AddPin(list12[num26].Item2, (list12[num26].Item1 is ZDO) ? Minimap.PinType.Icon2 : ((list12[num26].Item1 is ZoneSystem.LocationInstance) ? Minimap.PinType.Icon1 : Minimap.PinType.Icon3), (list12[num26].Item1 is ZDO zDO) ? zDO.GetString("tag") : "", save: false, isChecked: true, Player.m_localPlayer.GetPlayerID()));
				}
			}
			args.Context.AddString($"Found {list12.Count} objects containing '{text10}'");
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, findOpt);
		new ConsoleCommand("findtp", "[text] [index=-1] [closerange=30] searches loaded objects and location list matching name and teleports you to the closest one outside of closerange. Specify an index to tp to any other in the found list, a minus value means index by closest.", delegate(ConsoleEventArgs args)
		{
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length < 2 || (Object)(object)Player.m_localPlayer == (Object)null)
			{
				return false;
			}
			string text9 = args[1].ToLower();
			if (text9.Length < 1)
			{
				args.Context.AddString("You must specify a search query");
				return false;
			}
			List<Tuple<object, Vector3>> list11 = find(text9);
			int num22 = args.TryParameterInt(2, -1);
			if (num22 < 0)
			{
				list11.Sort((Tuple<object, Vector3> a, Tuple<object, Vector3> b) => Vector3.Distance(a.Item2, ((Component)Player.m_localPlayer).transform.position).CompareTo(Vector3.Distance(b.Item2, ((Component)Player.m_localPlayer).transform.position)));
				num22 *= -1;
				num22--;
			}
			num22 = Math.Min(list11.Count - 1, num22);
			if (list11.Count > 0)
			{
				int num23 = args.TryParameterInt(3, 30);
				for (int num24 = num22; num24 < list11.Count; num24++)
				{
					if (!(Vector3.Distance(((Component)Player.m_localPlayer).transform.position, list11[num24].Item2) < (float)num23))
					{
						Player.m_localPlayer.TeleportTo(list11[num24].Item2, ((Component)Player.m_localPlayer).transform.rotation, distantTeleport: true);
					}
				}
			}
			args.Context.AddString($"Found {list11.Count} objects containing '{text9}'");
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, findOpt);
		new ConsoleCommand("setfuel", "[amount=10] Sets all light fuel to specified amount", delegate(ConsoleEventArgs args)
		{
			if ((Object)(object)Player.m_localPlayer == (Object)null)
			{
				return false;
			}
			Object[] array12 = Object.FindObjectsOfType(typeof(Fireplace));
			int num20 = args.TryParameterInt(1, 10);
			Object[] array13 = array12;
			for (int num21 = 0; num21 < array13.Length; num21++)
			{
				((Fireplace)(object)array13[num21]).SetFuel(num20);
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("freefly", "freefly photo mode", delegate(ConsoleEventArgs args)
		{
			args.Context.AddString("Toggling free fly camera");
			GameCamera.instance.ToggleFreeFly();
		}, isCheat: true);
		new ConsoleCommand("ffsmooth", "freefly smoothness", delegate(ConsoleEventArgs args)
		{
			if (args.Length <= 1)
			{
				args.Context.AddString(GameCamera.instance.GetFreeFlySmoothness().ToString());
				return true;
			}
			if (args.TryParameterFloat(1, out var value11))
			{
				args.Context.AddString("Setting free fly camera smoothing:" + value11);
				GameCamera.instance.SetFreeFlySmoothness(value11);
				return true;
			}
			return false;
		}, isCheat: true);
		new ConsoleCommand("location", "[SAVE*] spawn location (CAUTION: saving permanently disabled, *unless you specify SAVE)", delegate(ConsoleEventArgs args)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length < 2)
			{
				return false;
			}
			string name2 = args[1];
			Vector3 pos = ((Component)Player.m_localPlayer).transform.position + ((Component)Player.m_localPlayer).transform.forward * 10f;
			ZoneSystem.instance.TestSpawnLocation(name2, pos, args.Length < 3 || args[2] != "SAVE");
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list10 = new List<string>();
			foreach (ZoneSystem.ZoneLocation location2 in ZoneSystem.instance.m_locations)
			{
				if (location2.m_prefab.IsValid)
				{
					list10.Add(location2.m_prefab.Name);
				}
			}
			return list10;
		}, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("vegetation", "spawn vegetation", delegate(ConsoleEventArgs args)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length < 2)
			{
				return false;
			}
			Vector3 p = ((Component)Player.m_localPlayer).transform.position + ((Component)Player.m_localPlayer).transform.forward * 2f;
			string text8 = args[1].ToLower();
			foreach (ZoneSystem.ZoneVegetation item12 in ZoneSystem.instance.m_vegetation)
			{
				if (((Object)item12.m_prefab).name.ToLower() == text8)
				{
					float num15 = Random.Range(0, 360);
					float num16 = Random.Range(item12.m_scaleMin, item12.m_scaleMax);
					float num17 = Random.Range(0f - item12.m_randTilt, item12.m_randTilt);
					float num18 = Random.Range(0f - item12.m_randTilt, item12.m_randTilt);
					ZoneSystem.instance.GetGroundData(ref p, out var normal, out var _, out var _, out var _);
					if (item12.m_snapToStaticSolid && ZoneSystem.instance.GetStaticSolidHeight(p, out var height, out var normal2))
					{
						p.y = height;
						normal = normal2;
					}
					if (item12.m_snapToWater)
					{
						p.y = 30f;
					}
					p.y += item12.m_groundOffset;
					Quaternion identity = Quaternion.identity;
					if (item12.m_chanceToUseGroundTilt > 0f && Random.value <= item12.m_chanceToUseGroundTilt)
					{
						Quaternion val6 = Quaternion.Euler(0f, num15, 0f);
						identity = Quaternion.LookRotation(Vector3.Cross(normal, val6 * Vector3.forward), normal);
					}
					else
					{
						identity = Quaternion.Euler(num17, num15, num18);
					}
					GameObject obj3 = Object.Instantiate<GameObject>(item12.m_prefab, p, identity);
					obj3.GetComponent<ZNetView>().SetLocalScale(new Vector3(num16, num16, num16));
					Collider[] componentsInChildren = obj3.GetComponentsInChildren<Collider>();
					foreach (Collider obj4 in componentsInChildren)
					{
						obj4.enabled = false;
						obj4.enabled = true;
					}
					return true;
				}
			}
			return "No vegeration prefab named '" + args[1] + "' found";
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list9 = new List<string>();
			foreach (ZoneSystem.ZoneVegetation item13 in ZoneSystem.instance.m_vegetation)
			{
				if ((Object)(object)item13.m_prefab != (Object)null)
				{
					list9.Add(((Object)item13.m_prefab).name);
				}
			}
			return list9;
		}, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("nextseed", "forces the next dungeon to a seed (CAUTION: saving permanently disabled)", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return true;
			}
			if (args.TryParameterInt(1, out var value10))
			{
				DungeonGenerator.m_forceSeed = value10;
				ZoneSystem.instance.m_didZoneTest = true;
				MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Location seed set, world saving DISABLED until restart");
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("spawn", "[amount] [level] [radius] [p/e/i] - spawn something. (End word with a star (*) to create each object containing that word.) Add a 'p' after to try to pick up the spawned items, adding 'e' will try to use/equip, 'i' will only spawn and pickup if you don't have one in your inventory.", delegate(ConsoleEventArgs args)
		{
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length <= 1 || !Object.op_Implicit((Object)(object)ZNetScene.instance))
			{
				return false;
			}
			string text5 = args[1];
			int count = args.TryParameterInt(2);
			int level2 = args.TryParameterInt(3);
			float radius = args.TryParameterFloat(4, 0.5f);
			args.TryParameterInt(5, -1);
			bool pickup = args.HasArgumentAnywhere("p", 2);
			bool use = args.HasArgumentAnywhere("e", 2);
			bool onlyIfMissing = args.HasArgumentAnywhere("i", 2);
			Dictionary<string, object> vals = null;
			string[] args2 = args.Args;
			foreach (string text6 in args2)
			{
				if (text6.Contains("::"))
				{
					string[] array9 = text6.Split(new string[1] { "::" }, StringSplitOptions.None);
					string[] array10 = array9[0].Split('.');
					if (array9.Length >= 2 && array10.Length >= 2)
					{
						if (vals == null)
						{
							vals = new Dictionary<string, object>();
						}
						bool result3;
						float result4;
						float result5;
						float result6;
						float result7;
						if (int.TryParse(array9[1], out var result2))
						{
							vals[array9[0]] = result2;
						}
						else if (bool.TryParse(array9[1], out result3))
						{
							vals[array9[0]] = result3;
						}
						else if (float.TryParse(array9[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result4))
						{
							vals[array9[0]] = result4;
						}
						else if (array9.Length >= 4 && float.TryParse(array9[1], out result5) && float.TryParse(array9[2], out result6) && float.TryParse(array9[3], out result7))
						{
							vals[array9[0]] = (object)new Vector3(result5, result6, result7);
						}
						else
						{
							vals[array9[0]] = array9[1];
						}
					}
				}
			}
			DateTime now = DateTime.Now;
			if (text5.Length >= 2 && text5[text5.Length - 1] == '*')
			{
				text5 = text5.Substring(0, text5.Length - 1).ToLower();
				foreach (string prefabName in ZNetScene.instance.GetPrefabNames())
				{
					string text7 = prefabName.ToLower();
					if (text7.Contains(text5) && (text5.Contains("fx") || !text7.Contains("fx")))
					{
						spawn(prefabName);
					}
				}
			}
			else
			{
				spawn(text5);
			}
			ZLog.Log((object)("Spawn time :" + (DateTime.Now - now).TotalMilliseconds + " ms"));
			Gogan.LogEvent("Cheat", "Spawn", text5, count);
			return true;
			void spawn(string name)
			{
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0054: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Unknown result type (might be due to invalid IL or missing references)
				//IL_0091: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Unknown result type (might be due to invalid IL or missing references)
				//IL_009b: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
				GameObject prefab2 = ZNetScene.instance.GetPrefab(name);
				if (!Object.op_Implicit((Object)(object)prefab2))
				{
					Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name);
				}
				else
				{
					for (int num14 = 0; num14 < count; num14++)
					{
						Vector3 val4 = Random.insideUnitSphere * ((count == 1) ? 0f : radius);
						Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + name);
						GameObject val5 = Object.Instantiate<GameObject>(prefab2, ((Component)Player.m_localPlayer).transform.position + ((Component)Player.m_localPlayer).transform.forward * 2f + Vector3.up + val4, Quaternion.identity);
						if (vals != null)
						{
							ZNetView component10 = val5.GetComponent<ZNetView>();
							if (component10 != null && component10.IsValid())
							{
								component10.GetZDO().Set("HasFields", value: true);
								foreach (KeyValuePair<string, object> item14 in vals)
								{
									string[] array11 = item14.Key.Split('.');
									if (array11.Length >= 2)
									{
										StringExtensionMethods.GetStableHashCode("HasFields" + array11[0]);
										component10.GetZDO().Set("HasFields" + array11[0], value: true);
										item14.Value.GetType();
										if (item14.Value is float)
										{
											component10.GetZDO().Set(item14.Key, (float)item14.Value);
										}
										else if (item14.Value is int)
										{
											component10.GetZDO().Set(item14.Key, (int)item14.Value);
										}
										else if (item14.Value is bool)
										{
											component10.GetZDO().Set(item14.Key, (bool)item14.Value);
										}
										else
										{
											component10.GetZDO().Set(item14.Key, item14.Value.ToString());
										}
									}
								}
								component10.LoadFields();
							}
						}
						ItemDrop component11 = val5.GetComponent<ItemDrop>();
						ItemDrop.OnCreateNew(val5);
						if (level2 > 1)
						{
							if (Object.op_Implicit((Object)(object)component11))
							{
								level2 = Mathf.Min(level2, 4);
							}
							else
							{
								level2 = Mathf.Min(level2, 9);
							}
							val5.GetComponent<Character>()?.SetLevel(level2);
							if (level2 > 4)
							{
								level2 = 4;
							}
							if (Object.op_Implicit((Object)(object)component11))
							{
								component11.SetQuality(level2);
							}
						}
						if (pickup || use || onlyIfMissing)
						{
							if (onlyIfMissing && Object.op_Implicit((Object)(object)component11) && Player.m_localPlayer.GetInventory().HaveItem(component11.m_itemData.m_shared.m_name))
							{
								ZNetView component12 = val5.GetComponent<ZNetView>();
								if (component12 != null)
								{
									component12.Destroy();
									continue;
								}
							}
							if (Player.m_localPlayer.Pickup(val5, autoequip: false, autoPickupDelay: false) && use && Object.op_Implicit((Object)(object)component11))
							{
								Player.m_localPlayer.UseItem(Player.m_localPlayer.GetInventory(), component11.m_itemData, fromInventoryGui: false);
							}
						}
					}
				}
			}
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, () => (!Object.op_Implicit((Object)(object)ZNetScene.instance)) ? new List<string>() : ZNetScene.instance.GetPrefabNames(), alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("catch", "[fishname] [level] simulates catching a fish", delegate(ConsoleEventArgs args)
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			string text4 = args[1];
			int num12 = args.TryParameterInt(2);
			num12 = Mathf.Min(num12, 4);
			GameObject prefab = ZNetScene.instance.GetPrefab(text4);
			if (!Object.op_Implicit((Object)(object)prefab))
			{
				return "No prefab named: " + text4;
			}
			Fish componentInChildren = prefab.GetComponentInChildren<Fish>();
			if (!Object.op_Implicit((Object)(object)componentInChildren))
			{
				return "No fish prefab named: " + text4;
			}
			GameObject obj2 = Object.Instantiate<GameObject>(prefab, ((Component)Player.m_localPlayer).transform.position, Quaternion.identity);
			componentInChildren = obj2.GetComponentInChildren<Fish>();
			ItemDrop component9 = obj2.GetComponent<ItemDrop>();
			if (Object.op_Implicit((Object)(object)component9))
			{
				component9.SetQuality(num12);
			}
			string msg = FishingFloat.Catch(componentInChildren, Player.m_localPlayer);
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg);
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, () => new List<string>
		{
			"Fish1", "Fish2", "Fish3", "Fish4_cave", "Fish5", "Fish6", "Fish7", "Fish8", "Fish9", "Fish10",
			"Fish11", "Fish12"
		});
		new ConsoleCommand("itemset", "[name] [item level override] [keep] - spawn a premade named set, add 'keep' to not drop current items.", delegate(ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				ItemSets.instance.TryGetSet(args.Args[1], !args.HasArgumentAnywhere("keep"), args.TryParameterInt(2, -1), args.TryParameterInt(3, -1));
				return true;
			}
			return "Specify name of itemset.";
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, () => ItemSets.instance.GetSetNames(), alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("pos", "print current player position", delegate(ConsoleEventArgs args)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			Player localPlayer2 = Player.m_localPlayer;
			if (Object.op_Implicit((Object)(object)localPlayer2) && Object.op_Implicit((Object)(object)ZoneSystem.instance))
			{
				Terminal context = args.Context;
				if (context != null)
				{
					Vector3 position = ((Component)localPlayer2).transform.position;
					context.AddString(string.Format("Player position (X,Y,Z): {0} (Zone: {1})", ((Vector3)(ref position)).ToString("F0"), ZoneSystem.GetZone(((Component)localPlayer2).transform.position)));
				}
			}
		}, isCheat: true);
		new ConsoleCommand("recall", "[*name] recalls players to you, optionally that match given name", delegate(ConsoleEventArgs args)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			foreach (ZNetPeer peer in ZNet.instance.GetPeers())
			{
				if (peer.m_playerName != Player.m_localPlayer.GetPlayerName() && (args.Length < 2 || peer.m_playerName.ToLower().Contains(args[1].ToLower())))
				{
					Chat.instance.TeleportPlayer(peer.m_uid, ((Component)Player.m_localPlayer).transform.position, ((Component)Player.m_localPlayer).transform.rotation, distantTeleport: true);
				}
			}
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("goto", "[x,z] - teleport", delegate(ConsoleEventArgs args)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length < 3 || !args.TryParameterInt(1, out var value8) || !args.TryParameterInt(2, out var value9))
			{
				return false;
			}
			Player localPlayer = Player.m_localPlayer;
			if (Object.op_Implicit((Object)(object)localPlayer))
			{
				Vector3 val3 = default(Vector3);
				((Vector3)(ref val3))._002Ector((float)value8, ((Component)localPlayer).transform.position.y, (float)value9);
				float num11 = (localPlayer.IsDebugFlying() ? 400f : 30f);
				val3.y = Mathf.Clamp(val3.y, 30f, num11);
				localPlayer.TeleportTo(val3, ((Component)localPlayer).transform.rotation, distantTeleport: true);
			}
			Gogan.LogEvent("Cheat", "Goto", "", 0L);
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("exploremap", "explore entire map", delegate
		{
			Minimap.instance.ExploreAll();
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: true);
		new ConsoleCommand("resetmap", "reset map exploration", delegate
		{
			Minimap.instance.Reset();
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: true);
		new ConsoleCommand("resetsharedmap", "removes any shared map data from cartography table", delegate
		{
			Minimap.instance.ResetSharedMapData();
		});
		new ConsoleCommand("restartparty", "restart playfab party network", delegate
		{
			if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
			{
				if (ZNet.instance.IsServer())
				{
					ZPlayFabMatchmaking.ResetParty();
				}
				else
				{
					ZPlayFabSocket.ScheduleResetParty();
				}
			}
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: true);
		new ConsoleCommand("puke", "empties your stomach of food", delegate
		{
			if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
			{
				Player.m_localPlayer.ClearFood();
			}
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("tame", "tame all nearby tameable creatures", delegate
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			Tameable.TameAllInArea(((Component)Player.m_localPlayer).transform.position, 20f);
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("aggravate", "aggravated all nearby neutrals", delegate
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			BaseAI.AggravateAllInArea(((Component)Player.m_localPlayer).transform.position, 20f, BaseAI.AggravatedReason.Damage);
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("killall", "kill nearby creatures", delegate
		{
			List<Character> allCharacters4 = Character.GetAllCharacters();
			int num8 = 0;
			int num9 = 0;
			foreach (Character item15 in allCharacters4)
			{
				if (!item15.IsPlayer() && !((Object)(object)((Component)item15).GetComponent<Piece>() != (Object)null))
				{
					item15.Damage(new HitData(1E+10f));
					num8++;
				}
			}
			SpawnArea[] array8 = Object.FindObjectsByType<SpawnArea>((FindObjectsSortMode)0);
			for (int num10 = 0; num10 < array8.Length; num10++)
			{
				Destructible component8 = ((Component)array8[num10]).gameObject.GetComponent<Destructible>();
				if (component8 != null)
				{
					component8.Damage(new HitData(1E+10f));
					num9++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, string.Format("Killed {0} monsters{1}", num8, (num9 > 0) ? $" & {num9} spawners." : "."));
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("killenemycreatures", "kill nearby enemies", delegate
		{
			List<Character> allCharacters3 = Character.GetAllCharacters();
			int num7 = 0;
			foreach (Character item16 in allCharacters3)
			{
				if (ShouldKillAll(item16))
				{
					item16.Damage(new HitData(1E+10f));
					num7++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"Killed {num7} monsters.");
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("killenemies", "kill nearby enemies", delegate
		{
			List<Character> allCharacters2 = Character.GetAllCharacters();
			int num5 = 0;
			int num6 = 0;
			foreach (Character item17 in allCharacters2)
			{
				if (ShouldKillAll(item17))
				{
					item17.Damage(new HitData(1E+10f));
					num5++;
				}
			}
			SpawnArea[] array7 = Object.FindObjectsByType<SpawnArea>((FindObjectsSortMode)0);
			for (int n = 0; n < array7.Length; n++)
			{
				Destructible component7 = ((Component)array7[n]).gameObject.GetComponent<Destructible>();
				if (component7 != null)
				{
					component7.Damage(new HitData(1E+10f));
					num6++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, string.Format("Killed {0} monsters{1}", num5, (num6 > 0) ? $" & {num6} spawners." : "."));
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("killtame", "kill nearby tame creatures.", delegate
		{
			List<Character> allCharacters = Character.GetAllCharacters();
			int num4 = 0;
			foreach (Character item18 in allCharacters)
			{
				if (ShouldKillAll(item18))
				{
					item18.Damage(new HitData
					{
						m_damage = 
						{
							m_damage = 1E+10f
						}
					});
					num4++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Killing all tame creatures:" + num4);
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("heal", "heal to full health & stamina", delegate
		{
			if (!((Object)(object)Player.m_localPlayer == (Object)null))
			{
				Player.m_localPlayer.Heal(Player.m_localPlayer.GetMaxHealth());
				Player.m_localPlayer.AddStamina(Player.m_localPlayer.GetMaxStamina());
				Player.m_localPlayer.AddEitr(Player.m_localPlayer.GetMaxEitr());
			}
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("adrenaline", "sets adrenaline level", delegate(ConsoleEventArgs args)
		{
			if (!((Object)(object)Player.m_localPlayer == (Object)null))
			{
				Player.m_localPlayer.AddAdrenaline(args.TryParameterFloat(1, 100f) - Player.m_localPlayer.GetAdrenaline());
			}
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("god", "invincible mode", delegate(ConsoleEventArgs args)
		{
			if (!((Object)(object)Player.m_localPlayer == (Object)null))
			{
				Player.m_localPlayer.SetGodMode(args.HasArgumentAnywhere("on") || (!args.HasArgumentAnywhere("off") && !Player.m_localPlayer.InGodMode()));
				args.Context.AddString("God mode:" + Player.m_localPlayer.InGodMode());
				Gogan.LogEvent("Cheat", "God", Player.m_localPlayer.InGodMode().ToString(), 0L);
			}
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("ghost", "", delegate(ConsoleEventArgs args)
		{
			if (!((Object)(object)Player.m_localPlayer == (Object)null))
			{
				Player.m_localPlayer.SetGhostMode(args.HasArgumentAnywhere("on") || (!args.HasArgumentAnywhere("off") && !Player.m_localPlayer.InGhostMode()));
				args.Context.AddString("Ghost mode:" + Player.m_localPlayer.InGhostMode());
				Gogan.LogEvent("Cheat", "Ghost", Player.m_localPlayer.InGhostMode().ToString(), 0L);
			}
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("nospawn", "toggles natural spawning of monsters", delegate(ConsoleEventArgs args)
		{
			SpawnSystem.m_nospawn = args.HasArgumentAnywhere("on") || (!args.HasArgumentAnywhere("off") && !SpawnSystem.m_nospawn);
			args.Context.AddString("Nospawn: " + SpawnSystem.m_nospawn);
		}, isCheat: true);
		new ConsoleCommand("beard", "change beard", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
			{
				Player.m_localPlayer.SetBeard(args[1]);
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list8 = new List<string>();
			foreach (ItemDrop allItem in ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard"))
			{
				list8.Add(((Object)allItem).name);
			}
			return list8;
		});
		new ConsoleCommand("hair", "change hair", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
			{
				Player.m_localPlayer.SetHair(args[1]);
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, delegate
		{
			List<string> list7 = new List<string>();
			foreach (ItemDrop allItem2 in ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair"))
			{
				list7.Add(((Object)allItem2).name);
			}
			return list7;
		});
		new ConsoleCommand("model", "change player model", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)Player.m_localPlayer) && args.TryParameterInt(1, out var value7))
			{
				Player.m_localPlayer.SetPlayerModel(value7);
			}
			return true;
		}, isCheat: true);
		new ConsoleCommand("tod", "-1 OR [0-1]", delegate(ConsoleEventArgs args)
		{
			if ((Object)(object)EnvMan.instance == (Object)null || args.Length < 2 || !args.TryParameterFloat(1, out var value6))
			{
				return false;
			}
			args.Context.AddString("Setting time of day:" + value6);
			if (value6 < 0f)
			{
				EnvMan.instance.m_debugTimeOfDay = false;
			}
			else
			{
				EnvMan.instance.m_debugTimeOfDay = true;
				EnvMan.instance.m_debugTime = Mathf.Clamp01(value6);
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: true);
		new ConsoleCommand("env", "[env] override environment", delegate(ConsoleEventArgs args)
		{
			if ((Object)(object)EnvMan.instance == (Object)null || args.Length < 2)
			{
				return false;
			}
			string text3 = string.Join(" ", args.Args, 1, args.Args.Length - 1);
			args.Context.AddString("Setting debug enviornment:" + text3);
			EnvMan.instance.m_debugEnv = text3;
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: true, delegate
		{
			List<string> list6 = new List<string>();
			foreach (EnvSetup environment in EnvMan.instance.m_environments)
			{
				list6.Add(environment.m_name);
			}
			return list6;
		});
		new ConsoleCommand("resetenv", "disables environment override", delegate(ConsoleEventArgs args)
		{
			if ((Object)(object)EnvMan.instance == (Object)null)
			{
				return false;
			}
			args.Context.AddString("Resetting debug environment");
			EnvMan.instance.m_debugEnv = "";
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: true, isSecret: false, allowInDevBuild: true);
		new ConsoleCommand("wind", "[angle] [intensity]", delegate(ConsoleEventArgs args)
		{
			if (args.TryParameterFloat(1, out var value4) && args.TryParameterFloat(2, out var value5))
			{
				EnvMan.instance.SetDebugWind(value4, value5);
				return true;
			}
			return false;
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("resetwind", "", delegate
		{
			EnvMan.instance.ResetDebugWind();
		}, isCheat: true, isNetwork: false, onlyServer: true);
		new ConsoleCommand("clear", "clear the console window", delegate(ConsoleEventArgs args)
		{
			args.Context.m_chatBuffer.Clear();
			args.Context.UpdateChat();
		});
		new ConsoleCommand("filtercraft", "[name] filters crafting list to contain part of text", delegate(ConsoleEventArgs args)
		{
			if (args.Length <= 1)
			{
				Player.s_FilterCraft.Clear();
			}
			else
			{
				Player.s_FilterCraft = args.ArgsAll.Split(' ').ToList();
			}
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true);
		new ConsoleCommand("clearstatus", "clear any status modifiers", delegate
		{
			Player.m_localPlayer.ClearHardDeath();
			Player.m_localPlayer.GetSEMan().RemoveAllStatusEffects();
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("addstatus", "[name] adds a status effect (ex: Rested, Burning, SoftDeath, Wet, etc)", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			Player.m_localPlayer.GetSEMan().AddStatusEffect(StringExtensionMethods.GetStableHashCode(args[1]), resetTime: true);
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, delegate
		{
			List<StatusEffect> statusEffects2 = ObjectDB.instance.m_StatusEffects;
			List<string> list5 = new List<string>();
			foreach (StatusEffect item19 in statusEffects2)
			{
				list5.Add(((Object)item19).name);
			}
			return list5;
		}, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("setpower", "[name] sets your current guardian power and resets cooldown (ex: GP_Eikthyr, GP_TheElder, etc)", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			Player.m_localPlayer.SetGuardianPower(args[1]);
			Player.m_localPlayer.m_guardianPowerCooldown = 0f;
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: true, delegate
		{
			List<StatusEffect> statusEffects = ObjectDB.instance.m_StatusEffects;
			List<string> list4 = new List<string>();
			foreach (StatusEffect item20 in statusEffects)
			{
				list4.Add(((Object)item20).name);
			}
			return list4;
		}, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("bind", "[keycode] [command and parameters] bind a key to a console command. note: may cause conflicts with game controls", delegate(ConsoleEventArgs args)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length < 2)
			{
				return false;
			}
			if (!Enum.TryParse<KeyCode>(args[1], ignoreCase: true, out KeyCode result))
			{
				args.Context.AddString("'" + args[1] + "' is not a valid UnityEngine.KeyCode.");
			}
			else if (!ZInput.IsKeyCodeValid(result))
			{
				args.Context.AddString($"'{result}' lacks a proper Key enum counterpart.");
			}
			else
			{
				string item = string.Join(" ", args.Args, 1, args.Length - 1);
				m_bindList.Add(item);
				updateBinds();
			}
			return true;
		});
		new ConsoleCommand("unbind", "[keycode] clears all binds connected to keycode", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			for (int num3 = m_bindList.Count - 1; num3 >= 0; num3--)
			{
				if (m_bindList[num3].Split(' ')[0].ToLower() == args[1].ToLower())
				{
					m_bindList.RemoveAt(num3);
				}
			}
			updateBinds();
			return true;
		});
		new ConsoleCommand("printbinds", "prints current binds", delegate(ConsoleEventArgs args)
		{
			foreach (string bind in m_bindList)
			{
				args.Context.AddString(bind);
			}
		});
		new ConsoleCommand("resetbinds", "resets all custom binds to default dev commands", delegate
		{
			for (int num2 = m_bindList.Count - 1; num2 >= 0; num2--)
			{
				m_bindList.Remove(m_bindList[num2]);
			}
			updateBinds();
		});
		new ConsoleCommand("tombstone", "[name] creates a tombstone with given name", delegate(ConsoleEventArgs args)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			GameObject obj = Object.Instantiate<GameObject>(Player.m_localPlayer.m_tombstone, Player.m_localPlayer.GetCenterPoint(), ((Component)Player.m_localPlayer).transform.rotation);
			Container component5 = obj.GetComponent<Container>();
			ItemDrop coinPrefab = StoreGui.instance.m_coinPrefab;
			component5.GetInventory().AddItem(((Object)((Component)coinPrefab).gameObject).name, 1, coinPrefab.m_itemData.m_quality, coinPrefab.m_itemData.m_variant, 0L, "", pickedUp: true);
			TombStone component6 = obj.GetComponent<TombStone>();
			PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
			string ownerName = ((args.Args.Length >= 2) ? args.Args[1] : playerProfile.GetName());
			component6.Setup(ownerName, playerProfile.GetPlayerID());
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("test", "[key] [value] set test string, with optional value. set empty existing key to remove", delegate(ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				m_showTests = !m_showTests;
				return true;
			}
			string text2 = ((args.Length >= 3) ? args[2] : "");
			if (m_testList.ContainsKey(args[1]) && text2.Length == 0)
			{
				m_testList.Remove(args[1]);
				args.Context?.AddString("'" + args[1] + "' removed");
			}
			else
			{
				m_testList[args[1]] = text2;
				args.Context?.AddString("'" + args[1] + "' added with value '" + text2 + "'");
			}
			switch (args[1].ToLower())
			{
			case "ngenemyac":
				Game.instance.m_worldLevelEnemyBaseAC = int.Parse(args[2]);
				break;
			case "ngenemyhp":
				Game.instance.m_worldLevelEnemyHPMultiplier = float.Parse(args[2]);
				break;
			case "ngenemydamage":
				Game.instance.m_worldLevelEnemyBaseDamage = int.Parse(args[2]);
				break;
			case "ngplayerac":
				Game.instance.m_worldLevelGearBaseAC = int.Parse(args[2]);
				break;
			case "ngplayerdamage":
				Game.instance.m_worldLevelGearBaseDamage = int.Parse(args[2]);
				break;
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: true);
		new ConsoleCommand("forcedelete", "[radius] [*name] force remove all objects within given radius. If name is entered, only deletes items with matching names. Caution! Use at your own risk. Make backups! Radius default: 5, max: 50.", delegate(ConsoleEventArgs args)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Player.m_localPlayer == (Object)null)
			{
				return false;
			}
			float num = Math.Min(50f, args.TryParameterFloat(1, 5f));
			Object[] array6 = Object.FindObjectsOfType(typeof(GameObject));
			for (int m = 0; m < array6.Length; m++)
			{
				GameObject val2 = (GameObject)array6[m];
				if (Vector3.Distance(val2.transform.position, ((Component)Player.m_localPlayer).transform.position) < num)
				{
					string path = Utils.GetPath(val2.gameObject.transform);
					if (!((Object)(object)val2.GetComponentInParent<Game>() != (Object)null) && !((Object)(object)val2.GetComponentInParent<Player>() != (Object)null) && !((Object)(object)val2.GetComponentInParent<Valkyrie>() != (Object)null) && !((Object)(object)val2.GetComponentInParent<LocationProxy>() != (Object)null) && !((Object)(object)val2.GetComponentInParent<Room>() != (Object)null) && !((Object)(object)val2.GetComponentInParent<Vegvisir>() != (Object)null) && !((Object)(object)val2.GetComponentInParent<DungeonGenerator>() != (Object)null) && !path.Contains("StartTemple") && !path.Contains("BossStone") && (args.Length <= 2 || ((Object)val2).name.ToLower().Contains(args[2].ToLower())))
					{
						Destructible component3 = val2.GetComponent<Destructible>();
						ZNetView component4 = val2.GetComponent<ZNetView>();
						if ((Object)(object)component3 != (Object)null)
						{
							component3.DestroyNow();
						}
						else if ((Object)(object)component4 != (Object)null && Object.op_Implicit((Object)(object)ZNetScene.instance))
						{
							ZNetScene.instance.Destroy(val2);
						}
					}
				}
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("stopfire", "Puts out all spreading fires and smoke", delegate
		{
			if ((Object)(object)Player.m_localPlayer == (Object)null)
			{
				return false;
			}
			RemoveObj(Object.FindObjectsOfType(typeof(Fire)));
			RemoveObj(Object.FindObjectsOfType(typeof(Smoke)));
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("stopsmoke", "Puts out all spreading fires", delegate
		{
			if ((Object)(object)Player.m_localPlayer == (Object)null)
			{
				return false;
			}
			Object[] array5 = Object.FindObjectsOfType(typeof(Fire));
			for (int l = 0; l < array5.Length; l++)
			{
				Fire fire = (Fire)(object)array5[l];
				Destructible component = ((Component)fire).GetComponent<Destructible>();
				ZNetView component2 = ((Component)fire).GetComponent<ZNetView>();
				if ((Object)(object)component != (Object)null)
				{
					component.DestroyNow();
				}
				else if ((Object)(object)component2 != (Object)null && Object.op_Implicit((Object)(object)ZNetScene.instance))
				{
					ZNetScene.instance.Destroy(((Component)fire).gameObject);
				}
			}
			return true;
		}, isCheat: true, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("printseeds", "print seeds of loaded dungeons", delegate(ConsoleEventArgs args)
		{
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Player.m_localPlayer == (Object)null)
			{
				return false;
			}
			Math.Min(20f, args.TryParameterFloat(1, 5f));
			Object[] array3 = Object.FindObjectsOfType(typeof(DungeonGenerator));
			args.Context.AddString(string.Format("{0} version {1}, world seed: {2}/{3}", (Object.op_Implicit((Object)(object)ZNet.instance) && ZNet.instance.IsServer()) ? "Server" : "Client", Version.GetVersionString(), ZNet.World.m_seed, ZNet.World.m_seedName));
			Object[] array4 = array3;
			for (int k = 0; k < array4.Length; k++)
			{
				DungeonGenerator dungeonGenerator = (DungeonGenerator)(object)array4[k];
				args.Context.AddString(string.Format("  {0}: Seed: {1}/{2}, Distance: {3}", ((Object)dungeonGenerator).name, dungeonGenerator.m_generatedSeed, dungeonGenerator.GetSeed(), Utils.DistanceXZ(((Component)Player.m_localPlayer).transform.position, ((Component)dungeonGenerator).transform.position).ToString("0.0")));
			}
			return true;
		});
		new ConsoleCommand("nomap", "disables map for this character. If used as host, will disable for all joining players from now on.", delegate(ConsoleEventArgs args)
		{
			if ((Object)(object)Player.m_localPlayer != (Object)null)
			{
				string text = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
				bool flag = PlatformPrefs.GetFloat(text, 1f) == 1f;
				PlatformPrefs.SetFloat(text, (float)((!flag) ? 1 : 0));
				Minimap.instance.SetMapMode(Minimap.MapMode.None);
				args.Context?.AddString("Map " + (flag ? "disabled" : "enabled"));
				if (Object.op_Implicit((Object)(object)ZNet.instance) && ZNet.instance.IsServer())
				{
					if (flag)
					{
						ZoneSystem.instance.SetGlobalKey(GlobalKeys.NoMap);
					}
					else
					{
						ZoneSystem.instance.RemoveGlobalKey(GlobalKeys.NoMap);
					}
				}
			}
		});
		new ConsoleCommand("noportals", "disables portals for server.", delegate(ConsoleEventArgs args)
		{
			if ((Object)(object)Player.m_localPlayer != (Object)null)
			{
				bool globalKey = ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoPortals);
				if (globalKey)
				{
					ZoneSystem.instance.RemoveGlobalKey(GlobalKeys.NoPortals);
				}
				else
				{
					ZoneSystem.instance.SetGlobalKey(GlobalKeys.NoPortals);
				}
				args.Context?.AddString("Portals " + (globalKey ? "enabled" : "disabled"));
			}
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: false, allowInDevBuild: false, null, alwaysRefreshTabOptions: false, remoteCommand: false, onlyAdmin: true);
		new ConsoleCommand("resetspawn", "resets spawn location", delegate(ConsoleEventArgs args)
		{
			if (!Object.op_Implicit((Object)(object)Game.instance))
			{
				return false;
			}
			Game.instance.GetPlayerProfile()?.ClearCustomSpawnPoint();
			args.Context?.AddString("Reseting spawn point");
			return true;
		});
		new ConsoleCommand("respawntime", "sets respawntime", delegate(ConsoleEventArgs args)
		{
			if (!Object.op_Implicit((Object)(object)Game.instance))
			{
				return false;
			}
			if (args.TryParameterFloat(1, out var value3))
			{
				Game.instance.m_respawnLoadDuration = (Game.instance.m_fadeTimeDeath = value3);
			}
			return true;
		}, isCheat: true);
		new ConsoleCommand("die", "kill yourself", delegate
		{
			if (!Object.op_Implicit((Object)(object)Player.m_localPlayer))
			{
				return false;
			}
			HitData hit = new HitData
			{
				m_damage = 
				{
					m_damage = 99999f
				},
				m_hitType = HitData.HitType.Self
			};
			Player.m_localPlayer.Damage(hit);
			return true;
		});
		new ConsoleCommand("say", "chat message", delegate(ConsoleEventArgs args)
		{
			if (args.FullLine.Length < 5 || (Object)(object)Chat.instance == (Object)null)
			{
				return false;
			}
			Chat.instance.SendText(Talker.Type.Normal, args.FullLine.Substring(4));
			return true;
		});
		new ConsoleCommand("s", "shout message", delegate(ConsoleEventArgs args)
		{
			if (args.FullLine.Length < 3 || (Object)(object)Chat.instance == (Object)null)
			{
				return false;
			}
			Chat.instance.SendText(Talker.Type.Shout, args.FullLine.Substring(2));
			return true;
		});
		new ConsoleCommand("w", "[playername] whispers a private message to a player", delegate(ConsoleEventArgs args)
		{
			if (args.FullLine.Length < 3 || (Object)(object)Chat.instance == (Object)null)
			{
				return false;
			}
			Chat.instance.SendText(Talker.Type.Whisper, args.FullLine.Substring(2));
			return true;
		});
		new ConsoleCommand("resetplayerprefs", "Resets any saved settings and variables (not the save game)", delegate(ConsoleEventArgs args)
		{
			PlatformPrefs.DeleteAll();
			args.Context?.AddString("Reset saved player preferences");
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: true, allowInDevBuild: true);
		for (int i = 0; i < 25; i++)
		{
			Emotes emote = (Emotes)i;
			new ConsoleCommand(emote.GetCommandName(), $"emote: {emote}", delegate
			{
				Emote.DoEmote(emote);
			});
		}
		new ConsoleCommand("resetplayerprefs", "Resets any saved settings and variables (not the save game)", delegate(ConsoleEventArgs args)
		{
			PlatformPrefs.DeleteAll();
			args.Context?.AddString("Reset saved player preferences");
		}, isCheat: false, isNetwork: false, onlyServer: false, isSecret: true, allowInDevBuild: true);
		static bool ShouldKillAll(Character c)
		{
			if (c.IsPlayer() || (Object)(object)((Component)c).GetComponent<Piece>() != (Object)null)
			{
				return false;
			}
			if (c.IsTamed())
			{
				return false;
			}
			return true;
		}
		void count(string key, int level, int increment = 1)
		{
			if (!P_3.counts.TryGetValue(key, out var value))
			{
				value = (P_3.counts[key] = new Dictionary<int, int>());
			}
			if (value.TryGetValue(level, out var value2))
			{
				value[level] = value2 + increment;
			}
			else
			{
				value[level] = increment;
			}
		}
		static List<Tuple<object, Vector3>> find(string q)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			new Dictionary<string, Dictionary<int, int>>();
			GameObject[] array = Object.FindObjectsOfType<GameObject>();
			List<Tuple<object, Vector3>> list2 = new List<Tuple<object, Vector3>>();
			GameObject[] array2 = array;
			foreach (GameObject val in array2)
			{
				if (((Object)val).name.ToLower().Contains(q))
				{
					list2.Add(new Tuple<object, Vector3>(val, val.transform.position));
				}
			}
			foreach (ZoneSystem.LocationInstance location3 in ZoneSystem.instance.GetLocationList())
			{
				if (location3.m_location.m_prefab.Name.ToLower().Contains(q))
				{
					list2.Add(new Tuple<object, Vector3>(location3, location3.m_position));
				}
			}
			List<ZDO> list3 = new List<ZDO>();
			int index = 0;
			while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(q, list3, ref index))
			{
			}
			foreach (ZDO item21 in list3)
			{
				list2.Add(new Tuple<object, Vector3>(item21, item21.GetPosition()));
			}
			return list2;
		}
		static List<string> findOpt()
		{
			if (!Object.op_Implicit((Object)(object)ZNetScene.instance))
			{
				return null;
			}
			List<string> list = new List<string>(ZNetScene.instance.GetPrefabNames());
			foreach (ZoneSystem.ZoneLocation location4 in ZoneSystem.instance.m_locations)
			{
				if (location4.m_enable || location4.m_prefab.IsValid)
				{
					list.Add(location4.m_prefab.Name);
				}
			}
			return list;
		}
	}

	private static void RemoveObj(Object[] objs)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		for (int i = 0; i < objs.Length; i++)
		{
			MonoBehaviour val = (MonoBehaviour)objs[i];
			Destructible component = ((Component)val).GetComponent<Destructible>();
			ZNetView component2 = ((Component)val).GetComponent<ZNetView>();
			if ((Object)(object)component != (Object)null)
			{
				component.DestroyNow();
			}
			else if ((Object)(object)component2 != (Object)null && Object.op_Implicit((Object)(object)ZNetScene.instance))
			{
				ZNetScene.instance.Destroy(((Component)val).gameObject);
			}
			else
			{
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
		}
	}

	private static void AddConsoleCheatCommands()
	{
		new ConsoleCommand("xb:version", "Prints mercurial hashset used for this build", delegate(ConsoleEventArgs args)
		{
			args.Context?.AddString("Buildhash: " + Version.GetVersionString(includeMercurialHash: true));
		});
	}

	protected static void updateBinds()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		m_binds.Clear();
		foreach (string item in new List<string>(m_bindList))
		{
			string[] array = item.Split(' ');
			string text = string.Join(" ", array, 1, array.Length - 1);
			if (Enum.TryParse<KeyCode>(array[0], ignoreCase: true, out KeyCode result))
			{
				List<string> value;
				if (!ZInput.IsKeyCodeValid(result))
				{
					Debug.LogWarning((object)$"Removing previously bound command \"{text}\" as \"{result}\" is no longer a valid key code.");
					m_bindList.Remove(item);
				}
				else if (m_binds.TryGetValue(result, out value))
				{
					value.Add(text);
				}
				else
				{
					m_binds[result] = new List<string> { text };
				}
			}
		}
		PlatformPrefs.SetString("ConsoleBindings", string.Join("\n", m_bindList));
	}

	private void updateCommandList()
	{
		m_commandList.Clear();
		foreach (KeyValuePair<string, ConsoleCommand> command in commands)
		{
			if (command.Value.ShowCommand(this) && (m_autoCompleteSecrets || !command.Value.IsSecret))
			{
				m_commandList.Add(command.Key);
			}
		}
	}

	public bool IsCheatsEnabled()
	{
		if (m_cheat)
		{
			if (Object.op_Implicit((Object)(object)ZNet.instance))
			{
				return ZNet.instance.IsServer();
			}
			return false;
		}
		return false;
	}

	public void TryRunCommand(string text, bool silentFail = false, bool skipAllowedCheck = false)
	{
		string[] array = text.Split(' ');
		if (commands.TryGetValue(array[0].ToLower(), out var value))
		{
			if (value.IsValid(this, skipAllowedCheck))
			{
				value.RunAction(new ConsoleEventArgs(text, this));
			}
			else if (value.RemoteCommand && Object.op_Implicit((Object)(object)ZNet.instance) && !ZNet.instance.IsServer())
			{
				ZNet.instance.RemoteCommand(text);
			}
			else if (!silentFail)
			{
				AddString("'" + text.Split(' ')[0] + "' is not valid in the current context.");
			}
		}
		else if (!silentFail)
		{
			AddString("'" + array[0] + "' is not a recognized command. Type 'help' to see a list of valid commands.");
		}
	}

	public virtual void Awake()
	{
		InitTerminal();
	}

	public virtual void Update()
	{
		if (m_focused)
		{
			UpdateInput();
		}
	}

	private void UpdateInput()
	{
		if (ZInput.GetButton("JoyButtonX"))
		{
			if (ZInput.GetButtonDown("JoyDPadLeft"))
			{
				m_quickSelect[0] = ((TMP_InputField)m_input).text;
				PlatformPrefs.SetString("quick_save_left", m_quickSelect[0]);
				PlatformPrefs.Save();
			}
			if (ZInput.GetButtonDown("JoyDPadRight"))
			{
				m_quickSelect[1] = ((TMP_InputField)m_input).text;
				PlatformPrefs.SetString("quick_save_right", m_quickSelect[1]);
				PlatformPrefs.Save();
			}
			if (ZInput.GetButtonDown("JoyDPadUp"))
			{
				m_quickSelect[2] = ((TMP_InputField)m_input).text;
				PlatformPrefs.SetString("quick_save_up", m_quickSelect[2]);
				PlatformPrefs.Save();
			}
			if (ZInput.GetButtonDown("JoyDPadDown"))
			{
				m_quickSelect[3] = ((TMP_InputField)m_input).text;
				PlatformPrefs.SetString("quick_save_down", m_quickSelect[3]);
				PlatformPrefs.Save();
			}
		}
		else if (ZInput.GetButton("JoyButtonY"))
		{
			if (ZInput.GetButtonDown("JoyDPadLeft"))
			{
				((TMP_InputField)m_input).text = m_quickSelect[0];
				((TMP_InputField)m_input).caretPosition = ((TMP_InputField)m_input).text.Length;
			}
			if (ZInput.GetButtonDown("JoyDPadRight"))
			{
				((TMP_InputField)m_input).text = m_quickSelect[1];
				((TMP_InputField)m_input).caretPosition = ((TMP_InputField)m_input).text.Length;
			}
			if (ZInput.GetButtonDown("JoyDPadUp"))
			{
				((TMP_InputField)m_input).caretPosition = ((TMP_InputField)m_input).text.Length;
				((TMP_InputField)m_input).text = m_quickSelect[2];
			}
			if (ZInput.GetButtonDown("JoyDPadDown"))
			{
				((TMP_InputField)m_input).caretPosition = ((TMP_InputField)m_input).text.Length;
				((TMP_InputField)m_input).text = m_quickSelect[3];
			}
		}
		else if ((ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("JoyDPadUp")) && !m_input.IsCompositionActive())
		{
			if (m_historyPosition > 0)
			{
				m_historyPosition--;
			}
			((TMP_InputField)m_input).text = ((m_history.Count > 0) ? m_history[m_historyPosition] : "");
			((TMP_InputField)m_input).caretPosition = ((TMP_InputField)m_input).text.Length;
		}
		else if ((ZInput.GetButtonDown("ChatDown") || ZInput.GetButtonDown("JoyDPadDown")) && !m_input.IsCompositionActive())
		{
			if (m_historyPosition < m_history.Count)
			{
				m_historyPosition++;
			}
			((TMP_InputField)m_input).text = ((m_historyPosition < m_history.Count) ? m_history[m_historyPosition] : "");
			((TMP_InputField)m_input).caretPosition = ((TMP_InputField)m_input).text.Length;
		}
		else if (ZInput.GetKeyDown((KeyCode)9, true) || ZInput.GetButtonDown("JoyDPadRight"))
		{
			if (m_commandList.Count == 0)
			{
				updateCommandList();
			}
			string[] array = ((TMP_InputField)m_input).text.Split(' ');
			if (array.Length == 1)
			{
				tabCycle(array[0], m_commandList, usePrefix: true);
			}
			else
			{
				string key = ((m_tabPrefix == '\0') ? array[0] : array[0].Substring(1));
				if (commands.TryGetValue(key, out var value))
				{
					tabCycle(array[1], value.GetTabOptions(), usePrefix: false);
				}
			}
		}
		if ((ZInput.GetButtonDown("ScrollChatUp") || ZInput.GetButtonDown("JoyScrollChatUp")) && m_scrollHeight < m_chatBuffer.Count - 5)
		{
			m_scrollHeight++;
			UpdateChat();
		}
		if ((ZInput.GetButtonDown("ScrollChatDown") || ZInput.GetButtonDown("JoyScrollChatDown")) && m_scrollHeight > 0)
		{
			m_scrollHeight--;
			UpdateChat();
		}
		if (((TMP_InputField)m_input).caretPosition != m_tabCaretPositionEnd)
		{
			m_tabCaretPosition = -1;
		}
		if (m_lastSearchLength == ((TMP_InputField)m_input).text.Length)
		{
			return;
		}
		m_lastSearchLength = ((TMP_InputField)m_input).text.Length;
		if (m_commandList.Count == 0)
		{
			updateCommandList();
		}
		string[] array2 = ((TMP_InputField)m_input).text.Split(' ');
		if (array2.Length == 1)
		{
			updateSearch(array2[0], m_commandList, usePrefix: true);
			return;
		}
		string key2 = ((m_tabPrefix == '\0') ? array2[0] : ((array2[0].Length == 0) ? "" : array2[0].Substring(1)));
		if (commands.TryGetValue(key2, out var value2))
		{
			updateSearch(array2[1], value2.GetTabOptions(), usePrefix: false);
		}
	}

	protected void SendInput()
	{
		if (!string.IsNullOrEmpty(((TMP_InputField)m_input).text))
		{
			InputText();
			if (m_history.Count == 0 || m_history[m_history.Count - 1] != ((TMP_InputField)m_input).text)
			{
				m_history.Add(((TMP_InputField)m_input).text);
			}
			m_historyPosition = m_history.Count;
			((TMP_InputField)m_input).text = "";
			m_scrollHeight = 0;
			UpdateChat();
			if (!Application.isConsolePlatform && !Application.isMobilePlatform)
			{
				m_input.ActivateInputField();
			}
		}
	}

	protected virtual void InputText()
	{
		string text = ((TMP_InputField)m_input).text;
		AddString(text);
		TryRunCommand(text);
	}

	protected virtual bool isAllowedCommand(ConsoleCommand cmd)
	{
		return true;
	}

	public void AddString(PlatformUserID user, string text, Talker.Type type, bool timestamp = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		Color val = Color.white;
		switch (type)
		{
		case Talker.Type.Shout:
			val = Color.yellow;
			text = text.ToUpper();
			break;
		case Talker.Type.Whisper:
			((Color)(ref val))._002Ector(1f, 1f, 1f, 0.75f);
			text = text.ToLowerInvariant();
			break;
		default:
			val = Color.white;
			break;
		}
		if (!ZNet.TryGetPlayerByPlatformUserID(user, out var playerInfo))
		{
			ZLog.LogError((object)$"Failed to get player info for player {user} who sent the chat message \"{text}\"!");
			return;
		}
		string text2 = CensorShittyWords.FilterUGC(playerInfo.m_name, UGCType.CharacterName, user, 0L);
		if (PlatformManager.DistributionPlatform.Platform == "Xbox")
		{
			IRelationsProvider relationsProvider = PlatformManager.DistributionPlatform.RelationsProvider;
			if (relationsProvider == null)
			{
				ZLog.LogError((object)$"Relations provider was unavailable when user {text2} ({user}) sent the chat message \"{text}\"! This should never happen!");
			}
			IUserProfile val2 = default(IUserProfile);
			string displayName;
			if (relationsProvider != null && PlatformManager.DistributionPlatform.Platform == user.m_platform && relationsProvider.TryGetUserProfile(user, ref val2) && ((IUser)val2).DisplayName != null)
			{
				if (((IUser)val2).DisplayName.Length > 0)
				{
					text2 = text2 + " [" + ((IUser)val2).DisplayName + "]";
				}
			}
			else if (ZNet.TryGetServerAssignedDisplayName(user, out displayName))
			{
				if (displayName.Length > 0)
				{
					text2 = text2 + " [" + displayName + "]";
				}
			}
			else
			{
				ZLog.LogWarning((object)$"Failed to get display name for player {text2} ({user}) who sent the chat message \"{text}\"!");
			}
		}
		string text3 = (timestamp ? ("[" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "] ") : "");
		text3 = text3 + "<color=orange>" + text2 + "</color>: <color=#" + ColorUtility.ToHtmlStringRGBA(val) + ">" + text + "</color>";
		AddString(text3);
	}

	public void AddString(string title, string text, Talker.Type type, bool timestamp = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Color val = Color.white;
		switch (type)
		{
		case Talker.Type.Shout:
			val = Color.yellow;
			text = text.ToUpper();
			break;
		case Talker.Type.Whisper:
			((Color)(ref val))._002Ector(1f, 1f, 1f, 0.75f);
			text = text.ToLowerInvariant();
			break;
		default:
			val = Color.white;
			break;
		}
		string text2 = (timestamp ? ("[" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "] ") : "");
		text2 = text2 + "<color=orange>" + title + "</color>: <color=#" + ColorUtility.ToHtmlStringRGBA(val) + ">" + text + "</color>";
		AddString(text2);
	}

	public void AddString(string text)
	{
		while (m_maxVisibleBufferLength > 1)
		{
			try
			{
				m_chatBuffer.Add(text);
				while (m_chatBuffer.Count > 300)
				{
					m_chatBuffer.RemoveAt(0);
				}
				UpdateChat();
				break;
			}
			catch (Exception)
			{
				m_maxVisibleBufferLength--;
			}
		}
	}

	public void UpdateDisplayName(string oldName, string newName)
	{
		if (string.IsNullOrEmpty(oldName))
		{
			Debug.LogError((object)("Failed to update display to \"" + newName + "\"! oldName was " + ((oldName == null) ? "null" : "empty") + " "));
		}
		else
		{
			for (int i = 0; i < m_chatBuffer.Count; i++)
			{
				m_chatBuffer[i] = m_chatBuffer[i].Replace(oldName, newName);
			}
			UpdateChat();
		}
	}

	private void UpdateChat()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = Mathf.Min(m_chatBuffer.Count, Mathf.Max(5, m_chatBuffer.Count - m_scrollHeight));
		for (int i = Mathf.Max(0, num - m_maxVisibleBufferLength); i < num; i++)
		{
			stringBuilder.Append(m_chatBuffer[i]);
			stringBuilder.Append("\n");
		}
		((TMP_Text)m_output).text = stringBuilder.ToString();
	}

	public static float GetTestValue(string key, float defaultIfMissing = 0f)
	{
		if (m_testList.TryGetValue(key, out var value) && float.TryParse(value, out var result))
		{
			return result;
		}
		return defaultIfMissing;
	}

	private void tabCycle(string word, List<string> options, bool usePrefix)
	{
		if (options == null || options.Count == 0)
		{
			return;
		}
		usePrefix = usePrefix && m_tabPrefix != '\0';
		if (usePrefix)
		{
			if (word.Length < 1 || word[0] != m_tabPrefix)
			{
				return;
			}
			word = word.Substring(1);
		}
		if (m_tabCaretPosition == -1)
		{
			m_tabOptions.Clear();
			m_tabCaretPosition = ((TMP_InputField)m_input).caretPosition;
			word = word.ToLower();
			m_tabLength = word.Length;
			if (m_tabLength == 0)
			{
				m_tabOptions.AddRange(options);
			}
			else
			{
				foreach (string option in options)
				{
					if (option != null && option.Length > m_tabLength && safeSubstring(option, 0, m_tabLength).ToLower() == word)
					{
						m_tabOptions.Add(option);
					}
				}
			}
			m_tabOptions.Sort();
			m_tabIndex = -1;
		}
		if (m_tabOptions.Count == 0)
		{
			m_tabOptions.AddRange(m_lastSearch);
		}
		if (m_tabOptions.Count != 0)
		{
			if (++m_tabIndex >= m_tabOptions.Count)
			{
				m_tabIndex = 0;
			}
			if (m_tabCaretPosition - m_tabLength >= 0)
			{
				((TMP_InputField)m_input).text = safeSubstring(((TMP_InputField)m_input).text, 0, m_tabCaretPosition - m_tabLength) + m_tabOptions[m_tabIndex];
			}
			int tabCaretPositionEnd = (((TMP_InputField)m_input).caretPosition = ((TMP_InputField)m_input).text.Length);
			m_tabCaretPositionEnd = tabCaretPositionEnd;
		}
	}

	private void updateSearch(string word, List<string> options, bool usePrefix)
	{
		if ((Object)(object)m_search == (Object)null)
		{
			return;
		}
		m_search.text = "";
		if (options == null || options.Count == 0)
		{
			return;
		}
		usePrefix = usePrefix && m_tabPrefix != '\0';
		if (usePrefix)
		{
			if (word.Length < 1 || word[0] != m_tabPrefix)
			{
				return;
			}
			word = word.Substring(1);
		}
		m_lastSearch.Clear();
		foreach (string option in options)
		{
			if (option != null)
			{
				string text = option.ToLower();
				if (text.Contains(word.ToLower()) && (word.Contains("fx") || !text.Contains("fx")))
				{
					m_lastSearch.Add(option);
				}
			}
		}
		int num = 10;
		for (int i = 0; i < Math.Min(m_lastSearch.Count, num); i++)
		{
			string text2 = m_lastSearch[i];
			int num2 = text2.ToLower().IndexOf(word.ToLower());
			TMP_Text search = m_search;
			search.text += safeSubstring(text2, 0, num2);
			TMP_Text search2 = m_search;
			search2.text = search2.text + "<color=white>" + safeSubstring(text2, num2, word.Length) + "</color>";
			TMP_Text search3 = m_search;
			search3.text = search3.text + safeSubstring(text2, num2 + word.Length) + " ";
		}
		if (m_lastSearch.Count > num)
		{
			TMP_Text search4 = m_search;
			search4.text += $"... {m_lastSearch.Count - num} more.";
		}
	}

	private string safeSubstring(string text, int start, int length = -1)
	{
		if (text.Length == 0)
		{
			return text;
		}
		if (start < 0)
		{
			start = 0;
		}
		if (start + length >= text.Length)
		{
			length = text.Length - start;
		}
		if (length >= 0)
		{
			return text.Substring(start, length);
		}
		return text.Substring(start);
	}

	protected void LoadQuickSelect()
	{
		m_quickSelect[0] = PlatformPrefs.GetString("quick_save_left", "");
		m_quickSelect[1] = PlatformPrefs.GetString("quick_save_right", "");
		m_quickSelect[2] = PlatformPrefs.GetString("quick_save_up", "");
		m_quickSelect[3] = PlatformPrefs.GetString("quick_save_down", "");
	}

	public static float TryTestFloat(string key, float defaultValue = 1f)
	{
		if (m_testList.TryGetValue(key, out var value) && float.TryParse(value, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static int TryTestInt(string key, int defaultValue = 1)
	{
		if (m_testList.TryGetValue(key, out var value) && int.TryParse(value, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static string TryTest(string key, string defaultValue = "")
	{
		if (m_testList.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public static int Increment(string key, int by = 1)
	{
		if (m_testList.TryGetValue(key, out var value))
		{
			m_testList[key] = (int.Parse(value) + by).ToString();
		}
		else
		{
			m_testList[key] = by.ToString();
		}
		return int.Parse(m_testList[key]);
	}

	public static void Log(object obj)
	{
		if (m_showTests)
		{
			ZLog.Log(obj);
			if (Object.op_Implicit((Object)(object)Console.instance))
			{
				Console.instance.AddString("Log", obj.ToString(), Talker.Type.Whisper, timestamp: true);
			}
		}
	}

	public static void LogWarning(object obj)
	{
		if (m_showTests)
		{
			ZLog.LogWarning(obj);
			if (Object.op_Implicit((Object)(object)Console.instance))
			{
				Console.instance.AddString("Warning", obj.ToString(), Talker.Type.Whisper, timestamp: true);
			}
		}
	}

	public static void LogError(object obj)
	{
		if (m_showTests)
		{
			ZLog.LogError(obj);
			if (Object.op_Implicit((Object)(object)Console.instance))
			{
				Console.instance.AddString("Warning", obj.ToString(), Talker.Type.Whisper, timestamp: true);
			}
		}
	}
}
