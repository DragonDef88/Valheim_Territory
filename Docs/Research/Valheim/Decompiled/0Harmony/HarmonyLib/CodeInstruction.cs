using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

public class CodeInstruction
{
	public OpCode opcode;

	public object operand;

	public List<Label> labels = new List<Label>();

	public List<ExceptionBlock> blocks = new List<ExceptionBlock>();

	internal CodeInstruction()
	{
	}

	public CodeInstruction(OpCode opcode, object operand = null)
	{
		this.opcode = opcode;
		this.operand = operand;
	}

	public CodeInstruction(CodeInstruction instruction)
	{
		opcode = instruction.opcode;
		operand = instruction.operand;
		labels = instruction.labels.ToList();
		blocks = instruction.blocks.ToList();
	}

	public CodeInstruction Clone()
	{
		return new CodeInstruction(this)
		{
			labels = new List<Label>(),
			blocks = new List<ExceptionBlock>()
		};
	}

	public CodeInstruction Clone(OpCode opcode)
	{
		CodeInstruction codeInstruction = Clone();
		codeInstruction.opcode = opcode;
		return codeInstruction;
	}

	public CodeInstruction Clone(object operand)
	{
		CodeInstruction codeInstruction = Clone();
		codeInstruction.operand = operand;
		return codeInstruction;
	}

	public static CodeInstruction Call(Type type, string name, Type[] parameters = null, Type[] generics = null)
	{
		MethodInfo methodInfo = AccessTools.Method(type, name, parameters, generics);
		if ((object)methodInfo == null)
		{
			throw new ArgumentException($"No method found for type={type}, name={name}, parameters={parameters.Description()}, generics={generics.Description()}");
		}
		return new CodeInstruction(OpCodes.Call, methodInfo);
	}

	public static CodeInstruction Call(string typeColonMethodname, Type[] parameters = null, Type[] generics = null)
	{
		MethodInfo methodInfo = AccessTools.Method(typeColonMethodname, parameters, generics);
		if ((object)methodInfo == null)
		{
			throw new ArgumentException("No method found for " + typeColonMethodname + ", parameters=" + parameters.Description() + ", generics=" + generics.Description());
		}
		return new CodeInstruction(OpCodes.Call, methodInfo);
	}

	public static CodeInstruction Call(Expression<Action> expression)
	{
		return new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(expression));
	}

	public static CodeInstruction Call<T>(Expression<Action<T>> expression)
	{
		return new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(expression));
	}

	public static CodeInstruction Call<T, TResult>(Expression<Func<T, TResult>> expression)
	{
		return new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(expression));
	}

	public static CodeInstruction Call(LambdaExpression expression)
	{
		return new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(expression));
	}

	public static CodeInstruction CallClosure<T>(T closure) where T : Delegate
	{
		return Transpilers.EmitDelegate(closure);
	}

	public static CodeInstruction LoadField(Type type, string name, bool useAddress = false)
	{
		FieldInfo fieldInfo = AccessTools.Field(type, name);
		if ((object)fieldInfo == null)
		{
			throw new ArgumentException($"No field found for {type} and {name}");
		}
		return new CodeInstruction((!useAddress) ? (fieldInfo.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld) : (fieldInfo.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda), fieldInfo);
	}

	public static CodeInstruction StoreField(Type type, string name)
	{
		FieldInfo fieldInfo = AccessTools.Field(type, name);
		if ((object)fieldInfo == null)
		{
			throw new ArgumentException($"No field found for {type} and {name}");
		}
		return new CodeInstruction(fieldInfo.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldInfo);
	}

	public override string ToString()
	{
		List<string> list = new List<string>();
		foreach (Label label in labels)
		{
			list.Add($"Label{label.GetHashCode()}");
		}
		foreach (ExceptionBlock block in blocks)
		{
			list.Add("EX_" + block.blockType.ToString().Replace("Block", ""));
		}
		string text = ((list.Count > 0) ? (" [" + string.Join(", ", list.ToArray()) + "]") : "");
		string text2 = FormatArgument(operand);
		if (text2.Length > 0)
		{
			text2 = " " + text2;
		}
		OpCode opCode = opcode;
		return opCode.ToString() + text2 + text;
	}

	internal static string FormatArgument(object argument, string extra = null)
	{
		if (argument == null)
		{
			return "NULL";
		}
		Type type = argument.GetType();
		if (argument is MethodBase member)
		{
			return member.FullDescription() + ((extra != null) ? (" " + extra) : "");
		}
		if (argument is FieldInfo fieldInfo)
		{
			return fieldInfo.FieldType.FullDescription() + " " + fieldInfo.DeclaringType.FullDescription() + "::" + fieldInfo.Name;
		}
		if ((object)type == typeof(Label))
		{
			return $"Label{((Label)argument).GetHashCode()}";
		}
		if ((object)type == typeof(Label[]))
		{
			return "Labels" + string.Join(",", ((Label[])argument).Select((Label l) => l.GetHashCode().ToString()).ToArray());
		}
		if ((object)type == typeof(LocalBuilder))
		{
			return $"{((LocalBuilder)argument).LocalIndex} ({((LocalBuilder)argument).LocalType})";
		}
		if ((object)type == typeof(string))
		{
			return argument.ToString().ToLiteral();
		}
		return argument.ToString().Trim();
	}
}
