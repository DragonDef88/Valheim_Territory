using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class ManagedWardRuntimeContexts
{
	private static readonly Dictionary<int, ManagedWardRuntimeContext> Contexts = new Dictionary<int, ManagedWardRuntimeContext>();

	internal static ManagedWardRuntimeContext GetOrCreate(PrivateArea area)
	{
		int instanceID = ((Object)area).GetInstanceID();
		if (!Contexts.TryGetValue(instanceID, out ManagedWardRuntimeContext value))
		{
			value = new ManagedWardRuntimeContext();
			Contexts[instanceID] = value;
		}
		return value;
	}

	internal static bool TryGet(PrivateArea? area, out ManagedWardRuntimeContext context)
	{
		context = null;
		if ((Object)(object)area != (Object)null)
		{
			return Contexts.TryGetValue(((Object)area).GetInstanceID(), out context);
		}
		return false;
	}

	internal static void Forget(PrivateArea? area)
	{
		if (!((Object)(object)area == (Object)null))
		{
			Contexts.Remove(((Object)area).GetInstanceID());
		}
	}

	internal static void Reset()
	{
		Contexts.Clear();
	}

	internal static void ClearObservedStates()
	{
		foreach (ManagedWardRuntimeContext value in Contexts.Values)
		{
			value.ClearObservedState();
		}
	}

	internal static void ClearHoverTexts()
	{
		foreach (ManagedWardRuntimeContext value in Contexts.Values)
		{
			value.ClearHoverText();
		}
	}

	internal static void ClearConfigurationCaches()
	{
		foreach (ManagedWardRuntimeContext value in Contexts.Values)
		{
			value.ClearConfigurationCaches();
		}
	}

	internal static void ClearAreaMarkerVisualState(PrivateArea? area)
	{
		if (TryGet(area, out ManagedWardRuntimeContext context))
		{
			context.ClearAreaMarkerVisualState();
		}
	}

	internal static void ClearObservedState(PrivateArea? area)
	{
		if (TryGet(area, out ManagedWardRuntimeContext context))
		{
			context.ClearObservedState();
		}
	}

	internal static bool TryGetCurrentDataRevision(PrivateArea? area, out uint dataRevision)
	{
		dataRevision = 0u;
		if ((Object)(object)area == (Object)null)
		{
			return false;
		}
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if (zdo == null || !zdo.IsValid())
		{
			return false;
		}
		dataRevision = zdo.DataRevision;
		return true;
	}

	internal static void ArmNextEnabledFanOutSuppression(PrivateArea? area, bool expectedEnabled)
	{
		if (!((Object)(object)area == (Object)null))
		{
			ManagedWardRuntimeContext orCreate = GetOrCreate(area);
			orCreate.HasPendingEnabledFanOutSuppression = true;
			orCreate.PendingEnabledFanOutState = expectedEnabled;
		}
	}

	internal static void ArmNextDataRevisionFanOutSuppression(PrivateArea? area)
	{
		if (TryGetCurrentDataRevision(area, out var dataRevision))
		{
			ArmNextDataRevisionFanOutSuppression(area, dataRevision);
		}
	}

	internal static void ArmNextDataRevisionFanOutSuppression(PrivateArea? area, uint baselineDataRevision)
	{
		if (!((Object)(object)area == (Object)null))
		{
			ManagedWardRuntimeContext orCreate = GetOrCreate(area);
			orCreate.HasPendingDataRevisionFanOutSuppression = true;
			orCreate.PendingDataRevisionFanOutBaseline = baselineDataRevision;
		}
	}

	internal static void ArmNextDataRevisionFanOutSuppressionIfChanged(PrivateArea? area, uint baselineDataRevision)
	{
		if (TryGetCurrentDataRevision(area, out var dataRevision) && dataRevision != baselineDataRevision)
		{
			ArmNextDataRevisionFanOutSuppression(area, baselineDataRevision);
		}
	}

	internal static bool TryConsumeEnabledFanOutSuppression(PrivateArea? area, bool currentEnabled)
	{
		if (!TryGet(area, out ManagedWardRuntimeContext context) || !context.HasPendingEnabledFanOutSuppression)
		{
			return false;
		}
		if (context.PendingEnabledFanOutState != currentEnabled)
		{
			return false;
		}
		context.HasPendingEnabledFanOutSuppression = false;
		return true;
	}

	internal static bool TryConsumeDataRevisionFanOutSuppression(PrivateArea? area, uint currentDataRevision)
	{
		if (!TryGet(area, out ManagedWardRuntimeContext context) || !context.HasPendingDataRevisionFanOutSuppression)
		{
			return false;
		}
		if (context.PendingDataRevisionFanOutBaseline == currentDataRevision)
		{
			return false;
		}
		context.HasPendingDataRevisionFanOutSuppression = false;
		return true;
	}

	internal static void ClearHoverText(PrivateArea? area)
	{
		if (TryGet(area, out ManagedWardRuntimeContext context))
		{
			context.ClearHoverText();
		}
	}

	internal static void ResetPresenceState(PrivateArea? area)
	{
		if (TryGet(area, out ManagedWardRuntimeContext context))
		{
			context.ResetPresenceState();
		}
	}

	internal static void ResetPresenceStates()
	{
		foreach (ManagedWardRuntimeContext value in Contexts.Values)
		{
			value.ResetPresenceState();
		}
	}
}
