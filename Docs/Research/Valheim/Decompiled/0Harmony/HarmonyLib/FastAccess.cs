using System;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Utils;

namespace HarmonyLib;

public static class FastAccess
{
	public static InstantiationHandler<T> CreateInstantiationHandler<T>()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		ConstructorInfo constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null);
		if ((object)constructor == null)
		{
			throw new ApplicationException($"The type {typeof(T)} must declare an empty constructor (the constructor may be private, internal, protected, protected internal, or public).");
		}
		DynamicMethodDefinition val = new DynamicMethodDefinition("InstantiateObject_" + typeof(T).Name, typeof(T), (Type[])null);
		ILGenerator iLGenerator = val.GetILGenerator();
		iLGenerator.Emit(OpCodes.Newobj, constructor);
		iLGenerator.Emit(OpCodes.Ret);
		return (InstantiationHandler<T>)Extensions.CreateDelegate((MethodBase)val.Generate(), typeof(InstantiationHandler<T>));
	}

	[Obsolete("Use AccessTools.MethodDelegate<Func<T, S>>(PropertyInfo.GetGetMethod(true))")]
	public static GetterHandler<T, S> CreateGetterHandler<T, S>(PropertyInfo propertyInfo)
	{
		MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
		DynamicMethodDefinition obj = CreateGetDynamicMethod<T, S>(propertyInfo.DeclaringType);
		ILGenerator iLGenerator = obj.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Call, getMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (GetterHandler<T, S>)Extensions.CreateDelegate((MethodBase)obj.Generate(), typeof(GetterHandler<T, S>));
	}

	[Obsolete("Use AccessTools.FieldRefAccess<T, S>(fieldInfo)")]
	public static GetterHandler<T, S> CreateGetterHandler<T, S>(FieldInfo fieldInfo)
	{
		DynamicMethodDefinition obj = CreateGetDynamicMethod<T, S>(fieldInfo.DeclaringType);
		ILGenerator iLGenerator = obj.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
		iLGenerator.Emit(OpCodes.Ret);
		return (GetterHandler<T, S>)Extensions.CreateDelegate((MethodBase)obj.Generate(), typeof(GetterHandler<T, S>));
	}

	[Obsolete("Use AccessTools.FieldRefAccess<T, S>(name) for fields and AccessTools.MethodDelegate<Func<T, S>>(AccessTools.PropertyGetter(typeof(T), name)) for properties")]
	public static GetterHandler<T, S> CreateFieldGetter<T, S>(params string[] names)
	{
		foreach (string name in names)
		{
			FieldInfo field = typeof(T).GetField(name, AccessTools.all);
			if ((object)field != null)
			{
				return CreateGetterHandler<T, S>(field);
			}
			PropertyInfo property = typeof(T).GetProperty(name, AccessTools.all);
			if ((object)property != null)
			{
				return CreateGetterHandler<T, S>(property);
			}
		}
		return null;
	}

	[Obsolete("Use AccessTools.MethodDelegate<Action<T, S>>(PropertyInfo.GetSetMethod(true))")]
	public static SetterHandler<T, S> CreateSetterHandler<T, S>(PropertyInfo propertyInfo)
	{
		MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic: true);
		DynamicMethodDefinition obj = CreateSetDynamicMethod<T, S>(propertyInfo.DeclaringType);
		ILGenerator iLGenerator = obj.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Call, setMethod);
		iLGenerator.Emit(OpCodes.Ret);
		return (SetterHandler<T, S>)Extensions.CreateDelegate((MethodBase)obj.Generate(), typeof(SetterHandler<T, S>));
	}

	[Obsolete("Use AccessTools.FieldRefAccess<T, S>(fieldInfo)")]
	public static SetterHandler<T, S> CreateSetterHandler<T, S>(FieldInfo fieldInfo)
	{
		DynamicMethodDefinition obj = CreateSetDynamicMethod<T, S>(fieldInfo.DeclaringType);
		ILGenerator iLGenerator = obj.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Stfld, fieldInfo);
		iLGenerator.Emit(OpCodes.Ret);
		return (SetterHandler<T, S>)Extensions.CreateDelegate((MethodBase)obj.Generate(), typeof(SetterHandler<T, S>));
	}

	private static DynamicMethodDefinition CreateGetDynamicMethod<T, S>(Type type)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		return new DynamicMethodDefinition("DynamicGet_" + type.Name, typeof(S), new Type[1] { typeof(T) });
	}

	private static DynamicMethodDefinition CreateSetDynamicMethod<T, S>(Type type)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		return new DynamicMethodDefinition("DynamicSet_" + type.Name, typeof(void), new Type[2]
		{
			typeof(T),
			typeof(S)
		});
	}
}
