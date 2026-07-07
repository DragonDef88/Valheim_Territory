using System;
using System.Collections.Generic;
using UnityEngine;

public class MineRock5 : MonoBehaviour, IDestructible, Hoverable
{
	private struct BoundData
	{
		public Vector3 m_pos;

		public Quaternion m_rot;

		public Vector3 m_size;
	}

	private class HitArea
	{
		public Collider m_collider;

		public MeshRenderer m_meshRenderer;

		public MeshFilter m_meshFilter;

		public StaticPhysics m_physics;

		public float m_health;

		public BoundData m_bound;

		public bool m_supported;

		public float m_baseScale;
	}

	private static Mesh m_tempMeshA;

	private static Mesh m_tempMeshB;

	private static List<CombineInstance> m_tempInstancesA = new List<CombineInstance>();

	private static List<CombineInstance> m_tempInstancesB = new List<CombineInstance>();

	public string m_name = "";

	public float m_health = 2f;

	public HitData.DamageModifiers m_damageModifiers;

	public int m_minToolTier;

	public bool m_supportCheck = true;

	public bool m_triggerPrivateArea;

	public EffectList m_destroyedEffect = new EffectList();

	public EffectList m_hitEffect = new EffectList();

	public DropTable m_dropItems;

	public bool m_hitEffectAreaCenter = true;

	private List<HitArea> m_hitAreas;

	private List<Renderer> m_extraRenderers;

	private bool m_haveSetupBounds;

	private ZNetView m_nview;

	private MeshFilter m_meshFilter;

	private MeshRenderer m_meshRenderer;

	private uint m_lastDataRevision = uint.MaxValue;

	private const int m_supportIterations = 3;

	private bool m_allDestroyed;

	private static int m_rayMask = 0;

	private static int m_groundLayer = 0;

	private static Collider[] m_tempColliders = (Collider[])(object)new Collider[128];

	private static HashSet<Collider> m_tempColliderSet = new HashSet<Collider>();

	private void Awake()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Expected O, but got Unknown
		Collider[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<Collider>();
		m_hitAreas = new List<HitArea>(componentsInChildren.Length);
		m_extraRenderers = new List<Renderer>();
		foreach (Collider val in componentsInChildren)
		{
			HitArea hitArea = new HitArea();
			hitArea.m_collider = val;
			hitArea.m_meshFilter = ((Component)val).GetComponent<MeshFilter>();
			hitArea.m_meshRenderer = ((Component)val).GetComponent<MeshRenderer>();
			hitArea.m_physics = ((Component)val).GetComponent<StaticPhysics>();
			hitArea.m_health = m_health + (float)Game.m_worldLevel * m_health * Game.instance.m_worldLevelMineHPMultiplier;
			hitArea.m_baseScale = ((Component)hitArea.m_collider).transform.localScale.x;
			for (int j = 0; j < ((Component)val).transform.childCount; j++)
			{
				Renderer[] componentsInChildren2 = ((Component)((Component)val).transform.GetChild(j)).GetComponentsInChildren<Renderer>();
				m_extraRenderers.AddRange(componentsInChildren2);
			}
			m_hitAreas.Add(hitArea);
		}
		if (m_rayMask == 0)
		{
			m_rayMask = LayerMask.GetMask(new string[5] { "piece", "Default", "static_solid", "Default_small", "terrain" });
		}
		if (m_groundLayer == 0)
		{
			m_groundLayer = LayerMask.NameToLayer("terrain");
		}
		Material[] array = null;
		foreach (HitArea hitArea2 in m_hitAreas)
		{
			if (array == null || ((Renderer)hitArea2.m_meshRenderer).sharedMaterials.Length > array.Length)
			{
				array = ((Renderer)hitArea2.m_meshRenderer).sharedMaterials;
			}
		}
		m_meshFilter = ((Component)this).gameObject.AddComponent<MeshFilter>();
		m_meshRenderer = ((Component)this).gameObject.AddComponent<MeshRenderer>();
		((Renderer)m_meshRenderer).sharedMaterials = array;
		m_meshFilter.mesh = new Mesh();
		((Object)m_meshFilter).name = "___MineRock5 m_meshFilter";
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.GetZDO() != null)
		{
			m_nview.Register<HitData, int>("RPC_Damage", RPC_Damage);
			m_nview.Register<int, float>("RPC_SetAreaHealth", RPC_SetAreaHealth);
		}
		CheckForUpdate();
		((MonoBehaviour)this).InvokeRepeating("CheckForUpdate", Random.Range(5f, 10f), 10f);
	}

	private void CheckSupport()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		UpdateSupport();
		for (int i = 0; i < m_hitAreas.Count; i++)
		{
			HitArea hitArea = m_hitAreas[i];
			if (hitArea.m_health > 0f && !hitArea.m_supported)
			{
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = m_health;
				Bounds bounds = hitArea.m_collider.bounds;
				hitData.m_point = ((Bounds)(ref bounds)).center;
				hitData.m_toolTier = 100;
				hitData.m_hitType = HitData.HitType.Structural;
				DamageArea(i, hitData);
			}
		}
	}

	private void CheckForUpdate()
	{
		if (m_nview.IsValid() && m_nview.GetZDO().DataRevision != m_lastDataRevision)
		{
			LoadHealth();
			UpdateMesh();
		}
	}

	private void LoadHealth()
	{
		string @string = m_nview.GetZDO().GetString(ZDOVars.s_health);
		if (@string.Length > 0)
		{
			ZPackage zPackage = new ZPackage(Convert.FromBase64String(@string));
			int num = zPackage.ReadInt();
			for (int i = 0; i < num; i++)
			{
				float health = zPackage.ReadSingle();
				HitArea hitArea = GetHitArea(i);
				if (hitArea != null)
				{
					hitArea.m_health = health;
				}
			}
		}
		m_lastDataRevision = m_nview.GetZDO().DataRevision;
	}

	private void SaveHealth()
	{
		ZPackage zPackage = new ZPackage();
		zPackage.Write(m_hitAreas.Count);
		foreach (HitArea hitArea in m_hitAreas)
		{
			zPackage.Write(hitArea.m_health);
		}
		string value = Convert.ToBase64String(zPackage.GetArray());
		m_nview.GetZDO().Set(ZDOVars.s_health, value);
		m_lastDataRevision = m_nview.GetZDO().DataRevision;
	}

	private void UpdateMesh()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		m_tempInstancesA.Clear();
		m_tempInstancesB.Clear();
		Material val = ((Renderer)m_meshRenderer).sharedMaterials[0];
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		Matrix4x4 inverse = ((Matrix4x4)(ref localToWorldMatrix)).inverse;
		for (int i = 0; i < m_hitAreas.Count; i++)
		{
			HitArea hitArea = m_hitAreas[i];
			if (hitArea.m_health > 0f)
			{
				CombineInstance item = default(CombineInstance);
				((CombineInstance)(ref item)).mesh = hitArea.m_meshFilter.sharedMesh;
				((CombineInstance)(ref item)).transform = inverse * ((Component)hitArea.m_meshFilter).transform.localToWorldMatrix;
				for (int j = 0; j < hitArea.m_meshFilter.sharedMesh.subMeshCount; j++)
				{
					((CombineInstance)(ref item)).subMeshIndex = j;
					if ((Object)(object)((Renderer)hitArea.m_meshRenderer).sharedMaterials[j] == (Object)(object)val)
					{
						m_tempInstancesA.Add(item);
					}
					else
					{
						m_tempInstancesB.Add(item);
					}
				}
				((Renderer)hitArea.m_meshRenderer).enabled = false;
				((Component)hitArea.m_collider).gameObject.SetActive(true);
			}
			else
			{
				((Component)hitArea.m_collider).gameObject.SetActive(false);
			}
		}
		if ((Object)(object)m_tempMeshA == (Object)null)
		{
			m_tempMeshA = new Mesh();
			m_tempMeshB = new Mesh();
			((Object)m_tempMeshA).name = "___MineRock5 m_tempMeshA";
			((Object)m_tempMeshB).name = "___MineRock5 m_tempMeshB";
		}
		m_tempMeshA.CombineMeshes(m_tempInstancesA.ToArray());
		m_tempMeshB.CombineMeshes(m_tempInstancesB.ToArray());
		CombineInstance val2 = default(CombineInstance);
		((CombineInstance)(ref val2)).mesh = m_tempMeshA;
		CombineInstance val3 = default(CombineInstance);
		((CombineInstance)(ref val3)).mesh = m_tempMeshB;
		m_meshFilter.mesh.CombineMeshes((CombineInstance[])(object)new CombineInstance[2] { val2, val3 }, false, false);
		((Renderer)m_meshRenderer).enabled = true;
		Renderer[] array = (Renderer[])(object)new Renderer[m_extraRenderers.Count + 1];
		m_extraRenderers.CopyTo(0, array, 0, m_extraRenderers.Count);
		array[^1] = (Renderer)(object)m_meshRenderer;
		LODGroup component = ((Component)this).gameObject.GetComponent<LODGroup>();
		LOD[] lODs = component.GetLODs();
		lODs[0].renderers = array;
		component.SetLODs(lODs);
	}

	public string GetHoverText()
	{
		return Localization.instance.Localize(m_name);
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	public void Damage(HitData hit)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid() || m_hitAreas == null)
		{
			return;
		}
		if ((Object)(object)hit.m_hitCollider == (Object)null || hit.m_radius > 0f)
		{
			int num = 0;
			m_tempColliderSet.Clear();
			int num2 = Physics.OverlapSphereNonAlloc(hit.m_point, (hit.m_radius > 0f) ? hit.m_radius : 0.05f, m_tempColliders, m_rayMask);
			for (int i = 0; i < num2; i++)
			{
				Transform parent = ((Component)m_tempColliders[i]).transform.parent;
				if ((Object)(object)parent == (Object)(object)((Component)this).transform || ((Object)(object)parent != (Object)null && (Object)(object)parent.parent == (Object)(object)((Component)this).transform))
				{
					m_tempColliderSet.Add(m_tempColliders[i]);
				}
			}
			if (m_tempColliderSet.Count > 0)
			{
				foreach (Collider item in m_tempColliderSet)
				{
					int areaIndex = GetAreaIndex(item);
					if (areaIndex >= 0)
					{
						num++;
						m_nview.InvokeRPC("RPC_Damage", hit, areaIndex);
						if (m_allDestroyed)
						{
							return;
						}
					}
				}
			}
			if (num == 0)
			{
				ZLog.Log((object)("Minerock hit has no collider or invalid hit area on " + ((Object)((Component)this).gameObject).name));
			}
		}
		else
		{
			int areaIndex2 = GetAreaIndex(hit.m_hitCollider);
			if (areaIndex2 < 0)
			{
				ZLog.Log((object)("Invalid hit area on " + ((Object)((Component)this).gameObject).name));
				return;
			}
			m_nview.InvokeRPC("RPC_Damage", hit, areaIndex2);
		}
	}

	private void RPC_Damage(long sender, HitData hit, int hitAreaIndex)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		bool flag = DamageArea(hitAreaIndex, hit);
		if (flag && m_supportCheck)
		{
			CheckSupport();
		}
		if (m_triggerPrivateArea)
		{
			Character attacker = hit.GetAttacker();
			if (attacker != null)
			{
				PrivateArea.OnObjectDamaged(((Component)this).transform.position, attacker, flag);
			}
		}
	}

	private bool DamageArea(int hitAreaIndex, HitData hit)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("hit mine rock " + hitAreaIndex));
		HitArea hitArea = GetHitArea(hitAreaIndex);
		if (hitArea == null)
		{
			ZLog.Log((object)("Missing hit area " + hitAreaIndex));
			return false;
		}
		LoadHealth();
		if (hitArea.m_health <= 0f)
		{
			ZLog.Log((object)"Already destroyed");
			return false;
		}
		hit.ApplyResistance(m_damageModifiers, out var significantModifier);
		float totalDamage = hit.GetTotalDamage();
		Vector3 val;
		if (!m_hitEffectAreaCenter || !((Object)(object)hitArea.m_collider != (Object)null))
		{
			val = hit.m_point;
		}
		else
		{
			Bounds bounds = hitArea.m_collider.bounds;
			val = ((Bounds)(ref bounds)).center;
		}
		Vector3 val2 = val;
		if (!hit.CheckToolTier(m_minToolTier))
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, val2, 0f);
			return false;
		}
		DamageText.instance.ShowText(significantModifier, val2, totalDamage);
		if (totalDamage <= 0f)
		{
			return false;
		}
		hitArea.m_health -= totalDamage;
		SaveHealth();
		m_hitEffect.Create(val2, Quaternion.identity);
		if (hit.m_hitType != HitData.HitType.CinderFire)
		{
			Player.GetClosestPlayer(val2, 10f)?.AddNoise(100f);
		}
		if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
		{
			Game.instance.IncrementPlayerStat(PlayerStatType.MineHits);
		}
		if (hitArea.m_health <= 0f)
		{
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetAreaHealth", hitAreaIndex, hitArea.m_health);
			m_destroyedEffect.Create(val2, Quaternion.identity);
			foreach (GameObject drop in m_dropItems.GetDropList())
			{
				Vector3 val3 = val2 + Random.insideUnitSphere * 0.3f;
				Object.Instantiate<GameObject>(drop, val3, Quaternion.identity);
				ItemDrop.OnCreateNew(drop);
			}
			if (AllDestroyed())
			{
				m_nview.Destroy();
				m_allDestroyed = true;
			}
			if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
			{
				Game.instance.IncrementPlayerStat(PlayerStatType.Mines);
				switch (m_minToolTier)
				{
				case 0:
					Game.instance.IncrementPlayerStat(PlayerStatType.MineTier0);
					break;
				case 1:
					Game.instance.IncrementPlayerStat(PlayerStatType.MineTier1);
					break;
				case 2:
					Game.instance.IncrementPlayerStat(PlayerStatType.MineTier2);
					break;
				case 3:
					Game.instance.IncrementPlayerStat(PlayerStatType.MineTier3);
					break;
				case 4:
					Game.instance.IncrementPlayerStat(PlayerStatType.MineTier4);
					break;
				case 5:
					Game.instance.IncrementPlayerStat(PlayerStatType.MineTier5);
					break;
				default:
					ZLog.LogWarning((object)("No stat for mine tier: " + m_minToolTier));
					break;
				}
			}
			return true;
		}
		return false;
	}

	private bool AllDestroyed()
	{
		for (int i = 0; i < m_hitAreas.Count; i++)
		{
			if (m_hitAreas[i].m_health > 0f)
			{
				return false;
			}
		}
		return true;
	}

	private bool NonDestroyed()
	{
		for (int i = 0; i < m_hitAreas.Count; i++)
		{
			if (m_hitAreas[i].m_health <= 0f)
			{
				return false;
			}
		}
		return true;
	}

	private void RPC_SetAreaHealth(long sender, int index, float health)
	{
		HitArea hitArea = GetHitArea(index);
		if (hitArea != null)
		{
			hitArea.m_health = health;
		}
		UpdateMesh();
	}

	private int GetAreaIndex(Collider area)
	{
		for (int i = 0; i < m_hitAreas.Count; i++)
		{
			if ((Object)(object)m_hitAreas[i].m_collider == (Object)(object)area)
			{
				return i;
			}
		}
		return -1;
	}

	private HitArea GetHitArea(int index)
	{
		if (index < 0 || index >= m_hitAreas.Count)
		{
			return null;
		}
		return m_hitAreas[index];
	}

	private void UpdateSupport()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!m_haveSetupBounds)
		{
			SetupColliders();
			m_haveSetupBounds = true;
		}
		foreach (HitArea hitArea in m_hitAreas)
		{
			hitArea.m_supported = false;
		}
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < 3; i++)
		{
			foreach (HitArea hitArea2 in m_hitAreas)
			{
				if (hitArea2.m_supported)
				{
					continue;
				}
				int num = Physics.OverlapBoxNonAlloc(position + hitArea2.m_bound.m_pos, hitArea2.m_bound.m_size, m_tempColliders, hitArea2.m_bound.m_rot, m_rayMask);
				for (int j = 0; j < num; j++)
				{
					Collider val = m_tempColliders[j];
					if (!((Object)(object)val == (Object)(object)hitArea2.m_collider) && !((Object)(object)val.attachedRigidbody != (Object)null) && !val.isTrigger)
					{
						hitArea2.m_supported = hitArea2.m_supported || GetSupport(val);
						if (hitArea2.m_supported)
						{
							break;
						}
					}
				}
			}
		}
		ZLog.Log((object)("Suport time " + (Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f));
	}

	private bool GetSupport(Collider c)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)c).gameObject.layer == m_groundLayer)
		{
			return true;
		}
		IDestructible componentInParent = ((Component)c).gameObject.GetComponentInParent<IDestructible>();
		if (componentInParent != null)
		{
			if (componentInParent == this)
			{
				foreach (HitArea hitArea in m_hitAreas)
				{
					if ((Object)(object)hitArea.m_collider == (Object)(object)c)
					{
						return hitArea.m_supported;
					}
				}
			}
			return ((Component)c).transform.position.y < ((Component)this).transform.position.y;
		}
		return true;
	}

	private void SetupColliders()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		foreach (HitArea hitArea in m_hitAreas)
		{
			hitArea.m_bound.m_rot = Quaternion.identity;
			ref BoundData bound = ref hitArea.m_bound;
			Bounds bounds = hitArea.m_collider.bounds;
			bound.m_pos = ((Bounds)(ref bounds)).center - position;
			ref BoundData bound2 = ref hitArea.m_bound;
			bounds = hitArea.m_collider.bounds;
			bound2.m_size = ((Bounds)(ref bounds)).size * 0.5f;
		}
	}
}
