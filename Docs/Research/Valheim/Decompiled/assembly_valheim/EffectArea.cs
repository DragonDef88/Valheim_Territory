using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectArea : MonoBehaviour, IMonoUpdater
{
	[Flags]
	public enum Type : byte
	{
		None = 0,
		Heat = 1,
		Fire = 2,
		PlayerBase = 4,
		Burning = 8,
		Teleport = 0x10,
		NoMonsters = 0x20,
		WarmCozyArea = 0x40,
		PrivateProperty = 0x80
	}

	private KeyValuePair<Bounds, EffectArea> noMonsterArea;

	private KeyValuePair<Bounds, EffectArea> noMonsterCloseToArea;

	private KeyValuePair<Bounds, EffectArea> burnCloseToArea;

	[BitMask(typeof(Type))]
	public Type m_type;

	public string m_statusEffect = "";

	public bool m_playerOnly;

	private int m_statusEffectHash;

	private Collider m_collider;

	private int m_collisions;

	private List<Character> m_collidedWithCharacter = new List<Character>();

	private bool m_isHeatType;

	private static int s_characterMask = 0;

	private static readonly List<EffectArea> s_allAreas = new List<EffectArea>();

	private static readonly List<KeyValuePair<Bounds, EffectArea>> s_noMonsterAreas = new List<KeyValuePair<Bounds, EffectArea>>();

	private static readonly List<KeyValuePair<Bounds, EffectArea>> s_noMonsterCloseToAreas = new List<KeyValuePair<Bounds, EffectArea>>();

	private static readonly List<KeyValuePair<Bounds, EffectArea>> s_BurningAreas = new List<KeyValuePair<Bounds, EffectArea>>();

	private static Collider[] m_tempColliders = (Collider[])(object)new Collider[128];

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(m_statusEffect))
		{
			m_statusEffectHash = StringExtensionMethods.GetStableHashCode(m_statusEffect);
		}
		if (s_characterMask == 0)
		{
			s_characterMask = LayerMask.GetMask(new string[1] { "character_trigger" });
		}
		m_collider = ((Component)this).GetComponent<Collider>();
		m_collider.isTrigger = true;
		if ((m_type & Type.NoMonsters) != 0)
		{
			noMonsterArea = new KeyValuePair<Bounds, EffectArea>(m_collider.bounds, this);
			s_noMonsterAreas.Add(noMonsterArea);
			Bounds bounds = m_collider.bounds;
			((Bounds)(ref bounds)).Expand(new Vector3(15f, 15f, 15f));
			noMonsterCloseToArea = new KeyValuePair<Bounds, EffectArea>(bounds, this);
			s_noMonsterCloseToAreas.Add(noMonsterCloseToArea);
		}
		if ((m_type & Type.Burning) != 0)
		{
			Bounds bounds2 = m_collider.bounds;
			((Bounds)(ref bounds2)).Expand(new Vector3(0.25f, 0.25f, 0.25f));
			burnCloseToArea = new KeyValuePair<Bounds, EffectArea>(bounds2, this);
			s_BurningAreas.Add(burnCloseToArea);
		}
		m_isHeatType = m_type.HasFlag(Type.Heat);
		s_allAreas.Add(this);
	}

	private void OnDestroy()
	{
		s_allAreas.Remove(this);
		if (s_noMonsterAreas.Contains(noMonsterArea))
		{
			s_noMonsterAreas.Remove(noMonsterArea);
		}
		if (s_noMonsterCloseToAreas.Contains(noMonsterCloseToArea))
		{
			s_noMonsterCloseToAreas.Remove(noMonsterCloseToArea);
		}
		if (s_BurningAreas.Contains(burnCloseToArea))
		{
			s_BurningAreas.Remove(burnCloseToArea);
		}
	}

	protected virtual void OnEnable()
	{
		Instances.Add(this);
	}

	protected virtual void OnDisable()
	{
		Instances.Remove(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		m_collisions++;
		if (m_isHeatType || m_statusEffectHash != 0)
		{
			Character component = ((Component)other).GetComponent<Character>();
			if (Object.op_Implicit((Object)(object)component) && component.IsOwner() && (!m_playerOnly || component.IsPlayer()) && !m_collidedWithCharacter.Contains(component))
			{
				m_collidedWithCharacter.Add(component);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		m_collisions--;
		Character component = ((Component)other).GetComponent<Character>();
		if ((Object)(object)component != (Object)null)
		{
			m_collidedWithCharacter.Remove(component);
		}
	}

	public void CustomFixedUpdate(float deltaTime)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (m_collisions <= 0 || m_collidedWithCharacter.Count == 0 || (Object)(object)ZNet.instance == (Object)null)
		{
			return;
		}
		foreach (Character item in m_collidedWithCharacter)
		{
			if (m_statusEffectHash != 0)
			{
				item.GetSEMan().AddStatusEffect(m_statusEffectHash, resetTime: true);
			}
			if (m_isHeatType)
			{
				item.OnNearFire(((Component)this).transform.position);
			}
		}
	}

	public float GetRadius()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Collider collider = m_collider;
		SphereCollider val = (SphereCollider)(object)((collider is SphereCollider) ? collider : null);
		if (val == null)
		{
			CapsuleCollider val2 = (CapsuleCollider)(object)((collider is CapsuleCollider) ? collider : null);
			if (val2 != null)
			{
				return val2.radius;
			}
			Bounds bounds = m_collider.bounds;
			Vector3 size = ((Bounds)(ref bounds)).size;
			return ((Vector3)(ref size)).magnitude;
		}
		return val.radius;
	}

	public static EffectArea IsPointInsideNoMonsterArea(Vector3 p)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Bounds, EffectArea> s_noMonsterArea in s_noMonsterAreas)
		{
			Bounds key = s_noMonsterArea.Key;
			if (((Bounds)(ref key)).Contains(p))
			{
				return s_noMonsterArea.Value;
			}
		}
		return null;
	}

	public static EffectArea IsPointCloseToNoMonsterArea(Vector3 p)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Bounds, EffectArea> s_noMonsterCloseToArea in s_noMonsterCloseToAreas)
		{
			Bounds key = s_noMonsterCloseToArea.Key;
			if (((Bounds)(ref key)).Contains(p))
			{
				return s_noMonsterCloseToArea.Value;
			}
		}
		return null;
	}

	public static EffectArea IsPointInsideArea(Vector3 p, Type type, float radius = 0f)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (type == Type.Burning && radius.Equals(0.25f))
		{
			return GetBurningAreaPointPlus025(p);
		}
		int num = Physics.OverlapSphereNonAlloc(p, radius, m_tempColliders, s_characterMask);
		for (int i = 0; i < num; i++)
		{
			EffectArea component = ((Component)m_tempColliders[i]).GetComponent<EffectArea>();
			if (Object.op_Implicit((Object)(object)component) && (component.m_type & type) != 0)
			{
				return component;
			}
		}
		return null;
	}

	public static bool IsPointPlus025InsideBurningArea(Vector3 p)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Bounds, EffectArea> s_BurningArea in s_BurningAreas)
		{
			Bounds key = s_BurningArea.Key;
			if (((Bounds)(ref key)).Contains(p))
			{
				return true;
			}
		}
		return false;
	}

	private static EffectArea GetBurningAreaPointPlus025(Vector3 p)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Bounds, EffectArea> s_BurningArea in s_BurningAreas)
		{
			Bounds key = s_BurningArea.Key;
			if (((Bounds)(ref key)).Contains(p))
			{
				return s_BurningArea.Value;
			}
		}
		return null;
	}

	public static int GetBaseValue(Vector3 p, float radius)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = Physics.OverlapSphereNonAlloc(p, radius, m_tempColliders, s_characterMask);
		for (int i = 0; i < num2; i++)
		{
			EffectArea component = ((Component)m_tempColliders[i]).GetComponent<EffectArea>();
			if (Object.op_Implicit((Object)(object)component) && (component.m_type & Type.PlayerBase) != 0)
			{
				num++;
			}
		}
		return num;
	}

	public static List<EffectArea> GetAllAreas()
	{
		return s_allAreas;
	}
}
