using HarmonyLib;
using LocalizationManager;

namespace STUWard;

[HarmonyPatch(typeof(ObjectDB), "Awake")]
internal static class ObjectDBAwakePatch
{
	private static void Postfix()
	{
		Localizer.ReloadCurrentLanguageIfAvailable();
		StuWardPrefab.ApplyRecipeSettings();
	}
}
