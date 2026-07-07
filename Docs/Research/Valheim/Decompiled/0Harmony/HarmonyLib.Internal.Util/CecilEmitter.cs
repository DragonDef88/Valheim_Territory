using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using HarmonyLib.Tools;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Utils;

namespace HarmonyLib.Internal.Util;

internal static class CecilEmitter
{
	private static readonly ConstructorInfo UnverifiableCodeAttributeConstructor = typeof(UnverifiableCodeAttribute).GetConstructor(Type.EmptyTypes);

	public static void Dump(MethodDefinition md, IEnumerable<string> dumpPaths, MethodBase original = null)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Expected O, but got Unknown
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Expected O, but got Unknown
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Expected O, but got Unknown
		string text = string.Format("HarmonyDump.{0}.{1:X8}", Extensions.GetID((MethodReference)(object)md, (string)null, (string)null, false, true).Replace(":", "_").Replace(" ", "_"), Guid.NewGuid().GetHashCode());
		string arg = (original?.Name ?? ((MemberReference)md).Name).Replace('.', '_');
		ModuleDefinition module = ModuleDefinition.CreateModule(text, new ModuleParameters
		{
			Kind = (ModuleKind)0,
			ReflectionImporterProvider = MMReflectionImporter.ProviderNoDefault
		});
		try
		{
			module.Assembly.CustomAttributes.Add(new CustomAttribute(module.ImportReference((MethodBase)UnverifiableCodeAttributeConstructor)));
			int hashCode = Guid.NewGuid().GetHashCode();
			TypeDefinition val = new TypeDefinition("", $"HarmonyDump<{arg}>?{hashCode}", (TypeAttributes)385)
			{
				BaseType = module.TypeSystem.Object
			};
			module.Types.Add(val);
			MethodDefinition clone = null;
			TypeReference val2 = new TypeReference("System.Runtime.CompilerServices", "IsVolatile", module, module.TypeSystem.CoreLibrary);
			Relinker val3 = (Relinker)((IMetadataTokenProvider mtp, IGenericParameterProvider _) => (IMetadataTokenProvider)((mtp != md) ? ((object)Extensions.ImportReference(module, mtp)) : ((object)clone)));
			clone = new MethodDefinition(original?.Name ?? ("_" + ((MemberReference)md).Name.Replace(".", "_")), md.Attributes, module.TypeSystem.Void)
			{
				MethodReturnType = ((MethodReference)md).MethodReturnType,
				Attributes = (MethodAttributes)150,
				ImplAttributes = (MethodImplAttributes)0,
				DeclaringType = val,
				HasThis = false
			};
			val.Methods.Add(clone);
			Enumerator<ParameterDefinition> enumerator = ((MethodReference)md).Parameters.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ParameterDefinition current = enumerator.Current;
					((MethodReference)clone).Parameters.Add(Extensions.Relink(Extensions.Clone(current), val3, (IGenericParameterProvider)(object)clone));
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			((MethodReference)clone).ReturnType = Extensions.Relink(((MethodReference)md).ReturnType, val3, (IGenericParameterProvider)(object)clone);
			MethodBody val5 = (clone.Body = Extensions.Clone(md.Body, clone));
			MethodBody val6 = val5;
			Enumerator<VariableDefinition> enumerator2 = clone.Body.Variables.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					VariableDefinition current2 = enumerator2.Current;
					((VariableReference)current2).VariableType = Extensions.Relink(((VariableReference)current2).VariableType, val3, (IGenericParameterProvider)(object)clone);
				}
			}
			finally
			{
				((IDisposable)enumerator2).Dispose();
			}
			foreach (ExceptionHandler item in ((IEnumerable<ExceptionHandler>)clone.Body.ExceptionHandlers).Where((ExceptionHandler handler) => handler.CatchType != null))
			{
				item.CatchType = Extensions.Relink(item.CatchType, val3, (IGenericParameterProvider)(object)clone);
			}
			Enumerator<Instruction> enumerator4 = val6.Instructions.GetEnumerator();
			try
			{
				while (enumerator4.MoveNext())
				{
					Instruction current4 = enumerator4.Current;
					object operand = current4.Operand;
					ParameterDefinition val7 = (ParameterDefinition)((operand is ParameterDefinition) ? operand : null);
					object obj;
					if (val7 == null)
					{
						IMetadataTokenProvider val8 = (IMetadataTokenProvider)((operand is IMetadataTokenProvider) ? operand : null);
						obj = ((val8 == null) ? operand : Extensions.Relink(val8, val3, (IGenericParameterProvider)(object)clone));
					}
					else
					{
						obj = ((MethodReference)clone).Parameters[((ParameterReference)val7).Index];
					}
					operand = obj;
					Instruction previous = current4.Previous;
					OpCode? val9 = ((previous != null) ? new OpCode?(previous.OpCode) : null);
					OpCode @volatile = OpCodes.Volatile;
					if (val9.HasValue && (!val9.HasValue || val9.GetValueOrDefault() == @volatile))
					{
						FieldReference val10 = (FieldReference)((operand is FieldReference) ? operand : null);
						if (val10 != null)
						{
							TypeReference fieldType = val10.FieldType;
							TypeReference obj2 = ((fieldType is RequiredModifierType) ? fieldType : null);
							if (((obj2 != null) ? ((RequiredModifierType)obj2).ModifierType : null) != val2)
							{
								val10.FieldType = (TypeReference)new RequiredModifierType(val2, val10.FieldType);
							}
						}
					}
					current4.Operand = operand;
				}
			}
			finally
			{
				((IDisposable)enumerator4).Dispose();
			}
			if (((MethodReference)md).HasThis)
			{
				TypeReference val11 = (TypeReference)(object)md.DeclaringType;
				if (val11.IsValueType)
				{
					val11 = (TypeReference)new ByReferenceType(val11);
				}
				((MethodReference)clone).Parameters.Insert(0, new ParameterDefinition("<>_this", (ParameterAttributes)0, Extensions.Relink(val11, val3, (IGenericParameterProvider)(object)clone)));
			}
			foreach (string dumpPath in dumpPaths)
			{
				string fullPath = Path.GetFullPath(dumpPath);
				try
				{
					Directory.CreateDirectory(fullPath);
					using FileStream fileStream = File.OpenWrite(Path.Combine(fullPath, ((ModuleReference)module).Name + ".dll"));
					module.Write((Stream)fileStream);
				}
				catch (Exception ex)
				{
					Exception e = ex;
					Logger.Log(Logger.LogChannel.Error, () => $"Failed to dump {Extensions.GetID((MethodReference)(object)md, (string)null, (string)null, true, true)} to {fullPath}: {e}");
				}
			}
		}
		finally
		{
			if (module != null)
			{
				((IDisposable)module).Dispose();
			}
		}
	}
}
