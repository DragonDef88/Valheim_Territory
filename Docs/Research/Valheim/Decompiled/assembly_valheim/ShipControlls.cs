using UnityEngine;

public class ShipControlls : MonoBehaviour, Interactable, Hoverable, IDoodadController
{
	public string m_hoverText = "";

	public Ship m_ship;

	public float m_maxUseRange = 10f;

	public Transform m_attachPoint;

	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	public string m_attachAnimation = "attach_chair";

	private ZNetView m_nview;

	private void Awake()
	{
		m_nview = ((Component)m_ship).GetComponent<ZNetView>();
		m_nview.Register<long>("RequestControl", RPC_RequestControl);
		m_nview.Register<long>("ReleaseControl", RPC_ReleaseControl);
		m_nview.Register<bool>("RequestRespons", RPC_RequestRespons);
	}

	public bool IsValid()
	{
		return Object.op_Implicit((Object)(object)this);
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (!InUseDistance(character))
		{
			return false;
		}
		Player player = character as Player;
		if ((Object)(object)player == (Object)null || player.IsEncumbered())
		{
			return false;
		}
		if ((Object)(object)player.GetStandingOnShip() != (Object)(object)m_ship)
		{
			return false;
		}
		m_nview.InvokeRPC("RequestControl", player.GetPlayerID());
		return false;
	}

	public Component GetControlledComponent()
	{
		return (Component)(object)m_ship;
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public void ApplyControlls(Vector3 moveDir, Vector3 lookDir, bool run, bool autoRun, bool block)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_ship.ApplyControlls(moveDir);
	}

	public string GetHoverText()
	{
		if (!InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=#888888>$piece_toofar</color>");
		}
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + m_hoverText);
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_hoverText);
	}

	private void RPC_RequestControl(long sender, long playerID)
	{
		if (m_nview.IsOwner() && m_ship.IsPlayerInBoat(playerID))
		{
			if (GetUser() == playerID || !HaveValidUser())
			{
				m_nview.GetZDO().Set(ZDOVars.s_user, playerID);
				m_nview.InvokeRPC(sender, "RequestRespons", true);
			}
			else
			{
				m_nview.InvokeRPC(sender, "RequestRespons", false);
			}
		}
	}

	private void RPC_ReleaseControl(long sender, long playerID)
	{
		if (m_nview.IsOwner() && GetUser() == playerID)
		{
			m_nview.GetZDO().Set(ZDOVars.s_user, 0L);
		}
	}

	private void RPC_RequestRespons(long sender, bool granted)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			return;
		}
		if (granted)
		{
			Player.m_localPlayer.StartDoodadControl(this);
			if ((Object)(object)m_attachPoint != (Object)null)
			{
				Player.m_localPlayer.AttachStart(m_attachPoint, null, hideWeapons: false, isBed: false, onShip: true, m_attachAnimation, m_detachOffset);
			}
		}
		else
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse");
		}
	}

	public void OnUseStop(Player player)
	{
		if (m_nview.IsValid())
		{
			m_nview.InvokeRPC("ReleaseControl", player.GetPlayerID());
			if ((Object)(object)m_attachPoint != (Object)null)
			{
				player.AttachStop();
			}
		}
	}

	public bool HaveValidUser()
	{
		long user = GetUser();
		if (user != 0L)
		{
			return m_ship.IsPlayerInBoat(user);
		}
		return false;
	}

	private long GetUser()
	{
		if (!m_nview.IsValid())
		{
			return 0L;
		}
		return m_nview.GetZDO().GetLong(ZDOVars.s_user, 0L);
	}

	private bool InUseDistance(Humanoid human)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)human).transform.position, m_attachPoint.position) < m_maxUseRange;
	}
}
