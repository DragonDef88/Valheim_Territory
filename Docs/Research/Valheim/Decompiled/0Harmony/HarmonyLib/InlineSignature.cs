using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Cecil;
using MonoMod.Utils;

namespace HarmonyLib;

public class InlineSignature : ICallSiteGenerator
{
	public class ModifierType
	{
		public bool IsOptional;

		public Type Modifier;

		public object Type;

		public override string ToString()
		{
			return ((Type is Type type) ? type.FullDescription() : Type?.ToString()) + " mod" + (IsOptional ? "opt" : "req") + "(" + Modifier?.FullDescription() + ")";
		}

		internal TypeReference ToTypeReference(ModuleDefinition module)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			if (!IsOptional)
			{
				return (TypeReference)new RequiredModifierType(module.ImportReference(Modifier), GetTypeReference(module, Type));
			}
			return (TypeReference)new OptionalModifierType(module.ImportReference(Modifier), GetTypeReference(module, Type));
		}
	}

	public bool HasThis { get; set; }

	public bool ExplicitThis { get; set; }

	public CallingConvention CallingConvention { get; set; } = CallingConvention.Winapi;


	public List<object> Parameters { get; set; } = new List<object>();


	public object ReturnType { get; set; } = typeof(void);


	public override string ToString()
	{
		return ((ReturnType is Type type) ? type.FullDescription() : ReturnType?.ToString()) + " (" + Parameters.Join((object p) => (!(p is Type type2)) ? p?.ToString() : type2.FullDescription()) + ")";
	}

	internal static TypeReference GetTypeReference(ModuleDefinition module, object param)
	{
		if (!(param is Type type))
		{
			if (!(param is InlineSignature inlineSignature))
			{
				if (param is ModifierType modifierType)
				{
					return modifierType.ToTypeReference(module);
				}
				throw new NotSupportedException($"Unsupported inline signature parameter type: {param} ({param?.GetType().FullDescription()})");
			}
			return (TypeReference)(object)inlineSignature.ToFunctionPointer(module);
		}
		return module.ImportReference(type);
	}

	CallSite ICallSiteGenerator.ToCallSite(ModuleDefinition module)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		CallSite val = new CallSite(GetTypeReference(module, ReturnType))
		{
			HasThis = HasThis,
			ExplicitThis = ExplicitThis,
			CallingConvention = (MethodCallingConvention)(byte)((byte)CallingConvention - 1)
		};
		foreach (object parameter in Parameters)
		{
			val.Parameters.Add(new ParameterDefinition(GetTypeReference(module, parameter)));
		}
		return val;
	}

	private FunctionPointerType ToFunctionPointer(ModuleDefinition module)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		FunctionPointerType val = new FunctionPointerType
		{
			ReturnType = GetTypeReference(module, ReturnType),
			HasThis = HasThis,
			ExplicitThis = ExplicitThis,
			CallingConvention = (MethodCallingConvention)(byte)((byte)CallingConvention - 1)
		};
		foreach (object parameter in Parameters)
		{
			val.Parameters.Add(new ParameterDefinition(GetTypeReference(module, parameter)));
		}
		return val;
	}
}
