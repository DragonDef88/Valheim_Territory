using System;
using JetBrains.Annotations;

namespace HarmonyLib;

[MeansImplicitUse]
public class HarmonyAttribute : Attribute
{
	public HarmonyMethod info = new HarmonyMethod();
}
