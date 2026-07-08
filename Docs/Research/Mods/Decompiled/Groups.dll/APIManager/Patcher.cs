using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Preloader;
using BepInEx.Preloader.Patching;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using UnityEngine;

namespace APIManager;

internal static class Patcher
{
	private class MonoAssemblyResolver : IAssemblyResolver, IDisposable
	{
		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			return AssemblyDefinition.ReadAssembly(AppDomain.CurrentDomain.Load(name.FullName).Location);
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			return Resolve(name);
		}

		public void Dispose()
		{
		}
	}

	private class AssemblyLoadInterceptor
	{
		private static string? assemblyPath;

		private static MethodInfo TargetMethod()
		{
			return AccessTools.DeclaredMethod(typeof(Assembly), "Load", new Type[1] { typeof(byte[]) }, (Type[])null);
		}

		private static bool Prefix(ref byte[] __0, ref Assembly? __result)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Expected O, but got Unknown
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Expected O, but got Unknown
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Expected O, but got Unknown
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Expected O, but got Unknown
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Expected O, but got Unknown
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Expected O, but got Unknown
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			assemblyPath = null;
			if (modifyNextLoad)
			{
				modifyNextLoad = false;
				try
				{
					AssemblyDefinition val = AssemblyDefinition.ReadAssembly((Stream)new MemoryStream(__0), new ReaderParameters
					{
						AssemblyResolver = (IAssemblyResolver)(object)new MonoAssemblyResolver()
					});
					try
					{
						((Dictionary<string, string>)typeof(EnvVars).Assembly.GetType("BepInEx.Preloader.RuntimeFixes.UnityPatches").GetProperty("AssemblyLocations").GetValue(null))[val.FullName] = currentAssemblyPath;
						FixupModuleReferences(val.MainModule);
						TypeDefinition val2 = val.MainModule.GetType("APIManager", "PatchedAttribute");
						if (val2 == null)
						{
							val2 = new TypeDefinition("APIManager", "PatchedAttribute", (TypeAttributes)3)
							{
								BaseType = val.MainModule.ImportReference(typeof(Attribute))
							};
							MethodDefinition val3 = new MethodDefinition(".ctor", (MethodAttributes)6278, val.MainModule.TypeSystem.Void);
							((MethodReference)val3).Parameters.Add(new ParameterDefinition(val.MainModule.TypeSystem.String));
							((MethodReference)val3).Parameters.Add(new ParameterDefinition(val.MainModule.TypeSystem.String));
							val3.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
							val2.Methods.Add(val3);
							val.MainModule.Types.Add(val2);
							CustomAttribute val4 = new CustomAttribute((MethodReference)(object)val3);
							val4.ConstructorArguments.Add(new CustomAttributeArgument(val.MainModule.TypeSystem.String, (object)""));
							val4.ConstructorArguments.Add(new CustomAttributeArgument(val.MainModule.TypeSystem.String, (object)val.MainModule.Mvid.ToString()));
							val.CustomAttributes.Add(val4);
						}
						CustomAttribute val5 = new CustomAttribute((MethodReference)(object)((IEnumerable<MethodDefinition>)val2.Methods).First((MethodDefinition m) => ((MemberReference)m).Name == ".ctor"));
						val5.ConstructorArguments.Add(new CustomAttributeArgument(val.MainModule.TypeSystem.String, (object)modGUID));
						val5.ConstructorArguments.Add(new CustomAttributeArgument(val.MainModule.TypeSystem.String, (object)patchingAssembly.ManifestModule.ModuleVersionId.ToString()));
						val.CustomAttributes.Add(val5);
						using MemoryStream memoryStream = new MemoryStream();
						val.Write((Stream)memoryStream);
						__0 = memoryStream.ToArray();
						string dumpedAssembliesPath = Patcher.dumpedAssembliesPath;
						char directorySeparatorChar = Path.DirectorySeparatorChar;
						string path = dumpedAssembliesPath + directorySeparatorChar + ((AssemblyNameReference)val.Name).Name + ".dll";
						Directory.CreateDirectory(Patcher.dumpedAssembliesPath);
						File.WriteAllBytes(path, __0);
						assemblyPath = path;
						__result = null;
						return false;
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
				catch (BadImageFormatException)
				{
				}
				catch (Exception ex2)
				{
					Debug.LogError((object)("Failed patching ... " + ex2));
				}
			}
			return true;
		}

		private static void Postfix(ref Assembly? __result)
		{
			if (assemblyPath != null && __result == null)
			{
				__result = Assembly.LoadFrom(assemblyPath);
				assemblyPath = null;
			}
		}
	}

	private static PluginInfo? lastPluginInfo;

	private static bool modifyNextLoad = false;

	private static string modGUID = null;

	private static HashSet<string> redirectedNamespaces = null;

	private static readonly Assembly patchingAssembly = Assembly.GetExecutingAssembly();

	private static string currentAssemblyPath = null;

	private static readonly string dumpedAssembliesPath = (string)AccessTools.DeclaredField(typeof(AssemblyPatcher), "DumpedAssembliesPath").GetValue(null);

	private static void GrabPluginInfo(PluginInfo __instance)
	{
		lastPluginInfo = __instance;
	}

	[HarmonyPriority(700)]
	private static void CheckAssemblyLoadFile(string __0)
	{
		PluginInfo? obj = lastPluginInfo;
		if (__0 == ((obj != null) ? obj.Location : null) && lastPluginInfo.Dependencies.Any((BepInDependency d) => d.DependencyGUID == modGUID))
		{
			modifyNextLoad = true;
			currentAssemblyPath = __0;
		}
		lastPluginInfo = null;
	}

	[HarmonyPriority(500)]
	private static bool InterceptAssemblyLoadFile(string __0, ref Assembly? __result)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (modifyNextLoad && (object)__result == null)
		{
			string text = dumpedAssembliesPath;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string text2 = text + directorySeparatorChar + Path.GetFileName(__0);
			if (File.Exists(text2))
			{
				AssemblyDefinition val = AssemblyDefinition.ReadAssembly(text2, new ReaderParameters
				{
					AssemblyResolver = (IAssemblyResolver)(object)new MonoAssemblyResolver()
				});
				try
				{
					bool flag = false;
					Enumerator<CustomAttribute> enumerator = val.CustomAttributes.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							CustomAttribute current = enumerator.Current;
							TypeReference declaringType = ((MemberReference)current.Constructor).DeclaringType;
							if (declaringType.Namespace == "APIManager" && ((MemberReference)declaringType).Name == "PatchedAttribute")
							{
								CustomAttributeArgument val2 = current.ConstructorArguments[0];
								if ((string)((CustomAttributeArgument)(ref val2)).Value == modGUID)
								{
									val2 = current.ConstructorArguments[1];
									flag = (string)((CustomAttributeArgument)(ref val2)).Value == patchingAssembly.ManifestModule.ModuleVersionId.ToString();
									break;
								}
							}
						}
					}
					finally
					{
						((IDisposable)enumerator).Dispose();
					}
					if (flag)
					{
						return true;
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			__result = Assembly.Load(File.ReadAllBytes(__0));
			return false;
		}
		return true;
	}

	[HarmonyPriority(499)]
	private static bool ReplaceAssemblyLoadWithCache(ref string __0, ref Assembly? __result)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (modifyNextLoad && (object)__result == null && !__0.StartsWith(dumpedAssembliesPath))
		{
			string text = dumpedAssembliesPath;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string text2 = text + directorySeparatorChar + Path.GetFileName(__0);
			AssemblyDefinition val = AssemblyDefinition.ReadAssembly(text2, new ReaderParameters
			{
				AssemblyResolver = (IAssemblyResolver)(object)new MonoAssemblyResolver()
			});
			AssemblyDefinition val2 = AssemblyDefinition.ReadAssembly(__0, new ReaderParameters
			{
				AssemblyResolver = (IAssemblyResolver)(object)new MonoAssemblyResolver()
			});
			Enumerator<CustomAttribute> enumerator = val.CustomAttributes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CustomAttribute current = enumerator.Current;
					TypeReference declaringType = ((MemberReference)current.Constructor).DeclaringType;
					if (declaringType.Namespace == "APIManager" && ((MemberReference)declaringType).Name == "PatchedAttribute")
					{
						CustomAttributeArgument val3 = current.ConstructorArguments[0];
						string text3 = (string)((CustomAttributeArgument)(ref val3)).Value;
						bool num;
						if (!(text3 == ""))
						{
							num = !Chainloader.PluginInfos.ContainsKey(text3);
						}
						else
						{
							val3 = current.ConstructorArguments[1];
							num = (string)((CustomAttributeArgument)(ref val3)).Value != val2.MainModule.Mvid.ToString();
						}
						if (num)
						{
							val2.Dispose();
							val.Dispose();
							__result = Assembly.Load(File.ReadAllBytes(__0));
							return false;
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			((Dictionary<string, string>)typeof(EnvVars).Assembly.GetType("BepInEx.Preloader.RuntimeFixes.UnityPatches").GetProperty("AssemblyLocations").GetValue(null))[val.FullName] = __0;
			val2.Dispose();
			val.Dispose();
			__0 = text2;
		}
		modifyNextLoad = false;
		return true;
	}

	private static void FixupModuleReferences(ModuleDefinition module)
	{
		ModuleDefinition module2 = module;
		foreach (TypeDefinition type4 in module2.GetTypes())
		{
			if ((object)patchingAssembly.GetType(((MemberReference)type4).FullName) == null)
			{
				Dispatch(type4);
			}
		}
		static bool AreSame(TypeReference a, TypeReference b)
		{
			return (bool)typeof(MetadataResolver).Assembly.GetType("Mono.Cecil.MetadataResolver").GetMethod("AreSame", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(TypeReference),
				typeof(TypeReference)
			}, null).Invoke(null, new object[2] { a, b });
		}
		void Dispatch(TypeDefinition type)
		{
			if (type.BaseType != null && type.BaseType.Scope == module2 && redirectedNamespaces.Contains(baseDeclaringType(type.BaseType).Namespace))
			{
				Type type3 = patchingAssembly.GetType(((MemberReference)type.BaseType).FullName);
				if ((object)type3 != null)
				{
					type.BaseType = module2.ImportReference(type3);
				}
			}
			DispatchGenericParameters((IGenericParameterProvider)(object)type, ((MemberReference)type).FullName);
			DispatchInterfaces(type, ((MemberReference)type).FullName);
			DispatchAttributes((ICustomAttributeProvider)(object)type, ((MemberReference)type).FullName);
			DispatchFields(type, ((MemberReference)type).FullName);
			DispatchProperties(type, ((MemberReference)type).FullName);
			DispatchEvents(type, ((MemberReference)type).FullName);
			DispatchMethods(type);
		}
		void DispatchAttributes(ICustomAttributeProvider provider, string referencingEntityName)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			if (!provider.HasCustomAttributes)
			{
				return;
			}
			Enumerator<CustomAttribute> enumerator11 = provider.CustomAttributes.GetEnumerator();
			try
			{
				while (enumerator11.MoveNext())
				{
					CustomAttribute current11 = enumerator11.Current;
					MethodReference val12 = importMethodReference(current11.Constructor);
					if (val12 != null)
					{
						current11.Constructor = val12;
					}
					else
					{
						VisitMethod(current11.Constructor, referencingEntityName);
					}
					for (int n = 0; n < current11.ConstructorArguments.Count; n++)
					{
						CustomAttributeArgument val13 = current11.ConstructorArguments[n];
						current11.ConstructorArguments[n] = new CustomAttributeArgument(VisitType(((CustomAttributeArgument)(ref val13)).Type, referencingEntityName), ((CustomAttributeArgument)(ref val13)).Value);
					}
					CustomAttributeArgument argument;
					for (int num = 0; num < current11.Properties.Count; num++)
					{
						CustomAttributeNamedArgument val14 = current11.Properties[num];
						Collection<CustomAttributeNamedArgument> properties = current11.Properties;
						int num2 = num;
						string name = ((CustomAttributeNamedArgument)(ref val14)).Name;
						argument = ((CustomAttributeNamedArgument)(ref val14)).Argument;
						TypeReference? obj2 = VisitType(((CustomAttributeArgument)(ref argument)).Type, referencingEntityName);
						argument = ((CustomAttributeNamedArgument)(ref val14)).Argument;
						properties[num2] = new CustomAttributeNamedArgument(name, new CustomAttributeArgument(obj2, ((CustomAttributeArgument)(ref argument)).Value));
					}
					for (int num3 = 0; num3 < current11.Fields.Count; num3++)
					{
						CustomAttributeNamedArgument val15 = current11.Fields[num3];
						Collection<CustomAttributeNamedArgument> fields = current11.Fields;
						int num4 = num3;
						string name2 = ((CustomAttributeNamedArgument)(ref val15)).Name;
						argument = ((CustomAttributeNamedArgument)(ref val15)).Argument;
						TypeReference? obj3 = VisitType(((CustomAttributeArgument)(ref argument)).Type, referencingEntityName);
						argument = ((CustomAttributeNamedArgument)(ref val15)).Argument;
						fields[num4] = new CustomAttributeNamedArgument(name2, new CustomAttributeArgument(obj3, ((CustomAttributeArgument)(ref argument)).Value));
					}
				}
			}
			finally
			{
				((IDisposable)enumerator11).Dispose();
			}
		}
		void DispatchEvents(TypeDefinition type, string referencingEntityName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<EventDefinition> enumerator6 = type.Events.GetEnumerator();
			try
			{
				while (enumerator6.MoveNext())
				{
					EventDefinition current6 = enumerator6.Current;
					((EventReference)current6).EventType = VisitType(((EventReference)current6).EventType, referencingEntityName);
					DispatchAttributes((ICustomAttributeProvider)(object)current6, referencingEntityName);
				}
			}
			finally
			{
				((IDisposable)enumerator6).Dispose();
			}
		}
		void DispatchFields(TypeDefinition type, string referencingEntityName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<FieldDefinition> enumerator8 = type.Fields.GetEnumerator();
			try
			{
				while (enumerator8.MoveNext())
				{
					FieldDefinition current8 = enumerator8.Current;
					((FieldReference)current8).FieldType = VisitType(((FieldReference)current8).FieldType, referencingEntityName);
					DispatchAttributes((ICustomAttributeProvider)(object)current8, referencingEntityName);
				}
			}
			finally
			{
				((IDisposable)enumerator8).Dispose();
			}
		}
		void DispatchGenericArguments(IGenericInstance genericInstance, string referencingEntityName)
		{
			for (int j = 0; j < genericInstance.GenericArguments.Count; j++)
			{
				genericInstance.GenericArguments[j] = VisitType(genericInstance.GenericArguments[j], referencingEntityName);
			}
		}
		void DispatchGenericParameters(IGenericParameterProvider provider, string referencingEntityName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<GenericParameter> enumerator12 = provider.GenericParameters.GetEnumerator();
			try
			{
				while (enumerator12.MoveNext())
				{
					GenericParameter current12 = enumerator12.Current;
					DispatchAttributes((ICustomAttributeProvider)(object)current12, referencingEntityName);
					for (int num5 = 0; num5 < current12.Constraints.Count; num5++)
					{
						current12.Constraints[num5] = VisitType(current12.Constraints[num5], referencingEntityName);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator12).Dispose();
			}
		}
		void DispatchInterfaces(TypeDefinition type, string referencingEntityName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<InterfaceImplementation> enumerator9 = type.Interfaces.GetEnumerator();
			try
			{
				while (enumerator9.MoveNext())
				{
					InterfaceImplementation current9 = enumerator9.Current;
					current9.InterfaceType = VisitType(current9.InterfaceType, referencingEntityName);
				}
			}
			finally
			{
				((IDisposable)enumerator9).Dispose();
			}
		}
		void DispatchMethod(MethodDefinition method)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			((MethodReference)method).ReturnType = VisitType(((MethodReference)method).ReturnType, ((MemberReference)method).FullName);
			DispatchAttributes((ICustomAttributeProvider)(object)((MethodReference)method).MethodReturnType, ((MemberReference)method).FullName);
			DispatchGenericParameters((IGenericParameterProvider)(object)method, ((MemberReference)method).FullName);
			Enumerator<ParameterDefinition> enumerator4 = ((MethodReference)method).Parameters.GetEnumerator();
			try
			{
				while (enumerator4.MoveNext())
				{
					ParameterDefinition current4 = enumerator4.Current;
					((ParameterReference)current4).ParameterType = VisitType(((ParameterReference)current4).ParameterType, ((MemberReference)method).FullName);
					DispatchAttributes((ICustomAttributeProvider)(object)current4, ((MemberReference)method).FullName);
				}
			}
			finally
			{
				((IDisposable)enumerator4).Dispose();
			}
			for (int i = 0; i < method.Overrides.Count; i++)
			{
				MethodReference val6 = importMethodReference(method.Overrides[i]);
				if (val6 != null)
				{
					method.Overrides[i] = val6;
				}
				else
				{
					VisitMethod(method.Overrides[i], ((MemberReference)method).FullName);
				}
			}
			if (method.HasBody)
			{
				DispatchMethodBody(method.Body);
			}
		}
		void DispatchMethodBody(MethodBody body)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<VariableDefinition> enumerator2 = body.Variables.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					VariableDefinition current2 = enumerator2.Current;
					((VariableReference)current2).VariableType = VisitType(((VariableReference)current2).VariableType, ((MemberReference)body.Method).FullName);
				}
			}
			finally
			{
				((IDisposable)enumerator2).Dispose();
			}
			Enumerator<Instruction> enumerator3 = body.Instructions.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					Instruction current3 = enumerator3.Current;
					object operand = current3.Operand;
					FieldReference val = (FieldReference)((operand is FieldReference) ? operand : null);
					if (val == null)
					{
						MethodReference val2 = (MethodReference)((operand is MethodReference) ? operand : null);
						if (val2 == null)
						{
							TypeReference val3 = (TypeReference)((operand is TypeReference) ? operand : null);
							if (val3 != null)
							{
								current3.Operand = VisitType(val3, ((MemberReference)body.Method).FullName);
							}
						}
						else
						{
							MethodReference val4 = importMethodReference(val2);
							if (val4 != null)
							{
								current3.Operand = val4;
							}
							else
							{
								VisitMethod(val2, ((MemberReference)body.Method).FullName);
							}
						}
					}
					else
					{
						FieldReference val5 = importFieldReference(val);
						if (val5 != null)
						{
							current3.Operand = val5;
						}
						else
						{
							VisitField(val, ((MemberReference)body.Method).FullName);
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator3).Dispose();
			}
		}
		void DispatchMethods(TypeDefinition type)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<MethodDefinition> enumerator5 = type.Methods.GetEnumerator();
			try
			{
				while (enumerator5.MoveNext())
				{
					MethodDefinition current5 = enumerator5.Current;
					DispatchMethod(current5);
				}
			}
			finally
			{
				((IDisposable)enumerator5).Dispose();
			}
		}
		void DispatchProperties(TypeDefinition type, string referencingEntityName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<PropertyDefinition> enumerator7 = type.Properties.GetEnumerator();
			try
			{
				while (enumerator7.MoveNext())
				{
					PropertyDefinition current7 = enumerator7.Current;
					((PropertyReference)current7).PropertyType = VisitType(((PropertyReference)current7).PropertyType, referencingEntityName);
					DispatchAttributes((ICustomAttributeProvider)(object)current7, referencingEntityName);
				}
			}
			finally
			{
				((IDisposable)enumerator7).Dispose();
			}
		}
		TypeReference FixupType(TypeReference type)
		{
			if (type.Scope == module2 && redirectedNamespaces.Contains(baseDeclaringType(type).Namespace))
			{
				if (type.IsNested)
				{
					return FixupType(((MemberReference)type).DeclaringType);
				}
				Type type2 = patchingAssembly.GetType(((MemberReference)type).FullName);
				if ((object)type2 != null)
				{
					return module2.ImportReference(type2);
				}
			}
			return type;
		}
		void VisitField(FieldReference? field, string referencingEntityName)
		{
			if (field != null)
			{
				field.FieldType = VisitType(field.FieldType, referencingEntityName);
				if (!(field is FieldDefinition))
				{
					((MemberReference)field).DeclaringType = VisitType(((MemberReference)field).DeclaringType, referencingEntityName);
				}
			}
		}
		void VisitMethod(MethodReference? method, string referencingEntityName)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if (method != null)
			{
				GenericInstanceMethod val7 = (GenericInstanceMethod)(object)((method is GenericInstanceMethod) ? method : null);
				if (val7 != null)
				{
					DispatchGenericArguments((IGenericInstance)(object)val7, referencingEntityName);
				}
				method.ReturnType = VisitType(method.ReturnType, referencingEntityName);
				Enumerator<ParameterDefinition> enumerator10 = method.Parameters.GetEnumerator();
				try
				{
					while (enumerator10.MoveNext())
					{
						ParameterDefinition current10 = enumerator10.Current;
						((ParameterReference)current10).ParameterType = VisitType(((ParameterReference)current10).ParameterType, referencingEntityName);
					}
				}
				finally
				{
					((IDisposable)enumerator10).Dispose();
				}
				if (!(method is MethodSpecification))
				{
					((MemberReference)method).DeclaringType = VisitType(((MemberReference)method).DeclaringType, referencingEntityName);
				}
			}
		}
		TypeReference? VisitType(TypeReference? type, string referencingEntityName)
		{
			if (type == null)
			{
				return type;
			}
			if (type.GetElementType().IsGenericParameter)
			{
				return type;
			}
			GenericInstanceType val8 = (GenericInstanceType)(object)((type is GenericInstanceType) ? type : null);
			if (val8 != null)
			{
				DispatchGenericArguments((IGenericInstance)(object)val8, referencingEntityName);
			}
			return FixupType(type);
		}
		static TypeReference baseDeclaringType(TypeReference type)
		{
			while (((MemberReference)type).DeclaringType != null)
			{
				type = ((MemberReference)type).DeclaringType;
			}
			return type;
		}
		FieldReference? importFieldReference(FieldReference field)
		{
			if (((MemberReference)field).DeclaringType.Scope == module2 && redirectedNamespaces.Contains(baseDeclaringType(((MemberReference)field).DeclaringType).Namespace))
			{
				FieldInfo fieldInfo = patchingAssembly.GetType(((MemberReference)((MemberReference)field).DeclaringType).FullName)?.GetField(((MemberReference)field).Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)fieldInfo != null)
				{
					return module2.ImportReference(fieldInfo);
				}
			}
			return null;
		}
		MethodReference? importMethodReference(MethodReference method)
		{
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Expected O, but got Unknown
			MethodReference method2 = method;
			if (((MemberReference)method2).DeclaringType.Scope == module2 && redirectedNamespaces.Contains(baseDeclaringType(((MemberReference)method2).DeclaringType).Namespace))
			{
				if (((MemberReference)method2).Name == ".cctor")
				{
					ConstructorInfo[] array = patchingAssembly.GetType(((MemberReference)((MemberReference)method2).DeclaringType).FullName)?.GetConstructors(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (array != null && array.Length == 1)
					{
						return module2.ImportReference((MethodBase)array[0]);
					}
				}
				else if (((MemberReference)method2).Name == ".ctor")
				{
					ConstructorInfo constructorInfo = patchingAssembly.GetType(((MemberReference)((MemberReference)method2).DeclaringType).FullName)?.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(CompareMethods);
					if ((object)constructorInfo != null)
					{
						return module2.ImportReference((MethodBase)constructorInfo);
					}
				}
				else
				{
					MethodInfo methodInfo = patchingAssembly.GetType(((MemberReference)((MemberReference)method2).DeclaringType).FullName)?.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo m)
					{
						if (m.Name != ((MemberReference)method2).Name)
						{
							return false;
						}
						return (((MemberReference)method2.ReturnType).ContainsGenericParameter == m.ReturnType.IsGenericParameter || AreSame(method2.ReturnType, module2.ImportReference(m.ReturnType))) && CompareMethods(m);
					});
					if ((object)methodInfo != null)
					{
						MethodReference val9 = module2.ImportReference((MethodBase)methodInfo);
						MethodReference obj = method2;
						GenericInstanceMethod val10 = (GenericInstanceMethod)(object)((obj is GenericInstanceMethod) ? obj : null);
						if (val10 != null)
						{
							GenericInstanceMethod val11 = new GenericInstanceMethod(val9);
							for (int k = 0; k < val10.GenericArguments.Count; k++)
							{
								val10.GenericArguments[k] = VisitType(val10.GenericArguments[k], ((MemberReference)method2).FullName);
								val11.GenericArguments.Add(val10.GenericArguments[k]);
							}
							val9 = (MethodReference)(object)val11;
						}
						return val9;
					}
				}
			}
			return null;
			bool CompareMethods(MethodBase m)
			{
				ParameterInfo[] parameters = m.GetParameters();
				if (method2.IsGenericInstance != m.IsGenericMethodDefinition || parameters.Length != 0 != method2.HasParameters)
				{
					return false;
				}
				if (method2.HasParameters)
				{
					if (method2.Parameters.Count != parameters.Length)
					{
						return false;
					}
					for (int l = 0; l < method2.Parameters.Count; l++)
					{
						if (parameters[l].ParameterType.IsGenericParameter ? (!((ParameterReference)method2.Parameters[l]).ParameterType.IsGenericParameter) : (!AreSame(((ParameterReference)method2.Parameters[l]).ParameterType, module2.ImportReference(parameters[l].ParameterType))))
						{
							return false;
						}
					}
				}
				return true;
			}
		}
	}

	public static bool PreventHarmonyInteropFixLoad(Assembly? __0)
	{
		return __0 == null;
	}

	public static void Patch(IEnumerable<string>? extraNamespaces = null)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Expected O, but got Unknown
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		Harmony val = new Harmony("org.bepinex.plugins.APIManager");
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(PluginInfo), "ToString", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Patcher), "GrabPluginInfo", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(Assembly), "LoadFile", new Type[1] { typeof(string) }, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Patcher), "InterceptAssemblyLoadFile", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(Assembly), "LoadFile", new Type[1] { typeof(string) }, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Patcher), "CheckAssemblyLoadFile", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(Assembly), "LoadFile", new Type[1] { typeof(string) }, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Patcher), "ReplaceAssemblyLoadWithCache", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		new PatchClassProcessor(val, typeof(AssemblyLoadInterceptor), true).Patch();
		Type type = typeof(AssemblyPatcher).Assembly.GetType("BepInEx.Preloader.RuntimeFixes.HarmonyInteropFix");
		if ((object)type != null)
		{
			val.Patch((MethodBase)AccessTools.DeclaredMethod(type, "OnAssemblyLoad", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Patcher), "PreventHarmonyInteropFixLoad", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
		IEnumerable<TypeInfo> source;
		try
		{
			source = patchingAssembly.DefinedTypes.ToList();
		}
		catch (ReflectionTypeLoadException ex)
		{
			source = from t in ex.Types
				where t != null
				select t.GetTypeInfo();
		}
		BaseUnityPlugin val2 = (BaseUnityPlugin)Chainloader.ManagerObject.GetComponent((Type)source.First((TypeInfo t) => t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));
		redirectedNamespaces = new HashSet<string>(extraNamespaces ?? Array.Empty<string>()) { ((object)val2).GetType().Namespace };
		modGUID = val2.Info.Metadata.GUID;
	}
}
