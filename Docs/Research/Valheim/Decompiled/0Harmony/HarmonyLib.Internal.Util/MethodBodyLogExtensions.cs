using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.Utils;

namespace HarmonyLib.Internal.Util;

internal static class MethodBodyLogExtensions
{
	public static string ToILDasmString(this MethodBody mb)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected I4, but got Unknown
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder sb = new StringBuilder();
		Collection<Instruction> instructions = mb.Instructions;
		Instruction val = ((IEnumerable<Instruction>)instructions).First();
		val.Offset = 0;
		foreach (Instruction item in ((IEnumerable<Instruction>)instructions).Skip(1))
		{
			object operand = val.Operand;
			ILLabel val2 = (ILLabel)((operand is ILLabel) ? operand : null);
			if (val2 != null)
			{
				val.Operand = val2.Target;
			}
			else if (operand is ILLabel[] source)
			{
				val.Operand = source.Select((ILLabel l) => l.Target).ToArray();
			}
			item.Offset = val.Offset + val.GetSize();
			val.Operand = operand;
			val = item;
		}
		Dictionary<Instruction, List<ExceptionBlock>> exBlocks = new Dictionary<Instruction, List<ExceptionBlock>>();
		Enumerator<ExceptionHandler> enumerator2 = mb.ExceptionHandlers.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				ExceptionHandler current2 = enumerator2.Current;
				AddBlock(current2.TryStart, ExceptionBlockType.BeginExceptionBlock);
				AddBlock(current2.TryEnd, ExceptionBlockType.EndExceptionBlock);
				AddBlock(current2.HandlerEnd, ExceptionBlockType.EndExceptionBlock);
				ExceptionHandlerType handlerType = current2.HandlerType;
				switch ((int)handlerType)
				{
				case 0:
					AddBlock(current2.HandlerStart, ExceptionBlockType.BeginCatchBlock).catchType = ReflectionHelper.ResolveReflection(current2.CatchType);
					break;
				case 1:
					AddBlock(current2.FilterStart, ExceptionBlockType.BeginExceptFilterBlock);
					break;
				case 2:
					AddBlock(current2.HandlerStart, ExceptionBlockType.BeginFinallyBlock);
					break;
				case 4:
					AddBlock(current2.HandlerStart, ExceptionBlockType.BeginFaultBlock);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2).Dispose();
		}
		int indent = 0;
		WriteLine(".locals init (");
		indent += 4;
		for (int i = 0; i < mb.Variables.Count; i++)
		{
			VariableDefinition val3 = mb.Variables[i];
			WriteLine($"{((MemberReference)((VariableReference)val3).VariableType).FullName} V_{i}");
		}
		indent -= 4;
		WriteLine(")");
		Stack<string> stack = new Stack<string>();
		Enumerator<Instruction> enumerator3 = instructions.GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				Instruction current3 = enumerator3.Current;
				List<ExceptionBlock> obj = exBlocks.GetValueSafe(current3) ?? new List<ExceptionBlock>();
				obj.Sort((ExceptionBlock a, ExceptionBlock b) => (a.blockType == ExceptionBlockType.EndExceptionBlock) ? (-1) : 0);
				foreach (ExceptionBlock item2 in obj)
				{
					switch (item2.blockType)
					{
					case ExceptionBlockType.BeginExceptionBlock:
						WriteLine(".try");
						WriteLine("{");
						indent += 2;
						stack.Push(".try");
						break;
					case ExceptionBlockType.BeginCatchBlock:
						WriteLine("catch " + item2.catchType.FullName);
						WriteLine("{");
						indent += 2;
						stack.Push("handler (catch)");
						break;
					case ExceptionBlockType.BeginExceptFilterBlock:
						WriteLine("filter");
						WriteLine("{");
						indent += 2;
						stack.Push("handler (filter)");
						break;
					case ExceptionBlockType.BeginFaultBlock:
						WriteLine("fault");
						WriteLine("{");
						indent += 2;
						stack.Push("handler (fault)");
						break;
					case ExceptionBlockType.BeginFinallyBlock:
						WriteLine("finally");
						WriteLine("{");
						indent += 2;
						stack.Push("handler (finally)");
						break;
					case ExceptionBlockType.EndExceptionBlock:
						indent -= 2;
						WriteLine("} // end " + stack.Pop());
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				object operand2 = current3.Operand;
				ILLabel val4 = (ILLabel)((operand2 is ILLabel) ? operand2 : null);
				if (val4 != null)
				{
					current3.Operand = val4.Target;
				}
				else if (operand2 is ILLabel[] source2)
				{
					current3.Operand = source2.Select((ILLabel l) => l.Target).ToArray();
				}
				WriteLine(((object)current3).ToString());
				current3.Operand = operand2;
			}
		}
		finally
		{
			((IDisposable)enumerator3).Dispose();
		}
		return sb.ToString();
		ExceptionBlock AddBlock(Instruction ins, ExceptionBlockType t)
		{
			if (ins == null)
			{
				return new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock);
			}
			if (!exBlocks.TryGetValue(ins, out var value))
			{
				value = (exBlocks[ins] = new List<ExceptionBlock>());
			}
			ExceptionBlock exceptionBlock = new ExceptionBlock(t);
			value.Add(exceptionBlock);
			return exceptionBlock;
		}
		void WriteLine(string text)
		{
			sb.Append(new string(' ', indent)).AppendLine(text);
		}
	}
}
