namespace STUWard;

internal struct WardCheckScopeState
{
	private WardAccess.RestrictionScope _restrictionScope;

	private WardAccess.ManagedWardAllowScope _allowScope;

	internal void EnterRestriction(WardRestrictionOptions restriction)
	{
		_restrictionScope = WardAccess.EnterRestrictionScope(restriction);
	}

	internal void EnterManagedWardAllow()
	{
		_allowScope = WardAccess.EnterManagedWardAllowScope();
	}

	internal void Dispose()
	{
		_restrictionScope.Dispose();
		_allowScope.Dispose();
	}
}
