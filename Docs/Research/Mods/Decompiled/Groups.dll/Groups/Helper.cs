using System.IO;
using System.Reflection;
using UnityEngine;

namespace Groups;

public static class Helper
{
	private static byte[] ReadEmbeddedFileBytes(string name)
	{
		using MemoryStream memoryStream = new MemoryStream();
		Assembly.GetExecutingAssembly().GetManifestResourceStream("Groups." + name)?.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public static Texture2D loadTexture(string name)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_001f: Expected O, but got Unknown
		Texture2D val = new Texture2D(0, 0);
		ImageConversion.LoadImage(val, ReadEmbeddedFileBytes("icons." + name));
		return val;
	}

	public static Sprite loadSprite(string name, int width, int height)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return Sprite.Create(loadTexture(name), new Rect(0f, 0f, (float)width, (float)height), Vector2.zero);
	}
}
