using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(ZNet), "RPC_CharacterID")]
internal static class ZNetRpcCharacterIdWardOwnershipPatch
{
	private static void Postfix(ZNet __instance, ZRpc rpc, ZDOID characterID)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (__instance.IsServer())
		{
			WardOwnership.RefreshServerSessionIdentity(__instance.GetPeer(rpc), characterID);
		}
	}
}
