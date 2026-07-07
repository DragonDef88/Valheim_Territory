using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HarmonyLib;

public class Patches
{
	public readonly ReadOnlyCollection<Patch> Prefixes;

	public readonly ReadOnlyCollection<Patch> Postfixes;

	public readonly ReadOnlyCollection<Patch> Transpilers;

	public readonly ReadOnlyCollection<Patch> Finalizers;

	public readonly ReadOnlyCollection<Patch> ILManipulators;

	public ReadOnlyCollection<string> Owners
	{
		get
		{
			HashSet<string> hashSet = new HashSet<string>();
			hashSet.UnionWith(Prefixes.Select((Patch p) => p.owner));
			hashSet.UnionWith(Postfixes.Select((Patch p) => p.owner));
			hashSet.UnionWith(Transpilers.Select((Patch p) => p.owner));
			hashSet.UnionWith(Finalizers.Select((Patch p) => p.owner));
			hashSet.UnionWith(ILManipulators.Select((Patch p) => p.owner));
			return hashSet.ToList().AsReadOnly();
		}
	}

	public Patches(Patch[] prefixes, Patch[] postfixes, Patch[] transpilers, Patch[] finalizers, Patch[] ilmanipulators)
	{
		if (prefixes == null)
		{
			prefixes = new Patch[0];
		}
		if (postfixes == null)
		{
			postfixes = new Patch[0];
		}
		if (transpilers == null)
		{
			transpilers = new Patch[0];
		}
		if (finalizers == null)
		{
			finalizers = new Patch[0];
		}
		if (ilmanipulators == null)
		{
			ilmanipulators = new Patch[0];
		}
		Prefixes = prefixes.ToList().AsReadOnly();
		Postfixes = postfixes.ToList().AsReadOnly();
		Transpilers = transpilers.ToList().AsReadOnly();
		Finalizers = finalizers.ToList().AsReadOnly();
		ILManipulators = ilmanipulators.ToList().AsReadOnly();
	}

	[Obsolete("Use newer constructor instead", true)]
	public Patches(Patch[] prefixes, Patch[] postfixes, Patch[] transpilers, Patch[] finalizers)
		: this(prefixes, postfixes, transpilers, finalizers, null)
	{
	}
}
