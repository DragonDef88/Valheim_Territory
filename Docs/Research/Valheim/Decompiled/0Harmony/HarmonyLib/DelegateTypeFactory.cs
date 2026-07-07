using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;
using MonoMod.Utils;

namespace HarmonyLib;

public class DelegateTypeFactory
{
	private class DelegateEntry
	{
		public CallingConvention? callingConvention;

		public Type delegateType;
	}

	private static int counter;

	private static readonly Dictionary<MethodInfo, List<DelegateEntry>> TypeCache = new Dictionary<MethodInfo, List<DelegateEntry>>();

	private static readonly MethodBase CallingConvAttr = AccessTools.Constructor(typeof(UnmanagedFunctionPointerAttribute), new Type[1] { typeof(CallingConvention) });

	public static readonly DelegateTypeFactory instance = new DelegateTypeFactory();

	public Type CreateDelegateType(Type returnType, Type[] argTypes)
	{
		return CreateDelegateType(returnType, argTypes, null);
	}

	public Type CreateDelegateType(Type returnType, Type[] argTypes, CallingConvention? convention)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		counter++;
		AssemblyDefinition val = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition($"HarmonyDTFAssembly{counter}", new Version(1, 0)), $"HarmonyDTFModule{counter}", (ModuleKind)0);
		ModuleDefinition module = val.MainModule;
		TypeDefinition val2 = new TypeDefinition("", $"HarmonyDTFType{counter}", (TypeAttributes)257)
		{
			BaseType = module.ImportReference(typeof(MulticastDelegate))
		};
		module.Types.Add(val2);
		if (convention.HasValue)
		{
			CustomAttribute val3 = new CustomAttribute(module.ImportReference(CallingConvAttr));
			val3.ConstructorArguments.Add(new CustomAttributeArgument(module.ImportReference(typeof(CallingConvention)), (object)convention.Value));
			val2.CustomAttributes.Add(val3);
		}
		MethodDefinition val4 = new MethodDefinition(".ctor", (MethodAttributes)4230, module.ImportReference(typeof(void)))
		{
			ImplAttributes = (MethodImplAttributes)3
		};
		Extensions.AddRange<ParameterDefinition>(((MethodReference)val4).Parameters, (IEnumerable<ParameterDefinition>)(object)new ParameterDefinition[2]
		{
			new ParameterDefinition(module.ImportReference(typeof(object))),
			new ParameterDefinition(module.ImportReference(typeof(IntPtr)))
		});
		val2.Methods.Add(val4);
		MethodDefinition val5 = new MethodDefinition("Invoke", (MethodAttributes)198, module.ImportReference(returnType))
		{
			ImplAttributes = (MethodImplAttributes)3
		};
		Extensions.AddRange<ParameterDefinition>(((MethodReference)val5).Parameters, ((IEnumerable<Type>)argTypes).Select((Func<Type, ParameterDefinition>)((Type t) => new ParameterDefinition(module.ImportReference(t)))));
		val2.Methods.Add(val5);
		return ReflectionHelper.Load(val.MainModule).GetType($"HarmonyDTFType{counter}");
	}

	public Type CreateDelegateType(MethodInfo method)
	{
		return CreateDelegateType(method, null);
	}

	public Type CreateDelegateType(MethodInfo method, CallingConvention? convention)
	{
		DelegateEntry delegateEntry;
		if (TypeCache.TryGetValue(method, out var value) && (delegateEntry = value.FirstOrDefault((DelegateEntry e) => e.callingConvention == convention)) != null)
		{
			return delegateEntry.delegateType;
		}
		if (value == null)
		{
			value = (TypeCache[method] = new List<DelegateEntry>());
		}
		delegateEntry = new DelegateEntry
		{
			delegateType = CreateDelegateType(method.ReturnType, method.GetParameters().Types().ToArray(), convention),
			callingConvention = convention
		};
		value.Add(delegateEntry);
		return delegateEntry.delegateType;
	}
}
