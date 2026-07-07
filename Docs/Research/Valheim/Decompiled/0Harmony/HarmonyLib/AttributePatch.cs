using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HarmonyLib;

internal class AttributePatch
{
	private static readonly HarmonyPatchType[] allPatchTypes = new HarmonyPatchType[6]
	{
		HarmonyPatchType.Prefix,
		HarmonyPatchType.Postfix,
		HarmonyPatchType.Transpiler,
		HarmonyPatchType.Finalizer,
		HarmonyPatchType.ReversePatch,
		HarmonyPatchType.ILManipulator
	};

	internal HarmonyMethod info;

	internal HarmonyPatchType? type;

	private static readonly string harmonyAttributeName = typeof(HarmonyAttribute).FullName;

	internal static IEnumerable<AttributePatch> Create(MethodInfo patch, bool collectIncomplete = false)
	{
		if ((object)patch == null)
		{
			throw new NullReferenceException("Patch method cannot be null");
		}
		object[] customAttributes = patch.GetCustomAttributes(inherit: true);
		string name = patch.Name;
		HarmonyPatchType? type = GetPatchType(name, customAttributes);
		if (!type.HasValue)
		{
			return Enumerable.Empty<AttributePatch>();
		}
		if (type != HarmonyPatchType.ReversePatch && !patch.IsStatic)
		{
			throw new ArgumentException("Patch method " + patch.FullDescription() + " must be static");
		}
		List<HarmonyMethod> list = (from attr in customAttributes
			where attr.GetType().BaseType.FullName == harmonyAttributeName
			select AccessTools.Field(attr.GetType(), "info").GetValue(attr) into harmonyInfo
			select AccessTools.MakeDeepCopy<HarmonyMethod>(harmonyInfo)).ToList();
		List<HarmonyMethod> list2 = new List<HarmonyMethod>();
		ILookup<bool, HarmonyMethod> lookup = list.ToLookup((HarmonyMethod m) => IsComplete(m, collectIncomplete));
		List<HarmonyMethod> incomplete = lookup[false].ToList();
		HarmonyMethod info = HarmonyMethod.Merge(incomplete);
		List<HarmonyMethod> list3 = lookup[true].Where((HarmonyMethod m) => !Same(m, info)).ToList();
		if (list3.Count > 1)
		{
			list2.AddRange(list3.Select((HarmonyMethod m) => HarmonyMethod.Merge(incomplete.AddItem(m))));
		}
		else
		{
			list2.Add(HarmonyMethod.Merge(list));
		}
		foreach (HarmonyMethod item in list2)
		{
			item.method = patch;
		}
		return list2.Select((HarmonyMethod i) => new AttributePatch
		{
			info = i,
			type = type
		}).ToList();
		static bool IsComplete(HarmonyMethod m, bool collectIncomplete)
		{
			if (collectIncomplete || (object)m.GetDeclaringType() != null)
			{
				return m.methodName != null;
			}
			return false;
		}
		static bool Same(HarmonyMethod m1, HarmonyMethod m2)
		{
			if ((object)m1.GetDeclaringType() == m2.GetDeclaringType() && m1.methodName == m2.methodName)
			{
				return m1.GetArgumentList().SequenceEqual(m2.GetArgumentList());
			}
			return false;
		}
	}

	private static HarmonyPatchType? GetPatchType(string methodName, object[] allAttributes)
	{
		HashSet<string> hashSet = new HashSet<string>(from attr in allAttributes
			select attr.GetType().FullName into name
			where name.StartsWith("Harmony")
			select name);
		HarmonyPatchType? result = null;
		HarmonyPatchType[] array = allPatchTypes;
		for (int i = 0; i < array.Length; i++)
		{
			HarmonyPatchType value = array[i];
			string text = value.ToString();
			if (text == methodName || hashSet.Contains("HarmonyLib.Harmony" + text))
			{
				result = value;
				break;
			}
		}
		return result;
	}
}
