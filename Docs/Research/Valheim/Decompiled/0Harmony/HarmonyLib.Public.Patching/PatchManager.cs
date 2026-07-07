using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace HarmonyLib.Public.Patching;

public static class PatchManager
{
	public class PatcherResolverEventArgs : EventArgs
	{
		public MethodBase Original { get; internal set; }

		public MethodPatcher MethodPatcher { get; set; }
	}

	private static readonly Dictionary<MethodBase, PatchInfo> PatchInfos;

	private static readonly Dictionary<MethodBase, MethodPatcher> MethodPatchers;

	private static readonly List<KeyValuePair<WeakReference, MethodBase>> ReplacementToOriginals;

	private static FieldInfo methodAddress;

	public static event EventHandler<PatcherResolverEventArgs> ResolvePatcher;

	static PatchManager()
	{
		PatchInfos = new Dictionary<MethodBase, PatchInfo>();
		MethodPatchers = new Dictionary<MethodBase, MethodPatcher>();
		ReplacementToOriginals = new List<KeyValuePair<WeakReference, MethodBase>>();
		ResolvePatcher += ManagedMethodPatcher.TryResolve;
		ResolvePatcher += NativeDetourMethodPatcher.TryResolve;
	}

	public static MethodPatcher GetMethodPatcher(this MethodBase methodBase)
	{
		lock (MethodPatchers)
		{
			if (MethodPatchers.TryGetValue(methodBase, out var value))
			{
				return value;
			}
			PatcherResolverEventArgs patcherResolverEventArgs = new PatcherResolverEventArgs
			{
				Original = methodBase
			};
			PatchManager.ResolvePatcher?.Invoke(null, patcherResolverEventArgs);
			if (patcherResolverEventArgs.MethodPatcher == null)
			{
				throw new NullReferenceException("No suitable patcher found for " + methodBase.FullDescription());
			}
			return MethodPatchers[methodBase] = patcherResolverEventArgs.MethodPatcher;
		}
	}

	public static PatchInfo GetPatchInfo(this MethodBase methodBase)
	{
		lock (PatchInfos)
		{
			return PatchInfos.GetValueSafe(methodBase);
		}
	}

	public static PatchInfo ToPatchInfo(this MethodBase methodBase)
	{
		lock (PatchInfos)
		{
			if (PatchInfos.TryGetValue(methodBase, out var value))
			{
				return value;
			}
			return PatchInfos[methodBase] = new PatchInfo();
		}
	}

	public static IEnumerable<MethodBase> GetPatchedMethods()
	{
		lock (PatchInfos)
		{
			return PatchInfos.Keys.ToList();
		}
	}

	internal static MethodBase GetOriginal(MethodInfo replacement)
	{
		lock (ReplacementToOriginals)
		{
			ReplacementToOriginals.RemoveAll((KeyValuePair<WeakReference, MethodBase> kv) => !kv.Key.IsAlive);
			foreach (KeyValuePair<WeakReference, MethodBase> replacementToOriginal in ReplacementToOriginals)
			{
				if ((object)(replacementToOriginal.Key.Target as MethodInfo) == replacement)
				{
					return replacementToOriginal.Value;
				}
			}
			return null;
		}
	}

	internal static MethodBase FindReplacement(StackFrame frame)
	{
		if ((object)methodAddress == null)
		{
			methodAddress = typeof(StackFrame).GetField("methodAddress", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		MethodBase method = frame.GetMethod();
		long methodStart = 0L;
		if ((object)method == null)
		{
			if ((object)methodAddress == null)
			{
				return null;
			}
			methodStart = (long)methodAddress.GetValue(frame);
		}
		else
		{
			MethodBase identifiable = DetourHelper.Runtime.GetIdentifiable(method);
			methodStart = DetourHelper.GetNativeStart(identifiable).ToInt64();
		}
		if (methodStart == 0L)
		{
			return method;
		}
		lock (ReplacementToOriginals)
		{
			return ReplacementToOriginals.FirstOrDefault((KeyValuePair<WeakReference, MethodBase> kv) => kv.Key.IsAlive && DetourHelper.GetNativeStart((MethodBase)kv.Key.Target).ToInt64() == methodStart).Key.Target as MethodBase;
		}
	}

	internal static void AddReplacementOriginal(MethodBase original, MethodInfo replacement)
	{
		if ((object)replacement == null)
		{
			return;
		}
		lock (ReplacementToOriginals)
		{
			ReplacementToOriginals.Add(new KeyValuePair<WeakReference, MethodBase>(new WeakReference(replacement), original));
		}
	}

	public static void ClearAllPatcherResolvers()
	{
		PatchManager.ResolvePatcher = null;
	}
}
