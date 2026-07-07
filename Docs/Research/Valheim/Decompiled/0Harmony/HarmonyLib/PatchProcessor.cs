using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib.Internal.Patching;
using HarmonyLib.Public.Patching;
using HarmonyLib.Tools;
using MonoMod.Utils;

namespace HarmonyLib;

public class PatchProcessor
{
	private readonly Harmony instance;

	private readonly MethodBase original;

	private HarmonyMethod prefix;

	private HarmonyMethod postfix;

	private HarmonyMethod transpiler;

	private HarmonyMethod finalizer;

	private HarmonyMethod ilmanipulator;

	internal static readonly object locker = new object();

	public PatchProcessor(Harmony instance, MethodBase original)
	{
		this.instance = instance;
		this.original = original;
	}

	public PatchProcessor AddPrefix(HarmonyMethod prefix)
	{
		this.prefix = prefix;
		return this;
	}

	public PatchProcessor AddPrefix(MethodInfo fixMethod)
	{
		prefix = new HarmonyMethod(fixMethod);
		return this;
	}

	public PatchProcessor AddPostfix(HarmonyMethod postfix)
	{
		this.postfix = postfix;
		return this;
	}

	public PatchProcessor AddPostfix(MethodInfo fixMethod)
	{
		postfix = new HarmonyMethod(fixMethod);
		return this;
	}

	public PatchProcessor AddTranspiler(HarmonyMethod transpiler)
	{
		this.transpiler = transpiler;
		return this;
	}

	public PatchProcessor AddTranspiler(MethodInfo fixMethod)
	{
		transpiler = new HarmonyMethod(fixMethod);
		return this;
	}

	public PatchProcessor AddFinalizer(HarmonyMethod finalizer)
	{
		this.finalizer = finalizer;
		return this;
	}

	public PatchProcessor AddFinalizer(MethodInfo fixMethod)
	{
		finalizer = new HarmonyMethod(fixMethod);
		return this;
	}

	public PatchProcessor AddILManipulator(HarmonyMethod ilmanipulator)
	{
		this.ilmanipulator = ilmanipulator;
		return this;
	}

	public PatchProcessor AddILManipulator(MethodInfo fixMethod)
	{
		ilmanipulator = new HarmonyMethod(fixMethod);
		return this;
	}

	public static IEnumerable<MethodBase> GetAllPatchedMethods()
	{
		return PatchManager.GetPatchedMethods();
	}

	public MethodInfo Patch()
	{
		if ((object)original == null)
		{
			throw new NullReferenceException("Null method for " + instance.Id);
		}
		if (!original.IsDeclaredMember())
		{
			Logger.Log(Logger.LogChannel.Warn, () => instance.Id + ": You should only patch implemented methods/constructors to avoid issues. Patch the declared method " + original.GetDeclaredMember().FullDescription() + " instead of " + original.FullDescription() + ".");
		}
		lock (locker)
		{
			PatchInfo patchInfo = original.ToPatchInfo();
			patchInfo.AddPrefixes(instance.Id, prefix);
			patchInfo.AddPostfixes(instance.Id, postfix);
			patchInfo.AddTranspilers(instance.Id, transpiler);
			patchInfo.AddFinalizers(instance.Id, finalizer);
			patchInfo.AddILManipulators(instance.Id, ilmanipulator);
			MethodInfo methodInfo = PatchFunctions.UpdateWrapper(original, patchInfo);
			PatchManager.AddReplacementOriginal(original, methodInfo);
			return methodInfo;
		}
	}

	public PatchProcessor Unpatch(HarmonyPatchType type, string harmonyID)
	{
		lock (locker)
		{
			PatchInfo patchInfo = original.ToPatchInfo();
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Prefix)
			{
				patchInfo.RemovePrefix(harmonyID);
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Postfix)
			{
				patchInfo.RemovePostfix(harmonyID);
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Transpiler)
			{
				patchInfo.RemoveTranspiler(harmonyID);
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Finalizer)
			{
				patchInfo.RemoveFinalizer(harmonyID);
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.ILManipulator)
			{
				patchInfo.RemoveILManipulator(harmonyID);
			}
			MethodInfo replacement = PatchFunctions.UpdateWrapper(original, patchInfo);
			PatchManager.AddReplacementOriginal(original, replacement);
			return this;
		}
	}

	public PatchProcessor Unpatch(MethodInfo patch)
	{
		lock (locker)
		{
			PatchInfo patchInfo = original.ToPatchInfo();
			patchInfo.RemovePatch(patch);
			MethodInfo replacement = PatchFunctions.UpdateWrapper(original, patchInfo);
			PatchManager.AddReplacementOriginal(original, replacement);
			return this;
		}
	}

	public static Patches GetPatchInfo(MethodBase method)
	{
		PatchInfo patchInfo = method.GetPatchInfo();
		if (patchInfo == null)
		{
			return null;
		}
		return new Patches(patchInfo.prefixes, patchInfo.postfixes, patchInfo.transpilers, patchInfo.finalizers, patchInfo.ilmanipulators);
	}

	public static List<MethodInfo> GetSortedPatchMethods(MethodBase original, Patch[] patches)
	{
		return PatchFunctions.GetSortedPatchMethods(original, patches, debug: false);
	}

	public static Dictionary<string, Version> VersionInfo(out Version currentVersion)
	{
		currentVersion = typeof(Harmony).Assembly.GetName().Version;
		Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
		GetAllPatchedMethods().Do(delegate(MethodBase method)
		{
			PatchInfo patchInfo = method.GetPatchInfo();
			patchInfo.prefixes.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
			patchInfo.postfixes.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
			patchInfo.transpilers.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
			patchInfo.finalizers.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
			patchInfo.ilmanipulators.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
		});
		Dictionary<string, Version> result = new Dictionary<string, Version>();
		assemblies.Do(delegate(KeyValuePair<string, Assembly> info)
		{
			AssemblyName assemblyName = info.Value.GetReferencedAssemblies().FirstOrDefault((AssemblyName a) => a.FullName.StartsWith("0Harmony, Version", StringComparison.Ordinal));
			if (assemblyName != null)
			{
				result[info.Key] = assemblyName.Version;
			}
		});
		return result;
	}

	public static ILGenerator CreateILGenerator()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return new DynamicMethodDefinition($"ILGenerator_{Guid.NewGuid()}", typeof(void), new Type[0]).GetILGenerator();
	}

	public static ILGenerator CreateILGenerator(MethodBase original)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Type type = ((original is MethodInfo methodInfo) ? methodInfo.ReturnType : typeof(void));
		List<Type> list = (from pi in original.GetParameters()
			select pi.ParameterType).ToList();
		if (!original.IsStatic)
		{
			list.Insert(0, original.DeclaringType);
		}
		return new DynamicMethodDefinition("ILGenerator_" + original.Name, type, list.ToArray()).GetILGenerator();
	}

	public static List<CodeInstruction> GetOriginalInstructions(MethodBase original, ILGenerator generator = null)
	{
		return PatchFunctions.ApplyTranspilers(original, generator ?? CreateILGenerator(original)).ToList();
	}

	public static List<CodeInstruction> GetOriginalInstructions(MethodBase original, out ILGenerator generator)
	{
		generator = CreateILGenerator(original);
		return PatchFunctions.ApplyTranspilers(original, generator).ToList();
	}

	public static List<CodeInstruction> GetCurrentInstructions(MethodBase original, int maxTranspilers = int.MaxValue, ILGenerator generator = null)
	{
		return PatchFunctions.ApplyTranspilers(original, generator ?? CreateILGenerator(original), maxTranspilers).ToList();
	}

	public static List<CodeInstruction> GetCurrentInstructions(MethodBase original, out ILGenerator generator, int maxTranspilers = int.MaxValue)
	{
		generator = CreateILGenerator(original);
		return PatchFunctions.ApplyTranspilers(original, generator, maxTranspilers).ToList();
	}

	public static IEnumerable<KeyValuePair<OpCode, object>> ReadMethodBody(MethodBase method)
	{
		DynamicMethodDefinition val = method.GetMethodPatcher().CopyOriginal();
		if (val == null)
		{
			return null;
		}
		return new ILManipulator(val.Definition.Body, debug: false).GetRawInstructions();
	}

	public static IEnumerable<KeyValuePair<OpCode, object>> ReadMethodBody(MethodBase method, ILGenerator generator)
	{
		DynamicMethodDefinition val = method.GetMethodPatcher().CopyOriginal();
		if (val == null)
		{
			return null;
		}
		ILManipulator iLManipulator = new ILManipulator(val.Definition.Body, debug: false);
		iLManipulator.GetInstructions(generator);
		return iLManipulator.GetRawInstructions();
	}
}
