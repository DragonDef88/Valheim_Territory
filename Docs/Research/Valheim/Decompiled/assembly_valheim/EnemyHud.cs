using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyHud : MonoBehaviour
{
	private class HudData
	{
		public Character m_character;

		public BaseAI m_ai;

		public GameObject m_gui;

		public RectTransform m_level2;

		public RectTransform m_level3;

		public RectTransform m_alerted;

		public RectTransform m_aware;

		public GuiBar m_healthFast;

		public GuiBar m_healthFastFriendly;

		public GuiBar m_healthSlow;

		public TextMeshProUGUI m_healthText;

		public GuiBar m_stamina;

		public TextMeshProUGUI m_staminaText;

		public TextMeshProUGUI m_name;

		public float m_hoverTimer = 99999f;

		public bool m_isMount;
	}

	private static EnemyHud m_instance;

	public GameObject m_hudRoot;

	public GameObject m_baseHud;

	public GameObject m_baseHudBoss;

	public GameObject m_baseHudPlayer;

	public GameObject m_baseHudMount;

	public float m_maxShowDistance = 10f;

	public float m_maxShowDistanceBoss = 100f;

	public float m_hoverShowDuration = 60f;

	private Vector3 m_refPoint = Vector3.zero;

	private Dictionary<Character, HudData> m_huds = new Dictionary<Character, HudData>();

	public static EnemyHud instance => m_instance;

	private void Awake()
	{
		m_instance = this;
		m_baseHud.SetActive(false);
		m_baseHudBoss.SetActive(false);
		m_baseHudPlayer.SetActive(false);
		m_baseHudMount.SetActive(false);
	}

	private void OnDestroy()
	{
		m_instance = null;
	}

	private void LateUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		m_hudRoot.SetActive(!Hud.IsUserHidden());
		Sadle sadle = null;
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null)
		{
			m_refPoint = ((Component)localPlayer).transform.position;
			sadle = localPlayer.GetDoodadController() as Sadle;
		}
		foreach (Character allCharacter in Character.GetAllCharacters())
		{
			if (!((Object)(object)allCharacter == (Object)(object)localPlayer) && (!Object.op_Implicit((Object)(object)sadle) || !((Object)(object)allCharacter == (Object)(object)sadle.GetCharacter())) && TestShow(allCharacter, isVisible: false))
			{
				bool isMount = Object.op_Implicit((Object)(object)sadle) && (Object)(object)allCharacter == (Object)(object)sadle.GetCharacter();
				ShowHud(allCharacter, isMount);
			}
		}
		UpdateHuds(localPlayer, sadle, Time.deltaTime);
	}

	private bool TestShow(Character c, bool isVisible)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.SqrMagnitude(((Component)c).transform.position - m_refPoint);
		if (c.IsBoss() && num < m_maxShowDistanceBoss * m_maxShowDistanceBoss)
		{
			if (isVisible && c.m_dontHideBossHud)
			{
				return true;
			}
			if (((Component)c).GetComponent<BaseAI>().IsAlerted())
			{
				return true;
			}
		}
		else if (num < m_maxShowDistance * m_maxShowDistance)
		{
			if (c.IsPlayer() && c.IsCrouching())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private void ShowHud(Character c, bool isMount)
	{
		if (!m_huds.TryGetValue(c, out var value))
		{
			GameObject val = (isMount ? m_baseHudMount : (c.IsPlayer() ? m_baseHudPlayer : ((!c.IsBoss()) ? m_baseHud : m_baseHudBoss)));
			value = new HudData();
			value.m_character = c;
			value.m_ai = ((Component)c).GetComponent<BaseAI>();
			value.m_gui = Object.Instantiate<GameObject>(val, m_hudRoot.transform);
			value.m_gui.SetActive(true);
			value.m_healthFast = ((Component)value.m_gui.transform.Find("Health/health_fast")).GetComponent<GuiBar>();
			value.m_healthSlow = ((Component)value.m_gui.transform.Find("Health/health_slow")).GetComponent<GuiBar>();
			Transform val2 = value.m_gui.transform.Find("Health/health_fast_friendly");
			if (Object.op_Implicit((Object)(object)val2))
			{
				value.m_healthFastFriendly = ((Component)val2).GetComponent<GuiBar>();
			}
			if (isMount)
			{
				value.m_stamina = ((Component)value.m_gui.transform.Find("Stamina/stamina_fast")).GetComponent<GuiBar>();
				value.m_staminaText = ((Component)value.m_gui.transform.Find("Stamina/StaminaText")).GetComponent<TextMeshProUGUI>();
				value.m_healthText = ((Component)value.m_gui.transform.Find("Health/HealthText")).GetComponent<TextMeshProUGUI>();
			}
			ref RectTransform level = ref value.m_level2;
			Transform obj = value.m_gui.transform.Find("level_2");
			level = (RectTransform)(object)((obj is RectTransform) ? obj : null);
			ref RectTransform level2 = ref value.m_level3;
			Transform obj2 = value.m_gui.transform.Find("level_3");
			level2 = (RectTransform)(object)((obj2 is RectTransform) ? obj2 : null);
			ref RectTransform alerted = ref value.m_alerted;
			Transform obj3 = value.m_gui.transform.Find("Alerted");
			alerted = (RectTransform)(object)((obj3 is RectTransform) ? obj3 : null);
			ref RectTransform aware = ref value.m_aware;
			Transform obj4 = value.m_gui.transform.Find("Aware");
			aware = (RectTransform)(object)((obj4 is RectTransform) ? obj4 : null);
			value.m_name = ((Component)value.m_gui.transform.Find("Name")).GetComponent<TextMeshProUGUI>();
			((TMP_Text)value.m_name).text = Localization.instance.Localize(c.GetHoverName());
			value.m_isMount = isMount;
			m_huds.Add(c, value);
		}
	}

	private void UpdateHuds(Player player, Sadle sadle, float dt)
	{
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (!Object.op_Implicit((Object)(object)mainCamera))
		{
			return;
		}
		Character character = (Object.op_Implicit((Object)(object)sadle) ? sadle.GetCharacter() : null);
		Character character2 = (Object.op_Implicit((Object)(object)player) ? player.GetHoverCreature() : null);
		Character character3 = null;
		foreach (KeyValuePair<Character, HudData> hud in m_huds)
		{
			HudData value = hud.Value;
			if (!Object.op_Implicit((Object)(object)value.m_character) || !TestShow(value.m_character, isVisible: true) || (Object)(object)value.m_character == (Object)(object)character)
			{
				if ((Object)(object)character3 == (Object)null)
				{
					character3 = value.m_character;
					Object.Destroy((Object)(object)value.m_gui);
				}
				continue;
			}
			if ((Object)(object)value.m_character == (Object)(object)character2)
			{
				value.m_hoverTimer = 0f;
			}
			value.m_hoverTimer += dt;
			float healthPercentage = value.m_character.GetHealthPercentage();
			if (value.m_character.IsPlayer() || value.m_character.IsBoss() || value.m_isMount || value.m_hoverTimer < m_hoverShowDuration)
			{
				value.m_gui.SetActive(true);
				int level = value.m_character.GetLevel();
				if (Object.op_Implicit((Object)(object)value.m_level2))
				{
					((Component)value.m_level2).gameObject.SetActive(level == 2);
				}
				if (Object.op_Implicit((Object)(object)value.m_level3))
				{
					((Component)value.m_level3).gameObject.SetActive(level == 3);
				}
				((TMP_Text)value.m_name).text = Localization.instance.Localize(value.m_character.GetHoverName());
				if (!value.m_character.IsBoss() && !value.m_character.IsPlayer())
				{
					bool flag = value.m_character.GetBaseAI().HaveTarget();
					bool flag2 = value.m_character.GetBaseAI().IsAlerted();
					((Component)value.m_alerted).gameObject.SetActive(flag2);
					((Component)value.m_aware).gameObject.SetActive(!flag2 && flag);
				}
			}
			else
			{
				value.m_gui.SetActive(false);
			}
			value.m_healthSlow.SetValue(healthPercentage);
			if (Object.op_Implicit((Object)(object)value.m_healthFastFriendly))
			{
				bool flag3 = !Object.op_Implicit((Object)(object)player) || BaseAI.IsEnemy(player, value.m_character);
				((Component)value.m_healthFast).gameObject.SetActive(flag3);
				((Component)value.m_healthFastFriendly).gameObject.SetActive(!flag3);
				value.m_healthFast.SetValue(healthPercentage);
				value.m_healthFastFriendly.SetValue(healthPercentage);
			}
			else
			{
				value.m_healthFast.SetValue(healthPercentage);
			}
			if (value.m_isMount)
			{
				float stamina = sadle.GetStamina();
				float maxStamina = sadle.GetMaxStamina();
				value.m_stamina.SetValue(stamina / maxStamina);
				((TMP_Text)value.m_healthText).text = Mathf.CeilToInt(value.m_character.GetHealth()).ToString();
				((TMP_Text)value.m_staminaText).text = Mathf.CeilToInt(stamina).ToString();
			}
			if (!value.m_character.IsBoss() && value.m_gui.activeSelf)
			{
				Vector3 zero = Vector3.zero;
				zero = (value.m_character.IsPlayer() ? (value.m_character.GetHeadPoint() + Vector3.up * 0.3f) : ((!value.m_isMount) ? value.m_character.GetTopPoint() : (((Component)player).transform.position - ((Component)player).transform.up * 0.5f)));
				Vector3 val = Utils.WorldToScreenPointScaled(mainCamera, zero);
				if (val.x < 0f || val.x > (float)Screen.width || val.y < 0f || val.y > (float)Screen.height || val.z > 0f)
				{
					value.m_gui.transform.position = val;
					value.m_gui.SetActive(true);
				}
				else
				{
					value.m_gui.SetActive(false);
				}
			}
		}
		if ((Object)(object)character3 != (Object)null)
		{
			m_huds.Remove(character3);
		}
	}

	public bool ShowingBossHud()
	{
		foreach (KeyValuePair<Character, HudData> hud in m_huds)
		{
			if (Object.op_Implicit((Object)(object)hud.Value.m_character) && hud.Value.m_character.IsBoss())
			{
				return true;
			}
		}
		return false;
	}

	public Character GetActiveBoss()
	{
		foreach (KeyValuePair<Character, HudData> hud in m_huds)
		{
			if (Object.op_Implicit((Object)(object)hud.Value.m_character) && hud.Value.m_character.IsBoss())
			{
				return hud.Value.m_character;
			}
		}
		return null;
	}

	public void RemoveCharacterHud(Character character)
	{
		if (m_huds.ContainsKey(character))
		{
			if ((Object)(object)m_huds[character].m_gui != (Object)null)
			{
				Object.Destroy((Object)(object)m_huds[character].m_gui);
			}
			m_huds.Remove(character);
		}
	}
}
