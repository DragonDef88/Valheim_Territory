using Splatform;
using UnityEngine;

public class TeleportWorld : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
	public float m_activationRange = 5f;

	public float m_exitDistance = 1f;

	public Transform m_proximityRoot;

	[ColorUsage(true, true)]
	public Color m_colorUnconnected = Color.white;

	[ColorUsage(true, true)]
	public Color m_colorTargetfound = Color.white;

	public EffectFade m_target_found;

	public MeshRenderer m_model;

	public EffectList m_connected;

	public bool m_allowAllItems;

	private ZNetView m_nview;

	private bool m_hadTarget;

	private float m_colorAlpha;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.GetZDO() == null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		m_hadTarget = HaveTarget();
		m_nview.Register<string, string>("RPC_SetTag", RPC_SetTag);
		m_nview.Register<ZDOID>("RPC_SetConnected", RPC_SetConnected);
		((MonoBehaviour)this).InvokeRepeating("UpdatePortal", 0.5f, 0.5f);
	}

	public string GetHoverText()
	{
		string text = StringExtensionMethods.RemoveRichTextTags(GetText());
		string text2 = (HaveTarget() ? "$piece_portal_connected" : "$piece_portal_unconnected");
		return Localization.instance.Localize("$piece_portal $piece_portal_tag:\"" + text + "\"  [" + text2 + "]\n[<color=yellow><b>$KEY_Use</b></color>] $piece_portal_settag");
	}

	public string GetHoverName()
	{
		return "Teleport";
	}

	public bool Interact(Humanoid human, bool hold, bool alt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(((Component)this).transform.position))
		{
			human.Message(MessageHud.MessageType.Center, "$piece_noaccess");
			return true;
		}
		TextInput.instance.RequestText(this, "$piece_portal_tag", 10);
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void UpdatePortal()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && !((Object)(object)m_proximityRoot == (Object)null))
		{
			Player closestPlayer = Player.GetClosestPlayer(m_proximityRoot.position, m_activationRange);
			bool flag = HaveTarget();
			if (flag && !m_hadTarget)
			{
				m_connected.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
			m_hadTarget = flag;
			bool flag2 = false;
			if (Object.op_Implicit((Object)(object)closestPlayer))
			{
				flag2 = closestPlayer.IsTeleportable() || m_allowAllItems;
			}
			m_target_found.SetActive(flag2 && TargetFound());
		}
	}

	private void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		m_colorAlpha = Mathf.MoveTowards(m_colorAlpha, m_hadTarget ? 1f : 0f, Time.deltaTime);
		((Renderer)m_model).material.SetColor("_EmissionColor", Color.Lerp(m_colorUnconnected, m_colorTargetfound, m_colorAlpha));
	}

	public void Teleport(Player player)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (!TargetFound())
		{
			return;
		}
		if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoPortals))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_blocked");
			return;
		}
		if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoBossPortals) && (RandEventSystem.instance.GetBossEvent() != null || (ZoneSystem.instance.GetGlobalKey(GlobalKeys.activeBosses, out float value) && value > 0f)))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_blockedbyboss");
			return;
		}
		if (!m_allowAllItems && !player.IsTeleportable())
		{
			player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
			return;
		}
		ZLog.Log((object)("Teleporting " + player.GetPlayerName()));
		ZDO zDO = ZDOMan.instance.GetZDO(m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal));
		if (zDO != null)
		{
			Vector3 position = zDO.GetPosition();
			Quaternion rotation = zDO.GetRotation();
			Vector3 val = rotation * Vector3.forward;
			Vector3 pos = position + val * m_exitDistance + Vector3.up;
			player.TeleportTo(pos, rotation, distantTeleport: true);
			Game.instance.IncrementPlayerStat(PlayerStatType.PortalsUsed);
		}
	}

	public string GetText()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		ZDO zDO = m_nview.GetZDO();
		if (zDO == null)
		{
			return "";
		}
		string @string = zDO.GetString(ZDOVars.s_tagauthor);
		PlatformUserID userId = (PlatformUserID)(string.IsNullOrEmpty(@string) ? PlatformUserID.None : new PlatformUserID(@string));
		return CensorShittyWords.FilterUGC(zDO.GetString(ZDOVars.s_tag), UGCType.Text, userId, 0L);
	}

	private void GetTagSignature(out string tagRaw, out string authorId)
	{
		ZDO zDO = m_nview.GetZDO();
		tagRaw = zDO.GetString(ZDOVars.s_tag);
		authorId = zDO.GetString(ZDOVars.s_tagauthor);
	}

	public void SetText(string text)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid())
		{
			ZNetView nview = m_nview;
			object[] obj = new object[2] { text, null };
			PlatformUserID platformUserID = ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID;
			obj[1] = ((object)(PlatformUserID)(ref platformUserID)).ToString();
			nview.InvokeRPC("RPC_SetTag", obj);
		}
	}

	private void RPC_SetTag(long sender, string tag, string authorId)
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			GetTagSignature(out var tagRaw, out var authorId2);
			if (!(tagRaw == tag) || !(authorId2 == authorId))
			{
				ZDO zDO = m_nview.GetZDO();
				zDO.UpdateConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
				ZDOID connectionZDOID = zDO.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
				SetConnectedPortal(connectionZDOID);
				zDO.Set(ZDOVars.s_tag, tag);
				zDO.Set(ZDOVars.s_tagauthor, authorId);
			}
		}
	}

	private void SetConnectedPortal(ZDOID targetID)
	{
		ZDO zDO = ZDOMan.instance.GetZDO(targetID);
		if (zDO != null)
		{
			long owner = zDO.GetOwner();
			if (owner == 0L)
			{
				zDO.SetOwner(ZDOMan.GetSessionID());
				zDO.SetConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
			}
			else
			{
				m_nview.InvokeRPC(owner, "RPC_SetConnected", targetID);
			}
		}
	}

	private void RPC_SetConnected(long sender, ZDOID portalID)
	{
		ZDO zDO = ZDOMan.instance.GetZDO(portalID);
		if (zDO != null && zDO.IsOwner())
		{
			zDO.UpdateConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
		}
	}

	private bool HaveTarget()
	{
		if ((Object)(object)m_nview == (Object)null || m_nview.GetZDO() == null)
		{
			return false;
		}
		return m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal) != ZDOID.None;
	}

	private bool TargetFound()
	{
		if ((Object)(object)m_nview == (Object)null || m_nview.GetZDO() == null)
		{
			return false;
		}
		ZDOID connectionZDOID = m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
		if (connectionZDOID == ZDOID.None)
		{
			return false;
		}
		if (ZDOMan.instance.GetZDO(connectionZDOID) == null)
		{
			ZDOMan.instance.RequestZDO(connectionZDOID);
			return false;
		}
		return true;
	}
}
