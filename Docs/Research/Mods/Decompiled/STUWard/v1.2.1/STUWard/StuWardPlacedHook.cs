using UnityEngine;

namespace STUWard;

internal sealed class StuWardPlacedHook : MonoBehaviour, IPlaced
{
	public void OnPlaced()
	{
		PrivateArea component = ((Component)this).GetComponent<PrivateArea>();
		ManagedWardRef ward = ManagedWardRef.FromArea(component);
		if (ManagedWardIdentity.EnsureManagedComponent(ward))
		{
			WardOwnership.TryStampLocalManagedWardOwnerAccount(ward);
			WardOwnership.NotifyServerManagedWardPlaced(ward);
			ManagedWardMapStateService.NotifyLiveWardMutation(component, ManagedWardMapMutationKind.IndexAndPins, "local managed ward placed");
			Plugin.LogWardDiagnosticVerbose("Placement.OnPlaced", "IPlaced.OnPlaced hit for managed ward. " + WardDiagnosticInfo.DescribeWard(component));
		}
	}
}
