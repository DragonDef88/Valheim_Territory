using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HarmonyEmitIL : HarmonyAttribute
{
	public HarmonyEmitIL()
	{
		info.debugEmitPath = "./";
	}

	public HarmonyEmitIL(string dir)
	{
		info.debugEmitPath = dir;
	}
}
