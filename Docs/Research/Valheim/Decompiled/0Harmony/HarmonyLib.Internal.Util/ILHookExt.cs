using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace HarmonyLib.Internal.Util;

internal class ILHookExt : ILHook
{
	public string dumpPath;

	public ILHookExt(MethodBase from, Manipulator manipulator, ILHookConfig config)
		: base(from, manipulator, config)
	{
	}//IL_0003: Unknown result type (might be due to invalid IL or missing references)

}
