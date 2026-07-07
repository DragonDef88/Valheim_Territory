using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Utils;
using MonoMod.Utils.Cil;

namespace HarmonyLib;

public static class Transpilers
{
	private static readonly Dictionary<int, Delegate> DelegateCache = new Dictionary<int, Delegate>();

	private static int delegateCounter;

	public static CodeInstruction EmitDelegate<T>(T action) where T : Delegate
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		if (action.Method.IsStatic && action.Target == null)
		{
			return new CodeInstruction(OpCodes.Call, action.Method);
		}
		Type[] array = (from x in action.Method.GetParameters()
			select x.ParameterType).ToArray();
		DynamicMethodDefinition val = new DynamicMethodDefinition(action.Method.Name, action.Method.ReturnType, array);
		ILGenerator iLGenerator = val.GetILGenerator();
		Type type = action.Target.GetType();
		if (action.Target != null && type.GetFields().Any((FieldInfo x) => !x.IsStatic))
		{
			int num = delegateCounter++;
			DelegateCache[num] = action;
			FieldInfo field = AccessTools.Field(typeof(Transpilers), "DelegateCache");
			MethodInfo meth = AccessTools.Method(typeof(Dictionary<int, Delegate>), "get_Item");
			iLGenerator.Emit(OpCodes.Ldsfld, field);
			iLGenerator.Emit(OpCodes.Ldc_I4, num);
			iLGenerator.Emit(OpCodes.Callvirt, meth);
		}
		else
		{
			if (action.Target == null)
			{
				iLGenerator.Emit(OpCodes.Ldnull);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Newobj, AccessTools.FirstConstructor(type, (ConstructorInfo x) => x.GetParameters().Length == 0 && !x.IsStatic));
			}
			iLGenerator.Emit(OpCodes.Ldftn, action.Method);
			iLGenerator.Emit(OpCodes.Newobj, AccessTools.Constructor(typeof(T), new Type[2]
			{
				typeof(object),
				typeof(IntPtr)
			}));
		}
		for (int i = 0; i < array.Length; i++)
		{
			iLGenerator.Emit(OpCodes.Ldarg_S, (short)i);
		}
		iLGenerator.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(T), "Invoke"));
		iLGenerator.Emit(OpCodes.Ret);
		return new CodeInstruction(OpCodes.Call, val.Generate());
	}

	public static IEnumerable<CodeInstruction> MethodReplacer(this IEnumerable<CodeInstruction> instructions, MethodBase from, MethodBase to)
	{
		if ((object)from == null)
		{
			throw new ArgumentException("Unexpected null argument", "from");
		}
		if ((object)to == null)
		{
			throw new ArgumentException("Unexpected null argument", "to");
		}
		foreach (CodeInstruction instruction in instructions)
		{
			if ((object)(instruction.operand as MethodBase) == from)
			{
				instruction.opcode = (to.IsConstructor ? OpCodes.Newobj : OpCodes.Call);
				instruction.operand = to;
			}
			yield return instruction;
		}
	}

	public static IEnumerable<CodeInstruction> Manipulator(this IEnumerable<CodeInstruction> instructions, Func<CodeInstruction, bool> predicate, Action<CodeInstruction> action)
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return instructions.Select(delegate(CodeInstruction instruction)
		{
			if (predicate(instruction))
			{
				action(instruction);
			}
			return instruction;
		}).AsEnumerable();
	}

	public static IEnumerable<CodeInstruction> DebugLogger(this IEnumerable<CodeInstruction> instructions, string text)
	{
		yield return new CodeInstruction(OpCodes.Ldstr, text);
		yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FileLog), "Log"));
		foreach (CodeInstruction instruction in instructions)
		{
			yield return instruction;
		}
	}

	public static IEnumerable<CodeInstruction> ReplaceWith(MethodBase replacement, ILGenerator ilGenerator)
	{
		if (replacement.GetMethodBody() == null)
		{
			throw new ArgumentException("Replacement method must be a managed method", "replacement");
		}
		Traverse.Create(ilGenerator).Field("Target").GetValue<CecilILGenerator>()
			.IL.Body.Variables.Clear();
		return PatchProcessor.GetOriginalInstructions(replacement, ilGenerator);
	}
}
