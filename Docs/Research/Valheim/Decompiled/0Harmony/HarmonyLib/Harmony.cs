using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib.Internal.RuntimeFixes;
using HarmonyLib.Public.Patching;
using HarmonyLib.Tools;
using MonoMod.Utils;

namespace HarmonyLib;

public class Harmony : IDisposable
{
	[Obsolete("Use HarmonyFileLog.Enabled instead")]
	public static bool DEBUG;

	public string Id { get; }

	static Harmony()
	{
		StackTraceFixes.Install();
	}

	public Harmony(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentException("id cannot be null or empty");
		}
		try
		{
			string environmentVariable = Environment.GetEnvironmentVariable("HARMONY_DEBUG");
			if (environmentVariable != null && environmentVariable.Length > 0)
			{
				environmentVariable = environmentVariable.Trim();
				DEBUG = environmentVariable == "1" || bool.Parse(environmentVariable);
			}
		}
		catch
		{
		}
		if (DEBUG)
		{
			HarmonyFileLog.Enabled = true;
		}
		MethodBase callingMethod = (Logger.IsEnabledFor(Logger.LogChannel.Info) ? AccessTools.GetOutsideCaller() : null);
		Logger.Log(Logger.LogChannel.Info, delegate
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			StringBuilder stringBuilder = new StringBuilder();
			Assembly assembly = typeof(Harmony).Assembly;
			Version version = assembly.GetName().Version;
			string text = assembly.Location;
			string text2 = Environment.Version.ToString();
			string text3 = Environment.OSVersion.Platform.ToString();
			if (string.IsNullOrEmpty(text))
			{
				text = new Uri(assembly.CodeBase).LocalPath;
			}
			int size = IntPtr.Size;
			Platform current = PlatformHelper.Current;
			stringBuilder.AppendLine($"### Harmony id={id}, version={version}, location={text}, env/clr={text2}, platform={text3}, ptrsize:runtime/env={size}/{current}");
			if ((object)callingMethod?.DeclaringType != null)
			{
				Assembly assembly2 = callingMethod.DeclaringType.Assembly;
				text = assembly2.Location;
				if (string.IsNullOrEmpty(text))
				{
					text = new Uri(assembly2.CodeBase).LocalPath;
				}
				stringBuilder.AppendLine("### Started from " + callingMethod.FullDescription() + ", location " + text);
				stringBuilder.Append($"### At {DateTime.Now:yyyy-MM-dd hh.mm.ss}");
			}
			return stringBuilder.ToString();
		});
		Id = id;
	}

	public void PatchAll()
	{
		Assembly assembly = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Assembly;
		PatchAll(assembly);
	}

	public PatchProcessor CreateProcessor(MethodBase original)
	{
		return new PatchProcessor(this, original);
	}

	public PatchClassProcessor CreateClassProcessor(Type type)
	{
		return new PatchClassProcessor(this, type);
	}

	public PatchClassProcessor CreateClassProcessor(Type type, bool allowUnannotatedType)
	{
		return new PatchClassProcessor(this, type, allowUnannotatedType);
	}

	public ReversePatcher CreateReversePatcher(MethodBase original, HarmonyMethod standin)
	{
		return new ReversePatcher(this, original, standin);
	}

	public void PatchAll(Assembly assembly)
	{
		AccessTools.GetTypesFromAssembly(assembly).Do(delegate(Type type)
		{
			CreateClassProcessor(type).Patch();
		});
	}

	public void PatchAll(Type type)
	{
		CreateClassProcessor(type, allowUnannotatedType: true).Patch();
	}

	public MethodInfo Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null, HarmonyMethod ilmanipulator = null)
	{
		PatchProcessor patchProcessor = CreateProcessor(original);
		patchProcessor.AddPrefix(prefix);
		patchProcessor.AddPostfix(postfix);
		patchProcessor.AddTranspiler(transpiler);
		patchProcessor.AddFinalizer(finalizer);
		patchProcessor.AddILManipulator(ilmanipulator);
		return patchProcessor.Patch();
	}

	[Obsolete("Use newer Patch() instead", true)]
	public MethodInfo Patch(MethodBase original, HarmonyMethod prefix, HarmonyMethod postfix, HarmonyMethod transpiler, HarmonyMethod finalizer)
	{
		return Patch(original, prefix, postfix, transpiler, finalizer, null);
	}

	public static MethodInfo ReversePatch(MethodBase original, HarmonyMethod standin, MethodInfo transpiler = null, MethodInfo ilmanipulator = null)
	{
		return PatchFunctions.ReversePatch(standin, original, transpiler, ilmanipulator);
	}

	[Obsolete("Use newer ReversePatch() instead", true)]
	public static MethodInfo ReversePatch(MethodBase original, HarmonyMethod standin, MethodInfo transpiler)
	{
		return PatchFunctions.ReversePatch(standin, original, transpiler, null);
	}

	public static void UnpatchID(string harmonyID)
	{
		if (string.IsNullOrEmpty(harmonyID))
		{
			throw new ArgumentNullException("harmonyID", "UnpatchID was called with a null or empty harmonyID.");
		}
		PatchFunctions.UnpatchConditional((Patch patchInfo) => patchInfo.owner == harmonyID);
	}

	void IDisposable.Dispose()
	{
		UnpatchSelf();
	}

	public void UnpatchSelf()
	{
		UnpatchID(Id);
	}

	public static void UnpatchAll()
	{
		Logger.Log(Logger.LogChannel.Warn, () => "UnpatchAll has been called - This will remove ALL HARMONY PATCHES.");
		PatchFunctions.UnpatchConditional((Patch _) => true);
	}

	[Obsolete("Use UnpatchSelf() to unpatch the current instance. The functionality to unpatch either other ids or EVERYTHING has been moved the static methods UnpatchID() and UnpatchAll() respectively", true)]
	public void UnpatchAll(string harmonyID = null)
	{
		if (harmonyID == null)
		{
			if (HarmonyGlobalSettings.DisallowLegacyGlobalUnpatchAll)
			{
				Logger.Log(Logger.LogChannel.Warn, () => "Legacy UnpatchAll has been called AND DisallowLegacyGlobalUnpatchAll=true. Skipping execution of UnpatchAll");
			}
			else
			{
				UnpatchAll();
			}
		}
		else if (harmonyID.Length == 0)
		{
			Logger.Log(Logger.LogChannel.Warn, () => "Legacy UnpatchAll was called with harmonyID=\"\" which is an invalid id. Skipping execution of UnpatchAll");
		}
		else
		{
			UnpatchID(harmonyID);
		}
	}

	public void Unpatch(MethodBase original, HarmonyPatchType type, string harmonyID = null)
	{
		CreateProcessor(original).Unpatch(type, harmonyID);
	}

	public void Unpatch(MethodBase original, MethodInfo patch)
	{
		CreateProcessor(original).Unpatch(patch);
	}

	public static bool HasAnyPatches(string harmonyID)
	{
		return (from original in GetAllPatchedMethods()
			select GetPatchInfo(original)).Any((Patches info) => info.Owners.Contains(harmonyID));
	}

	public static Patches GetPatchInfo(MethodBase method)
	{
		return PatchProcessor.GetPatchInfo(method);
	}

	public IEnumerable<MethodBase> GetPatchedMethods()
	{
		return from original in GetAllPatchedMethods()
			where GetPatchInfo(original).Owners.Contains(Id)
			select original;
	}

	public static IEnumerable<MethodBase> GetAllPatchedMethods()
	{
		return PatchProcessor.GetAllPatchedMethods();
	}

	public static MethodBase GetOriginalMethod(MethodInfo replacement)
	{
		if ((object)replacement == null)
		{
			throw new ArgumentNullException("replacement");
		}
		return PatchManager.GetOriginal(replacement);
	}

	public static MethodBase GetMethodFromStackframe(StackFrame frame)
	{
		if (frame == null)
		{
			throw new ArgumentNullException("frame");
		}
		return PatchManager.FindReplacement(frame) ?? frame.GetMethod();
	}

	public static Dictionary<string, Version> VersionInfo(out Version currentVersion)
	{
		return PatchProcessor.VersionInfo(out currentVersion);
	}

	public static Harmony CreateAndPatchAll(Type type, string harmonyInstanceId = null)
	{
		Harmony harmony = new Harmony(harmonyInstanceId ?? $"harmony-auto-{Guid.NewGuid()}");
		harmony.PatchAll(type);
		return harmony;
	}

	public static Harmony CreateAndPatchAll(Assembly assembly, string harmonyInstanceId = null)
	{
		Harmony harmony = new Harmony(harmonyInstanceId ?? $"harmony-auto-{Guid.NewGuid()}");
		harmony.PatchAll(assembly);
		return harmony;
	}
}
