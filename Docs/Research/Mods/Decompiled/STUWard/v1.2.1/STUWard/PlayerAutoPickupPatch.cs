using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "AutoPickup")]
internal static class PlayerAutoPickupPatch
{
	private static readonly FieldInfo? EnableAutoPickupField = AccessTools.DeclaredField(typeof(Player), "m_enableAutoPickup");

	private static readonly int AutoPickupMask = LayerMask.GetMask(new string[1] { "item" });

	private static Collider[] _autoPickupColliders = (Collider[])(object)new Collider[64];

	private static readonly List<PrivateArea> DeniedAutoPickupWardCandidates = new List<PrivateArea>();

	private static bool Prefix(Player __instance, float dt)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		if (((Character)__instance).IsTeleporting() || !IsAutoPickupEnabled())
		{
			return false;
		}
		bool flag = WardAccess.HasEnabledManagedWards() && WardItemPrefabPolicy.CanAnyPickupBeBlocked();
		Vector3 val = ((Component)__instance).transform.position + Vector3.up;
		long playerID = __instance.GetPlayerID();
		Inventory inventory = ((Humanoid)__instance).GetInventory();
		float autoPickupRange = __instance.m_autoPickupRange;
		float num = autoPickupRange * autoPickupRange;
		bool flag2 = false;
		DeniedAutoPickupWardCandidates.Clear();
		bool flag3 = false;
		int num2 = Physics.OverlapSphereNonAlloc(val, autoPickupRange, _autoPickupColliders, AutoPickupMask);
		if (num2 == _autoPickupColliders.Length)
		{
			Array.Resize(ref _autoPickupColliders, _autoPickupColliders.Length * 2);
			num2 = Physics.OverlapSphereNonAlloc(val, autoPickupRange, _autoPickupColliders, AutoPickupMask);
		}
		for (int i = 0; i < num2; i++)
		{
			Collider val2 = _autoPickupColliders[i];
			if ((Object)(object)val2 == (Object)null || (Object)(object)val2.attachedRigidbody == (Object)null)
			{
				continue;
			}
			ItemDrop component = ((Component)val2.attachedRigidbody).GetComponent<ItemDrop>();
			FloatingTerrainDummy val3 = null;
			if ((Object)(object)component == (Object)null)
			{
				val3 = ((Component)val2.attachedRigidbody).GetComponent<FloatingTerrainDummy>();
				if ((Object)(object)val3 != (Object)null)
				{
					FloatingTerrain parent = val3.m_parent;
					if ((Object)(object)parent != (Object)null)
					{
						component = ((Component)parent).GetComponent<ItemDrop>();
					}
				}
			}
			if ((Object)(object)component == (Object)null || !component.m_autoPickup || component.IsPiece() || ((Humanoid)__instance).HaveUniqueKey(component.m_itemData.m_shared.m_name))
			{
				continue;
			}
			Vector3 val4 = ((Component)component).transform.position - val;
			float sqrMagnitude = ((Vector3)(ref val4)).sqrMagnitude;
			if (sqrMagnitude > num)
			{
				continue;
			}
			ZNetView component2 = ((Component)component).GetComponent<ZNetView>();
			if ((Object)(object)component2 == (Object)null || !component2.IsValid())
			{
				continue;
			}
			int num3;
			if (flag)
			{
				num3 = (WardItemPrefabPolicy.ShouldBlockPickup(component) ? 1 : 0);
				if (num3 != 0 && !flag2)
				{
					flag3 = WardAccess.CollectDeniedManagedWardCandidates(playerID, WardAccess.GetCandidateManagedWards(val, autoPickupRange, requireEnabled: true), DeniedAutoPickupWardCandidates) > 0;
					flag2 = true;
				}
			}
			else
			{
				num3 = 0;
			}
			if (((uint)num3 & (flag3 ? 1u : 0u)) != 0 && WardAccess.IsInsideAnyManagedWard(((Component)component).transform.position, 0f, DeniedAutoPickupWardCandidates))
			{
				continue;
			}
			if (!component.CanPickup(true))
			{
				component.RequestOwn();
			}
			else
			{
				if (component.InTar())
				{
					continue;
				}
				component.Load();
				if (!inventory.CanAddItem(component.m_itemData, -1) || component.m_itemData.GetWeight(-1) + inventory.GetTotalWeight() > __instance.GetMaxCarryWeight())
				{
					continue;
				}
				if (sqrMagnitude < 0.09f)
				{
					((Humanoid)__instance).Pickup(((Component)component).gameObject, true, true);
					continue;
				}
				val4 = val - ((Component)component).transform.position;
				Vector3 val5 = ((Vector3)(ref val4)).normalized * (15f * dt);
				Transform transform = ((Component)component).transform;
				transform.position += val5;
				if ((Object)(object)val3 != (Object)null)
				{
					Transform transform2 = ((Component)val3).transform;
					transform2.position += val5;
				}
			}
		}
		return false;
	}

	private static bool IsAutoPickupEnabled()
	{
		if (!(EnableAutoPickupField == null))
		{
			return (bool)EnableAutoPickupField.GetValue(null);
		}
		return true;
	}
}
