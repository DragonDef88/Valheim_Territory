using UnityEngine;

namespace STUWard;

internal readonly struct ManagedWardRef
{
	internal PrivateArea? Area { get; }

	internal ZNetView? NView { get; }

	internal ZDO? Zdo { get; }

	internal StuWardArea? Component { get; }

	internal bool HasArea => (Object)(object)Area != (Object)null;

	internal bool HasManagedComponent => (Object)(object)Component != (Object)null;

	internal bool IsManagedZdo => WardOwnership.IsManagedWardZdo(Zdo);

	internal bool IsManaged
	{
		get
		{
			if (!HasManagedComponent)
			{
				return IsManagedZdo;
			}
			return true;
		}
	}

	internal bool IsPlacementGhost
	{
		get
		{
			if ((Object)(object)Area != (Object)null)
			{
				return Player.IsPlacementGhost(((Component)Area).gameObject);
			}
			return false;
		}
	}

	internal bool HasValidNetworkIdentity
	{
		get
		{
			if ((Object)(object)NView != (Object)null && NView.IsValid())
			{
				return Zdo != null;
			}
			return false;
		}
	}

	internal bool IsOwner
	{
		get
		{
			if ((Object)(object)NView != (Object)null && NView.IsValid())
			{
				return NView.IsOwner();
			}
			return false;
		}
	}

	internal long CreatorPlayerId
	{
		get
		{
			ZDO? zdo = Zdo;
			if (zdo == null)
			{
				return 0L;
			}
			return zdo.GetLong(ZDOVars.s_creator, 0L);
		}
	}

	private ManagedWardRef(PrivateArea? area, ZNetView? nview, ZDO? zdo, StuWardArea? component)
	{
		Area = area;
		NView = nview;
		Zdo = zdo;
		Component = component;
	}

	internal static ManagedWardRef FromArea(PrivateArea? area)
	{
		return FromArea(area, null);
	}

	internal static ManagedWardRef FromArea(PrivateArea? area, ZDO? knownZdo)
	{
		ZNetView nView = WardPrivateAreaSafeAccess.GetNView(area);
		ZDO zdo = ((knownZdo != null && knownZdo.IsValid()) ? knownZdo : WardPrivateAreaSafeAccess.GetZdo(nView));
		return new ManagedWardRef(area, nView, zdo, ((Object)(object)area != (Object)null) ? ((Component)area).GetComponent<StuWardArea>() : null);
	}

	internal ManagedWardRef EnsureManagedComponent(out bool added)
	{
		added = false;
		if ((Object)(object)Area == (Object)null || HasManagedComponent || !IsManagedZdo || IsPlacementGhost)
		{
			return this;
		}
		((Component)Area).gameObject.AddComponent<StuWardArea>();
		added = true;
		return FromArea(Area, Zdo);
	}
}
