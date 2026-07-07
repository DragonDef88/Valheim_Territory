using System;
using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class WardAccess
{
	internal enum AccessDecision
	{
		NoWard,
		Allowed,
		Denied
	}

	internal readonly struct AccessResult
	{
		internal AccessDecision Decision { get; }

		internal bool IsDenied => Decision == AccessDecision.Denied;

		internal bool IsCoveredAndAllowed => Decision == AccessDecision.Allowed;

		internal AccessResult(AccessDecision decision)
		{
			Decision = decision;
		}
	}

	internal readonly struct RestrictionScope : IDisposable
	{
		private readonly bool _active;

		private readonly WardRestrictionOptions _previousRestriction;

		internal RestrictionScope(WardRestrictionOptions restriction)
		{
			_active = true;
			_previousRestriction = _restrictionScope;
			_restrictionScope = restriction;
			_restrictionScopeDepth++;
		}

		public void Dispose()
		{
			if (_active && _restrictionScopeDepth > 0)
			{
				_restrictionScopeDepth--;
				if (_restrictionScopeDepth == 0)
				{
					_restrictionScope = WardRestrictionOptions.None;
				}
				else
				{
					_restrictionScope = _previousRestriction;
				}
			}
		}
	}

	internal readonly struct ManagedWardAllowScope : IDisposable
	{
		private readonly bool _active;

		internal ManagedWardAllowScope(bool active)
		{
			_active = active;
			if (_active)
			{
				_managedWardAllowScopeDepth++;
			}
		}

		public void Dispose()
		{
			if (_active && _managedWardAllowScopeDepth > 0)
			{
				_managedWardAllowScopeDepth--;
			}
		}
	}

	private const string NoAccessMessageKey = "$piece_noaccess";

	private static readonly ManagedWardIndex AllWardIndex = new ManagedWardIndex((PrivateArea area) => IsTrackableManagedWard(area, requireEnabled: false));

	private static readonly ManagedWardIndex EnabledWardIndex = new ManagedWardIndex((PrivateArea area) => IsTrackableManagedWard(area, requireEnabled: true));

	private static readonly List<PrivateArea> SpatialQueryBuffer = new List<PrivateArea>();

	[ThreadStatic]
	private static int _restrictionScopeDepth;

	[ThreadStatic]
	private static WardRestrictionOptions _restrictionScope;

	[ThreadStatic]
	private static int _managedWardAllowScopeDepth;

	private static bool _wardCacheInitialized;

	private static bool _managedWardSpatialIndexRequiresFullRebuild = true;

	private static float _managedWardSpatialIndexMaxRadius = -1f;

	private static int _managedWardSpatialIndexRevision;

	internal static bool IsManagedWardAllowScopeActive => _managedWardAllowScopeDepth > 0;

	internal static RestrictionScope EnterRestrictionScope(WardRestrictionOptions restriction)
	{
		return new RestrictionScope(restriction);
	}

	internal static ManagedWardAllowScope EnterManagedWardAllowScope()
	{
		return new ManagedWardAllowScope(active: true);
	}

	internal static bool TryGetRestrictionScope(out WardRestrictionOptions restriction)
	{
		restriction = _restrictionScope;
		if (_restrictionScopeDepth > 0)
		{
			return restriction != WardRestrictionOptions.None;
		}
		return false;
	}

	internal static void RegisterManagedWard(PrivateArea? area)
	{
		RegisterManagedWard(ManagedWardRef.FromArea(area));
	}

	internal static void RegisterManagedWard(ManagedWardRef ward)
	{
		RefreshManagedWardState(ward);
	}

	internal static void RefreshManagedWardState(PrivateArea? area)
	{
		RefreshManagedWardState(ManagedWardRef.FromArea(area));
	}

	internal static void RefreshManagedWardState(ManagedWardRef ward)
	{
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null)
		{
			return;
		}
		EnsureManagedWardCacheInitialized();
		if (!IsTrackableManagedWard(ward, requireEnabled: false))
		{
			UnregisterManagedWard(ward);
			return;
		}
		int instanceID = ((Object)area).GetInstanceID();
		bool flag = false;
		bool flag2 = false;
		if (AllWardIndex.Add(area))
		{
			flag = true;
		}
		if (area.IsEnabled())
		{
			if (EnabledWardIndex.Add(area))
			{
				flag2 = true;
			}
		}
		else if (EnabledWardIndex.Remove(area))
		{
			ManagedWardRuntimeContexts.ResetPresenceState(area);
			flag2 = true;
		}
		if (flag || flag2)
		{
			UpdateManagedWardSpatialIndexMembership(area, instanceID, flag, flag2);
		}
		if (flag2)
		{
			ManagedWardPresenceService.Invalidate();
			ManagedWardPlacementPreviewService.Invalidate();
		}
	}

	internal static void UnregisterManagedWard(PrivateArea? area)
	{
		UnregisterManagedWard(ManagedWardRef.FromArea(area));
	}

	internal static void UnregisterManagedWard(ManagedWardRef ward)
	{
		PrivateArea area = ward.Area;
		if (!((Object)(object)area == (Object)null))
		{
			int instanceID = ((Object)area).GetInstanceID();
			bool flag = false;
			bool flag2 = false;
			if (AllWardIndex.Remove(area))
			{
				flag = true;
			}
			if (EnabledWardIndex.Remove(area))
			{
				flag2 = true;
			}
			ManagedWardRuntimeContexts.ResetPresenceState(area);
			if (flag || flag2)
			{
				UpdateManagedWardSpatialIndexMembership(area, instanceID, flag, flag2);
			}
			if (flag2)
			{
				ManagedWardPresenceService.Invalidate();
				ManagedWardPlacementPreviewService.Invalidate();
			}
		}
	}

	internal static bool HasEnabledManagedWards()
	{
		EnsureManagedWardCacheInitialized();
		return EnabledWardIndex.Count > 0;
	}

	internal static void InvalidateWardPresenceCache()
	{
		ManagedWardPresenceService.Invalidate();
	}

	internal static void InvalidateManagedWardSpatialIndex()
	{
		if (_managedWardSpatialIndexRequiresFullRebuild)
		{
			ManagedWardPlacementPreviewService.Invalidate();
			return;
		}
		_managedWardSpatialIndexRequiresFullRebuild = true;
		BumpManagedWardSpatialRevision();
		ManagedWardPlacementPreviewService.Invalidate();
	}

	internal static void RefreshManagedWardSpatialIndexEntry(PrivateArea? area)
	{
		RefreshManagedWardSpatialIndexEntry(ManagedWardRef.FromArea(area));
	}

	internal static void RefreshManagedWardSpatialIndexEntry(ManagedWardRef ward)
	{
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null)
		{
			return;
		}
		EnsureManagedWardCacheInitialized();
		if (_managedWardSpatialIndexRequiresFullRebuild || !Mathf.Approximately(_managedWardSpatialIndexMaxRadius, WardSettings.MaxRadius))
		{
			if (!_managedWardSpatialIndexRequiresFullRebuild)
			{
				_managedWardSpatialIndexRequiresFullRebuild = true;
				BumpManagedWardSpatialRevision();
			}
			ManagedWardPlacementPreviewService.Invalidate();
			return;
		}
		int instanceID = ((Object)area).GetInstanceID();
		bool flag = AllWardIndex.Contains(instanceID);
		bool flag2 = EnabledWardIndex.Contains(instanceID);
		if (flag || flag2)
		{
			UpdateManagedWardSpatialIndexMembership(area, instanceID, flag, flag2);
		}
	}

	internal static void ResetManagedWardCache()
	{
		AllWardIndex.Clear();
		EnabledWardIndex.Clear();
		SpatialQueryBuffer.Clear();
		ManagedWardPresenceService.ResetRuntimeState();
		_wardCacheInitialized = false;
		_managedWardSpatialIndexRequiresFullRebuild = true;
		_managedWardSpatialIndexMaxRadius = -1f;
		_managedWardSpatialIndexRevision = 0;
		ManagedWardPlacementPreviewService.Invalidate();
	}

	internal static void UpdateTrustedPlayerPresenceSweep()
	{
		ManagedWardPresenceService.Update();
	}

	internal static IReadOnlyList<PrivateArea> GetManagedWards(bool requireEnabled)
	{
		EnsureManagedWardCacheInitialized();
		if (!requireEnabled)
		{
			return AllWardIndex.Areas;
		}
		return EnabledWardIndex.Areas;
	}

	internal static bool CheckAccess(Vector3 point, float radius, long playerId, bool flash = true, bool wardCheck = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return !EvaluateAccess(point, radius, playerId, flash, wardCheck).IsDenied;
	}

	internal static AccessResult EvaluateAccess(Vector3 point, float radius, long playerId, bool flash = true, bool wardCheck = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<PrivateArea> candidateManagedWards = GetCandidateManagedWards(point, radius, requireEnabled: true);
		return EvaluateAccessAgainstCandidates(point, radius, playerId, candidateManagedWards, flash, wardCheck);
	}

	internal static AccessResult EvaluateAccessAgainstCandidates(Vector3 point, float radius, long playerId, IReadOnlyList<PrivateArea> areas, bool flash = true, bool wardCheck = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return EvaluateAccessAgainstCandidates(point, radius, playerId, areas, null, flash, wardCheck);
	}

	internal static bool CheckRestrictionAccess(WardRestrictionOptions restriction, Vector3 point, float radius, long playerId, bool flash = true, bool wardCheck = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return !EvaluateRestrictionAccess(restriction, point, radius, playerId, flash, wardCheck).IsDenied;
	}

	internal static AccessResult EvaluateRestrictionAccess(WardRestrictionOptions restriction, Vector3 point, float radius, long playerId, bool flash = true, bool wardCheck = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<PrivateArea> candidateManagedWards = GetCandidateManagedWards(point, radius, requireEnabled: true);
		return EvaluateRestrictionAccessAgainstCandidates(restriction, point, radius, playerId, candidateManagedWards, flash, wardCheck);
	}

	internal static AccessResult EvaluateRestrictionAccessAgainstCandidates(WardRestrictionOptions restriction, Vector3 point, float radius, long playerId, IReadOnlyList<PrivateArea> areas, bool flash = true, bool wardCheck = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return EvaluateAccessAgainstCandidates(point, radius, playerId, areas, restriction, flash, wardCheck);
	}

	private static AccessResult EvaluateAccessAgainstCandidates(Vector3 point, float radius, long playerId, IReadOnlyList<PrivateArea> areas, WardRestrictionOptions? restriction, bool flash, bool wardCheck)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (areas.Count == 0)
		{
			return new AccessResult(AccessDecision.NoWard);
		}
		bool includeDiagnosticData = Plugin.ShouldLogWardDiagnosticVerbose();
		ManagedWardAccessActor actor;
		bool flag = ManagedWardAccessEvaluator.TryCreateActorForAccessCheck(playerId, out actor);
		bool flag2 = false;
		bool flag3 = false;
		List<PrivateArea> list = null;
		foreach (PrivateArea area in areas)
		{
			if ((Object)(object)area == (Object)null || !area.IsInside(point, radius) || (restriction.HasValue && !WardSettings.HasRestriction(WardSettings.GetConfiguration(area), restriction.Value)))
			{
				continue;
			}
			flag2 = true;
			if (flag && ManagedWardAccessEvaluator.HasPlayerAccess(area, actor, includeDiagnosticData))
			{
				continue;
			}
			flag3 = true;
			if (flash)
			{
				if (list == null)
				{
					list = new List<PrivateArea>();
				}
				list.Add(area);
			}
			if (wardCheck)
			{
				break;
			}
		}
		if (!flag2)
		{
			return new AccessResult(AccessDecision.NoWard);
		}
		if (!flag3)
		{
			return new AccessResult(AccessDecision.Allowed);
		}
		if (list != null)
		{
			foreach (PrivateArea item in list)
			{
				item.FlashShield(false);
			}
		}
		return new AccessResult(AccessDecision.Denied);
	}

	internal static int CollectDeniedManagedWardCandidates(long playerId, IReadOnlyList<PrivateArea> areas, List<PrivateArea> deniedAreas)
	{
		deniedAreas.Clear();
		if (playerId == 0L || areas.Count == 0)
		{
			return 0;
		}
		bool includeDiagnosticData = Plugin.ShouldLogWardDiagnosticVerbose();
		ManagedWardAccessActor actor = ManagedWardAccessEvaluator.CreateActor(playerId);
		foreach (PrivateArea area in areas)
		{
			if (!((Object)(object)area == (Object)null) && !ManagedWardAccessEvaluator.HasPlayerAccess(area, actor, includeDiagnosticData))
			{
				deniedAreas.Add(area);
			}
		}
		return deniedAreas.Count;
	}

	internal static bool IsInsideAnyManagedWard(Vector3 point, float radius, IReadOnlyList<PrivateArea> areas)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < areas.Count; i++)
		{
			PrivateArea val = areas[i];
			if ((Object)(object)val != (Object)null && val.IsInside(point, radius))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool TryBlockInteraction(Component target, Player? player, ref bool result)
	{
		bool flag = ShouldBlock(target, player, 0f);
		LogLocalInteractionAttemptVerbose("Interaction", target, player, flag);
		if (!flag)
		{
			return true;
		}
		ShowNoAccessMessage(player);
		result = true;
		return false;
	}

	internal static bool TryBlockInteraction(WardRestrictionOptions restriction, Component target, Player? player, ref bool result)
	{
		bool flag = ShouldBlockRestriction(restriction, target, player, 0f);
		LogLocalInteractionAttemptVerbose($"Interaction.{restriction}", target, player, flag);
		if (!flag)
		{
			return true;
		}
		ShowNoAccessMessage(player);
		result = true;
		return false;
	}

	internal static bool TryBlockAction(Component target, Player? player, ref bool result)
	{
		bool flag = ShouldBlock(target, player, 0f);
		LogLocalInteractionAttemptVerbose("Action", target, player, flag);
		if (!flag)
		{
			return true;
		}
		ShowNoAccessMessage(player);
		result = false;
		return false;
	}

	internal static bool TryBlockVoid(Component target, Player? player)
	{
		bool flag = ShouldBlock(target, player, 0f);
		LogLocalInteractionAttemptVerbose("Void", target, player, flag);
		if (!flag)
		{
			return true;
		}
		ShowNoAccessMessage(player);
		return false;
	}

	internal static bool TryBlockVoid(WardRestrictionOptions restriction, Component target, Player? player)
	{
		bool flag = ShouldBlockRestriction(restriction, target, player, 0f);
		LogLocalInteractionAttemptVerbose($"Void.{restriction}", target, player, flag);
		if (!flag)
		{
			return true;
		}
		ShowNoAccessMessage(player);
		return false;
	}

	internal static bool TryBlockPlacement(Player? player, Vector3 point, float radius, ref bool result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldBlock(point, radius, player))
		{
			return true;
		}
		ShowNoAccessMessage(player);
		result = false;
		return false;
	}

	internal static bool TryBlockPlacement(Player? player, Vector3 point, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldBlock(point, radius, player))
		{
			return true;
		}
		ShowNoAccessMessage(player);
		return false;
	}

	internal static bool TryBlockItemUse(Player? player, ItemData? item, ref bool result)
	{
		if (!ShouldBlockConfiguredItemUse(player, item))
		{
			return true;
		}
		ShowBlockedItemMessage(player);
		result = false;
		return false;
	}

	internal static bool TryBlockItemUse(Player? player, ItemData? item)
	{
		if (!ShouldBlockConfiguredItemUse(player, item))
		{
			return true;
		}
		ShowBlockedItemMessage(player);
		return false;
	}

	internal static bool TryBlockItemUse(Player? player, ItemData? item, Vector3 targetPoint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldBlockConfiguredItemUse(player, item, targetPoint))
		{
			return true;
		}
		ShowBlockedItemMessage(player);
		return false;
	}

	internal static bool TryForceUnequipBlockedItems(Player? player)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)Player.m_localPlayer)
		{
			return false;
		}
		if (!HasEnabledManagedWards())
		{
			return false;
		}
		Inventory inventory = ((Humanoid)player).GetInventory();
		if (inventory == null)
		{
			return false;
		}
		List<ItemData> inventory2 = inventory.m_inventory;
		if (inventory2 == null || inventory2.Count == 0)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < inventory2.Count; i++)
		{
			ItemData val = inventory2[i];
			if (val != null && val.m_equipped && IsConfiguredBlockedItem(val))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (!ShouldBlock(((Component)player).transform.position, 0f, player, flash: false))
		{
			return false;
		}
		bool flag2 = false;
		for (int j = 0; j < inventory2.Count; j++)
		{
			ItemData val2 = inventory2[j];
			if (val2 != null && val2.m_equipped && IsConfiguredBlockedItem(val2))
			{
				((Humanoid)player).UnequipItem(val2, false);
				flag2 = true;
			}
		}
		if (flag2)
		{
			ShowBlockedItemMessage(player);
		}
		return flag2;
	}

	internal static bool TryBlockAttack(Player? player, ref bool result)
	{
		return TryBlockAttack(player, (player != null) ? ((Humanoid)player).GetCurrentWeapon() : null, ref result);
	}

	internal static bool TryBlockAttack(Player? player, ItemData? item, ref bool result)
	{
		if (!ShouldBlockConfiguredItemUse(player, item) && !ShouldBlockConfiguredItemUseAgainstHoveredTamedCreature(player, item))
		{
			return true;
		}
		if (!TryForceUnequipBlockedItems(player))
		{
			ShowBlockedItemMessage(player);
		}
		result = false;
		return false;
	}

	internal static bool ShouldBlock(Component? target, Player? player, float radius, bool flash = true)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		return ShouldBlock(target.transform.position, radius, player, flash);
	}

	internal static bool ShouldBlock(Vector3 point, float radius, Player? player, bool flash = true)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!HasEnabledManagedWards())
		{
			return false;
		}
		return EvaluateAccess(point, radius, player.GetPlayerID(), flash).IsDenied;
	}

	internal static bool ShouldBlockRestriction(WardRestrictionOptions restriction, Component? target, Player? player, float radius, bool flash = true)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		return ShouldBlockRestriction(restriction, target.transform.position, radius, player, flash);
	}

	internal static bool ShouldBlockRestriction(WardRestrictionOptions restriction, Vector3 point, float radius, Player? player, bool flash = true)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!HasEnabledManagedWards())
		{
			return false;
		}
		return EvaluateRestrictionAccess(restriction, point, radius, player.GetPlayerID(), flash).IsDenied;
	}

	internal static bool ShouldBlockPickup(GameObject? go, Player? player)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)go == (Object)null || !WardItemPrefabPolicy.CanAnyPickupBeBlocked())
		{
			return false;
		}
		if (IsPlacedConsumable(go.GetComponent<ItemDrop>()))
		{
			return false;
		}
		if (WardItemPrefabPolicy.ShouldBlockPickup(go))
		{
			return ShouldBlockRestriction(WardRestrictionOptions.Pickup, go.transform.position, 0f, player);
		}
		return false;
	}

	internal static bool ShouldBlockPickup(ItemDrop? itemDrop, Player? player)
	{
		if ((Object)(object)itemDrop == (Object)null || !WardItemPrefabPolicy.CanAnyPickupBeBlocked())
		{
			return false;
		}
		if (!IsPlacedConsumable(itemDrop) && WardItemPrefabPolicy.ShouldBlockPickup(itemDrop))
		{
			return ShouldBlockRestriction(WardRestrictionOptions.Pickup, (Component?)(object)itemDrop, player, 0f);
		}
		return false;
	}

	internal static bool IsPlacedConsumable(ItemDrop? itemDrop)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		if ((Object)(object)itemDrop != (Object)null && itemDrop.IsPiece())
		{
			ItemData itemData = itemDrop.m_itemData;
			if (itemData == null)
			{
				return false;
			}
			return (int)(itemData.m_shared?.m_itemType).GetValueOrDefault() == 2;
		}
		return false;
	}

	internal static void ShowNoAccessMessage(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			((Character)player).Message((MessageType)2, "$piece_noaccess", 0, (Sprite)null);
		}
	}

	private static void LogLocalInteractionAttemptVerbose(string context, Component? target, Player? player, bool blocked)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.ShouldLogWardDiagnosticVerbose() && !((Object)(object)target == (Object)null) && !((Object)(object)player == (Object)null))
		{
			long playerID = player.GetPlayerID();
			Plugin.LogWardDiagnosticVerbose("Access." + context, $"Evaluated local interaction before server handling. blocked={blocked}, targetType={((object)target).GetType().Name}, targetName='{((Object)target).name}', playerId={playerID}, playerName='{player.GetPlayerName()}', position={target.transform.position}");
		}
	}

	internal static void ShowBlockedItemMessage(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			((Character)player).Message((MessageType)2, WardLocalization.Localize("$stuw_msg_blocked_item", "A ward prevents using this item here."), 0, (Sprite)null);
		}
	}

	internal static void ShowProtectedBuildingDamageMessage(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			((Character)player).Message((MessageType)2, WardLocalization.Localize("$stuw_msg_building_damage_protected", "An active Ward prevents damaging protected structures."), 0, (Sprite)null);
		}
	}

	internal static void ShowWardOverlapMessage(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			((Character)player).Message((MessageType)2, WardLocalization.Localize("$stuw_msg_overlap", "Another Ward is too close."), 0, (Sprite)null);
		}
	}

	internal static void ShowWardLimitMessage(Player? player, int limit)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			string text = WardLocalization.LocalizeFormat("$stuw_msg_limit_with_max", "Ward limit reached (max {0})", limit);
			((Character)player).Message((MessageType)2, text, 0, (Sprite)null);
		}
	}

	internal static Player? GetPlayer(Humanoid? humanoid)
	{
		return (Player?)(object)((humanoid is Player) ? humanoid : null);
	}

	internal static Player? GetPlayer(Collider? collider)
	{
		if ((Object)(object)collider == (Object)null)
		{
			return null;
		}
		return ((Component)collider).GetComponentInParent<Player>();
	}

	internal static float GetTerrainRadius(GameObject? prefab)
	{
		if ((Object)(object)prefab == (Object)null)
		{
			return 0f;
		}
		TerrainModifier component = prefab.GetComponent<TerrainModifier>();
		if ((Object)(object)component != (Object)null)
		{
			return component.GetRadius();
		}
		TerrainOp component2 = prefab.GetComponent<TerrainOp>();
		if (!((Object)(object)component2 != (Object)null))
		{
			return 0f;
		}
		return component2.GetRadius();
	}

	internal static bool IsRelevantWard(PrivateArea? area)
	{
		return IsManagedWard(area, requireEnabled: true);
	}

	internal static bool IsManagedWard(PrivateArea? area, bool requireEnabled)
	{
		return IsManagedWard(ManagedWardRef.FromArea(area), requireEnabled);
	}

	internal static bool IsManagedWard(ManagedWardRef ward, bool requireEnabled)
	{
		return IsTrackableManagedWard(ward, requireEnabled);
	}

	internal static bool CanConfigureWard(PrivateArea? area, Player? player)
	{
		return CanConfigureWard(ManagedWardRef.FromArea(area), player);
	}

	internal static bool CanConfigureWard(ManagedWardRef ward, Player? player)
	{
		if ((Object)(object)ward.Area == (Object)null || (Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)Player.m_localPlayer || !IsManagedWard(ward, requireEnabled: false))
		{
			return false;
		}
		if (!IsDirectWardOwner(ward, player.GetPlayerID()))
		{
			return WardAdminDebugAccess.CanLocallyControlAnyWard(ward.Area, player);
		}
		return true;
	}

	internal static bool CanControlManagedWard(PrivateArea? area, long playerId)
	{
		return CanControlManagedWard(ManagedWardRef.FromArea(area), playerId);
	}

	internal static bool CanControlManagedWard(ManagedWardRef ward, long playerId)
	{
		if ((Object)(object)ward.Area == (Object)null || playerId == 0L || !IsManagedWard(ward, requireEnabled: false))
		{
			return false;
		}
		if (!IsDirectWardOwner(ward, playerId))
		{
			return WardAdminDebugAccess.IsPlayerAdminDebugController(playerId);
		}
		return true;
	}

	internal static bool IsDirectWardOwner(PrivateArea? area, Player? player)
	{
		if ((Object)(object)area != (Object)null && (Object)(object)player != (Object)null)
		{
			return IsDirectWardOwner(area, player.GetPlayerID());
		}
		return false;
	}

	internal static bool IsDirectWardOwner(PrivateArea? area, long playerId)
	{
		return IsDirectWardOwner(ManagedWardRef.FromArea(area), playerId);
	}

	internal static bool IsDirectWardOwner(ManagedWardRef ward, long playerId)
	{
		if ((Object)(object)ward.Area == (Object)null || playerId == 0L || !IsManagedWard(ward, requireEnabled: false))
		{
			return false;
		}
		return GetCanonicalCreatorPlayerId(ward.Area) == playerId;
	}

	internal static bool IsPlayerInWardGuild(Player? player, PrivateArea? area)
	{
		return IsPlayerGuildMatchingWardGuild(GuildsCompat.GetPlayerGuildIdentity(player), GuildsCompat.GetWardGuildIdentity(area));
	}

	internal static bool IsPlayerIdInWardGuild(long playerId, PrivateArea? area)
	{
		if (playerId == 0L)
		{
			return false;
		}
		return IsPlayerGuildMatchingWardGuild(GuildsCompat.GetPlayerGuildIdentity(playerId), GuildsCompat.GetWardGuildIdentity(area));
	}

	internal static bool IsPlayerIdInWardGuild(long playerId, ZDO? zdo)
	{
		if (playerId == 0L)
		{
			return false;
		}
		return IsPlayerGuildMatchingWardGuild(GuildsCompat.GetPlayerGuildIdentity(playerId), GuildsCompat.GetWardGuildIdentity(zdo));
	}

	internal static bool IsPlayerGuildMatchingWardGuild(WardGuildIdentity playerGuild, WardGuildIdentity wardGuild)
	{
		return ManagedWardAccessEvaluator.HasMatchingGuild(playerGuild, wardGuild);
	}

	internal static bool ShouldBlockHostileCreatureDamageToBuilding(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return ManagedWardPresenceService.ShouldBlockHostileCreatureDamageToBuilding(point);
	}

	internal static bool TryBlockManagedWardPlacement(Player? player, Component? candidate, Vector3 point, ref bool result)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsManagedWardPlacementCandidate(candidate))
		{
			return true;
		}
		if (!WouldBlockManagedWardPlacement(player, candidate, point, flash: true))
		{
			return true;
		}
		ShowWardOverlapMessage(player);
		result = false;
		return false;
	}

	internal static bool TryBlockManagedWardPlacement(Player? player, Component? candidate, Vector3 point)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsManagedWardPlacementCandidate(candidate))
		{
			return true;
		}
		if (!WouldBlockManagedWardPlacement(player, candidate, point, flash: true))
		{
			return true;
		}
		ShowWardOverlapMessage(player);
		return false;
	}

	internal static float GetMaxNonOverlappingRadius(PrivateArea? area)
	{
		return GetMaxNonOverlappingRadius(area, WardSettings.MaxRadius);
	}

	internal static float GetMaxNonOverlappingRadius(PrivateArea? area, float fallbackRadius)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		long canonicalCreatorPlayerId = GetCanonicalCreatorPlayerId(area);
		int guildId = (GuildsCompat.IsAvailable() ? GuildsCompat.GetWardGuildId(area) : 0);
		if (!((Object)(object)area == (Object)null))
		{
			return GetMaxNonOverlappingRadius(((Component)area).transform.position, canonicalCreatorPlayerId, guildId, area, fallbackRadius);
		}
		return fallbackRadius;
	}

	internal static float GetMaxNonOverlappingRadius(Vector3 point, long ownerCreatorPlayerId, int guildId, PrivateArea? ignoredWard, float fallbackRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<PrivateArea> candidateManagedWards = GetCandidateManagedWards(point, fallbackRadius, requireEnabled: false);
		if (candidateManagedWards.Count == 0)
		{
			return fallbackRadius;
		}
		return WardOverlapPolicy.GetMaxNonOverlappingRadius(fallbackRadius, CreateWardOverlapQuery(point, fallbackRadius, ownerCreatorPlayerId, guildId, ignoredWard), BuildWardOverlapAreas(candidateManagedWards, guildId));
	}

	internal static long GetCanonicalCreatorPlayerId(PrivateArea? area)
	{
		long wardCreatorId = GetWardCreatorId(area);
		if (wardCreatorId != 0L)
		{
			return wardCreatorId;
		}
		Piece val = (((Object)(object)area?.m_piece != (Object)null) ? area.m_piece : ((area != null) ? ((Component)area).GetComponent<Piece>() : null));
		if (!((Object)(object)val != (Object)null))
		{
			return 0L;
		}
		return val.GetCreator();
	}

	internal static long GetWardCreatorId(PrivateArea? area)
	{
		return ManagedWardRef.FromArea(area).CreatorPlayerId;
	}

	internal static PrivateArea? FindNearestManagedWard(Vector3 point, float radius = 0f, bool requireEnabled = true, Predicate<PrivateArea>? predicate = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<PrivateArea> candidateManagedWards = GetCandidateManagedWards(point, radius, requireEnabled);
		if (candidateManagedWards.Count == 0)
		{
			return null;
		}
		PrivateArea result = null;
		float num = float.MaxValue;
		foreach (PrivateArea item in candidateManagedWards)
		{
			if (!((Object)(object)item == (Object)null) && item.IsInside(point, radius) && (predicate == null || predicate(item)))
			{
				float num2 = Utils.DistanceXZ(((Component)item).transform.position, point);
				if (!(num2 >= num))
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	private static bool ShouldBlockConfiguredItemUse(Player? player, ItemData? item)
	{
		return ShouldBlockConfiguredItemUse(player, item, null);
	}

	private static bool ShouldBlockConfiguredItemUse(Player? player, ItemData? item, Vector3? targetPoint)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || item == null)
		{
			return false;
		}
		if (!HasEnabledManagedWards())
		{
			return false;
		}
		if (!IsConfiguredBlockedItem(item))
		{
			return false;
		}
		if (ShouldBlock(((Component)player).transform.position, 0f, player))
		{
			return true;
		}
		if (targetPoint.HasValue)
		{
			return ShouldBlock(targetPoint.Value, 0f, player);
		}
		return false;
	}

	private static bool ShouldBlockConfiguredItemUseAgainstHoveredTamedCreature(Player? player, ItemData? item)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetHoveredTamedCreaturePoint(player, out var targetPoint))
		{
			return ShouldBlockConfiguredItemUse(player, item, targetPoint);
		}
		return false;
	}

	private static bool IsConfiguredBlockedItem(ItemData? item)
	{
		if (item == null)
		{
			return false;
		}
		return WardItemPrefabPolicy.IsBlockedItem(item);
	}

	private static bool TryGetHoveredTamedCreaturePoint(Player? player, out Vector3 targetPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		targetPoint = default(Vector3);
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		Character hoveringCreature = player.m_hoveringCreature;
		if ((Object)(object)hoveringCreature != (Object)null && hoveringCreature.IsTamed())
		{
			targetPoint = ((Component)hoveringCreature).transform.position;
			return true;
		}
		try
		{
			Character val = null;
			GameObject val2 = null;
			player.FindHoverObject(ref val2, ref val);
			if ((Object)(object)val == (Object)null || !val.IsTamed())
			{
				return false;
			}
			targetPoint = ((Component)val).transform.position;
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool WouldBlockManagedWardPlacement(Player? player, Component? candidate, Vector3 point, bool flash)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!StuWardArea.IsManaged(((Object)(object)candidate != (Object)null) ? (candidate.GetComponent<PrivateArea>() ?? candidate.GetComponentInParent<PrivateArea>()) : null))
		{
			return false;
		}
		float radius = 8f;
		return OverlapsForeignManagedWard(point, radius, player.GetPlayerID(), GuildsCompat.GetPlayerGuildId(player), null, flash);
	}

	private static bool OverlapsForeignManagedWard(Vector3 point, float radius, long ownerCreatorPlayerId, int guildId, PrivateArea? ignoredWard, bool flash)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		List<PrivateArea> list = (flash ? new List<PrivateArea>() : null);
		IReadOnlyList<PrivateArea> candidateManagedWards = GetCandidateManagedWards(point, radius, requireEnabled: false);
		if (candidateManagedWards.Count == 0)
		{
			return false;
		}
		bool flag = false;
		WardOverlapQuery query = CreateWardOverlapQuery(point, radius, ownerCreatorPlayerId, guildId, ignoredWard);
		foreach (PrivateArea item in candidateManagedWards)
		{
			if (!((Object)(object)item == (Object)null) && !((Object)(object)item == (Object)(object)ignoredWard))
			{
				WardOverlapArea area = CreateWardOverlapArea(item, guildId);
				if (!WardOverlapPolicy.SharesTrustedWardGroup(area, query) && WardOverlapPolicy.Overlaps(query, area))
				{
					flag = true;
					list?.Add(item);
				}
			}
		}
		if (!flag || list == null)
		{
			return flag;
		}
		foreach (PrivateArea item2 in list)
		{
			ZNetView nView = WardPrivateAreaSafeAccess.GetNView(item2);
			if (!((Object)(object)nView == (Object)null) && nView.IsValid())
			{
				item2.FlashShield(false);
			}
		}
		return true;
	}

	private static WardOverlapQuery CreateWardOverlapQuery(Vector3 point, float radius, long ownerCreatorPlayerId, int guildId, PrivateArea? ignoredWard)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new WardOverlapQuery(point.x, point.z, radius, ownerCreatorPlayerId, guildId, ((Object)(object)ignoredWard != (Object)null) ? ((Object)ignoredWard).GetInstanceID() : 0);
	}

	private static List<WardOverlapArea> BuildWardOverlapAreas(IReadOnlyList<PrivateArea> areas, int queryGuildId)
	{
		List<WardOverlapArea> list = new List<WardOverlapArea>(areas.Count);
		for (int i = 0; i < areas.Count; i++)
		{
			PrivateArea val = areas[i];
			if ((Object)(object)val != (Object)null)
			{
				list.Add(CreateWardOverlapArea(val, queryGuildId));
			}
		}
		return list;
	}

	private static WardOverlapArea CreateWardOverlapArea(PrivateArea area, int queryGuildId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)area).transform.position;
		return new WardOverlapArea(((Object)area).GetInstanceID(), position.x, position.z, WardSettings.GetStoredRadiusOrMin(area), GetCanonicalCreatorPlayerId(area), (queryGuildId != 0 && GuildsCompat.IsAvailable()) ? GuildsCompat.GetWardGuildId(area) : 0);
	}

	internal static int GetManagedWardSpatialIndexRevision()
	{
		return _managedWardSpatialIndexRevision;
	}

	internal static bool IsManagedWardPlacementCandidate(Component? candidate)
	{
		return StuWardArea.IsManaged(((Object)(object)candidate != (Object)null) ? (candidate.GetComponent<PrivateArea>() ?? candidate.GetComponentInParent<PrivateArea>()) : null);
	}

	private static void EnsureManagedWardCacheInitialized()
	{
		if (_wardCacheInitialized)
		{
			return;
		}
		_wardCacheInitialized = true;
		AllWardIndex.Clear();
		EnabledWardIndex.Clear();
		List<PrivateArea> allAreas = PrivateArea.m_allAreas;
		if (allAreas == null)
		{
			return;
		}
		for (int i = 0; i < allAreas.Count; i++)
		{
			PrivateArea val = allAreas[i];
			if (IsTrackableManagedWard(val, requireEnabled: false))
			{
				AllWardIndex.Add(val);
				if (val.IsEnabled())
				{
					EnabledWardIndex.Add(val);
				}
			}
		}
		_managedWardSpatialIndexRequiresFullRebuild = true;
	}

	private static bool IsTrackableManagedWard(PrivateArea? area, bool requireEnabled)
	{
		return IsTrackableManagedWard(ManagedWardRef.FromArea(area), requireEnabled);
	}

	private static bool IsTrackableManagedWard(ManagedWardRef ward, bool requireEnabled)
	{
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null || ward.IsPlacementGhost || !ManagedWardIdentity.EnsureManagedComponent(ward))
		{
			return false;
		}
		if (requireEnabled && !area.IsEnabled())
		{
			return false;
		}
		return ward.HasValidNetworkIdentity;
	}

	internal static IReadOnlyList<PrivateArea> GetCandidateManagedWards(Vector3 point, float radius, bool requireEnabled)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		FillCandidateManagedWards(point, radius, requireEnabled, SpatialQueryBuffer);
		return SpatialQueryBuffer;
	}

	internal static void FillCandidateManagedWards(Vector3 point, float radius, bool requireEnabled, List<PrivateArea> destination)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		EnsureManagedWardSpatialIndexInitialized();
		(requireEnabled ? EnabledWardIndex : AllWardIndex).FillCandidates(point, radius, destination);
	}

	private static void EnsureManagedWardSpatialIndexInitialized()
	{
		EnsureManagedWardCacheInitialized();
		float maxRadius = WardSettings.MaxRadius;
		if (_managedWardSpatialIndexRequiresFullRebuild || !Mathf.Approximately(_managedWardSpatialIndexMaxRadius, maxRadius))
		{
			AllWardIndex.ClearSpatialIndex();
			EnabledWardIndex.ClearSpatialIndex();
			SpatialQueryBuffer.Clear();
			AllWardIndex.RebuildSpatialIndex();
			EnabledWardIndex.RebuildSpatialIndex();
			_managedWardSpatialIndexRequiresFullRebuild = false;
			_managedWardSpatialIndexMaxRadius = maxRadius;
		}
	}

	private static void UpdateManagedWardSpatialIndexMembership(PrivateArea area, int instanceId, bool updateAllWardIndex, bool updateEnabledWardIndex)
	{
		EnsureManagedWardCacheInitialized();
		if (_managedWardSpatialIndexRequiresFullRebuild || !Mathf.Approximately(_managedWardSpatialIndexMaxRadius, WardSettings.MaxRadius))
		{
			if (!_managedWardSpatialIndexRequiresFullRebuild)
			{
				_managedWardSpatialIndexRequiresFullRebuild = true;
				if (updateAllWardIndex)
				{
					BumpManagedWardSpatialRevision();
				}
			}
			if (updateAllWardIndex)
			{
				ManagedWardPlacementPreviewService.Invalidate();
			}
		}
		else
		{
			if (updateAllWardIndex)
			{
				AllWardIndex.UpdateSpatialIndex(area, instanceId, AllWardIndex.Contains(instanceId));
				BumpManagedWardSpatialRevision();
				ManagedWardPlacementPreviewService.Invalidate();
			}
			if (updateEnabledWardIndex)
			{
				EnabledWardIndex.UpdateSpatialIndex(area, instanceId, EnabledWardIndex.Contains(instanceId));
			}
		}
	}

	private static void BumpManagedWardSpatialRevision()
	{
		_managedWardSpatialIndexRevision = ((_managedWardSpatialIndexRevision == int.MaxValue) ? 1 : (_managedWardSpatialIndexRevision + 1));
	}
}
