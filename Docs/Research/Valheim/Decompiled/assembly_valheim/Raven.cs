using System;
using System.Collections.Generic;
using UnityEngine;

public class Raven : MonoBehaviour, Hoverable, Interactable, IDestructible
{
	[Serializable]
	public class RavenText
	{
		public bool m_alwaysSpawn = true;

		public bool m_munin;

		public int m_priority;

		public string m_key = "";

		public string m_topic = "";

		public string m_label = "";

		[TextArea]
		public string m_text = "";

		[NonSerialized]
		public bool m_static;

		[NonSerialized]
		public GuidePoint m_guidePoint;
	}

	public GameObject m_visual;

	public GameObject m_exclamation;

	public string m_name = "Name";

	public bool m_isMunin;

	public bool m_autoTalk = true;

	public float m_idleEffectIntervalMin = 10f;

	public float m_idleEffectIntervalMax = 20f;

	public float m_spawnDistance = 15f;

	public float m_despawnDistance = 20f;

	public float m_autoTalkDistance = 3f;

	public float m_enemyCheckDistance = 10f;

	public float m_rotateSpeed = 10f;

	public float m_minRotationAngle = 15f;

	public float m_dialogVisibleTime = 10f;

	public float m_longDialogVisibleTime = 10f;

	public float m_dontFlyDistance = 3f;

	public float m_textOffset = 1.5f;

	public float m_textCullDistance = 20f;

	public float m_randomTextInterval = 30f;

	public float m_randomTextIntervalImportant = 10f;

	public List<string> m_randomTextsImportant = new List<string>();

	public List<string> m_randomTexts = new List<string>();

	public EffectList m_idleEffect = new EffectList();

	public EffectList m_despawnEffect = new EffectList();

	private RavenText m_currentText;

	private GameObject m_groundObject;

	private Animator m_animator;

	private Collider m_collider;

	private bool m_hasTalked;

	private float m_randomTextTimer = 9999f;

	private float m_timeSinceTeleport = 9999f;

	private static List<RavenText> m_tempTexts = new List<RavenText>();

	private static List<RavenText> m_staticTexts = new List<RavenText>();

	private static Raven m_instance = null;

	public static bool m_tutorialsEnabled = true;

	public static bool IsInstantiated()
	{
		return (Object)(object)m_instance != (Object)null;
	}

	private void Awake()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = new Vector3(0f, 100000f, 0f);
		m_instance = this;
		m_animator = m_visual.GetComponentInChildren<Animator>();
		m_collider = ((Component)this).GetComponent<Collider>();
		((MonoBehaviour)this).InvokeRepeating("IdleEffect", Random.Range(m_idleEffectIntervalMin, m_idleEffectIntervalMax), Random.Range(m_idleEffectIntervalMin, m_idleEffectIntervalMax));
		((MonoBehaviour)this).InvokeRepeating("CheckSpawn", 1f, 1f);
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_instance == (Object)(object)this)
		{
			m_instance = null;
		}
	}

	public string GetHoverText()
	{
		if (IsSpawned())
		{
			return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $raven_interact");
		}
		return "";
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_name);
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (m_hasTalked && Chat.instance.IsDialogVisible(((Component)this).gameObject))
		{
			Chat.instance.ClearNpcText(((Component)this).gameObject);
		}
		else
		{
			Talk();
		}
		return false;
	}

	private void Talk()
	{
		if (Object.op_Implicit((Object)(object)Player.m_localPlayer) && m_currentText != null)
		{
			if (m_currentText.m_key.Length > 0)
			{
				Player.m_localPlayer.SetSeenTutorial(m_currentText.m_key);
				Gogan.LogEvent("Game", "Raven", m_currentText.m_key, 0L);
			}
			else
			{
				Gogan.LogEvent("Game", "Raven", m_currentText.m_topic, 0L);
			}
			m_hasTalked = true;
			if (m_currentText.m_label.Length > 0)
			{
				Player.m_localPlayer.AddKnownText(m_currentText.m_label, m_currentText.m_text);
			}
			Say(m_currentText.m_topic, m_currentText.m_text, showName: false, longTimeout: true, large: true);
			Game.instance.IncrementPlayerStat(PlayerStatType.RavenTalk);
		}
	}

	private void Say(string topic, string text, bool showName, bool longTimeout, bool large)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (topic.Length > 0)
		{
			text = "<color=orange>" + topic + "</color>\n" + text;
		}
		Chat.instance.SetNpcText(((Component)this).gameObject, Vector3.up * m_textOffset, m_textCullDistance, longTimeout ? m_longDialogVisibleTime : m_dialogVisibleTime, showName ? m_name : "", text, large);
		m_animator.SetTrigger("talk");
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void IdleEffect()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (IsSpawned())
		{
			m_idleEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			((MonoBehaviour)this).CancelInvoke("IdleEffect");
			((MonoBehaviour)this).InvokeRepeating("IdleEffect", Random.Range(m_idleEffectIntervalMin, m_idleEffectIntervalMax), Random.Range(m_idleEffectIntervalMin, m_idleEffectIntervalMax));
		}
	}

	private bool CanHide()
	{
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return true;
		}
		if (Chat.instance.IsDialogVisible(((Component)this).gameObject))
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		m_timeSinceTeleport += Time.deltaTime;
		if (!IsAway() && !IsFlying() && Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			Vector3 val = ((Component)Player.m_localPlayer).transform.position - ((Component)this).transform.position;
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			float num = Vector3.SignedAngle(((Component)this).transform.forward, val, Vector3.up);
			if (Mathf.Abs(num) > m_minRotationAngle)
			{
				m_animator.SetFloat("anglevel", m_rotateSpeed * Mathf.Sign(num), 0.4f, Time.deltaTime);
				((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, Quaternion.LookRotation(val), Time.deltaTime * m_rotateSpeed);
			}
			else
			{
				m_animator.SetFloat("anglevel", 0f, 0.4f, Time.deltaTime);
			}
		}
		if (IsSpawned())
		{
			if ((Object)(object)Player.m_localPlayer != (Object)null && !Chat.instance.IsDialogVisible(((Component)this).gameObject) && Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)this).transform.position) < m_autoTalkDistance)
			{
				m_randomTextTimer += Time.deltaTime;
				float num2 = (m_hasTalked ? m_randomTextInterval : m_randomTextIntervalImportant);
				if (m_randomTextTimer >= num2)
				{
					m_randomTextTimer = 0f;
					if (m_hasTalked)
					{
						Say("", m_randomTexts[Random.Range(0, m_randomTexts.Count)], showName: false, longTimeout: false, large: false);
					}
					else
					{
						Say("", m_randomTextsImportant[Random.Range(0, m_randomTextsImportant.Count)], showName: false, longTimeout: false, large: false);
					}
				}
			}
			if (((Object)(object)Player.m_localPlayer == (Object)null || Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)this).transform.position) > m_despawnDistance || EnemyNearby(((Component)this).transform.position) || RandEventSystem.InEvent() || m_currentText == null || (Object)(object)m_groundObject == (Object)null || m_hasTalked) && CanHide())
			{
				bool forceTeleport = GetBestText() != null || (Object)(object)m_groundObject == (Object)null;
				FlyAway(forceTeleport);
				RestartSpawnCheck(3f);
			}
			m_exclamation.SetActive(!m_hasTalked);
		}
		else
		{
			m_exclamation.SetActive(false);
		}
	}

	private bool FindSpawnPoint(out Vector3 point, out GameObject landOn)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)Player.m_localPlayer).transform.position;
		Vector3 forward = ((Component)Utils.GetMainCamera()).transform.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		point = new Vector3(0f, -999f, 0f);
		landOn = null;
		bool result = false;
		for (int i = 0; i < 20; i++)
		{
			Vector3 val = Quaternion.Euler(0f, (float)Random.Range(-30, 30), 0f) * forward;
			Vector3 val2 = position + val * Random.Range(m_spawnDistance - 5f, m_spawnDistance);
			if (ZoneSystem.instance.GetSolidHeight(val2, out var height, out var normal, out var go) && height > 30f && height > point.y && height < 2000f && normal.y > 0.5f && Mathf.Abs(height - position.y) < 2f)
			{
				val2.y = height;
				point = val2;
				landOn = go;
				result = true;
			}
		}
		return result;
	}

	private bool EnemyNearby(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return LootSpawner.IsMonsterInRange(point, m_enemyCheckDistance);
	}

	private bool InState(string name)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!m_animator.isInitialized)
		{
			return false;
		}
		AnimatorStateInfo val = m_animator.GetCurrentAnimatorStateInfo(0);
		if (((AnimatorStateInfo)(ref val)).IsTag(name))
		{
			return true;
		}
		val = m_animator.GetNextAnimatorStateInfo(0);
		if (((AnimatorStateInfo)(ref val)).IsTag(name))
		{
			return true;
		}
		return false;
	}

	private RavenText GetBestText()
	{
		RavenText ravenText = GetTempText();
		RavenText closestStaticText = GetClosestStaticText(m_spawnDistance);
		if (closestStaticText != null && (ravenText == null || closestStaticText.m_priority >= ravenText.m_priority))
		{
			ravenText = closestStaticText;
		}
		return ravenText;
	}

	private RavenText GetTempText()
	{
		foreach (RavenText tempText in m_tempTexts)
		{
			if (tempText.m_munin == m_isMunin)
			{
				return tempText;
			}
		}
		return null;
	}

	private RavenText GetClosestStaticText(float maxDistance)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return null;
		}
		RavenText ravenText = null;
		float num = 9999f;
		bool flag = false;
		Vector3 position = ((Component)Player.m_localPlayer).transform.position;
		foreach (RavenText staticText in m_staticTexts)
		{
			if (staticText.m_munin != m_isMunin || !Object.op_Implicit((Object)(object)staticText.m_guidePoint))
			{
				continue;
			}
			float num2 = Vector3.Distance(position, ((Component)staticText.m_guidePoint).transform.position);
			if (!(num2 < maxDistance))
			{
				continue;
			}
			bool flag2 = staticText.m_key.Length > 0 && Player.m_localPlayer.HaveSeenTutorial(staticText.m_key);
			if (!staticText.m_alwaysSpawn && flag2)
			{
				continue;
			}
			if (ravenText == null)
			{
				ravenText = staticText;
				num = num2;
				flag = flag2;
			}
			else if (flag2 == flag)
			{
				if (staticText.m_priority == ravenText.m_priority || flag2)
				{
					if (num2 < num)
					{
						ravenText = staticText;
						num = num2;
						flag = flag2;
					}
				}
				else if (staticText.m_priority > ravenText.m_priority)
				{
					ravenText = staticText;
					num = num2;
					flag = flag2;
				}
			}
			else if (!flag2 && flag)
			{
				ravenText = staticText;
				num = num2;
				flag = flag2;
			}
		}
		return ravenText;
	}

	private void RemoveSeendTempTexts()
	{
		for (int i = 0; i < m_tempTexts.Count; i++)
		{
			if (Player.m_localPlayer.HaveSeenTutorial(m_tempTexts[i].m_key))
			{
				m_tempTexts.RemoveAt(i);
				break;
			}
		}
	}

	private void FlyAway(bool forceTeleport = false)
	{
		Chat.instance.ClearNpcText(((Component)this).gameObject);
		if (forceTeleport || IsUnderRoof())
		{
			m_animator.SetTrigger("poff");
			m_timeSinceTeleport = 0f;
		}
		else
		{
			m_animator.SetTrigger("flyaway");
		}
	}

	private void CheckSpawn()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.m_localPlayer == (Object)null))
		{
			RemoveSeendTempTexts();
			RavenText bestText = GetBestText();
			if (IsSpawned() && CanHide() && bestText != null && bestText != m_currentText)
			{
				FlyAway(forceTeleport: true);
				m_currentText = null;
			}
			if (IsAway() && bestText != null && !EnemyNearby(((Component)this).transform.position) && !RandEventSystem.InEvent())
			{
				bool forceTeleport = m_timeSinceTeleport < 6f;
				Spawn(bestText, forceTeleport);
			}
		}
	}

	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Character;
	}

	public void Damage(HitData hit)
	{
		if (IsSpawned())
		{
			FlyAway(forceTeleport: true);
			RestartSpawnCheck(4f);
			Game.instance.IncrementPlayerStat(PlayerStatType.RavenHits);
		}
	}

	private void RestartSpawnCheck(float delay)
	{
		((MonoBehaviour)this).CancelInvoke("CheckSpawn");
		((MonoBehaviour)this).InvokeRepeating("CheckSpawn", delay, 1f);
	}

	private bool IsSpawned()
	{
		return InState("visible");
	}

	public bool IsAway()
	{
		return InState("away");
	}

	public bool IsFlying()
	{
		return InState("flying");
	}

	private void Spawn(RavenText text, bool forceTeleport)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Utils.GetMainCamera() == (Object)null || !m_tutorialsEnabled)
		{
			return;
		}
		if (text.m_static)
		{
			m_groundObject = ((Component)text.m_guidePoint).gameObject;
			((Component)this).transform.position = ((Component)text.m_guidePoint).transform.position;
		}
		else
		{
			if (!FindSpawnPoint(out var point, out var landOn))
			{
				return;
			}
			((Component)this).transform.position = point;
			m_groundObject = landOn;
		}
		m_currentText = text;
		m_hasTalked = false;
		m_randomTextTimer = 99999f;
		if (m_currentText.m_key.Length > 0 && Player.m_localPlayer.HaveSeenTutorial(m_currentText.m_key))
		{
			m_hasTalked = true;
		}
		Vector3 val = ((Component)Player.m_localPlayer).transform.position - ((Component)this).transform.position;
		val.y = 0f;
		((Vector3)(ref val)).Normalize();
		((Component)this).transform.rotation = Quaternion.LookRotation(val);
		if (forceTeleport)
		{
			m_animator.SetTrigger("teleportin");
		}
		else if (text.m_static)
		{
			if (IsUnderRoof())
			{
				m_animator.SetTrigger("teleportin");
			}
			else
			{
				m_animator.SetTrigger("flyin");
			}
		}
		else
		{
			m_animator.SetTrigger("flyin");
		}
		Game.instance.IncrementPlayerStat(PlayerStatType.RavenAppear);
	}

	private bool IsUnderRoof()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return Physics.Raycast(((Component)this).transform.position + Vector3.up * 0.2f, Vector3.up, 20f, LayerMask.GetMask(new string[3] { "Default", "static_solid", "piece" }));
	}

	public static void RegisterStaticText(RavenText text)
	{
		m_staticTexts.Add(text);
	}

	public static void UnregisterStaticText(RavenText text)
	{
		m_staticTexts.Remove(text);
	}

	public static void AddTempText(string key, string topic, string text, string label, bool munin)
	{
		if (key.Length > 0)
		{
			foreach (RavenText tempText in m_tempTexts)
			{
				if (tempText.m_key == key)
				{
					return;
				}
			}
		}
		RavenText ravenText = new RavenText();
		ravenText.m_key = key;
		ravenText.m_topic = topic;
		ravenText.m_label = label;
		ravenText.m_text = text;
		ravenText.m_static = false;
		ravenText.m_munin = munin;
		m_tempTexts.Add(ravenText);
	}
}
