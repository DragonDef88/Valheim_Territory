using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PrivateArea : MonoBehaviour, Hoverable, Interactable
{
	public string m_name = "Guard stone";

	public float m_radius = 10f;

	public float m_updateConnectionsInterval = 5f;

	public bool m_enabledByDefault;

	public Character.Faction m_ownerFaction;

	public GameObject m_enabledEffect;

	public CircleProjector m_areaMarker;

	public EffectList m_flashEffect = new EffectList();

	public EffectList m_activateEffect = new EffectList();

	public EffectList m_deactivateEffect = new EffectList();

	public EffectList m_addPermittedEffect = new EffectList();

	public EffectList m_removedPermittedEffect = new EffectList();

	public GameObject m_connectEffect;

	public GameObject m_inRangeEffect;

	public MeshRenderer m_model;

	private ZNetView m_nview;

	private Piece m_piece;

	private bool m_flashAvailable = true;

	private bool m_tempChecked;

	private List<GameObject> m_connectionInstances = new List<GameObject>();

	private float m_connectionUpdateTime = -1000f;

	private List<PrivateArea> m_connectedAreas = new List<PrivateArea>();

	private static List<PrivateArea> m_allAreas = new List<PrivateArea>();

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)m_areaMarker))
		{
			m_areaMarker.m_radius = m_radius;
		}
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.IsValid())
		{
			WearNTear component = ((Component)this).GetComponent<WearNTear>();
			component.m_onDamaged = (Action)Delegate.Combine(component.m_onDamaged, new Action(OnDamaged));
			m_piece = ((Component)this).GetComponent<Piece>();
			if (Object.op_Implicit((Object)(object)m_areaMarker))
			{
				((Component)m_areaMarker).gameObject.SetActive(false);
			}
			if (Object.op_Implicit((Object)(object)m_inRangeEffect))
			{
				m_inRangeEffect.SetActive(false);
			}
			m_allAreas.Add(this);
			((MonoBehaviour)this).InvokeRepeating("UpdateStatus", 0f, 1f);
			m_nview.Register<long>("ToggleEnabled", RPC_ToggleEnabled);
			m_nview.Register<long, string>("TogglePermitted", RPC_TogglePermitted);
			m_nview.Register("FlashShield", RPC_FlashShield);
			if (m_enabledByDefault && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_enabled, value: true);
			}
		}
	}

	private void OnDestroy()
	{
		m_allAreas.Remove(this);
	}

	private void UpdateStatus()
	{
		bool flag = IsEnabled();
		m_enabledEffect.SetActive(flag);
		m_flashAvailable = true;
		Material[] materials = ((Renderer)m_model).materials;
		foreach (Material val in materials)
		{
			if (flag)
			{
				val.EnableKeyword("_EMISSION");
			}
			else
			{
				val.DisableKeyword("_EMISSION");
			}
		}
	}

	public string GetHoverText()
	{
		if (!m_nview.IsValid())
		{
			return "";
		}
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return "";
		}
		if (m_ownerFaction != 0)
		{
			return Localization.instance.Localize(m_name);
		}
		ShowAreaMarker();
		StringBuilder stringBuilder = new StringBuilder(256);
		if (m_piece.IsCreator())
		{
			if (IsEnabled())
			{
				stringBuilder.Append(m_name + " ( $piece_guardstone_active )");
				stringBuilder.Append("\n$piece_guardstone_owner:" + GetCreatorName());
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_deactivate");
			}
			else
			{
				stringBuilder.Append(m_name + " ($piece_guardstone_inactive )");
				stringBuilder.Append("\n$piece_guardstone_owner:" + GetCreatorName());
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_activate");
			}
		}
		else if (IsEnabled())
		{
			stringBuilder.Append(m_name + " ( $piece_guardstone_active )");
			stringBuilder.Append("\n$piece_guardstone_owner:" + GetCreatorName());
		}
		else
		{
			stringBuilder.Append(m_name + " ( $piece_guardstone_inactive )");
			stringBuilder.Append("\n$piece_guardstone_owner:" + GetCreatorName());
			if (IsPermitted(Player.m_localPlayer.GetPlayerID()))
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_remove");
			}
			else
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_add");
			}
		}
		AddUserList(stringBuilder);
		return Localization.instance.Localize(stringBuilder.ToString());
	}

	private void AddUserList(StringBuilder text)
	{
		List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers();
		text.Append("\n$piece_guardstone_additional: ");
		for (int i = 0; i < permittedPlayers.Count; i++)
		{
			text.Append(CensorShittyWords.FilterUGC(permittedPlayers[i].Value, UGCType.CharacterName, m_piece.GetCreator()));
			if (i != permittedPlayers.Count - 1)
			{
				text.Append(", ");
			}
		}
	}

	private void RemovePermitted(long playerID)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers();
		if (permittedPlayers.RemoveAll((KeyValuePair<long, string> x) => x.Key == playerID) > 0)
		{
			SetPermittedPlayers(permittedPlayers);
			m_removedPermittedEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}

	private bool IsPermitted(long playerID)
	{
		foreach (KeyValuePair<long, string> permittedPlayer in GetPermittedPlayers())
		{
			if (permittedPlayer.Key == playerID)
			{
				return true;
			}
		}
		return false;
	}

	private void AddPermitted(long playerID, string playerName)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers();
		foreach (KeyValuePair<long, string> item in permittedPlayers)
		{
			if (item.Key == playerID)
			{
				return;
			}
		}
		permittedPlayers.Add(new KeyValuePair<long, string>(playerID, playerName));
		SetPermittedPlayers(permittedPlayers);
		m_addPermittedEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
	}

	private void SetPermittedPlayers(List<KeyValuePair<long, string>> users)
	{
		m_nview.GetZDO().Set(ZDOVars.s_permitted, users.Count);
		for (int i = 0; i < users.Count; i++)
		{
			KeyValuePair<long, string> keyValuePair = users[i];
			m_nview.GetZDO().Set("pu_id" + i, keyValuePair.Key);
			m_nview.GetZDO().Set("pu_name" + i, keyValuePair.Value);
		}
	}

	private List<KeyValuePair<long, string>> GetPermittedPlayers()
	{
		List<KeyValuePair<long, string>> list = new List<KeyValuePair<long, string>>();
		int @int = m_nview.GetZDO().GetInt(ZDOVars.s_permitted);
		for (int i = 0; i < @int; i++)
		{
			long @long = m_nview.GetZDO().GetLong("pu_id" + i, 0L);
			string @string = m_nview.GetZDO().GetString("pu_name" + i);
			if (@long != 0L)
			{
				list.Add(new KeyValuePair<long, string>(@long, @string));
			}
		}
		return list;
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid human, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (m_ownerFaction != 0)
		{
			return false;
		}
		Player player = human as Player;
		if (m_piece.IsCreator())
		{
			m_nview.InvokeRPC("ToggleEnabled", player.GetPlayerID());
			return true;
		}
		if (IsEnabled())
		{
			return false;
		}
		m_nview.InvokeRPC("TogglePermitted", player.GetPlayerID(), player.GetPlayerName());
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void RPC_TogglePermitted(long uid, long playerID, string name)
	{
		if (m_nview.IsOwner() && !IsEnabled())
		{
			if (IsPermitted(playerID))
			{
				RemovePermitted(playerID);
			}
			else
			{
				AddPermitted(playerID, name);
			}
		}
	}

	private void RPC_ToggleEnabled(long uid, long playerID)
	{
		ZLog.Log((object)("Toggle enabled from " + playerID + "  creator is " + m_piece.GetCreator()));
		if (m_nview.IsOwner() && m_piece.GetCreator() == playerID)
		{
			SetEnabled(!IsEnabled());
		}
	}

	private bool IsEnabled()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.GetZDO().GetBool(ZDOVars.s_enabled);
	}

	private void SetEnabled(bool enabled)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		m_nview.GetZDO().Set(ZDOVars.s_enabled, enabled);
		UpdateStatus();
		if (enabled)
		{
			m_activateEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
		else
		{
			m_deactivateEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}

	public void Setup(string name)
	{
		m_nview.GetZDO().Set(ZDOVars.s_creatorName, name);
	}

	public void PokeAllAreasInRange()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		foreach (PrivateArea allArea in m_allAreas)
		{
			if (!((Object)(object)allArea == (Object)(object)this) && IsInside(((Component)allArea).transform.position, 0f))
			{
				allArea.StartInRangeEffect();
			}
		}
	}

	private void StartInRangeEffect()
	{
		m_inRangeEffect.SetActive(true);
		((MonoBehaviour)this).CancelInvoke("StopInRangeEffect");
		((MonoBehaviour)this).Invoke("StopInRangeEffect", 0.2f);
	}

	private void StopInRangeEffect()
	{
		m_inRangeEffect.SetActive(false);
	}

	public void PokeConnectionEffects()
	{
		List<PrivateArea> connectedAreas = GetConnectedAreas();
		StartConnectionEffects();
		foreach (PrivateArea item in connectedAreas)
		{
			item.StartConnectionEffects();
		}
	}

	private void StartConnectionEffects()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		List<PrivateArea> list = new List<PrivateArea>();
		foreach (PrivateArea allArea in m_allAreas)
		{
			if (!((Object)(object)allArea == (Object)(object)this) && IsInside(((Component)allArea).transform.position, 0f))
			{
				list.Add(allArea);
			}
		}
		Vector3 val = ((Component)this).transform.position + Vector3.up * 1.4f;
		if (m_connectionInstances.Count != list.Count)
		{
			StopConnectionEffects();
			for (int i = 0; i < list.Count; i++)
			{
				GameObject item = Object.Instantiate<GameObject>(m_connectEffect, val, Quaternion.identity, ((Component)this).transform);
				m_connectionInstances.Add(item);
			}
		}
		if (m_connectionInstances.Count != 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				Vector3 val2 = ((Component)list[j]).transform.position + Vector3.up * 1.4f - val;
				Quaternion rotation = Quaternion.LookRotation(((Vector3)(ref val2)).normalized);
				GameObject obj = m_connectionInstances[j];
				obj.transform.position = val;
				obj.transform.rotation = rotation;
				obj.transform.localScale = new Vector3(1f, 1f, ((Vector3)(ref val2)).magnitude);
			}
			((MonoBehaviour)this).CancelInvoke("StopConnectionEffects");
			((MonoBehaviour)this).Invoke("StopConnectionEffects", 0.3f);
		}
	}

	private void StopConnectionEffects()
	{
		foreach (GameObject connectionInstance in m_connectionInstances)
		{
			Object.Destroy((Object)(object)connectionInstance);
		}
		m_connectionInstances.Clear();
	}

	private string GetCreatorName()
	{
		return CensorShittyWords.FilterUGC(m_nview.GetZDO().GetString(ZDOVars.s_creatorName), UGCType.CharacterName, m_piece.GetCreator());
	}

	public static bool OnObjectDamaged(Vector3 point, Character attacker, bool destroyed)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (PrivateArea allArea in m_allAreas)
		{
			if (allArea.IsEnabled() && allArea.IsInside(point, 0f))
			{
				allArea.OnObjectDamaged(attacker, destroyed);
				return true;
			}
		}
		return false;
	}

	public static bool CheckAccess(Vector3 point, float radius = 0f, bool flash = true, bool wardCheck = false)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		List<PrivateArea> list = new List<PrivateArea>();
		bool flag = true;
		if (wardCheck)
		{
			flag = true;
			foreach (PrivateArea allArea in m_allAreas)
			{
				if (allArea.IsEnabled() && allArea.IsInside(point, radius) && !allArea.HaveLocalAccess())
				{
					flag = false;
					list.Add(allArea);
				}
			}
		}
		else
		{
			flag = false;
			foreach (PrivateArea allArea2 in m_allAreas)
			{
				if (allArea2.IsEnabled() && allArea2.IsInside(point, radius))
				{
					if (allArea2.HaveLocalAccess())
					{
						flag = true;
					}
					else
					{
						list.Add(allArea2);
					}
				}
			}
		}
		if (!flag && list.Count > 0)
		{
			if (flash)
			{
				foreach (PrivateArea item in list)
				{
					item.FlashShield(flashConnected: false);
				}
			}
			return false;
		}
		return true;
	}

	private bool HaveLocalAccess()
	{
		if (m_piece.IsCreator())
		{
			return true;
		}
		if (IsPermitted(Player.m_localPlayer.GetPlayerID()))
		{
			return true;
		}
		return false;
	}

	private List<PrivateArea> GetConnectedAreas(bool forceUpdate = false)
	{
		if (Time.time - m_connectionUpdateTime > m_updateConnectionsInterval || forceUpdate)
		{
			GetAllConnectedAreas(m_connectedAreas);
			m_connectionUpdateTime = Time.time;
		}
		return m_connectedAreas;
	}

	private void GetAllConnectedAreas(List<PrivateArea> areas)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Queue<PrivateArea> queue = new Queue<PrivateArea>();
		queue.Enqueue(this);
		foreach (PrivateArea allArea in m_allAreas)
		{
			allArea.m_tempChecked = false;
		}
		m_tempChecked = true;
		while (queue.Count > 0)
		{
			PrivateArea privateArea = queue.Dequeue();
			foreach (PrivateArea allArea2 in m_allAreas)
			{
				if (!allArea2.m_tempChecked && allArea2.IsEnabled() && allArea2.IsInside(((Component)privateArea).transform.position, 0f))
				{
					allArea2.m_tempChecked = true;
					queue.Enqueue(allArea2);
					areas.Add(allArea2);
				}
			}
		}
	}

	private void OnObjectDamaged(Character attacker, bool destroyed)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		FlashShield(flashConnected: false);
		if (m_ownerFaction == Character.Faction.Players)
		{
			return;
		}
		List<Character> list = new List<Character>();
		Character.GetCharactersInRange(((Component)this).transform.position, m_radius * 2f, list);
		foreach (Character item in list)
		{
			if (item.GetFaction() == m_ownerFaction)
			{
				MonsterAI component = ((Component)item).GetComponent<MonsterAI>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.OnPrivateAreaAttacked(attacker, destroyed);
				}
				NpcTalk component2 = ((Component)item).GetComponent<NpcTalk>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.OnPrivateAreaAttacked(attacker);
				}
			}
		}
	}

	private void FlashShield(bool flashConnected)
	{
		if (!m_flashAvailable)
		{
			return;
		}
		m_flashAvailable = false;
		m_nview.InvokeRPC(ZNetView.Everybody, "FlashShield");
		if (!flashConnected)
		{
			return;
		}
		foreach (PrivateArea connectedArea in GetConnectedAreas())
		{
			if (connectedArea.m_nview.IsValid())
			{
				connectedArea.m_nview.InvokeRPC(ZNetView.Everybody, "FlashShield");
			}
		}
	}

	private void RPC_FlashShield(long uid)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_flashEffect.Create(((Component)this).transform.position, Quaternion.identity);
	}

	public static bool InsideFactionArea(Vector3 point, Character.Faction faction)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (PrivateArea allArea in m_allAreas)
		{
			if (allArea.m_ownerFaction == faction && allArea.IsInside(point, 0f))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsInside(Vector3 point, float radius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Utils.DistanceXZ(((Component)this).transform.position, point) < m_radius + radius;
	}

	public void ShowAreaMarker()
	{
		if (Object.op_Implicit((Object)(object)m_areaMarker))
		{
			((Component)m_areaMarker).gameObject.SetActive(true);
			((MonoBehaviour)this).CancelInvoke("HideMarker");
			((MonoBehaviour)this).Invoke("HideMarker", 0.5f);
		}
	}

	private void HideMarker()
	{
		((Component)m_areaMarker).gameObject.SetActive(false);
	}

	private void OnDamaged()
	{
		if (IsEnabled())
		{
			FlashShield(flashConnected: false);
		}
	}

	private void OnDrawGizmosSelected()
	{
	}
}
