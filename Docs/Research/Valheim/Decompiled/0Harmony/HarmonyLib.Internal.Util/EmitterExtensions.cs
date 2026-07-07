using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using MonoMod.Utils.Cil;

namespace HarmonyLib.Internal.Util;

internal static class EmitterExtensions
{
	private static DynamicMethodDefinition emitDMD;

	private static MethodInfo emitDMDMethod;

	private static Action<CecilILGenerator, OpCode, object> emitCodeDelegate;

	private static Dictionary<Type, OpCode> storeOpCodes;

	private static Dictionary<Type, OpCode> loadOpCodes;

	private static readonly ConstructorInfo c_LocalBuilder;

	private static readonly FieldInfo f_LocalBuilder_position;

	private static readonly FieldInfo f_LocalBuilder_is_pinned;

	private static int c_LocalBuilder_params;

	[MethodImpl(MethodImplOptions.Synchronized)]
	static EmitterExtensions()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		storeOpCodes = new Dictionary<Type, OpCode>
		{
			[typeof(sbyte)] = OpCodes.Stind_I1,
			[typeof(byte)] = OpCodes.Stind_I1,
			[typeof(short)] = OpCodes.Stind_I2,
			[typeof(ushort)] = OpCodes.Stind_I2,
			[typeof(int)] = OpCodes.Stind_I4,
			[typeof(uint)] = OpCodes.Stind_I4,
			[typeof(long)] = OpCodes.Stind_I8,
			[typeof(ulong)] = OpCodes.Stind_I8,
			[typeof(float)] = OpCodes.Stind_R4,
			[typeof(double)] = OpCodes.Stind_R8
		};
		loadOpCodes = new Dictionary<Type, OpCode>
		{
			[typeof(sbyte)] = OpCodes.Ldind_I1,
			[typeof(byte)] = OpCodes.Ldind_I1,
			[typeof(short)] = OpCodes.Ldind_I2,
			[typeof(ushort)] = OpCodes.Ldind_I2,
			[typeof(int)] = OpCodes.Ldind_I4,
			[typeof(uint)] = OpCodes.Ldind_I4,
			[typeof(long)] = OpCodes.Ldind_I8,
			[typeof(ulong)] = OpCodes.Ldind_I8,
			[typeof(float)] = OpCodes.Ldind_R4,
			[typeof(double)] = OpCodes.Ldind_R8
		};
		c_LocalBuilder = (from c in typeof(LocalBuilder).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			orderby c.GetParameters().Length descending
			select c).First();
		f_LocalBuilder_position = typeof(LocalBuilder).GetField("position", BindingFlags.Instance | BindingFlags.NonPublic);
		f_LocalBuilder_is_pinned = typeof(LocalBuilder).GetField("is_pinned", BindingFlags.Instance | BindingFlags.NonPublic);
		c_LocalBuilder_params = c_LocalBuilder.GetParameters().Length;
		if (emitDMD == null)
		{
			InitEmitterHelperDMD();
		}
	}

	public static OpCode GetCecilStoreOpCode(this Type t)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (t.IsEnum)
		{
			return OpCodes.Stind_I4;
		}
		if (!storeOpCodes.TryGetValue(t, out var value))
		{
			return OpCodes.Stind_Ref;
		}
		return value;
	}

	public static OpCode GetCecilLoadOpCode(this Type t)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (t.IsEnum)
		{
			return OpCodes.Ldind_I4;
		}
		if (!loadOpCodes.TryGetValue(t, out var value))
		{
			return OpCodes.Ldind_Ref;
		}
		return value;
	}

	public static Type OpenRefType(this Type t)
	{
		if (t.IsByRef)
		{
			return t.GetElementType();
		}
		return t;
	}

	private static void InitEmitterHelperDMD()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		emitDMD = new DynamicMethodDefinition("EmitOpcodeWithOperand", typeof(void), new Type[3]
		{
			typeof(CecilILGenerator),
			typeof(OpCode),
			typeof(object)
		});
		ILGenerator iLGenerator = emitDMD.GetILGenerator();
		Label label = iLGenerator.DefineLabel();
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.Emit(OpCodes.Brtrue, label);
		iLGenerator.Emit(OpCodes.Ldstr, "Provided operand is null!");
		iLGenerator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[1] { typeof(string) }));
		iLGenerator.Emit(OpCodes.Throw);
		foreach (MethodInfo item in from m in typeof(CecilILGenerator).GetMethods()
			where m.Name == "Emit"
			select m)
		{
			ParameterInfo[] parameters = item.GetParameters();
			if (parameters.Length != 2)
			{
				continue;
			}
			Type[] array = parameters.Select((ParameterInfo p) => p.ParameterType).ToArray();
			if ((object)array[0] == typeof(OpCode))
			{
				Type type = array[1];
				iLGenerator.MarkLabel(label);
				label = iLGenerator.DefineLabel();
				iLGenerator.Emit(OpCodes.Ldarg_2);
				iLGenerator.Emit(OpCodes.Isinst, type);
				iLGenerator.Emit(OpCodes.Brfalse, label);
				iLGenerator.Emit(OpCodes.Ldarg_2);
				if (type.IsValueType)
				{
					iLGenerator.Emit(OpCodes.Unbox_Any, type);
				}
				LocalBuilder local = iLGenerator.DeclareLocal(type);
				iLGenerator.Emit(OpCodes.Stloc, local);
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Ldloc, local);
				iLGenerator.Emit(OpCodes.Callvirt, item);
				iLGenerator.Emit(OpCodes.Ret);
			}
		}
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ldstr, "The operand is none of the supported types!");
		iLGenerator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[1] { typeof(string) }));
		iLGenerator.Emit(OpCodes.Throw);
		iLGenerator.Emit(OpCodes.Ret);
		emitDMDMethod = emitDMD.Generate();
		emitCodeDelegate = Extensions.CreateDelegate<Action<CecilILGenerator, OpCode, object>>((MethodBase)emitDMDMethod);
	}

	public static void Emit(this CecilILGenerator il, OpCode opcode, object operand)
	{
		emitCodeDelegate(il, opcode, operand);
	}

	public static void MarkBlockBefore(this CecilILGenerator il, ExceptionBlock block)
	{
		switch (block.blockType)
		{
		case ExceptionBlockType.BeginExceptionBlock:
			((ILGeneratorShim)il).BeginExceptionBlock();
			break;
		case ExceptionBlockType.BeginCatchBlock:
			((ILGeneratorShim)il).BeginCatchBlock(block.catchType);
			break;
		case ExceptionBlockType.BeginExceptFilterBlock:
			((ILGeneratorShim)il).BeginExceptFilterBlock();
			break;
		case ExceptionBlockType.BeginFaultBlock:
			((ILGeneratorShim)il).BeginFaultBlock();
			break;
		case ExceptionBlockType.BeginFinallyBlock:
			((ILGeneratorShim)il).BeginFinallyBlock();
			break;
		case ExceptionBlockType.EndExceptionBlock:
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static void MarkBlockAfter(this CecilILGenerator il, ExceptionBlock block)
	{
		if (block.blockType == ExceptionBlockType.EndExceptionBlock)
		{
			((ILGeneratorShim)il).EndExceptionBlock();
		}
	}

	public static LocalBuilder GetLocal(this CecilILGenerator il, VariableDefinition varDef)
	{
		Dictionary<LocalBuilder, VariableDefinition> dictionary = (Dictionary<LocalBuilder, VariableDefinition>)AccessTools.Field(typeof(CecilILGenerator), "_Variables").GetValue(il);
		LocalBuilder key = dictionary.FirstOrDefault((KeyValuePair<LocalBuilder, VariableDefinition> kv) => kv.Value == varDef).Key;
		if (key != null)
		{
			return key;
		}
		Type type = ReflectionHelper.ResolveReflection(((VariableReference)varDef).VariableType);
		bool isPinned = ((VariableReference)varDef).VariableType.IsPinned;
		int index = ((VariableReference)varDef).Index;
		object obj;
		if (c_LocalBuilder_params != 4)
		{
			if (c_LocalBuilder_params != 3)
			{
				if (c_LocalBuilder_params != 2)
				{
					if (c_LocalBuilder_params != 0)
					{
						throw new NotSupportedException();
					}
					obj = c_LocalBuilder.Invoke(new object[0]);
				}
				else
				{
					obj = c_LocalBuilder.Invoke(new object[2] { type, null });
				}
			}
			else
			{
				obj = c_LocalBuilder.Invoke(new object[3] { index, type, null });
			}
		}
		else
		{
			obj = c_LocalBuilder.Invoke(new object[4] { index, type, null, isPinned });
		}
		key = (LocalBuilder)obj;
		f_LocalBuilder_position?.SetValue(key, (ushort)index);
		f_LocalBuilder_is_pinned?.SetValue(key, isPinned);
		dictionary[key] = varDef;
		return key;
	}
}
