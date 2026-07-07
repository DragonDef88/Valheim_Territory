namespace STUWard;

internal readonly struct ManagedWardMetadataMutationResult
{
	internal ManagedWardProjectionApplyResult ProjectionResult { get; }

	internal bool AuthoritativeMetadataChanged { get; }

	internal bool RegistrySynchronized { get; }

	internal bool FastSendTriggered { get; }

	internal bool AnyMetadataChanged
	{
		get
		{
			if (!AuthoritativeMetadataChanged)
			{
				return ProjectionResult.AnyChanged;
			}
			return true;
		}
	}

	internal ManagedWardMetadataMutationResult(ManagedWardProjectionApplyResult projectionResult, bool authoritativeMetadataChanged, bool registrySynchronized, bool fastSendTriggered)
	{
		ProjectionResult = projectionResult;
		AuthoritativeMetadataChanged = authoritativeMetadataChanged;
		RegistrySynchronized = registrySynchronized;
		FastSendTriggered = fastSendTriggered;
	}
}
