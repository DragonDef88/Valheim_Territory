namespace STUWard;

internal static class ManagedWardIdentity
{
	internal static bool IsManaged(PrivateArea? area)
	{
		bool matchedByComponent;
		bool matchedByZdo;
		return TryResolve(ManagedWardRef.FromArea(area), repairComponent: false, out matchedByComponent, out matchedByZdo);
	}

	internal static bool EnsureManagedComponent(PrivateArea? area)
	{
		return EnsureManagedComponent(ManagedWardRef.FromArea(area));
	}

	internal static bool EnsureManagedComponent(PrivateArea? area, ZDO? zdo)
	{
		return EnsureManagedComponent(ManagedWardRef.FromArea(area, zdo));
	}

	internal static bool EnsureManagedComponent(ManagedWardRef ward)
	{
		bool matchedByComponent;
		bool matchedByZdo;
		return TryResolve(ward, repairComponent: true, out matchedByComponent, out matchedByZdo) && matchedByComponent;
	}

	internal static bool TryResolve(PrivateArea? area, ZDO? zdo, bool repairComponent, out bool matchedByComponent, out bool matchedByZdo)
	{
		return TryResolve(ManagedWardRef.FromArea(area, zdo), repairComponent, out matchedByComponent, out matchedByZdo);
	}

	internal static bool TryResolve(ManagedWardRef ward, bool repairComponent, out bool matchedByComponent, out bool matchedByZdo)
	{
		matchedByComponent = ward.HasManagedComponent;
		matchedByZdo = ward.IsManagedZdo;
		if (!ward.HasArea)
		{
			return matchedByZdo;
		}
		if ((!matchedByComponent & matchedByZdo) && repairComponent)
		{
			bool added;
			ManagedWardRef managedWardRef = ward.EnsureManagedComponent(out added);
			matchedByComponent = managedWardRef.HasManagedComponent;
			if (added)
			{
				Plugin.LogWardDiagnosticVerbose("Placement.Identity", "Restored missing StuWardArea component from managed ward ZDO identity. " + WardDiagnosticInfo.DescribeWard(managedWardRef.Area));
			}
		}
		return matchedByComponent | matchedByZdo;
	}
}
