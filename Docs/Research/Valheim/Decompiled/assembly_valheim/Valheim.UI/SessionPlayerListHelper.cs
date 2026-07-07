using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Valheim.UI;

public static class SessionPlayerListHelper
{
	public static IEnumerator SetSpriteFromUri(this Image image, string uri)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);
		yield return www.SendWebRequest();
		if ((int)www.result != 1)
		{
			Debug.Log((object)www.error);
			yield break;
		}
		Texture2D content = DownloadHandlerTexture.GetContent(www);
		image.sprite = Sprite.Create(content, new Rect(0f, 0f, (float)((Texture)content).width, (float)((Texture)content).height), new Vector2(0.5f, 0.5f));
		((Component)image).transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public static void SetSpriteFromTexture(this Image image, Texture2D texture)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		image.sprite = Sprite.Create(texture, new Rect(0f, 0f, (float)((Texture)texture).width, (float)((Texture)texture).height), new Vector2(0.5f, 0.5f));
		((Component)image).transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public static void SetSpriteFromSteamImageId(this Image image, int imageId)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		uint num = default(uint);
		uint num2 = default(uint);
		if (imageId <= 0)
		{
			image.SetTransparent();
		}
		else if (SteamUtils.GetImageSize(imageId, ref num, ref num2))
		{
			uint num3 = num * num2 * 4;
			byte[] array = new byte[num3];
			Texture2D val = new Texture2D((int)num, (int)num2, (TextureFormat)4, false, true);
			if (SteamUtils.GetImageRGBA(imageId, array, (int)num3))
			{
				val.LoadRawTextureData(array);
				val.FlipInYDirection();
				image.sprite = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f));
			}
		}
	}

	private static void SetTransparent(this Image image)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(1, 1, (TextureFormat)4, false);
		val.SetPixels((Color[])(object)new Color[1]
		{
			new Color(0f, 0f, 0f, 0f)
		});
		image.sprite = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f));
	}

	private static void FlipInYDirection(this Texture2D texture)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Color[] pixels = texture.GetPixels();
		Color[] array = (Color[])(object)new Color[pixels.Length];
		int num = 0;
		for (int num2 = ((Texture)texture).height - 1; num2 >= 0; num2--)
		{
			for (int i = 0; i < ((Texture)texture).width; i++)
			{
				array[num] = pixels[num2 * ((Texture)texture).height + i];
				num++;
			}
		}
		texture.SetPixels(array);
		texture.Apply();
	}

	public static bool TryFindPlayerByZDOID(this List<ZNet.PlayerInfo> players, ZDOID playerID, out ZNet.PlayerInfo? playerInfo)
	{
		playerInfo = null;
		for (int i = 0; i < players.Count; i++)
		{
			ZNet.PlayerInfo value = players[i];
			if (value.m_characterID == playerID)
			{
				playerInfo = value;
				return true;
			}
		}
		return false;
	}

	public static bool TryFindPlayerByPlayername(this List<ZNet.PlayerInfo> players, string name, out ZNet.PlayerInfo? playerInfo)
	{
		playerInfo = null;
		for (int i = 0; i < players.Count; i++)
		{
			ZNet.PlayerInfo value = players[i];
			if (value.m_name == name)
			{
				playerInfo = value;
				return true;
			}
		}
		return false;
	}

	public static bool IsBanned(string characterName)
	{
		return ZNet.instance.Banned.Contains(characterName);
	}
}
