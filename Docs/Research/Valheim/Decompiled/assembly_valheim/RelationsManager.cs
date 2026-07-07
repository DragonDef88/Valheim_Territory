using System;
using Splatform;
using UnityEngine;
using UserManagement;

public static class RelationsManager
{
	public const string c_AuthorHostPlaceholder = "host";

	public static bool PlatformRequiresTextFiltering()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		if (PlatformManager.DistributionPlatform.Platform == "Xbox")
		{
			return true;
		}
		if (PlatformManager.DistributionPlatform.HardwareInfoProvider != null && (int)PlatformManager.DistributionPlatform.HardwareInfoProvider.HardwareInfo.m_category == 2)
		{
			return true;
		}
		return false;
	}

	public static bool FilterTextCommunicationSentToUser(PlatformUserID recipient)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!PlatformRequiresTextFiltering())
		{
			return false;
		}
		if (recipient == ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID)
		{
			return false;
		}
		IRelationsProvider relationsProvider = PlatformManager.DistributionPlatform.RelationsProvider;
		if (relationsProvider == null)
		{
			return true;
		}
		if (relationsProvider.IsFriend(recipient))
		{
			return false;
		}
		return true;
	}

	public static void CheckPermissionAsync(PlatformUserID user, Permission permission, bool isSender, CheckPermissionCompletedHandler completedHandler)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected I4, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_019b: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!((PlatformUserID)(ref user)).IsValid)
		{
			ZLog.LogError((object)$"Failed to check permission {permission}: UserID was invalid");
			completedHandler(RelationsManagerPermissionResult.Error);
			return;
		}
		if (user == ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID)
		{
			completedHandler(RelationsManagerPermissionResult.Granted);
			return;
		}
		if (!isSender && MuteList.Contains(user))
		{
			Permission val = permission;
			switch ((int)val)
			{
			case 0:
			case 2:
				completedHandler(RelationsManagerPermissionResult.Denied);
				return;
			default:
				throw new NotImplementedException($"Permission {permission} has not been implemented!");
			case 1:
				break;
			}
		}
		if (!TryCheckEquivalentPrivilege(permission, out var result))
		{
			ZLog.LogError((object)$"Failed to check permission {permission} for user {user}: Equivalent privilege check failed");
			completedHandler(RelationsManagerPermissionResult.Error);
		}
		else if (!result)
		{
			ZLog.Log((object)$"Permission {permission} was denied for user {user}: Equivalent privilege was denied");
			completedHandler?.Invoke(RelationsManagerPermissionResult.Denied);
		}
		else if (PlatformManager.DistributionPlatform.RelationsProvider == null)
		{
			completedHandler?.Invoke(PlatformRequiresTextFiltering() ? RelationsManagerPermissionResult.GrantedRequiresFiltering : RelationsManagerPermissionResult.Granted);
		}
		else
		{
			PlatformManager.DistributionPlatform.RelationsProvider.GetUserProfileAsync(user, new GetUserProfileCompletedHandler(OnGetUserProfileCompleted), new GetUserProfileFailedHandler(OnGetUserProfileFailed));
		}
		void OnGetUserProfileCompleted(IUserProfile profile)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			PermissionResult val2 = profile.CheckPermission(permission);
			if (PermissionResultExtentions.IsError(val2))
			{
				ZLog.LogError((object)$"Failed to check permission {permission} for user {user}: {val2}");
				completedHandler(RelationsManagerPermissionResult.Error);
			}
			else
			{
				RelationsManagerPermissionResult result2;
				if ((int)val2 == 0)
				{
					result2 = (FilterTextCommunicationSentToUser(user) ? RelationsManagerPermissionResult.GrantedRequiresFiltering : RelationsManagerPermissionResult.Granted);
				}
				else
				{
					ZLog.Log((object)$"Permission {permission} was denied for user {user}: {val2}");
					result2 = RelationsManagerPermissionResult.Denied;
				}
				completedHandler(result2);
			}
		}
		void OnGetUserProfileFailed(PlatformUserID userId, GetUserProfileFailReason reason)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected I4, but got Unknown
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			switch ((int)reason)
			{
			case 1:
			case 2:
				completedHandler(FilterTextCommunicationSentToUser(userId) ? RelationsManagerPermissionResult.GrantedRequiresFiltering : RelationsManagerPermissionResult.Granted);
				break;
			case 0:
			case 3:
				ZLog.LogError((object)$"Failed to get user profile for user {userId}: {reason}, {permission} permission check for user {userId} will fail.");
				completedHandler(RelationsManagerPermissionResult.Error);
				break;
			default:
				ZLog.LogError((object)$"GetUserProfileFailReason {reason} not implemented!");
				completedHandler(RelationsManagerPermissionResult.Error);
				break;
			}
		}
	}

	private static bool TryCheckEquivalentPrivilege(Permission permission, out bool result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Privilege val;
		switch ((int)permission)
		{
		case 1:
			val = (Privilege)0;
			break;
		case 0:
			val = (Privilege)3;
			break;
		case 2:
			val = (Privilege)2;
			break;
		default:
			ZLog.LogError((object)$"Failed to check equivalent privilege for permission {permission}: There is no equivalent privilege");
			result = false;
			return false;
		}
		PrivilegeResult val2 = PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege(val);
		if (PrivilegeResultExtentions.IsError(val2))
		{
			ZLog.LogError((object)$"Failed to check privilege {val}: {val2}");
			result = false;
			return false;
		}
		result = PrivilegeResultExtentions.IsGranted(val2);
		return true;
	}

	public static bool UpdateAuthorIfHost(string authorString, ref string resolvedAuthor)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (authorString != "host")
		{
			return false;
		}
		if (!ZNet.instance.IsCurrentServerDedicated())
		{
			return false;
		}
		if (ZNet.instance.GetPlayerList().Count <= 0)
		{
			return false;
		}
		PlatformUserID id = ZNet.instance.GetPlayerList()[0].m_userInfo.m_id;
		if (!((PlatformUserID)(ref id)).IsValid)
		{
			Debug.LogWarning((object)"Server host lacked valid ID while trying to resolve unclaimed object authorship.");
			return false;
		}
		Debug.Log((object)("There was an update from a placeholder PlatformUserID to the following:" + ((object)(PlatformUserID)(ref id)).ToString()));
		resolvedAuthor = ((object)(PlatformUserID)(ref id)).ToString();
		return true;
	}

	public static bool IsBlocked(PlatformUserID user)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (PlatformManager.DistributionPlatform.RelationsProvider == null)
		{
			return false;
		}
		return PlatformManager.DistributionPlatform.RelationsProvider.IsBlocked(user);
	}
}
