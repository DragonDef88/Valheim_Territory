using System;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
	public float m_velMultiplier = 1f;

	public float m_ttl;

	public Renderer m_mainModel;

	public EffectList m_removeEffect = new EffectList();

	public Action<Vector3> m_onDestroyed;

	public bool m_float;

	public float m_floatOffset = -0.1f;

	public bool m_dropItems = true;

	public GameObject m_lootSpawnJoint;

	private const float m_floatForce = 20f;

	private const float m_damping = 0.05f;

	private ZNetView m_nview;

	private Rigidbody[] m_bodies;

	private const float m_dropOffset = 0.75f;

	private const float m_dropArea = 0.5f;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_bodies = ((Component)this).GetComponentsInChildren<Rigidbody>();
		((MonoBehaviour)this).Invoke("RemoveInitVel", 2f);
		if (Object.op_Implicit((Object)(object)m_mainModel))
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_hue);
			float float2 = m_nview.GetZDO().GetFloat(ZDOVars.s_saturation);
			float float3 = m_nview.GetZDO().GetFloat(ZDOVars.s_value);
			m_mainModel.material.SetFloat("_Hue", @float);
			m_mainModel.material.SetFloat("_Saturation", float2);
			m_mainModel.material.SetFloat("_Value", float3);
		}
		((MonoBehaviour)this).InvokeRepeating("DestroyNow", m_ttl, 1f);
	}

	public Vector3 GetAverageBodyPosition()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (m_bodies.Length == 0)
		{
			return ((Component)this).transform.position;
		}
		Vector3 val = Vector3.zero;
		Rigidbody[] bodies = m_bodies;
		foreach (Rigidbody val2 in bodies)
		{
			val += val2.position;
		}
		return val / (float)m_bodies.Length;
	}

	private void DestroyNow()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			Vector3 val = GetAverageBodyPosition();
			_ = Quaternion.identity;
			m_removeEffect.Create(val, Quaternion.identity);
			if ((Object)(object)m_lootSpawnJoint != (Object)null)
			{
				val = m_lootSpawnJoint.transform.position;
			}
			SpawnLoot(val);
			ZNetScene.instance.Destroy(((Component)this).gameObject);
		}
	}

	private void RemoveInitVel()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_initVel, Vector3.zero);
		}
	}

	private void Start()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vec = m_nview.GetZDO().GetVec3(ZDOVars.s_initVel, Vector3.zero);
		if (vec != Vector3.zero)
		{
			vec.y = Mathf.Min(vec.y, 4f);
			Rigidbody[] bodies = m_bodies;
			for (int i = 0; i < bodies.Length; i++)
			{
				bodies[i].linearVelocity = vec * Random.value;
			}
		}
	}

	public void Setup(Vector3 velocity, float hue, float saturation, float value, CharacterDrop characterDrop)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		velocity.x *= m_velMultiplier;
		velocity.z *= m_velMultiplier;
		m_nview.GetZDO().Set(ZDOVars.s_initVel, velocity);
		m_nview.GetZDO().Set(ZDOVars.s_hue, hue);
		m_nview.GetZDO().Set(ZDOVars.s_saturation, saturation);
		m_nview.GetZDO().Set(ZDOVars.s_value, value);
		if (Object.op_Implicit((Object)(object)m_mainModel))
		{
			m_mainModel.material.SetFloat("_Hue", hue);
			m_mainModel.material.SetFloat("_Saturation", saturation);
			m_mainModel.material.SetFloat("_Value", value);
		}
		if (Object.op_Implicit((Object)(object)characterDrop) && m_dropItems)
		{
			SaveLootList(characterDrop);
		}
	}

	private void SaveLootList(CharacterDrop characterDrop)
	{
		List<KeyValuePair<GameObject, int>> list = characterDrop.GenerateDropList();
		if (list.Count > 0)
		{
			ZDO zDO = m_nview.GetZDO();
			zDO.Set(ZDOVars.s_drops, list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				KeyValuePair<GameObject, int> keyValuePair = list[i];
				int prefabHash = ZNetScene.instance.GetPrefabHash(keyValuePair.Key);
				zDO.Set("drop_hash" + i, prefabHash);
				zDO.Set("drop_amount" + i, keyValuePair.Value);
			}
		}
	}

	private void SpawnLoot(Vector3 center)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		ZDO zDO = m_nview.GetZDO();
		int @int = zDO.GetInt(ZDOVars.s_drops);
		if (@int <= 0)
		{
			return;
		}
		List<KeyValuePair<GameObject, int>> list = new List<KeyValuePair<GameObject, int>>();
		for (int i = 0; i < @int; i++)
		{
			int int2 = zDO.GetInt("drop_hash" + i);
			int int3 = zDO.GetInt("drop_amount" + i);
			GameObject prefab = ZNetScene.instance.GetPrefab(int2);
			if ((Object)(object)prefab == (Object)null)
			{
				ZLog.LogWarning((object)("Ragdoll: Missing prefab:" + int2 + " when dropping loot"));
			}
			else
			{
				list.Add(new KeyValuePair<GameObject, int>(prefab, int3));
			}
		}
		CharacterDrop.DropItems(list, center + Vector3.up * 0.75f, 0.5f);
	}

	private void FixedUpdate()
	{
		if (m_float)
		{
			UpdateFloating(Time.fixedDeltaTime);
		}
	}

	private void UpdateFloating(float dt)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody[] bodies = m_bodies;
		foreach (Rigidbody val in bodies)
		{
			Vector3 worldCenterOfMass = val.worldCenterOfMass;
			worldCenterOfMass.y += m_floatOffset;
			float liquidLevel = Floating.GetLiquidLevel(worldCenterOfMass);
			if (worldCenterOfMass.y < liquidLevel)
			{
				float num = (liquidLevel - worldCenterOfMass.y) / 0.5f;
				Vector3 val2 = Vector3.up * 20f * num;
				val.AddForce(val2 * dt, (ForceMode)2);
				val.linearVelocity -= val.linearVelocity * 0.05f * num;
			}
		}
	}
}
