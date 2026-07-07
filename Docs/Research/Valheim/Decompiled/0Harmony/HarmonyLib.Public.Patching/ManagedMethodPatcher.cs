using System;
using System.Reflection;
using HarmonyLib.Internal.Util;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace HarmonyLib.Public.Patching;

public class ManagedMethodPatcher : MethodPatcher
{
	private MethodBody hookBody;

	private ILHook ilHook;

	public ManagedMethodPatcher(MethodBase original)
		: base(original)
	{
	}

	public override DynamicMethodDefinition PrepareOriginal()
	{
		return null;
	}

	public override MethodBase DetourTo(MethodBase replacement)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (ilHook == null)
		{
			ilHook = new ILHook(base.Original, new Manipulator(Manipulator), new ILHookConfig
			{
				ManualApply = true
			});
		}
		ILHookExtensions.SetIsApplied(ilHook, arg2: false);
		try
		{
			ilHook.Apply();
		}
		catch (Exception ex)
		{
			throw HarmonyException.Create(ex, hookBody);
		}
		return ilHook.GetCurrentTarget();
	}

	public override DynamicMethodDefinition CopyOriginal()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new DynamicMethodDefinition(base.Original);
	}

	private void Manipulator(ILContext ctx)
	{
		hookBody = ctx.Body;
		HarmonyManipulator.Manipulate(base.Original, ctx);
	}

	public static void TryResolve(object sender, PatchManager.PatcherResolverEventArgs args)
	{
		if (args.Original.GetMethodBody() != null)
		{
			args.MethodPatcher = new ManagedMethodPatcher(args.Original);
		}
	}
}
