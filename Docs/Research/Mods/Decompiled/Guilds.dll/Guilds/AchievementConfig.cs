using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Guilds;

[PublicAPI]
public class AchievementConfig
{
	public string name = "";

	public string description = "";

	public List<float> progress = new List<float> { 1f };

	public List<int>? guild;

	public List<int> level = new List<int>();

	public string icon = "";

	public bool first;

	public Dictionary<string, string> config = new Dictionary<string, string>();

	public T getConfigValue<T>(string name, T defaultValue = default(T))
	{
		if (config.TryGetValue(name, out string value))
		{
			if (typeof(T) == typeof(int))
			{
				if (int.TryParse(value, out var result))
				{
					return (T)(object)result;
				}
			}
			else
			{
				if (!(typeof(T) == typeof(float)))
				{
					return (T)(object)value;
				}
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					return (T)(object)result2;
				}
			}
		}
		return defaultValue;
	}

	internal int GetLevel(int completed)
	{
		if (completed > level.Count)
		{
			return level.Max();
		}
		return level[completed - 1];
	}

	internal Sprite? GetIcon()
	{
		if (!Interface.AchievementIcons.TryGetValue(icon, out Sprite value))
		{
			return null;
		}
		return value;
	}
}
