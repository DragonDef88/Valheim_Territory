using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;

namespace Guilds;

public static class Interface
{
	internal static GameObject NoGuildUIPrefab = null;

	internal static GameObject SearchGuildUIPrefab = null;

	internal static GameObject CreateGuildUIPrefab = null;

	internal static GameObject GuildManagementUIPrefab = null;

	internal static GameObject ApplicationsUIPrefab = null;

	internal static GameObject EditGuildUIPrefab = null;

	internal static GameObject AchievementUIPrefab = null;

	internal static GameObject AchievementPopupPrefab = null;

	internal static GameObject NoGuildUI = null;

	internal static GameObject SearchGuildUI = null;

	internal static GameObject CreateGuildUI = null;

	internal static GameObject GuildManagementUI = null;

	internal static GameObject ApplicationsUI = null;

	internal static GameObject EditGuildUI = null;

	internal static GameObject AchievementUI = null;

	internal static GameObject AchievementPopup = null;

	internal static readonly Dictionary<int, Sprite> GuildIcons = new Dictionary<int, Sprite>();

	internal static readonly Dictionary<string, Sprite> AchievementIcons = new Dictionary<string, Sprite>();

	internal static void LoadAssets()
	{
		AssetBundle obj = Tools.LoadAssetBundle("guildsbundle");
		NoGuildUIPrefab = obj.LoadAsset<GameObject>("NoGuild");
		CreateGuildUIPrefab = obj.LoadAsset<GameObject>("CreateGuild");
		SearchGuildUIPrefab = obj.LoadAsset<GameObject>("SearchGuild");
		GuildManagementUIPrefab = obj.LoadAsset<GameObject>("GuildManagementUI");
		ApplicationsUIPrefab = obj.LoadAsset<GameObject>("ApplicationsUI");
		EditGuildUIPrefab = obj.LoadAsset<GameObject>("EditGuild");
		AchievementUIPrefab = obj.LoadAsset<GameObject>("AchievementUI");
		AchievementPopupPrefab = obj.LoadAsset<GameObject>("AchievementPopup");
		obj.Unload(false);
		string[] manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (text.StartsWith("Guilds.Icons.Badges", StringComparison.Ordinal))
			{
				string[] array = text.Split(new char[1] { '.' });
				GuildIcons.Add(int.Parse(array[^2]), Tools.loadSprite(text.Replace("Guilds.Icons.", ""), 128, 128));
			}
			else if (text.StartsWith("Guilds.Icons.Achievements", StringComparison.Ordinal))
			{
				AchievementIcons[text.Replace("Guilds.Icons.Achievements.", "")] = Tools.loadSprite(text.Replace("Guilds.Icons.", ""), 128, 128);
			}
		}
	}

	private static AssetBundle LoadAssetBundle(string bundleName)
	{
		string bundleName2 = bundleName;
		string name = typeof(Guilds).Assembly.GetManifestResourceNames().Single((string s) => s.EndsWith(bundleName2));
		return AssetBundle.LoadFromStream(typeof(Guilds).Assembly.GetManifestResourceStream(name));
	}

	internal static void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Patches.PreventMainMenu.AllowMainMenu = true;
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return;
		}
		if (((Character)localPlayer).TakeInput())
		{
			KeyboardShortcut value = Guilds.guildInterfaceKey.Value;
			if (((KeyboardShortcut)(ref value)).IsDown())
			{
				if (API.GetOwnGuild() == null)
				{
					NoGuildUI.SetActive(true);
				}
				else
				{
					GuildManagementUI.SetActive(true);
				}
			}
		}
		if (UIIsActive() && Input.GetKey((KeyCode)27))
		{
			HideUI();
			Patches.PreventMainMenu.AllowMainMenu = false;
		}
	}

	internal static bool UIIsActive()
	{
		if ((!Object.op_Implicit((Object)(object)NoGuildUI) || !NoGuildUI.activeSelf) && (!Object.op_Implicit((Object)(object)CreateGuildUI) || !CreateGuildUI.activeSelf) && (!Object.op_Implicit((Object)(object)SearchGuildUI) || !SearchGuildUI.activeSelf) && (!Object.op_Implicit((Object)(object)GuildManagementUI) || !GuildManagementUI.activeSelf) && (!Object.op_Implicit((Object)(object)ApplicationsUI) || !ApplicationsUI.activeSelf) && (!Object.op_Implicit((Object)(object)EditGuildUI) || !EditGuildUI.activeSelf))
		{
			if (Object.op_Implicit((Object)(object)AchievementUI))
			{
				return AchievementUI.activeSelf;
			}
			return false;
		}
		return true;
	}

	internal static void HideUI()
	{
		NoGuildUI.SetActive(false);
		SearchGuildUI.SetActive(false);
		CreateGuildUI.SetActive(false);
		GuildManagementUI.SetActive(false);
		ApplicationsUI.SetActive(false);
		EditGuildUI.SetActive(false);
		AchievementUI.SetActive(false);
	}

	internal static void SwitchUI(GameObject newUI, bool hideOld = true)
	{
		if (hideOld)
		{
			HideUI();
		}
		newUI.SetActive(true);
	}
}
