using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using APIManager;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LocalizationManager;
using ServerSync;
using UnityEngine;

namespace Guilds;

[BepInPlugin("org.bepinex.plugins.guilds", "Guilds", "1.1.13")]
[BepInIncompatibility("org.bepinex.plugins.valheim_plus")]
public class Guilds : BaseUnityPlugin
{
	internal enum GuildAchievements
	{
		Disabled,
		Default,
		External
	}

	[HarmonyPatch(typeof(FejdStartup), "Awake")]
	private static class ReadGuildList
	{
		private static bool first = true;

		private static void Postfix()
		{
			if (first)
			{
				first = false;
				string saveDataPath = Utils.GetSaveDataPath((FileSource)1);
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				GuildsPath = saveDataPath + directorySeparatorChar + "Guilds";
				GuildList.Init();
				GuildList.readGuildFiles();
				addFileWatchEvent(new FileSystemWatcher(GuildsPath, "*.yml"), delegate
				{
					GuildList.readGuildFiles();
				});
			}
		}
	}

	[HarmonyPatch(typeof(Game), "Start")]
	private class ShowPlayerMessage
	{
		private static void Postfix()
		{
			ZRoutedRpc.instance.Register<string>("Guilds PlayerMessage", (Action<long, string>)delegate(long _, string message)
			{
				MessageHud.instance.ShowMessage((MessageType)2, message, 0, (Sprite)null, false);
			});
		}
	}

	private const string ModName = "Guilds";

	private const string ModVersion = "1.1.13";

	private const string ModGUID = "org.bepinex.plugins.guilds";

	public static readonly ConfigSync configSync = new ConfigSync("Guilds")
	{
		DisplayName = "Guilds",
		CurrentVersion = "1.1.13",
		MinimumRequiredVersion = "1.1.13"
	};

	internal static Guilds self = null;

	public static string GuildsPath = null;

	private static ConfigEntry<Toggle> serverConfigLocked = null;

	internal static ConfigEntry<Toggle> friendlyFire = null;

	internal static ConfigEntry<KeyboardShortcut> guildInterfaceKey = null;

	internal static ConfigEntry<Toggle> displayGuildLevel = null;

	internal static ConfigEntry<Toggle> guildColors = null;

	internal static ConfigEntry<Color> guildChatColor = null;

	internal static ConfigEntry<int> minimumGuildNameLength = null;

	internal static ConfigEntry<int> maximumGuildNameLength = null;

	internal static ConfigEntry<uint> maximumGuildMembers = null;

	internal static ConfigEntry<Toggle> allowGuildCreation = null;

	internal static ConfigEntry<Toggle> allowGuildEdit = null;

	internal static ConfigEntry<KeyboardShortcut> guildPingHotkey = null;

	internal static ConfigEntry<GuildAchievements> guildAchievementConfig = null;

	private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
	{
		ConfigEntry<T> val = ((BaseUnityPlugin)this).Config.Bind<T>(group, name, value, description);
		configSync.AddConfigEntry<T>(val).SynchronizedConfig = synchronizedSetting;
		return val;
	}

	private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		return config(group, name, value, new ConfigDescription(description, (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting);
	}

	public void Awake()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Expected O, but got Unknown
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected O, but got Unknown
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Expected O, but got Unknown
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Expected O, but got Unknown
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		self = this;
		Patcher.Patch();
		Localizer.Load();
		serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.Off, new ConfigDescription("Locks the config and enforces the servers configuration.", (AcceptableValueBase)null, Array.Empty<object>()));
		configSync.AddLockingConfigEntry<Toggle>(serverConfigLocked);
		guildInterfaceKey = config<KeyboardShortcut>("1 - General", "Guild Interface Key", new KeyboardShortcut((KeyCode)111, Array.Empty<KeyCode>()), new ConfigDescription("Keyboard shortcut to press in order to display the guild interface.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		friendlyFire = config("1 - General", "Friendly fire in guilds", Toggle.Off, new ConfigDescription("If members from the same guild can damage each other in PvP.", (AcceptableValueBase)null, Array.Empty<object>()));
		displayGuildLevel = config("1 - General", "Display guild level on nameplate", Toggle.Off, new ConfigDescription("Displays the level of the guild after its name on nameplates.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		guildColors = config("1 - General", "Guild Colors", Toggle.On, new ConfigDescription("If off, the guild colors will be replaced with Valheims default colors instead.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		guildChatColor = config<Color>("1 - General", "Guild Chat Color", new Color(1f, 61f / 85f, 49f / 136f), new ConfigDescription("The color for messages in the guild chat.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		minimumGuildNameLength = config("1 - General", "Minimum Name Length", 2, new ConfigDescription("The minimum length of guild names as the number of characters.", (AcceptableValueBase)(object)new AcceptableValueRange<int>(1, 16), Array.Empty<object>()));
		minimumGuildNameLength.SettingChanged += delegate
		{
			if (minimumGuildNameLength.Value > maximumGuildNameLength.Value)
			{
				minimumGuildNameLength.Value = maximumGuildNameLength.Value;
			}
		};
		maximumGuildNameLength = config("1 - General", "Maximum Name Length", 32, new ConfigDescription("The maximum length of guild names as the number of characters.", (AcceptableValueBase)(object)new AcceptableValueRange<int>(2, 64), Array.Empty<object>()));
		maximumGuildNameLength.SettingChanged += delegate
		{
			if (maximumGuildNameLength.Value < minimumGuildNameLength.Value)
			{
				maximumGuildNameLength.Value = minimumGuildNameLength.Value;
			}
		};
		allowGuildCreation = config("1 - General", "Allow Guild Creation", Toggle.On, new ConfigDescription("If off, only admins can create new guilds.", (AcceptableValueBase)null, Array.Empty<object>()));
		allowGuildEdit = config("1 - General", "Allow Guild Edit", Toggle.On, new ConfigDescription("If off, only admins can edit guilds.", (AcceptableValueBase)null, Array.Empty<object>()));
		guildPingHotkey = config<KeyboardShortcut>("1 - General", "Guild Ping Modifier Key", new KeyboardShortcut((KeyCode)304, Array.Empty<KeyCode>()), new ConfigDescription("Modifier key that has to be pressed while pinging the map, to make the map ping visible to guild members only.", (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting: false);
		maximumGuildMembers = config("1 - General", "Maximum Guild Members", 0u, new ConfigDescription("Maximum number of guild members per guild. Set to 0 for no maximum.", (AcceptableValueBase)null, Array.Empty<object>()));
		guildAchievementConfig = config("2 - Achievements", "Ignore Internal Achievement Config", GuildAchievements.Default, new ConfigDescription("Disabled: Guild achievements are disabled and not available on your server.\nDefault: The internal guild achievement config is enabled and can be adjusted via an optional external AchievementConfig.yml file.\nExternal: The internal guild achievement configs are ignored and guild achievements are parsed from an external AchievementConfig.yml file only. This means that you have to keep track of newly added achievements yourself and add them to your config file, if you want to have them on your server.", (AcceptableValueBase)null, Array.Empty<object>()));
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		new Harmony("org.bepinex.plugins.guilds").PatchAll(executingAssembly);
		Interface.LoadAssets();
		Map.Init();
		Achievements.Init();
		((MonoBehaviour)this).InvokeRepeating("updatePositon", 0f, 2f);
	}

	public void Update()
	{
		Interface.Update();
	}

	internal static void addFileWatchEvent(FileSystemWatcher watcher, Action<object, EventArgs> handler)
	{
		watcher.Created += handler.Invoke;
		watcher.Changed += handler.Invoke;
		watcher.Renamed += handler.Invoke;
		watcher.Deleted += handler.Invoke;
		watcher.IncludeSubdirectories = true;
		watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
		watcher.EnableRaisingEvents = true;
	}

	public static void SendMessageToPlayer(PlayerReference player, string message)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		PlayerInfo val = ((IEnumerable<PlayerInfo>)ZNet.instance.m_players).FirstOrDefault((Func<PlayerInfo, bool>)((PlayerInfo p) => PlayerReference.fromPlayerInfo(p) == player));
		ZDOID characterID = val.m_characterID;
		if (((ZDOID)(ref characterID)).UserID != 0L)
		{
			ZRoutedRpc.instance.InvokeRoutedRPC(((ZDOID)(ref val.m_characterID)).UserID, "Guilds PlayerMessage", new object[1] { message });
		}
	}

	public static void SendMessageToAllPlayers(string message)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "Guilds PlayerMessage", new object[1] { message });
	}

	private void updatePositon()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return;
		}
		Guild ownGuild = API.GetOwnGuild();
		if (ownGuild == null || ZNet.instance.m_publicReferencePosition)
		{
			return;
		}
		foreach (PlayerInfo player in ZNet.instance.m_players)
		{
			if (ownGuild.Members.ContainsKey(PlayerReference.fromPlayerInfo(player)) && player.m_characterID != ((Character)localPlayer).GetZDOID())
			{
				ZRoutedRpc instance = ZRoutedRpc.instance;
				ZDOID characterID = player.m_characterID;
				instance.InvokeRoutedRPC(((ZDOID)(ref characterID)).UserID, "Guilds UpdatePosition", new object[1] { ((Component)localPlayer).transform.position });
			}
		}
	}
}
