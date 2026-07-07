using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace HarmonyLib.Internal.Util;

internal class ILEmitter
{
	public class Label
	{
		public bool emitted;

		public Instruction instruction = Instruction.Create(OpCodes.Nop);

		public List<Instruction> targets = new List<Instruction>();
	}

	public class ExceptionBlock
	{
		public LabelledExceptionHandler prev;

		public LabelledExceptionHandler cur;

		public Label start;

		public Label skip;
	}

	public class LabelledExceptionHandler
	{
		public TypeReference exceptionType;

		public ExceptionHandlerType handlerType;

		public Label tryStart;

		public Label tryEnd;

		public Label filterStart;

		public Label handlerStart;

		public Label handlerEnd;
	}

	public readonly ILProcessor IL;

	private readonly List<LabelledExceptionHandler> pendingExceptionHandlers = new List<LabelledExceptionHandler>();

	private readonly List<Label> pendingLabels = new List<Label>();

	public Instruction emitBefore;

	private Instruction Target => emitBefore ?? IL.Body.Instructions[IL.Body.Instructions.Count - 1];

	public ILEmitter(ILProcessor il)
	{
		IL = il;
	}

	public ExceptionBlock BeginExceptionBlock(Label start)
	{
		return new ExceptionBlock
		{
			start = start
		};
	}

	public void EndExceptionBlock(ExceptionBlock block)
	{
		EndHandler(block, block.cur);
	}

	public void BeginHandler(ExceptionBlock block, ExceptionHandlerType handlerType, Type exceptionType = null)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		LabelledExceptionHandler labelledExceptionHandler = (block.prev = block.cur);
		if (labelledExceptionHandler != null)
		{
			EndHandler(block, labelledExceptionHandler);
		}
		block.skip = DeclareLabel();
		Emit(OpCodes.Leave, block.skip);
		Label label = DeclareLabel();
		MarkLabel(label);
		block.cur = new LabelledExceptionHandler
		{
			tryStart = block.start,
			tryEnd = label,
			handlerType = handlerType,
			handlerEnd = block.skip,
			exceptionType = (((object)exceptionType != null) ? Extensions.Import(IL, exceptionType) : null)
		};
		if ((int)handlerType == 1)
		{
			block.cur.filterStart = label;
		}
		else
		{
			block.cur.handlerStart = label;
		}
	}

	public void EndHandler(ExceptionBlock block, LabelledExceptionHandler handler)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		ExceptionHandlerType handlerType = handler.handlerType;
		if ((int)handlerType != 1)
		{
			if ((int)handlerType == 2)
			{
				Emit(OpCodes.Endfinally);
			}
			else
			{
				Emit(OpCodes.Leave, block.skip);
			}
		}
		else
		{
			Emit(OpCodes.Endfilter);
		}
		MarkLabel(block.skip);
		pendingExceptionHandlers.Add(block.cur);
	}

	public VariableDefinition DeclareVariable(Type type)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		VariableDefinition val = new VariableDefinition(Extensions.Import(IL, type));
		IL.Body.Variables.Add(val);
		return val;
	}

	public Label DeclareLabel()
	{
		return new Label();
	}

	public Label DeclareLabelFor(Instruction ins)
	{
		return new Label
		{
			emitted = true,
			instruction = ins
		};
	}

	public void MarkLabel(Label label)
	{
		if (!label.emitted)
		{
			pendingLabels.Add(label);
		}
	}

	public Instruction SetOpenLabelsTo(Instruction ins)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		if (pendingLabels.Count != 0)
		{
			foreach (Label pendingLabel in pendingLabels)
			{
				foreach (Instruction target in pendingLabel.targets)
				{
					if (target.Operand is Instruction)
					{
						target.Operand = ins;
					}
					else
					{
						if (!(target.Operand is Instruction[] array))
						{
							continue;
						}
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] == pendingLabel.instruction)
							{
								array[i] = ins;
								break;
							}
						}
					}
				}
				pendingLabel.instruction = ins;
				pendingLabel.emitted = true;
			}
			pendingLabels.Clear();
		}
		if (pendingExceptionHandlers.Count != 0)
		{
			foreach (LabelledExceptionHandler pendingExceptionHandler in pendingExceptionHandlers)
			{
				IL.Body.ExceptionHandlers.Add(new ExceptionHandler(pendingExceptionHandler.handlerType)
				{
					TryStart = pendingExceptionHandler.tryStart?.instruction,
					TryEnd = pendingExceptionHandler.tryEnd?.instruction,
					FilterStart = pendingExceptionHandler.filterStart?.instruction,
					HandlerStart = pendingExceptionHandler.handlerStart?.instruction,
					HandlerEnd = pendingExceptionHandler.handlerEnd?.instruction,
					CatchType = pendingExceptionHandler.exceptionType
				});
			}
			pendingExceptionHandlers.Clear();
		}
		return ins;
	}

	public void Emit(OpCode opcode)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode)));
	}

	public void Emit(OpCode opcode, Label label)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Instruction val = SetOpenLabelsTo(IL.Create(opcode, label.instruction));
		label.targets.Add(val);
		IL.InsertBefore(Target, val);
	}

	public void Emit(OpCode opcode, ConstructorInfo cInfo)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, Extensions.Import(IL, (MethodBase)cInfo))));
	}

	public void Emit(OpCode opcode, MethodInfo mInfo)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, Extensions.Import(IL, (MethodBase)mInfo))));
	}

	public void Emit(OpCode opcode, Type cls)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, Extensions.Import(IL, cls))));
	}

	public void EmitUnsafe(OpCode opcode, object arg)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(Extensions.Create(IL, opcode, arg)));
	}

	public void Emit(OpCode opcode, int arg)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, arg)));
	}

	public void Emit(OpCode opcode, string arg)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, arg)));
	}

	public void Emit(OpCode opcode, FieldInfo fInfo)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, Extensions.Import(IL, fInfo))));
	}

	public void Emit(OpCode opcode, VariableDefinition varDef)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, varDef)));
	}
}
