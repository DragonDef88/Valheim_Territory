using System;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
	public DropTable m_items = new DropTable();

	public EffectList m_spawnEffect = new EffectList();

	public float m_respawnTimeMinuts = 10f;

	public bool m_spawnAtNight = true;

	public bool m_spawnAtDay = true;

	public bool m_spawnWhenEnemiesCleared;

	public float m_enemiesCheckRange = 30f;

	private const float c_TriggerDistance = 20f;

	private ZNetView m_nview;

	private bool m_seenEnemies;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.GetZDO() != null)
		{
			((MonoBehaviour)this).InvokeRepeating("UpdateSpawner", 10f, 2f);
		}
	}

	private void UpdateSpawner()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner() || (!m_spawnAtDay && EnvMan.IsDay()) || (!m_spawnAtNight && EnvMan.IsNight()))
		{
			return;
		}
		if (m_spawnWhenEnemiesCleared)
		{
			bool num = IsMonsterInRange(((Component)this).transform.position, m_enemiesCheckRange);
			if (num && !m_seenEnemies)
			{
				m_seenEnemies = true;
			}
			if (num || !m_seenEnemies)
			{
				return;
			}
		}
		long @long = m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L);
		DateTime time = ZNet.instance.GetTime();
		DateTime dateTime = new DateTime(@long);
		TimeSpan timeSpan = time - dateTime;
		if ((!(m_respawnTimeMinuts <= 0f) || @long == 0L) && !(timeSpan.TotalMinutes < (double)m_respawnTimeMinuts) && Player.IsPlayerInRange(((Component)this).transform.position, 20f))
		{
			List<GameObject> dropList = m_items.GetDropList();
			for (int i = 0; i < dropList.Count; i++)
			{
				Vector2 val = Random.insideUnitCircle * 0.3f;
				Vector3 val2 = ((Component)this).transform.position + new Vector3(val.x, 0.3f * (float)i, val.y);
				Quaternion val3 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
				Object.Instantiate<GameObject>(dropList[i], val2, val3);
			}
			m_spawnEffect.Create(((Component)this).transform.position, Quaternion.identity);
			m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
			m_seenEnemies = false;
		}
	}

	public static bool IsMonsterInRange(Vector3 point, float range)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		List<Character> allCharacters = Character.GetAllCharacters();
		float time = Time.time;
		foreach (Character item in allCharacters)
		{
			if (item.IsMonsterFaction(time) && Vector3.Distance(((Component)item).transform.position, point) < range)
			{
				return true;
			}
		}
		return false;
	}

	private void OnDrawGizmos()
	{
	}
}
