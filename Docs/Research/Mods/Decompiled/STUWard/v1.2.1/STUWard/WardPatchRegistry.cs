using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace STUWard;

internal static class WardPatchRegistry
{
	private static readonly HashSet<Type> RequiredPatchTypes = new HashSet<Type>
	{
		typeof(PrivateAreaAwakePatch),
		typeof(PrivateAreaOnDestroyPatch),
		typeof(PrivateAreaUpdateStatusPatch),
		typeof(PrivateAreaSetupPatch),
		typeof(ZNetSceneCreateObjectManagedWardPatch),
		typeof(PrivateAreaInteractAdminDebugPatch),
		typeof(PrivateAreaRpcTogglePermittedManagedPatch),
		typeof(PrivateAreaRpcToggleEnabledAdminDebugPatch),
		typeof(DirectInteractionPatches),
		typeof(PrivateAreaHaveLocalAccessManagedPatch),
		typeof(PrivateAreaCheckAccessManagedPatch),
		typeof(ContainerCheckAccessManagedPatch),
		typeof(UseItemInteractionPatches),
		typeof(StationUsePatches),
		typeof(ProcessingInteractionPatches),
		typeof(TeleportWorldTeleportPatch),
		typeof(TeleportWorldTriggerPatch),
		typeof(ItemDropPickupPatch),
		typeof(HumanoidPickupPatch),
		typeof(PlayerAutoPickupPatch),
		typeof(PlayerTryPlacePiecePatch),
		typeof(PlayerSetupPlacementGhostPatch),
		typeof(PlayerUpdatePlacementGhostPatch),
		typeof(PlayerPlacePiecePatch),
		typeof(PlayerCheckCanRemovePiecePatch),
		typeof(PlayerRemovePiecePatch),
		typeof(PlayerRepairPatch),
		typeof(ZNetSceneDestroyPatch),
		typeof(AttackSpawnOnHitTerrainPatch),
		typeof(TerrainOpAwakePatch),
		typeof(WearNTearDamagePatch),
		typeof(WearNTearRpcDamagePatch),
		typeof(WearNTearRemovePatch),
		typeof(WearNTearRpcRemovePatch),
		typeof(DestructibleDamagePatch),
		typeof(DestructibleRpcDamagePatch),
		typeof(TreeBaseDamagePatch),
		typeof(TreeBaseRpcDamagePatch),
		typeof(FeastRpcTryEatPatch),
		typeof(PlayerUseHotbarItemPatch),
		typeof(HumanoidUseItemPatch),
		typeof(HumanoidUpdateEquipmentPatch),
		typeof(HumanoidEquipItemPatch),
		typeof(HumanoidStartAttackPatch),
		typeof(AttackStartBlockedItemTargetPatch),
		typeof(InventoryGuiOnRightClickItemPatch)
	};

	internal static void ApplyAll(Harmony harmony)
	{
		Harmony harmony2 = harmony;
		List<string> list = new List<string>();
		IReadOnlyList<Type> harmonyPatchTypes = GetHarmonyPatchTypes(Assembly.GetExecutingAssembly());
		ValidateRequiredPatchDiscovery(harmonyPatchTypes, list);
		for (int i = 0; i < harmonyPatchTypes.Count; i++)
		{
			ApplyPatch(harmony2, harmonyPatchTypes[i], list);
		}
		PatchOptionalCompat("GuildsCompat", delegate
		{
			GuildsCompat.TryPatch(harmony2);
		});
		PatchOptionalCompat("TargetPortalCompat", delegate
		{
			TargetPortalCompat.TryPatch(harmony2);
		});
		if (list.Count == 0)
		{
			return;
		}
		string text = "Failed to apply required patches: " + string.Join(", ", list);
		Plugin.Log.LogError((object)text);
		throw new InvalidOperationException(text);
	}

	private static void ValidateRequiredPatchDiscovery(IReadOnlyList<Type> patchTypes, ICollection<string> failedRequiredPatches)
	{
		HashSet<Type> hashSet = new HashSet<Type>(patchTypes);
		foreach (Type requiredPatchType in RequiredPatchTypes)
		{
			if (!hashSet.Contains(requiredPatchType))
			{
				failedRequiredPatches.Add(requiredPatchType.Name + " (not discovered)");
				Plugin.Log.LogError((object)("Required patch " + requiredPatchType.Name + " was not discovered. Check its HarmonyPatch attribute."));
			}
		}
	}

	private static void ApplyPatch(Harmony harmony, Type patchType, ICollection<string> failedRequiredPatches)
	{
		bool flag = RequiredPatchTypes.Contains(patchType);
		try
		{
			harmony.CreateClassProcessor(patchType).Patch();
		}
		catch (Exception ex)
		{
			if (flag)
			{
				failedRequiredPatches.Add(patchType.Name);
				Plugin.Log.LogError((object)("Failed to patch required " + patchType.Name + ": " + ex.GetType().Name + ": " + ex.Message));
			}
			else
			{
				Plugin.Log.LogWarning((object)("Failed to patch optional " + patchType.Name + ": " + ex.GetType().Name + ": " + ex.Message));
			}
		}
	}

	private static IReadOnlyList<Type> GetHarmonyPatchTypes(Assembly assembly)
	{
		List<Type> list = new List<Type>();
		foreach (Type loadableType in GetLoadableTypes(assembly))
		{
			if (loadableType.IsClass && HasHarmonyPatchAttribute(loadableType))
			{
				list.Add(loadableType);
			}
		}
		list.Sort((Type left, Type right) => string.Compare(left.FullName, right.FullName, StringComparison.Ordinal));
		return list;
	}

	private static void PatchOptionalCompat(string name, Action patchAction)
	{
		try
		{
			patchAction();
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Failed to patch " + name + ": " + ex.GetType().Name + ": " + ex.Message));
		}
	}

	private static bool HasHarmonyPatchAttribute(Type type)
	{
		return type.GetCustomAttributes(typeof(HarmonyPatch), inherit: false).Length != 0;
	}

	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			List<Type> list = new List<Type>();
			if (ex.Types == null)
			{
				return list;
			}
			for (int i = 0; i < ex.Types.Length; i++)
			{
				Type type = ex.Types[i];
				if (type != null)
				{
					list.Add(type);
				}
			}
			return list;
		}
	}
}
