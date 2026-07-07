using System;
using System.Reflection;

namespace HarmonyLib;

public class InvalidHarmonyPatchArgumentException : Exception
{
	public MethodBase Original { get; }

	public MethodInfo Patch { get; }

	public override string Message => "(" + Patch.FullDescription() + "): " + base.Message;

	public InvalidHarmonyPatchArgumentException(string message, MethodBase original, MethodInfo patch)
		: base(message)
	{
		Original = original;
		Patch = patch;
	}
}
