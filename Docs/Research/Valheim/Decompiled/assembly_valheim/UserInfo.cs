using Splatform;

public class UserInfo : ISerializableParameter
{
	public string Name;

	public PlatformUserID UserId;

	public static UserInfo GetLocalUser()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return new UserInfo
		{
			Name = Game.instance.GetPlayerProfile().GetName(),
			UserId = ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID
		};
	}

	public void Deserialize(ref ZPackage pkg)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Name = pkg.ReadString();
		UserId = new PlatformUserID(pkg.ReadString());
	}

	public void Serialize(ref ZPackage pkg)
	{
		pkg.Write(Name);
		pkg.Write(((object)(PlatformUserID)(ref UserId)).ToString());
	}

	public string GetDisplayName()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return CensorShittyWords.FilterUGC(Name, UGCType.CharacterName, UserId, 0L);
	}
}
