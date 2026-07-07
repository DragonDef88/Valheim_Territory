using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SoftReferenceableAssets;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : Humanoid
{
	public enum RequirementMode
	{
		CanBuild,
		IsKnown,
		CanAlmostBuild
	}

	public class Food
	{
		public string m_name = "";

		public ItemDrop.ItemData m_item;

		public float m_time;

		public float m_health;

		public float m_stamina;

		public float m_eitr;

		public bool CanEatAgain()
		{
			return m_time < m_item.m_shared.m_foodBurnTime / 2f;
		}
	}

	public class MinorActionData
	{
		public enum ActionType
		{
			Equip,
			Unequip,
			Reload
		}

		public ActionType m_type;

		public ItemDrop.ItemData m_item;

		public string m_progressText = "";

		public float m_time;

		public float m_duration;

		public string m_animation = "";

		public string m_doneAnimation = "";

		public float m_staminaDrain;

		public float m_eitrDrain;

		public EffectList m_startEffect;
	}

	public enum PlacementStatus
	{
		Valid,
		Invalid,
		BlockedbyPlayer,
		NoBuildZone,
		PrivateZone,
		MoreSpace,
		NoTeleportArea,
		ExtensionMissingStation,
		WrongBiome,
		NeedCultivated,
		NeedDirt,
		NotInDungeon,
		NoRayHits
	}

	private class RaycastHitComparer : IComparer<RaycastHit>
	{
		public static RaycastHitComparer Instance = new RaycastHitComparer();

		public int Compare(RaycastHit x, RaycastHit y)
		{
			return ((RaycastHit)(ref x)).distance.CompareTo(((RaycastHit)(ref y)).distance);
		}
	}

	[Serializable]
	public struct StatusEffectLevel
	{
		public float m_rate;

		public StatusEffect m_se;
	}

	private Vector3 m_lastDistCheck;

	private float m_statCheck;

	[Header("Effects")]
	public EffectList m_buttonEffects = new EffectList();

	private List<string> m_readyEvents = new List<string>();

	private float m_lastMaxAdrenaline;

	private static List<IPlaced> m_placed = new List<IPlaced>();

	public static string LastEmote;

	public static DateTime LastEmoteTime;

	private float[] m_equipmentModifierValues;

	private static FieldInfo[] s_equipmentModifierSourceFields;

	private static readonly string[] s_equipmentModifierSources = new string[11]
	{
		"m_movementModifier", "m_homeItemsStaminaModifier", "m_heatResistanceModifier", "m_jumpStaminaModifier", "m_attackStaminaModifier", "m_blockStaminaModifier", "m_dodgeStaminaModifier", "m_swimStaminaModifier", "m_sneakStaminaModifier", "m_runStaminaModifier",
		"m_maxAdrenaline"
	};

	private static readonly string[] s_equipmentModifierTooltips = new string[11]
	{
		"$item_movement_modifier", "$base_item_modifier", "$item_heat_modifier", "$se_jumpstamina", "$se_attackstamina", "$se_blockstamina", "$se_dodgestamina", "$se_swimstamina", "$se_sneakstamina", "$se_runstamina",
		"$item_maxadrenaline"
	};

	private float m_baseValueUpdateTimer;

	private float m_rotatePieceTimer;

	private float m_rotatePieceTimeSince;

	private float m_scrollCurrAmount;

	private bool m_altPlace;

	public static Player m_localPlayer = null;

	private static readonly List<Player> s_players = new List<Player>();

	public static List<string> m_addUniqueKeyQueue = new List<string>();

	public static List<string> s_FilterCraft = new List<string>();

	public static bool m_debugMode = false;

	[Header("Player")]
	public float m_maxPlaceDistance = 5f;

	public float m_maxInteractDistance = 5f;

	public float m_scrollSens = 4f;

	public float m_autoPickupRange = 2f;

	public float m_maxCarryWeight = 300f;

	public float m_encumberedStaminaDrain = 10f;

	public float m_hardDeathCooldown = 10f;

	public float m_baseCameraShake = 4f;

	public float m_placeDelay = 0.4f;

	public float m_removeDelay = 0.25f;

	public GameObject m_placeMarker;

	public GameObject m_tombstone;

	public SoftReference<GameObject> m_valkyrie;

	public Sprite m_textIcon;

	public float m_baseHP = 25f;

	public float m_baseStamina = 75f;

	public double m_wakeupTime;

	[Header("Stamina")]
	public float m_staminaRegen = 5f;

	public float m_staminaRegenTimeMultiplier = 1f;

	public float m_staminaRegenDelay = 1f;

	public float m_runStaminaDrain = 10f;

	public float m_sneakStaminaDrain = 5f;

	public float m_swimStaminaDrainMinSkill = 5f;

	public float m_swimStaminaDrainMaxSkill = 2f;

	public float m_dodgeStaminaUsage = 10f;

	public float m_dodgeAdrenaline = 10f;

	public float m_weightStaminaFactor = 0.1f;

	[Header("Adrenaline")]
	public float m_maxAdrenaline = 100f;

	public AnimationCurve m_adrenalineDegen = new AnimationCurve();

	public AnimationCurve m_adrenalineDegenDelay = new AnimationCurve();

	public float m_perfectDodgeAdrenaline = 10f;

	public float m_perfectDodgeStaminaReturnMultiplier;

	public float m_attackMissAdrenaline = -5f;

	public float m_nonBlockDamageAdrenaline = -5f;

	public float m_staggerEnemyAdrenaline = 5f;

	public AnimationCurve m_adrenalineGainMultiplier = new AnimationCurve();

	public List<StatusEffectLevel> m_adrenalineEffects = new List<StatusEffectLevel>();

	public EffectList m_adrenalinePopEffects = new EffectList();

	[Header("Eitr")]
	public float m_eiterRegen = 5f;

	public float m_eitrRegenDelay = 1f;

	[Header("Player Effects")]
	public EffectList m_drownEffects = new EffectList();

	public EffectList m_spawnEffects = new EffectList();

	public EffectList m_removeEffects = new EffectList();

	public EffectList m_dodgeEffects = new EffectList();

	public EffectList m_autopickupEffects = new EffectList();

	public EffectList m_skillLevelupEffects = new EffectList();

	public EffectList m_equipStartEffects = new EffectList();

	public EffectList m_perfectDodgeEffects = new EffectList();

	private Skills m_skills;

	private PieceTable m_buildPieces;

	private bool m_noPlacementCost;

	private const bool m_hideUnavailable = false;

	private static bool m_enableAutoPickup = true;

	private readonly HashSet<string> m_knownRecipes = new HashSet<string>();

	private readonly Dictionary<string, int> m_knownStations = new Dictionary<string, int>();

	private readonly HashSet<string> m_knownMaterial = new HashSet<string>();

	private readonly HashSet<string> m_shownTutorials = new HashSet<string>();

	private readonly HashSet<string> m_uniques = new HashSet<string>();

	private readonly HashSet<string> m_trophies = new HashSet<string>();

	private readonly HashSet<Heightmap.Biome> m_knownBiome = new HashSet<Heightmap.Biome>();

	private readonly Dictionary<string, string> m_knownTexts = new Dictionary<string, string>();

	private float m_stationDiscoverTimer;

	private bool m_debugFly;

	private bool m_godMode;

	private bool m_ghostMode;

	private float m_lookPitch;

	private const int m_maxFoods = 3;

	private const float m_foodDrainPerSec = 0.1f;

	private float m_foodUpdateTimer;

	private float m_foodRegenTimer;

	private readonly List<Food> m_foods = new List<Food>();

	private float m_stamina = 100f;

	private float m_maxStamina = 100f;

	private float m_staminaRegenTimer;

	private float m_adrenaline;

	private float m_adrenalineDegenTimer;

	private float m_adrenalineGuardianPower = 10f;

	private float m_eitr;

	private float m_maxEitr;

	private float m_eitrRegenTimer;

	private string m_guardianPower = "";

	private int m_guardianPowerHash;

	public float m_guardianPowerCooldown;

	private StatusEffect m_guardianSE;

	private float m_placePressedTime = -1000f;

	private float m_removePressedTime = -1000f;

	private bool m_blockRemove;

	private float m_lastToolUseTime;

	private GameObject m_placementMarkerInstance;

	private GameObject m_placementGhost;

	private string m_placementGhostLast;

	private PlacementStatus m_placementStatus = PlacementStatus.Invalid;

	private float m_placeRotationDegrees = 22.5f;

	private int m_placeRotation;

	public float m_scrollAmountThreshold = 0.1f;

	private int m_buildRemoveDebt;

	private int m_placeRayMask;

	private int m_placeGroundRayMask;

	private int m_placeWaterRayMask;

	private int m_removeRayMask;

	private int m_interactMask;

	private int m_autoPickupMask;

	private readonly List<MinorActionData> m_actionQueue = new List<MinorActionData>();

	private float m_actionQueuePause;

	private string m_actionAnimation;

	private GameObject m_hovering;

	private Character m_hoveringCreature;

	private float m_lastHoverInteractTime;

	private bool m_pvp;

	private float m_updateCoverTimer;

	private float m_coverPercentage;

	private bool m_underRoof = true;

	private float m_nearFireTimer;

	private bool m_isLoading;

	private ItemDrop.ItemData m_weaponLoaded;

	private float m_queuedAttackTimer;

	private float m_queuedSecondAttackTimer;

	private float m_queuedDodgeTimer;

	private Vector3 m_queuedDodgeDir = Vector3.zero;

	private bool m_inDodge;

	private bool m_dodgeInvincible;

	private CraftingStation m_currentStation;

	private bool m_inCraftingStation;

	private Ragdoll m_ragdoll;

	private Piece m_hoveringPiece;

	private Dictionary<Material, float> m_ghostRippleDistance = new Dictionary<Material, float>();

	private bool m_attackTowardsPlayerLookDir;

	private string m_emoteState = "";

	private int m_emoteID;

	private bool m_intro;

	private bool m_crouchToggled;

	public bool m_autoRun;

	private bool m_safeInHome;

	private IDoodadController m_doodadController;

	private bool m_attached;

	private string m_attachAnimation = "";

	private bool m_sleeping;

	private bool m_attachedToShip;

	private Transform m_attachPoint;

	private Vector3 m_detachOffset = Vector3.zero;

	private Transform m_attachPointCamera;

	private Collider[] m_attachColliders;

	private int m_modelIndex;

	private Vector3 m_skinColor = Vector3.one;

	private Vector3 m_hairColor = Vector3.one;

	private bool m_teleporting;

	private bool m_distantTeleport;

	private float m_teleportTimer;

	private float m_teleportCooldown;

	private Vector3 m_teleportFromPos;

	private Quaternion m_teleportFromRot;

	private Vector3 m_teleportTargetPos;

	private Quaternion m_teleportTargetRot;

	private bool m_beenHitWhileDodging;

	private Heightmap.Biome m_currentBiome;

	private float m_biomeTimer;

	private List<string> m_tempUniqueKeys = new List<string>();

	private int m_baseValue;

	private int m_baseValueOld = -1;

	private int m_comfortLevel;

	private float m_drownDamageTimer;

	private float m_timeSinceTargeted;

	private float m_timeSinceSensed;

	private float m_stealthFactorUpdateTimer;

	private float m_stealthFactor;

	private float m_stealthFactorTarget;

	private Vector3 m_lastStealthPosition = Vector3.zero;

	private float m_lastVelocity;

	private float m_wakeupTimer = -1f;

	private float m_timeSinceDeath = 999999f;

	private float m_runSkillImproveTimer;

	private float m_swimSkillImproveTimer;

	private float m_sneakSkillImproveTimer;

	private int m_manualSnapPoint = -1;

	private readonly List<PieceTable> m_tempOwnedPieceTables = new List<PieceTable>();

	private readonly List<Transform> m_tempSnapPoints1 = new List<Transform>();

	private readonly List<Transform> m_tempSnapPoints2 = new List<Transform>();

	private readonly List<Piece> m_tempPieces = new List<Piece>();

	[HideInInspector]
	public Dictionary<string, string> m_customData = new Dictionary<string, string>();

	private static int s_attackMask = 0;

	private static readonly int s_crouching = ZSyncAnimation.GetHash("crouching");

	private static readonly int s_animatorTagDodge = ZSyncAnimation.GetHash("dodge");

	private static readonly int s_animatorTagCutscene = ZSyncAnimation.GetHash("cutscene");

	private static readonly int s_animatorTagCrouch = ZSyncAnimation.GetHash("crouch");

	private static readonly int s_animatorTagMinorAction = ZSyncAnimation.GetHash("minoraction");

	private static readonly int s_animatorTagMinorActionFast = ZSyncAnimation.GetHash("minoraction_fast");

	private static readonly int s_animatorTagEmote = ZSyncAnimation.GetHash("emote");

	public const string BaseValueKey = "baseValue";

	private int m_cachedFrame;

	private bool m_cachedAttack;

	private bool m_dodgeInvincibleCached;

	[Header("Seasonal Items")]
	[SerializeField]
	private List<SeasonalItemGroup> m_seasonalItemGroups = new List<SeasonalItemGroup>();

	private SeasonalItemGroup m_currentSeason;

	private readonly RaycastHit[] m_raycastHoverHits = (RaycastHit[])(object)new RaycastHit[64];

	public bool AttackTowardsPlayerLookDir
	{
		get
		{
			return m_attackTowardsPlayerLookDir;
		}
		set
		{
			m_attackTowardsPlayerLookDir = value;
		}
	}

	public bool AlternativePlacementActive => m_altPlace;

	public SeasonalItemGroup CurrentSeason => m_currentSeason;

	protected override void Awake()
	{
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		s_players.Add(this);
		m_skills = ((Component)this).GetComponent<Skills>();
		SetupAwake();
		m_equipmentModifierValues = new float[s_equipmentModifierSources.Length];
		if (s_equipmentModifierSourceFields == null)
		{
			s_equipmentModifierSourceFields = new FieldInfo[s_equipmentModifierSources.Length];
			for (int i = 0; i < s_equipmentModifierSources.Length; i++)
			{
				s_equipmentModifierSourceFields[i] = typeof(ItemDrop.ItemData.SharedData).GetField(s_equipmentModifierSources[i], BindingFlags.Instance | BindingFlags.Public);
			}
			if (s_equipmentModifierSources.Length != s_equipmentModifierTooltips.Length)
			{
				ZLog.LogError((object)"Equipment modifier tooltip missmatch in player!");
			}
		}
		if (m_nview.GetZDO() == null)
		{
			return;
		}
		m_placeRayMask = LayerMask.GetMask(new string[7] { "Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "vehicle" });
		m_placeWaterRayMask = LayerMask.GetMask(new string[8] { "Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "Water", "vehicle" });
		m_removeRayMask = LayerMask.GetMask(new string[7] { "Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "vehicle" });
		m_interactMask = LayerMask.GetMask(new string[10] { "item", "piece", "piece_nonsolid", "Default", "static_solid", "Default_small", "character", "character_net", "terrain", "vehicle" });
		m_autoPickupMask = LayerMask.GetMask(new string[1] { "item" });
		Inventory inventory = m_inventory;
		inventory.m_onChanged = (Action)Delegate.Combine(inventory.m_onChanged, new Action(OnInventoryChanged));
		if (s_attackMask == 0)
		{
			s_attackMask = LayerMask.GetMask(new string[12]
			{
				"Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "character", "character_net", "character_ghost", "hitbox",
				"character_noenv", "vehicle"
			});
		}
		m_nview.Register("OnDeath", RPC_OnDeath);
		m_nview.Register("RPC_HitWhileDodging", RPC_HitWhileDodging);
		if (m_nview.IsOwner())
		{
			m_nview.Register<int, string, int>("Message", RPC_Message);
			m_nview.Register<bool, bool>("OnTargeted", RPC_OnTargeted);
			m_nview.Register<float>("UseStamina", RPC_UseStamina);
			if (Object.op_Implicit((Object)(object)MusicMan.instance))
			{
				MusicMan.instance.TriggerMusic("Wakeup");
			}
			UpdateKnownRecipesList();
			UpdateAvailablePiecesList();
			SetupPlacementGhost();
			m_dodgeInvincibleCached = m_nview.GetZDO().GetBool(ZDOVars.s_dodgeinv);
		}
		m_placeRotation = Random.Range(0, 16);
		float num = Random.Range(0f, (float)Math.PI * 2f);
		SetLookDir(new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num)));
		FaceLookDirection();
		AddQueuedKeys();
		UpdateCurrentSeason();
		m_attackTowardsPlayerLookDir = PlatformPrefs.GetInt("AttackTowardsPlayerLookDir", 1) == 1;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	public void SetLocalPlayer()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_localPlayer == (Object)(object)this))
		{
			m_localPlayer = this;
			Game.instance.IncrementPlayerStat(PlayerStatType.WorldLoads);
			ZNet.instance.SetReferencePosition(((Component)this).transform.position);
			EnvMan.instance.SetForceEnvironment("");
			AddQueuedKeys();
		}
	}

	private void AddQueuedKeys()
	{
		if (m_addUniqueKeyQueue.Count <= 0)
		{
			return;
		}
		foreach (string item in m_addUniqueKeyQueue)
		{
			AddUniqueKey(item);
		}
		m_addUniqueKeyQueue.Clear();
	}

	public void SetPlayerID(long playerID, string name)
	{
		if (m_nview.GetZDO() != null && GetPlayerID() == 0L)
		{
			m_nview.GetZDO().Set(ZDOVars.s_playerID, playerID);
			m_nview.GetZDO().Set(ZDOVars.s_playerName, name);
		}
	}

	public long GetPlayerID()
	{
		if (!m_nview.IsValid())
		{
			return 0L;
		}
		return m_nview.GetZDO().GetLong(ZDOVars.s_playerID, 0L);
	}

	public string GetPlayerName()
	{
		if (!m_nview.IsValid())
		{
			return "";
		}
		return m_nview.GetZDO().GetString(ZDOVars.s_playerName, "...");
	}

	public override string GetHoverText()
	{
		return "";
	}

	public override string GetHoverName()
	{
		return CensorShittyWords.FilterUGC(GetPlayerName(), UGCType.CharacterName, GetPlayerID());
	}

	protected override void Start()
	{
		base.Start();
		InvalidateCachedLiquidDepth();
	}

	protected override void OnDestroy()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null && (Object)(object)ZNet.instance != (Object)null)
		{
			string[] obj = new string[8] { "Player destroyed sec:", null, null, null, null, null, null, null };
			Vector2i sector = zDO.GetSector();
			obj[1] = ((object)(Vector2i)(ref sector)).ToString();
			obj[2] = "  pos:";
			Vector3 val = ((Component)this).transform.position;
			obj[3] = ((object)(Vector3)(ref val)).ToString();
			obj[4] = "  zdopos:";
			val = zDO.GetPosition();
			obj[5] = ((object)(Vector3)(ref val)).ToString();
			obj[6] = "  ref ";
			val = ZNet.instance.GetReferencePosition();
			obj[7] = ((object)(Vector3)(ref val)).ToString();
			ZLog.LogWarning((object)string.Concat(obj));
		}
		if (Object.op_Implicit((Object)(object)m_placementGhost))
		{
			Object.Destroy((Object)(object)m_placementGhost);
			m_placementGhost = null;
		}
		base.OnDestroy();
		s_players.Remove(this);
		if ((Object)(object)m_localPlayer == (Object)(object)this)
		{
			ZLog.LogWarning((object)"Local player destroyed");
			m_localPlayer = null;
		}
	}

	private void FixedUpdate()
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		float fixedDeltaTime = Time.fixedDeltaTime;
		UpdateAwake(fixedDeltaTime);
		if (m_nview.GetZDO() == null)
		{
			return;
		}
		UpdateTargeted(fixedDeltaTime);
		if (!m_nview.IsOwner())
		{
			return;
		}
		if ((Object)(object)m_localPlayer != (Object)(object)this)
		{
			ZLog.Log((object)"Destroying old local player");
			ZNetScene.instance.Destroy(((Component)this).gameObject);
		}
		else if (!IsDead())
		{
			UpdateActionQueue(fixedDeltaTime);
			PlayerAttackInput(fixedDeltaTime);
			UpdateAttach();
			UpdateDoodadControls(fixedDeltaTime);
			UpdateCrouch(fixedDeltaTime);
			UpdateDodge(fixedDeltaTime);
			UpdateCover(fixedDeltaTime);
			UpdateStations(fixedDeltaTime);
			UpdateGuardianPower(fixedDeltaTime);
			UpdateBaseValue(fixedDeltaTime);
			UpdateStats(fixedDeltaTime);
			UpdateTeleport(fixedDeltaTime);
			AutoPickup(fixedDeltaTime);
			EdgeOfWorldKill(fixedDeltaTime);
			UpdateBiome(fixedDeltaTime);
			UpdateStealth(fixedDeltaTime);
			if (Object.op_Implicit((Object)(object)GameCamera.instance) && (Object)(object)m_attachPointCamera == (Object)null && Vector3.Distance(((Component)GameCamera.instance).transform.position, ((Component)this).transform.position) < 2f)
			{
				SetVisible(visible: false);
			}
			AudioMan.instance.SetIndoor(InShelter() || ShieldGenerator.IsInsideShield(((Component)this).transform.position));
		}
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = InventoryGui.IsVisible();
		if ((int)ZInput.InputLayout != 0 && ZInput.IsGamepadActive() && !flag && ZInput.GetButtonUp("JoyAltPlace") && ZInput.GetButton("JoyAltKeys"))
		{
			m_altPlace = !m_altPlace;
			if ((Object)(object)MessageHud.instance != (Object)null)
			{
				string text = Localization.instance.Localize("$hud_altplacement");
				string text2 = (m_altPlace ? Localization.instance.Localize("$hud_on") : Localization.instance.Localize("$hud_off"));
				MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, text + " " + text2);
			}
		}
		UpdateClothFix();
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		bool flag2 = TakeInput();
		UpdateHover();
		bool num;
		if (flag2)
		{
			if (m_debugMode && Console.instance.IsCheatsEnabled())
			{
				if (ZInput.GetKeyDown((KeyCode)122, true))
				{
					ToggleDebugFly();
				}
				if (ZInput.GetKeyDown((KeyCode)98, true))
				{
					ToggleNoPlacementCost();
				}
				if (ZInput.GetKeyDown((KeyCode)107, true))
				{
					Console.instance.TryRunCommand("killenemies");
				}
				if (ZInput.GetKeyDown((KeyCode)108, true))
				{
					Console.instance.TryRunCommand("removedrops");
				}
			}
			bool alt = ((ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive()) ? ZInput.GetButton("JoyAltKeys") : (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltPlace")));
			if ((ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("JoyUse")) && !Hud.InRadial())
			{
				if (Object.op_Implicit((Object)(object)m_hovering))
				{
					Interact(m_hovering, hold: false, alt);
				}
				else if (m_doodadController != null)
				{
					StopDoodadControl();
				}
			}
			else if ((ZInput.GetButton("Use") || ZInput.GetButton("JoyUse")) && !Hud.InRadial() && Object.op_Implicit((Object)(object)m_hovering))
			{
				Interact(m_hovering, hold: true, alt);
			}
			bool flag3 = !Hud.InRadial() && ZInput.GetButtonUp("JoyHide") && ZInput.GetButtonLastPressedTimer("JoyHide") < 0.33f;
			if ((int)ZInput.InputLayout == 0 || !ZInput.IsGamepadActive())
			{
				if (ZInput.GetButtonDown("Hide"))
				{
					goto IL_028b;
				}
				if (flag3 && !ZInput.GetButton("JoyAltKeys"))
				{
					num = !InPlaceMode();
					goto IL_0289;
				}
			}
			else if (!InPlaceMode() && flag3)
			{
				num = !ZInput.GetButton("JoyAltKeys");
				goto IL_0289;
			}
			goto IL_02d6;
		}
		goto IL_0496;
		IL_028b:
		if (GetRightItem() != null || GetLeftItem() != null)
		{
			if (!InAttack() && !InDodge())
			{
				HideHandItems();
			}
		}
		else if ((!IsSwimming() || IsOnGround()) && !InDodge())
		{
			ShowHandItems();
		}
		goto IL_02d6;
		IL_0289:
		if (num)
		{
			goto IL_028b;
		}
		goto IL_02d6;
		IL_02d6:
		if (ZInput.GetButtonDown("ToggleWalk") && !Hud.InRadial())
		{
			SetWalk(!GetWalk());
			if (GetWalk())
			{
				Message(MessageHud.MessageType.TopLeft, "$msg_walk $hud_on");
			}
			else
			{
				Message(MessageHud.MessageType.TopLeft, "$msg_walk $hud_off");
			}
		}
		HandleRadialInput();
		bool flag4 = ZInput.IsGamepadActive() && !ZInput.GetButton("JoyAltKeys");
		bool flag5 = (int)ZInput.InputLayout == 0 && ZInput.GetButtonDown("JoyGP");
		bool flag6 = ZInput.IsNonClassicFunctionality() && ZInput.GetButton("JoyLStick") && ZInput.GetButton("JoyRStick");
		if (!Hud.InRadial() && !Hud.IsPieceSelectionVisible() && (ZInput.GetButtonDown("GP") || (flag4 && (flag5 || flag6))))
		{
			StartGuardianPower();
		}
		bool flag7 = ZInput.GetButtonDown("JoyAutoPickup") && ZInput.GetButton("JoyAltKeys");
		if (ZInput.GetButtonDown("AutoPickup") || flag7)
		{
			m_enableAutoPickup = !m_enableAutoPickup;
			Message(MessageHud.MessageType.TopLeft, "$hud_autopickup:" + (m_enableAutoPickup ? "$hud_on" : "$hud_off"));
		}
		if (ZInput.GetButtonDown("Hotbar1"))
		{
			UseHotbarItem(1);
		}
		if (ZInput.GetButtonDown("Hotbar2"))
		{
			UseHotbarItem(2);
		}
		if (ZInput.GetButtonDown("Hotbar3"))
		{
			UseHotbarItem(3);
		}
		if (ZInput.GetButtonDown("Hotbar4"))
		{
			UseHotbarItem(4);
		}
		if (ZInput.GetButtonDown("Hotbar5"))
		{
			UseHotbarItem(5);
		}
		if (ZInput.GetButtonDown("Hotbar6"))
		{
			UseHotbarItem(6);
		}
		if (ZInput.GetButtonDown("Hotbar7"))
		{
			UseHotbarItem(7);
		}
		if (ZInput.GetButtonDown("Hotbar8"))
		{
			UseHotbarItem(8);
		}
		goto IL_0496;
		IL_0496:
		UpdatePlacement(flag2, Time.deltaTime);
		UpdateStats();
	}

	private void UpdateClothFix()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Vector3 velocity = GetVelocity();
		float magnitude = ((Vector3)(ref velocity)).magnitude;
		if (magnitude > 0.01f && m_lastVelocity < 0.01f)
		{
			ResetCloth();
			Terminal.Increment("resetcloth");
		}
		m_lastVelocity = magnitude;
	}

	private bool CheckKeyboardRadialPressed()
	{
		return ZInput.GetButtonDown("OpenEmote");
	}

	private bool CheckGamepadRadialBuildPressed()
	{
		return false;
	}

	private void HandleRadialInput()
	{
		if (Hud.InRadial())
		{
			return;
		}
		if (!Hud.instance.m_radialMenu.CanOpen)
		{
			Hud.instance.m_radialMenu.CanOpen = ZInput.GetButtonDown("JoyRadial") || CheckGamepadRadialBuildPressed() || CheckKeyboardRadialPressed();
		}
		bool flag = false;
		bool flag2 = ZInput.GetButtonPressedTimer("JoyRadial") > 0.33f;
		if ((!ZInput.GetButton("JoyAltKeys") && flag) || (!ZInput.GetButton("JoyAltKeys") && flag2 && !InPlaceMode()) || CheckKeyboardRadialPressed())
		{
			Hud.instance.m_radialMenu.Open(Hud.instance.m_config);
		}
		else if ((!InPlaceMode() && !ZInput.GetButton("JoyAltKeys") && ZInput.GetButtonUp("JoySit")) || ZInput.GetButtonDown("Sit"))
		{
			if (InEmote() && IsSitting())
			{
				StopEmote();
			}
			else
			{
				StartEmote("sit", oneshot: false);
			}
		}
	}

	private void UpdateStats()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (IsDebugFlying())
		{
			return;
		}
		m_statCheck += Time.deltaTime;
		if (m_statCheck < 0.5f)
		{
			return;
		}
		m_statCheck = 0f;
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		playerProfile.IncrementStat(IsSafeInHome() ? PlayerStatType.TimeInBase : PlayerStatType.TimeOutOfBase, 0.5f);
		float num = Vector3.Distance(((Component)this).transform.position, m_lastDistCheck);
		if (!(num > 1f))
		{
			return;
		}
		if (num < 20f)
		{
			playerProfile.IncrementStat(PlayerStatType.DistanceTraveled, num);
			if ((Object)(object)Ship.GetLocalShip() != (Object)null)
			{
				playerProfile.IncrementStat(PlayerStatType.DistanceSail);
			}
			else if (IsOnGround())
			{
				playerProfile.IncrementStat(IsRunning() ? PlayerStatType.DistanceRun : PlayerStatType.DistanceWalk, num);
			}
			else
			{
				playerProfile.IncrementStat(PlayerStatType.DistanceAir, num);
			}
		}
		m_lastDistCheck = ((Component)this).transform.position;
	}

	private float GetBuildStamina()
	{
		float attackStamina = GetRightItem().m_shared.m_attack.m_attackStamina;
		attackStamina *= 1f + GetEquipmentHomeItemModifier();
		m_seman.ModifyHomeItemStaminaUsage(attackStamina, ref attackStamina);
		if (m_buildPieces.m_skill != 0)
		{
			float skillFactor = GetSkillFactor(m_buildPieces.m_skill);
			attackStamina -= attackStamina * 0.5f * skillFactor;
		}
		return attackStamina;
	}

	private void UpdatePlacement(bool takeInput, float dt)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Invalid comparison between Unknown and I4
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		UpdateWearNTearHover();
		ItemDrop.ItemData rightItem;
		Piece hoveringPiece;
		object obj;
		if (InPlaceMode() && !IsDead())
		{
			if (!takeInput)
			{
				return;
			}
			UpdateBuildGuiInput();
			if (Hud.IsPieceSelectionVisible())
			{
				return;
			}
			rightItem = GetRightItem();
			if ((ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltKeys")) && (ZInput.GetButtonDown("JoyLStick") || ZInput.GetButtonDown("JoyRStick") || ZInput.GetButtonDown("JoyButtonA") || ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("JoyButtonX") || ZInput.GetButtonDown("JoyButtonY") || ZInput.GetButtonDown("JoyDPadUp") || ZInput.GetButtonDown("JoyDPadDown") || ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetButtonDown("JoyDPadRight")))
			{
				m_blockRemove = true;
			}
			if ((ZInput.GetButtonDown("Remove") || ZInput.GetButtonDown("JoyRemove")) && (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltKeys")) && ((int)ZInput.InputLayout == 0 || !ZInput.IsGamepadActive()))
			{
				CopyPiece();
				m_blockRemove = true;
			}
			else if (!m_blockRemove && (ZInput.GetButtonUp("Remove") || ZInput.GetButtonUp("JoyRemove")))
			{
				m_removePressedTime = Time.time;
			}
			if (!ZInput.GetButton("AltPlace") && !ZInput.GetButton("JoyAltKeys"))
			{
				m_blockRemove = false;
			}
			hoveringPiece = GetHoveringPiece();
			if (Object.op_Implicit((Object)(object)hoveringPiece))
			{
				Feast component = ((Component)hoveringPiece).GetComponent<Feast>();
				if (component != null)
				{
					obj = component;
					goto IL_0181;
				}
			}
			obj = null;
			goto IL_0181;
		}
		if (Object.op_Implicit((Object)(object)m_placementGhost))
		{
			m_placementGhost.SetActive(false);
		}
		return;
		IL_0181:
		Feast feast = (Feast)obj;
		object obj2;
		if (Object.op_Implicit((Object)(object)hoveringPiece))
		{
			ItemDrop component2 = ((Component)hoveringPiece).GetComponent<ItemDrop>();
			if (component2 != null)
			{
				obj2 = component2;
				goto IL_019b;
			}
		}
		obj2 = null;
		goto IL_019b;
		IL_019b:
		ItemDrop itemDrop = (ItemDrop)obj2;
		bool flag = (rightItem.m_shared.m_buildPieces.m_canRemovePieces && (!Object.op_Implicit((Object)(object)hoveringPiece) || (!Object.op_Implicit((Object)(object)feast) && (!Object.op_Implicit((Object)(object)itemDrop) || !itemDrop.IsPiece())))) || (rightItem.m_shared.m_buildPieces.m_canRemoveFeasts && (!Object.op_Implicit((Object)(object)hoveringPiece) || Object.op_Implicit((Object)(object)feast) || (Object.op_Implicit((Object)(object)itemDrop) && itemDrop.IsPiece())));
		if (Time.time - m_removePressedTime < 0.2f && flag && Time.time - m_lastToolUseTime > m_removeDelay)
		{
			m_removePressedTime = -9999f;
			if (HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
			{
				if (RemovePiece())
				{
					m_lastToolUseTime = Time.time;
					AddNoise(50f);
					UseStamina(GetBuildStamina());
					if (m_buildPieces.m_skill != 0 && m_buildRemoveDebt < 20)
					{
						m_buildRemoveDebt++;
					}
					if (rightItem.m_shared.m_useDurability)
					{
						rightItem.m_durability -= GetPlaceDurability(rightItem);
					}
					rightItem.m_shared.m_destroyEffect.Create(((Component)hoveringPiece).transform.position, Quaternion.identity);
				}
			}
			else
			{
				Hud.instance.StaminaBarEmptyFlash();
			}
		}
		if ((ZInput.GetButtonDown("Attack") || ZInput.GetButtonDown("JoyPlace")) && !Hud.InRadial())
		{
			m_placePressedTime = Time.time;
		}
		if (Time.time - m_placePressedTime < 0.2f && Time.time - m_lastToolUseTime > m_placeDelay)
		{
			m_placePressedTime = -9999f;
			if (ZInput.GetButton("JoyAltKeys"))
			{
				CopyPiece();
				m_blockRemove = true;
			}
			else
			{
				Piece selectedPiece = m_buildPieces.GetSelectedPiece();
				if ((Object)(object)selectedPiece != (Object)null)
				{
					if (HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
					{
						if (selectedPiece.m_repairPiece)
						{
							Repair(rightItem, selectedPiece);
						}
						else if (selectedPiece.m_removePiece && flag)
						{
							RemovePiece();
						}
						else if ((Object)(object)m_placementGhost != (Object)null)
						{
							if (m_noPlacementCost || HaveRequirements(selectedPiece, RequirementMode.CanBuild))
							{
								if (TryPlacePiece(selectedPiece))
								{
									m_lastToolUseTime = Time.time;
									if (!ZoneSystem.instance.GetGlobalKey(selectedPiece.FreeBuildKey()))
									{
										ConsumeResources(selectedPiece.m_resources, 0);
									}
									UseStamina(GetBuildStamina());
									if (m_buildPieces.m_skill != 0)
									{
										if (m_buildRemoveDebt > 0)
										{
											m_buildRemoveDebt--;
										}
										else
										{
											RaiseSkill(m_buildPieces.m_skill);
										}
									}
									if (rightItem.m_shared.m_useDurability)
									{
										rightItem.m_durability -= GetPlaceDurability(rightItem);
									}
									rightItem.m_shared.m_buildEffect.Create(((Component)this).transform.position, Quaternion.identity);
								}
							}
							else
							{
								Message(MessageHud.MessageType.Center, "$msg_missingrequirement");
							}
						}
					}
					else
					{
						Hud.instance.StaminaBarEmptyFlash();
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)m_placementGhost))
		{
			m_placementGhost.gameObject.GetComponent<IPieceMarker>()?.ShowBuildMarker();
		}
		if (Object.op_Implicit((Object)(object)hoveringPiece))
		{
			((Component)hoveringPiece).gameObject.GetComponent<IPieceMarker>()?.ShowHoverMarker();
		}
		if (Object.op_Implicit((Object)(object)m_placementGhost))
		{
			Piece component3 = m_placementGhost.GetComponent<Piece>();
			if (component3 != null && component3.m_canRotate && m_placementGhost.activeInHierarchy)
			{
				m_scrollCurrAmount += ZInput.GetMouseScrollWheel();
				if (m_scrollCurrAmount > m_scrollAmountThreshold)
				{
					m_scrollCurrAmount = 0f;
					m_placeRotation++;
				}
				if (m_scrollCurrAmount < 0f - m_scrollAmountThreshold)
				{
					m_scrollCurrAmount = 0f;
					m_placeRotation--;
				}
			}
		}
		float num = 0f;
		bool flag2 = false;
		if (ZInput.IsGamepadActive())
		{
			InputLayout inputLayout = ZInput.InputLayout;
			if ((int)inputLayout != 0)
			{
				if (inputLayout - 1 <= 1)
				{
					bool button = ZInput.GetButton("JoyRotate");
					bool button2 = ZInput.GetButton("JoyRotateRight");
					flag2 = button || button2;
					if (button)
					{
						num = 0.5f;
					}
					else if (button2)
					{
						num = -0.5f;
					}
				}
			}
			else
			{
				num = ZInput.GetJoyRightStickX(true);
				flag2 = ZInput.GetButton("JoyRotate") && Mathf.Abs(num) > 0.5f;
			}
		}
		if (flag2)
		{
			if (m_rotatePieceTimer == 0f)
			{
				if (num < 0f)
				{
					m_placeRotation++;
				}
				else
				{
					m_placeRotation--;
				}
			}
			else if (m_rotatePieceTimer > 0.25f)
			{
				if (num < 0f)
				{
					m_placeRotation++;
				}
				else
				{
					m_placeRotation--;
				}
				m_rotatePieceTimer = 0.17f;
			}
			m_rotatePieceTimer += dt;
		}
		else
		{
			m_rotatePieceTimer = 0f;
		}
		foreach (KeyValuePair<Material, float> item in m_ghostRippleDistance)
		{
			item.Key.SetFloat("_RippleDistance", ZInput.GetKey((KeyCode)306, true) ? item.Value : 0f);
		}
	}

	private float GetPlaceDurability(ItemDrop.ItemData tool)
	{
		float num = tool.m_shared.m_useDurabilityDrain;
		if (tool.m_shared.m_placementDurabilitySkill != 0)
		{
			float skillFactor = GetSkillFactor(tool.m_shared.m_placementDurabilitySkill);
			num -= num * tool.m_shared.m_placementDurabilityMax * skillFactor;
		}
		return num;
	}

	private void UpdateBuildGuiInputAlternative1()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!Hud.IsPieceSelectionVisible() && ZInput.GetButtonDown("JoyBuildMenu") && !PlayerController.HasInputDelay && !Hud.InRadial())
		{
			for (int i = 0; i < m_buildPieces.m_selectedPiece.Length; i++)
			{
				m_buildPieces.m_lastSelectedPiece[i] = m_buildPieces.m_selectedPiece[i];
			}
			Hud.instance.TogglePieceSelection();
		}
		else
		{
			if (!Hud.IsPieceSelectionVisible())
			{
				return;
			}
			if (ZInput.GetKeyDown((KeyCode)27, true) || ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("BuildMenu"))
			{
				for (int j = 0; j < m_buildPieces.m_selectedPiece.Length; j++)
				{
					m_buildPieces.m_selectedPiece[j] = m_buildPieces.m_lastSelectedPiece[j];
				}
				Hud.HidePieceSelection();
				SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyButtonA"))
			{
				Hud.HidePieceSelection();
				PlayButtonSound();
			}
			m_scrollCurrAmount += ZInput.GetMouseScrollWheel();
			if (ZInput.GetButtonDown("JoyTabLeft") || ZInput.GetButtonDown("TabLeft") || m_scrollCurrAmount > m_scrollAmountThreshold)
			{
				m_scrollCurrAmount = 0f;
				m_buildPieces.PrevCategory();
				UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyTabRight") || ZInput.GetButtonDown("TabRight") || m_scrollCurrAmount < 0f - m_scrollAmountThreshold)
			{
				m_scrollCurrAmount = 0f;
				m_buildPieces.NextCategory();
				UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyLStickLeft") || ZInput.GetButtonDown("JoyDPadLeft"))
			{
				m_buildPieces.LeftPiece();
				SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickRight") || ZInput.GetButtonDown("JoyDPadRight"))
			{
				m_buildPieces.RightPiece();
				SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				m_buildPieces.UpPiece();
				SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				m_buildPieces.DownPiece();
				SetupPlacementGhost();
			}
		}
	}

	private void UpdateBuildGuiInput()
	{
		if (ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive())
		{
			UpdateBuildGuiInputAlternative1();
		}
		else if (!Hud.IsPieceSelectionVisible())
		{
			if (Hud.instance.IsQuickPieceSelectEnabled())
			{
				if (!Hud.IsPieceSelectionVisible() && ZInput.GetButtonDown("BuildMenu") && !PlayerController.HasInputDelay && !Hud.InRadial())
				{
					Hud.instance.TogglePieceSelection();
				}
			}
			else if (ZInput.GetButtonDown("BuildMenu") && !PlayerController.HasInputDelay && !Hud.InRadial())
			{
				Hud.instance.TogglePieceSelection();
			}
			if (ZInput.GetButtonDown("JoyUse") && !PlayerController.HasInputDelay && !Hud.InRadial())
			{
				Hud.instance.TogglePieceSelection();
			}
		}
		else if (Hud.IsPieceSelectionVisible())
		{
			if (ZInput.GetKeyDown((KeyCode)27, true) || ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("BuildMenu"))
			{
				Hud.HidePieceSelection();
			}
			if (ZInput.GetButtonDown("JoyUse"))
			{
				Hud.HidePieceSelection();
				PlayButtonSound();
			}
			m_scrollCurrAmount += ZInput.GetMouseScrollWheel();
			if (ZInput.GetButtonDown("JoyTabLeft") || ZInput.GetButtonDown("TabLeft") || m_scrollCurrAmount > m_scrollAmountThreshold)
			{
				m_scrollCurrAmount = 0f;
				m_buildPieces.PrevCategory();
				UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyTabRight") || ZInput.GetButtonDown("TabRight") || m_scrollCurrAmount < 0f - m_scrollAmountThreshold)
			{
				m_scrollCurrAmount = 0f;
				m_buildPieces.NextCategory();
				UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyLStickLeft") || ZInput.GetButtonDown("JoyDPadLeft"))
			{
				m_buildPieces.LeftPiece();
				SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickRight") || ZInput.GetButtonDown("JoyDPadRight"))
			{
				m_buildPieces.RightPiece();
				SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				m_buildPieces.UpPiece();
				SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				m_buildPieces.DownPiece();
				SetupPlacementGhost();
			}
		}
	}

	private void PlayButtonSound()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_localPlayer))
		{
			m_buttonEffects?.Create(((Component)m_localPlayer).transform.position, Quaternion.identity);
		}
	}

	public bool SetSelectedPiece(Piece p)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (m_buildPieces.GetPieceIndex(p, out var index, out var category))
		{
			SetBuildCategory(category);
			SetSelectedPiece(index);
			return true;
		}
		return false;
	}

	public void SetSelectedPiece(Vector2Int p)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_buildPieces) && m_buildPieces.GetSelectedIndex() != p)
		{
			m_buildPieces.SetSelected(p);
			SetupPlacementGhost();
		}
	}

	public Piece GetPiece(Vector2Int p)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_buildPieces != (Object)null))
		{
			return null;
		}
		return m_buildPieces.GetPiece(p);
	}

	public bool IsPieceAvailable(Piece piece)
	{
		if ((Object)(object)m_buildPieces != (Object)null)
		{
			return m_buildPieces.IsPieceAvailable(piece);
		}
		return false;
	}

	public Piece GetSelectedPiece()
	{
		if (!((Object)(object)m_buildPieces != (Object)null))
		{
			return null;
		}
		return m_buildPieces.GetSelectedPiece();
	}

	private void LateUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid())
		{
			UpdateEmote();
			if (m_nview.IsOwner())
			{
				ZNet.instance.SetReferencePosition(((Component)this).transform.position);
				UpdatePlacementGhost(flashGuardStone: false);
			}
		}
	}

	public void UpdateEvents()
	{
		if (!Object.op_Implicit((Object)(object)RandEventSystem.instance))
		{
			return;
		}
		m_readyEvents.Clear();
		foreach (RandomEvent @event in RandEventSystem.instance.m_events)
		{
			if (RandEventSystem.instance.PlayerIsReadyForEvent(this, @event))
			{
				m_readyEvents.Add(@event.m_name);
			}
		}
		if (Object.op_Implicit((Object)(object)ZNet.instance))
		{
			RandEventSystem.SetRandomEventsNeedsRefresh();
			ZNet.instance.m_serverSyncedPlayerData["possibleEvents"] = string.Join(",", m_readyEvents);
		}
	}

	private void SetupAwake()
	{
		if (m_nview.GetZDO() == null)
		{
			m_animator.SetBool("wakeup", false);
			return;
		}
		bool @bool = m_nview.GetZDO().GetBool(ZDOVars.s_wakeup, defaultValue: true);
		m_animator.SetBool("wakeup", @bool);
		if (@bool)
		{
			m_wakeupTimer = 0f;
		}
	}

	private void UpdateAwake(float dt)
	{
		if (!(m_wakeupTimer >= 0f))
		{
			return;
		}
		m_wakeupTimer += dt;
		if (m_wakeupTimer > 1f)
		{
			m_wakeupTimer = -1f;
			m_animator.SetBool("wakeup", false);
			if (m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_wakeup, value: false);
			}
		}
	}

	private void EdgeOfWorldKill(float dt)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!IsDead())
		{
			float num = Utils.DistanceXZ(Vector3.zero, ((Component)this).transform.position);
			float num2 = 10420f;
			if (num > num2 && (IsSwimming() || ((Component)this).transform.position.y < 30f))
			{
				Vector3 val = Vector3.Normalize(((Component)this).transform.position);
				float num3 = Utils.LerpStep(num2, 10500f, num) * 10f;
				m_body.MovePosition(m_body.position + val * num3 * dt);
			}
			if (num > num2 && ((Component)this).transform.position.y < -10f)
			{
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = 99999f;
				hitData.m_hitType = HitData.HitType.EdgeOfWorld;
				Damage(hitData);
			}
		}
	}

	private void AutoPickup(float dt)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		if (IsTeleporting() || !m_enableAutoPickup)
		{
			return;
		}
		Vector3 val = ((Component)this).transform.position + Vector3.up;
		Collider[] array = Physics.OverlapSphere(val, m_autoPickupRange, m_autoPickupMask);
		foreach (Collider val2 in array)
		{
			if (!Object.op_Implicit((Object)(object)val2.attachedRigidbody))
			{
				continue;
			}
			ItemDrop component = ((Component)val2.attachedRigidbody).GetComponent<ItemDrop>();
			FloatingTerrainDummy floatingTerrainDummy = null;
			if ((Object)(object)component == (Object)null && Object.op_Implicit((Object)(object)(floatingTerrainDummy = ((Component)val2.attachedRigidbody).gameObject.GetComponent<FloatingTerrainDummy>())) && Object.op_Implicit((Object)(object)floatingTerrainDummy))
			{
				component = ((Component)floatingTerrainDummy.m_parent).gameObject.GetComponent<ItemDrop>();
			}
			if ((Object)(object)component == (Object)null || !component.m_autoPickup || component.IsPiece() || HaveUniqueKey(component.m_itemData.m_shared.m_name) || !((Component)component).GetComponent<ZNetView>().IsValid())
			{
				continue;
			}
			if (!component.CanPickup())
			{
				component.RequestOwn();
			}
			else
			{
				if (component.InTar())
				{
					continue;
				}
				component.Load();
				if (!m_inventory.CanAddItem(component.m_itemData) || component.m_itemData.GetWeight() + m_inventory.GetTotalWeight() > GetMaxCarryWeight())
				{
					continue;
				}
				float num = Vector3.Distance(((Component)component).transform.position, val);
				if (num > m_autoPickupRange)
				{
					continue;
				}
				if (num < 0.3f)
				{
					Pickup(((Component)component).gameObject);
					continue;
				}
				Vector3 val3 = Vector3.Normalize(val - ((Component)component).transform.position);
				float num2 = 15f;
				Vector3 val4 = val3 * num2 * dt;
				Transform transform = ((Component)component).transform;
				transform.position += val4;
				if (Object.op_Implicit((Object)(object)floatingTerrainDummy))
				{
					Transform transform2 = ((Component)floatingTerrainDummy).transform;
					transform2.position += val4;
				}
			}
		}
	}

	private void PlayerAttackInput(float dt)
	{
		if (InPlaceMode())
		{
			return;
		}
		ItemDrop.ItemData currentWeapon = GetCurrentWeapon();
		UpdateWeaponLoading(currentWeapon, dt);
		if (currentWeapon != null && currentWeapon.m_shared.m_attack.m_bowDraw)
		{
			UpdateAttackBowDraw(currentWeapon, dt);
		}
		else
		{
			if (m_attack)
			{
				m_queuedAttackTimer = 0.5f;
				m_queuedSecondAttackTimer = 0f;
			}
			if (m_secondaryAttack)
			{
				m_queuedSecondAttackTimer = 0.5f;
				m_queuedAttackTimer = 0f;
			}
			m_queuedAttackTimer -= Time.fixedDeltaTime;
			m_queuedSecondAttackTimer -= Time.fixedDeltaTime;
			if ((m_queuedAttackTimer > 0f || m_attackHold) && StartAttack(null, secondaryAttack: false))
			{
				m_queuedAttackTimer = 0f;
			}
			if ((m_queuedSecondAttackTimer > 0f || m_secondaryAttackHold) && StartAttack(null, secondaryAttack: true))
			{
				m_queuedSecondAttackTimer = 0f;
			}
		}
		if (m_currentAttack != null && m_currentAttack.m_loopingAttack && !(m_currentAttackIsSecondary ? m_secondaryAttackHold : m_attackHold))
		{
			m_currentAttack.Abort();
		}
	}

	private void UpdateWeaponLoading(ItemDrop.ItemData weapon, float dt)
	{
		if (weapon == null || !weapon.m_shared.m_attack.m_requiresReload)
		{
			SetWeaponLoaded(null);
		}
		else if (m_weaponLoaded != weapon && weapon.m_shared.m_attack.m_requiresReload && !IsReloadActionQueued() && TryUseEitr(weapon.m_shared.m_attack.m_reloadEitrDrain))
		{
			QueueReloadAction();
		}
	}

	private void CancelReloadAction()
	{
		foreach (MinorActionData item in m_actionQueue)
		{
			if (item.m_type == MinorActionData.ActionType.Reload)
			{
				m_actionQueue.Remove(item);
				break;
			}
		}
	}

	public override void ResetLoadedWeapon()
	{
		SetWeaponLoaded(null);
		foreach (MinorActionData item in m_actionQueue)
		{
			if (item.m_type == MinorActionData.ActionType.Reload)
			{
				m_actionQueue.Remove(item);
				break;
			}
		}
	}

	private void SetWeaponLoaded(ItemDrop.ItemData weapon)
	{
		if (weapon != m_weaponLoaded)
		{
			m_weaponLoaded = weapon;
			m_nview.GetZDO().Set(ZDOVars.s_weaponLoaded, weapon != null);
		}
	}

	public override bool IsWeaponLoaded()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (!m_nview.IsOwner())
		{
			return m_nview.GetZDO().GetBool(ZDOVars.s_weaponLoaded);
		}
		return m_weaponLoaded != null;
	}

	private void UpdateAttackBowDraw(ItemDrop.ItemData weapon, float dt)
	{
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if (m_blocking || InMinorAction() || IsAttached())
		{
			m_attackDrawTime = -1f;
			if (!string.IsNullOrEmpty(weapon.m_shared.m_attack.m_drawAnimationState))
			{
				m_zanim.SetBool(weapon.m_shared.m_attack.m_drawAnimationState, value: false);
			}
			return;
		}
		float num = weapon.GetDrawStaminaDrain();
		float drawEitrDrain = weapon.GetDrawEitrDrain();
		if ((double)GetAttackDrawPercentage() >= 1.0)
		{
			num *= 0.5f;
		}
		num += num * GetEquipmentAttackStaminaModifier();
		m_seman.ModifyAttackStaminaUsage(num, ref num);
		bool flag = num <= 0f || HaveStamina();
		bool flag2 = drawEitrDrain <= 0f || HaveEitr();
		if (m_attackDrawTime < 0f)
		{
			if (!m_attackHold)
			{
				m_attackDrawTime = 0f;
			}
		}
		else if (m_attackHold && flag && m_attackDrawTime >= 0f)
		{
			if (m_attackDrawTime == 0f)
			{
				if (!weapon.m_shared.m_attack.StartDraw(this, weapon))
				{
					m_attackDrawTime = -1f;
					return;
				}
				weapon.m_shared.m_holdStartEffect.Create(((Component)this).transform.position, Quaternion.identity, ((Component)this).transform);
			}
			m_attackDrawTime += Time.fixedDeltaTime;
			if (!string.IsNullOrEmpty(weapon.m_shared.m_attack.m_drawAnimationState))
			{
				m_zanim.SetBool(weapon.m_shared.m_attack.m_drawAnimationState, value: true);
				m_zanim.SetFloat("drawpercent", GetAttackDrawPercentage());
			}
			UseStamina(num * dt);
			UseEitr(drawEitrDrain * dt);
		}
		else if (m_attackDrawTime > 0f)
		{
			if (flag && flag2)
			{
				StartAttack(null, secondaryAttack: false);
			}
			if (!string.IsNullOrEmpty(weapon.m_shared.m_attack.m_drawAnimationState))
			{
				m_zanim.SetBool(weapon.m_shared.m_attack.m_drawAnimationState, value: false);
			}
			m_attackDrawTime = 0f;
		}
	}

	protected override bool HaveQueuedChain()
	{
		if ((m_queuedAttackTimer > 0f || m_attackHold) && GetCurrentWeapon() != null && m_currentAttack != null)
		{
			return m_currentAttack.CanStartChainAttack();
		}
		return false;
	}

	private void UpdateBaseValue(float dt)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		m_baseValueUpdateTimer += dt;
		if (m_baseValueUpdateTimer > 2f)
		{
			m_baseValueUpdateTimer = 0f;
			m_baseValue = EffectArea.GetBaseValue(((Component)this).transform.position, 20f);
			m_comfortLevel = SE_Rested.CalculateComfortLevel(this);
			if (m_baseValueOld != m_baseValue)
			{
				m_baseValueOld = m_baseValue;
				ZNet.instance.m_serverSyncedPlayerData["baseValue"] = m_baseValue.ToString();
				m_nview.GetZDO().Set(ZDOVars.s_baseValue, m_baseValue);
				RandEventSystem.SetRandomEventsNeedsRefresh();
			}
		}
	}

	public int GetComfortLevel()
	{
		if ((Object)(object)m_nview == (Object)null)
		{
			return 0;
		}
		return m_comfortLevel;
	}

	public int GetBaseValue()
	{
		if (!m_nview.IsValid())
		{
			return 0;
		}
		if (m_nview.IsOwner())
		{
			return m_baseValue;
		}
		return m_nview.GetZDO().GetInt(ZDOVars.s_baseValue);
	}

	public bool IsSafeInHome()
	{
		return m_safeInHome;
	}

	private void UpdateBiome(float dt)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (InIntro())
		{
			return;
		}
		if (m_biomeTimer == 0f)
		{
			Location location = Location.GetLocation(((Component)this).transform.position, checkDungeons: false);
			if (location != null && !string.IsNullOrEmpty(location.m_discoverLabel))
			{
				AddKnownLocationName(location.m_discoverLabel);
			}
		}
		m_biomeTimer += dt;
		if (m_biomeTimer > 1f)
		{
			m_biomeTimer = 0f;
			Heightmap.Biome biome = Heightmap.FindBiome(((Component)this).transform.position);
			if (m_currentBiome != biome)
			{
				m_currentBiome = biome;
				AddKnownBiome(biome);
			}
		}
	}

	public Heightmap.Biome GetCurrentBiome()
	{
		return m_currentBiome;
	}

	public override void RaiseSkill(Skills.SkillType skill, float value = 1f)
	{
		if (skill != 0)
		{
			float multiplier = 1f;
			m_seman.ModifyRaiseSkill(skill, ref multiplier);
			value *= multiplier;
			m_skills.RaiseSkill(skill, value);
		}
	}

	private void UpdateStats(float dt)
	{
		if (InIntro() || IsTeleporting())
		{
			return;
		}
		m_timeSinceDeath += dt;
		UpdateModifiers();
		UpdateFood(dt, forceUpdate: false);
		bool flag = IsEncumbered();
		float maxStamina = GetMaxStamina();
		float num = 1f;
		if (IsBlocking())
		{
			num *= 0.8f;
		}
		if ((IsSwimming() && !IsOnGround()) || InAttack() || InDodge() || m_wallRunning || flag)
		{
			num = 0f;
		}
		float num2 = (m_staminaRegen + (1f - m_stamina / maxStamina) * m_staminaRegen * m_staminaRegenTimeMultiplier) * num;
		float staminaMultiplier = 1f;
		m_seman.ModifyStaminaRegen(ref staminaMultiplier);
		num2 *= staminaMultiplier;
		m_staminaRegenTimer -= dt;
		if (m_stamina < maxStamina && m_staminaRegenTimer <= 0f)
		{
			m_stamina = Mathf.Min(maxStamina, m_stamina + num2 * dt * Game.m_staminaRegenRate);
		}
		m_nview.GetZDO().Set(ZDOVars.s_stamina, m_stamina);
		float adrenaline = GetAdrenaline();
		float maxAdrenaline = GetMaxAdrenaline();
		if (maxAdrenaline > 0f)
		{
			m_lastMaxAdrenaline = maxAdrenaline;
		}
		m_adrenalineDegenTimer -= dt;
		if (adrenaline > 0f && m_adrenalineDegenTimer <= 0f)
		{
			float num3 = adrenaline / m_lastMaxAdrenaline;
			float num4 = m_adrenalineDegen.Evaluate(num3) * dt;
			AddAdrenaline(0f - num4);
		}
		m_nview.GetZDO().Set(ZDOVars.s_adrenaline, m_adrenaline);
		float maxEitr = GetMaxEitr();
		float num5 = 1f;
		if (IsBlocking())
		{
			num5 *= 0.8f;
		}
		if (InAttack() || InDodge())
		{
			num5 = 0f;
		}
		float num6 = (m_eiterRegen + (1f - m_eitr / maxEitr) * m_eiterRegen) * num5;
		float eitrMultiplier = 1f;
		m_seman.ModifyEitrRegen(ref eitrMultiplier);
		eitrMultiplier += GetEquipmentEitrRegenModifier();
		num6 *= eitrMultiplier;
		m_eitrRegenTimer -= dt;
		if (m_eitr < maxEitr && m_eitrRegenTimer <= 0f)
		{
			m_eitr = Mathf.Min(maxEitr, m_eitr + num6 * dt);
		}
		m_nview.GetZDO().Set(ZDOVars.s_eitr, m_eitr);
		if (flag)
		{
			if (((Vector3)(ref m_moveDir)).magnitude > 0.1f)
			{
				UseStamina(m_encumberedStaminaDrain * dt);
			}
			m_seman.AddStatusEffect(SEMan.s_statusEffectEncumbered);
			ShowTutorial("encumbered");
		}
		else
		{
			m_seman.RemoveStatusEffect(SEMan.s_statusEffectEncumbered);
		}
		if (!HardDeath() && (Object)(object)m_seman.GetStatusEffect(SEMan.s_statusEffectSoftDeath) == (Object)null)
		{
			m_seman.AddStatusEffect(SEMan.s_statusEffectSoftDeath);
		}
		UpdateEnvStatusEffects(dt);
	}

	public float GetEquipmentEitrRegenModifier()
	{
		float num = 0f;
		if (m_chestItem != null)
		{
			num += m_chestItem.m_shared.m_eitrRegenModifier;
		}
		if (m_legItem != null)
		{
			num += m_legItem.m_shared.m_eitrRegenModifier;
		}
		if (m_helmetItem != null)
		{
			num += m_helmetItem.m_shared.m_eitrRegenModifier;
		}
		if (m_shoulderItem != null)
		{
			num += m_shoulderItem.m_shared.m_eitrRegenModifier;
		}
		if (m_leftItem != null)
		{
			num += m_leftItem.m_shared.m_eitrRegenModifier;
		}
		if (m_rightItem != null)
		{
			num += m_rightItem.m_shared.m_eitrRegenModifier;
		}
		if (m_utilityItem != null)
		{
			num += m_utilityItem.m_shared.m_eitrRegenModifier;
		}
		return num;
	}

	private void UpdateEnvStatusEffects(float dt)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		m_nearFireTimer += dt;
		HitData.DamageModifiers damageModifiers = GetDamageModifiers();
		bool flag = m_nearFireTimer < 0.25f;
		bool flag2 = m_seman.HaveStatusEffect(SEMan.s_statusEffectBurning);
		bool flag3 = InShelter();
		HitData.DamageModifier modifier = damageModifiers.GetModifier(HitData.DamageType.Frost);
		bool flag4 = EnvMan.IsFreezing();
		bool num = EnvMan.IsCold();
		bool flag5 = EnvMan.IsWet();
		bool flag6 = IsSensed();
		bool flag7 = m_seman.HaveStatusEffect(SEMan.s_statusEffectWet);
		bool flag8 = IsSitting();
		bool flag9 = Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(((Component)this).transform.position, EffectArea.Type.WarmCozyArea, 1f));
		bool flag10 = ShieldGenerator.IsInsideShield(((Component)this).transform.position);
		bool flag11 = flag4 && !flag && !flag3;
		bool flag12 = (num && !flag) || (flag4 && flag && !flag3) || (flag4 && !flag && flag3);
		if (modifier == HitData.DamageModifier.Resistant || modifier == HitData.DamageModifier.VeryResistant || modifier == HitData.DamageModifier.SlightlyResistant || flag9)
		{
			flag11 = false;
			flag12 = false;
		}
		if (flag5 && !m_underRoof && !flag10)
		{
			m_seman.AddStatusEffect(SEMan.s_statusEffectWet, resetTime: true);
		}
		if (flag3)
		{
			m_seman.AddStatusEffect(SEMan.s_statusEffectShelter);
		}
		else
		{
			m_seman.RemoveStatusEffect(SEMan.s_statusEffectShelter);
		}
		if (flag)
		{
			m_seman.AddStatusEffect(SEMan.s_statusEffectCampFire);
		}
		else
		{
			m_seman.RemoveStatusEffect(SEMan.s_statusEffectCampFire);
		}
		bool flag13 = !flag6 && (flag8 || flag3) && !flag12 && !flag11 && (!flag7 || flag9) && !flag2 && flag;
		if (flag13)
		{
			m_seman.AddStatusEffect(SEMan.s_statusEffectResting);
		}
		else
		{
			m_seman.RemoveStatusEffect(SEMan.s_statusEffectResting);
		}
		m_safeInHome = flag13 && flag3 && (float)GetBaseValue() >= 1f;
		if (flag11)
		{
			if (!m_seman.RemoveStatusEffect(SEMan.s_statusEffectCold, quiet: true))
			{
				m_seman.AddStatusEffect(SEMan.s_statusEffectFreezing);
			}
		}
		else if (flag12)
		{
			if (!m_seman.RemoveStatusEffect(SEMan.s_statusEffectFreezing, quiet: true) && Object.op_Implicit((Object)(object)m_seman.AddStatusEffect(SEMan.s_statusEffectCold)))
			{
				ShowTutorial("cold");
			}
		}
		else
		{
			m_seman.RemoveStatusEffect(SEMan.s_statusEffectCold);
			m_seman.RemoveStatusEffect(SEMan.s_statusEffectFreezing);
		}
	}

	public bool CanEat(ItemDrop.ItemData item, bool showMessages)
	{
		foreach (Food food in m_foods)
		{
			if (food.m_item.m_shared.m_name == item.m_shared.m_name)
			{
				if (food.CanEatAgain())
				{
					return true;
				}
				if (showMessages)
				{
					Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_nomore", new string[1] { item.m_shared.m_name }));
				}
				return false;
			}
		}
		foreach (Food food2 in m_foods)
		{
			if (food2.CanEatAgain())
			{
				return true;
			}
		}
		if (m_foods.Count >= 3)
		{
			if (showMessages)
			{
				Message(MessageHud.MessageType.Center, "$msg_isfull");
			}
			return false;
		}
		return true;
	}

	private Food GetMostDepletedFood()
	{
		Food food = null;
		foreach (Food food2 in m_foods)
		{
			if (food2.CanEatAgain() && (food == null || food2.m_time < food.m_time))
			{
				food = food2;
			}
		}
		return food;
	}

	public void ClearFood()
	{
		m_foods.Clear();
	}

	public bool RemoveOneFood()
	{
		if (m_foods.Count == 0)
		{
			return false;
		}
		m_foods.RemoveAt(Random.Range(0, m_foods.Count));
		return true;
	}

	public bool EatFood(ItemDrop.ItemData item)
	{
		if (!CanEat(item, showMessages: false))
		{
			return false;
		}
		string text = "";
		if (item.m_shared.m_food > 0f)
		{
			text = text + " +" + item.m_shared.m_food + " $item_food_health ";
		}
		if (item.m_shared.m_foodStamina > 0f)
		{
			text = text + " +" + item.m_shared.m_foodStamina + " $item_food_stamina ";
		}
		if (item.m_shared.m_foodEitr > 0f)
		{
			text = text + " +" + item.m_shared.m_foodEitr + " $item_food_eitr ";
		}
		Message(MessageHud.MessageType.Center, text);
		foreach (Food food2 in m_foods)
		{
			if (food2.m_item.m_shared.m_name == item.m_shared.m_name)
			{
				if (food2.CanEatAgain())
				{
					food2.m_time = item.m_shared.m_foodBurnTime;
					food2.m_health = item.m_shared.m_food;
					food2.m_stamina = item.m_shared.m_foodStamina;
					food2.m_eitr = item.m_shared.m_foodEitr;
					UpdateFood(0f, forceUpdate: true);
					return true;
				}
				return false;
			}
		}
		if (m_foods.Count < 3)
		{
			Food food = new Food();
			food.m_name = ((Object)item.m_dropPrefab).name;
			food.m_item = item;
			food.m_time = item.m_shared.m_foodBurnTime;
			food.m_health = item.m_shared.m_food;
			food.m_stamina = item.m_shared.m_foodStamina;
			food.m_eitr = item.m_shared.m_foodEitr;
			m_foods.Add(food);
			UpdateFood(0f, forceUpdate: true);
			return true;
		}
		Food mostDepletedFood = GetMostDepletedFood();
		if (mostDepletedFood != null)
		{
			mostDepletedFood.m_name = ((Object)item.m_dropPrefab).name;
			mostDepletedFood.m_item = item;
			mostDepletedFood.m_time = item.m_shared.m_foodBurnTime;
			mostDepletedFood.m_health = item.m_shared.m_food;
			mostDepletedFood.m_stamina = item.m_shared.m_foodStamina;
			UpdateFood(0f, forceUpdate: true);
			return true;
		}
		Game.instance.IncrementPlayerStat(PlayerStatType.FoodEaten);
		return false;
	}

	private void UpdateFood(float dt, bool forceUpdate)
	{
		m_foodUpdateTimer += dt;
		if (m_foodUpdateTimer >= 1f || forceUpdate)
		{
			m_foodUpdateTimer -= 1f;
			foreach (Food food in m_foods)
			{
				food.m_time -= 1f;
				float num = Mathf.Clamp01(food.m_time / food.m_item.m_shared.m_foodBurnTime);
				num = Mathf.Pow(num, 0.3f);
				food.m_health = food.m_item.m_shared.m_food * num;
				food.m_stamina = food.m_item.m_shared.m_foodStamina * num;
				food.m_eitr = food.m_item.m_shared.m_foodEitr * num;
				if (food.m_time <= 0f)
				{
					Message(MessageHud.MessageType.Center, "$msg_food_done");
					m_foods.Remove(food);
					break;
				}
			}
			GetTotalFoodValue(out var hp, out var stamina, out var eitr);
			SetMaxHealth(hp, flashBar: true);
			SetMaxStamina(stamina, flashBar: true);
			SetMaxEitr(eitr, flashBar: true);
			if (eitr > 0f)
			{
				ShowTutorial("eitr");
			}
		}
		if (forceUpdate)
		{
			return;
		}
		m_foodRegenTimer += dt;
		if (!(m_foodRegenTimer >= 10f))
		{
			return;
		}
		m_foodRegenTimer = 0f;
		float num2 = 0f;
		foreach (Food food2 in m_foods)
		{
			num2 += food2.m_item.m_shared.m_foodRegen;
		}
		if (num2 > 0f)
		{
			float regenMultiplier = 1f;
			m_seman.ModifyHealthRegen(ref regenMultiplier);
			num2 *= regenMultiplier;
			Heal(num2);
		}
	}

	private void GetTotalFoodValue(out float hp, out float stamina, out float eitr)
	{
		hp = m_baseHP;
		stamina = m_baseStamina;
		eitr = 0f;
		foreach (Food food in m_foods)
		{
			hp += food.m_health;
			stamina += food.m_stamina;
			eitr += food.m_eitr;
		}
	}

	public float GetBaseFoodHP()
	{
		return m_baseHP;
	}

	public List<Food> GetFoods()
	{
		return m_foods;
	}

	public void OnSpawned(bool spawnValkyrie)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_spawnEffects.Create(((Component)this).transform.position, Quaternion.identity);
		if (spawnValkyrie)
		{
			SetIntro(intro: true);
			SpawnValkyrie();
		}
	}

	private void SpawnValkyrie()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		m_valkyrie.Load();
		Object.Instantiate<GameObject>(m_valkyrie.Asset, ((Component)this).transform.position, Quaternion.identity).GetComponent<ZNetView>().HoldReferenceTo((IReferenceCounted)(object)m_valkyrie);
		m_valkyrie.Release();
	}

	protected override bool CheckRun(Vector3 moveDir, float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!base.CheckRun(moveDir, dt))
		{
			return false;
		}
		bool flag = HaveStamina();
		float skillFactor = m_skills.GetSkillFactor(Skills.SkillType.Run);
		float num = Mathf.Lerp(1f, 0.5f, skillFactor);
		float num2 = m_runStaminaDrain * num;
		num2 -= num2 * GetEquipmentMovementModifier();
		num2 += num2 * GetEquipmentRunStaminaModifier();
		m_seman.ModifyRunStaminaDrain(num2, ref num2, moveDir);
		UseStamina(dt * num2 * Game.m_moveStaminaRate);
		if (HaveStamina())
		{
			m_runSkillImproveTimer += dt;
			if (m_runSkillImproveTimer > 1f)
			{
				m_runSkillImproveTimer = 0f;
				RaiseSkill(Skills.SkillType.Run);
			}
			ClearActionQueue();
			return true;
		}
		if (flag)
		{
			Hud.instance.StaminaBarEmptyFlash();
		}
		return false;
	}

	private void UpdateModifiers()
	{
		if (s_equipmentModifierSourceFields == null)
		{
			return;
		}
		for (int i = 0; i < m_equipmentModifierValues.Length; i++)
		{
			float num = 0f;
			if (m_rightItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_rightItem.m_shared);
			}
			if (m_leftItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_leftItem.m_shared);
			}
			if (m_chestItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_chestItem.m_shared);
			}
			if (m_legItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_legItem.m_shared);
			}
			if (m_helmetItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_helmetItem.m_shared);
			}
			if (m_shoulderItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_shoulderItem.m_shared);
			}
			if (m_utilityItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_utilityItem.m_shared);
			}
			if (m_trinketItem != null)
			{
				num += (float)s_equipmentModifierSourceFields[i].GetValue(m_trinketItem.m_shared);
			}
			m_equipmentModifierValues[i] = num;
		}
	}

	public void AppendEquipmentModifierTooltips(ItemDrop.ItemData item, StringBuilder sb)
	{
		for (int i = 0; i < m_equipmentModifierValues.Length; i++)
		{
			if (!(s_equipmentModifierSourceFields[i].GetValue(item.m_shared) is float num) || num == 0f)
			{
				continue;
			}
			float equipmentModifierPlusSE = GetEquipmentModifierPlusSE(i);
			if (i >= 10)
			{
				sb.AppendFormat($"\n{s_equipmentModifierTooltips[i]}: <color=orange>{num}</color>");
				continue;
			}
			sb.AppendFormat("\n" + s_equipmentModifierTooltips[i] + ": <color=orange>" + (num * 100f).ToString("+0;-0") + "%</color>");
			if (equipmentModifierPlusSE != num)
			{
				sb.AppendFormat(" ($item_total:<color=yellow>" + (GetEquipmentModifierPlusSE(i) * 100f).ToString("+0;-0") + "%</color>)");
			}
		}
	}

	public void OnSkillLevelup(Skills.SkillType skill, float level)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		m_skillLevelupEffects.Create(m_head.position, m_head.rotation, m_head);
	}

	protected override void OnJump()
	{
		ClearActionQueue();
		float staminaUse = m_jumpStaminaUsage - m_jumpStaminaUsage * GetEquipmentMovementModifier() + m_jumpStaminaUsage * GetEquipmentJumpStaminaModifier();
		m_seman.ModifyJumpStaminaUsage(staminaUse, ref staminaUse);
		UseStamina(staminaUse * Game.m_moveStaminaRate);
		Game.instance.IncrementPlayerStat(PlayerStatType.Jumps);
	}

	protected override void OnSwimming(Vector3 targetVel, float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		base.OnSwimming(targetVel, dt);
		if (((Vector3)(ref targetVel)).magnitude > 0.1f)
		{
			float skillFactor = m_skills.GetSkillFactor(Skills.SkillType.Swim);
			float num = Mathf.Lerp(m_swimStaminaDrainMinSkill, m_swimStaminaDrainMaxSkill, skillFactor);
			num += num * GetEquipmentSwimStaminaModifier();
			m_seman.ModifySwimStaminaUsage(num, ref num);
			UseStamina(dt * num * Game.m_moveStaminaRate);
			m_swimSkillImproveTimer += dt;
			if (m_swimSkillImproveTimer > 1f)
			{
				m_swimSkillImproveTimer = 0f;
				RaiseSkill(Skills.SkillType.Swim);
			}
		}
		if (!HaveStamina())
		{
			m_drownDamageTimer += dt;
			if (m_drownDamageTimer > 1f)
			{
				m_drownDamageTimer = 0f;
				float damage = Mathf.Ceil(GetMaxHealth() / 20f);
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = damage;
				hitData.m_point = GetCenterPoint();
				hitData.m_dir = Vector3.down;
				hitData.m_pushForce = 10f;
				hitData.m_hitType = HitData.HitType.Drowning;
				Damage(hitData);
				Vector3 position = ((Component)this).transform.position;
				position.y = GetLiquidLevel();
				m_drownEffects.Create(position, ((Component)this).transform.rotation);
			}
		}
	}

	protected override bool TakeInput()
	{
		bool result = (!Object.op_Implicit((Object)(object)Chat.instance) || !Chat.instance.HasFocus()) && !Console.IsVisible() && !TextInput.IsVisible() && !StoreGui.IsVisible() && !InventoryGui.IsVisible() && !Menu.IsVisible() && (!Object.op_Implicit((Object)(object)TextViewer.instance) || !TextViewer.instance.IsVisible()) && !Minimap.IsOpen() && !GameCamera.InFreeFly() && !PlayerCustomizaton.IsBarberGuiVisible();
		if (IsDead() || InCutscene() || IsTeleporting())
		{
			result = false;
		}
		return result;
	}

	public void UseHotbarItem(int index)
	{
		ItemDrop.ItemData itemAt = m_inventory.GetItemAt(index - 1, 0);
		if (itemAt != null)
		{
			UseItem(null, itemAt, fromInventoryGui: false);
		}
	}

	public bool RequiredCraftingStation(Recipe recipe, int qualityLevel, bool checkLevel)
	{
		CraftingStation requiredStation = recipe.GetRequiredStation(qualityLevel);
		if ((Object)(object)requiredStation != (Object)null)
		{
			if ((Object)(object)m_currentStation == (Object)null)
			{
				return false;
			}
			if (requiredStation.m_name != m_currentStation.m_name)
			{
				return false;
			}
			if (checkLevel)
			{
				int requiredStationLevel = recipe.GetRequiredStationLevel(qualityLevel);
				if (m_currentStation.GetLevel() < requiredStationLevel)
				{
					return false;
				}
			}
		}
		else if ((Object)(object)m_currentStation != (Object)null && !m_currentStation.m_showBasicRecipies)
		{
			return false;
		}
		return true;
	}

	public bool HaveRequirements(Recipe recipe, bool discover, int qualityLevel, int amount = 1)
	{
		if (discover)
		{
			if (Object.op_Implicit((Object)(object)recipe.m_craftingStation) && !KnowStationLevel(recipe.m_craftingStation.m_name, recipe.m_minStationLevel))
			{
				return false;
			}
		}
		else if (!RequiredCraftingStation(recipe, qualityLevel, checkLevel: true))
		{
			return false;
		}
		if (recipe.m_item.m_itemData.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc))
		{
			return false;
		}
		if (!HaveRequirementItems(recipe, discover, qualityLevel, amount))
		{
			return false;
		}
		return true;
	}

	private bool HaveRequirementItems(Recipe piece, bool discover, int qualityLevel, int amount = 1)
	{
		Piece.Requirement[] resources = piece.m_resources;
		foreach (Piece.Requirement requirement in resources)
		{
			if (!Object.op_Implicit((Object)(object)requirement.m_resItem))
			{
				continue;
			}
			if (discover)
			{
				if (requirement.m_amount <= 0)
				{
					continue;
				}
				if (piece.m_requireOnlyOneIngredient)
				{
					if (m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
					{
						return true;
					}
				}
				else if (!m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
				{
					return false;
				}
				continue;
			}
			int num = requirement.GetAmount(qualityLevel) * amount;
			int num2 = 0;
			for (int j = 1; j < requirement.m_resItem.m_itemData.m_shared.m_maxQuality + 1; j++)
			{
				int num3 = m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name, j);
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			if (piece.m_requireOnlyOneIngredient)
			{
				if (num2 >= num)
				{
					return true;
				}
			}
			else if (num2 < num)
			{
				return false;
			}
		}
		if (piece.m_requireOnlyOneIngredient)
		{
			return false;
		}
		return true;
	}

	public ItemDrop.ItemData GetFirstRequiredItem(Inventory inventory, Recipe recipe, int qualityLevel, out int amount, out int extraAmount, int craftMultiplier = 1)
	{
		Piece.Requirement[] resources = recipe.m_resources;
		foreach (Piece.Requirement requirement in resources)
		{
			if (!Object.op_Implicit((Object)(object)requirement.m_resItem))
			{
				continue;
			}
			int num = requirement.GetAmount(qualityLevel) * craftMultiplier;
			for (int j = 0; j <= requirement.m_resItem.m_itemData.m_shared.m_maxQuality; j++)
			{
				if (m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name, j) >= num)
				{
					amount = num;
					extraAmount = requirement.m_extraAmountOnlyOneIngredient;
					return inventory.GetItem(requirement.m_resItem.m_itemData.m_shared.m_name, j);
				}
			}
		}
		amount = 0;
		extraAmount = 0;
		return null;
	}

	public bool HaveRequirements(Piece piece, RequirementMode mode)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)piece.m_craftingStation))
		{
			if (mode == RequirementMode.IsKnown || mode == RequirementMode.CanAlmostBuild)
			{
				if (!m_knownStations.ContainsKey(piece.m_craftingStation.m_name))
				{
					return false;
				}
			}
			else if (!Object.op_Implicit((Object)(object)CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, ((Component)this).transform.position)) && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoWorkbench))
			{
				return false;
			}
		}
		if (piece.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(piece.m_dlc))
		{
			return false;
		}
		if (mode != RequirementMode.IsKnown && ZoneSystem.instance.GetGlobalKey(piece.FreeBuildKey()))
		{
			return true;
		}
		Piece.Requirement[] resources = piece.m_resources;
		foreach (Piece.Requirement requirement in resources)
		{
			if (!Object.op_Implicit((Object)(object)requirement.m_resItem) || requirement.m_amount <= 0)
			{
				continue;
			}
			switch (mode)
			{
			case RequirementMode.IsKnown:
				if (!m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
				{
					return false;
				}
				break;
			case RequirementMode.CanAlmostBuild:
				if (!m_inventory.HaveItem(requirement.m_resItem.m_itemData.m_shared.m_name))
				{
					return false;
				}
				break;
			case RequirementMode.CanBuild:
				if (m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) < requirement.m_amount)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	public void ConsumeResources(Piece.Requirement[] requirements, int qualityLevel, int itemQuality = -1, int multiplier = 1)
	{
		foreach (Piece.Requirement requirement in requirements)
		{
			if (Object.op_Implicit((Object)(object)requirement.m_resItem))
			{
				int num = requirement.GetAmount(qualityLevel) * multiplier;
				if (num > 0)
				{
					m_inventory.RemoveItem(requirement.m_resItem.m_itemData.m_shared.m_name, num, itemQuality);
				}
			}
		}
	}

	private void UpdateHover()
	{
		if (InPlaceMode() || IsDead() || m_doodadController != null)
		{
			m_hovering = null;
			m_hoveringCreature = null;
		}
		else
		{
			FindHoverObject(out m_hovering, out m_hoveringCreature);
		}
	}

	public bool IsMaterialKnown(string sharedName)
	{
		return m_knownMaterial.Contains(sharedName);
	}

	private bool CheckCanRemovePiece(Piece piece)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!m_noPlacementCost && (Object)(object)piece.m_craftingStation != (Object)null && !Object.op_Implicit((Object)(object)CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, ((Component)this).transform.position)) && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoWorkbench))
		{
			Message(MessageHud.MessageType.Center, "$msg_missingstation");
			return false;
		}
		return true;
	}

	private bool CopyPiece()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)GameCamera.instance).transform.position, ((Component)GameCamera.instance).transform.forward, ref val, 50f, m_removeRayMask) && Vector3.Distance(((RaycastHit)(ref val)).point, m_eye.position) < m_maxPlaceDistance)
		{
			Piece piece = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<Piece>();
			if ((Object)(object)piece == (Object)null && Object.op_Implicit((Object)(object)((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>()))
			{
				piece = TerrainModifier.FindClosestModifierPieceInRange(((RaycastHit)(ref val)).point, 2.5f);
			}
			if (Object.op_Implicit((Object)(object)piece))
			{
				if (SetSelectedPiece(piece))
				{
					Quaternion rotation = ((Component)piece).transform.rotation;
					m_placeRotation = (int)Math.Round(((Quaternion)(ref rotation)).eulerAngles.y / m_placeRotationDegrees);
					return true;
				}
				Message(MessageHud.MessageType.Center, "$msg_missingrequirement");
				return false;
			}
		}
		return false;
	}

	private bool RemovePiece()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)GameCamera.instance).transform.position, ((Component)GameCamera.instance).transform.forward, ref val, 50f, m_removeRayMask) && Vector3.Distance(((RaycastHit)(ref val)).point, m_eye.position) < m_maxPlaceDistance)
		{
			Piece piece = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<Piece>();
			if ((Object)(object)piece == (Object)null && Object.op_Implicit((Object)(object)((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>()))
			{
				piece = TerrainModifier.FindClosestModifierPieceInRange(((RaycastHit)(ref val)).point, 2.5f);
			}
			if (Object.op_Implicit((Object)(object)piece))
			{
				if (!piece.m_canBeRemoved)
				{
					return false;
				}
				if (Location.IsInsideNoBuildLocation(((Component)piece).transform.position))
				{
					Message(MessageHud.MessageType.Center, "$msg_nobuildzone");
					return false;
				}
				if (!PrivateArea.CheckAccess(((Component)piece).transform.position))
				{
					Message(MessageHud.MessageType.Center, "$msg_privatezone");
					return false;
				}
				if (!CheckCanRemovePiece(piece))
				{
					return false;
				}
				ZNetView component = ((Component)piece).GetComponent<ZNetView>();
				if ((Object)(object)component == (Object)null)
				{
					return false;
				}
				if (!piece.CanBeRemoved())
				{
					Message(MessageHud.MessageType.Center, "$msg_cantremovenow");
					return false;
				}
				((Component)piece).GetComponent<IRemoved>()?.OnRemoved();
				WearNTear component2 = ((Component)piece).GetComponent<WearNTear>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.Remove();
				}
				else
				{
					Character component3 = ((Component)piece).GetComponent<Character>();
					if (component3 != null)
					{
						component3.Damage(new HitData(1E+10f));
					}
					else
					{
						ZLog.Log((object)("Removing non WNT object with hammer " + ((Object)piece).name));
						component.ClaimOwnership();
						piece.DropResources();
						piece.m_placeEffect.Create(((Component)piece).transform.position, ((Component)piece).transform.rotation, ((Component)piece).gameObject.transform);
						m_removeEffects.Create(((Component)piece).transform.position, Quaternion.identity);
						ZNetScene.instance.Destroy(((Component)piece).gameObject);
					}
				}
				ItemDrop.ItemData rightItem = GetRightItem();
				if (rightItem != null)
				{
					FaceLookDirection();
					m_zanim.SetTrigger(rightItem.m_shared.m_attack.m_attackAnimation);
				}
				return true;
			}
		}
		return false;
	}

	public void FaceLookDirection()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = GetLookYaw();
		Physics.SyncTransforms();
	}

	public bool TryPlacePiece(Piece piece)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		UpdatePlacementGhost(flashGuardStone: true);
		switch (m_placementStatus)
		{
		case PlacementStatus.NoBuildZone:
			Message(MessageHud.MessageType.Center, "$msg_nobuildzone");
			return false;
		case PlacementStatus.BlockedbyPlayer:
			Message(MessageHud.MessageType.Center, "$msg_blocked");
			return false;
		case PlacementStatus.PrivateZone:
			Message(MessageHud.MessageType.Center, "$msg_privatezone");
			return false;
		case PlacementStatus.MoreSpace:
			Message(MessageHud.MessageType.Center, "$msg_needspace");
			return false;
		case PlacementStatus.NoTeleportArea:
			Message(MessageHud.MessageType.Center, "$msg_noteleportarea");
			return false;
		case PlacementStatus.Invalid:
		case PlacementStatus.NoRayHits:
			Message(MessageHud.MessageType.Center, "$msg_invalidplacement");
			return false;
		case PlacementStatus.ExtensionMissingStation:
			Message(MessageHud.MessageType.Center, "$msg_extensionmissingstation");
			return false;
		case PlacementStatus.WrongBiome:
			Message(MessageHud.MessageType.Center, "$msg_wrongbiome");
			return false;
		case PlacementStatus.NeedCultivated:
			Message(MessageHud.MessageType.Center, "$msg_needcultivated");
			return false;
		case PlacementStatus.NeedDirt:
			Message(MessageHud.MessageType.Center, "$msg_needdirt");
			return false;
		case PlacementStatus.NotInDungeon:
			Message(MessageHud.MessageType.Center, "$msg_notindungeon");
			return false;
		default:
			ZLog.Log((object)("Placed " + ((Object)((Component)piece).gameObject).name));
			Gogan.LogEvent("Game", "PlacedPiece", ((Object)((Component)piece).gameObject).name, 0L);
			Game.instance.IncrementPlayerStat(PlayerStatType.Builds);
			PlacePiece(piece, m_placementGhost.transform.position, m_placementGhost.transform.rotation);
			return true;
		}
	}

	public void PlacePiece(Piece piece, Vector3 pos, Quaternion rot, bool doAttack = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)piece).gameObject;
		TerrainModifier.SetTriggerOnPlaced(trigger: true);
		GameObject val = Object.Instantiate<GameObject>(gameObject, pos, rot);
		TerrainModifier.SetTriggerOnPlaced(trigger: false);
		CraftingStation componentInChildren = val.GetComponentInChildren<CraftingStation>();
		if (Object.op_Implicit((Object)(object)componentInChildren))
		{
			AddKnownStation(componentInChildren);
		}
		Piece component = val.GetComponent<Piece>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.SetCreator(GetPlayerID());
		}
		val.GetComponent<PrivateArea>()?.Setup(Game.instance.GetPlayerProfile().GetName());
		val.GetComponent<WearNTear>()?.OnPlaced();
		if (doAttack)
		{
			ItemDrop.ItemData rightItem = GetRightItem();
			if (rightItem != null)
			{
				FaceLookDirection();
				m_zanim.SetTrigger(rightItem.m_shared.m_attack.m_attackAnimation);
			}
		}
		if (piece.m_randomInitBuildRotation)
		{
			m_placeRotation = Random.Range(0, 16);
		}
		val.gameObject.GetComponent<ItemDrop>()?.MakePiece(sendRPC: true);
		m_placed.Clear();
		val.GetComponents<IPlaced>(m_placed);
		foreach (IPlaced item in m_placed)
		{
			item.OnPlaced();
		}
		piece.m_placeEffect.Create(pos, rot, val.transform);
		AddNoise(50f);
	}

	public override bool IsPlayer()
	{
		return true;
	}

	public void GetBuildSelection(out Piece go, out Vector2Int id, out int total, out Piece.PieceCategory category, out PieceTable pieceTable)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		category = m_buildPieces.GetSelectedCategory();
		pieceTable = m_buildPieces;
		if (m_buildPieces.GetAvailablePiecesInSelectedCategory() == 0)
		{
			go = null;
			id = Vector2Int.zero;
			total = 0;
		}
		else
		{
			GameObject selectedPrefab = m_buildPieces.GetSelectedPrefab();
			go = (Object.op_Implicit((Object)(object)selectedPrefab) ? selectedPrefab.GetComponent<Piece>() : null);
			id = m_buildPieces.GetSelectedIndex();
			total = m_buildPieces.GetAvailablePiecesInSelectedCategory();
		}
	}

	public List<Piece> GetBuildPieces()
	{
		if (!((Object)(object)m_buildPieces != (Object)null))
		{
			return null;
		}
		return m_buildPieces.GetPiecesInSelectedCategory();
	}

	public int GetAvailableBuildPiecesInCategory(Piece.PieceCategory cat)
	{
		if (!((Object)(object)m_buildPieces != (Object)null))
		{
			return 0;
		}
		return m_buildPieces.GetAvailablePiecesInCategory(cat);
	}

	private void RPC_OnDeath(long sender)
	{
		m_visual.SetActive(false);
	}

	private void CreateDeathEffects()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		GameObject[] array = m_deathEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		for (int i = 0; i < array.Length; i++)
		{
			Ragdoll component = array[i].GetComponent<Ragdoll>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Vector3 velocity = m_body.linearVelocity;
				if (((Vector3)(ref m_pushForce)).magnitude * 0.5f > ((Vector3)(ref velocity)).magnitude)
				{
					velocity = m_pushForce * 0.5f;
				}
				component.Setup(velocity, 0f, 0f, 0f, null);
				OnRagdollCreated(component);
				m_ragdoll = component;
			}
		}
	}

	public void UnequipDeathDropItems()
	{
		if (m_rightItem != null)
		{
			UnequipItem(m_rightItem, triggerEquipEffects: false);
		}
		if (m_leftItem != null)
		{
			UnequipItem(m_leftItem, triggerEquipEffects: false);
		}
		if (m_ammoItem != null)
		{
			UnequipItem(m_ammoItem, triggerEquipEffects: false);
		}
		if (m_utilityItem != null)
		{
			UnequipItem(m_utilityItem, triggerEquipEffects: false);
		}
		if (m_trinketItem != null)
		{
			UnequipItem(m_trinketItem, triggerEquipEffects: false);
		}
	}

	public void CreateTombStone()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (m_inventory.NrOfItems() != 0)
		{
			if (!ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathKeepEquip) && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathDeleteUnequipped))
			{
				UnequipAllItems();
			}
			if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathDeleteItems) || ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathDeleteUnequipped))
			{
				m_inventory.RemoveUnequipped();
			}
			if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathDeleteUnequipped) && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathKeepEquip))
			{
				UnequipAllItems();
			}
			GameObject obj = Object.Instantiate<GameObject>(m_tombstone, GetCenterPoint(), ((Component)this).transform.rotation);
			obj.GetComponent<Container>().GetInventory().MoveInventoryToGrave(m_inventory);
			TombStone component = obj.GetComponent<TombStone>();
			PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
			component.Setup(playerProfile.GetName(), playerProfile.GetPlayerID());
		}
	}

	private bool HardDeath()
	{
		return m_timeSinceDeath > m_hardDeathCooldown;
	}

	public void ClearHardDeath()
	{
		m_timeSinceDeath = m_hardDeathCooldown + 1f;
	}

	protected override void OnDeath()
	{
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			Debug.Log((object)"OnDeath call but not the owner");
			return;
		}
		bool flag = HardDeath();
		m_nview.GetZDO().Set(ZDOVars.s_dead, value: true);
		m_nview.InvokeRPC(ZNetView.Everybody, "OnDeath");
		Game.instance.IncrementPlayerStat(PlayerStatType.Deaths);
		switch (m_lastHit.m_hitType)
		{
		case HitData.HitType.Undefined:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByUndefined);
			break;
		case HitData.HitType.EnemyHit:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByEnemyHit);
			break;
		case HitData.HitType.PlayerHit:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByPlayerHit);
			break;
		case HitData.HitType.Fall:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByFall);
			break;
		case HitData.HitType.Drowning:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByDrowning);
			break;
		case HitData.HitType.Burning:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByBurning);
			break;
		case HitData.HitType.Freezing:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByFreezing);
			break;
		case HitData.HitType.Poisoned:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByPoisoned);
			break;
		case HitData.HitType.Water:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByWater);
			break;
		case HitData.HitType.Smoke:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathBySmoke);
			break;
		case HitData.HitType.EdgeOfWorld:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByEdgeOfWorld);
			break;
		case HitData.HitType.Impact:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByImpact);
			break;
		case HitData.HitType.Cart:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByCart);
			break;
		case HitData.HitType.Tree:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByTree);
			break;
		case HitData.HitType.Self:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathBySelf);
			break;
		case HitData.HitType.Structural:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByStructural);
			break;
		case HitData.HitType.Turret:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByTurret);
			break;
		case HitData.HitType.Boat:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByBoat);
			break;
		case HitData.HitType.Stalagtite:
			Game.instance.IncrementPlayerStat(PlayerStatType.DeathByStalagtite);
			break;
		default:
			ZLog.LogWarning((object)("Not implemented death type " + m_lastHit.m_hitType));
			break;
		}
		Game.instance.GetPlayerProfile().SetDeathPoint(((Component)this).transform.position);
		CreateDeathEffects();
		CreateTombStone();
		m_foods.Clear();
		if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathSkillsReset))
		{
			m_skills.Clear();
		}
		else if (flag)
		{
			m_skills.OnDeath();
		}
		m_seman.RemoveAllStatusEffects();
		Game.instance.RequestRespawn(10f, afterDeath: true);
		m_timeSinceDeath = 0f;
		if (!flag)
		{
			Message(MessageHud.MessageType.TopLeft, "$msg_softdeath");
		}
		Message(MessageHud.MessageType.Center, "$msg_youdied");
		ShowTutorial("death");
		Minimap.instance.AddPin(((Component)this).transform.position, Minimap.PinType.Death, $"$hud_mapday {EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds())}", save: true, isChecked: false, 0L);
		if (m_onDeath != null)
		{
			m_onDeath();
		}
		string eventLabel = "biome:" + GetCurrentBiome();
		Gogan.LogEvent("Game", "Death", eventLabel, 0L);
	}

	public void OnRespawn()
	{
		m_nview.GetZDO().Set(ZDOVars.s_dead, value: false);
		SetHealth(GetMaxHealth());
	}

	private void SetupPlacementGhost()
	{
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_placementGhost))
		{
			Object.Destroy((Object)(object)m_placementGhost);
			m_placementGhost = null;
		}
		if ((Object)(object)m_buildPieces == (Object)null || IsDead())
		{
			return;
		}
		GameObject selectedPrefab = m_buildPieces.GetSelectedPrefab();
		if ((Object)(object)selectedPrefab == (Object)null)
		{
			return;
		}
		Piece component = selectedPrefab.GetComponent<Piece>();
		if (component.m_repairPiece || component.m_removePiece)
		{
			return;
		}
		bool enabled = false;
		TerrainModifier componentInChildren = selectedPrefab.GetComponentInChildren<TerrainModifier>();
		if (Object.op_Implicit((Object)(object)componentInChildren))
		{
			enabled = ((Behaviour)componentInChildren).enabled;
			((Behaviour)componentInChildren).enabled = false;
		}
		TerrainOp.m_forceDisableTerrainOps = true;
		ZNetView.m_forceDisableInit = true;
		_ = m_placementGhost;
		m_placementGhost = Object.Instantiate<GameObject>(selectedPrefab);
		Piece component2 = m_placementGhost.GetComponent<Piece>();
		if (component2 != null && component2.m_randomInitBuildRotation)
		{
			m_placeRotation = Random.Range(0, 16);
		}
		m_placementGhost.gameObject.GetComponent<ItemDrop>()?.MakePiece();
		ZNetView.m_forceDisableInit = false;
		TerrainOp.m_forceDisableTerrainOps = false;
		((Object)m_placementGhost).name = ((Object)selectedPrefab).name;
		if (m_placementGhostLast != ((Object)m_placementGhost).name)
		{
			m_manualSnapPoint = -1;
		}
		m_placementGhostLast = ((Object)m_placementGhost).name;
		if (Object.op_Implicit((Object)(object)componentInChildren))
		{
			((Behaviour)componentInChildren).enabled = enabled;
		}
		Joint[] componentsInChildren = m_placementGhost.GetComponentsInChildren<Joint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren[i]);
		}
		Rigidbody[] componentsInChildren2 = m_placementGhost.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren2[i]);
		}
		ParticleSystemForceField[] componentsInChildren3 = m_placementGhost.GetComponentsInChildren<ParticleSystemForceField>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren3[i]);
		}
		Demister[] componentsInChildren4 = m_placementGhost.GetComponentsInChildren<Demister>();
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren4[i]);
		}
		Collider[] componentsInChildren5 = m_placementGhost.GetComponentsInChildren<Collider>();
		foreach (Collider val in componentsInChildren5)
		{
			if (((1 << ((Component)val).gameObject.layer) & m_placeRayMask) == 0)
			{
				ZLog.Log((object)("Disabling " + ((Object)((Component)val).gameObject).name + "  " + LayerMask.LayerToName(((Component)val).gameObject.layer)));
				val.enabled = false;
			}
		}
		Transform[] componentsInChildren6 = m_placementGhost.GetComponentsInChildren<Transform>();
		int layer = LayerMask.NameToLayer("ghost");
		Transform[] array = componentsInChildren6;
		for (int i = 0; i < array.Length; i++)
		{
			((Component)array[i]).gameObject.layer = layer;
		}
		TerrainModifier[] componentsInChildren7 = m_placementGhost.GetComponentsInChildren<TerrainModifier>();
		for (int i = 0; i < componentsInChildren7.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren7[i]);
		}
		GuidePoint[] componentsInChildren8 = m_placementGhost.GetComponentsInChildren<GuidePoint>();
		for (int i = 0; i < componentsInChildren8.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren8[i]);
		}
		LightLod[] componentsInChildren9 = m_placementGhost.GetComponentsInChildren<LightLod>();
		for (int i = 0; i < componentsInChildren9.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren9[i]);
		}
		LightFlicker[] componentsInChildren10 = m_placementGhost.GetComponentsInChildren<LightFlicker>();
		for (int i = 0; i < componentsInChildren10.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren10[i]);
		}
		Light[] componentsInChildren11 = m_placementGhost.GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren11.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren11[i]);
		}
		AudioSource[] componentsInChildren12 = m_placementGhost.GetComponentsInChildren<AudioSource>();
		for (int i = 0; i < componentsInChildren12.Length; i++)
		{
			((Behaviour)componentsInChildren12[i]).enabled = false;
		}
		ZSFX[] componentsInChildren13 = m_placementGhost.GetComponentsInChildren<ZSFX>();
		for (int i = 0; i < componentsInChildren13.Length; i++)
		{
			((Behaviour)componentsInChildren13[i]).enabled = false;
		}
		WispSpawner componentInChildren2 = m_placementGhost.GetComponentInChildren<WispSpawner>();
		if (Object.op_Implicit((Object)(object)componentInChildren2))
		{
			Object.Destroy((Object)(object)componentInChildren2);
		}
		Windmill componentInChildren3 = m_placementGhost.GetComponentInChildren<Windmill>();
		if (Object.op_Implicit((Object)(object)componentInChildren3))
		{
			((Behaviour)componentInChildren3).enabled = false;
		}
		ParticleSystem[] componentsInChildren14 = m_placementGhost.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren14.Length; i++)
		{
			((Component)componentsInChildren14[i]).gameObject.SetActive(false);
		}
		Transform val2 = m_placementGhost.transform.Find("_GhostOnly");
		if (Object.op_Implicit((Object)(object)val2))
		{
			((Component)val2).gameObject.SetActive(true);
		}
		m_placementGhost.transform.position = ((Component)this).transform.position;
		m_placementGhost.transform.localScale = selectedPrefab.transform.localScale;
		m_ghostRippleDistance.Clear();
		CleanupGhostMaterials<MeshRenderer>(m_placementGhost);
		CleanupGhostMaterials<SkinnedMeshRenderer>(m_placementGhost);
	}

	public static bool IsPlacementGhost(GameObject obj)
	{
		if (Object.op_Implicit((Object)(object)m_localPlayer))
		{
			return obj == m_localPlayer.m_placementGhost;
		}
		return false;
	}

	private void CleanupGhostMaterials<T>(GameObject ghost) where T : Renderer
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		T[] componentsInChildren = m_placementGhost.GetComponentsInChildren<T>();
		foreach (T val in componentsInChildren)
		{
			if ((Object)(object)((Renderer)val).sharedMaterial == (Object)null)
			{
				continue;
			}
			Material[] sharedMaterials = ((Renderer)val).sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				Material val2 = new Material(sharedMaterials[j]);
				if (val2.HasProperty("_RippleDistance"))
				{
					m_ghostRippleDistance[val2] = val2.GetFloat("_RippleDistance");
				}
				val2.SetFloat("_ValueNoise", 0f);
				val2.SetFloat("_TriplanarLocalPos", 1f);
				sharedMaterials[j] = val2;
			}
			((Renderer)val).sharedMaterials = sharedMaterials;
			((Renderer)val).shadowCastingMode = (ShadowCastingMode)0;
		}
	}

	private void SetPlacementGhostValid(bool valid)
	{
		m_placementGhost.GetComponent<Piece>().SetInvalidPlacementHeightlight(!valid);
	}

	protected override void SetPlaceMode(PieceTable buildPieces)
	{
		base.SetPlaceMode(buildPieces);
		m_buildPieces = buildPieces;
		UpdateAvailablePiecesList();
	}

	public void SetBuildCategory(int index)
	{
		if ((Object)(object)m_buildPieces != (Object)null)
		{
			m_buildPieces.SetCategory(index);
			UpdateAvailablePiecesList();
		}
	}

	public override bool InPlaceMode()
	{
		return (Object)(object)m_buildPieces != (Object)null;
	}

	public bool InRepairMode()
	{
		if (InPlaceMode())
		{
			Piece selectedPiece = m_buildPieces.GetSelectedPiece();
			if (selectedPiece != null)
			{
				if (!selectedPiece.m_repairPiece)
				{
					return selectedPiece.m_removePiece;
				}
				return true;
			}
		}
		return false;
	}

	public PlacementStatus GetPlacementStatus()
	{
		return m_placementStatus;
	}

	public bool CanRotatePiece()
	{
		if (InPlaceMode())
		{
			Piece selectedPiece = m_buildPieces.GetSelectedPiece();
			if (selectedPiece != null)
			{
				return selectedPiece.m_canRotate;
			}
		}
		return false;
	}

	private void Repair(ItemDrop.ItemData toolItem, Piece repairPiece)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!InPlaceMode())
		{
			return;
		}
		Piece hoveringPiece = GetHoveringPiece();
		if (!Object.op_Implicit((Object)(object)hoveringPiece) || !CheckCanRemovePiece(hoveringPiece) || !PrivateArea.CheckAccess(((Component)hoveringPiece).transform.position))
		{
			return;
		}
		bool flag = false;
		WearNTear component = ((Component)hoveringPiece).GetComponent<WearNTear>();
		if (Object.op_Implicit((Object)(object)component) && component.Repair())
		{
			flag = true;
		}
		if (flag)
		{
			FaceLookDirection();
			m_zanim.SetTrigger(toolItem.m_shared.m_attack.m_attackAnimation);
			hoveringPiece.m_placeEffect.Create(((Component)hoveringPiece).transform.position, ((Component)hoveringPiece).transform.rotation);
			Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_repaired", new string[1] { hoveringPiece.m_name }));
			UseStamina(GetBuildStamina());
			UseEitr(toolItem.m_shared.m_attack.m_attackEitr);
			if (toolItem.m_shared.m_useDurability)
			{
				toolItem.m_durability -= toolItem.m_shared.m_useDurabilityDrain;
			}
		}
		else
		{
			Message(MessageHud.MessageType.TopLeft, hoveringPiece.m_name + " $msg_doesnotneedrepair");
		}
	}

	private void UpdateWearNTearHover()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!InPlaceMode())
		{
			m_hoveringPiece = null;
			return;
		}
		m_hoveringPiece = null;
		RaycastHit val = default(RaycastHit);
		if (!Physics.Raycast(((Component)GameCamera.instance).transform.position, ((Component)GameCamera.instance).transform.forward, ref val, 50f, m_removeRayMask) || !(Vector3.Distance(m_eye.position, ((RaycastHit)(ref val)).point) < m_maxPlaceDistance))
		{
			return;
		}
		Piece piece = (m_hoveringPiece = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<Piece>());
		if (Object.op_Implicit((Object)(object)piece))
		{
			WearNTear component = ((Component)piece).GetComponent<WearNTear>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Highlight();
			}
		}
	}

	public Piece GetHoveringPiece()
	{
		if (!InPlaceMode())
		{
			return null;
		}
		return m_hoveringPiece;
	}

	private void UpdatePlacementGhost(bool flashGuardStone)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_078b: Unknown result type (might be due to invalid IL or missing references)
		//IL_078c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Unknown result type (might be due to invalid IL or missing references)
		//IL_0798: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0745: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0734: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_09af: Unknown result type (might be due to invalid IL or missing references)
		//IL_083b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0840: Unknown result type (might be due to invalid IL or missing references)
		//IL_0842: Unknown result type (might be due to invalid IL or missing references)
		//IL_0847: Unknown result type (might be due to invalid IL or missing references)
		//IL_0931: Unknown result type (might be due to invalid IL or missing references)
		//IL_0939: Unknown result type (might be due to invalid IL or missing references)
		//IL_0940: Unknown result type (might be due to invalid IL or missing references)
		//IL_0950: Unknown result type (might be due to invalid IL or missing references)
		//IL_0955: Unknown result type (might be due to invalid IL or missing references)
		//IL_095a: Unknown result type (might be due to invalid IL or missing references)
		//IL_095f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0962: Unknown result type (might be due to invalid IL or missing references)
		//IL_096f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0868: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_099d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0896: Unknown result type (might be due to invalid IL or missing references)
		//IL_0872: Unknown result type (might be due to invalid IL or missing references)
		//IL_0885: Unknown result type (might be due to invalid IL or missing references)
		//IL_088a: Unknown result type (might be due to invalid IL or missing references)
		//IL_088f: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0802: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_0808: Unknown result type (might be due to invalid IL or missing references)
		//IL_080a: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_081a: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a32: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_placementGhost == (Object)null)
		{
			if (Object.op_Implicit((Object)(object)m_placementMarkerInstance))
			{
				m_placementMarkerInstance.SetActive(false);
			}
			return;
		}
		bool flag = ((ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive()) ? m_altPlace : (ZInput.GetButton("AltPlace") || (ZInput.GetButton("JoyAltPlace") && !ZInput.GetButton("JoyRotate"))));
		Piece component = m_placementGhost.GetComponent<Piece>();
		bool water = component.m_waterPiece || component.m_noInWater;
		Quaternion val;
		int manualSnapPoint;
		bool num;
		if (PieceRayTest(out var point, out var normal, out var piece, out var heightmap, out var waterSurface, water))
		{
			m_placementStatus = PlacementStatus.Valid;
			val = Quaternion.Euler(0f, m_placeRotationDegrees * (float)m_placeRotation, 0f);
			if ((Object)(object)m_placementMarkerInstance == (Object)null)
			{
				m_placementMarkerInstance = Object.Instantiate<GameObject>(m_placeMarker, point, Quaternion.identity);
			}
			m_placementMarkerInstance.SetActive(true);
			m_placementMarkerInstance.transform.position = point;
			m_placementMarkerInstance.transform.rotation = Quaternion.LookRotation(normal, val * Vector3.forward);
			if (component.m_groundOnly || component.m_groundPiece || component.m_cultivatedGroundOnly)
			{
				m_placementMarkerInstance.SetActive(false);
			}
			WearNTear wearNTear = (((Object)(object)piece != (Object)null) ? ((Component)piece).GetComponent<WearNTear>() : null);
			StationExtension component2 = ((Component)component).GetComponent<StationExtension>();
			if ((Object)(object)component2 != (Object)null)
			{
				CraftingStation craftingStation = component2.FindClosestStationInRange(point);
				if (Object.op_Implicit((Object)(object)craftingStation))
				{
					component2.StartConnectionEffect(craftingStation);
				}
				else
				{
					component2.StopConnectionEffect();
					m_placementStatus = PlacementStatus.ExtensionMissingStation;
				}
				if (component2.OtherExtensionInRange(component.m_spaceRequirement))
				{
					m_placementStatus = PlacementStatus.MoreSpace;
				}
			}
			if (component.m_blockRadius > 0f && component.m_blockingPieces.Count > 0)
			{
				Collider[] array = Physics.OverlapSphere(point, component.m_blockRadius, LayerMask.GetMask(new string[1] { "piece" }));
				for (int i = 0; i < array.Length; i++)
				{
					Piece componentInParent = ((Component)array[i]).gameObject.GetComponentInParent<Piece>();
					if (componentInParent == null || !((Object)(object)componentInParent != (Object)(object)component))
					{
						continue;
					}
					foreach (Piece blockingPiece in component.m_blockingPieces)
					{
						if (blockingPiece.m_name == componentInParent.m_name)
						{
							m_placementStatus = PlacementStatus.MoreSpace;
							break;
						}
					}
				}
			}
			if ((Object)(object)component.m_mustConnectTo != (Object)null)
			{
				ZNetView zNetView = null;
				Collider[] array = Physics.OverlapSphere(((Component)component).transform.position, component.m_connectRadius);
				RaycastHit val2 = default(RaycastHit);
				for (int i = 0; i < array.Length; i++)
				{
					ZNetView componentInParent2 = ((Component)array[i]).GetComponentInParent<ZNetView>();
					if (componentInParent2 == null || !((Object)(object)componentInParent2 != (Object)(object)m_nview) || !((Object)componentInParent2).name.Contains(((Object)component.m_mustConnectTo).name))
					{
						continue;
					}
					if (component.m_mustBeAboveConnected)
					{
						Physics.Raycast(((Component)component).transform.position, Vector3.down, ref val2);
						if ((Object)(object)((Component)((RaycastHit)(ref val2)).transform).GetComponentInParent<ZNetView>() != (Object)(object)componentInParent2)
						{
							continue;
						}
					}
					zNetView = componentInParent2;
					break;
				}
				if (!Object.op_Implicit((Object)(object)zNetView))
				{
					m_placementStatus = PlacementStatus.Invalid;
				}
			}
			if (Object.op_Implicit((Object)(object)wearNTear) && !wearNTear.m_supports)
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_waterPiece && (Object)(object)waterSurface == (Object)null && !flag)
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_noInWater && (Object)(object)waterSurface != (Object)null)
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_groundPiece && (Object)(object)heightmap == (Object)null)
			{
				m_placementGhost.SetActive(false);
				m_placementStatus = PlacementStatus.Invalid;
				return;
			}
			if (component.m_groundOnly && (Object)(object)heightmap == (Object)null)
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_cultivatedGroundOnly && ((Object)(object)heightmap == (Object)null || !heightmap.IsCultivated(point)))
			{
				m_placementStatus = PlacementStatus.NeedCultivated;
			}
			if (component.m_vegetationGroundOnly)
			{
				bool flag2 = (Object)(object)heightmap == (Object)null;
				if (!flag2)
				{
					Heightmap.Biome biome = heightmap.GetBiome(point);
					float vegetationMask = heightmap.GetVegetationMask(point);
					flag2 = ((biome == Heightmap.Biome.AshLands) ? (vegetationMask > 0.1f) : (vegetationMask < 0.25f));
				}
				if (flag2)
				{
					m_placementStatus = PlacementStatus.NeedDirt;
				}
			}
			if (component.m_notOnWood && Object.op_Implicit((Object)(object)piece) && Object.op_Implicit((Object)(object)wearNTear) && (wearNTear.m_materialType == WearNTear.MaterialType.Wood || wearNTear.m_materialType == WearNTear.MaterialType.HardWood))
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_notOnTiltingSurface && normal.y < 0.8f)
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_inCeilingOnly && normal.y > -0.5f)
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_notOnFloor && normal.y > 0.1f)
			{
				m_placementStatus = PlacementStatus.Invalid;
			}
			if (component.m_onlyInTeleportArea && !Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(point, EffectArea.Type.Teleport)))
			{
				m_placementStatus = PlacementStatus.NoTeleportArea;
			}
			if (!component.m_allowedInDungeons && InInterior() && !EnvMan.instance.CheckInteriorBuildingOverride() && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.DungeonBuild))
			{
				m_placementStatus = PlacementStatus.NotInDungeon;
			}
			if (Object.op_Implicit((Object)(object)heightmap))
			{
				normal = Vector3.up;
			}
			m_placementGhost.SetActive(true);
			manualSnapPoint = m_manualSnapPoint;
			if (!ZInput.GetButton("JoyAltKeys") && !Hud.IsPieceSelectionVisible() && Minimap.instance.m_mode != Minimap.MapMode.Large && !Console.IsVisible() && !Chat.instance.HasFocus())
			{
				if (ZInput.GetButtonDown("TabLeft") || (ZInput.GetButtonUp("JoyPrevSnap") && ZInput.GetButtonLastPressedTimer("JoyPrevSnap") < 0.33f))
				{
					m_manualSnapPoint--;
				}
				if (ZInput.GetButtonDown("TabRight") || (ZInput.GetButtonUp("JoyNextSnap") && ZInput.GetButtonLastPressedTimer("JoyNextSnap") < 0.33f))
				{
					m_manualSnapPoint++;
				}
			}
			m_tempSnapPoints1.Clear();
			m_placementGhost.GetComponent<Piece>().GetSnapPoints(m_tempSnapPoints1);
			if (m_manualSnapPoint < -1)
			{
				m_manualSnapPoint = m_tempSnapPoints1.Count - 1;
			}
			if (m_manualSnapPoint >= m_tempSnapPoints1.Count)
			{
				m_manualSnapPoint = -1;
			}
			if (((component.m_groundPiece || component.m_clipGround) && Object.op_Implicit((Object)(object)heightmap)) || component.m_clipEverything)
			{
				GameObject selectedPrefab = m_buildPieces.GetSelectedPrefab();
				TerrainModifier component3 = selectedPrefab.GetComponent<TerrainModifier>();
				TerrainOp component4 = selectedPrefab.GetComponent<TerrainOp>();
				if ((Object.op_Implicit((Object)(object)component3) || Object.op_Implicit((Object)(object)component4)) && component.m_allowAltGroundPlacement)
				{
					if (!ZInput.IsNonClassicFunctionality() || !ZInput.IsGamepadActive())
					{
						if (component.m_groundPiece && !ZInput.GetButton("AltPlace"))
						{
							num = !ZInput.GetButton("JoyAltPlace");
							goto IL_06ea;
						}
					}
					else if (component.m_groundPiece)
					{
						num = !m_altPlace;
						goto IL_06ea;
					}
				}
				goto IL_070c;
			}
			Collider[] componentsInChildren = m_placementGhost.GetComponentsInChildren<Collider>();
			if (componentsInChildren.Length != 0)
			{
				m_placementGhost.transform.position = point + normal * 50f;
				m_placementGhost.transform.rotation = val;
				Vector3 val3 = Vector3.zero;
				float num2 = 999999f;
				Collider[] array = componentsInChildren;
				foreach (Collider val4 in array)
				{
					if (val4.isTrigger || !val4.enabled)
					{
						continue;
					}
					MeshCollider val5 = (MeshCollider)(object)((val4 is MeshCollider) ? val4 : null);
					if (!((Object)(object)val5 != (Object)null) || val5.convex)
					{
						Vector3 val6 = val4.ClosestPoint(point);
						float num3 = Vector3.Distance(val6, point);
						if (num3 < num2)
						{
							val3 = val6;
							num2 = num3;
						}
					}
				}
				Vector3 val7 = m_placementGhost.transform.position - val3;
				if (component.m_waterPiece)
				{
					val7.y = 3f;
				}
				m_placementGhost.transform.position = point + ((m_manualSnapPoint < 0) ? val7 : (val * -m_tempSnapPoints1[m_manualSnapPoint].localPosition));
				m_placementGhost.transform.rotation = val;
			}
			goto IL_08b4;
		}
		if (Object.op_Implicit((Object)(object)m_placementMarkerInstance))
		{
			m_placementMarkerInstance.SetActive(false);
		}
		m_placementGhost.SetActive(false);
		m_placementStatus = PlacementStatus.NoRayHits;
		goto IL_0a9d;
		IL_070c:
		m_placementGhost.transform.position = point + ((m_manualSnapPoint < 0) ? Vector3.zero : (val * -m_tempSnapPoints1[m_manualSnapPoint].localPosition));
		m_placementGhost.transform.rotation = val;
		goto IL_08b4;
		IL_0a9d:
		SetPlacementGhostValid(m_placementStatus == PlacementStatus.Valid);
		return;
		IL_06ea:
		if (num)
		{
			float groundHeight = ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position);
			point.y = groundHeight;
		}
		goto IL_070c;
		IL_08b4:
		if (manualSnapPoint != m_manualSnapPoint)
		{
			Message(MessageHud.MessageType.Center, "$msg_snapping " + ((m_manualSnapPoint == -1) ? "$msg_snapping_auto" : ((Object)m_tempSnapPoints1[m_manualSnapPoint]).name));
		}
		if (!flag)
		{
			m_tempPieces.Clear();
			if (FindClosestSnapPoints(m_placementGhost.transform, 0.5f, out var a, out var b, m_tempPieces))
			{
				_ = b.parent.position;
				Vector3 val8 = b.position - (a.position - m_placementGhost.transform.position);
				if (!IsOverlappingOtherPiece(val8, m_placementGhost.transform.rotation, ((Object)m_placementGhost).name, m_tempPieces, component.m_allowRotatedOverlap))
				{
					m_placementGhost.transform.position = val8;
				}
			}
		}
		if (Location.IsInsideNoBuildLocation(m_placementGhost.transform.position))
		{
			m_placementStatus = PlacementStatus.NoBuildZone;
		}
		PrivateArea component5 = ((Component)component).GetComponent<PrivateArea>();
		float radius = (Object.op_Implicit((Object)(object)component5) ? component5.m_radius : 0f);
		bool wardCheck = (Object)(object)component5 != (Object)null;
		if (!PrivateArea.CheckAccess(m_placementGhost.transform.position, radius, flashGuardStone, wardCheck))
		{
			m_placementStatus = PlacementStatus.PrivateZone;
		}
		if (CheckPlacementGhostVSPlayers())
		{
			m_placementStatus = PlacementStatus.BlockedbyPlayer;
		}
		if (component.m_onlyInBiome != 0 && (Heightmap.FindBiome(m_placementGhost.transform.position) & component.m_onlyInBiome) == 0)
		{
			m_placementStatus = PlacementStatus.WrongBiome;
		}
		if (component.m_noClipping && TestGhostClipping(m_placementGhost, 0.2f))
		{
			m_placementStatus = PlacementStatus.Invalid;
		}
		goto IL_0a9d;
	}

	private bool IsOverlappingOtherPiece(Vector3 p, Quaternion rotation, string pieceName, List<Piece> pieces, bool allowRotatedOverlap)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		foreach (Piece tempPiece in m_tempPieces)
		{
			if (Vector3.Distance(p, ((Component)tempPiece).transform.position) < 0.05f && (!allowRotatedOverlap || !(Quaternion.Angle(((Component)tempPiece).transform.rotation, rotation) > 10f)) && Utils.CustomStartsWith(((Object)((Component)tempPiece).gameObject).name, pieceName))
			{
				return true;
			}
		}
		return false;
	}

	private bool FindClosestSnapPoints(Transform ghost, float maxSnapDistance, out Transform a, out Transform b, List<Piece> pieces)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		m_tempSnapPoints1.Clear();
		((Component)ghost).GetComponent<Piece>().GetSnapPoints(m_tempSnapPoints1);
		m_tempSnapPoints2.Clear();
		m_tempPieces.Clear();
		Piece.GetSnapPoints(((Component)ghost).transform.position, 10f, m_tempSnapPoints2, m_tempPieces);
		float num = 9999999f;
		a = null;
		b = null;
		if (m_manualSnapPoint >= 0)
		{
			if (FindClosestSnappoint(m_tempSnapPoints1[m_manualSnapPoint].position, m_tempSnapPoints2, maxSnapDistance, out var closest, out var _))
			{
				a = m_tempSnapPoints1[m_manualSnapPoint];
				b = closest;
				return true;
			}
			return false;
		}
		foreach (Transform item in m_tempSnapPoints1)
		{
			if (FindClosestSnappoint(item.position, m_tempSnapPoints2, maxSnapDistance, out var closest2, out var distance2) && distance2 < num)
			{
				num = distance2;
				a = item;
				b = closest2;
			}
		}
		return (Object)(object)a != (Object)null;
	}

	private bool FindClosestSnappoint(Vector3 p, List<Transform> snapPoints, float maxDistance, out Transform closest, out float distance)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		closest = null;
		distance = 999999f;
		foreach (Transform snapPoint in snapPoints)
		{
			float num = Vector3.Distance(snapPoint.position, p);
			if (!(num > maxDistance) && num < distance)
			{
				closest = snapPoint;
				distance = num;
			}
		}
		return (Object)(object)closest != (Object)null;
	}

	private bool TestGhostClipping(GameObject ghost, float maxPenetration)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Collider[] componentsInChildren = ghost.GetComponentsInChildren<Collider>();
		Collider[] array = Physics.OverlapSphere(ghost.transform.position, 10f, m_placeRayMask);
		Collider[] array2 = componentsInChildren;
		Vector3 val3 = default(Vector3);
		float num = default(float);
		foreach (Collider val in array2)
		{
			Collider[] array3 = array;
			foreach (Collider val2 in array3)
			{
				if (Physics.ComputePenetration(val, ((Component)val).transform.position, ((Component)val).transform.rotation, val2, ((Component)val2).transform.position, ((Component)val2).transform.rotation, ref val3, ref num) && num > maxPenetration)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CheckPlacementGhostVSPlayers()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_placementGhost == (Object)null)
		{
			return false;
		}
		List<Character> list = new List<Character>();
		Character.GetCharactersInRange(((Component)this).transform.position, 30f, list);
		Collider[] componentsInChildren = m_placementGhost.GetComponentsInChildren<Collider>();
		Vector3 val3 = default(Vector3);
		float num = default(float);
		foreach (Collider val in componentsInChildren)
		{
			if (val.isTrigger || !val.enabled || (Object)(object)((Component)val).gameObject == (Object)(object)m_placementGhost)
			{
				continue;
			}
			MeshCollider val2 = (MeshCollider)(object)((val is MeshCollider) ? val : null);
			if ((Object)(object)val2 != (Object)null && !val2.convex)
			{
				continue;
			}
			foreach (Character item in list)
			{
				CapsuleCollider collider = item.GetCollider();
				if (Physics.ComputePenetration(val, ((Component)val).transform.position, ((Component)val).transform.rotation, (Collider)(object)collider, ((Component)collider).transform.position, ((Component)collider).transform.rotation, ref val3, ref num))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool PieceRayTest(out Vector3 point, out Vector3 normal, out Piece piece, out Heightmap heightmap, out Collider waterSurface, bool water)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		int num = m_placeRayMask;
		if (water)
		{
			num = m_placeWaterRayMask;
		}
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)GameCamera.instance).transform.position, ((Component)GameCamera.instance).transform.forward, ref val, 50f, num))
		{
			float num2 = m_maxPlaceDistance;
			if (Object.op_Implicit((Object)(object)m_placementGhost))
			{
				Piece component = m_placementGhost.GetComponent<Piece>();
				if (component != null)
				{
					num2 += (float)component.m_extraPlacementDistance;
				}
			}
			if (Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider) && !Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider.attachedRigidbody) && Vector3.Distance(m_eye.position, ((RaycastHit)(ref val)).point) < num2)
			{
				point = ((RaycastHit)(ref val)).point;
				normal = ((RaycastHit)(ref val)).normal;
				piece = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<Piece>();
				heightmap = ((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>();
				if (((Component)((RaycastHit)(ref val)).collider).gameObject.layer == LayerMask.NameToLayer("Water"))
				{
					waterSurface = ((RaycastHit)(ref val)).collider;
				}
				else
				{
					waterSurface = null;
				}
				return true;
			}
		}
		point = Vector3.zero;
		normal = Vector3.zero;
		piece = null;
		heightmap = null;
		waterSurface = null;
		return false;
	}

	private void FindHoverObject(out GameObject hover, out Character hoverCreature)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		hover = null;
		hoverCreature = null;
		int num = Physics.RaycastNonAlloc(((Component)GameCamera.instance).transform.position, ((Component)GameCamera.instance).transform.forward, m_raycastHoverHits, 50f, m_interactMask);
		Array.Sort(m_raycastHoverHits, 0, num, RaycastHitComparer.Instance);
		for (int i = 0; i < num; i++)
		{
			RaycastHit val = m_raycastHoverHits[i];
			if (Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider.attachedRigidbody) && (Object)(object)((Component)((RaycastHit)(ref val)).collider.attachedRigidbody).gameObject == (Object)(object)((Component)this).gameObject)
			{
				continue;
			}
			if ((Object)(object)hoverCreature == (Object)null)
			{
				Character character = (Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider.attachedRigidbody) ? ((Component)((RaycastHit)(ref val)).collider.attachedRigidbody).GetComponent<Character>() : ((Component)((RaycastHit)(ref val)).collider).GetComponent<Character>());
				if ((Object)(object)character != (Object)null && (!Object.op_Implicit((Object)(object)character.GetBaseAI()) || !character.GetBaseAI().IsSleeping()) && !ParticleMist.IsMistBlocked(GetCenterPoint(), character.GetCenterPoint()))
				{
					hoverCreature = character;
				}
			}
			if (Vector3.Distance(m_eye.position, ((RaycastHit)(ref val)).point) < m_maxInteractDistance)
			{
				if (((Component)((RaycastHit)(ref val)).collider).GetComponent<Hoverable>() != null)
				{
					hover = ((Component)((RaycastHit)(ref val)).collider).gameObject;
				}
				else if (Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider.attachedRigidbody))
				{
					hover = ((Component)((RaycastHit)(ref val)).collider.attachedRigidbody).gameObject;
				}
				else
				{
					hover = ((Component)((RaycastHit)(ref val)).collider).gameObject;
				}
			}
			break;
		}
	}

	private void Interact(GameObject go, bool hold, bool alt)
	{
		if (InAttack() || InDodge() || (hold && Time.time - m_lastHoverInteractTime < 0.2f))
		{
			return;
		}
		Interactable componentInParent = go.GetComponentInParent<Interactable>();
		if (componentInParent != null)
		{
			m_lastHoverInteractTime = Time.time;
			if (componentInParent.Interact(this, hold, alt))
			{
				DoInteractAnimation(((Component)((componentInParent is MonoBehaviour) ? componentInParent : null)).gameObject);
			}
		}
	}

	private void UpdateStations(float dt)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		m_stationDiscoverTimer += dt;
		if (m_stationDiscoverTimer > 1f)
		{
			m_stationDiscoverTimer = 0f;
			CraftingStation.UpdateKnownStationsInRange(this);
		}
		if ((Object)(object)m_currentStation != (Object)null)
		{
			if (!m_currentStation.InUseDistance(this))
			{
				InventoryGui.instance.Hide();
				SetCraftingStation(null);
				return;
			}
			if (!InventoryGui.IsVisible())
			{
				SetCraftingStation(null);
				return;
			}
			m_currentStation.PokeInUse();
			if (!AlwaysRotateCamera())
			{
				Vector3 val = ((Component)m_currentStation).transform.position - ((Component)this).transform.position;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				normalized.y = 0f;
				((Vector3)(ref normalized)).Normalize();
				Quaternion val2 = Quaternion.LookRotation(normalized);
				((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val2, m_turnSpeed * dt);
			}
			m_zanim.SetInt("crafting", m_currentStation.m_useAnimation);
			m_inCraftingStation = true;
		}
		else if (m_inCraftingStation)
		{
			m_zanim.SetInt("crafting", 0);
			m_inCraftingStation = false;
			if (InventoryGui.IsVisible())
			{
				InventoryGui.instance.Hide();
			}
		}
	}

	public void SetCraftingStation(CraftingStation station)
	{
		if (!((Object)(object)m_currentStation == (Object)(object)station))
		{
			if (Object.op_Implicit((Object)(object)station))
			{
				AddKnownStation(station);
				station.PokeInUse();
				HideHandItems();
			}
			m_currentStation = station;
		}
	}

	public CraftingStation GetCurrentCraftingStation()
	{
		return m_currentStation;
	}

	private void UpdateCover(float dt)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		m_updateCoverTimer += dt;
		if (m_updateCoverTimer > 1f)
		{
			m_updateCoverTimer = 0f;
			Cover.GetCoverForPoint(GetCenterPoint(), ref m_coverPercentage, ref m_underRoof, 0.5f);
		}
	}

	public Character GetHoverCreature()
	{
		return m_hoveringCreature;
	}

	public override GameObject GetHoverObject()
	{
		return m_hovering;
	}

	public override void OnNearFire(Vector3 point)
	{
		m_nearFireTimer = 0f;
	}

	public bool InShelter()
	{
		if (m_coverPercentage >= 0.8f)
		{
			return m_underRoof;
		}
		return false;
	}

	public float GetStamina()
	{
		return m_stamina;
	}

	public override float GetMaxStamina()
	{
		return m_maxStamina;
	}

	public float GetAdrenaline()
	{
		return m_adrenaline;
	}

	public override float GetMaxAdrenaline()
	{
		return m_maxAdrenaline + base.GetMaxAdrenaline();
	}

	public float GetEitr()
	{
		return m_eitr;
	}

	public override float GetMaxEitr()
	{
		return m_maxEitr;
	}

	public override float GetEitrPercentage()
	{
		return m_eitr / m_maxEitr;
	}

	public override float GetStaminaPercentage()
	{
		return m_stamina / m_maxStamina;
	}

	public void SetGodMode(bool godMode)
	{
		m_godMode = godMode;
	}

	public override bool InGodMode()
	{
		return m_godMode;
	}

	public void SetGhostMode(bool ghostmode)
	{
		m_ghostMode = ghostmode;
	}

	public override bool InGhostMode()
	{
		return m_ghostMode;
	}

	public override bool IsDebugFlying()
	{
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid())
		{
			return false;
		}
		if (m_nview.IsOwner())
		{
			return m_debugFly;
		}
		return m_nview.GetZDO().GetBool(ZDOVars.s_debugFly);
	}

	public override void AddEitr(float v)
	{
		m_eitr += v;
		if (m_eitr > m_maxEitr)
		{
			m_eitr = m_maxEitr;
		}
	}

	public override void AddStamina(float v)
	{
		m_stamina += v;
		if (m_stamina > m_maxStamina)
		{
			m_stamina = m_maxStamina;
		}
	}

	public override void AddAdrenaline(float v)
	{
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		float maxAdrenaline = GetMaxAdrenaline();
		if (v > 0f && maxAdrenaline > 0f)
		{
			float num = GetAdrenaline() / GetMaxAdrenaline();
			m_adrenalineDegenTimer = m_adrenalineDegenDelay.Evaluate(num);
			v *= Game.m_adrenalineRate;
			v *= m_adrenalineGainMultiplier.Evaluate(num);
			m_seman.ModifyAdrenaline(v, ref v);
		}
		if (v < 0f || (v > 0f && m_adrenaline < maxAdrenaline))
		{
			m_adrenaline += v;
		}
		if (m_adrenaline >= maxAdrenaline && maxAdrenaline > 0f)
		{
			List<ItemDrop.ItemData> allItems = GetInventory().GetAllItems();
			bool flag = false;
			foreach (ItemDrop.ItemData item in allItems)
			{
				if (item.m_equipped && (Object)(object)item.m_shared.m_fullAdrenalineSE != (Object)null)
				{
					flag = true;
					StatusEffect statusEffect = GetSEMan().GetStatusEffect(item.m_shared.m_fullAdrenalineSE.NameHash());
					if ((Object)(object)statusEffect != (Object)null)
					{
						statusEffect.ResetTime();
					}
					else
					{
						GetSEMan().AddStatusEffect(item.m_shared.m_fullAdrenalineSE);
					}
				}
			}
			m_adrenaline = (flag ? 0f : maxAdrenaline);
			if (flag)
			{
				m_adrenalinePopEffects.Create(((Component)this).transform.position, Quaternion.identity);
			}
		}
		if (m_adrenaline < 0f)
		{
			m_adrenaline = 0f;
		}
		StatusEffect statusEffect2 = null;
		for (int num2 = m_adrenalineEffects.Count - 1; num2 >= 0; num2--)
		{
			StatusEffectLevel statusEffectLevel = m_adrenalineEffects[num2];
			if (m_adrenaline >= statusEffectLevel.m_rate)
			{
				statusEffect2 = statusEffectLevel.m_se;
				break;
			}
		}
		if (m_adrenalineEffects.Count <= 0 || (!((Object)(object)(((Object)(object)statusEffect2 == (Object)null) ? null : GetSEMan().GetStatusEffect(statusEffect2.NameHash())) != (Object)(object)statusEffect2) && (!((Object)(object)statusEffect2 == (Object)null) || !((Object)(object)GetSEMan().GetStatusEffect(m_adrenalineEffects[0].m_se.NameHash()) != (Object)null))))
		{
			return;
		}
		foreach (StatusEffectLevel adrenalineEffect in m_adrenalineEffects)
		{
			GetSEMan().RemoveStatusEffect(adrenalineEffect.m_se.NameHash());
		}
		if ((Object)(object)statusEffect2 != (Object)null)
		{
			GetSEMan().AddStatusEffect(statusEffect2.NameHash(), resetTime: true);
			Hud.instance.AdrenalineBarFlash();
		}
	}

	public override void UseEitr(float v)
	{
		if (v != 0f && m_nview.IsValid())
		{
			if (m_nview.IsOwner())
			{
				RPC_UseEitr(0L, v);
				return;
			}
			m_nview.InvokeRPC("UseEitr", v);
		}
	}

	private void RPC_UseEitr(long sender, float v)
	{
		if (v != 0f)
		{
			m_eitr -= v;
			if (m_eitr < 0f)
			{
				m_eitr = 0f;
			}
			m_eitrRegenTimer = m_eitrRegenDelay;
		}
	}

	public override bool HaveEitr(float amount = 0f)
	{
		if (m_nview.IsValid() && !m_nview.IsOwner())
		{
			return m_nview.GetZDO().GetFloat(ZDOVars.s_eitr, m_maxEitr) > amount;
		}
		return m_eitr > amount;
	}

	public override void UseStamina(float v)
	{
		if (v == 0f || float.IsNaN(v))
		{
			return;
		}
		v *= Game.m_staminaRate;
		if (m_nview.IsValid())
		{
			if (m_nview.IsOwner())
			{
				RPC_UseStamina(0L, v);
				return;
			}
			m_nview.InvokeRPC("UseStamina", v);
		}
	}

	private void RPC_UseStamina(long sender, float v)
	{
		if (v != 0f)
		{
			m_stamina -= v;
			if (m_stamina < 0f)
			{
				m_stamina = 0f;
			}
			m_staminaRegenTimer = m_staminaRegenDelay;
		}
	}

	public override bool HaveStamina(float amount = 0f)
	{
		if (m_nview.IsValid() && !m_nview.IsOwner())
		{
			return m_nview.GetZDO().GetFloat(ZDOVars.s_stamina, m_maxStamina) > amount;
		}
		return m_stamina > amount;
	}

	public void Save(ZPackage pkg)
	{
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		pkg.Write(29);
		pkg.Write(GetMaxHealth());
		pkg.Write(GetHealth());
		pkg.Write(GetMaxStamina());
		pkg.Write(m_timeSinceDeath);
		pkg.Write(m_guardianPower);
		pkg.Write(m_guardianPowerCooldown);
		m_inventory.Save(pkg);
		pkg.Write(m_knownRecipes.Count);
		foreach (string knownRecipe in m_knownRecipes)
		{
			pkg.Write(knownRecipe);
		}
		pkg.Write(m_knownStations.Count);
		foreach (KeyValuePair<string, int> knownStation in m_knownStations)
		{
			pkg.Write(knownStation.Key);
			pkg.Write(knownStation.Value);
		}
		pkg.Write(m_knownMaterial.Count);
		foreach (string item in m_knownMaterial)
		{
			pkg.Write(item);
		}
		pkg.Write(m_shownTutorials.Count);
		foreach (string shownTutorial in m_shownTutorials)
		{
			pkg.Write(shownTutorial);
		}
		pkg.Write(m_uniques.Count);
		foreach (string unique in m_uniques)
		{
			pkg.Write(unique);
		}
		pkg.Write(m_trophies.Count);
		foreach (string trophy in m_trophies)
		{
			pkg.Write(trophy);
		}
		pkg.Write(m_knownBiome.Count);
		foreach (Heightmap.Biome item2 in m_knownBiome)
		{
			pkg.Write((int)item2);
		}
		pkg.Write(m_knownTexts.Count);
		foreach (KeyValuePair<string, string> knownText in m_knownTexts)
		{
			pkg.Write(knownText.Key.Replace("\u0016", ""));
			pkg.Write(knownText.Value.Replace("\u0016", ""));
		}
		pkg.Write(m_beardItem);
		pkg.Write(m_hairItem);
		pkg.Write(m_skinColor);
		pkg.Write(m_hairColor);
		pkg.Write(m_modelIndex);
		pkg.Write(m_foods.Count);
		foreach (Food food in m_foods)
		{
			pkg.Write(food.m_name);
			pkg.Write(food.m_time);
		}
		m_skills.Save(pkg);
		pkg.Write(m_customData.Count);
		foreach (KeyValuePair<string, string> customDatum in m_customData)
		{
			pkg.Write(customDatum.Key);
			pkg.Write(customDatum.Value);
		}
		pkg.Write(GetStamina());
		pkg.Write(GetMaxEitr());
		pkg.Write(GetEitr());
	}

	public void Load(ZPackage pkg)
	{
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		m_isLoading = true;
		UnequipAllItems();
		int num = pkg.ReadInt();
		if (num >= 7)
		{
			SetMaxHealth(pkg.ReadSingle(), flashBar: false);
		}
		float num2 = pkg.ReadSingle();
		float maxHealth = GetMaxHealth();
		if (num2 <= 0f || num2 > maxHealth || float.IsNaN(num2))
		{
			num2 = maxHealth;
		}
		SetHealth(num2);
		if (num >= 10)
		{
			float stamina = pkg.ReadSingle();
			SetMaxStamina(stamina, flashBar: false);
			m_stamina = stamina;
		}
		if (num >= 8 && num < 28)
		{
			pkg.ReadBool();
		}
		if (num >= 20)
		{
			m_timeSinceDeath = pkg.ReadSingle();
		}
		if (num >= 23)
		{
			string guardianPower = pkg.ReadString();
			SetGuardianPower(guardianPower);
		}
		if (num >= 24)
		{
			m_guardianPowerCooldown = pkg.ReadSingle();
		}
		if (num == 2)
		{
			pkg.ReadZDOID();
		}
		m_inventory.Load(pkg);
		int num3 = pkg.ReadInt();
		for (int i = 0; i < num3; i++)
		{
			string item = pkg.ReadString();
			m_knownRecipes.Add(item);
		}
		if (num < 15)
		{
			int num4 = pkg.ReadInt();
			for (int j = 0; j < num4; j++)
			{
				pkg.ReadString();
			}
		}
		else
		{
			int num5 = pkg.ReadInt();
			for (int k = 0; k < num5; k++)
			{
				string key = pkg.ReadString();
				int value = pkg.ReadInt();
				m_knownStations.Add(key, value);
			}
		}
		int num6 = pkg.ReadInt();
		for (int l = 0; l < num6; l++)
		{
			string item2 = pkg.ReadString();
			m_knownMaterial.Add(item2);
		}
		if (num < 19 || num >= 21)
		{
			int num7 = pkg.ReadInt();
			for (int m = 0; m < num7; m++)
			{
				string item3 = pkg.ReadString();
				m_shownTutorials.Add(item3);
			}
		}
		if (num >= 6)
		{
			int num8 = pkg.ReadInt();
			for (int n = 0; n < num8; n++)
			{
				string item4 = pkg.ReadString();
				m_uniques.Add(item4);
			}
		}
		if (num >= 9)
		{
			int num9 = pkg.ReadInt();
			for (int num10 = 0; num10 < num9; num10++)
			{
				string item5 = pkg.ReadString();
				m_trophies.Add(item5);
			}
		}
		if (num >= 18)
		{
			int num11 = pkg.ReadInt();
			for (int num12 = 0; num12 < num11; num12++)
			{
				Heightmap.Biome item6 = (Heightmap.Biome)pkg.ReadInt();
				m_knownBiome.Add(item6);
			}
		}
		if (num >= 22)
		{
			int num13 = pkg.ReadInt();
			for (int num14 = 0; num14 < num13; num14++)
			{
				string key2 = pkg.ReadString();
				string value2 = pkg.ReadString();
				m_knownTexts[key2] = value2;
			}
		}
		if (num >= 4)
		{
			string beard = pkg.ReadString();
			string hair = pkg.ReadString();
			SetBeard(beard);
			SetHair(hair);
		}
		if (num >= 5)
		{
			Vector3 skinColor = pkg.ReadVector3();
			Vector3 hairColor = pkg.ReadVector3();
			SetSkinColor(skinColor);
			SetHairColor(hairColor);
		}
		if (num >= 11)
		{
			int playerModel = pkg.ReadInt();
			SetPlayerModel(playerModel);
		}
		if (num >= 12)
		{
			m_foods.Clear();
			int num15 = pkg.ReadInt();
			for (int num16 = 0; num16 < num15; num16++)
			{
				if (num >= 14)
				{
					Food food = new Food();
					food.m_name = pkg.ReadString();
					if (num >= 25)
					{
						food.m_time = pkg.ReadSingle();
					}
					else
					{
						food.m_health = pkg.ReadSingle();
						if (num >= 16)
						{
							food.m_stamina = pkg.ReadSingle();
						}
					}
					GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(food.m_name);
					if ((Object)(object)itemPrefab == (Object)null)
					{
						ZLog.LogWarning((object)("Failed to find food item " + food.m_name));
						continue;
					}
					food.m_item = itemPrefab.GetComponent<ItemDrop>().m_itemData;
					m_foods.Add(food);
				}
				else
				{
					pkg.ReadString();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					if (num >= 13)
					{
						pkg.ReadSingle();
					}
				}
			}
		}
		if (num >= 17)
		{
			m_skills.Load(pkg);
		}
		if (num >= 26)
		{
			int num17 = pkg.ReadInt();
			for (int num18 = 0; num18 < num17; num18++)
			{
				string key3 = pkg.ReadString();
				string value3 = pkg.ReadString();
				m_customData[key3] = value3;
			}
			m_stamina = Mathf.Clamp(pkg.ReadSingle(), 0f, m_maxStamina);
			SetMaxEitr(pkg.ReadSingle(), flashBar: false);
			m_eitr = Mathf.Clamp(pkg.ReadSingle(), 0f, m_maxEitr);
		}
		if (num < 27)
		{
			if (m_knownMaterial.Contains("$item_flametal"))
			{
				ZLog.DevLog((object)"Pre ashlands character loaded, replacing flametal with ancient as known material.");
				m_knownMaterial.Remove("$item_flametal");
				m_knownMaterial.Add("$item_flametal_old");
			}
			if (m_knownMaterial.Contains("$item_flametalore"))
			{
				ZLog.DevLog((object)"Pre ashlands character loaded, replacing flametal ore with ancient as known material.");
				m_knownMaterial.Remove("$item_flametalore");
				m_knownMaterial.Add("$item_flametalore_old");
			}
		}
		m_isLoading = false;
		UpdateAvailablePiecesList();
		EquipInventoryItems();
		UpdateEvents();
	}

	private void EquipInventoryItems()
	{
		foreach (ItemDrop.ItemData equippedItem in m_inventory.GetEquippedItems())
		{
			if (!EquipItem(equippedItem, triggerEquipEffects: false))
			{
				equippedItem.m_equipped = false;
			}
		}
	}

	public override bool CanMove()
	{
		if (m_teleporting)
		{
			return false;
		}
		if (InCutscene())
		{
			return false;
		}
		if (IsEncumbered() && !HaveStamina())
		{
			return false;
		}
		return base.CanMove();
	}

	public override bool IsEncumbered()
	{
		return m_inventory.GetTotalWeight() > GetMaxCarryWeight();
	}

	public float GetMaxCarryWeight()
	{
		float limit = m_maxCarryWeight;
		m_seman.ModifyMaxCarryWeight(limit, ref limit);
		return limit;
	}

	public override bool HaveUniqueKey(string name)
	{
		return m_uniques.Contains(name);
	}

	public bool HaveUniqueKeyValue(string key, string value)
	{
		key = key.ToLower();
		value = value.ToLower();
		foreach (string unique in m_uniques)
		{
			string[] array = unique.Split(' ');
			if (array.Length >= 2 && array[0].ToLower() == key && array[1].ToLower() == value)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetUniqueKeyValue(string key, out string value)
	{
		key = key.ToLower();
		foreach (string unique in m_uniques)
		{
			string[] array = unique.Split(' ');
			if (array.Length >= 2 && array[0].ToLower() == key)
			{
				value = array[1];
				return true;
			}
		}
		value = null;
		return false;
	}

	public bool RemoveUniqueKeyValue(string key)
	{
		key = key.ToLower();
		int count = m_uniques.Count;
		m_uniques.RemoveWhere(delegate(string x)
		{
			string[] array = x.Split(' ');
			return (array.Length >= 2 && array[0].ToLower() == key) ? true : false;
		});
		if (m_uniques.Count != count)
		{
			ZoneSystem.instance?.UpdateWorldRates();
			UpdateEvents();
			return true;
		}
		return false;
	}

	public void AddUniqueKeyValue(string key, string value)
	{
		AddUniqueKey(key + " " + value);
	}

	public override void AddUniqueKey(string name)
	{
		if (!m_uniques.Contains(name))
		{
			m_uniques.Add(name);
		}
		ZoneSystem.instance?.UpdateWorldRates();
		UpdateEvents();
	}

	public override bool RemoveUniqueKey(string name)
	{
		if (m_uniques.Contains(name))
		{
			m_uniques.Remove(name);
			ZoneSystem.instance.UpdateWorldRates();
			UpdateEvents();
			return true;
		}
		return false;
	}

	public List<string> GetUniqueKeys()
	{
		m_tempUniqueKeys.Clear();
		m_tempUniqueKeys.AddRange(m_uniques);
		return m_tempUniqueKeys;
	}

	public void ResetUniqueKeys()
	{
		m_uniques.Clear();
	}

	public bool IsBiomeKnown(Heightmap.Biome biome)
	{
		return m_knownBiome.Contains(biome);
	}

	private void AddKnownBiome(Heightmap.Biome biome)
	{
		if (!m_knownBiome.Contains(biome))
		{
			m_knownBiome.Add(biome);
			if (biome != Heightmap.Biome.Meadows && biome != 0)
			{
				string text = "$biome_" + biome.ToString().ToLower();
				MessageHud.instance.ShowBiomeFoundMsg(text, playStinger: true);
			}
			if (biome == Heightmap.Biome.BlackForest && !ZoneSystem.instance.GetGlobalKey("defeated_eikthyr"))
			{
				ShowTutorial("blackforest");
			}
			Gogan.LogEvent("Game", "BiomeFound", biome.ToString(), 0L);
		}
		if (biome == Heightmap.Biome.BlackForest)
		{
			ShowTutorial("haldor");
		}
		if (biome == Heightmap.Biome.AshLands)
		{
			ShowTutorial("ashlands");
		}
	}

	public void AddKnownLocationName(string label)
	{
		if (!m_shownTutorials.Contains(label))
		{
			m_shownTutorials.Add(label);
			MessageHud.instance.ShowBiomeFoundMsg(label, playStinger: true);
		}
	}

	public bool IsRecipeKnown(string name)
	{
		return m_knownRecipes.Contains(name);
	}

	private void AddKnownRecipe(Recipe recipe)
	{
		if (!m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name))
		{
			m_knownRecipes.Add(recipe.m_item.m_itemData.m_shared.m_name);
			MessageHud.instance.QueueUnlockMsg(recipe.m_item.m_itemData.GetIcon(), "$msg_newrecipe", recipe.m_item.m_itemData.m_shared.m_name);
			Gogan.LogEvent("Game", "RecipeFound", recipe.m_item.m_itemData.m_shared.m_name, 0L);
		}
	}

	private void AddKnownPiece(Piece piece)
	{
		if (!m_knownRecipes.Contains(piece.m_name))
		{
			m_knownRecipes.Add(piece.m_name);
			string topic = ((piece.m_category == Piece.PieceCategory.Feasts || piece.m_category == Piece.PieceCategory.Food || piece.m_category == Piece.PieceCategory.Meads) ? "$msg_newdish" : "$msg_newpiece");
			MessageHud.instance.QueueUnlockMsg(piece.m_icon, topic, piece.m_name);
			Gogan.LogEvent("Game", "PieceFound", piece.m_name, 0L);
		}
	}

	public void AddKnownStation(CraftingStation station)
	{
		int level = station.GetLevel();
		if (m_knownStations.TryGetValue(station.m_name, out var value))
		{
			if (value < level)
			{
				m_knownStations[station.m_name] = level;
				MessageHud.instance.QueueUnlockMsg(station.m_icon, "$msg_newstation_level", station.m_name + " $msg_level " + level);
				UpdateKnownRecipesList();
			}
		}
		else
		{
			m_knownStations.Add(station.m_name, level);
			MessageHud.instance.QueueUnlockMsg(station.m_icon, "$msg_newstation", station.m_name);
			Gogan.LogEvent("Game", "StationFound", station.m_name, 0L);
			UpdateKnownRecipesList();
		}
	}

	private bool KnowStationLevel(string name, int level)
	{
		if (m_knownStations.TryGetValue(name, out var value))
		{
			return value >= level;
		}
		return false;
	}

	public void AddKnownText(string label, string text)
	{
		if (label.Length == 0)
		{
			ZLog.LogWarning((object)("Text " + text + " Is missing label"));
		}
		else if (!m_knownTexts.ContainsKey(label.Replace("\u0016", "")))
		{
			m_knownTexts.Add(label, text);
			Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_newtext", new string[1] { label }), 0, m_textIcon);
		}
	}

	public List<KeyValuePair<string, string>> GetKnownTexts()
	{
		return m_knownTexts.ToList();
	}

	public void AddKnownItem(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy)
		{
			AddTrophy(item);
		}
		if (!m_knownMaterial.Contains(item.m_shared.m_name))
		{
			m_knownMaterial.Add(item.m_shared.m_name);
			if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Material)
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newmaterial", item.m_shared.m_name);
			}
			else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy)
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newtrophy", item.m_shared.m_name);
			}
			else
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newitem", item.m_shared.m_name);
			}
			Gogan.LogEvent("Game", "ItemFound", item.m_shared.m_name, 0L);
			UpdateKnownRecipesList();
			UpdateEvents();
		}
	}

	private void AddTrophy(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Trophy)
		{
			return;
		}
		string name;
		if ((Object)(object)item.m_dropPrefab != (Object)null)
		{
			name = ((Object)item.m_dropPrefab).name;
		}
		else
		{
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(item.m_shared);
			if (!((Object)(object)itemPrefab != (Object)null))
			{
				ZLog.LogError((object)("Trying to add known trophy that is missing itemprefab in the database: " + item.m_shared.m_name));
				return;
			}
			name = ((Object)itemPrefab).name;
		}
		if (!m_trophies.Contains(name))
		{
			m_trophies.Add(name);
		}
	}

	public List<string> GetTrophies()
	{
		List<string> list = new List<string>();
		list.AddRange(m_trophies);
		return list;
	}

	private void UpdateKnownRecipesList()
	{
		if ((Object)(object)Game.instance == (Object)null)
		{
			return;
		}
		foreach (Recipe recipe in ObjectDB.instance.m_recipes)
		{
			bool flag = (Object)(object)m_currentSeason != (Object)null && m_currentSeason.Recipes.Contains(recipe);
			if ((recipe.m_enabled || flag) && Object.op_Implicit((Object)(object)recipe.m_item) && !m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name) && HaveRequirements(recipe, discover: true, 0))
			{
				AddKnownRecipe(recipe);
			}
		}
		m_tempOwnedPieceTables.Clear();
		m_inventory.GetAllPieceTables(m_tempOwnedPieceTables);
		bool flag2 = false;
		foreach (PieceTable tempOwnedPieceTable in m_tempOwnedPieceTables)
		{
			foreach (GameObject piece in tempOwnedPieceTable.m_pieces)
			{
				Piece component = piece.GetComponent<Piece>();
				bool flag3 = (Object)(object)m_currentSeason != (Object)null && m_currentSeason.Pieces.Contains(piece);
				if ((component.m_enabled || flag3) && !m_knownRecipes.Contains(component.m_name) && HaveRequirements(component, RequirementMode.IsKnown))
				{
					AddKnownPiece(component);
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			UpdateAvailablePiecesList();
		}
	}

	private void UpdateAvailablePiecesList()
	{
		if ((Object)(object)m_buildPieces != (Object)null)
		{
			m_buildPieces.UpdateAvailable(m_knownRecipes, this, hideUnavailable: false, m_noPlacementCost || ZoneSystem.instance.GetGlobalKey(GlobalKeys.AllPiecesUnlocked));
		}
		SetupPlacementGhost();
	}

	private void UpdateCurrentSeason()
	{
		m_currentSeason = null;
		foreach (SeasonalItemGroup seasonalItemGroup in m_seasonalItemGroups)
		{
			if (seasonalItemGroup.IsInSeason())
			{
				m_currentSeason = seasonalItemGroup;
				break;
			}
		}
	}

	public override void Message(MessageHud.MessageType type, string msg, int amount = 0, Sprite icon = null)
	{
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid())
		{
			return;
		}
		if (m_nview.IsOwner())
		{
			if (Object.op_Implicit((Object)(object)MessageHud.instance))
			{
				MessageHud.instance.ShowMessage(type, msg, amount, icon);
			}
		}
		else
		{
			m_nview.InvokeRPC("Message", (int)type, msg, amount);
		}
	}

	private void RPC_Message(long sender, int type, string msg, int amount)
	{
		if (m_nview.IsOwner() && Object.op_Implicit((Object)(object)MessageHud.instance))
		{
			MessageHud.instance.ShowMessage((MessageHud.MessageType)type, msg, amount);
		}
	}

	public static Player GetPlayer(long playerID)
	{
		foreach (Player s_player in s_players)
		{
			if (s_player.GetPlayerID() == playerID)
			{
				return s_player;
			}
		}
		return null;
	}

	public static Player GetClosestPlayer(Vector3 point, float maxRange)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Player result = null;
		float num = 999999f;
		foreach (Player s_player in s_players)
		{
			float num2 = Vector3.Distance(((Component)s_player).transform.position, point);
			if (num2 < num && num2 < maxRange)
			{
				num = num2;
				result = s_player;
			}
		}
		return result;
	}

	public static bool IsPlayerInRange(Vector3 point, float range, long playerID)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player s_player in s_players)
		{
			if (s_player.GetPlayerID() == playerID)
			{
				return Utils.DistanceXZ(((Component)s_player).transform.position, point) < range;
			}
		}
		return false;
	}

	public static void MessageAllInRange(Vector3 point, float range, MessageHud.MessageType type, string msg, Sprite icon = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player s_player in s_players)
		{
			if (Vector3.Distance(((Component)s_player).transform.position, point) < range)
			{
				s_player.Message(type, msg, 0, icon);
			}
		}
	}

	public static int GetPlayersInRangeXZ(Vector3 point, float range)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (Player s_player in s_players)
		{
			if (Utils.DistanceXZ(((Component)s_player).transform.position, point) < range)
			{
				num++;
			}
		}
		return num;
	}

	public static void GetPlayersInRange(Vector3 point, float range, List<Player> players)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player s_player in s_players)
		{
			if (Vector3.Distance(((Component)s_player).transform.position, point) < range)
			{
				players.Add(s_player);
			}
		}
	}

	public static bool IsPlayerInRange(Vector3 point, float range)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player s_player in s_players)
		{
			if (Vector3.Distance(((Component)s_player).transform.position, point) < range)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPlayerInRange(Vector3 point, float range, float minNoise)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player s_player in s_players)
		{
			if (Vector3.Distance(((Component)s_player).transform.position, point) < range)
			{
				float noiseRange = s_player.GetNoiseRange();
				if (range <= noiseRange && noiseRange >= minNoise)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Player GetPlayerNoiseRange(Vector3 point, float maxNoiseRange = 100f)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Player s_player in s_players)
		{
			float num = Vector3.Distance(((Component)s_player).transform.position, point);
			float num2 = Mathf.Min(s_player.GetNoiseRange(), maxNoiseRange);
			if (num < num2)
			{
				return s_player;
			}
		}
		return null;
	}

	public static List<Player> GetAllPlayers()
	{
		return s_players;
	}

	public static Player GetRandomPlayer()
	{
		if (s_players.Count == 0)
		{
			return null;
		}
		return s_players[Random.Range(0, s_players.Count)];
	}

	public void GetAvailableRecipes(ref List<Recipe> available)
	{
		available.Clear();
		foreach (Recipe recipe in ObjectDB.instance.m_recipes)
		{
			bool flag = (Object)(object)m_currentSeason != (Object)null && m_currentSeason.Recipes.Contains(recipe);
			if ((!recipe.m_enabled && !flag) || !Object.op_Implicit((Object)(object)recipe.m_item))
			{
				continue;
			}
			if (s_FilterCraft.Count > 0)
			{
				bool flag2 = false;
				for (int i = 0; i < s_FilterCraft.Count && (s_FilterCraft[i].Length <= 0 || (!((Object)recipe.m_item).name.ToLower().Contains(s_FilterCraft[i].ToLower()) && !recipe.m_item.m_itemData.m_shared.m_name.ToLower().Contains(s_FilterCraft[i].ToLower()) && !Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name).ToLower().Contains(s_FilterCraft[i].ToLower()))); i++)
				{
					if (i + 1 == s_FilterCraft.Count)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					continue;
				}
			}
			if ((recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && (m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name) || m_noPlacementCost || ZoneSystem.instance.GetGlobalKey(GlobalKeys.AllRecipesUnlocked)) && (RequiredCraftingStation(recipe, 1, checkLevel: false) || m_noPlacementCost))
			{
				available.Add(recipe);
			}
		}
	}

	private void OnInventoryChanged()
	{
		if (m_isLoading)
		{
			return;
		}
		foreach (ItemDrop.ItemData allItem in m_inventory.GetAllItems())
		{
			AddKnownItem(allItem);
			if (!allItem.m_pickedUp)
			{
				allItem.m_pickedUp = true;
				PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
				playerProfile.IncrementStat(PlayerStatType.ItemsPickedUp);
				Utils.IncrementOrSet<string>(playerProfile.m_itemPickupStats, allItem.m_shared.m_name, (float)allItem.m_stack);
			}
			if (allItem.m_shared.m_name == "$item_hammer")
			{
				ShowTutorial("hammer");
			}
			else if (allItem.m_shared.m_name == "$item_hoe")
			{
				ShowTutorial("hoe");
			}
			else if (allItem.m_shared.m_name == "$item_bellfragment")
			{
				ShowTutorial("bellfragment");
			}
			else if (allItem.m_shared.m_name == "$item_pickaxe_antler")
			{
				ShowTutorial("pickaxe");
			}
			else if (Utils.CustomStartsWith(allItem.m_shared.m_name, "$item_shield"))
			{
				ShowTutorial("shield");
			}
			if (allItem.m_shared.m_name == "$item_trophy_eikthyr")
			{
				ShowTutorial("boss_trophy");
			}
			if (allItem.m_shared.m_name == "$item_wishbone")
			{
				ShowTutorial("wishbone");
			}
			else if (allItem.m_shared.m_name == "$item_copperore" || allItem.m_shared.m_name == "$item_tinore")
			{
				ShowTutorial("ore");
			}
			else if (allItem.m_shared.m_food > 0f || allItem.m_shared.m_foodStamina > 0f)
			{
				ShowTutorial("food");
			}
			else if (allItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trinket)
			{
				ShowTutorial("trinket");
			}
		}
		UpdateKnownRecipesList();
		UpdateAvailablePiecesList();
	}

	public bool InDebugFlyMode()
	{
		return m_debugFly;
	}

	public void ShowTutorial(string name, bool force = false)
	{
		if (!HaveSeenTutorial(name))
		{
			Tutorial.instance.ShowText(name, force);
		}
	}

	public void SetSeenTutorial(string name)
	{
		if (name.Length != 0 && !m_shownTutorials.Contains(name))
		{
			m_shownTutorials.Add(name);
		}
	}

	public bool HaveSeenTutorial(string name)
	{
		if (name.Length == 0)
		{
			return false;
		}
		return m_shownTutorials.Contains(name);
	}

	public static bool IsSeenTutorialsCleared()
	{
		if (Object.op_Implicit((Object)(object)m_localPlayer))
		{
			return m_localPlayer.m_shownTutorials.Count == 0;
		}
		return true;
	}

	public static void ResetSeenTutorials()
	{
		if (Object.op_Implicit((Object)(object)m_localPlayer))
		{
			m_localPlayer.m_shownTutorials.Clear();
		}
	}

	public void SetMouseLook(Vector2 mouseLook)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = m_lookYaw * Quaternion.Euler(0f, mouseLook.x, 0f);
		if (PlayerCustomizaton.IsBarberGuiVisible())
		{
			if (Vector3.Dot(((Component)this).transform.rotation * Vector3.forward, m_lookYaw * Vector3.forward) > 0f)
			{
				SetMouseLookBackward();
			}
			if (Vector3.Dot(((Component)this).transform.rotation * Vector3.forward, val * Vector3.forward) < 0f)
			{
				m_lookYaw = val;
			}
		}
		else
		{
			m_lookYaw = val;
		}
		m_lookPitch = Mathf.Clamp(m_lookPitch - mouseLook.y, -89f, 89f);
		UpdateEyeRotation();
		m_lookDir = m_eye.forward;
		if (m_lookTransitionTime > 0f && mouseLook != Vector2.zero)
		{
			m_lookTransitionTime = 0f;
		}
	}

	public void SetMouseLookForward(bool includePitch = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Quaternion rotation = ((Component)this).transform.rotation;
		m_lookYaw = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
		if (includePitch)
		{
			m_lookPitch = 0f;
		}
	}

	public void SetMouseLookBackward(bool includePitch = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Quaternion rotation = ((Component)this).transform.rotation;
		m_lookYaw = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y + 180f, 0f);
		if (includePitch)
		{
			m_lookPitch = 0f;
		}
	}

	protected override void UpdateEyeRotation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_eye.rotation = m_lookYaw * Quaternion.Euler(m_lookPitch, 0f, 0f);
	}

	public Ragdoll GetRagdoll()
	{
		return m_ragdoll;
	}

	public void OnDodgeMortal()
	{
		m_dodgeInvincible = false;
	}

	private float GetDodgeStaminaUse()
	{
		float staminaUse = m_dodgeStaminaUsage - m_dodgeStaminaUsage * GetEquipmentMovementModifier() + m_dodgeStaminaUsage * GetEquipmentDodgeStaminaModifier();
		m_seman.ModifyDodgeStaminaUsage(staminaUse, ref staminaUse);
		float skillFactor = m_skills.GetSkillFactor(Skills.SkillType.Dodge);
		float num = Mathf.Lerp(1f, 0.5f, skillFactor);
		return staminaUse * num;
	}

	private void UpdateDodge(float dt)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		m_queuedDodgeTimer -= dt;
		if (m_queuedDodgeTimer > 0f && IsOnGround() && !IsDead() && !InAttack() && !IsEncumbered() && !InDodge() && !IsStaggering())
		{
			float dodgeStaminaUse = GetDodgeStaminaUse();
			if (HaveStamina(dodgeStaminaUse))
			{
				ClearActionQueue();
				m_queuedDodgeTimer = 0f;
				m_dodgeInvincible = true;
				((Component)this).transform.rotation = Quaternion.LookRotation(m_queuedDodgeDir);
				m_body.rotation = ((Component)this).transform.rotation;
				m_zanim.SetTrigger("dodge");
				AddNoise(5f);
				UseStamina(dodgeStaminaUse);
				m_dodgeEffects.Create(((Component)this).transform.position, Quaternion.identity, ((Component)this).transform);
			}
			else
			{
				Hud.instance.StaminaBarEmptyFlash();
			}
		}
		bool flag = m_animator.GetBool(s_animatorTagDodge) || GetNextOrCurrentAnimHash() == s_animatorTagDodge;
		bool flag2 = flag && m_dodgeInvincible;
		if (m_dodgeInvincibleCached != flag2)
		{
			m_nview.GetZDO().Set(ZDOVars.s_dodgeinv, flag2);
		}
		m_dodgeInvincibleCached = flag2;
		if (!m_inDodge)
		{
			m_beenHitWhileDodging = false;
		}
		m_inDodge = flag;
	}

	public override bool IsDodgeInvincible()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (m_nview.IsOwner())
		{
			return m_dodgeInvincibleCached;
		}
		return m_nview.GetZDO().GetBool(ZDOVars.s_dodgeinv);
	}

	public override bool InDodge()
	{
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return false;
		}
		return m_inDodge;
	}

	public override bool IsDead()
	{
		return m_nview.GetZDO()?.GetBool(ZDOVars.s_dead) ?? false;
	}

	private void Dodge(Vector3 dodgeDir)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEncumbered())
		{
			m_queuedDodgeTimer = 0.5f;
			m_queuedDodgeDir = dodgeDir;
			m_skills.RaiseSkill(Skills.SkillType.Dodge, 0.1f);
		}
	}

	public void HitWhileDodging()
	{
		m_nview.InvokeRPC("RPC_HitWhileDodging");
	}

	private void RPC_HitWhileDodging(long sender)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && !m_beenHitWhileDodging)
		{
			m_beenHitWhileDodging = true;
			m_perfectDodgeEffects.Create(((Component)this).transform.position, Quaternion.identity, ((Component)this).transform);
			float dodgeStaminaUse = GetDodgeStaminaUse();
			AddStamina(dodgeStaminaUse * m_perfectDodgeStaminaReturnMultiplier);
			AddAdrenaline(m_perfectDodgeAdrenaline);
			m_skills.RaiseSkill(Skills.SkillType.Dodge);
		}
	}

	protected override bool AlwaysRotateCamera()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		ItemDrop.ItemData currentWeapon = GetCurrentWeapon();
		if ((currentWeapon != null && m_currentAttack != null && m_lastCombatTimer < 1f && m_currentAttack.m_attackType != Attack.AttackType.None && !m_attackTowardsPlayerLookDir) || IsDrawingBow() || m_blocking)
		{
			return true;
		}
		if (currentWeapon != null && currentWeapon.m_shared.m_alwaysRotate && ((Vector3)(ref m_moveDir)).magnitude < 0.01f)
		{
			return true;
		}
		if (m_currentAttack != null && m_currentAttack.m_loopingAttack && InAttack())
		{
			return true;
		}
		if (InPlaceMode())
		{
			Vector3 val = GetLookYaw() * Vector3.forward;
			Vector3 forward = ((Component)this).transform.forward;
			if (Vector3.Angle(val, forward) > 95f)
			{
				return true;
			}
		}
		return false;
	}

	public override bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			m_nview.InvokeRPC("RPC_TeleportTo", pos, rot, distantTeleport);
			return false;
		}
		if (IsTeleporting())
		{
			return false;
		}
		if (m_teleportCooldown < 2f)
		{
			return false;
		}
		m_teleporting = true;
		m_distantTeleport = distantTeleport;
		m_teleportTimer = 0f;
		m_teleportCooldown = 0f;
		InvalidateCachedLiquidDepth();
		m_teleportFromPos = ((Component)this).transform.position;
		m_teleportFromRot = ((Component)this).transform.rotation;
		m_teleportTargetPos = pos;
		m_teleportTargetRot = rot;
		return true;
	}

	private void UpdateTeleport(float dt)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		if (!m_teleporting)
		{
			m_teleportCooldown += dt;
			return;
		}
		m_teleportCooldown = 0f;
		m_teleportTimer += dt;
		if (!(m_teleportTimer > 2f))
		{
			return;
		}
		Vector3 dir = m_teleportTargetRot * Vector3.forward;
		((Component)this).transform.position = m_teleportTargetPos;
		((Component)this).transform.rotation = m_teleportTargetRot;
		m_body.linearVelocity = Vector3.zero;
		m_maxAirAltitude = ((Component)this).transform.position.y;
		SetLookDir(dir);
		if ((!(m_teleportTimer > 8f) && m_distantTeleport) || !ZNetScene.instance.IsAreaReady(m_teleportTargetPos))
		{
			return;
		}
		float height = 0f;
		if (ZoneSystem.instance.FindFloor(m_teleportTargetPos, out height))
		{
			m_teleportTimer = 0f;
			m_teleporting = false;
			ResetCloth();
		}
		else if (m_teleportTimer > 15f || !m_distantTeleport)
		{
			if (m_distantTeleport)
			{
				Vector3 position = ((Component)this).transform.position;
				position.y = ZoneSystem.instance.GetSolidHeight(m_teleportTargetPos) + 0.5f;
				((Component)this).transform.position = position;
			}
			else
			{
				((Component)this).transform.rotation = m_teleportFromRot;
				((Component)this).transform.position = m_teleportFromPos;
				m_maxAirAltitude = ((Component)this).transform.position.y;
				Message(MessageHud.MessageType.Center, "$msg_portal_blocked");
			}
			m_teleportTimer = 0f;
			m_teleporting = false;
			ResetCloth();
		}
	}

	public override bool IsTeleporting()
	{
		return m_teleporting;
	}

	public bool ShowTeleportAnimation()
	{
		if (m_teleporting)
		{
			return m_distantTeleport;
		}
		return false;
	}

	public void SetPlayerModel(int index)
	{
		if (m_modelIndex != index)
		{
			m_modelIndex = index;
			m_visEquipment.SetModel(index);
		}
	}

	public int GetPlayerModel()
	{
		return m_modelIndex;
	}

	public void SetSkinColor(Vector3 color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!(color == m_skinColor))
		{
			m_skinColor = color;
			m_visEquipment.SetSkinColor(m_skinColor);
		}
	}

	public void SetHairColor(Vector3 color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_hairColor == color))
		{
			m_hairColor = color;
			m_visEquipment.SetHairColor(m_hairColor);
		}
	}

	public Vector3 GetHairColor()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return m_hairColor;
	}

	protected override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.SetupVisEquipment(visEq, isRagdoll);
		visEq.SetModel(m_modelIndex);
		visEq.SetSkinColor(m_skinColor);
		visEq.SetHairColor(m_hairColor);
	}

	public override bool CanConsumeItem(ItemDrop.ItemData item, bool checkWorldLevel = false)
	{
		if (!base.CanConsumeItem(item, checkWorldLevel))
		{
			return false;
		}
		if (item.m_shared.m_food > 0f && !CanEat(item, showMessages: true))
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)item.m_shared.m_consumeStatusEffect))
		{
			StatusEffect consumeStatusEffect = item.m_shared.m_consumeStatusEffect;
			if (m_seman.HaveStatusEffect(item.m_shared.m_consumeStatusEffect.NameHash()) || m_seman.HaveStatusEffectCategory(consumeStatusEffect.m_category))
			{
				Message(MessageHud.MessageType.Center, "$msg_cantconsume");
				return false;
			}
		}
		return true;
	}

	public override bool ConsumeItem(Inventory inventory, ItemDrop.ItemData item, bool checkWorldLevel = false)
	{
		if (!CanConsumeItem(item, checkWorldLevel))
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)item.m_shared.m_consumeStatusEffect))
		{
			_ = item.m_shared.m_consumeStatusEffect;
			m_seman.AddStatusEffect(item.m_shared.m_consumeStatusEffect, resetTime: true);
		}
		if (item.m_shared.m_food > 0f)
		{
			EatFood(item);
		}
		inventory.RemoveOneItem(item);
		return true;
	}

	public void SetIntro(bool intro)
	{
		if (m_intro != intro)
		{
			m_intro = intro;
			m_zanim.SetBool("intro", intro);
		}
	}

	public override bool InIntro()
	{
		return m_intro;
	}

	public override bool InCutscene()
	{
		if (GetCurrentAnimHash() == s_animatorTagCutscene)
		{
			return true;
		}
		if (InIntro())
		{
			return true;
		}
		if (m_sleeping)
		{
			return true;
		}
		return base.InCutscene();
	}

	public void SetMaxStamina(float stamina, bool flashBar)
	{
		if (flashBar && (Object)(object)Hud.instance != (Object)null && stamina > m_maxStamina)
		{
			Hud.instance.StaminaBarUppgradeFlash();
		}
		m_maxStamina = stamina;
		m_stamina = Mathf.Clamp(m_stamina, 0f, m_maxStamina);
	}

	private void SetMaxEitr(float eitr, bool flashBar)
	{
		if (flashBar && (Object)(object)Hud.instance != (Object)null && eitr > m_maxEitr)
		{
			Hud.instance.EitrBarUppgradeFlash();
		}
		m_maxEitr = eitr;
		m_eitr = Mathf.Clamp(m_eitr, 0f, m_maxEitr);
	}

	public void SetMaxHealth(float health, bool flashBar)
	{
		if (flashBar && (Object)(object)Hud.instance != (Object)null && health > GetMaxHealth())
		{
			Hud.instance.FlashHealthBar();
		}
		SetMaxHealth(health);
	}

	public override bool IsPVPEnabled()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (m_nview.IsOwner())
		{
			return m_pvp;
		}
		return m_nview.GetZDO().GetBool(ZDOVars.s_pvp);
	}

	public void SetPVP(bool enabled)
	{
		if (m_pvp != enabled)
		{
			m_pvp = enabled;
			m_nview.GetZDO().Set(ZDOVars.s_pvp, m_pvp);
			if (m_pvp)
			{
				Message(MessageHud.MessageType.Center, "$msg_pvpon");
			}
			else
			{
				Message(MessageHud.MessageType.Center, "$msg_pvpoff");
			}
		}
	}

	public bool CanSwitchPVP()
	{
		return m_lastCombatTimer > 10f;
	}

	public bool NoCostCheat()
	{
		return m_noPlacementCost;
	}

	public bool StartEmote(string emote, bool oneshot = true)
	{
		if (!CanMove() || InAttack() || IsDrawingBow() || IsAttached() || IsAttachedToShip())
		{
			return false;
		}
		SetCrouch(crouch: false);
		int @int = m_nview.GetZDO().GetInt(ZDOVars.s_emoteID);
		m_nview.GetZDO().Set(ZDOVars.s_emoteID, @int + 1);
		m_nview.GetZDO().Set(ZDOVars.s_emote, emote);
		m_nview.GetZDO().Set(ZDOVars.s_emoteOneshot, oneshot);
		LastEmote = emote;
		LastEmoteTime = DateTime.Now;
		return true;
	}

	protected override void StopEmote()
	{
		if (m_nview.GetZDO().GetString(ZDOVars.s_emote) != "")
		{
			int @int = m_nview.GetZDO().GetInt(ZDOVars.s_emoteID);
			m_nview.GetZDO().Set(ZDOVars.s_emoteID, @int + 1);
			m_nview.GetZDO().Set(ZDOVars.s_emote, "");
		}
	}

	private void UpdateEmote()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && InEmote() && m_moveDir != Vector3.zero)
		{
			StopEmote();
		}
		int @int = m_nview.GetZDO().GetInt(ZDOVars.s_emoteID);
		if (@int == m_emoteID)
		{
			return;
		}
		m_emoteID = @int;
		if (!string.IsNullOrEmpty(m_emoteState))
		{
			m_animator.SetBool("emote_" + m_emoteState, false);
		}
		m_emoteState = "";
		m_animator.SetTrigger("emote_stop");
		string @string = m_nview.GetZDO().GetString(ZDOVars.s_emote);
		if (!string.IsNullOrEmpty(@string))
		{
			bool @bool = m_nview.GetZDO().GetBool(ZDOVars.s_emoteOneshot);
			m_animator.ResetTrigger("emote_stop");
			if (@bool)
			{
				m_animator.SetTrigger("emote_" + @string);
				return;
			}
			m_emoteState = @string;
			m_animator.SetBool("emote_" + @string, true);
		}
	}

	public override bool InEmote()
	{
		if (string.IsNullOrEmpty(m_emoteState))
		{
			return GetCurrentAnimHash() == s_animatorTagEmote;
		}
		return true;
	}

	public override bool IsCrouching()
	{
		return GetCurrentAnimHash() == s_animatorTagCrouch;
	}

	private void UpdateCrouch(float dt)
	{
		if (m_crouchToggled)
		{
			if (!HaveStamina() || IsSwimming() || InBed() || InPlaceMode() || m_run || IsBlocking() || IsFlying())
			{
				SetCrouch(crouch: false);
			}
			bool flag = InAttack() || IsDrawingBow();
			m_zanim.SetBool(s_crouching, m_crouchToggled && !flag);
		}
		else
		{
			m_zanim.SetBool(s_crouching, value: false);
		}
	}

	protected override void SetCrouch(bool crouch)
	{
		m_crouchToggled = crouch;
	}

	public void SetGuardianPower(string name)
	{
		m_guardianPower = name;
		m_guardianPowerHash = ((!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
		m_guardianSE = ObjectDB.instance.GetStatusEffect(m_guardianPowerHash);
		if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
		{
			AddUniqueKey(name);
		}
	}

	public string GetGuardianPowerName()
	{
		return m_guardianPower;
	}

	public void GetGuardianPowerHUD(out StatusEffect se, out float cooldown)
	{
		se = m_guardianSE;
		cooldown = m_guardianPowerCooldown;
	}

	public bool StartGuardianPower()
	{
		if ((Object)(object)m_guardianSE == (Object)null)
		{
			return false;
		}
		if ((InAttack() && !HaveQueuedChain()) || InDodge() || !CanMove() || IsKnockedBack() || IsStaggering() || InMinorAction())
		{
			return false;
		}
		if (m_guardianPowerCooldown > 0f)
		{
			Message(MessageHud.MessageType.Center, "$hud_powernotready");
			return false;
		}
		m_zanim.SetTrigger("gpower");
		Game.instance.IncrementPlayerStat(PlayerStatType.UseGuardianPower);
		string prefabName = Utils.GetPrefabName(((Object)m_guardianSE).name);
		switch (prefabName)
		{
		case "GP_Eikthyr":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerEikthyr);
			break;
		case "GP_TheElder":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerElder);
			break;
		case "GP_Bonemass":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerBonemass);
			break;
		case "GP_Moder":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerModer);
			break;
		case "GP_Yagluth":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerYagluth);
			break;
		case "GP_Queen":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerQueen);
			break;
		case "GP_Ashlands":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerAshlands);
			break;
		case "GP_DeepNorth":
			Game.instance.IncrementPlayerStat(PlayerStatType.UsePowerDeepNorth);
			break;
		default:
			ZLog.LogWarning((object)("Missing stat for guardian power: " + prefabName));
			break;
		}
		return true;
	}

	public bool ActivateGuardianPower()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (m_guardianPowerCooldown > 0f)
		{
			return false;
		}
		if ((Object)(object)m_guardianSE == (Object)null)
		{
			return false;
		}
		List<Player> list = new List<Player>();
		GetPlayersInRange(((Component)this).transform.position, 10f, list);
		foreach (Player item in list)
		{
			item.GetSEMan().AddStatusEffect(m_guardianSE.NameHash(), resetTime: true);
		}
		if (m_adrenalineGuardianPower != 0f)
		{
			AddAdrenaline(m_adrenalineGuardianPower);
		}
		m_guardianPowerCooldown = m_guardianSE.m_cooldown;
		return false;
	}

	private void UpdateGuardianPower(float dt)
	{
		m_guardianPowerCooldown -= dt;
		if (m_guardianPowerCooldown < 0f)
		{
			m_guardianPowerCooldown = 0f;
		}
	}

	public override void AttachStart(Transform attachPoint, GameObject colliderRoot, bool hideWeapons, bool isBed, bool onShip, string attachAnimation, Vector3 detachOffset, Transform cameraPos = null)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (m_attached)
		{
			return;
		}
		m_attached = true;
		m_attachedToShip = onShip;
		m_attachPoint = attachPoint;
		m_detachOffset = detachOffset;
		m_attachAnimation = attachAnimation;
		m_attachPointCamera = cameraPos;
		m_zanim.SetBool(attachAnimation, value: true);
		m_nview.GetZDO().Set(ZDOVars.s_inBed, isBed);
		if ((Object)(object)colliderRoot != (Object)null)
		{
			m_attachColliders = colliderRoot.GetComponentsInChildren<Collider>();
			ZLog.Log((object)("Ignoring " + m_attachColliders.Length + " colliders"));
			Collider[] attachColliders = m_attachColliders;
			foreach (Collider val in attachColliders)
			{
				Physics.IgnoreCollision((Collider)(object)m_collider, val, true);
			}
		}
		if (hideWeapons)
		{
			HideHandItems();
		}
		UpdateAttach();
		ResetCloth();
	}

	private void UpdateAttach()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (m_attached)
		{
			if ((Object)(object)m_attachPoint != (Object)null)
			{
				((Component)this).transform.position = m_attachPoint.position;
				((Component)this).transform.rotation = m_attachPoint.rotation;
				Rigidbody componentInParent = ((Component)m_attachPoint).GetComponentInParent<Rigidbody>();
				m_body.useGravity = false;
				m_body.linearVelocity = (Object.op_Implicit((Object)(object)componentInParent) ? componentInParent.GetPointVelocity(((Component)this).transform.position) : Vector3.zero);
				m_body.angularVelocity = Vector3.zero;
				m_maxAirAltitude = ((Component)this).transform.position.y;
			}
			else
			{
				AttachStop();
			}
		}
	}

	public override bool IsAttached()
	{
		if (!m_attached)
		{
			return base.IsAttached();
		}
		return true;
	}

	public Transform GetAttachPoint()
	{
		return m_attachPoint;
	}

	public Transform GetAttachCameraPoint()
	{
		return m_attachPointCamera;
	}

	public void ResetAttachCameraPoint()
	{
		m_attachPointCamera = null;
	}

	public override bool IsAttachedToShip()
	{
		if (m_attached)
		{
			return m_attachedToShip;
		}
		return false;
	}

	public override bool IsRiding()
	{
		if (m_doodadController != null && m_doodadController.IsValid())
		{
			return m_doodadController is Sadle;
		}
		return false;
	}

	public override bool InBed()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.GetZDO().GetBool(ZDOVars.s_inBed);
	}

	public override void AttachStop()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (m_sleeping || !m_attached)
		{
			return;
		}
		if ((Object)(object)m_attachPoint != (Object)null)
		{
			((Component)this).transform.position = m_attachPoint.TransformPoint(m_detachOffset);
		}
		if (m_attachColliders != null)
		{
			Collider[] attachColliders = m_attachColliders;
			foreach (Collider val in attachColliders)
			{
				if (Object.op_Implicit((Object)(object)val))
				{
					Physics.IgnoreCollision((Collider)(object)m_collider, val, false);
				}
			}
			m_attachColliders = null;
		}
		m_body.useGravity = true;
		m_attached = false;
		m_attachPoint = null;
		m_attachPointCamera = null;
		m_zanim.SetBool(m_attachAnimation, value: false);
		m_nview.GetZDO().Set(ZDOVars.s_inBed, value: false);
		ResetCloth();
	}

	public void StartDoodadControl(IDoodadController shipControl)
	{
		m_doodadController = shipControl;
		ZLog.Log((object)("Doodad controlls set " + ((Object)shipControl.GetControlledComponent().gameObject).name));
	}

	public void StopDoodadControl()
	{
		if (m_doodadController != null)
		{
			if (m_doodadController.IsValid())
			{
				m_doodadController.OnUseStop(this);
			}
			ZLog.Log((object)"Stop doodad controlls");
			m_doodadController = null;
		}
	}

	private void SetDoodadControlls(ref Vector3 moveDir, ref Vector3 lookDir, ref bool run, ref bool autoRun, bool block)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (m_doodadController.IsValid())
		{
			m_doodadController.ApplyControlls(moveDir, lookDir, run, autoRun, block);
		}
		moveDir = Vector3.zero;
		autoRun = false;
		run = false;
	}

	public Ship GetControlledShip()
	{
		if (m_doodadController != null && m_doodadController.IsValid())
		{
			return m_doodadController.GetControlledComponent() as Ship;
		}
		return null;
	}

	public IDoodadController GetDoodadController()
	{
		return m_doodadController;
	}

	private void UpdateDoodadControls(float dt)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (m_doodadController == null)
		{
			return;
		}
		if (!m_doodadController.IsValid())
		{
			StopDoodadControl();
			return;
		}
		Vector3 forward = m_doodadController.GetControlledComponent().transform.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		Quaternion val = Quaternion.LookRotation(forward);
		((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val, 100f * dt);
		if (Vector3.Distance(m_doodadController.GetPosition(), ((Component)this).transform.position) > m_maxInteractDistance)
		{
			StopDoodadControl();
		}
	}

	public bool IsSleeping()
	{
		return m_sleeping;
	}

	public void SetSleeping(bool sleep)
	{
		if (m_sleeping != sleep)
		{
			m_sleeping = sleep;
			if (!sleep)
			{
				Message(MessageHud.MessageType.Center, "$msg_goodmorning");
				m_seman.AddStatusEffect(SEMan.s_statusEffectRested, resetTime: true);
				m_wakeupTime = ZNet.instance.GetTimeSeconds();
				Game.instance.IncrementPlayerStat(PlayerStatType.Sleep);
			}
		}
	}

	public void SetControls(Vector3 movedir, bool attack, bool attackHold, bool secondaryAttack, bool secondaryAttackHold, bool block, bool blockHold, bool jump, bool crouch, bool run, bool autoRun, bool dodge = false)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		if ((IsAttached() || InEmote()) && (movedir != Vector3.zero || attack || secondaryAttack || block || blockHold || jump || crouch) && GetDoodadController() == null)
		{
			attack = false;
			attackHold = false;
			secondaryAttack = false;
			secondaryAttackHold = false;
			StopEmote();
			AttachStop();
		}
		if (m_doodadController != null)
		{
			SetDoodadControlls(ref movedir, ref m_lookDir, ref run, ref autoRun, blockHold);
			if (jump || attack || secondaryAttack || dodge)
			{
				attack = false;
				attackHold = false;
				secondaryAttack = false;
				secondaryAttackHold = false;
				StopDoodadControl();
			}
		}
		if (run)
		{
			m_walk = false;
		}
		if (!m_autoRun)
		{
			Vector3 lookDir = m_lookDir;
			lookDir.y = 0f;
			((Vector3)(ref lookDir)).Normalize();
			m_moveDir = movedir.z * lookDir + movedir.x * Vector3.Cross(Vector3.up, lookDir);
		}
		if (!m_autoRun && autoRun && !InPlaceMode())
		{
			m_autoRun = true;
			SetCrouch(crouch: false);
			m_moveDir = m_lookDir;
			m_moveDir.y = 0f;
			((Vector3)(ref m_moveDir)).Normalize();
		}
		else if (m_autoRun)
		{
			if (attack || jump || dodge || crouch || movedir != Vector3.zero || InPlaceMode() || attackHold || secondaryAttackHold)
			{
				m_autoRun = false;
			}
			else if (autoRun || blockHold)
			{
				m_moveDir = m_lookDir;
				m_moveDir.y = 0f;
				((Vector3)(ref m_moveDir)).Normalize();
				blockHold = false;
				block = false;
			}
		}
		m_attack = attack;
		m_attackHold = attackHold;
		m_secondaryAttack = secondaryAttack;
		m_secondaryAttackHold = secondaryAttackHold;
		m_blocking = blockHold;
		m_run = run;
		if (crouch)
		{
			SetCrouch(!m_crouchToggled);
		}
		if ((int)ZInput.InputLayout == 0 || !ZInput.IsGamepadActive())
		{
			if (!jump)
			{
				return;
			}
			if (m_blocking)
			{
				Vector3 dodgeDir = m_moveDir;
				if (((Vector3)(ref dodgeDir)).magnitude < 0.1f)
				{
					dodgeDir = -m_lookDir;
					dodgeDir.y = 0f;
					((Vector3)(ref dodgeDir)).Normalize();
				}
				Dodge(dodgeDir);
			}
			else if (IsCrouching() || m_crouchToggled)
			{
				Vector3 dodgeDir2 = m_moveDir;
				if (((Vector3)(ref dodgeDir2)).magnitude < 0.1f)
				{
					dodgeDir2 = m_lookDir;
					dodgeDir2.y = 0f;
					((Vector3)(ref dodgeDir2)).Normalize();
				}
				Dodge(dodgeDir2);
			}
			else
			{
				Jump();
			}
		}
		else
		{
			if (!ZInput.IsNonClassicFunctionality())
			{
				return;
			}
			if (dodge)
			{
				if (m_blocking)
				{
					Vector3 dodgeDir3 = m_moveDir;
					if (((Vector3)(ref dodgeDir3)).magnitude < 0.1f)
					{
						dodgeDir3 = -m_lookDir;
						dodgeDir3.y = 0f;
						((Vector3)(ref dodgeDir3)).Normalize();
					}
					Dodge(dodgeDir3);
				}
				else if (IsCrouching() || m_crouchToggled)
				{
					Vector3 dodgeDir4 = m_moveDir;
					if (((Vector3)(ref dodgeDir4)).magnitude < 0.1f)
					{
						dodgeDir4 = m_lookDir;
						dodgeDir4.y = 0f;
						((Vector3)(ref dodgeDir4)).Normalize();
					}
					Dodge(dodgeDir4);
				}
			}
			if (jump)
			{
				Jump();
			}
		}
	}

	private void UpdateTargeted(float dt)
	{
		m_timeSinceTargeted += dt;
		m_timeSinceSensed += dt;
	}

	public override void OnTargeted(bool sensed, bool alerted)
	{
		if (sensed)
		{
			if (m_timeSinceSensed > 0.5f)
			{
				m_timeSinceSensed = 0f;
				m_nview.InvokeRPC("OnTargeted", sensed, alerted);
			}
		}
		else if (m_timeSinceTargeted > 0.5f)
		{
			m_timeSinceTargeted = 0f;
			m_nview.InvokeRPC("OnTargeted", sensed, alerted);
		}
	}

	private void RPC_OnTargeted(long sender, bool sensed, bool alerted)
	{
		m_timeSinceTargeted = 0f;
		if (sensed)
		{
			m_timeSinceSensed = 0f;
		}
		if (alerted)
		{
			MusicMan.instance.ResetCombatTimer();
		}
	}

	protected override void OnDamaged(HitData hit)
	{
		base.OnDamaged(hit);
		if (hit.GetTotalDamage() > GetMaxHealth() / 10f)
		{
			Hud.instance.DamageFlash();
		}
	}

	public bool IsTargeted()
	{
		return m_timeSinceTargeted < 1f;
	}

	public bool IsSensed()
	{
		return m_timeSinceSensed < 1f;
	}

	protected override void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
	{
		if (m_chestItem != null)
		{
			mods.Apply(m_chestItem.m_shared.m_damageModifiers);
		}
		if (m_legItem != null)
		{
			mods.Apply(m_legItem.m_shared.m_damageModifiers);
		}
		if (m_helmetItem != null)
		{
			mods.Apply(m_helmetItem.m_shared.m_damageModifiers);
		}
		if (m_shoulderItem != null)
		{
			mods.Apply(m_shoulderItem.m_shared.m_damageModifiers);
		}
	}

	public override float GetBodyArmor()
	{
		float armor = 0f;
		if (m_chestItem != null)
		{
			armor += m_chestItem.GetArmor();
		}
		if (m_legItem != null)
		{
			armor += m_legItem.GetArmor();
		}
		if (m_helmetItem != null)
		{
			armor += m_helmetItem.GetArmor();
		}
		if (m_shoulderItem != null)
		{
			armor += m_shoulderItem.GetArmor();
		}
		m_seman.ApplyArmorMods(ref armor);
		return armor;
	}

	public bool TryGetArmorDifference(ItemDrop.ItemData item, out float difference)
	{
		switch (item.m_shared.m_itemType)
		{
		case ItemDrop.ItemData.ItemType.Helmet:
			if (m_helmetItem == null)
			{
				difference = item.m_shared.m_armor;
			}
			else if (item == m_helmetItem)
			{
				difference = 0f - item.m_shared.m_armor;
			}
			else
			{
				difference = GetBodyArmor() - m_helmetItem.m_shared.m_armor + item.m_shared.m_armor - GetBodyArmor();
			}
			return true;
		case ItemDrop.ItemData.ItemType.Chest:
			if (m_chestItem == null)
			{
				difference = item.m_shared.m_armor;
			}
			else if (item == m_chestItem)
			{
				difference = 0f - item.m_shared.m_armor;
			}
			else
			{
				difference = GetBodyArmor() - m_chestItem.m_shared.m_armor + item.m_shared.m_armor - GetBodyArmor();
			}
			return true;
		case ItemDrop.ItemData.ItemType.Legs:
			if (m_legItem == null)
			{
				difference = item.m_shared.m_armor;
			}
			else if (item == m_legItem)
			{
				difference = 0f - item.m_shared.m_armor;
			}
			else
			{
				difference = GetBodyArmor() - m_legItem.m_shared.m_armor + item.m_shared.m_armor - GetBodyArmor();
			}
			return true;
		case ItemDrop.ItemData.ItemType.Shoulder:
			if (m_shoulderItem == null)
			{
				difference = item.m_shared.m_armor;
			}
			else if (item == m_shoulderItem)
			{
				difference = 0f - item.m_shared.m_armor;
			}
			else
			{
				difference = GetBodyArmor() - m_shoulderItem.m_shared.m_armor + item.m_shared.m_armor - GetBodyArmor();
			}
			return true;
		default:
			difference = 0f;
			return false;
		}
	}

	protected override void OnSneaking(float dt)
	{
		float num = Mathf.Pow(m_skills.GetSkillFactor(Skills.SkillType.Sneak), 0.5f);
		float num2 = Mathf.Lerp(1f, 0.25f, num);
		float num3 = dt * m_sneakStaminaDrain * num2;
		num3 += num3 * GetEquipmentSneakStaminaModifier();
		m_seman.ModifySneakStaminaUsage(num3, ref num3);
		UseStamina(num3);
		if (!HaveStamina())
		{
			Hud.instance.StaminaBarEmptyFlash();
		}
		m_sneakSkillImproveTimer += dt;
		if (m_sneakSkillImproveTimer > 1f)
		{
			m_sneakSkillImproveTimer = 0f;
			if (BaseAI.InStealthRange(this))
			{
				RaiseSkill(Skills.SkillType.Sneak);
			}
			else
			{
				RaiseSkill(Skills.SkillType.Sneak, 0.1f);
			}
		}
	}

	private void UpdateStealth(float dt)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		m_stealthFactorUpdateTimer += dt;
		if (m_stealthFactorUpdateTimer > 0.5f)
		{
			m_stealthFactorUpdateTimer = 0f;
			m_stealthFactorTarget = 0f;
			if (IsCrouching())
			{
				m_lastStealthPosition = ((Component)this).transform.position;
				float skillFactor = m_skills.GetSkillFactor(Skills.SkillType.Sneak);
				float lightFactor = StealthSystem.instance.GetLightFactor(GetCenterPoint());
				m_stealthFactorTarget = Mathf.Lerp(0.5f + lightFactor * 0.5f, 0.2f + lightFactor * 0.4f, skillFactor);
				m_stealthFactorTarget = Mathf.Clamp01(m_stealthFactorTarget);
				m_seman.ModifyStealth(m_stealthFactorTarget, ref m_stealthFactorTarget);
				m_stealthFactorTarget = Mathf.Clamp01(m_stealthFactorTarget);
			}
			else
			{
				m_stealthFactorTarget = 1f;
			}
		}
		float num = Mathf.MoveTowards(m_stealthFactor, m_stealthFactorTarget, dt / 4f);
		if (!m_stealthFactor.Equals(num))
		{
			m_nview.GetZDO().Set(ZDOVars.s_stealth, num);
		}
		m_stealthFactor = num;
	}

	public override float GetStealthFactor()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		if (m_nview.IsOwner())
		{
			return m_stealthFactor;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_stealth);
	}

	public override bool InAttack()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (MonoUpdaters.UpdateCount == m_cachedFrame)
		{
			return m_cachedAttack;
		}
		m_cachedFrame = MonoUpdaters.UpdateCount;
		if (GetNextOrCurrentAnimHash() == Humanoid.s_animatorTagAttack)
		{
			m_cachedAttack = true;
			return true;
		}
		for (int i = 1; i < m_animator.layerCount; i++)
		{
			AnimatorStateInfo val;
			int tagHash;
			if (!m_animator.IsInTransition(i))
			{
				val = m_animator.GetCurrentAnimatorStateInfo(i);
				tagHash = ((AnimatorStateInfo)(ref val)).tagHash;
			}
			else
			{
				val = m_animator.GetNextAnimatorStateInfo(i);
				tagHash = ((AnimatorStateInfo)(ref val)).tagHash;
			}
			if (tagHash == Humanoid.s_animatorTagAttack)
			{
				m_cachedAttack = true;
				return true;
			}
		}
		m_cachedAttack = false;
		return false;
	}

	private float GetEquipmentModifier(int index)
	{
		if (m_equipmentModifierValues != null)
		{
			return m_equipmentModifierValues[index];
		}
		return 0f;
	}

	public override float GetEquipmentMovementModifier()
	{
		return GetEquipmentModifier(0);
	}

	public override float GetEquipmentHomeItemModifier()
	{
		return GetEquipmentModifier(1);
	}

	public override float GetEquipmentHeatResistanceModifier()
	{
		return GetEquipmentModifier(2);
	}

	public override float GetEquipmentJumpStaminaModifier()
	{
		return GetEquipmentModifier(3);
	}

	public override float GetEquipmentAttackStaminaModifier()
	{
		return GetEquipmentModifier(4);
	}

	public override float GetEquipmentBlockStaminaModifier()
	{
		return GetEquipmentModifier(5);
	}

	public override float GetEquipmentDodgeStaminaModifier()
	{
		return GetEquipmentModifier(6);
	}

	public override float GetEquipmentSwimStaminaModifier()
	{
		return GetEquipmentModifier(7);
	}

	public override float GetEquipmentSneakStaminaModifier()
	{
		return GetEquipmentModifier(8);
	}

	public override float GetEquipmentRunStaminaModifier()
	{
		return GetEquipmentModifier(9);
	}

	public override float GetEquipmentMaxAdrenaline()
	{
		return GetEquipmentModifier(10);
	}

	private float GetEquipmentModifierPlusSE(int index)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		float staminaUse = m_equipmentModifierValues[index];
		switch (index)
		{
		case 3:
			m_seman.ModifyJumpStaminaUsage(1f, ref staminaUse, minZero: false);
			break;
		case 4:
			m_seman.ModifyAttackStaminaUsage(1f, ref staminaUse, minZero: false);
			break;
		case 5:
			m_seman.ModifyBlockStaminaUsage(1f, ref staminaUse, minZero: false);
			break;
		case 6:
			m_seman.ModifyDodgeStaminaUsage(1f, ref staminaUse, minZero: false);
			break;
		case 7:
			m_seman.ModifySwimStaminaUsage(1f, ref staminaUse, minZero: false);
			break;
		case 8:
			m_seman.ModifySneakStaminaUsage(1f, ref staminaUse, minZero: false);
			break;
		case 9:
			m_seman.ModifyRunStaminaDrain(1f, ref staminaUse, Vector3.zero, minZero: false);
			break;
		}
		return staminaUse;
	}

	protected override float GetJogSpeedFactor()
	{
		return 1f + GetEquipmentMovementModifier();
	}

	protected override float GetRunSpeedFactor()
	{
		float skillFactor = m_skills.GetSkillFactor(Skills.SkillType.Run);
		return (1f + skillFactor * 0.25f) * (1f + GetEquipmentMovementModifier() * 1.5f);
	}

	public override bool InMinorAction()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		AnimatorStateInfo val = m_animator.GetCurrentAnimatorStateInfo(1);
		int tagHash = ((AnimatorStateInfo)(ref val)).tagHash;
		if (tagHash == s_animatorTagMinorAction || tagHash == s_animatorTagMinorActionFast)
		{
			return true;
		}
		if (m_animator.IsInTransition(1))
		{
			val = m_animator.GetNextAnimatorStateInfo(1);
			int tagHash2 = ((AnimatorStateInfo)(ref val)).tagHash;
			if (tagHash2 != s_animatorTagMinorAction)
			{
				return tagHash2 == s_animatorTagMinorActionFast;
			}
			return true;
		}
		return false;
	}

	public override bool InMinorActionSlowdown()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		AnimatorStateInfo val = m_animator.GetCurrentAnimatorStateInfo(1);
		if (((AnimatorStateInfo)(ref val)).tagHash == s_animatorTagMinorAction)
		{
			return true;
		}
		if (m_animator.IsInTransition(1))
		{
			val = m_animator.GetNextAnimatorStateInfo(1);
			return ((AnimatorStateInfo)(ref val)).tagHash == s_animatorTagMinorAction;
		}
		return false;
	}

	public override bool GetRelativePosition(out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (m_attached && Object.op_Implicit((Object)(object)m_attachPoint))
		{
			ZNetView componentInParent = ((Component)m_attachPoint).GetComponentInParent<ZNetView>();
			if (Object.op_Implicit((Object)(object)componentInParent) && componentInParent.IsValid())
			{
				parent = componentInParent.GetZDO().m_uid;
				if ((Object)(object)((Component)componentInParent).GetComponent<Character>() != (Object)null)
				{
					attachJoint = ((Object)m_attachPoint).name;
					relativePos = Vector3.zero;
					relativeRot = Quaternion.identity;
				}
				else
				{
					attachJoint = "";
					relativePos = ((Component)componentInParent).transform.InverseTransformPoint(((Component)this).transform.position);
					relativeRot = Quaternion.Inverse(((Component)componentInParent).transform.rotation) * ((Component)this).transform.rotation;
				}
				relativeVel = Vector3.zero;
				return true;
			}
		}
		return base.GetRelativePosition(out parent, out attachJoint, out relativePos, out relativeRot, out relativeVel);
	}

	public override Skills GetSkills()
	{
		return m_skills;
	}

	public override float GetRandomSkillFactor(Skills.SkillType skill)
	{
		return m_skills.GetRandomSkillFactor(skill);
	}

	public override float GetSkillFactor(Skills.SkillType skill)
	{
		return m_skills.GetSkillFactor(skill);
	}

	protected override void DoDamageCameraShake(HitData hit)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float totalStaggerDamage = hit.m_damage.GetTotalStaggerDamage();
		if (Object.op_Implicit((Object)(object)GameCamera.instance) && totalStaggerDamage > 0f)
		{
			float num = Mathf.Clamp01(totalStaggerDamage / GetMaxHealth());
			GameCamera.instance.AddShake(((Component)this).transform.position, 50f, m_baseCameraShake * num, continous: false);
		}
	}

	protected override void DamageArmorDurability(HitData hit)
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		if (m_chestItem != null)
		{
			list.Add(m_chestItem);
		}
		if (m_legItem != null)
		{
			list.Add(m_legItem);
		}
		if (m_helmetItem != null)
		{
			list.Add(m_helmetItem);
		}
		if (m_shoulderItem != null)
		{
			list.Add(m_shoulderItem);
		}
		if (list.Count != 0)
		{
			float num = hit.GetTotalPhysicalDamage() + hit.GetTotalElementalDamage();
			if (!(num <= 0f))
			{
				int index = Random.Range(0, list.Count);
				ItemDrop.ItemData itemData = list[index];
				itemData.m_durability = Mathf.Max(0f, itemData.m_durability - num);
			}
		}
	}

	protected override bool ToggleEquipped(ItemDrop.ItemData item)
	{
		if (!item.IsEquipable())
		{
			return false;
		}
		if (InAttack())
		{
			return true;
		}
		if (item.m_shared.m_equipDuration <= 0f)
		{
			if (IsItemEquiped(item))
			{
				UnequipItem(item);
			}
			else
			{
				EquipItem(item);
			}
		}
		else if (IsItemEquiped(item))
		{
			QueueUnequipAction(item);
		}
		else
		{
			QueueEquipAction(item);
		}
		return true;
	}

	public void GetActionProgress(out string name, out float progress, out MinorActionData data)
	{
		if (TryGetFirstElementProgress(out var firstElement, out var progress2))
		{
			data = firstElement;
			name = firstElement.m_progressText;
			progress = progress2;
		}
		else
		{
			data = null;
			name = null;
			progress = 0f;
		}
	}

	public void GetActionProgress(out string name, out float progress)
	{
		GetActionProgress(out name, out progress, out var _);
	}

	private bool TryGetFirstElementProgress(out MinorActionData firstElement, out float progress)
	{
		firstElement = null;
		progress = 0f;
		if (m_actionQueue.Count > 0)
		{
			firstElement = m_actionQueue[0];
			if (firstElement.m_duration > 0f)
			{
				progress = Mathf.Clamp01(firstElement.m_time / firstElement.m_duration);
			}
			return true;
		}
		return false;
	}

	public int GetActionQueueCount()
	{
		return m_actionQueue.Count;
	}

	private void UpdateActionQueue(float dt)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if (m_actionQueuePause > 0f)
		{
			m_actionQueuePause -= dt;
			if (m_actionAnimation != null)
			{
				m_zanim.SetBool(m_actionAnimation, value: false);
				m_actionAnimation = null;
			}
			return;
		}
		if (InAttack())
		{
			if (m_actionAnimation != null)
			{
				m_zanim.SetBool(m_actionAnimation, value: false);
				m_actionAnimation = null;
			}
			return;
		}
		if (m_actionQueue.Count == 0)
		{
			if (m_actionAnimation != null)
			{
				m_zanim.SetBool(m_actionAnimation, value: false);
				m_actionAnimation = null;
			}
			return;
		}
		MinorActionData minorActionData = m_actionQueue[0];
		if (m_actionAnimation != null && m_actionAnimation != minorActionData.m_animation)
		{
			m_zanim.SetBool(m_actionAnimation, value: false);
			m_actionAnimation = null;
		}
		m_zanim.SetBool(minorActionData.m_animation, value: true);
		m_actionAnimation = minorActionData.m_animation;
		if (minorActionData.m_time == 0f && minorActionData.m_startEffect != null)
		{
			minorActionData.m_startEffect.Create(((Component)this).transform.position, Quaternion.identity);
		}
		if (minorActionData.m_staminaDrain > 0f)
		{
			UseStamina(minorActionData.m_staminaDrain * dt);
		}
		if (minorActionData.m_eitrDrain > 0f)
		{
			UseEitr(minorActionData.m_eitrDrain * dt);
		}
		minorActionData.m_time += dt;
		if (minorActionData.m_time > minorActionData.m_duration)
		{
			m_actionQueue.RemoveAt(0);
			m_zanim.SetBool(m_actionAnimation, value: false);
			m_actionAnimation = null;
			if (!string.IsNullOrEmpty(minorActionData.m_doneAnimation))
			{
				m_zanim.SetTrigger(minorActionData.m_doneAnimation);
			}
			switch (minorActionData.m_type)
			{
			case MinorActionData.ActionType.Equip:
				EquipItem(minorActionData.m_item);
				break;
			case MinorActionData.ActionType.Unequip:
				UnequipItem(minorActionData.m_item);
				break;
			case MinorActionData.ActionType.Reload:
				SetWeaponLoaded(minorActionData.m_item);
				break;
			}
			m_actionQueuePause = 0.3f;
		}
	}

	private void QueueEquipAction(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		if (IsEquipActionQueued(item))
		{
			RemoveEquipAction(item);
			return;
		}
		CancelReloadAction();
		MinorActionData minorActionData = new MinorActionData();
		minorActionData.m_item = item;
		minorActionData.m_type = MinorActionData.ActionType.Equip;
		minorActionData.m_duration = item.m_shared.m_equipDuration;
		minorActionData.m_progressText = "$hud_equipping " + item.m_shared.m_name;
		minorActionData.m_animation = "equipping";
		if (minorActionData.m_duration >= 1f)
		{
			minorActionData.m_startEffect = m_equipStartEffects;
		}
		m_actionQueue.Add(minorActionData);
	}

	private void QueueUnequipAction(ItemDrop.ItemData item)
	{
		if (item != null)
		{
			if (IsEquipActionQueued(item))
			{
				RemoveEquipAction(item);
				return;
			}
			CancelReloadAction();
			MinorActionData minorActionData = new MinorActionData();
			minorActionData.m_item = item;
			minorActionData.m_type = MinorActionData.ActionType.Unequip;
			minorActionData.m_duration = item.m_shared.m_equipDuration;
			minorActionData.m_progressText = "$hud_unequipping " + item.m_shared.m_name;
			minorActionData.m_animation = "equipping";
			m_actionQueue.Add(minorActionData);
		}
	}

	private void QueueReloadAction()
	{
		if (!IsReloadActionQueued())
		{
			ItemDrop.ItemData currentWeapon = GetCurrentWeapon();
			if (currentWeapon != null && currentWeapon.m_shared.m_attack.m_requiresReload)
			{
				MinorActionData minorActionData = new MinorActionData();
				minorActionData.m_item = currentWeapon;
				minorActionData.m_type = MinorActionData.ActionType.Reload;
				minorActionData.m_duration = currentWeapon.GetWeaponLoadingTime();
				minorActionData.m_progressText = "$hud_reloading " + currentWeapon.m_shared.m_name;
				minorActionData.m_animation = currentWeapon.m_shared.m_attack.m_reloadAnimation;
				minorActionData.m_doneAnimation = currentWeapon.m_shared.m_attack.m_reloadAnimation + "_done";
				minorActionData.m_staminaDrain = currentWeapon.m_shared.m_attack.m_reloadStaminaDrain;
				minorActionData.m_eitrDrain = currentWeapon.m_shared.m_attack.m_reloadEitrDrain;
				m_actionQueue.Add(minorActionData);
			}
		}
	}

	protected override void ClearActionQueue()
	{
		m_actionQueue.Clear();
	}

	public override void RemoveEquipAction(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		foreach (MinorActionData item2 in m_actionQueue)
		{
			if (item2.m_item == item)
			{
				m_actionQueue.Remove(item2);
				break;
			}
		}
	}

	public bool IsEquipActionQueued(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return false;
		}
		foreach (MinorActionData item2 in m_actionQueue)
		{
			if ((item2.m_type == MinorActionData.ActionType.Equip || item2.m_type == MinorActionData.ActionType.Unequip) && item2.m_item == item)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsReloadActionQueued()
	{
		foreach (MinorActionData item in m_actionQueue)
		{
			if (item.m_type == MinorActionData.ActionType.Reload)
			{
				return true;
			}
		}
		return false;
	}

	public void ResetCharacter()
	{
		m_guardianPowerCooldown = 0f;
		ResetSeenTutorials();
		m_knownRecipes.Clear();
		m_knownStations.Clear();
		m_knownMaterial.Clear();
		m_uniques.Clear();
		m_trophies.Clear();
		m_skills.Clear();
		m_knownBiome.Clear();
		m_knownTexts.Clear();
	}

	public void ResetCharacterKnownItems()
	{
		m_knownRecipes.Clear();
		m_knownStations.Clear();
		m_knownMaterial.Clear();
		m_trophies.Clear();
	}

	public bool ToggleDebugFly()
	{
		m_debugFly = !m_debugFly;
		m_nview.GetZDO().Set(ZDOVars.s_debugFly, m_debugFly);
		Message(MessageHud.MessageType.TopLeft, "Debug fly:" + m_debugFly);
		return m_debugFly;
	}

	public void SetNoPlacementCost(bool value)
	{
		if (value != m_noPlacementCost)
		{
			ToggleNoPlacementCost();
		}
	}

	public bool ToggleNoPlacementCost()
	{
		m_noPlacementCost = !m_noPlacementCost;
		Message(MessageHud.MessageType.TopLeft, "No placement cost:" + m_noPlacementCost);
		UpdateAvailablePiecesList();
		return m_noPlacementCost;
	}

	public bool IsKnownMaterial(string name)
	{
		return m_knownMaterial.Contains(name);
	}
}
