using HarmonyLib;
using LocalizationManager;

namespace STUWard;

[HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
internal static class ObjectDBCopyOtherDbPatch
{
	private static void Postfix()
	{
		Localizer.ReloadCurrentLanguageIfAvailable();
		StuWardPrefab.ApplyRecipeSettings();
	}
}
