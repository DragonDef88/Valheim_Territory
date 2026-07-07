using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib.Internal.Patching;
using HarmonyLib.Internal.Util;
using HarmonyLib.Public.Patching;
using HarmonyLib.Tools;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace HarmonyLib;

internal static class PatchFunctions
{
	internal static List<MethodInfo> GetSortedPatchMethods(MethodBase original, Patch[] patches, bool debug)
	{
		return new PatchSorter(patches, debug).Sort(original);
	}

	internal static Patch[] GetSortedPatchMethodsAsPatches(MethodBase original, Patch[] patches, bool debug)
	{
		return new PatchSorter(patches, debug).SortAsPatches(original);
	}

	internal static MethodInfo UpdateWrapper(MethodBase original, PatchInfo patchInfo)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		MethodPatcher methodPatcher = original.GetMethodPatcher();
		DynamicMethodDefinition val = methodPatcher.PrepareOriginal();
		if (val != null)
		{
			ILContext ctx = new ILContext(val.Definition);
			HarmonyManipulator.Manipulate(original, patchInfo, ctx);
		}
		try
		{
			return methodPatcher.DetourTo((val != null) ? val.Generate() : null) as MethodInfo;
		}
		catch (Exception ex)
		{
			object body;
			if (val == null)
			{
				body = null;
			}
			else
			{
				MethodDefinition definition = val.Definition;
				body = ((definition != null) ? definition.Body : null);
			}
			throw HarmonyException.Create(ex, (MethodBody)body);
		}
	}

	internal static MethodInfo ReversePatch(HarmonyMethod standin, MethodBase original, MethodInfo postTranspiler, MethodInfo postManipulator)
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		if (standin == null)
		{
			throw new ArgumentNullException("standin");
		}
		if ((object)standin.method == null)
		{
			throw new ArgumentNullException("standin", "standin.method is NULL");
		}
		bool debug = standin.debug.GetValueOrDefault();
		List<MethodInfo> transpilers = new List<MethodInfo>();
		List<MethodInfo> ilmanipulators = new List<MethodInfo>();
		if (standin.reversePatchType == HarmonyReversePatchType.Snapshot)
		{
			Patches patchInfo = Harmony.GetPatchInfo(original);
			transpilers.AddRange(GetSortedPatchMethods(original, patchInfo.Transpilers.ToArray(), debug));
			ilmanipulators.AddRange(GetSortedPatchMethods(original, patchInfo.ILManipulators.ToArray(), debug));
		}
		if ((object)postTranspiler != null)
		{
			transpilers.Add(postTranspiler);
		}
		if ((object)postManipulator != null)
		{
			ilmanipulators.Add(postManipulator);
		}
		Logger.Log(Logger.LogChannel.Info, delegate
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Reverse patching " + standin.method.FullDescription() + " with " + original.FullDescription());
			PrintInfo(stringBuilder, transpilers, "Transpiler");
			PrintInfo(stringBuilder, ilmanipulators, "Manipulators");
			return stringBuilder.ToString();
		}, debug);
		MethodBody patchBody = null;
		ILHook val = new ILHook((MethodBase)standin.method, (Manipulator)delegate(ILContext ctx)
		{
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Expected O, but got Unknown
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			if (original is MethodInfo methodInfo2)
			{
				patchBody = ctx.Body;
				MethodPatcher methodPatcher = methodInfo2.GetMethodPatcher();
				DynamicMethodDefinition val2 = methodPatcher.CopyOriginal();
				if (val2 == null)
				{
					throw new NullReferenceException("Cannot reverse patch " + methodInfo2.FullDescription() + ": method patcher (" + methodPatcher.GetType().FullDescription() + ") can't copy original method body");
				}
				ILManipulator iLManipulator = new ILManipulator(val2.Definition.Body, debug);
				ctx.Body.Variables.Clear();
				Enumerator<VariableDefinition> enumerator2 = iLManipulator.Body.Variables.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						VariableDefinition current2 = enumerator2.Current;
						ctx.Body.Variables.Add(new VariableDefinition(ctx.Module.ImportReference(((VariableReference)current2).VariableType)));
					}
				}
				finally
				{
					((IDisposable)enumerator2).Dispose();
				}
				foreach (MethodInfo item in transpilers)
				{
					iLManipulator.AddTranspiler(item);
				}
				iLManipulator.WriteTo(ctx.Body, standin.method);
				HarmonyManipulator.ApplyManipulators(ctx, original, ilmanipulators, null);
				Instruction val3 = null;
				foreach (Instruction item2 in ((IEnumerable<Instruction>)ctx.Instrs).Where((Instruction i) => i.OpCode == OpCodes.Ret))
				{
					if (val3 == null)
					{
						val3 = ctx.IL.Create(OpCodes.Ret);
					}
					item2.OpCode = OpCodes.Br;
					item2.Operand = val3;
				}
				if (val3 != null)
				{
					ctx.IL.Append(val3);
				}
				Logger.Log(Logger.LogChannel.IL, () => "Generated reverse patcher (" + ((MemberReference)ctx.Method).FullName + "):\n" + ctx.Body.ToILDasmString(), debug);
			}
		}, new ILHookConfig
		{
			ManualApply = true
		});
		try
		{
			val.Apply();
		}
		catch (Exception ex)
		{
			throw HarmonyException.Create(ex, patchBody);
		}
		MethodInfo methodInfo = val.GetCurrentTarget() as MethodInfo;
		PatchTools.RememberObject(standin.method, methodInfo);
		return methodInfo;
		static void PrintInfo(StringBuilder sb, ICollection<MethodInfo> methods, string name)
		{
			if (methods.Count <= 0)
			{
				return;
			}
			sb.AppendLine(name + ":");
			foreach (MethodInfo method in methods)
			{
				sb.AppendLine("  * " + method.FullDescription());
			}
		}
	}

	internal static IEnumerable<CodeInstruction> ApplyTranspilers(MethodBase methodBase, ILGenerator generator, int maxTranspilers = 0)
	{
		MethodPatcher methodPatcher = methodBase.GetMethodPatcher();
		DynamicMethodDefinition val = methodPatcher.CopyOriginal();
		if (val == null)
		{
			throw new NullReferenceException("Cannot reverse patch " + methodBase.FullDescription() + ": method patcher (" + methodPatcher.GetType().FullDescription() + ") can't copy original method body");
		}
		ILManipulator iLManipulator = new ILManipulator(val.Definition.Body, debug: false);
		PatchInfo patchInfo = methodBase.GetPatchInfo();
		if (patchInfo != null)
		{
			List<MethodInfo> sortedPatchMethods = GetSortedPatchMethods(methodBase, patchInfo.transpilers, debug: false);
			for (int i = 0; i < maxTranspilers && i < sortedPatchMethods.Count; i++)
			{
				iLManipulator.AddTranspiler(sortedPatchMethods[i]);
			}
		}
		return iLManipulator.GetInstructions(generator, methodBase);
	}

	internal static void UnpatchConditional(Func<Patch, bool> executionCondition)
	{
		foreach (MethodBase item in PatchProcessor.GetAllPatchedMethods().ToList())
		{
			bool num = item.HasMethodBody();
			Patches patchInfo2 = PatchProcessor.GetPatchInfo(item);
			PatchProcessor patchProcessor = new PatchProcessor(null, item);
			if (num)
			{
				patchInfo2.Postfixes.DoIf(executionCondition, delegate(Patch patchInfo)
				{
					patchProcessor.Unpatch(patchInfo.PatchMethod);
				});
				patchInfo2.Prefixes.DoIf(executionCondition, delegate(Patch patchInfo)
				{
					patchProcessor.Unpatch(patchInfo.PatchMethod);
				});
			}
			patchInfo2.ILManipulators.DoIf(executionCondition, delegate(Patch patchInfo)
			{
				patchProcessor.Unpatch(patchInfo.PatchMethod);
			});
			patchInfo2.Transpilers.DoIf(executionCondition, delegate(Patch patchInfo)
			{
				patchProcessor.Unpatch(patchInfo.PatchMethod);
			});
			if (num)
			{
				patchInfo2.Finalizers.DoIf(executionCondition, delegate(Patch patchInfo)
				{
					patchProcessor.Unpatch(patchInfo.PatchMethod);
				});
			}
		}
	}
}
