using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
internal static class ZNetRpcPeerInfoWardOwnershipPatch
{
	private static void Postfix(ZNet __instance, ZRpc rpc)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (__instance.IsServer())
		{
			WardOwnership.RefreshServerSessionIdentity(__instance.GetPeer(rpc));
		}
	}
}
