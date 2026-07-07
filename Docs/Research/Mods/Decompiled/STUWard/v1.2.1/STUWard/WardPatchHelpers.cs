using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace STUWard;

internal static class WardPatchHelpers
{
	internal enum ProtectedRpcDecision
	{
		Allow,
		Deny,
		Unresolved
	}

	private sealed class StumpClassification
	{
		internal bool Initialized;

		internal string CachedName = string.Empty;

		internal bool IsLikelyStump;
	}

	private static readonly string[] StumpNameTokens = new string[3] { "stub", "stump", "stomp" };

	private static readonly ConditionalWeakTable<GameObject, StumpClassification> StumpClassificationCache = new ConditionalWeakTable<GameObject, StumpClassification>();

	internal static Piece? FindRemoveTarget(Player player)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Piece hoveringPiece = player.GetHoveringPiece();
		if ((Object)(object)hoveringPiece != (Object)null)
		{
			return hoveringPiece;
		}
		GameCamera instance = GameCamera.instance;
		if ((Object)(object)instance == (Object)null || (Object)(object)((Character)player).m_eye == (Object)null)
		{
			return null;
		}
		RaycastHit val = default(RaycastHit);
		if (!Physics.Raycast(((Component)instance).transform.position, ((Component)instance).transform.forward, ref val, 50f, player.m_removeRayMask))
		{
			return null;
		}
		if (Vector3.Distance(((RaycastHit)(ref val)).point, ((Character)player).m_eye.position) >= player.m_maxPlaceDistance)
		{
			return null;
		}
		Piece componentInParent = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<Piece>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			return componentInParent;
		}
		if (!((Object)(object)((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>() != (Object)null))
		{
			return null;
		}
		return TerrainModifier.FindClosestModifierPieceInRange(((RaycastHit)(ref val)).point, 2.5f);
	}

	internal static bool ShouldBlockLocalRemoval(Piece? piece)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)piece == (Object)null || (Object)(object)localPlayer == (Object)null || !((Character)localPlayer).InPlaceMode())
		{
			return false;
		}
		if ((Object)(object)localPlayer.GetHoveringPiece() != (Object)(object)piece && (Object)(object)FindRemoveTarget(localPlayer) != (Object)(object)piece)
		{
			return false;
		}
		PrivateArea component = ((Component)piece).GetComponent<PrivateArea>();
		if (ManagedWardIdentity.EnsureManagedComponent(component))
		{
			return !WardAccess.CanControlManagedWard(component, localPlayer.GetPlayerID());
		}
		return WardAccess.ShouldBlock(((Component)piece).transform.position, 0f, localPlayer);
	}

	internal static ProtectedRpcDecision EvaluateRemovalBySender(Piece? piece, long sender)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)piece == (Object)null)
		{
			return ProtectedRpcDecision.Allow;
		}
		if (!TryResolveAuthoritativeSenderPlayerId(sender, "Protected.Remove", out var playerId))
		{
			return ProtectedRpcDecision.Unresolved;
		}
		PrivateArea component = ((Component)piece).GetComponent<PrivateArea>();
		if (ManagedWardIdentity.EnsureManagedComponent(component))
		{
			if (!WardAccess.CanControlManagedWard(component, playerId))
			{
				return ProtectedRpcDecision.Deny;
			}
			return ProtectedRpcDecision.Allow;
		}
		if (!WardAccess.CheckAccess(((Component)piece).transform.position, 0f, playerId))
		{
			return ProtectedRpcDecision.Deny;
		}
		return ProtectedRpcDecision.Allow;
	}

	internal static bool IsPlacedConsumablePiece(Piece? piece)
	{
		if ((Object)(object)piece != (Object)null)
		{
			return WardAccess.IsPlacedConsumable(((Component)piece).GetComponent<ItemDrop>());
		}
		return false;
	}

	internal static ProtectedRpcDecision EvaluatePlacedConsumableRemovalBySender(Piece? piece, long sender)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)piece == (Object)null)
		{
			return ProtectedRpcDecision.Allow;
		}
		if (!TryResolveAuthoritativeSenderPlayerId(sender, "Protected.PlacedConsumableRemove", out var playerId))
		{
			return ProtectedRpcDecision.Unresolved;
		}
		if (!WardAccess.CheckRestrictionAccess(WardRestrictionOptions.PlacedConsumables, ((Component)piece).transform.position, 0f, playerId))
		{
			return ProtectedRpcDecision.Deny;
		}
		return ProtectedRpcDecision.Allow;
	}

	internal static ProtectedRpcDecision EvaluateDamageBySender(Vector3 point, long sender)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!TryResolveAuthoritativeSenderPlayerId(sender, "Protected.Damage", out var playerId))
		{
			return ProtectedRpcDecision.Unresolved;
		}
		if (!WardAccess.CheckAccess(point, 0f, playerId))
		{
			return ProtectedRpcDecision.Deny;
		}
		return ProtectedRpcDecision.Allow;
	}

	internal static Piece? GetProtectedBuildingPiece(Component? target)
	{
		if ((Object)(object)target == (Object)null || IsLikelyStumpDestructible(target))
		{
			return null;
		}
		return target.GetComponent<Piece>();
	}

	internal static BuildingDamageBlockReason GetBuildingDamageBlockReason(Vector3 point, Piece? piece, long sender)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!TryResolveAuthoritativeSenderPlayerId(sender, "Protected.BuildingDamage", out var playerId))
		{
			return BuildingDamageBlockReason.UnresolvedSender;
		}
		return EvaluateBuildingDamagePolicy(point, piece, DamageSourceKind.Player, playerId);
	}

	internal static BuildingDamageBlockReason GetBuildingDamageBlockReason(Vector3 point, Piece? piece, HitData? hit, long sender)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Character val = ((hit != null) ? hit.GetAttacker() : null);
		if (!((Object)(object)val != (Object)null))
		{
			return GetBuildingDamageBlockReason(point, piece, sender);
		}
		return GetBuildingDamageBlockReason(point, piece, val);
	}

	internal static BuildingDamageBlockReason GetBuildingDamageBlockReason(Vector3 point, Piece? piece, Character? attacker)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return EvaluateBuildingDamagePolicy(point, piece, GetDamageSourceKind(attacker), GetPlayerIdFromCharacter(attacker));
	}

	internal static ProtectedRpcDecision EvaluateInteractionBySender(Vector3 point, long sender)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!TryResolveAuthoritativeSenderPlayerId(sender, "Protected.Interaction", out var playerId))
		{
			return ProtectedRpcDecision.Unresolved;
		}
		if (!WardAccess.CheckAccess(point, 0f, playerId))
		{
			return ProtectedRpcDecision.Deny;
		}
		return ProtectedRpcDecision.Allow;
	}

	internal static ProtectedRpcDecision EvaluateInteractionBySender(Vector3 point, long sender, WardRestrictionOptions restriction)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!TryResolveAuthoritativeSenderPlayerId(sender, "Protected.Interaction", out var playerId))
		{
			return ProtectedRpcDecision.Unresolved;
		}
		if (!WardAccess.CheckRestrictionAccess(restriction, point, 0f, playerId))
		{
			return ProtectedRpcDecision.Deny;
		}
		return ProtectedRpcDecision.Allow;
	}

	internal static bool ShouldBlockDamageByCharacter(Vector3 point, Character? attacker)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		long playerIdFromCharacter = GetPlayerIdFromCharacter(attacker);
		if (playerIdFromCharacter == 0L)
		{
			return false;
		}
		return !WardAccess.CheckAccess(point, 0f, playerIdFromCharacter);
	}

	private static BuildingDamageBlockReason EvaluateBuildingDamagePolicy(Vector3 point, Piece? piece, DamageSourceKind sourceKind, long playerId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		bool num = (Object)(object)piece != (Object)null;
		bool insideEnabledWard = num && (Object)(object)WardAccess.FindNearestManagedWard(point) != (Object)null;
		bool blocksHostileCreatureDamage = num && sourceKind == DamageSourceKind.MonsterAI && WardAccess.ShouldBlockHostileCreatureDamageToBuilding(point);
		bool playerHasAccess = playerId == 0L || !WardAccess.EvaluateAccess(point, 0f, playerId, flash: false).IsDenied;
		return BuildingDamagePolicy.Evaluate(new BuildingDamagePolicyInput(num, sourceKind, playerId, insideEnabledWard, blocksHostileCreatureDamage, playerHasAccess));
	}

	private static DamageSourceKind GetDamageSourceKind(Character? attacker)
	{
		if (attacker is Player)
		{
			return DamageSourceKind.Player;
		}
		if ((Object)(object)attacker != (Object)null && attacker.IsTamed())
		{
			return DamageSourceKind.TamedCreature;
		}
		if (((attacker != null) ? attacker.GetBaseAI() : null) is MonsterAI)
		{
			return DamageSourceKind.MonsterAI;
		}
		return DamageSourceKind.Unknown;
	}

	internal static Player? GetLocalPlayerForCharacter(Character? attacker)
	{
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return null;
		}
		if (localPlayer.GetPlayerID() != GetPlayerIdFromCharacter(attacker))
		{
			return null;
		}
		return localPlayer;
	}

	internal static Player? GetLocalPlayerForSender(long sender)
	{
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return null;
		}
		if (localPlayer.GetPlayerID() != GetPlayerIdFromSender(sender))
		{
			return null;
		}
		return localPlayer;
	}

	private static bool TryResolveAuthoritativeSenderPlayerId(long sender, string context, out long playerId)
	{
		return WardOwnership.TryResolveAuthoritativePlayerIdFromSender(sender, context, out playerId);
	}

	private static long GetPlayerIdFromCharacter(Character? attacker)
	{
		if (attacker != null)
		{
			Player val = (Player)(object)((attacker is Player) ? attacker : null);
			if (val != null)
			{
				return val.GetPlayerID();
			}
			if (!attacker.IsTamed())
			{
				return 0L;
			}
			long owner = attacker.GetOwner();
			if (owner == 0L)
			{
				return 0L;
			}
			Player localPlayer = Player.m_localPlayer;
			if ((Object)(object)localPlayer != (Object)null && ((Character)localPlayer).GetOwner() == owner)
			{
				return localPlayer.GetPlayerID();
			}
			return GetPlayerIdFromSender(owner);
		}
		return 0L;
	}

	private static long GetPlayerIdFromSender(long sender)
	{
		return WardOwnership.GetPlayerIdFromSender(sender);
	}

	private static bool IsLikelyStumpDestructible(Component target)
	{
		GameObject gameObject = target.gameObject;
		StumpClassification orCreateValue = StumpClassificationCache.GetOrCreateValue(gameObject);
		string text = ((Object)gameObject).name ?? string.Empty;
		if (!orCreateValue.Initialized || !string.Equals(orCreateValue.CachedName, text, StringComparison.Ordinal))
		{
			orCreateValue.CachedName = text;
			orCreateValue.IsLikelyStump = ComputeIsLikelyStumpDestructible(gameObject, text);
			orCreateValue.Initialized = true;
		}
		return orCreateValue.IsLikelyStump;
	}

	private static bool ComputeIsLikelyStumpDestructible(GameObject target, string targetName)
	{
		if ((Object)(object)target.GetComponent<Destructible>() != (Object)null && (Object)(object)target.GetComponent<StaticPhysics>() != (Object)null)
		{
			return IsLikelyStumpName(targetName);
		}
		return false;
	}

	private static bool IsLikelyStumpName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		string text = NormalizeName(name);
		for (int i = 0; i < StumpNameTokens.Length; i++)
		{
			if (text.IndexOf(StumpNameTokens[i], StringComparison.Ordinal) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private static string NormalizeName(string name)
	{
		string text = name.Trim();
		int num = text.IndexOf("(Clone)", StringComparison.OrdinalIgnoreCase);
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		return text.Trim().ToLowerInvariant();
	}
}
