using System;
using System.Globalization;
using UnityEngine;

public class ZLog
{
	public static void Log(object o)
	{
		Debug.Log((object)(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": " + o?.ToString() + "\n"));
	}

	public static void DevLog(object o)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log((object)(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": " + o?.ToString() + "\n"));
		}
	}

	public static void LogError(object o)
	{
		Debug.LogError((object)(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": " + o?.ToString() + "\n"));
	}

	public static void LogWarning(object o)
	{
		Debug.LogWarning((object)(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": " + o?.ToString() + "\n"));
	}
}
