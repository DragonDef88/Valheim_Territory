using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib.Public.Patching;
using HarmonyLib.Tools;

namespace HarmonyLib;

public class PatchClassProcessor
{
	private readonly Harmony instance;

	private readonly Type containerType;

	private readonly HarmonyMethod containerAttributes;

	private readonly Dictionary<Type, MethodInfo> auxilaryMethods;

	private readonly List<AttributePatch> patchMethods;

	private static readonly List<Type> auxilaryTypes = new List<Type>
	{
		typeof(HarmonyPrepare),
		typeof(HarmonyCleanup),
		typeof(HarmonyTargetMethod),
		typeof(HarmonyTargetMethods)
	};

	public PatchClassProcessor(Harmony instance, Type type)
		: this(instance, type, allowUnannotatedType: false)
	{
	}

	public PatchClassProcessor(Harmony instance, Type type, bool allowUnannotatedType)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		this.instance = instance;
		containerType = type;
		List<HarmonyMethod> fromType = HarmonyMethodExtensions.GetFromType(type);
		if (!allowUnannotatedType && (fromType == null || fromType.Count == 0))
		{
			return;
		}
		containerAttributes = HarmonyMethod.Merge(fromType);
		MethodType? methodType = containerAttributes.methodType;
		if (!methodType.HasValue)
		{
			containerAttributes.methodType = MethodType.Normal;
		}
		auxilaryMethods = new Dictionary<Type, MethodInfo>();
		foreach (Type auxilaryType in auxilaryTypes)
		{
			MethodInfo patchMethod = PatchTools.GetPatchMethod(containerType, auxilaryType.FullName);
			if ((object)patchMethod != null)
			{
				auxilaryMethods[auxilaryType] = patchMethod;
			}
		}
		patchMethods = PatchTools.GetPatchMethods(containerType, (object)containerAttributes.GetDeclaringType() != null);
		foreach (AttributePatch patchMethod2 in patchMethods)
		{
			MethodInfo method = patchMethod2.info.method;
			patchMethod2.info = containerAttributes.Merge(patchMethod2.info);
			patchMethod2.info.method = method;
		}
	}

	public List<MethodInfo> Patch()
	{
		if (containerAttributes == null)
		{
			return null;
		}
		Exception exception = null;
		if (!RunMethod<HarmonyPrepare, bool>(defaultIfNotExisting: true, defaultIfFailing: false, null, new object[0]))
		{
			RunMethod<HarmonyCleanup>(ref exception, new object[0]);
			ReportException(exception, null);
			return new List<MethodInfo>();
		}
		List<MethodInfo> result = new List<MethodInfo>();
		MethodBase lastOriginal = null;
		try
		{
			List<MethodBase> bulkMethods = GetBulkMethods();
			if (bulkMethods.Count == 1)
			{
				lastOriginal = bulkMethods[0];
			}
			ReversePatch(ref lastOriginal);
			result = ((bulkMethods.Count > 0) ? BulkPatch(bulkMethods, ref lastOriginal) : PatchWithAttributes(ref lastOriginal));
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		RunMethod<HarmonyCleanup>(ref exception, new object[1] { exception });
		ReportException(exception, lastOriginal);
		return result;
	}

	private void ReversePatch(ref MethodBase lastOriginal)
	{
		for (int i = 0; i < patchMethods.Count; i++)
		{
			AttributePatch attributePatch = patchMethods[i];
			if (attributePatch.type == HarmonyPatchType.ReversePatch)
			{
				MethodBase originalMethod = attributePatch.info.GetOriginalMethod();
				if ((object)originalMethod != null)
				{
					lastOriginal = originalMethod;
				}
				ReversePatcher reversePatcher = instance.CreateReversePatcher(lastOriginal, attributePatch.info);
				lock (PatchProcessor.locker)
				{
					reversePatcher.Patch();
				}
			}
		}
	}

	private List<MethodInfo> BulkPatch(List<MethodBase> originals, ref MethodBase lastOriginal)
	{
		PatchJobs<MethodInfo> patchJobs = new PatchJobs<MethodInfo>();
		for (int i = 0; i < originals.Count; i++)
		{
			lastOriginal = originals[i];
			PatchJobs<MethodInfo>.Job job = patchJobs.GetJob(lastOriginal);
			foreach (AttributePatch patchMethod in patchMethods)
			{
				string text = "You cannot combine TargetMethod, TargetMethods or [HarmonyPatchAll] with individual annotations";
				HarmonyMethod info = patchMethod.info;
				if (info.methodName != null)
				{
					throw new ArgumentException(text + " [" + info.methodName + "]");
				}
				if (info.methodType.HasValue && info.methodType.Value != 0)
				{
					throw new ArgumentException($"{text} [{info.methodType}]");
				}
				if (info.argumentTypes != null)
				{
					throw new ArgumentException(text + " [" + info.argumentTypes.Description() + "]");
				}
				job.AddPatch(patchMethod);
			}
		}
		foreach (PatchJobs<MethodInfo>.Job job2 in patchJobs.GetJobs())
		{
			lastOriginal = job2.original;
			ProcessPatchJob(job2);
		}
		return patchJobs.GetReplacements();
	}

	private List<MethodInfo> PatchWithAttributes(ref MethodBase lastOriginal)
	{
		PatchJobs<MethodInfo> patchJobs = new PatchJobs<MethodInfo>();
		foreach (AttributePatch patchMethod in patchMethods)
		{
			lastOriginal = patchMethod.info.GetOriginalMethod();
			if ((object)lastOriginal == null)
			{
				throw new ArgumentException("Undefined target method for patch method " + patchMethod.info.method.FullDescription());
			}
			patchJobs.GetJob(lastOriginal).AddPatch(patchMethod);
		}
		foreach (PatchJobs<MethodInfo>.Job job in patchJobs.GetJobs())
		{
			lastOriginal = job.original;
			ProcessPatchJob(job);
		}
		return patchJobs.GetReplacements();
	}

	private void ProcessPatchJob(PatchJobs<MethodInfo>.Job job)
	{
		MethodInfo replacement = null;
		bool num = RunMethod<HarmonyPrepare, bool>(defaultIfNotExisting: true, defaultIfFailing: false, null, new object[1] { job.original });
		Exception exception = null;
		if (num)
		{
			lock (PatchProcessor.locker)
			{
				try
				{
					PatchInfo patchInfo = job.original.ToPatchInfo();
					patchInfo.AddPrefixes(instance.Id, job.prefixes.ToArray());
					patchInfo.AddPostfixes(instance.Id, job.postfixes.ToArray());
					patchInfo.AddTranspilers(instance.Id, job.transpilers.ToArray());
					patchInfo.AddFinalizers(instance.Id, job.finalizers.ToArray());
					patchInfo.AddILManipulators(instance.Id, job.ilmanipulators.ToArray());
					replacement = PatchFunctions.UpdateWrapper(job.original, patchInfo);
					PatchManager.AddReplacementOriginal(job.original, replacement);
				}
				catch (Exception ex)
				{
					exception = ex;
				}
			}
		}
		RunMethod<HarmonyCleanup>(ref exception, new object[2] { job.original, exception });
		ReportException(exception, job.original);
		job.replacement = replacement;
	}

	private List<MethodBase> GetBulkMethods()
	{
		if (containerType.GetCustomAttributes(inherit: true).Any((object a) => a.GetType().FullName == typeof(HarmonyPatchAll).FullName))
		{
			Type declaringType = containerAttributes.GetDeclaringType();
			if ((object)declaringType == null)
			{
				throw new ArgumentException("Using " + typeof(HarmonyPatchAll).FullName + " requires an additional attribute for specifying the Class/Type");
			}
			List<MethodBase> list = new List<MethodBase>();
			list.AddRange(AccessTools.GetDeclaredConstructors(declaringType).Cast<MethodBase>());
			list.AddRange(AccessTools.GetDeclaredMethods(declaringType).Cast<MethodBase>());
			List<PropertyInfo> declaredProperties = AccessTools.GetDeclaredProperties(declaringType);
			list.AddRange((from prop in declaredProperties
				select prop.GetGetMethod(nonPublic: true) into method
				where (object)method != null
				select method).Cast<MethodBase>());
			list.AddRange((from prop in declaredProperties
				select prop.GetSetMethod(nonPublic: true) into method
				where (object)method != null
				select method).Cast<MethodBase>());
			return list;
		}
		IEnumerable<MethodBase> enumerable = RunMethod<HarmonyTargetMethods, IEnumerable<MethodBase>>(null, null, FailOnResult, new object[0]);
		if (enumerable != null)
		{
			return enumerable.ToList();
		}
		List<MethodBase> list2 = new List<MethodBase>();
		MethodBase methodBase = RunMethod<HarmonyTargetMethod, MethodBase>(null, null, (MethodBase method) => ((object)method != null) ? null : "null", new object[0]);
		if ((object)methodBase != null)
		{
			list2.Add(methodBase);
		}
		return list2;
		static string FailOnResult(IEnumerable<MethodBase> res)
		{
			if (res == null)
			{
				return "null";
			}
			if (res.Any((MethodBase m) => (object)m == null))
			{
				return "some element was null";
			}
			return null;
		}
	}

	private void ReportException(Exception exception, MethodBase original)
	{
		if (exception == null)
		{
			return;
		}
		Logger.Log(Logger.LogChannel.Debug, delegate
		{
			Harmony.VersionInfo(out var currentVersion);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"### Exception from user \"{instance.Id}\", Harmony v{currentVersion}");
			stringBuilder.AppendLine("### Original: " + (original?.FullDescription() ?? "NULL"));
			stringBuilder.AppendLine("### Patch class: " + containerType.FullDescription());
			Exception ex = exception;
			if (ex is HarmonyException ex2)
			{
				ex = ex2.InnerException;
			}
			string text = ex.ToString();
			while (text.Contains("\n\n"))
			{
				text = text.Replace("\n\n", "\n");
			}
			text = text.Split(new char[1] { '\n' }).Join((string line) => "### " + line, "\n");
			stringBuilder.AppendLine(text.Trim());
			return stringBuilder.ToString();
		});
		if (exception is HarmonyException)
		{
			throw exception;
		}
		throw new HarmonyException("Patching exception in method " + original.FullDescription(), exception);
	}

	private T RunMethod<S, T>(T defaultIfNotExisting, T defaultIfFailing, Func<T, string> failOnResult = null, params object[] parameters)
	{
		if (auxilaryMethods.TryGetValue(typeof(S), out var value))
		{
			object[] inputs = (parameters ?? new object[0]).Union(new object[1] { instance }).ToArray();
			object[] parameters2 = AccessTools.ActualParameters(value, inputs);
			if ((object)value.ReturnType != typeof(void) && !typeof(T).IsAssignableFrom(value.ReturnType))
			{
				throw new Exception("Method " + value.FullDescription() + " has wrong return type (should be assignable to " + typeof(T).FullName + ")");
			}
			T val = defaultIfFailing;
			try
			{
				if ((object)value.ReturnType == typeof(void))
				{
					value.Invoke(null, parameters2);
					val = defaultIfNotExisting;
				}
				else
				{
					val = (T)value.Invoke(null, parameters2);
				}
				if (failOnResult != null)
				{
					string text = failOnResult(val);
					if (text != null)
					{
						throw new Exception("Method " + value.FullDescription() + " returned an unexpected result: " + text);
					}
				}
			}
			catch (Exception exception)
			{
				ReportException(exception, value);
			}
			return val;
		}
		return defaultIfNotExisting;
	}

	private void RunMethod<S>(ref Exception exception, params object[] parameters)
	{
		if (!auxilaryMethods.TryGetValue(typeof(S), out var value))
		{
			return;
		}
		object[] inputs = (parameters ?? new object[0]).Union(new object[1] { instance }).ToArray();
		object[] parameters2 = AccessTools.ActualParameters(value, inputs);
		try
		{
			object obj = value.Invoke(null, parameters2);
			if ((object)value.ReturnType == typeof(Exception))
			{
				exception = obj as Exception;
			}
		}
		catch (Exception exception2)
		{
			ReportException(exception2, value);
		}
	}
}
