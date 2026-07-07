using System.Reflection;
using MonoMod.Utils;

namespace HarmonyLib.Public.Patching;

public abstract class MethodPatcher
{
	public MethodBase Original { get; }

	protected MethodPatcher(MethodBase original)
	{
		Original = original;
	}

	public abstract DynamicMethodDefinition PrepareOriginal();

	public abstract MethodBase DetourTo(MethodBase replacement);

	public abstract DynamicMethodDefinition CopyOriginal();
}
