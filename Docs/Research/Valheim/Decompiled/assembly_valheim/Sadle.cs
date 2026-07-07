using UnityEngine;

public class Sadle : MonoBehaviour, Interactable, Hoverable, IDoodadController
{
	private enum Speed
	{
		Stop,
		Walk,
		Run,
		Turn,
		NoChange
	}

	public string m_hoverText = "";

	public float m_maxUseRange = 10f;

	public Transform m_attachPoint;

	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	public string m_attachAnimation = "attach_chair";

	public float m_maxStamina = 100f;

	public float m_runStaminaDrain = 10f;

	public float m_swimStaminaDrain = 10f;

	public float m_staminaRegen = 10f;

	public float m_staminaRegenHungry = 10f;

	public EffectList m_drownEffects = new EffectList();

	public Sprite m_mountIcon;

	private const float m_staminaRegenDelay = 1f;

	private Vector3 m_controlDir;

	private Speed m_speed;

	private float m_rideSkill;

	private float m_staminaRegenTimer;

	private float m_drownDamageTimer;

	private float m_raiseSkillTimer;

	private Character m_character;

	private ZNetView m_nview;

	private Tameable m_tambable;

	private MonsterAI m_monsterAI;

	private bool m_haveValidUser;

	private void Awake()
	{
		m_character = ((Component)this).gameObject.GetComponentInParent<Character>();
		m_nview = ((Component)m_character).GetComponent<ZNetView>();
		m_tambable = ((Component)m_character).GetComponent<Tameable>();
		m_monsterAI = ((Component)m_character).GetComponent<MonsterAI>();
		m_nview.Register<long>("RequestControl", RPC_RequestControl);
		m_nview.Register<long>("ReleaseControl", RPC_ReleaseControl);
		m_nview.Register<bool>("RequestRespons", RPC_RequestRespons);
		m_nview.Register<Vector3>("RemoveSaddle", RPC_RemoveSaddle);
		m_nview.Register<Vector3, int, float>("Controls", RPC_Controls);
	}

	public bool IsValid()
	{
		return Object.op_Implicit((Object)(object)this);
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void FixedUpdate()
	{
		if (!m_nview.IsValid())
		{
			return;
		}
		CalculateHaveValidUser();
		if (m_character.IsTamed())
		{
			if (IsLocalUser())
			{
				UpdateRidingSkill(Time.fixedDeltaTime);
			}
			if (m_nview.IsOwner())
			{
				float fixedDeltaTime = Time.fixedDeltaTime;
				UpdateStamina(fixedDeltaTime);
				UpdateDrown(fixedDeltaTime);
			}
		}
	}

	private void UpdateDrown(float dt)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (m_character.IsSwimming() && !m_character.IsOnGround() && !HaveStamina())
		{
			m_drownDamageTimer += dt;
			if (m_drownDamageTimer > 1f)
			{
				m_drownDamageTimer = 0f;
				float damage = Mathf.Ceil(m_character.GetMaxHealth() / 20f);
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = damage;
				hitData.m_point = m_character.GetCenterPoint();
				hitData.m_dir = Vector3.down;
				hitData.m_pushForce = 10f;
				hitData.m_hitType = HitData.HitType.Drowning;
				m_character.Damage(hitData);
				Vector3 position = ((Component)this).transform.position;
				position.y = m_character.GetLiquidLevel();
				m_drownEffects.Create(position, ((Component)this).transform.rotation);
			}
		}
	}

	public bool UpdateRiding(float dt)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).isActiveAndEnabled)
		{
			return false;
		}
		if (!m_character.IsTamed())
		{
			return false;
		}
		if (!HaveValidUser())
		{
			return false;
		}
		if (m_speed == Speed.Stop || ((Vector3)(ref m_controlDir)).magnitude == 0f)
		{
			return false;
		}
		if (m_speed == Speed.Walk || m_speed == Speed.Run)
		{
			if (m_speed == Speed.Run && !HaveStamina())
			{
				m_speed = Speed.Walk;
			}
			m_monsterAI.MoveTowards(m_controlDir, m_speed == Speed.Run);
			float riderSkill = GetRiderSkill();
			float num = Mathf.Lerp(1f, 0.5f, riderSkill);
			if (m_character.IsSwimming())
			{
				UseStamina(m_swimStaminaDrain * num * dt);
			}
			else if (m_speed == Speed.Run)
			{
				UseStamina(m_runStaminaDrain * num * dt);
			}
		}
		else if (m_speed == Speed.Turn)
		{
			m_monsterAI.StopMoving();
			m_character.SetRun(run: false);
			m_monsterAI.LookTowards(m_controlDir);
		}
		m_monsterAI.ResetRandomMovement();
		return true;
	}

	public string GetHoverText()
	{
		if (!InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=#888888>$piece_toofar</color>");
		}
		string text = Localization.instance.Localize(m_hoverText);
		text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
		if (ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive())
		{
			return text + Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltKeys + $KEY_Use</b></color>] $hud_saddle_remove");
		}
		return text + Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_saddle_remove");
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_hoverText);
	}

	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
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
		if (!m_character.IsTamed())
		{
			return false;
		}
		Player player = character as Player;
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (alt)
		{
			m_nview.InvokeRPC("RemoveSaddle", ((Component)character).transform.position);
			return true;
		}
		m_nview.InvokeRPC("RequestControl", player.GetZDOID().UserID);
		return false;
	}

	public Character GetCharacter()
	{
		return m_character;
	}

	public Tameable GetTameable()
	{
		return m_tambable;
	}

	public void ApplyControlls(Vector3 moveDir, Vector3 lookDir, bool run, bool autoRun, bool block)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.m_localPlayer == (Object)null))
		{
			float skillFactor = Player.m_localPlayer.GetSkills().GetSkillFactor(Skills.SkillType.Ride);
			Speed speed = Speed.NoChange;
			Vector3 val = Vector3.zero;
			if (block || (double)moveDir.z > 0.5 || run)
			{
				Vector3 val2 = lookDir;
				val2.y = 0f;
				((Vector3)(ref val2)).Normalize();
				val = val2;
			}
			if (run)
			{
				speed = Speed.Run;
			}
			else if ((double)moveDir.z > 0.5)
			{
				speed = Speed.Walk;
			}
			else if ((double)moveDir.z < -0.5)
			{
				speed = Speed.Stop;
			}
			else if (block)
			{
				speed = Speed.Turn;
			}
			m_nview.InvokeRPC("Controls", val, (int)speed, skillFactor);
		}
	}

	private void RPC_Controls(long sender, Vector3 rideDir, int rideSpeed, float skill)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			return;
		}
		m_rideSkill = skill;
		if (rideDir != Vector3.zero)
		{
			m_controlDir = rideDir;
		}
		switch (rideSpeed)
		{
		case 4:
			if (m_speed == Speed.Turn)
			{
				m_speed = Speed.Stop;
			}
			return;
		case 3:
			if (m_speed == Speed.Walk || m_speed == Speed.Run)
			{
				return;
			}
			break;
		}
		m_speed = (Speed)rideSpeed;
	}

	private void UpdateRidingSkill(float dt)
	{
		m_raiseSkillTimer += dt;
		if (m_raiseSkillTimer > 1f)
		{
			m_raiseSkillTimer = 0f;
			if (m_speed == Speed.Run)
			{
				Player.m_localPlayer.RaiseSkill(Skills.SkillType.Ride);
			}
		}
	}

	private void ResetControlls()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_controlDir = Vector3.zero;
		m_speed = Speed.Stop;
		m_rideSkill = 0f;
	}

	public Component GetControlledComponent()
	{
		return (Component)(object)m_character;
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	private void RPC_RemoveSaddle(long sender, Vector3 userPoint)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && !HaveValidUser())
		{
			m_tambable.DropSaddle(userPoint);
		}
	}

	private void RPC_RequestControl(long sender, long playerID)
	{
		if (m_nview.IsOwner())
		{
			CalculateHaveValidUser();
			if (GetUser() == playerID || !HaveValidUser())
			{
				m_nview.GetZDO().Set(ZDOVars.s_user, playerID);
				ResetControlls();
				m_nview.InvokeRPC(sender, "RequestRespons", true);
				m_nview.GetZDO().SetOwner(sender);
			}
			else
			{
				m_nview.InvokeRPC(sender, "RequestRespons", false);
			}
		}
	}

	public bool HaveValidUser()
	{
		return m_haveValidUser;
	}

	private void CalculateHaveValidUser()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		m_haveValidUser = false;
		long user = GetUser();
		if (user == 0L)
		{
			return;
		}
		foreach (ZDO allCharacterZDO in ZNet.instance.GetAllCharacterZDOS())
		{
			if (allCharacterZDO.m_uid.UserID == user)
			{
				m_haveValidUser = Vector3.Distance(allCharacterZDO.GetPosition(), ((Component)this).transform.position) < m_maxUseRange;
				break;
			}
		}
	}

	private void RPC_ReleaseControl(long sender, long playerID)
	{
		if (m_nview.IsOwner() && GetUser() == playerID)
		{
			m_nview.GetZDO().Set(ZDOVars.s_user, 0L);
			ResetControlls();
		}
	}

	private void RPC_RequestRespons(long sender, bool granted)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			return;
		}
		if (granted)
		{
			Player.m_localPlayer.StartDoodadControl(this);
			if ((Object)(object)m_attachPoint != (Object)null)
			{
				Player.m_localPlayer.AttachStart(m_attachPoint, ((Component)m_character).gameObject, hideWeapons: false, isBed: false, onShip: false, m_attachAnimation, m_detachOffset);
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
			m_nview.InvokeRPC("ReleaseControl", player.GetZDOID().UserID);
			if ((Object)(object)m_attachPoint != (Object)null)
			{
				player.AttachStop();
			}
		}
	}

	private bool IsLocalUser()
	{
		if (!Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			return false;
		}
		long user = GetUser();
		if (user == 0L)
		{
			return false;
		}
		return user == Player.m_localPlayer.GetZDOID().UserID;
	}

	private long GetUser()
	{
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid())
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

	private void UseStamina(float v)
	{
		if (v != 0f && m_nview.IsValid() && m_nview.IsOwner())
		{
			float stamina = GetStamina();
			stamina -= v;
			if (stamina < 0f)
			{
				stamina = 0f;
			}
			SetStamina(stamina);
			m_staminaRegenTimer = 1f;
		}
	}

	private bool HaveStamina(float amount = 0f)
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return GetStamina() > amount;
	}

	public float GetStamina()
	{
		if ((Object)(object)m_nview == (Object)null)
		{
			return 0f;
		}
		if (m_nview.GetZDO() == null)
		{
			return 0f;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_stamina, GetMaxStamina());
	}

	private void SetStamina(float stamina)
	{
		m_nview.GetZDO().Set(ZDOVars.s_stamina, stamina);
	}

	public float GetMaxStamina()
	{
		return m_maxStamina;
	}

	private void UpdateStamina(float dt)
	{
		m_staminaRegenTimer -= dt;
		if (m_staminaRegenTimer > 0f || m_character.InAttack() || m_character.IsSwimming())
		{
			return;
		}
		float stamina = GetStamina();
		float maxStamina = GetMaxStamina();
		if (stamina < maxStamina || stamina > maxStamina)
		{
			float num = (m_tambable.IsHungry() ? m_staminaRegenHungry : m_staminaRegen);
			float num2 = num + (1f - stamina / maxStamina) * num;
			stamina += num2 * dt;
			if (stamina > maxStamina)
			{
				stamina = maxStamina;
			}
			SetStamina(stamina);
		}
	}

	public float GetRiderSkill()
	{
		return m_rideSkill;
	}
}
