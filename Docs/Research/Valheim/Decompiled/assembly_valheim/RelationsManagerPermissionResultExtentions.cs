public static class RelationsManagerPermissionResultExtentions
{
	public static bool IsGranted(this RelationsManagerPermissionResult result)
	{
		if (result != 0)
		{
			return result == RelationsManagerPermissionResult.GrantedRequiresFiltering;
		}
		return true;
	}
}
