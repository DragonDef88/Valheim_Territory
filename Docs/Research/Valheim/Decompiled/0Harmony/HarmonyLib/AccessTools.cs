using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Threading;
using HarmonyLib.Tools;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace HarmonyLib;

public static class AccessTools
{
	public delegate ref F FieldRef<in T, F>(T instance = default(T));

	public delegate ref F StructFieldRef<T, F>(ref T instance) where T : struct;

	public delegate ref F FieldRef<F>();

	public static readonly BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

	public static readonly BindingFlags allDeclared = all | BindingFlags.DeclaredOnly;

	private static readonly MethodInfo m_PrepForRemoting = Method(typeof(Exception), "PrepForRemoting") ?? Method(typeof(Exception), "FixRemotingException");

	private static readonly FastInvokeHandler PrepForRemoting = MethodInvoker.GetHandler(m_PrepForRemoting);

	private static readonly Dictionary<Type, FastInvokeHandler> addHandlerCache = new Dictionary<Type, FastInvokeHandler>();

	private static readonly ReaderWriterLock addHandlerCacheLock = new ReaderWriterLock();

	public static bool IsMonoRuntime { get; } = (object)Type.GetType("Mono.Runtime") != null;


	public static bool IsNetFrameworkRuntime { get; } = Type.GetType("System.Runtime.InteropServices.RuntimeInformation", throwOnError: false)?.GetProperty("FrameworkDescription").GetValue(null, null).ToString()
		.StartsWith(".NET Framework") ?? (!IsMonoRuntime);


	public static bool IsNetCoreRuntime { get; } = Type.GetType("System.Runtime.InteropServices.RuntimeInformation", throwOnError: false)?.GetProperty("FrameworkDescription").GetValue(null, null).ToString()
		.StartsWith(".NET Core") ?? false;


	public static IEnumerable<Assembly> AllAssemblies()
	{
		return from a in AppDomain.CurrentDomain.GetAssemblies()
			where !a.FullName.StartsWith("Microsoft.VisualStudio")
			select a;
	}

	public static Type TypeByName(string name)
	{
		Type type = Type.GetType(name, throwOnError: false);
		if ((object)type == null)
		{
			type = AllTypes().FirstOrDefault((Type t) => t.FullName == name);
		}
		if ((object)type == null)
		{
			type = AllTypes().FirstOrDefault((Type t) => t.Name == name);
		}
		if ((object)type == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => "AccessTools.TypeByName: Could not find type named " + name);
		}
		return type;
	}

	public static Type[] GetTypesFromAssembly(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex2)
		{
			ReflectionTypeLoadException ex = ex2;
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.GetTypesFromAssembly: assembly {assembly} => {ex}");
			return ex.Types.Where((Type type) => (object)type != null).ToArray();
		}
	}

	public static IEnumerable<Type> AllTypes()
	{
		return AllAssemblies().SelectMany((Assembly a) => GetTypesFromAssembly(a));
	}

	public static T FindIncludingBaseTypes<T>(Type type, Func<Type, T> func) where T : class
	{
		do
		{
			T val = func(type);
			if (val != null)
			{
				return val;
			}
			type = type.BaseType;
		}
		while ((object)type != null);
		return null;
	}

	public static T FindIncludingInnerTypes<T>(Type type, Func<Type, T> func) where T : class
	{
		T val = func(type);
		if (val != null)
		{
			return val;
		}
		Type[] nestedTypes = type.GetNestedTypes(all);
		for (int i = 0; i < nestedTypes.Length; i++)
		{
			val = FindIncludingInnerTypes(nestedTypes[i], func);
			if (val != null)
			{
				break;
			}
		}
		return val;
	}

	public static FieldInfo DeclaredField(Type type, string name)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredField: type is null");
			return null;
		}
		if (name == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredField: name is null");
			return null;
		}
		FieldInfo? field = type.GetField(name, allDeclared);
		if ((object)field == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.DeclaredField: Could not find field for type {type} and name {name}");
		}
		return field;
	}

	public static FieldInfo Field(Type type, string name)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Field: type is null");
			return null;
		}
		if (name == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Field: name is null");
			return null;
		}
		FieldInfo? fieldInfo = FindIncludingBaseTypes(type, (Type t) => t.GetField(name, all));
		if ((object)fieldInfo == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.Field: Could not find field for type {type} and name {name}");
		}
		return fieldInfo;
	}

	public static FieldInfo DeclaredField(Type type, int idx)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredField: type is null");
			return null;
		}
		FieldInfo? fieldInfo = GetDeclaredFields(type).ElementAtOrDefault(idx);
		if ((object)fieldInfo == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.DeclaredField: Could not find field for type {type} and idx {idx}");
		}
		return fieldInfo;
	}

	public static PropertyInfo DeclaredProperty(Type type, string name)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredProperty: type is null");
			return null;
		}
		if (name == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredProperty: name is null");
			return null;
		}
		PropertyInfo? property = type.GetProperty(name, allDeclared);
		if ((object)property == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.DeclaredProperty: Could not find property for type {type} and name {name}");
		}
		return property;
	}

	public static MethodInfo DeclaredPropertyGetter(Type type, string name)
	{
		return DeclaredProperty(type, name)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredPropertySetter(Type type, string name)
	{
		return DeclaredProperty(type, name)?.GetSetMethod(nonPublic: true);
	}

	public static PropertyInfo Property(Type type, string name)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Property: type is null");
			return null;
		}
		if (name == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Property: name is null");
			return null;
		}
		PropertyInfo? propertyInfo = FindIncludingBaseTypes(type, (Type t) => t.GetProperty(name, all));
		if ((object)propertyInfo == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.Property: Could not find property for type {type} and name {name}");
		}
		return propertyInfo;
	}

	public static MethodInfo PropertyGetter(Type type, string name)
	{
		return Property(type, name)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo PropertySetter(Type type, string name)
	{
		return Property(type, name)?.GetSetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredMethod(Type type, string name, Type[] parameters = null, Type[] generics = null)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredMethod: type is null");
			return null;
		}
		if (name == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredMethod: name is null");
			return null;
		}
		ParameterModifier[] modifiers = new ParameterModifier[0];
		MethodInfo methodInfo = ((parameters != null) ? type.GetMethod(name, allDeclared, null, parameters, modifiers) : type.GetMethod(name, allDeclared));
		if ((object)methodInfo == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.DeclaredMethod: Could not find method for type {type} and name {name} and parameters {parameters?.Description()}");
			return null;
		}
		if (generics != null)
		{
			methodInfo = methodInfo.MakeGenericMethod(generics);
		}
		return methodInfo;
	}

	public static MethodInfo Method(Type type, string name, Type[] parameters = null, Type[] generics = null)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Method: type is null");
			return null;
		}
		if (name == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Method: name is null");
			return null;
		}
		ParameterModifier[] modifiers = new ParameterModifier[0];
		MethodInfo methodInfo;
		if (parameters == null)
		{
			try
			{
				methodInfo = FindIncludingBaseTypes(type, (Type t) => t.GetMethod(name, all));
			}
			catch (AmbiguousMatchException inner)
			{
				methodInfo = FindIncludingBaseTypes(type, (Type t) => t.GetMethod(name, all, null, new Type[0], modifiers));
				if ((object)methodInfo == null)
				{
					throw new AmbiguousMatchException($"Ambiguous match in Harmony patch for {type}:{name}", inner);
				}
			}
		}
		else
		{
			methodInfo = FindIncludingBaseTypes(type, (Type t) => t.GetMethod(name, all, null, parameters, modifiers));
		}
		if ((object)methodInfo == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => $"AccessTools.Method: Could not find method for type {type} and name {name} and parameters {parameters?.Description()}");
			return null;
		}
		if (generics != null)
		{
			methodInfo = methodInfo.MakeGenericMethod(generics);
		}
		return methodInfo;
	}

	public static MethodInfo Method(string typeColonMethodname, Type[] parameters = null, Type[] generics = null)
	{
		if (typeColonMethodname == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Method: typeColonMethodname is null");
			return null;
		}
		string[] array = typeColonMethodname.Split(new char[1] { ':' });
		if (array.Length != 2)
		{
			throw new ArgumentException("Method must be specified as 'Namespace.Type1.Type2:MethodName", "typeColonMethodname");
		}
		return DeclaredMethod(TypeByName(array[0]), array[1], parameters, generics);
	}

	public static MethodInfo EnumeratorMoveNext(MethodBase enumerator)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		if ((object)enumerator == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "EnumeratorMoveNext.Method: enumerator is null");
			return null;
		}
		ILContext val = new ILContext(new DynamicMethodDefinition(enumerator).Definition);
		ILCursor val2 = new ILCursor(val);
		Type enumeratorType;
		if (((MemberReference)((MethodReference)val.Method).ReturnType).Name.StartsWith("UniTask"))
		{
			VariableDefinition? obj = ((IEnumerable<VariableDefinition>)val.Body.Variables).FirstOrDefault();
			TypeReference firstVar = ((obj != null) ? ((VariableReference)obj).VariableType : null);
			if (firstVar != null && !((MemberReference)firstVar).Name.Contains(enumerator.Name))
			{
				Logger.Log(Logger.LogChannel.Warn, () => "EnumeratorMoveNext.Method: Unexpected type name " + ((MemberReference)firstVar).Name + ", should contain " + enumerator.Name);
				return null;
			}
			enumeratorType = ReflectionHelper.ResolveReflection(firstVar);
		}
		else
		{
			MethodReference enumeratorCtor = null;
			val2.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction i) => ILPatternMatchingExt.MatchNewobj(i, ref enumeratorCtor)
			});
			if (enumeratorCtor == null)
			{
				Logger.Log(Logger.LogChannel.Warn, () => "EnumeratorMoveNext.Method: " + enumerator.FullDescription() + " does not create enumerators");
				return null;
			}
			if (((MemberReference)enumeratorCtor).Name != ".ctor")
			{
				Logger.Log(Logger.LogChannel.Warn, () => "EnumeratorMoveNext.Method: " + enumerator.FullDescription() + " does not create an enumerator (got " + Extensions.GetID(enumeratorCtor, (string)null, (string)null, true, true) + ")");
				return null;
			}
			enumeratorType = ReflectionHelper.ResolveReflection(((MemberReference)enumeratorCtor).DeclaringType);
		}
		MethodInfo methodInfo = Method(enumeratorType, "MoveNext");
		if ((object)methodInfo == null)
		{
			Logger.Log(Logger.LogChannel.Warn, () => "EnumeratorMoveNext.Method: " + enumerator.FullDescription() + " creates an object " + enumeratorType.FullDescription() + " but it doesn't have MoveNext");
			return null;
		}
		return methodInfo;
	}

	public static List<string> GetMethodNames(Type type)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetMethodNames: type is null");
			return new List<string>();
		}
		return (from m in GetDeclaredMethods(type)
			select m.Name).ToList();
	}

	public static List<string> GetMethodNames(object instance)
	{
		if (instance == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetMethodNames: instance is null");
			return new List<string>();
		}
		return GetMethodNames(instance.GetType());
	}

	public static List<string> GetFieldNames(Type type)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetFieldNames: type is null");
			return new List<string>();
		}
		return (from f in GetDeclaredFields(type)
			select f.Name).ToList();
	}

	public static List<string> GetFieldNames(object instance)
	{
		if (instance == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetFieldNames: instance is null");
			return new List<string>();
		}
		return GetFieldNames(instance.GetType());
	}

	public static List<string> GetPropertyNames(Type type)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetPropertyNames: type is null");
			return new List<string>();
		}
		return (from f in GetDeclaredProperties(type)
			select f.Name).ToList();
	}

	public static List<string> GetPropertyNames(object instance)
	{
		if (instance == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetPropertyNames: instance is null");
			return new List<string>();
		}
		return GetPropertyNames(instance.GetType());
	}

	public static Type GetUnderlyingType(this MemberInfo member)
	{
		return member.MemberType switch
		{
			MemberTypes.Event => ((EventInfo)member).EventHandlerType, 
			MemberTypes.Field => ((FieldInfo)member).FieldType, 
			MemberTypes.Method => ((MethodInfo)member).ReturnType, 
			MemberTypes.Property => ((PropertyInfo)member).PropertyType, 
			_ => throw new ArgumentException("Member must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"), 
		};
	}

	public static bool IsDeclaredMember<T>(this T member) where T : MemberInfo
	{
		return (object)member.DeclaringType == member.ReflectedType;
	}

	public static T GetDeclaredMember<T>(this T member) where T : MemberInfo
	{
		if ((object)member.DeclaringType == null || member.IsDeclaredMember())
		{
			return member;
		}
		int metadataToken = member.MetadataToken;
		MemberInfo[] array = member.DeclaringType?.GetMembers(all) ?? new MemberInfo[0];
		foreach (MemberInfo memberInfo in array)
		{
			if (memberInfo.MetadataToken == metadataToken)
			{
				return (T)memberInfo;
			}
		}
		return member;
	}

	public static ConstructorInfo DeclaredConstructor(Type type, Type[] parameters = null, bool searchForStatic = false)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.DeclaredConstructor: type is null");
			return null;
		}
		if (parameters == null)
		{
			parameters = new Type[0];
		}
		BindingFlags bindingAttr = (searchForStatic ? (allDeclared & ~BindingFlags.Instance) : (allDeclared & ~BindingFlags.Static));
		return type.GetConstructor(bindingAttr, null, parameters, new ParameterModifier[0]);
	}

	public static ConstructorInfo Constructor(Type type, Type[] parameters = null, bool searchForStatic = false)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.ConstructorInfo: type is null");
			return null;
		}
		if (parameters == null)
		{
			parameters = new Type[0];
		}
		BindingFlags flags = (searchForStatic ? (all & ~BindingFlags.Instance) : (all & ~BindingFlags.Static));
		return FindIncludingBaseTypes(type, (Type t) => t.GetConstructor(flags, null, parameters, new ParameterModifier[0]));
	}

	public static List<ConstructorInfo> GetDeclaredConstructors(Type type, bool? searchForStatic = null)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetDeclaredConstructors: type is null");
			return new List<ConstructorInfo>();
		}
		BindingFlags bindingFlags = allDeclared;
		if (searchForStatic.HasValue)
		{
			bindingFlags = (searchForStatic.Value ? (bindingFlags & ~BindingFlags.Instance) : (bindingFlags & ~BindingFlags.Static));
		}
		return (from method in type.GetConstructors(bindingFlags)
			where (object)method.DeclaringType == type
			select method).ToList();
	}

	public static List<MethodInfo> GetDeclaredMethods(Type type)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetDeclaredMethods: type is null");
			return new List<MethodInfo>();
		}
		return type.GetMethods(allDeclared).ToList();
	}

	public static List<PropertyInfo> GetDeclaredProperties(Type type)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetDeclaredProperties: type is null");
			return new List<PropertyInfo>();
		}
		return type.GetProperties(allDeclared).ToList();
	}

	public static List<FieldInfo> GetDeclaredFields(Type type)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetDeclaredFields: type is null");
			return new List<FieldInfo>();
		}
		return type.GetFields(allDeclared).ToList();
	}

	public static Type GetReturnedType(MethodBase methodOrConstructor)
	{
		if ((object)methodOrConstructor == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetReturnedType: methodOrConstructor is null");
			return null;
		}
		if (methodOrConstructor is ConstructorInfo)
		{
			return typeof(void);
		}
		return ((MethodInfo)methodOrConstructor).ReturnType;
	}

	public static Type Inner(Type type, string name)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Inner: type is null");
			return null;
		}
		if (name == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.Inner: name is null");
			return null;
		}
		return FindIncludingBaseTypes(type, (Type t) => t.GetNestedType(name, all));
	}

	public static Type FirstInner(Type type, Func<Type, bool> predicate)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstInner: type is null");
			return null;
		}
		if (predicate == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstInner: predicate is null");
			return null;
		}
		return type.GetNestedTypes(all).FirstOrDefault((Type subType) => predicate(subType));
	}

	public static MethodInfo FirstMethod(Type type, Func<MethodInfo, bool> predicate)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstMethod: type is null");
			return null;
		}
		if (predicate == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstMethod: predicate is null");
			return null;
		}
		return type.GetMethods(allDeclared).FirstOrDefault((MethodInfo method) => predicate(method));
	}

	public static ConstructorInfo FirstConstructor(Type type, Func<ConstructorInfo, bool> predicate)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstConstructor: type is null");
			return null;
		}
		if (predicate == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstConstructor: predicate is null");
			return null;
		}
		return type.GetConstructors(allDeclared).FirstOrDefault((ConstructorInfo constructor) => predicate(constructor));
	}

	public static PropertyInfo FirstProperty(Type type, Func<PropertyInfo, bool> predicate)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstProperty: type is null");
			return null;
		}
		if (predicate == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.FirstProperty: predicate is null");
			return null;
		}
		return type.GetProperties(allDeclared).FirstOrDefault((PropertyInfo property) => predicate(property));
	}

	public static Type[] GetTypes(object[] parameters)
	{
		if (parameters == null)
		{
			return new Type[0];
		}
		return parameters.Select((object p) => (p != null) ? p.GetType() : typeof(object)).ToArray();
	}

	public static object[] ActualParameters(MethodBase method, object[] inputs)
	{
		List<Type> inputTypes = inputs.Select((object obj) => obj?.GetType()).ToList();
		return (from p in method.GetParameters()
			select p.ParameterType).Select(delegate(Type pType)
		{
			int num = inputTypes.FindIndex((Type inType) => (object)inType != null && pType.IsAssignableFrom(inType));
			return (num >= 0) ? inputs[num] : GetDefaultValue(pType);
		}).ToArray();
	}

	public static FieldRef<T, F> FieldRefAccess<T, F>(string fieldName)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			return FieldRefAccessInternal<T, F>(GetInstanceField(typeFromHandle, fieldName), needCastclass: false);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldName} caused an exception", innerException);
		}
	}

	public static ref F FieldRefAccess<T, F>(T instance, string fieldName)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			return ref FieldRefAccessInternal<T, F>(GetInstanceField(typeFromHandle, fieldName), needCastclass: false)(instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldName} caused an exception", innerException);
		}
	}

	public static FieldRef<object, F> FieldRefAccess<F>(Type type, string fieldName)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			FieldInfo fieldInfo = Field(type, fieldName);
			if ((object)fieldInfo == null)
			{
				throw new MissingFieldException(type.Name, fieldName);
			}
			if (!fieldInfo.IsStatic)
			{
				Type declaringType = fieldInfo.DeclaringType;
				if ((object)declaringType != null && declaringType.IsValueType)
				{
					throw new ArgumentException("Either FieldDeclaringType must be a class or field must be static");
				}
			}
			return FieldRefAccessInternal<object, F>(fieldInfo, needCastclass: true);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(F)}> for {type}, {fieldName} caused an exception", innerException);
		}
	}

	public static FieldRef<T, F> FieldRefAccess<T, F>(FieldInfo fieldInfo)
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			bool needCastclass = false;
			if (!fieldInfo.IsStatic)
			{
				Type declaringType = fieldInfo.DeclaringType;
				if ((object)declaringType != null)
				{
					if (declaringType.IsValueType)
					{
						throw new ArgumentException("Either FieldDeclaringType must be a class or field must be static");
					}
					needCastclass = FieldRefNeedsClasscast(typeFromHandle, declaringType);
				}
			}
			return FieldRefAccessInternal<T, F>(fieldInfo, needCastclass);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	public static ref F FieldRefAccess<T, F>(T instance, FieldInfo fieldInfo)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field must not be static");
			}
			bool needCastclass = false;
			Type declaringType = fieldInfo.DeclaringType;
			if ((object)declaringType != null)
			{
				if (declaringType.IsValueType)
				{
					throw new ArgumentException("FieldDeclaringType must be a class");
				}
				needCastclass = FieldRefNeedsClasscast(typeFromHandle, declaringType);
			}
			return ref FieldRefAccessInternal<T, F>(fieldInfo, needCastclass)(instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldInfo} caused an exception", innerException);
		}
	}

	private static FieldRef<T, F> FieldRefAccessInternal<T, F>(FieldInfo fieldInfo, bool needCastclass)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		ValidateFieldType<F>(fieldInfo);
		Type typeFromHandle = typeof(T);
		Type declaringType = fieldInfo.DeclaringType;
		DynamicMethodDefinition val = new DynamicMethodDefinition("__refget_" + typeFromHandle.Name + "_fi_" + fieldInfo.Name, typeof(F).MakeByRefType(), new Type[1] { typeFromHandle });
		ILGenerator iLGenerator = val.GetILGenerator();
		if (fieldInfo.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldsflda, fieldInfo);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			if (needCastclass)
			{
				iLGenerator.Emit(OpCodes.Castclass, declaringType);
			}
			iLGenerator.Emit(OpCodes.Ldflda, fieldInfo);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (FieldRef<T, F>)Extensions.CreateDelegate((MethodBase)val.Generate(), typeof(FieldRef<T, F>));
	}

	public static StructFieldRef<T, F> StructFieldRefAccess<T, F>(string fieldName) where T : struct
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			return StructFieldRefAccessInternal<T, F>(GetInstanceField(typeof(T), fieldName));
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldName} caused an exception", innerException);
		}
	}

	public static ref F StructFieldRefAccess<T, F>(ref T instance, string fieldName) where T : struct
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			return ref StructFieldRefAccessInternal<T, F>(GetInstanceField(typeof(T), fieldName))(ref instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldName} caused an exception", innerException);
		}
	}

	public static StructFieldRef<T, F> StructFieldRefAccess<T, F>(FieldInfo fieldInfo) where T : struct
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			ValidateStructField<T, F>(fieldInfo);
			return StructFieldRefAccessInternal<T, F>(fieldInfo);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	public static ref F StructFieldRefAccess<T, F>(ref T instance, FieldInfo fieldInfo) where T : struct
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			ValidateStructField<T, F>(fieldInfo);
			return ref StructFieldRefAccessInternal<T, F>(fieldInfo)(ref instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldInfo} caused an exception", innerException);
		}
	}

	private static StructFieldRef<T, F> StructFieldRefAccessInternal<T, F>(FieldInfo fieldInfo) where T : struct
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		ValidateFieldType<F>(fieldInfo);
		DynamicMethodDefinition val = new DynamicMethodDefinition("__refget_" + typeof(T).Name + "_struct_fi_" + fieldInfo.Name, typeof(F).MakeByRefType(), new Type[1] { typeof(T).MakeByRefType() });
		ILGenerator iLGenerator = val.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldflda, fieldInfo);
		iLGenerator.Emit(OpCodes.Ret);
		return (StructFieldRef<T, F>)Extensions.CreateDelegate((MethodBase)val.Generate(), typeof(StructFieldRef<T, F>));
	}

	public static ref F StaticFieldRefAccess<T, F>(string fieldName)
	{
		return ref StaticFieldRefAccess<F>(typeof(T), fieldName);
	}

	public static ref F StaticFieldRefAccess<F>(Type type, string fieldName)
	{
		try
		{
			return ref StaticFieldRefAccessInternal<F>(Field(type, fieldName) ?? throw new MissingFieldException(type.Name, fieldName))();
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StaticFieldRefAccess<{typeof(F)}> for {type}, {fieldName} caused an exception", innerException);
		}
	}

	public static ref F StaticFieldRefAccess<T, F>(FieldInfo fieldInfo)
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			return ref StaticFieldRefAccessInternal<F>(fieldInfo)();
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StaticFieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	public static FieldRef<F> StaticFieldRefAccess<F>(FieldInfo fieldInfo)
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			return StaticFieldRefAccessInternal<F>(fieldInfo);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StaticFieldRefAccess<{typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	private static FieldRef<F> StaticFieldRefAccessInternal<F>(FieldInfo fieldInfo)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!fieldInfo.IsStatic)
		{
			throw new ArgumentException("Field must be static");
		}
		ValidateFieldType<F>(fieldInfo);
		DynamicMethodDefinition val = new DynamicMethodDefinition("__refget_" + (fieldInfo.DeclaringType?.Name ?? "null") + "_static_fi_" + fieldInfo.Name, typeof(F).MakeByRefType(), new Type[0]);
		ILGenerator iLGenerator = val.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldsflda, fieldInfo);
		iLGenerator.Emit(OpCodes.Ret);
		return (FieldRef<F>)Extensions.CreateDelegate((MethodBase)val.Generate(), typeof(FieldRef<F>));
	}

	private static FieldInfo GetInstanceField(Type type, string fieldName)
	{
		FieldInfo obj = Field(type, fieldName) ?? throw new MissingFieldException(type.Name, fieldName);
		if (obj.IsStatic)
		{
			throw new ArgumentException("Field must not be static");
		}
		return obj;
	}

	private static bool FieldRefNeedsClasscast(Type delegateInstanceType, Type declaringType)
	{
		bool flag = false;
		if ((object)delegateInstanceType != declaringType)
		{
			flag = delegateInstanceType.IsAssignableFrom(declaringType);
			if (!flag && !declaringType.IsAssignableFrom(delegateInstanceType))
			{
				throw new ArgumentException("FieldDeclaringType must be assignable from or to T (FieldRefAccess instance type) - \"instanceOfT is FieldDeclaringType\" must be possible");
			}
		}
		return flag;
	}

	private static void ValidateStructField<T, F>(FieldInfo fieldInfo) where T : struct
	{
		if (fieldInfo.IsStatic)
		{
			throw new ArgumentException("Field must not be static");
		}
		if ((object)fieldInfo.DeclaringType != typeof(T))
		{
			throw new ArgumentException("FieldDeclaringType must be T (StructFieldRefAccess instance type)");
		}
	}

	private static void ValidateFieldType<F>(FieldInfo fieldInfo)
	{
		Type typeFromHandle = typeof(F);
		Type fieldType = fieldInfo.FieldType;
		if ((object)typeFromHandle == fieldType)
		{
			return;
		}
		if (fieldType.IsEnum)
		{
			Type underlyingType = Enum.GetUnderlyingType(fieldType);
			if ((object)typeFromHandle == underlyingType)
			{
				return;
			}
			throw new ArgumentException("FieldRefAccess return type must be the same as FieldType or " + $"FieldType's underlying integral type ({underlyingType}) for enum types");
		}
		if (fieldType.IsValueType)
		{
			throw new ArgumentException("FieldRefAccess return type must be the same as FieldType for value types");
		}
		if (typeFromHandle.IsAssignableFrom(fieldType))
		{
			return;
		}
		throw new ArgumentException("FieldRefAccess return type must be assignable from FieldType for reference types");
	}

	public static DelegateType MethodDelegate<DelegateType>(MethodInfo method, object instance = null, bool virtualCall = true) where DelegateType : Delegate
	{
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Expected O, but got Unknown
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		Type typeFromHandle = typeof(DelegateType);
		if (method.IsStatic)
		{
			return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method);
		}
		Type type = method.DeclaringType;
		if ((object)type != null && type.IsInterface && !virtualCall)
		{
			throw new ArgumentException("Interface methods must be called virtually");
		}
		if (instance == null)
		{
			ParameterInfo[] parameters = typeFromHandle.GetMethod("Invoke").GetParameters();
			if (parameters.Length == 0)
			{
				Delegate.CreateDelegate(typeof(DelegateType), method);
				throw new ArgumentException("Invalid delegate type");
			}
			Type parameterType = parameters[0].ParameterType;
			if ((object)type != null && type.IsInterface && parameterType.IsValueType)
			{
				InterfaceMapping interfaceMap = parameterType.GetInterfaceMap(type);
				method = interfaceMap.TargetMethods[Array.IndexOf(interfaceMap.InterfaceMethods, method)];
				type = parameterType;
			}
			if ((object)type != null && virtualCall)
			{
				if (type.IsInterface)
				{
					return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method);
				}
				if (parameterType.IsInterface)
				{
					InterfaceMapping interfaceMap2 = type.GetInterfaceMap(parameterType);
					MethodInfo method2 = interfaceMap2.InterfaceMethods[Array.IndexOf(interfaceMap2.TargetMethods, method)];
					return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method2);
				}
				if (!type.IsValueType)
				{
					return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method.GetBaseDefinition());
				}
			}
			ParameterInfo[] parameters2 = method.GetParameters();
			int num = parameters2.Length;
			Type[] array = new Type[num + 1];
			array[0] = type;
			for (int i = 0; i < num; i++)
			{
				array[i + 1] = parameters2[i].ParameterType;
			}
			DynamicMethodDefinition val = new DynamicMethodDefinition("OpenInstanceDelegate_" + method.Name, method.ReturnType, array)
			{
				OwnerType = type
			};
			ILGenerator iLGenerator = val.GetILGenerator();
			if ((object)type != null && type.IsValueType)
			{
				iLGenerator.Emit(OpCodes.Ldarga_S, 0);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
			}
			for (int j = 1; j < array.Length; j++)
			{
				iLGenerator.Emit(OpCodes.Ldarg, j);
			}
			iLGenerator.Emit(OpCodes.Call, method);
			iLGenerator.Emit(OpCodes.Ret);
			return (DelegateType)Extensions.CreateDelegate((MethodBase)val.Generate(), typeFromHandle);
		}
		if (virtualCall)
		{
			return (DelegateType)Delegate.CreateDelegate(typeFromHandle, instance, method.GetBaseDefinition());
		}
		if ((object)type != null && !type.IsInstanceOfType(instance))
		{
			Delegate.CreateDelegate(typeof(DelegateType), instance, method);
			throw new ArgumentException("Invalid delegate type");
		}
		if (IsMonoRuntime)
		{
			DynamicMethodDefinition val2 = new DynamicMethodDefinition("LdftnDelegate_" + method.Name, typeFromHandle, new Type[1] { typeof(object) })
			{
				OwnerType = typeFromHandle
			};
			ILGenerator iLGenerator2 = val2.GetILGenerator();
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Ldftn, method);
			iLGenerator2.Emit(OpCodes.Newobj, typeFromHandle.GetConstructor(new Type[2]
			{
				typeof(object),
				typeof(IntPtr)
			}));
			iLGenerator2.Emit(OpCodes.Ret);
			return (DelegateType)val2.Generate().Invoke(null, new object[1] { instance });
		}
		return (DelegateType)Activator.CreateInstance(typeFromHandle, instance, method.MethodHandle.GetFunctionPointer());
	}

	public static DelegateType HarmonyDelegate<DelegateType>(object instance = null) where DelegateType : Delegate
	{
		HarmonyMethod mergedFromType = HarmonyMethodExtensions.GetMergedFromType(typeof(DelegateType));
		MethodType? methodType = mergedFromType.methodType;
		if (!methodType.HasValue)
		{
			mergedFromType.methodType = MethodType.Normal;
		}
		return MethodDelegate<DelegateType>((mergedFromType.GetOriginalMethod() as MethodInfo) ?? throw new NullReferenceException($"Delegate {typeof(DelegateType)} has no defined original method"), instance, !mergedFromType.nonVirtualDelegate);
	}

	public static MethodBase GetOutsideCaller()
	{
		StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();
		for (int i = 0; i < frames.Length; i++)
		{
			MethodBase method = frames[i].GetMethod();
			if (method.DeclaringType?.Namespace != typeof(Harmony).Namespace)
			{
				return method;
			}
		}
		throw new Exception("Unexpected end of stack trace");
	}

	public static void RethrowException(Exception exception)
	{
		PrepForRemoting(exception);
		throw exception;
	}

	public static void ThrowMissingMemberException(Type type, params string[] names)
	{
		string text = string.Join(",", GetFieldNames(type).ToArray());
		string text2 = string.Join(",", GetPropertyNames(type).ToArray());
		throw new MissingMemberException(string.Join(",", names) + "; available fields: " + text + "; available properties: " + text2);
	}

	public static object GetDefaultValue(Type type)
	{
		if ((object)type == null)
		{
			Logger.LogText(Logger.LogChannel.Warn, "AccessTools.GetDefaultValue: type is null");
			return null;
		}
		if ((object)type == typeof(void))
		{
			return null;
		}
		if (type.IsValueType)
		{
			return Activator.CreateInstance(type);
		}
		return null;
	}

	public static object CreateInstance(Type type)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, new Type[0], null);
		if ((object)constructor != null)
		{
			return constructor.Invoke(null);
		}
		return FormatterServices.GetUninitializedObject(type);
	}

	public static T CreateInstance<T>()
	{
		object obj = CreateInstance(typeof(T));
		if (obj is T)
		{
			return (T)obj;
		}
		return default(T);
	}

	public static T MakeDeepCopy<T>(object source) where T : class
	{
		return MakeDeepCopy(source, typeof(T)) as T;
	}

	public static void MakeDeepCopy<T>(object source, out T result, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
	{
		result = (T)MakeDeepCopy(source, typeof(T), processor, pathRoot);
	}

	public static object MakeDeepCopy(object source, Type resultType, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
	{
		if (source == null || (object)resultType == null)
		{
			return null;
		}
		resultType = Nullable.GetUnderlyingType(resultType) ?? resultType;
		Type type = source.GetType();
		if (type.IsPrimitive)
		{
			return source;
		}
		if (type.IsEnum)
		{
			return Enum.ToObject(resultType, (int)source);
		}
		if (type.IsGenericType && resultType.IsGenericType)
		{
			addHandlerCacheLock.AcquireReaderLock(200);
			try
			{
				if (!addHandlerCache.TryGetValue(resultType, out var value))
				{
					MethodInfo methodInfo = FirstMethod(resultType, (MethodInfo m) => m.Name == "Add" && m.GetParameters().Length == 1);
					if ((object)methodInfo != null)
					{
						value = MethodInvoker.GetHandler(methodInfo);
					}
					addHandlerCacheLock.UpgradeToWriterLock(200);
					addHandlerCacheLock.AcquireWriterLock(200);
					try
					{
						addHandlerCache[resultType] = value;
					}
					finally
					{
						addHandlerCacheLock.ReleaseWriterLock();
					}
				}
				if (value != null)
				{
					object obj = Activator.CreateInstance(resultType);
					Type resultType2 = resultType.GetGenericArguments()[0];
					int num = 0;
					foreach (object item in source as IEnumerable)
					{
						string text = num++.ToString();
						object obj2 = MakeDeepCopy(pathRoot: (pathRoot.Length > 0) ? (pathRoot + "." + text) : text, source: item, resultType: resultType2, processor: processor);
						value(obj, obj2);
					}
					return obj;
				}
			}
			finally
			{
				addHandlerCacheLock.ReleaseReaderLock();
			}
		}
		if (type.IsArray && resultType.IsArray)
		{
			Type elementType = resultType.GetElementType();
			int length = ((Array)source).Length;
			object[] array = Activator.CreateInstance(resultType, length) as object[];
			object[] array2 = source as object[];
			for (int i = 0; i < length; i++)
			{
				string text2 = i.ToString();
				string pathRoot3 = ((pathRoot.Length > 0) ? (pathRoot + "." + text2) : text2);
				array[i] = MakeDeepCopy(array2[i], elementType, processor, pathRoot3);
			}
			return array;
		}
		string @namespace = type.Namespace;
		if (@namespace == "System" || (@namespace != null && @namespace.StartsWith("System.")))
		{
			return source;
		}
		object obj3 = CreateInstance(((object)resultType == typeof(object)) ? type : resultType);
		Traverse.IterateFields(source, obj3, delegate(string name, Traverse src, Traverse dst)
		{
			string text3 = ((pathRoot.Length > 0) ? (pathRoot + "." + name) : name);
			object source2 = ((processor != null) ? processor(text3, src, dst) : src.GetValue());
			dst.SetValue(MakeDeepCopy(source2, dst.GetValueType(), processor, text3));
		});
		return obj3;
	}

	public static bool IsStruct(Type type)
	{
		if ((object)type == null)
		{
			return false;
		}
		if (type.IsValueType && !IsValue(type))
		{
			return !IsVoid(type);
		}
		return false;
	}

	public static bool IsClass(Type type)
	{
		if ((object)type == null)
		{
			return false;
		}
		return !type.IsValueType;
	}

	public static bool IsValue(Type type)
	{
		if ((object)type == null)
		{
			return false;
		}
		if (!type.IsPrimitive)
		{
			return type.IsEnum;
		}
		return true;
	}

	public static bool IsInteger(Type type)
	{
		if ((object)type == null)
		{
			return false;
		}
		TypeCode typeCode = Type.GetTypeCode(type);
		if ((uint)(typeCode - 5) <= 7u)
		{
			return true;
		}
		return false;
	}

	public static bool IsFloatingPoint(Type type)
	{
		if ((object)type == null)
		{
			return false;
		}
		TypeCode typeCode = Type.GetTypeCode(type);
		if ((uint)(typeCode - 13) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsNumber(Type type)
	{
		if (!IsInteger(type))
		{
			return IsFloatingPoint(type);
		}
		return true;
	}

	public static bool IsVoid(Type type)
	{
		return (object)type == typeof(void);
	}

	public static bool IsOfNullableType<T>(T instance)
	{
		return (object)Nullable.GetUnderlyingType(typeof(T)) != null;
	}

	public static bool IsStatic(MemberInfo member)
	{
		if ((object)member == null)
		{
			throw new ArgumentNullException("member");
		}
		switch (member.MemberType)
		{
		case MemberTypes.Constructor:
		case MemberTypes.Method:
			return ((MethodBase)member).IsStatic;
		case MemberTypes.Event:
			return IsStatic((EventInfo)member);
		case MemberTypes.Field:
			return ((FieldInfo)member).IsStatic;
		case MemberTypes.Property:
			return IsStatic((PropertyInfo)member);
		case MemberTypes.TypeInfo:
		case MemberTypes.NestedType:
			return IsStatic((Type)member);
		default:
			throw new ArgumentException($"Unknown member type: {member.MemberType}");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool IsStatic(Type type)
	{
		if ((object)type == null)
		{
			return false;
		}
		if (type.IsAbstract)
		{
			return type.IsSealed;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool IsStatic(PropertyInfo propertyInfo)
	{
		if ((object)propertyInfo == null)
		{
			throw new ArgumentNullException("propertyInfo");
		}
		return propertyInfo.GetAccessors(nonPublic: true)[0].IsStatic;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool IsStatic(EventInfo eventInfo)
	{
		if ((object)eventInfo == null)
		{
			throw new ArgumentNullException("eventInfo");
		}
		return eventInfo.GetAddMethod(nonPublic: true).IsStatic;
	}

	public static int CombinedHashCode(IEnumerable<object> objects)
	{
		int num = 352654597;
		int num2 = num;
		int num3 = 0;
		foreach (object @object in objects)
		{
			if (num3 % 2 == 0)
			{
				num = ((num << 5) + num + (num >> 27)) ^ @object.GetHashCode();
			}
			else
			{
				num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ @object.GetHashCode();
			}
			num3++;
		}
		return num + num2 * 1566083941;
	}
}
