using System;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour, IDestructible
{
	public Action m_onDestroyed;

	public Action m_onDamaged;

	[Header("Destruction")]
	public DestructibleType m_destructibleType = DestructibleType.Default;

	public float m_health = 1f;

	public HitData.DamageModifiers m_damages;

	public float m_minDamageTreshold;

	public int m_minToolTier;

	public float m_hitNoise;

	public float m_destroyNoise;

	public bool m_triggerPrivateArea;

	public float m_ttl;

	public GameObject m_spawnWhenDestroyed;

	[Header("Effects")]
	public EffectList m_destroyedEffect = new EffectList();

	public EffectList m_hitEffect = new EffectList();

	public bool m_autoCreateFragments;

	private ZNetView m_nview;

	private Rigidbody m_body;

	private bool m_firstFrame = true;

	private bool m_destroyed;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.GetZDO() != null)
		{
			m_nview.Register<HitData>("RPC_Damage", RPC_Damage);
			if (m_autoCreateFragments)
			{
				m_nview.Register("RPC_CreateFragments", RPC_CreateFragments);
			}
			if (m_ttl > 0f)
			{
				((MonoBehaviour)this).InvokeRepeating("DestroyNow", m_ttl, 1f);
			}
		}
	}

	private void Start()
	{
		m_firstFrame = false;
	}

	public GameObject GetParentObject()
	{
		return null;
	}

	public DestructibleType GetDestructibleType()
	{
		return m_destructibleType;
	}

	public void Damage(HitData hit)
	{
		if (!m_firstFrame && m_nview.IsValid())
		{
			m_nview.InvokeRPC("RPC_Damage", hit);
		}
	}

	private void RPC_Damage(long sender, HitData hit)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner() || m_destroyed)
		{
			return;
		}
		float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_health, m_health + (float)Game.m_worldLevel * m_health * Game.instance.m_worldLevelMineHPMultiplier);
		if (@float <= 0f || m_destroyed)
		{
			return;
		}
		hit.ApplyResistance(m_damages, out var significantModifier);
		float totalDamage = hit.GetTotalDamage();
		if (Object.op_Implicit((Object)(object)m_body))
		{
			m_body.AddForceAtPosition(hit.m_dir * hit.m_pushForce, hit.m_point, (ForceMode)1);
		}
		if (!hit.CheckToolTier(m_minToolTier))
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
		if (m_triggerPrivateArea)
		{
			Character attacker = hit.GetAttacker();
			if (Object.op_Implicit((Object)(object)attacker))
			{
				bool destroyed = @float <= 0f;
				PrivateArea.OnObjectDamaged(((Component)this).transform.position, attacker, destroyed);
			}
		}
		m_hitEffect.Create(hit.m_point, Quaternion.identity, ((Component)this).transform);
		if (m_onDamaged != null)
		{
			m_onDamaged();
		}
		if (m_hitNoise > 0f && hit.m_hitType != HitData.HitType.CinderFire)
		{
			Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
			if (Object.op_Implicit((Object)(object)closestPlayer))
			{
				closestPlayer.AddNoise(m_hitNoise);
			}
		}
		if (@float <= 0f)
		{
			Destroy(hit);
		}
	}

	public void DestroyNow()
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			Destroy();
		}
	}

	public void Destroy(HitData hit = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 hitPoint = hit?.m_point ?? Vector3.zero;
		Vector3 hitDir = hit?.m_dir ?? Vector3.zero;
		CreateDestructionEffects(hitPoint, hitDir);
		if (m_destroyNoise > 0f && (hit == null || hit.m_hitType != HitData.HitType.CinderFire))
		{
			Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 10f);
			if (Object.op_Implicit((Object)(object)closestPlayer))
			{
				closestPlayer.AddNoise(m_destroyNoise);
			}
		}
		if (Object.op_Implicit((Object)(object)m_spawnWhenDestroyed))
		{
			GameObject val = Object.Instantiate<GameObject>(m_spawnWhenDestroyed, ((Component)this).transform.position, ((Component)this).transform.rotation);
			val.GetComponent<ZNetView>().SetLocalScale(((Component)this).transform.localScale);
			val.GetComponent<Gibber>()?.Setup(hitPoint, hitDir);
			if (hit != null)
			{
				val.GetComponent<MineRock5>()?.Damage(hit);
			}
		}
		if (m_onDestroyed != null)
		{
			m_onDestroyed();
		}
		ZNetScene.instance.Destroy(((Component)this).gameObject);
		m_destroyed = true;
	}

	private void CreateDestructionEffects(Vector3 hitPoint, Vector3 hitDir)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		GameObject[] array = m_destroyedEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		for (int i = 0; i < array.Length; i++)
		{
			Gibber component = array[i].GetComponent<Gibber>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Setup(hitPoint, hitDir);
			}
		}
		if (m_autoCreateFragments)
		{
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_CreateFragments");
		}
	}

	private void RPC_CreateFragments(long peer)
	{
		CreateFragments(((Component)this).gameObject);
	}

	public static void CreateFragments(GameObject rootObject, bool visibleOnly = true)
	{
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		MeshRenderer[] componentsInChildren = rootObject.GetComponentsInChildren<MeshRenderer>(true);
		int layer = LayerMask.NameToLayer("effect");
		List<Rigidbody> list = new List<Rigidbody>();
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer val in array)
		{
			if (!((Component)val).gameObject.activeInHierarchy || (visibleOnly && !((Renderer)val).isVisible))
			{
				continue;
			}
			MeshFilter component = ((Component)val).gameObject.GetComponent<MeshFilter>();
			if (!((Object)(object)component == (Object)null))
			{
				if ((Object)(object)component.sharedMesh == (Object)null)
				{
					ZLog.Log((object)("Meshfilter missing mesh " + ((Object)((Component)component).gameObject).name));
					continue;
				}
				GameObject val2 = new GameObject();
				val2.layer = layer;
				val2.transform.position = ((Component)component).gameObject.transform.position;
				val2.transform.rotation = ((Component)component).gameObject.transform.rotation;
				val2.transform.localScale = ((Component)component).gameObject.transform.lossyScale * 0.9f;
				val2.AddComponent<MeshFilter>().sharedMesh = component.sharedMesh;
				((Renderer)val2.AddComponent<MeshRenderer>()).sharedMaterials = ((Renderer)val).sharedMaterials;
				MaterialMan.instance.SetValue(val2, ShaderProps._RippleDistance, 0f);
				MaterialMan.instance.SetValue(val2, ShaderProps._ValueNoise, 0f);
				Rigidbody item = val2.AddComponent<Rigidbody>();
				val2.AddComponent<BoxCollider>();
				list.Add(item);
				val2.AddComponent<TimedDestruction>().Trigger(Random.Range(2, 4));
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		Vector3 val3 = Vector3.zero;
		int num = 0;
		foreach (Rigidbody item2 in list)
		{
			val3 += item2.worldCenterOfMass;
			num++;
		}
		val3 /= (float)num;
		foreach (Rigidbody item3 in list)
		{
			Vector3 val4 = item3.worldCenterOfMass - val3;
			Vector3 val5 = ((Vector3)(ref val4)).normalized * 4f;
			val5 += Random.onUnitSphere * 1f;
			item3.AddForce(val5, (ForceMode)2);
		}
	}
}
