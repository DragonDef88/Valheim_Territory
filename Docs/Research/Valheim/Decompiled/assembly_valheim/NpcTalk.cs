using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcTalk : MonoBehaviour
{
	private class QueuedSay
	{
		public string text;

		public string trigger;

		public EffectList m_effect;
	}

	private float m_lastTargetUpdate;

	public string m_name = "Haldor";

	public float m_maxRange = 15f;

	public float m_greetRange = 10f;

	public float m_byeRange = 15f;

	public float m_offset = 2f;

	public float m_minTalkInterval = 1.5f;

	private const int m_maxQueuedTexts = 3;

	public float m_hideDialogDelay = 5f;

	public float m_randomTalkInterval = 10f;

	public float m_randomTalkChance = 1f;

	public List<string> m_randomTalk = new List<string>();

	public List<string> m_randomTalkInFactionBase = new List<string>();

	public List<string> m_randomGreets = new List<string>();

	public List<string> m_randomGoodbye = new List<string>();

	public List<string> m_privateAreaAlarm = new List<string>();

	public List<string> m_aggravated = new List<string>();

	public EffectList m_randomTalkFX = new EffectList();

	public EffectList m_randomGreetFX = new EffectList();

	public EffectList m_randomGoodbyeFX = new EffectList();

	private bool m_didGreet;

	private bool m_didGoodbye;

	private MonsterAI m_monsterAI;

	private Animator m_animator;

	private Character m_character;

	private ZNetView m_nview;

	private Player m_targetPlayer;

	private bool m_seeTarget;

	private bool m_hearTarget;

	private Queue<QueuedSay> m_queuedTexts = new Queue<QueuedSay>();

	private static float m_lastTalkTime;

	private void Start()
	{
		m_character = ((Component)this).GetComponentInChildren<Character>();
		m_monsterAI = ((Component)this).GetComponent<MonsterAI>();
		m_animator = ((Component)this).GetComponentInChildren<Animator>();
		m_nview = ((Component)this).GetComponent<ZNetView>();
		MonsterAI monsterAI = m_monsterAI;
		monsterAI.m_onBecameAggravated = (Action<BaseAI.AggravatedReason>)Delegate.Combine(monsterAI.m_onBecameAggravated, new Action<BaseAI.AggravatedReason>(OnBecameAggravated));
		((MonoBehaviour)this).InvokeRepeating("RandomTalk", Random.Range(m_randomTalkInterval / 5f, m_randomTalkInterval), m_randomTalkInterval);
	}

	private void Update()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_monsterAI.GetTargetCreature() != (Object)null || (Object)(object)m_monsterAI.GetStaticTarget() != (Object)null || !m_nview.IsValid())
		{
			return;
		}
		UpdateTarget();
		if (Object.op_Implicit((Object)(object)m_targetPlayer))
		{
			if (m_nview.IsOwner())
			{
				Vector3 val = m_character.GetVelocity();
				if (((Vector3)(ref val)).magnitude < 0.5f)
				{
					val = m_targetPlayer.GetEyePoint() - m_character.GetEyePoint();
					Vector3 normalized = ((Vector3)(ref val)).normalized;
					m_character.SetLookDir(normalized);
				}
			}
			if (m_seeTarget)
			{
				float num = Vector3.Distance(((Component)m_targetPlayer).transform.position, ((Component)this).transform.position);
				if (!m_didGreet && num < m_greetRange)
				{
					m_didGreet = true;
					QueueSay(m_randomGreets, "Greet", m_randomGreetFX);
				}
				if (m_didGreet && !m_didGoodbye && num > m_byeRange)
				{
					m_didGoodbye = true;
					QueueSay(m_randomGoodbye, "Greet", m_randomGoodbyeFX);
				}
			}
		}
		UpdateSayQueue();
	}

	private void UpdateTarget()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.time - m_lastTargetUpdate > 1f))
		{
			return;
		}
		m_lastTargetUpdate = Time.time;
		m_targetPlayer = null;
		Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, m_maxRange);
		if (!((Object)(object)closestPlayer == (Object)null) && !m_monsterAI.IsEnemy(closestPlayer))
		{
			m_seeTarget = m_monsterAI.CanSeeTarget(closestPlayer);
			m_hearTarget = m_monsterAI.CanHearTarget(closestPlayer);
			if (m_seeTarget || m_hearTarget)
			{
				m_targetPlayer = closestPlayer;
			}
		}
	}

	private void OnBecameAggravated(BaseAI.AggravatedReason reason)
	{
		QueueSay(m_aggravated, "Aggravated", null);
	}

	public void OnPrivateAreaAttacked(Character attacker)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (attacker.IsPlayer() && m_monsterAI.IsAggravatable() && !m_monsterAI.IsAggravated() && Vector3.Distance(((Component)this).transform.position, ((Component)attacker).transform.position) < m_maxRange)
		{
			QueueSay(m_privateAreaAlarm, "Angry", null);
		}
	}

	private void RandomTalk()
	{
		if (!(Time.time - m_lastTalkTime < m_minTalkInterval) && !(Random.Range(0f, 1f) > m_randomTalkChance))
		{
			UpdateTarget();
			if (Object.op_Implicit((Object)(object)m_targetPlayer) && m_seeTarget)
			{
				List<string> texts = (InFactionBase() ? m_randomTalkInFactionBase : m_randomTalk);
				QueueSay(texts, "Talk", m_randomTalkFX);
			}
		}
	}

	private void QueueSay(List<string> texts, string trigger, EffectList effect)
	{
		if (texts.Count != 0 && m_queuedTexts.Count < 3)
		{
			QueuedSay queuedSay = new QueuedSay();
			queuedSay.text = texts[Random.Range(0, texts.Count)];
			queuedSay.trigger = trigger;
			queuedSay.m_effect = effect;
			m_queuedTexts.Enqueue(queuedSay);
		}
	}

	private void UpdateSayQueue()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (m_queuedTexts.Count != 0 && !(Time.time - m_lastTalkTime < m_minTalkInterval))
		{
			QueuedSay queuedSay = m_queuedTexts.Dequeue();
			Say(queuedSay.text, queuedSay.trigger);
			if (queuedSay.m_effect != null)
			{
				queuedSay.m_effect.Create(((Component)this).transform.position, Quaternion.identity);
			}
		}
	}

	private void Say(string text, string trigger)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_lastTalkTime = Time.time;
		Chat.instance.SetNpcText(((Component)this).gameObject, Vector3.up * m_offset, 20f, m_hideDialogDelay, "", text, large: false);
		if (trigger.Length > 0)
		{
			m_animator.SetTrigger(trigger);
		}
	}

	private bool InFactionBase()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return PrivateArea.InsideFactionArea(((Component)this).transform.position, m_character.GetFaction());
	}
}
