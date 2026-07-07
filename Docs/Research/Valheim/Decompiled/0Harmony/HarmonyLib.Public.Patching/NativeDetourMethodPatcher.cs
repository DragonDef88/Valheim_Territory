using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib.Tools;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace HarmonyLib.Public.Patching;

public class NativeDetourMethodPatcher : MethodPatcher
{
	private static readonly Dictionary<int, Delegate> TrampolineCache = new Dictionary<int, Delegate>();

	private static int counter;

	private static readonly object CounterLock = new object();

	private static readonly MethodInfo GetTrampolineMethod = AccessTools.Method(typeof(NativeDetourMethodPatcher), "GetTrampoline");

	private string[] argTypeNames;

	private Type[] argTypes;

	private int currentOriginal = -1;

	private int newOriginal;

	private MethodInfo invokeTrampolineMethod;

	private NativeDetour nativeDetour;

	private Type returnType;

	private Type trampolineDelegateType;

	public NativeDetourMethodPatcher(MethodBase original)
		: base(original)
	{
		Init();
	}

	private void Init()
	{
		if (AccessTools.IsNetCoreRuntime)
		{
			Logger.Log(Logger.LogChannel.Warn, () => "Patch target " + base.Original.FullDescription() + " is marked as extern. Extern methods may not be patched because of inlining behaviour of coreclr (refer to https://github.com/dotnet/coreclr/pull/8263).If you need to patch externs, consider using pure NativeDetour instead.");
		}
		MethodBase original = base.Original;
		ParameterInfo[] parameters = original.GetParameters();
		int num = ((!original.IsStatic) ? 1 : 0);
		argTypes = new Type[parameters.Length + num];
		argTypeNames = new string[parameters.Length + num];
		returnType = (original as MethodInfo)?.ReturnType;
		if (!original.IsStatic)
		{
			argTypes[0] = Extensions.GetThisParamType(original);
			argTypeNames[0] = "this";
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			argTypes[i + num] = parameters[i].ParameterType;
			argTypeNames[i + num] = parameters[i].Name;
		}
		trampolineDelegateType = DelegateTypeFactory.instance.CreateDelegateType(returnType, argTypes);
		invokeTrampolineMethod = AccessTools.Method(trampolineDelegateType, "Invoke");
	}

	public override DynamicMethodDefinition PrepareOriginal()
	{
		return GenerateManagedOriginal();
	}

	public override MethodBase DetourTo(MethodBase replacement)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		NativeDetour obj = nativeDetour;
		if (obj != null)
		{
			obj.Dispose();
		}
		nativeDetour = new NativeDetour(base.Original, replacement, new NativeDetourConfig
		{
			ManualApply = true
		});
		lock (TrampolineCache)
		{
			if (currentOriginal >= 0)
			{
				TrampolineCache.Remove(currentOriginal);
			}
			currentOriginal = newOriginal;
			TrampolineCache[currentOriginal] = CreateDelegate(trampolineDelegateType, nativeDetour.GenerateTrampoline((MethodBase)invokeTrampolineMethod));
		}
		nativeDetour.Apply();
		return replacement;
	}

	public override DynamicMethodDefinition CopyOriginal()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (!(base.Original is MethodInfo meth))
		{
			return null;
		}
		DynamicMethodDefinition val = new DynamicMethodDefinition("OrigWrapper", returnType, argTypes);
		ILGenerator iLGenerator = val.GetILGenerator();
		for (int i = 0; i < argTypes.Length; i++)
		{
			iLGenerator.Emit(OpCodes.Ldarg, i);
		}
		iLGenerator.Emit(OpCodes.Call, meth);
		iLGenerator.Emit(OpCodes.Ret);
		return val;
	}

	private Delegate CreateDelegate(Type delegateType, MethodBase mb)
	{
		if (mb is DynamicMethod dynamicMethod)
		{
			return dynamicMethod.CreateDelegate(delegateType);
		}
		return Delegate.CreateDelegate(delegateType, (mb as MethodInfo) ?? throw new InvalidCastException($"Unexpected method type: {mb.GetType()}"));
	}

	private static Delegate GetTrampoline(int hash)
	{
		lock (TrampolineCache)
		{
			return TrampolineCache[hash];
		}
	}

	private DynamicMethodDefinition GenerateManagedOriginal()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		MethodBase original = base.Original;
		lock (CounterLock)
		{
			newOriginal = counter;
			counter++;
		}
		DynamicMethodDefinition val = new DynamicMethodDefinition($"NativeDetour_Wrapper<{Extensions.GetID(original, (string)null, (string)null, true, false, true)}>?{newOriginal}", returnType, argTypes);
		MethodDefinition definition = val.Definition;
		for (int i = 0; i < argTypeNames.Length; i++)
		{
			((ParameterReference)((MethodReference)definition).Parameters[i]).Name = argTypeNames[i];
		}
		ILGenerator iLGenerator = val.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldc_I4, newOriginal);
		iLGenerator.Emit(OpCodes.Call, GetTrampolineMethod);
		for (int j = 0; j < argTypes.Length; j++)
		{
			iLGenerator.Emit(OpCodes.Ldarg, j);
		}
		iLGenerator.Emit(OpCodes.Call, invokeTrampolineMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return val;
	}

	public static void TryResolve(object sender, PatchManager.PatcherResolverEventArgs args)
	{
		if (args.Original.GetMethodBody() == null)
		{
			args.MethodPatcher = new NativeDetourMethodPatcher(args.Original);
		}
	}
}
