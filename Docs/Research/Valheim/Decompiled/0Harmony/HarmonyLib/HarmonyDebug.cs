using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HarmonyDebug : HarmonyAttribute
{
	public HarmonyDebug()
	{
		info.debug = true;
	}
}
