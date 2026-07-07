using System;
using System.Collections.Generic;
using Splatform;
using UnityEngine;

public class Tameable : MonoBehaviour, Interactable, TextReceiver
{
	public delegate string TextGetter();

	private const float m_playerMaxDistance = 15f;

	private const float m_tameDeltaTime = 3f;

	private static List<Player> s_nearbyPlayers = new List<Player>();

	public float m_fedDuration = 30f;

	public float m_tamingTime = 1800f;

	public bool m_startsTamed;

	public EffectList m_tamedEffect = new EffectList();

	public EffectList m_sootheEffect = new EffectList();

	public EffectList m_petEffect = new EffectList();

	public bool m_commandable;

	public float m_unsummonDistance;

	public float m_unsummonOnOwnerLogoutSeconds;

	public EffectList m_unSummonEffect = new EffectList();

	public Skills.SkillType m_levelUpOwnerSkill;

	public float m_levelUpFactor = 1f;

	public ItemDrop m_saddleItem;

	public Sadle m_saddle;

	public bool m_dropSaddleOnDeath = true;

	public Vector3 m_dropSaddleOffset = new Vector3(0f, 1f, 0f);

	public float m_dropItemVel = 5f;

	public List<string> m_randomStartingName = new List<string>();

	public float m_tamingSpeedMultiplierRange = 60f;

	public float m_tamingBoostMultiplier = 2f;

	public bool m_nameBeforeText = true;

	public string m_tameText = "$hud_tamelove";

	public TextGetter m_tameTextGetter;

	private Character m_character;

	private MonsterAI m_monsterAI;

	private Piece m_piece;

	private ZNetView m_nview;

	private float m_lastPetTime;

	private float m_unsummonTime;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_character = ((Component)this).GetComponent<Character>();
		m_monsterAI = ((Component)this).GetComponent<MonsterAI>();
		m_piece = ((Component)this).GetComponent<Piece>();
		if (Object.op_Implicit((Object)(object)m_character))
		{
			Character character = m_character;
			character.m_onDeath = (Action)Delegate.Combine(character.m_onDeath, new Action(OnDeath));
		}
		if (Object.op_Implicit((Object)(object)m_monsterAI))
		{
			MonsterAI monsterAI = m_monsterAI;
			monsterAI.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(monsterAI.m_onConsumedItem, new Action<ItemDrop>(OnConsumedItem));
		}
		if (m_nview.IsValid())
		{
			m_nview.Register<ZDOID, bool>("Command", RPC_Command);
			m_nview.Register<string, string>("SetName", RPC_SetName);
			m_nview.Register("RPC_UnSummon", RPC_UnSummon);
			if ((Object)(object)m_saddle != (Object)null)
			{
				m_nview.Register("AddSaddle", RPC_AddSaddle);
				m_nview.Register<bool>("SetSaddle", RPC_SetSaddle);
				SetSaddle(HaveSaddle());
			}
			((MonoBehaviour)this).InvokeRepeating("TamingUpdate", 3f, 3f);
		}
		if (m_startsTamed && Object.op_Implicit((Object)(object)m_character))
		{
			m_character.SetTamed(tamed: true);
		}
		if (m_randomStartingName.Count > 0 && m_nview.IsValid() && m_nview.GetZDO().GetString(ZDOVars.s_tamedName).Length == 0)
		{
			SetText(Localization.instance.Localize(m_randomStartingName[Random.Range(0, m_randomStartingName.Count)]));
		}
	}

	public void Update()
	{
		UpdateSummon();
		UpdateSavedFollowTarget();
	}

	public string GetHoverText()
	{
		if (!m_nview.IsValid())
		{
			return "";
		}
		string text = GetName();
		if (IsTamed())
		{
			if (Object.op_Implicit((Object)(object)m_character))
			{
				text += Localization.instance.Localize(" ( $hud_tame, " + GetStatusString() + " )");
			}
			text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
			if (ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive())
			{
				return text + Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltKeys + $KEY_Use</b></color>] $hud_rename");
			}
			return text + Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
		}
		int tameness = GetTameness();
		if (tameness <= 0)
		{
			return text + Localization.instance.Localize(" ( $hud_wild, " + GetStatusString() + " )");
		}
		return text + Localization.instance.Localize(" ( $hud_tameness  " + tameness + "%, " + GetStatusString() + " )");
	}

	public string GetStatusString()
	{
		if (Object.op_Implicit((Object)(object)m_monsterAI) && m_monsterAI.IsAlerted())
		{
			return "$hud_tamefrightened";
		}
		if (IsHungry())
		{
			return "$hud_tamehungry";
		}
		if (!Object.op_Implicit((Object)(object)m_character) || m_character.IsTamed())
		{
			return "$hud_tamehappy";
		}
		return "$hud_tameinprogress";
	}

	public bool IsTamed()
	{
		if (!Object.op_Implicit((Object)(object)m_character) || !m_character.IsTamed())
		{
			return m_startsTamed;
		}
		return true;
	}

	public string GetName()
	{
		return Localization.instance.Localize(Object.op_Implicit((Object)(object)m_character) ? m_character.m_name : ((Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid()) ? m_nview.GetZDO().GetString(ZDOVars.s_tamedName, m_piece.m_name) : m_piece.m_name));
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (hold)
		{
			return false;
		}
		if (alt)
		{
			SetName();
			return true;
		}
		string hoverName = GetHoverName();
		object msg;
		if (IsTamed())
		{
			if (Time.time - m_lastPetTime > 1f)
			{
				m_lastPetTime = Time.time;
				m_petEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				if (m_commandable)
				{
					Command(user);
					goto IL_00da;
				}
				if (m_tameTextGetter != null)
				{
					string text = m_tameTextGetter();
					if (text != null && text.Length > 0)
					{
						msg = text;
						goto IL_00d3;
					}
				}
				msg = (m_nameBeforeText ? (hoverName + " " + m_tameText) : m_tameText);
				goto IL_00d3;
			}
			return false;
		}
		return false;
		IL_00da:
		return true;
		IL_00d3:
		user.Message(MessageHud.MessageType.Center, (string)msg);
		goto IL_00da;
	}

	public string GetHoverName()
	{
		if (IsTamed())
		{
			string text = StringExtensionMethods.RemoveRichTextTags(GetText());
			if (text.Length > 0)
			{
				return text;
			}
			return GetName();
		}
		return GetName();
	}

	private void SetName()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!IsTamed())
		{
			return;
		}
		PrivilegeResult val = PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)2);
		if (!PrivilegeResultExtentions.IsGranted(val))
		{
			if (PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege != null)
			{
				PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege.Open((Privilege)2, (PrivilegeResult)64);
				if (!((UIController)PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege).IsOpen)
				{
					ZLog.LogError((object)string.Format("{0} can't resolve the {1} privilege on this platform, which was denied with result {2}. Tameable rename was blocked without meaningful feedback to the user!", "ResolvePrivilegeUI", (object)(Privilege)2, val));
				}
			}
			else
			{
				ZLog.LogError((object)string.Format("{0} is not available on this platform to resolve the {1} privilege, which was denied with result {2}. Tameable rename was blocked without meaningful feedback to the user!", "ResolvePrivilegeUI", (object)(Privilege)2, val));
			}
		}
		else
		{
			TextInput.instance.RequestText(this, "$hud_rename", 10);
		}
	}

	public string GetText()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return "";
		}
		string @string = m_nview.GetZDO().GetString(ZDOVars.s_tamedName);
		string resolvedAuthor = m_nview.GetZDO().GetString(ZDOVars.s_tamedNameAuthor, null);
		if (m_nview.IsOwner() && RelationsManager.UpdateAuthorIfHost(resolvedAuthor, ref resolvedAuthor))
		{
			m_nview.GetZDO().Set(ZDOVars.s_tamedNameAuthor, resolvedAuthor);
		}
		if (string.IsNullOrEmpty(resolvedAuthor) || resolvedAuthor == "host")
		{
			return CensorShittyWords.FilterUGC(@string, UGCType.Text, default(PlatformUserID), 0L);
		}
		return CensorShittyWords.FilterUGC(@string, UGCType.Text, new PlatformUserID(resolvedAuthor), 0L);
	}

	public void SetText(string text)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid())
		{
			PlatformUserID platformUserID = ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID;
			m_nview.InvokeRPC("SetName", text, PlatformManager.DistributionPlatform.LocalUser.IsSignedIn ? ((object)(PlatformUserID)(ref platformUserID)).ToString() : "host");
		}
	}

	private void RPC_SetName(long sender, string name, string authorId)
	{
		if (m_nview.IsValid() && m_nview.IsOwner() && IsTamed())
		{
			m_nview.GetZDO().Set(ZDOVars.s_tamedName, name);
			m_nview.GetZDO().Set(ZDOVars.s_tamedNameAuthor, authorId);
		}
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		if ((Object)(object)m_saddleItem != (Object)null && IsTamed() && item.m_shared.m_name == m_saddleItem.m_itemData.m_shared.m_name)
		{
			if (HaveSaddle())
			{
				user.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_saddle_already");
				return true;
			}
			m_nview.InvokeRPC("AddSaddle");
			user.GetInventory().RemoveOneItem(item);
			user.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_saddle_ready");
			return true;
		}
		return false;
	}

	private void RPC_AddSaddle(long sender)
	{
		if (m_nview.IsOwner() && !HaveSaddle())
		{
			m_nview.GetZDO().Set(ZDOVars.s_haveSaddleHash, value: true);
			m_nview.InvokeRPC(ZNetView.Everybody, "SetSaddle", true);
		}
	}

	public bool DropSaddle(Vector3 userPoint)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!HaveSaddle())
		{
			return false;
		}
		m_nview.GetZDO().Set(ZDOVars.s_haveSaddleHash, value: false);
		m_nview.InvokeRPC(ZNetView.Everybody, "SetSaddle", false);
		Vector3 flyDirection = userPoint - ((Component)this).transform.position;
		SpawnSaddle(flyDirection);
		return true;
	}

	private void SpawnSaddle(Vector3 flyDirection)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody component = Object.Instantiate<GameObject>(((Component)m_saddleItem).gameObject, ((Component)this).transform.TransformPoint(m_dropSaddleOffset), Quaternion.identity).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Vector3 val = Vector3.up;
			if (((Vector3)(ref flyDirection)).magnitude > 0.1f)
			{
				flyDirection.y = 0f;
				((Vector3)(ref flyDirection)).Normalize();
				val += flyDirection;
			}
			component.AddForce(val * m_dropItemVel, (ForceMode)2);
		}
	}

	private bool HaveSaddle()
	{
		if ((Object)(object)m_saddle == (Object)null)
		{
			return false;
		}
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.GetZDO().GetBool(ZDOVars.s_haveSaddleHash);
	}

	private void RPC_SetSaddle(long sender, bool enabled)
	{
		SetSaddle(enabled);
	}

	private void SetSaddle(bool enabled)
	{
		ZLog.Log((object)("Setting saddle:" + enabled));
		if ((Object)(object)m_saddle != (Object)null)
		{
			((Component)m_saddle).gameObject.SetActive(enabled);
		}
	}

	private void TamingUpdate()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner() && !IsTamed() && !IsHungry() && Object.op_Implicit((Object)(object)m_monsterAI) && !m_monsterAI.IsAlerted())
		{
			m_monsterAI.SetDespawnInDay(despawn: false);
			m_monsterAI.SetEventCreature(despawn: false);
			DecreaseRemainingTime(3f);
			if (GetRemainingTime() <= 0f)
			{
				Tame();
			}
			else
			{
				m_sootheEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
		}
	}

	private void Tame()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);
		if (m_nview.IsValid() && m_nview.IsOwner() && Object.op_Implicit((Object)(object)m_monsterAI) && Object.op_Implicit((Object)(object)m_character) && !IsTamed())
		{
			m_monsterAI.MakeTame();
			m_tamedEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 30f);
			if (Object.op_Implicit((Object)(object)closestPlayer))
			{
				closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
			}
		}
	}

	public static void TameAllInArea(Vector3 point, float radius)
	{
		foreach (Character allCharacter in Character.GetAllCharacters())
		{
			if (!allCharacter.IsPlayer())
			{
				Tameable component = ((Component)allCharacter).GetComponent<Tameable>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.Tame();
				}
			}
		}
	}

	public void Command(Humanoid user, bool message = true)
	{
		m_nview.InvokeRPC("Command", user.GetZDOID(), message);
	}

	private Player GetPlayer(ZDOID characterID)
	{
		GameObject val = ZNetScene.instance.FindInstance(characterID);
		if (Object.op_Implicit((Object)(object)val))
		{
			return val.GetComponent<Player>();
		}
		return null;
	}

	private void RPC_Command(long sender, ZDOID characterID, bool message)
	{
		Player player = GetPlayer(characterID);
		if ((Object)(object)player == (Object)null || !Object.op_Implicit((Object)(object)m_monsterAI))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)m_monsterAI.GetFollowTarget()))
		{
			m_monsterAI.SetFollowTarget(null);
			m_monsterAI.SetPatrolPoint();
			if (m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_follow, "");
			}
			if (message)
			{
				player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamestay");
			}
		}
		else
		{
			m_monsterAI.ResetPatrolPoint();
			m_monsterAI.SetFollowTarget(((Component)player).gameObject);
			if (m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_follow, player.GetPlayerName());
			}
			if (message)
			{
				player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamefollow");
			}
			int @int = m_nview.GetZDO().GetInt(ZDOVars.s_maxInstances);
			if (@int > 0)
			{
				UnsummonMaxInstances(@int);
			}
		}
		m_unsummonTime = 0f;
	}

	private void UpdateSavedFollowTarget()
	{
		if (!Object.op_Implicit((Object)(object)m_monsterAI) || (Object)(object)m_monsterAI.GetFollowTarget() != (Object)null || !m_nview.IsOwner())
		{
			return;
		}
		string @string = m_nview.GetZDO().GetString(ZDOVars.s_follow);
		if (string.IsNullOrEmpty(@string))
		{
			return;
		}
		foreach (Player allPlayer in Player.GetAllPlayers())
		{
			if (allPlayer.GetPlayerName() == @string)
			{
				Command(allPlayer, message: false);
				return;
			}
		}
		if (m_unsummonOnOwnerLogoutSeconds > 0f)
		{
			m_unsummonTime += Time.fixedDeltaTime;
			if (m_unsummonTime > m_unsummonOnOwnerLogoutSeconds)
			{
				UnSummon();
			}
		}
	}

	public bool IsHungry()
	{
		if (!Object.op_Implicit((Object)(object)m_character))
		{
			return false;
		}
		if ((Object)(object)m_nview == (Object)null)
		{
			return false;
		}
		ZDO zDO = m_nview.GetZDO();
		if (zDO == null)
		{
			return false;
		}
		DateTime dateTime = new DateTime(zDO.GetLong(ZDOVars.s_tameLastFeeding, 0L));
		return (ZNet.instance.GetTime() - dateTime).TotalSeconds > (double)m_fedDuration;
	}

	private void ResetFeedingTimer()
	{
		m_nview.GetZDO().Set(ZDOVars.s_tameLastFeeding, ZNet.instance.GetTime().Ticks);
	}

	private void OnDeath()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("Valid " + m_nview.IsValid()));
		ZLog.Log((object)("On death " + HaveSaddle()));
		if (HaveSaddle() && m_dropSaddleOnDeath)
		{
			ZLog.Log((object)"Spawning saddle ");
			SpawnSaddle(Vector3.zero);
		}
	}

	private int GetTameness()
	{
		float remainingTime = GetRemainingTime();
		return (int)((1f - Mathf.Clamp01(remainingTime / m_tamingTime)) * 100f);
	}

	private void OnConsumedItem(ItemDrop item)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (IsHungry())
		{
			m_sootheEffect.Create(Object.op_Implicit((Object)(object)m_character) ? m_character.GetCenterPoint() : ((Component)this).transform.position, Quaternion.identity);
		}
		ResetFeedingTimer();
	}

	private void DecreaseRemainingTime(float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		float remainingTime = GetRemainingTime();
		s_nearbyPlayers.Clear();
		Player.GetPlayersInRange(((Component)this).transform.position, m_tamingSpeedMultiplierRange, s_nearbyPlayers);
		foreach (Player s_nearbyPlayer in s_nearbyPlayers)
		{
			if (s_nearbyPlayer.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.TamingBoost))
			{
				time *= m_tamingBoostMultiplier;
			}
		}
		remainingTime -= time;
		if (remainingTime < 0f)
		{
			remainingTime = 0f;
		}
		m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, remainingTime);
	}

	private float GetRemainingTime()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_tameTimeLeft, m_tamingTime);
	}

	public bool HaveRider()
	{
		if (Object.op_Implicit((Object)(object)m_saddle))
		{
			return m_saddle.HaveValidUser();
		}
		return false;
	}

	public float GetRiderSkill()
	{
		if (Object.op_Implicit((Object)(object)m_saddle))
		{
			return m_saddle.GetRiderSkill();
		}
		return 0f;
	}

	private void UpdateSummon()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner() && m_unsummonDistance > 0f && Object.op_Implicit((Object)(object)m_monsterAI))
		{
			GameObject followTarget = m_monsterAI.GetFollowTarget();
			if (Object.op_Implicit((Object)(object)followTarget) && Vector3.Distance(followTarget.transform.position, ((Component)this).gameObject.transform.position) > m_unsummonDistance)
			{
				UnSummon();
			}
		}
	}

	private void UnsummonMaxInstances(int maxInstances)
	{
		if (!m_nview.IsValid() || !m_nview.IsOwner() || !Object.op_Implicit((Object)(object)m_character))
		{
			return;
		}
		GameObject followTarget = m_monsterAI.GetFollowTarget();
		object obj;
		if (followTarget != null)
		{
			Player component = followTarget.GetComponent<Player>();
			if (component != null)
			{
				obj = component.GetPlayerName();
				goto IL_004a;
			}
		}
		obj = null;
		goto IL_004a;
		IL_004a:
		string text = (string)obj;
		if (text == null)
		{
			return;
		}
		List<Character> allCharacters = Character.GetAllCharacters();
		List<BaseAI> list = new List<BaseAI>();
		foreach (Character item in allCharacters)
		{
			if (!(item.m_name == m_character.m_name))
			{
				continue;
			}
			ZNetView component2 = ((Component)item).GetComponent<ZNetView>();
			object obj2;
			if (component2 != null)
			{
				ZDO zDO = component2.GetZDO();
				if (zDO != null)
				{
					obj2 = zDO.GetString(ZDOVars.s_follow);
					goto IL_00b7;
				}
			}
			obj2 = "";
			goto IL_00b7;
			IL_00b7:
			if ((string?)obj2 == text)
			{
				MonsterAI component3 = ((Component)item).GetComponent<MonsterAI>();
				if (component3 != null)
				{
					list.Add(component3);
				}
			}
		}
		list.Sort((BaseAI a, BaseAI b) => b.GetTimeSinceSpawned().CompareTo(a.GetTimeSinceSpawned()));
		int num = list.Count - maxInstances;
		for (int i = 0; i < num; i++)
		{
			((Component)list[i]).GetComponent<Tameable>()?.UnSummon();
		}
		if (num > 0 && Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$hud_maxsummonsreached");
		}
	}

	private void UnSummon()
	{
		if (m_nview.IsValid())
		{
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UnSummon");
		}
	}

	private void RPC_UnSummon(long sender)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_unSummonEffect.Create(((Component)this).gameObject.transform.position, ((Component)this).gameObject.transform.rotation);
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			ZNetScene.instance.Destroy(((Component)this).gameObject);
		}
	}
}
