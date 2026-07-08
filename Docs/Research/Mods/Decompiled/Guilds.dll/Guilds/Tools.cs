using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Guilds;

public static class Tools
{
	internal class NameValidator : TMP_InputValidator
	{
		public override char Validate(ref string text, ref int pos, char ch)
		{
			if (ValidateChar(ch))
			{
				text = text.Insert(pos++, ch.ToString());
				return ch;
			}
			return '\0';
		}
	}

	internal static Texture2D loadTexture(string name)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_001f: Expected O, but got Unknown
		Texture2D val = new Texture2D(0, 0);
		ImageConversion.LoadImage(val, ReadEmbeddedFileBytes("Icons." + name));
		return val;
	}

	internal static Sprite loadSprite(string name, int width, int height)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return Sprite.Create(loadTexture(name), new Rect(0f, 0f, (float)width, (float)height), Vector2.zero);
	}

	public static byte[] ReadEmbeddedFileBytes(string name)
	{
		using MemoryStream memoryStream = new MemoryStream();
		Assembly.GetExecutingAssembly().GetManifestResourceStream("Guilds." + name)?.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	internal static AssetBundle LoadAssetBundle(string bundleName)
	{
		string bundleName2 = bundleName;
		string name = typeof(Guilds).Assembly.GetManifestResourceNames().Single((string s) => s.EndsWith(bundleName2));
		return AssetBundle.LoadFromStream(typeof(Guilds).Assembly.GetManifestResourceStream(name));
	}

	internal static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100f, SpriteMeshType spriteType = 1)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = LoadTexture(FilePath);
		return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0f, 0f), PixelsPerUnit, 0u, spriteType);
	}

	internal static Sprite ConvertTextureToSprite(Texture2D texture, float PixelsPerUnit = 100f, SpriteMeshType spriteType = 1)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		return Sprite.Create(texture, new Rect(0f, 0f, (float)((Texture)texture).width, (float)((Texture)texture).height), new Vector2(0f, 0f), PixelsPerUnit, 0u, spriteType);
	}

	private static Texture2D? LoadTexture(string FilePath)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if (File.Exists(FilePath))
		{
			byte[] array = File.ReadAllBytes(FilePath);
			Texture2D val = new Texture2D(2, 2);
			if (ImageConversion.LoadImage(val, array))
			{
				return val;
			}
		}
		return null;
	}

	internal static string GetHumanFriendlyTime(int seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		if (timeSpan.TotalSeconds < 60.0)
		{
			return Localization.instance.Localize("$guilds_less_than_minute");
		}
		string text = Localization.instance.Localize((timeSpan.Days >= 2) ? "$guilds_day_plural" : "$guilds_day_singular", new string[1] { ((int)timeSpan.TotalDays).ToString() });
		if (timeSpan.TotalDays >= 30.0)
		{
			return text;
		}
		string text2 = Localization.instance.Localize((timeSpan.Hours >= 2) ? "$guilds_hour_plural" : "$guilds_hour_singular", new string[1] { timeSpan.Hours.ToString() });
		if (timeSpan.TotalDays >= 1.0)
		{
			if (timeSpan.Hours != 0)
			{
				return Localization.instance.Localize("$guilds_bind_day_hour", new string[2] { text, text2 });
			}
			return text;
		}
		string text3 = Localization.instance.Localize((timeSpan.Minutes >= 2) ? "$guilds_minute_plural" : "$guilds_minute_singular", new string[1] { timeSpan.Minutes.ToString() });
		if (!(timeSpan.TotalHours >= 1.0))
		{
			return text3;
		}
		if (timeSpan.Minutes != 0)
		{
			return Localization.instance.Localize("$guilds_bind_hour_minute", new string[2] { text2, text3 });
		}
		return text2;
	}

	internal static bool ValidateChar(char ch)
	{
		if (!char.IsLetterOrDigit(ch))
		{
			return "@!#$%&'*+-=?^_{|}~ ".IndexOf(ch) != -1;
		}
		return true;
	}

	public static List<Player> GetNearbyGuildMembers(Player player, float range, bool includeSelf = false)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Player player2 = player;
		Guild playerGuild = API.GetPlayerGuild(player2);
		if (playerGuild == null)
		{
			return new List<Player>();
		}
		List<PlayerReference> guildPlayers = API.GetOnlinePlayers(playerGuild).ToList();
		List<Player> list = new List<Player>();
		Player.GetPlayersInRange(((Component)player2).transform.position, range, list);
		list.RemoveAll((Player p) => !guildPlayers.Contains(PlayerReference.fromPlayer(p)) || (!includeSelf && (Object)(object)p == (Object)(object)player2));
		return list;
	}
}
