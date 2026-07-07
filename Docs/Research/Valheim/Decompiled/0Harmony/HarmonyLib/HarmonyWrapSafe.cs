using System;

namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HarmonyWrapSafe : HarmonyAttribute
{
	public HarmonyWrapSafe()
	{
		info.wrapTryCatch = true;
	}
}
