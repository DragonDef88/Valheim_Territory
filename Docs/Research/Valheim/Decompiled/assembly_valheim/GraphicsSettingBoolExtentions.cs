public static class GraphicsSettingBoolExtentions
{
	public static bool IsShownToUser(this GraphicsSettingBool setting)
	{
		if (setting == GraphicsSettingBool.AnisotropicTextures)
		{
			return false;
		}
		return true;
	}

	public static bool IsPresentSetting(this GraphicsSettingBool setting)
	{
		if (setting == GraphicsSettingBool.Vsync)
		{
			return true;
		}
		return false;
	}

	public static string ToDisplayName(this GraphicsSettingBool obj)
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		return obj switch
		{
			GraphicsSettingBool.Vsync => Localization.instance.Localize("$settings_vsync"), 
			GraphicsSettingBool.DistantShadows => Localization.instance.Localize("$settings_distantshadows"), 
			GraphicsSettingBool.Tesselation => Localization.instance.Localize("$settings_tesselation"), 
			GraphicsSettingBool.Bloom => Localization.instance.Localize("$settings_bloom"), 
			GraphicsSettingBool.DepthOfField => Localization.instance.Localize("$settings_dof"), 
			GraphicsSettingBool.MotionBlur => Localization.instance.Localize("$settings_motionblur"), 
			GraphicsSettingBool.ChromaticAberration => Localization.instance.Localize("$settings_ca"), 
			GraphicsSettingBool.SunShafts => Localization.instance.Localize("$settings_sunshafts"), 
			GraphicsSettingBool.SoftParticles => Localization.instance.Localize("$settings_softpart"), 
			GraphicsSettingBool.AntiAliasing => Localization.instance.Localize("$settings_antialiasing"), 
			GraphicsSettingBool.AnisotropicTextures => Localization.instance.Localize("$settings_anisotropictextures"), 
			_ => ((object)EnumUtils.GetDisplayName<GraphicsSettingBool>(obj, (EnumWordSeparationMode)1, (EnumWordCasing)1)).ToString(), 
		};
	}
}
