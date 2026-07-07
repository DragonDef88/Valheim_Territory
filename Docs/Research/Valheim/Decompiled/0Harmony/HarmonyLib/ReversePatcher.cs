using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.Cil;

namespace HarmonyLib;

public class ReversePatcher
{
	private readonly Harmony instance;

	private readonly MethodBase original;

	private readonly HarmonyMethod standin;

	public ReversePatcher(Harmony instance, MethodBase original, HarmonyMethod standin)
	{
		this.instance = instance;
		this.original = original;
		this.standin = standin;
	}

	public MethodInfo Patch(HarmonyReversePatchType type = HarmonyReversePatchType.Original)
	{
		if ((object)original == null)
		{
			throw new NullReferenceException("Null method for " + instance.Id);
		}
		standin.reversePatchType = type;
		MethodInfo transpiler = GetTranspiler(standin.method);
		MethodInfo manipulator = GetManipulator(standin.method);
		return PatchFunctions.ReversePatch(standin, original, transpiler, manipulator);
	}

	internal static MethodInfo GetTranspiler(MethodInfo method)
	{
		string methodName = method.Name;
		List<MethodInfo> declaredMethods = AccessTools.GetDeclaredMethods(method.DeclaringType);
		Type ici = typeof(IEnumerable<CodeInstruction>);
		return declaredMethods.FirstOrDefault((MethodInfo m) => (object)m.ReturnType == ici && m.Name.StartsWith("<" + methodName + ">"));
	}

	internal static MethodInfo GetManipulator(MethodInfo method)
	{
		string methodName = method.Name;
		List<MethodInfo> declaredMethods = AccessTools.GetDeclaredMethods(method.DeclaringType);
		Type ctxType = typeof(ILContext);
		return declaredMethods.FirstOrDefault((MethodInfo m) => (from p in m.GetParameters()
			select p.ParameterType).Contains(ctxType) && m.Name.StartsWith("<" + methodName + ">"));
	}
}
