using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib.Tools;
using MonoMod.RuntimeDetour;

namespace HarmonyLib.Internal.RuntimeFixes;

internal static class StackTraceFixes
{
	private static bool _applied;

	private static readonly Dictionary<MethodBase, MethodBase> RealMethodMap = new Dictionary<MethodBase, MethodBase>();

	private static Func<Assembly> _realGetAss;

	private static Func<StackFrame, MethodBase> _origGetMethod;

	private static Action<object> _origRefresh;

	public static void Install()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (!_applied)
		{
			try
			{
				_origRefresh = new Detour((MethodBase)AccessTools.Method(AccessTools.Inner(typeof(ILHook), "Context"), "Refresh"), (MethodBase)AccessTools.Method(typeof(StackTraceFixes), "OnILChainRefresh")).GenerateTrampoline<Action<object>>();
				_origGetMethod = new Detour((MethodBase)AccessTools.Method(typeof(StackFrame), "GetMethod"), (MethodBase)AccessTools.Method(typeof(StackTraceFixes), "GetMethodFix")).GenerateTrampoline<Func<StackFrame, MethodBase>>();
				_realGetAss = new NativeDetour((MethodBase)AccessTools.Method(typeof(Assembly), "GetExecutingAssembly"), (MethodBase)AccessTools.Method(typeof(StackTraceFixes), "GetAssemblyFix")).GenerateTrampoline<Func<Assembly>>();
			}
			catch (Exception ex)
			{
				Logger.LogText(Logger.LogChannel.Error, "Failed to apply stack trace fix: (" + ex.GetType().FullName + ") " + ex.Message);
			}
			_applied = true;
		}
	}

	private static MethodBase GetMethodFix(StackFrame self)
	{
		MethodBase methodBase = _origGetMethod(self);
		if ((object)methodBase == null)
		{
			return null;
		}
		lock (RealMethodMap)
		{
			MethodBase value;
			return RealMethodMap.TryGetValue(methodBase, out value) ? value : methodBase;
		}
	}

	private static Assembly GetAssemblyFix()
	{
		return new StackFrame(1).GetMethod()?.Module.Assembly ?? _realGetAss();
	}

	private static void OnILChainRefresh(object self)
	{
		_origRefresh(self);
		object? value = AccessTools.Field(self.GetType(), "Detour").GetValue(self);
		Detour val = (Detour)((value is Detour) ? value : null);
		if (val == null)
		{
			return;
		}
		lock (RealMethodMap)
		{
			RealMethodMap[val.Target] = val.Method;
		}
	}
}
