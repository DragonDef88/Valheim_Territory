using System;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace HarmonyLib.Internal.Util;

internal static class ILHookExtensions
{
	private static readonly MethodInfo IsAppliedSetter;

	public static readonly Action<ILHook, bool> SetIsApplied;

	private static Func<ILHook, Detour> GetAppliedDetour;

	static ILHookExtensions()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		IsAppliedSetter = AccessTools.PropertySetter(typeof(ILHook), "IsApplied");
		SetIsApplied = Extensions.CreateDelegate<Action<ILHook, bool>>((MethodBase)IsAppliedSetter);
		DynamicMethodDefinition val = new DynamicMethodDefinition("GetDetour", typeof(Detour), new Type[1] { typeof(ILHook) });
		ILGenerator iLGenerator = val.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(ILHook), "_Ctx"));
		iLGenerator.Emit(OpCodes.Ldfld, AccessTools.Field(AccessTools.Inner(typeof(ILHook), "Context"), "Detour"));
		iLGenerator.Emit(OpCodes.Ret);
		GetAppliedDetour = Extensions.CreateDelegate<Func<ILHook, Detour>>((MethodBase)val.Generate());
	}

	public static MethodBase GetCurrentTarget(this ILHook hook)
	{
		return GetAppliedDetour(hook).Target;
	}
}
