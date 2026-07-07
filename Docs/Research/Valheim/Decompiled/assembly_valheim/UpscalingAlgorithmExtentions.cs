public static class UpscalingAlgorithmExtentions
{
	public static string ToDisplayName(this UpscalingAlgorithm _enum)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		return _enum switch
		{
			UpscalingAlgorithm.Bilinear => Localization.instance.Localize("$settings_bilinear"), 
			UpscalingAlgorithm.NearestNeighbor => Localization.instance.Localize("$settings_nearestneighbor"), 
			_ => ((object)EnumUtils.GetDisplayName<UpscalingAlgorithm>(_enum, (EnumWordSeparationMode)1, (EnumWordCasing)1)).ToString(), 
		};
	}
}
