using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib.Internal.Util;
using HarmonyLib.Tools;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.Utils;
using MonoMod.Utils.Cil;

namespace HarmonyLib.Internal.Patching;

internal class ILManipulator
{
	private class RawInstruction
	{
		public CodeInstruction Instruction { get; set; }

		public object Operand { get; set; }

		public Instruction CILInstruction { get; set; }
	}

	private static readonly Dictionary<short, OpCode> SREOpCodes;

	private static readonly Dictionary<short, OpCode> CecilOpCodes;

	private static readonly Dictionary<OpCode, OpCode> ShortToLongMap;

	private readonly IEnumerable<RawInstruction> codeInstructions;

	private readonly bool debug;

	private readonly Dictionary<VariableDefinition, LocalBuilder> localsCache = new Dictionary<VariableDefinition, LocalBuilder>();

	private readonly List<MethodInfo> transpilers = new List<MethodInfo>();

	public MethodBody Body { get; }

	static ILManipulator()
	{
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		SREOpCodes = new Dictionary<short, OpCode>();
		CecilOpCodes = new Dictionary<short, OpCode>();
		ShortToLongMap = new Dictionary<OpCode, OpCode>
		{
			[OpCodes.Beq_S] = OpCodes.Beq,
			[OpCodes.Bge_S] = OpCodes.Bge,
			[OpCodes.Bge_Un_S] = OpCodes.Bge_Un,
			[OpCodes.Bgt_S] = OpCodes.Bgt,
			[OpCodes.Bgt_Un_S] = OpCodes.Bgt_Un,
			[OpCodes.Ble_S] = OpCodes.Ble,
			[OpCodes.Ble_Un_S] = OpCodes.Ble_Un,
			[OpCodes.Blt_S] = OpCodes.Blt,
			[OpCodes.Blt_Un_S] = OpCodes.Blt_Un,
			[OpCodes.Bne_Un_S] = OpCodes.Bne_Un,
			[OpCodes.Brfalse_S] = OpCodes.Brfalse,
			[OpCodes.Brtrue_S] = OpCodes.Brtrue,
			[OpCodes.Br_S] = OpCodes.Br,
			[OpCodes.Leave_S] = OpCodes.Leave
		};
		FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			OpCode value = (OpCode)fields[i].GetValue(null);
			SREOpCodes[value.Value] = value;
		}
		fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			OpCode value2 = (OpCode)fields[i].GetValue(null);
			CecilOpCodes[((OpCode)(ref value2)).Value] = value2;
		}
	}

	public ILManipulator(MethodBody body, bool debug)
	{
		Body = body;
		this.debug = debug;
		codeInstructions = ReadBody(Body);
	}

	private int GetTarget(MethodBody body, object insOp)
	{
		ILLabel val = (ILLabel)((insOp is ILLabel) ? insOp : null);
		if (val != null)
		{
			return body.Instructions.IndexOf(val.Target);
		}
		Instruction val2 = (Instruction)((insOp is Instruction) ? insOp : null);
		if (val2 != null)
		{
			return body.Instructions.IndexOf(val2);
		}
		return -1;
	}

	private int[] GetTargets(MethodBody body, object insOp)
	{
		if (insOp is ILLabel[] arr2)
		{
			return Result<ILLabel>(arr2, (ILLabel l) => l.Target);
		}
		if (insOp is Instruction[] arr3)
		{
			return Result<Instruction>(arr3, (Instruction i) => i);
		}
		return new int[0];
		int[] Result<T>(IEnumerable<T> arr, Func<T, Instruction> insGetter)
		{
			return arr.Select((T i) => body.Instructions.IndexOf(insGetter(i))).ToArray();
		}
	}

	private IEnumerable<RawInstruction> ReadBody(MethodBody body)
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected I4, but got Unknown
		List<RawInstruction> instructions = new List<RawInstruction>(body.Instructions.Count);
		instructions.AddRange(((IEnumerable<Instruction>)body.Instructions).Select(ReadInstruction));
		foreach (RawInstruction item in instructions)
		{
			RawInstruction rawInstruction = item;
			rawInstruction.Operand = item.Instruction.opcode.OperandType switch
			{
				OperandType.ShortInlineBrTarget => instructions[(int)item.Operand].Instruction, 
				OperandType.InlineBrTarget => instructions[(int)item.Operand].Instruction, 
				OperandType.InlineSwitch => ((int[])item.Operand).Select((int i) => instructions[i].Instruction).ToArray(), 
				_ => item.Operand, 
			};
		}
		Enumerator<ExceptionHandler> enumerator2 = body.ExceptionHandlers.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				ExceptionHandler current2 = enumerator2.Current;
				CodeInstruction instruction = instructions[body.Instructions.IndexOf(current2.TryStart)].Instruction;
				_ = instructions[body.Instructions.IndexOf(current2.TryEnd)].Instruction;
				CodeInstruction instruction2 = instructions[body.Instructions.IndexOf(current2.HandlerStart)].Instruction;
				int index = ((current2.HandlerEnd == null) ? (instructions.Count - 1) : body.Instructions.IndexOf(current2.HandlerEnd.Previous));
				CodeInstruction instruction3 = instructions[index].Instruction;
				instruction.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock));
				instruction3.blocks.Add(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock));
				ExceptionHandlerType handlerType = current2.HandlerType;
				switch ((int)handlerType)
				{
				case 0:
					instruction2.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, ReflectionHelper.ResolveReflection(current2.CatchType)));
					break;
				case 1:
					instructions[body.Instructions.IndexOf(current2.FilterStart)].Instruction.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptFilterBlock));
					break;
				case 2:
					instruction2.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock));
					break;
				case 4:
					instruction2.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginFaultBlock));
					break;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2).Dispose();
		}
		return instructions;
		RawInstruction ReadInstruction(Instruction ins)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Expected I4, but got Unknown
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Expected O, but got Unknown
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Expected O, but got Unknown
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			RawInstruction rawInstruction2 = new RawInstruction();
			Dictionary<short, OpCode> sREOpCodes = SREOpCodes;
			OpCode opCode = ins.OpCode;
			rawInstruction2.Instruction = new CodeInstruction(sREOpCodes[((OpCode)(ref opCode)).Value]);
			RawInstruction rawInstruction3 = rawInstruction2;
			opCode = ins.OpCode;
			OperandType operandType = ((OpCode)(ref opCode)).OperandType;
			rawInstruction3.Operand = (int)operandType switch
			{
				1 => ReflectionHelper.ResolveReflection((MemberReference)ins.Operand), 
				4 => ReflectionHelper.ResolveReflection((MemberReference)ins.Operand), 
				12 => ReflectionHelper.ResolveReflection((MemberReference)ins.Operand), 
				11 => ReflectionHelper.ResolveReflection((MemberReference)ins.Operand), 
				13 => (object)(VariableDefinition)ins.Operand, 
				18 => (object)(VariableDefinition)ins.Operand, 
				14 => (short)((ParameterReference)(ParameterDefinition)ins.Operand).Index, 
				19 => (byte)((ParameterReference)(ParameterDefinition)ins.Operand).Index, 
				0 => GetTarget(body, ins.Operand), 
				15 => GetTarget(body, ins.Operand), 
				10 => GetTargets(body, ins.Operand), 
				_ => ins.Operand, 
			};
			rawInstruction2.CILInstruction = ins;
			return rawInstruction2;
		}
	}

	public void AddTranspiler(MethodInfo transpiler)
	{
		transpilers.Add(transpiler);
	}

	private object[] GetTranspilerArguments(ILGenerator il, MethodInfo transpiler, IEnumerable<CodeInstruction> instructions, MethodBase orignal = null)
	{
		List<object> list = new List<object>();
		foreach (Type item in from p in transpiler.GetParameters()
			select p.ParameterType)
		{
			if (item.IsAssignableFrom(typeof(ILGenerator)))
			{
				list.Add(il);
			}
			else if (item.IsAssignableFrom(typeof(MethodBase)) && (object)orignal != null)
			{
				list.Add(orignal);
			}
			else if (item.IsAssignableFrom(typeof(IEnumerable<CodeInstruction>)))
			{
				list.Add(instructions);
			}
		}
		return list.ToArray();
	}

	public IEnumerable<KeyValuePair<OpCode, object>> GetRawInstructions()
	{
		return codeInstructions.Select((RawInstruction i) => new KeyValuePair<OpCode, object>(i.Instruction.opcode, i.Operand));
	}

	public List<CodeInstruction> GetInstructions(ILGenerator il, MethodBase original = null)
	{
		return NormalizeInstructions(ApplyTranspilers(il, original, (VariableDefinition vDef) => il.DeclareLocal(ReflectionHelper.ResolveReflection(((VariableReference)vDef).VariableType)), il.DefineLabel)).ToList();
	}

	private IEnumerable<CodeInstruction> ApplyTranspilers(ILGenerator il, MethodBase original, Func<VariableDefinition, LocalBuilder> getLocal, Func<Label> defineLabel)
	{
		List<CodeInstruction> list = (from i in Prepare(getLocal, defineLabel)
			select i.Instruction).ToList();
		if (transpilers.Count == 0)
		{
			return list;
		}
		IEnumerable<CodeInstruction> enumerable = NormalizeInstructions(list);
		foreach (MethodInfo transpiler in transpilers)
		{
			object[] transpilerArguments = GetTranspilerArguments(il, transpiler, enumerable, original);
			Logger.Log(Logger.LogChannel.Info, () => "Running transpiler " + transpiler.FullDescription(), debug);
			enumerable = NormalizeInstructions(transpiler.Invoke(null, transpilerArguments) as IEnumerable<CodeInstruction>).ToList();
		}
		return enumerable;
	}

	public Dictionary<int, CodeInstruction> GetIndexedInstructions(ILGenerator il)
	{
		int size = 0;
		return Prepare((VariableDefinition vDef) => il.DeclareLocal(ReflectionHelper.ResolveReflection(((VariableReference)vDef).VariableType)), il.DefineLabel).ToDictionary((RawInstruction i) => Grow(ref size, i.CILInstruction.GetSize()), (RawInstruction i) => i.Instruction);
		static int Grow(ref int i, int s)
		{
			int result = i;
			i += s;
			return result;
		}
	}

	private IEnumerable<RawInstruction> Prepare(Func<VariableDefinition, LocalBuilder> getLocal, Func<Label> defineLabel)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		localsCache.Clear();
		Enumerator<VariableDefinition> enumerator = Body.Variables.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				VariableDefinition current = enumerator.Current;
				localsCache[current] = getLocal(current);
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		foreach (RawInstruction codeInstruction2 in codeInstructions)
		{
			codeInstruction2.Instruction.operand = codeInstruction2.Operand;
			switch (codeInstruction2.Instruction.opcode.OperandType)
			{
			case OperandType.InlineVar:
			case OperandType.ShortInlineVar:
			{
				object operand = codeInstruction2.Operand;
				VariableDefinition val = (VariableDefinition)((operand is VariableDefinition) ? operand : null);
				if (val != null)
				{
					codeInstruction2.Instruction.operand = localsCache[val];
				}
				break;
			}
			case OperandType.InlineSwitch:
				if (codeInstruction2.Operand is CodeInstruction[] array)
				{
					List<Label> list = new List<Label>();
					CodeInstruction[] array2 = array;
					foreach (CodeInstruction obj in array2)
					{
						Label item = defineLabel();
						obj.labels.Add(item);
						list.Add(item);
					}
					codeInstruction2.Instruction.operand = list.ToArray();
				}
				break;
			case OperandType.InlineBrTarget:
			case OperandType.ShortInlineBrTarget:
				if (codeInstruction2.Instruction.operand is CodeInstruction codeInstruction)
				{
					Label label = defineLabel();
					codeInstruction.labels.Add(label);
					codeInstruction2.Instruction.operand = label;
				}
				break;
			}
		}
		return codeInstructions;
	}

	public void WriteTo(MethodBody body, MethodBase original = null)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		body.Instructions.Clear();
		body.ExceptionHandlers.Clear();
		CecilILGenerator il = new CecilILGenerator(body.GetILProcessor());
		ILGenerator proxy = ((ILGeneratorShim)il).GetProxy();
		((ILGeneratorShim)il).DefineLabel();
		foreach (var (codeInstruction, codeInstruction2) in ApplyTranspilers(proxy, original, (Func<VariableDefinition, LocalBuilder>)((VariableDefinition vDef) => il.GetLocal(vDef)), (Func<Label>)((ILGeneratorShim)il).DefineLabel).Pairwise())
		{
			codeInstruction.labels.ForEach(delegate(Label l)
			{
				((ILGeneratorShim)il).MarkLabel(l);
			});
			codeInstruction.blocks.ForEach(delegate(ExceptionBlock b)
			{
				il.MarkBlockBefore(b);
			});
			if (((!(codeInstruction.opcode == OpCodes.Leave) && !(codeInstruction.opcode == OpCodes.Leave_S)) || (codeInstruction.blocks.Count <= 0 && (codeInstruction2 == null || codeInstruction2.blocks.Count <= 0))) && ((!(codeInstruction.opcode == OpCodes.Endfilter) && !(codeInstruction.opcode == OpCodes.Endfinally)) || codeInstruction.blocks.Count <= 0))
			{
				switch (codeInstruction.opcode.OperandType)
				{
				case OperandType.InlineNone:
					((ILGeneratorShim)il).Emit(codeInstruction.opcode);
					break;
				case OperandType.InlineSig:
					throw new NotSupportedException("Emitting opcodes with CallSites is currently not fully implemented");
				default:
					if (codeInstruction.operand == null)
					{
						throw new ArgumentNullException("operand", $"Invalid argument for {codeInstruction}");
					}
					il.Emit(codeInstruction.opcode, codeInstruction.operand);
					break;
				}
			}
			codeInstruction.blocks.ForEach(delegate(ExceptionBlock b)
			{
				il.MarkBlockAfter(b);
			});
		}
		if (body.Instructions.Count == 0)
		{
			((ILGeneratorShim)il).Emit(OpCodes.Ret);
		}
	}

	private static IEnumerable<CodeInstruction> NormalizeInstructions(IEnumerable<CodeInstruction> instrs)
	{
		foreach (CodeInstruction instr in instrs)
		{
			CodeInstruction codeInstruction = instr;
			if (codeInstruction.labels == null)
			{
				codeInstruction.labels = new List<Label>();
			}
			codeInstruction = instr;
			if (codeInstruction.blocks == null)
			{
				codeInstruction.blocks = new List<ExceptionBlock>();
			}
			if (ShortToLongMap.TryGetValue(instr.opcode, out var value))
			{
				instr.opcode = value;
			}
			yield return instr;
		}
	}

	public static Dictionary<int, CodeInstruction> GetInstructions(MethodBody body)
	{
		if (body == null)
		{
			return null;
		}
		try
		{
			return new ILManipulator(body, debug: false).GetIndexedInstructions(PatchProcessor.CreateILGenerator());
		}
		catch (Exception ex)
		{
			Exception e = ex;
			Logger.Log(Logger.LogChannel.Warn, () => "Could not read instructions of " + Extensions.GetID((MethodReference)(object)body.Method, (string)null, (string)null, true, false) + ": " + e.Message);
			return null;
		}
	}
}
