using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HarmonyLib;

public class HarmonyMethod
{
	public MethodInfo method;

	public Type declaringType;

	public string methodName;

	public MethodType? methodType;

	public Type[] argumentTypes;

	public int priority = -1;

	public string[] before;

	public string[] after;

	public HarmonyReversePatchType? reversePatchType;

	public bool? debug;

	public string debugEmitPath;

	public bool nonVirtualDelegate;

	public bool? wrapTryCatch;

	internal string assemblyQualifiedDeclaringTypeName;

	public HarmonyMethod()
	{
	}

	private void ImportMethod(MethodInfo theMethod)
	{
		method = theMethod;
		if ((object)method != null)
		{
			List<HarmonyMethod> fromMethod = HarmonyMethodExtensions.GetFromMethod(method);
			if (fromMethod != null)
			{
				Merge(fromMethod).CopyTo(this);
			}
		}
	}

	public HarmonyMethod(MethodInfo method)
	{
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		ImportMethod(method);
	}

	public HarmonyMethod(MethodInfo method, int priority = -1, string[] before = null, string[] after = null, bool? debug = null)
	{
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		ImportMethod(method);
		this.priority = priority;
		this.before = before;
		this.after = after;
		this.debug = debug;
	}

	public HarmonyMethod(Type methodType, string methodName, Type[] argumentTypes = null)
	{
		MethodInfo methodInfo = AccessTools.Method(methodType, methodName, argumentTypes);
		if ((object)methodInfo == null)
		{
			throw new ArgumentException($"Cannot not find method for type {methodType} and name {methodName} and parameters {argumentTypes?.Description()}");
		}
		ImportMethod(methodInfo);
	}

	public static List<string> HarmonyFields()
	{
		return (from s in AccessTools.GetFieldNames(typeof(HarmonyMethod))
			where s != "method"
			select s).ToList();
	}

	public static HarmonyMethod Merge(List<HarmonyMethod> attributes)
	{
		return Merge((IEnumerable<HarmonyMethod>)attributes);
	}

	internal static HarmonyMethod Merge(IEnumerable<HarmonyMethod> attributes)
	{
		HarmonyMethod harmonyMethod = new HarmonyMethod();
		if (attributes == null)
		{
			return harmonyMethod;
		}
		Traverse resultTrv = Traverse.Create(harmonyMethod);
		attributes.Do(delegate(HarmonyMethod attribute)
		{
			Traverse trv = Traverse.Create(attribute);
			HarmonyFields().ForEach(delegate(string f)
			{
				object value = trv.Field(f).GetValue();
				if (value != null && (f != "priority" || (int)value != -1))
				{
					HarmonyMethodExtensions.SetValue(resultTrv, f, value);
				}
			});
		});
		return harmonyMethod;
	}

	public override string ToString()
	{
		string result = "";
		Traverse trv = Traverse.Create(this);
		HarmonyFields().ForEach(delegate(string f)
		{
			if (result.Length > 0)
			{
				result += ", ";
			}
			result += $"{f}={trv.Field(f).GetValue()}";
		});
		return "HarmonyMethod[" + result + "]";
	}

	internal string Description()
	{
		string text = (((object)declaringType != null) ? declaringType.FullName : ((assemblyQualifiedDeclaringTypeName != null) ? assemblyQualifiedDeclaringTypeName : "undefined"));
		string text2 = methodName ?? "undefined";
		string text3 = (methodType.HasValue ? methodType.Value.ToString() : "undefined");
		string text4 = ((argumentTypes != null) ? argumentTypes.Description() : "undefined");
		return "(class=" + text + ", methodname=" + text2 + ", type=" + text3 + ", args=" + text4 + ")";
	}

	internal Type GetDeclaringType()
	{
		if ((object)declaringType == null && assemblyQualifiedDeclaringTypeName != null)
		{
			declaringType = Type.GetType(assemblyQualifiedDeclaringTypeName, throwOnError: true);
		}
		return declaringType;
	}

	internal Type[] GetArgumentList()
	{
		return argumentTypes ?? EmptyType.NoArgs;
	}
}
