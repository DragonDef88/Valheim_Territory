using System;
using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour, Hoverable, Interactable
{
	[Serializable]
	public class TradeItem
	{
		public ItemDrop m_prefab;

		public int m_stack = 1;

		public int m_price = 100;

		public string m_requiredGlobalKey;
	}

	[Serializable]
	public class TraderUseItem
	{
		public ItemDrop m_prefab;

		public string m_setsGlobalKey;

		public bool m_removesItem;

		public string m_dialog;
	}

	[Serializable]
	public class ConditionalDialog
	{
		public List<string> m_keyConditions = new List<string>();

		[Tooltip("Default unchecked will run when they keys are set in the world, check this to run when keys are NOT set.")]
		public bool m_whenKeyNotSet;

		[Tooltip("Which places this text will be used.")]
		public TalkPlacement m_textPlacement;

		public KeySetType m_keyCheck;

		public GameKeyType m_keyType;

		public List<string> m_dialog;
	}

	public enum TalkPlacement
	{
		ReplaceRandomTalk,
		ReplaceGreetAndRandomTalk,
		ReplaceGreet
	}

	public string m_name = "Haldor";

	public float m_standRange = 15f;

	public float m_greetRange = 5f;

	public float m_byeRange = 5f;

	public List<TradeItem> m_items = new List<TradeItem>();

	public List<TraderUseItem> m_useItems = new List<TraderUseItem>();

	[Header("Dialog")]
	public float m_hideDialogDelay = 5f;

	public float m_randomTalkInterval = 30f;

	public float m_dialogHeight = 1.5f;

	public List<string> m_randomTalk = new List<string>();

	public List<string> m_randomGreets = new List<string>();

	public List<string> m_randomGoodbye = new List<string>();

	public List<string> m_randomStartTrade = new List<string>();

	public List<string> m_randomBuy = new List<string>();

	public List<string> m_randomSell = new List<string>();

	public List<string> m_randomGiveItemNo = new List<string>();

	public List<string> m_randomUseItemAlreadyRecieved = new List<string>();

	[Tooltip("These will be used instead of random talk if any of the conditions are met")]
	public List<ConditionalDialog> m_randomTalkConditionals = new List<ConditionalDialog>();

	public EffectList m_randomTalkFX = new EffectList();

	public EffectList m_randomGreetFX = new EffectList();

	public EffectList m_randomGoodbyeFX = new EffectList();

	public EffectList m_randomStartTradeFX = new EffectList();

	public EffectList m_randomBuyFX = new EffectList();

	public EffectList m_randomSellFX = new EffectList();

	private bool m_didGreet;

	private bool m_didGoodbye;

	private Animator m_animator;

	private LookAt m_lookAt;

	private void Start()
	{
		m_animator = ((Component)this).GetComponentInChildren<Animator>();
		m_lookAt = ((Component)this).GetComponentInChildren<LookAt>();
		SnapToGround component = ((Component)this).GetComponent<SnapToGround>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.Snap();
		}
		((MonoBehaviour)this).InvokeRepeating("RandomTalk", m_randomTalkInterval, m_randomTalkInterval);
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, Mathf.Max(m_byeRange + 3f, m_standRange));
		if (Object.op_Implicit((Object)(object)closestPlayer))
		{
			float num = Vector3.Distance(((Component)closestPlayer).transform.position, ((Component)this).transform.position);
			if (num < m_standRange)
			{
				m_animator.SetBool("Stand", true);
				m_lookAt.SetLoockAtTarget(closestPlayer.GetHeadPoint());
			}
			if (!m_didGreet && num < m_greetRange)
			{
				m_didGreet = true;
				List<string> texts = CheckConditionals(m_randomGreets, isGreet: true);
				Say(texts, "Greet");
				m_randomGreetFX.Create(((Component)this).transform.position, Quaternion.identity);
			}
			if (m_didGreet && !m_didGoodbye && num > m_byeRange)
			{
				m_didGoodbye = true;
				Say(m_randomGoodbye, "Greet");
				m_randomGoodbyeFX.Create(((Component)this).transform.position, Quaternion.identity);
			}
		}
		else
		{
			m_animator.SetBool("Stand", false);
			m_lookAt.ResetTarget();
		}
	}

	private void RandomTalk()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (m_animator.GetBool("Stand") && !StoreGui.IsVisible() && Player.IsPlayerInRange(((Component)this).transform.position, m_greetRange))
		{
			List<string> texts = CheckConditionals(m_randomTalk, isGreet: false);
			Say(texts, "Talk");
			m_randomTalkFX.Create(((Component)this).transform.position, Quaternion.identity);
		}
	}

	private List<string> CheckConditionals(List<string> defaultList, bool isGreet)
	{
		foreach (ConditionalDialog randomTalkConditional in m_randomTalkConditionals)
		{
			if ((isGreet && randomTalkConditional.m_textPlacement == TalkPlacement.ReplaceRandomTalk) || (!isGreet && randomTalkConditional.m_textPlacement == TalkPlacement.ReplaceGreet))
			{
				continue;
			}
			if (randomTalkConditional.m_keyCheck == KeySetType.All)
			{
				bool flag = true;
				foreach (string keyCondition in randomTalkConditional.m_keyConditions)
				{
					if (!ZoneSystem.instance.CheckKey(keyCondition, randomTalkConditional.m_keyType, !randomTalkConditional.m_whenKeyNotSet))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return randomTalkConditional.m_dialog;
				}
				continue;
			}
			if (randomTalkConditional.m_keyCheck == KeySetType.Exlusive)
			{
				bool flag2 = false;
				bool flag3 = false;
				foreach (string keyCondition2 in randomTalkConditional.m_keyConditions)
				{
					if (ZoneSystem.instance.CheckKey(keyCondition2, randomTalkConditional.m_keyType, !randomTalkConditional.m_whenKeyNotSet))
					{
						flag2 = true;
					}
					else
					{
						flag3 = true;
					}
				}
				if (flag2 && flag3)
				{
					return randomTalkConditional.m_dialog;
				}
				continue;
			}
			bool flag4 = false;
			foreach (string keyCondition3 in randomTalkConditional.m_keyConditions)
			{
				if (ZoneSystem.instance.CheckKey(keyCondition3, randomTalkConditional.m_keyType, !randomTalkConditional.m_whenKeyNotSet))
				{
					flag4 = true;
					break;
				}
			}
			if ((flag4 && randomTalkConditional.m_keyCheck == KeySetType.Any) || (!flag4 && randomTalkConditional.m_keyCheck == KeySetType.None))
			{
				return randomTalkConditional.m_dialog;
			}
		}
		return defaultList;
	}

	public string GetHoverText()
	{
		string text = m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $raven_interact";
		if (m_useItems.Count > 0)
		{
			text += "\n[<color=yellow><b>1-8</b></color>] $npc_giveitem";
		}
		return Localization.instance.Localize(text);
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_name);
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		StoreGui.instance.Show(this);
		Say(m_randomStartTrade, "Talk");
		m_randomStartTradeFX.Create(((Component)this).transform.position, Quaternion.identity);
		return false;
	}

	private void DiscoverItems(Player player)
	{
		foreach (TradeItem availableItem in GetAvailableItems())
		{
			player.AddKnownItem(availableItem.m_prefab.m_itemData);
		}
	}

	private void Say(List<string> texts, string trigger)
	{
		Say(texts[Random.Range(0, texts.Count)], trigger);
	}

	private void Say(string text, string trigger)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Chat.instance.SetNpcText(((Component)this).gameObject, Vector3.up * m_dialogHeight, 20f, m_hideDialogDelay, "", text, large: false);
		if (trigger.Length > 0)
		{
			m_animator.SetTrigger(trigger);
		}
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (m_useItems.Count > 0)
		{
			foreach (TraderUseItem useItem in m_useItems)
			{
				if (item.m_shared.m_name == useItem.m_prefab.m_itemData.m_shared.m_name)
				{
					if (!string.IsNullOrEmpty(useItem.m_setsGlobalKey) && ZoneSystem.instance.GetGlobalKey(useItem.m_setsGlobalKey))
					{
						Say(m_randomUseItemAlreadyRecieved, "Talk");
						return true;
					}
					if (!string.IsNullOrEmpty(useItem.m_dialog))
					{
						Say(useItem.m_dialog, "Talk");
					}
					if (!string.IsNullOrEmpty(useItem.m_setsGlobalKey))
					{
						ZoneSystem.instance.SetGlobalKey(useItem.m_setsGlobalKey);
					}
					if (useItem.m_removesItem)
					{
						user.GetInventory().RemoveItem(item, 1);
						user.ShowRemovedMessage(item, 1);
					}
					return true;
				}
			}
			Say(m_randomGiveItemNo, "Talk");
			return true;
		}
		return false;
	}

	public void OnBought(TradeItem item)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Say(m_randomBuy, "Buy");
		m_randomBuyFX.Create(((Component)this).transform.position, Quaternion.identity);
	}

	public void OnSold()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Say(m_randomSell, "Sell");
		m_randomSellFX.Create(((Component)this).transform.position, Quaternion.identity);
	}

	public List<TradeItem> GetAvailableItems()
	{
		List<TradeItem> list = new List<TradeItem>();
		foreach (TradeItem item in m_items)
		{
			if (string.IsNullOrEmpty(item.m_requiredGlobalKey) || ZoneSystem.instance.GetGlobalKey(item.m_requiredGlobalKey))
			{
				list.Add(item);
			}
		}
		return list;
	}
}
