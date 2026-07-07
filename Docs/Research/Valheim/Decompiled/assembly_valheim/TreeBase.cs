using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBase : MonoBehaviour, IDestructible
{
	private ZNetView m_nview;

	public float m_health = 1f;

	public HitData.DamageModifiers m_damageModifiers;

	public int m_minToolTier;

	public EffectList m_destroyedEffect = new EffectList();

	public EffectList m_hitEffect = new EffectList();

	public EffectList m_respawnEffect = new EffectList();

	public GameObject m_trunk;

	public GameObject m_stubPrefab;

	public GameObject m_logPrefab;

	public Transform m_logSpawnPoint;

	[Header("Drops")]
	public DropTable m_dropWhenDestroyed = new DropTable();

	public float m_spawnYOffset = 0.5f;

	public float m_spawnYStep = 0.3f;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_nview.Register<HitData>("RPC_Damage", RPC_Damage);
		m_nview.Register("RPC_Grow", RPC_Grow);
		m_nview.Register("RPC_Shake", RPC_Shake);
		if (m_nview.IsOwner() && m_nview.GetZDO().GetFloat(ZDOVars.s_health, m_health + (float)Game.m_worldLevel * m_health * Game.instance.m_worldLevelMineHPMultiplier) <= 0f)
		{
			m_nview.Destroy();
		}
	}

	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Tree;
	}

	public void Damage(HitData hit)
	{
		m_nview.InvokeRPC("RPC_Damage", hit);
	}

	public void Grow()
	{
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_Grow");
	}

	private void RPC_Grow(long uid)
	{
		((MonoBehaviour)this).StartCoroutine("GrowAnimation");
	}

	private IEnumerator GrowAnimation()
	{
		GameObject animatedTrunk = Object.Instantiate<GameObject>(m_trunk, m_trunk.transform.position, m_trunk.transform.rotation, ((Component)this).transform);
		animatedTrunk.isStatic = false;
		LODGroup component = ((Component)((Component)this).transform).GetComponent<LODGroup>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.fadeMode = (LODFadeMode)0;
		}
		m_trunk.SetActive(false);
		for (float t = 0f; t < 0.3f; t += Time.deltaTime)
		{
			float num = Mathf.Clamp01(t / 0.3f);
			animatedTrunk.transform.localScale = m_trunk.transform.localScale * num;
			yield return null;
		}
		Object.Destroy((Object)(object)animatedTrunk);
		m_trunk.SetActive(true);
		if (m_nview.IsOwner())
		{
			m_respawnEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		}
	}

	private void RPC_Damage(long sender, HitData hit)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			return;
		}
		float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_health, m_health);
		if (@float <= 0f)
		{
			m_nview.Destroy();
			return;
		}
		bool flag = hit.m_damage.GetMajorityDamageType() == HitData.DamageType.Fire;
		bool flag2 = hit.m_hitType == HitData.HitType.CinderFire;
		hit.ApplyResistance(m_damageModifiers, out var significantModifier);
		float totalDamage = hit.GetTotalDamage();
		if (!hit.CheckToolTier(m_minToolTier, alwaysAllowTierZero: true))
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f);
			return;
		}
		DamageText.instance.ShowText(significantModifier, hit.m_point, totalDamage);
		if (totalDamage <= 0f)
		{
			return;
		}
		@float -= totalDamage;
		m_nview.GetZDO().Set(ZDOVars.s_health, @float);
		if (!flag && !flag2)
		{
			Shake();
		}
		if (!flag2)
		{
			m_hitEffect.Create(hit.m_point, Quaternion.identity, ((Component)this).transform);
			Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 10f);
			if (Object.op_Implicit((Object)(object)closestPlayer))
			{
				closestPlayer.AddNoise(100f);
			}
		}
		if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
		{
			Game.instance.IncrementPlayerStat(PlayerStatType.TreeChops);
		}
		if (!(@float <= 0f))
		{
			return;
		}
		m_destroyedEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		SpawnLog(hit.m_dir);
		List<GameObject> dropList = m_dropWhenDestroyed.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector2 val = Random.insideUnitCircle * 0.5f;
			Vector3 val2 = ((Component)this).transform.position + Vector3.up * m_spawnYOffset + new Vector3(val.x, m_spawnYStep * (float)i, val.y);
			Quaternion val3 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
			Object.Instantiate<GameObject>(dropList[i], val2, val3);
		}
		((Component)this).gameObject.SetActive(false);
		m_nview.Destroy();
		if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
		{
			Game.instance.IncrementPlayerStat(PlayerStatType.Tree);
			switch (m_minToolTier)
			{
			case 0:
				Game.instance.IncrementPlayerStat(PlayerStatType.TreeTier0);
				break;
			case 1:
				Game.instance.IncrementPlayerStat(PlayerStatType.TreeTier1);
				break;
			case 2:
				Game.instance.IncrementPlayerStat(PlayerStatType.TreeTier2);
				break;
			case 3:
				Game.instance.IncrementPlayerStat(PlayerStatType.TreeTier3);
				break;
			case 4:
				Game.instance.IncrementPlayerStat(PlayerStatType.TreeTier4);
				break;
			case 5:
				Game.instance.IncrementPlayerStat(PlayerStatType.TreeTier5);
				break;
			default:
				ZLog.LogWarning((object)("No stat for tree tier: " + m_minToolTier));
				break;
			}
		}
	}

	private void Shake()
	{
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_Shake");
	}

	private void RPC_Shake(long uid)
	{
		((MonoBehaviour)this).StopCoroutine("ShakeAnimation");
		((MonoBehaviour)this).StartCoroutine("ShakeAnimation");
	}

	private IEnumerator ShakeAnimation()
	{
		m_trunk.gameObject.isStatic = false;
		float t = Time.time;
		while (Time.time - t < 1f)
		{
			float time = Time.time;
			float num = 1f - Mathf.Clamp01((time - t) / 1f);
			float num2 = num * num * num * 1.5f;
			Quaternion localRotation = Quaternion.Euler(Mathf.Sin(time * 40f) * num2, 0f, Mathf.Cos(time * 0.9f * 40f) * num2);
			m_trunk.transform.localRotation = localRotation;
			yield return null;
		}
		m_trunk.transform.localRotation = Quaternion.identity;
		m_trunk.gameObject.isStatic = true;
	}

	private void SpawnLog(Vector3 hitDir)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(m_logPrefab, m_logSpawnPoint.position, m_logSpawnPoint.rotation);
		val.GetComponent<ZNetView>().SetLocalScale(((Component)this).transform.localScale);
		Rigidbody component = val.GetComponent<Rigidbody>();
		component.mass *= ((Component)this).transform.localScale.x;
		component.ResetInertiaTensor();
		component.AddForceAtPosition(hitDir * 0.2f * component.mass, val.transform.position + Vector3.up * 4f * ((Component)this).transform.localScale.y, (ForceMode)1);
		if (Object.op_Implicit((Object)(object)m_stubPrefab))
		{
			Object.Instantiate<GameObject>(m_stubPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation).GetComponent<ZNetView>().SetLocalScale(((Component)this).transform.localScale);
		}
	}
}
