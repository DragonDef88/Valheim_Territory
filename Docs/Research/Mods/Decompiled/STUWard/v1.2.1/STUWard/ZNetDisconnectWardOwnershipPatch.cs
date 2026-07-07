using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(ZNet), "Disconnect")]
internal static class ZNetDisconnectWardOwnershipPatch
{
	private static void Prefix(ZNet __instance, ZNetPeer peer)
	{
		if (__instance.IsServer())
		{
			WardOwnership.ForgetServerSessionIdentity(peer);
		}
	}
}
