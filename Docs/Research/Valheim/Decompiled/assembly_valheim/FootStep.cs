using System;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour, IMonoUpdater
{
	[Flags]
	public enum MotionType
	{
		Jog = 1,
		Run = 2,
		Sneak = 4,
		Climbing = 8,
		Swimming = 0x10,
		Land = 0x20,
		Walk = 0x40
	}

	[Flags]
	public enum GroundMaterial
	{
		None = 0,
		Default = 1,
		Water = 2,
		Stone = 4,
		Wood = 8,
		Snow = 0x10,
		Mud = 0x20,
		Grass = 0x40,
		GenericGround = 0x80,
		Metal = 0x100,
		Tar = 0x200,
		Ashlands = 0x400,
		Lava = 0x800,
		Everything = 0xFFF
	}

	[Serializable]
	public class StepEffect
	{
		public string m_name = "";

		[BitMask(typeof(MotionType))]
		public MotionType m_motionType = MotionType.Jog;

		[BitMask(typeof(GroundMaterial))]
		public GroundMaterial m_material = GroundMaterial.Default;

		public GameObject[] m_effectPrefabs = Array.Empty<GameObject>();
	}

	[Header("Footless")]
	public bool m_footlessFootsteps;

	public float m_footlessTriggerDistance = 1f;

	[Space(16f)]
	public float m_footstepCullDistance = 20f;

	public List<StepEffect> m_effects = new List<StepEffect>();

	public Transform[] m_feet = Array.Empty<Transform>();

	private static readonly int s_footstepID = ZSyncAnimation.GetHash("footstep");

	private static readonly int s_forwardSpeedID = ZSyncAnimation.GetHash("forward_speed");

	private static readonly int s_sidewaySpeedID = ZSyncAnimation.GetHash("sideway_speed");

	private static readonly Queue<GameObject> s_stepInstances = new Queue<GameObject>();

	private float m_footstep;

	private float m_footstepTimer;

	private int m_pieceLayer;

	private float m_distanceAccumulator;

	private Vector3 m_lastPosition;

	private const float c_MinFootstepInterval = 0.2f;

	private const int c_MaxFootstepInstances = 30;

	private Animator m_animator;

	private Character m_character;

	private ZNetView m_nview;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Start()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		m_animator = ((Component)this).GetComponentInChildren<Animator>();
		m_character = ((Component)this).GetComponent<Character>();
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_footstep = m_animator.GetFloat(s_footstepID);
		if (m_pieceLayer == 0)
		{
			m_pieceLayer = LayerMask.NameToLayer("piece");
		}
		Character character = m_character;
		character.m_onLand = (Action<Vector3>)Delegate.Combine(character.m_onLand, new Action<Vector3>(OnLand));
		m_lastPosition = ((Component)m_character).transform.position;
		if (m_nview.IsValid())
		{
			m_nview.Register<int, Vector3>("Step", RPC_Step);
		}
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void CustomUpdate(float dt, float time)
	{
		if (!((Object)(object)m_nview == (Object)null) && m_nview.IsOwner())
		{
			UpdateFootstep(dt);
			UpdateFootlessFootstep(dt);
		}
	}

	private void UpdateFootstep(float dt)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (m_feet.Length != 0)
		{
			Camera mainCamera = Utils.GetMainCamera();
			if (!((Object)(object)mainCamera == (Object)null) && !(Vector3.Distance(((Component)this).transform.position, ((Component)mainCamera).transform.position) > m_footstepCullDistance))
			{
				UpdateFootstepCurveTrigger(dt);
			}
		}
	}

	private void UpdateFootlessFootstep(float dt)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (m_feet.Length == 0 && m_footlessFootsteps)
		{
			Vector3 position = ((Component)this).transform.position;
			if (!m_character.IsOnGround())
			{
				m_distanceAccumulator = 0f;
			}
			else
			{
				m_distanceAccumulator += Vector3.Distance(position, m_lastPosition);
			}
			m_lastPosition = position;
			if (m_distanceAccumulator > m_footlessTriggerDistance)
			{
				m_distanceAccumulator -= m_footlessTriggerDistance;
				OnFoot(((Component)this).transform);
			}
		}
	}

	private void UpdateFootstepCurveTrigger(float dt)
	{
		m_footstepTimer += dt;
		float @float = m_animator.GetFloat(s_footstepID);
		if (Utils.SignDiffers(@float, m_footstep) && Mathf.Max(Mathf.Abs(m_animator.GetFloat(s_forwardSpeedID)), Mathf.Abs(m_animator.GetFloat(s_sidewaySpeedID))) > 0.2f && m_footstepTimer > 0.2f)
		{
			m_footstepTimer = 0f;
			OnFoot();
		}
		m_footstep = @float;
	}

	private Transform FindActiveFoot()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Transform val = null;
		float num = 9999f;
		Vector3 forward = ((Component)this).transform.forward;
		Transform[] feet = m_feet;
		foreach (Transform val2 in feet)
		{
			if (!((Object)(object)val2 == (Object)null))
			{
				Vector3 val3 = val2.position - ((Component)this).transform.position;
				float num2 = Vector3.Dot(forward, val3);
				if (num2 > num || (Object)(object)val == (Object)null)
				{
					val = val2;
					num = num2;
				}
			}
		}
		return val;
	}

	private Transform FindFoot(string name)
	{
		Transform[] feet = m_feet;
		foreach (Transform val in feet)
		{
			if (((Object)((Component)val).gameObject).name == name)
			{
				return val;
			}
		}
		return null;
	}

	public void OnFoot()
	{
		Transform foot = FindActiveFoot();
		OnFoot(foot);
	}

	public void OnFoot(string name)
	{
		Transform val = FindFoot(name);
		if ((Object)(object)val == (Object)null)
		{
			ZLog.LogWarning((object)("FAiled to find foot:" + name));
		}
		else
		{
			OnFoot(val);
		}
	}

	private void OnLand(Vector3 point)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid())
		{
			GroundMaterial groundMaterial = GetGroundMaterial(m_character, point);
			int num = FindBestStepEffect(groundMaterial, MotionType.Land);
			if (num != -1)
			{
				m_nview.InvokeRPC(ZNetView.Everybody, "Step", num, point);
			}
		}
	}

	private void OnFoot(Transform foot)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid())
		{
			Vector3 val = (((Object)(object)foot != (Object)null) ? foot.position : ((Component)this).transform.position);
			MotionType motionType = GetMotionType(m_character);
			GroundMaterial groundMaterial = GetGroundMaterial(m_character, val);
			int num = FindBestStepEffect(groundMaterial, motionType);
			if (num != -1)
			{
				m_nview.InvokeRPC(ZNetView.Everybody, "Step", num, val);
			}
		}
	}

	private static void PurgeOldEffects()
	{
		while (s_stepInstances.Count > 30)
		{
			GameObject val = s_stepInstances.Dequeue();
			if (Object.op_Implicit((Object)(object)val))
			{
				Object.Destroy((Object)(object)val);
			}
		}
	}

	private void DoEffect(StepEffect effect, Vector3 point)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		GameObject[] effectPrefabs = effect.m_effectPrefabs;
		foreach (GameObject val in effectPrefabs)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, point, ((Component)this).transform.rotation);
			s_stepInstances.Enqueue(val2);
			if ((Object)(object)val2.GetComponent<ZNetView>() != (Object)null)
			{
				ZLog.LogWarning((object)("Foot step effect " + effect.m_name + " prefab " + ((Object)val).name + " in " + ((Object)((Component)m_character).gameObject).name + " should not contain a ZNetView component"));
			}
		}
		PurgeOldEffects();
	}

	private void RPC_Step(long sender, int effectIndex, Vector3 point)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		StepEffect effect = m_effects[effectIndex];
		DoEffect(effect, point);
	}

	private static MotionType GetMotionType(Character character)
	{
		if (character.IsWalking())
		{
			return MotionType.Walk;
		}
		if (character.IsSwimming())
		{
			return MotionType.Swimming;
		}
		if (character.IsWallRunning())
		{
			return MotionType.Climbing;
		}
		if (character.IsRunning())
		{
			return MotionType.Run;
		}
		if (character.IsSneaking())
		{
			return MotionType.Sneak;
		}
		return MotionType.Jog;
	}

	private GroundMaterial GetGroundMaterial(Character character, Vector3 point)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (character.InWater())
		{
			return GroundMaterial.Water;
		}
		if (character.InLiquid())
		{
			return GroundMaterial.Tar;
		}
		Collider lastGroundCollider = character.GetLastGroundCollider();
		if ((Object)(object)lastGroundCollider == (Object)null)
		{
			return GroundMaterial.Default;
		}
		Heightmap component = ((Component)lastGroundCollider).GetComponent<Heightmap>();
		if ((Object)(object)component != (Object)null)
		{
			Vector3 lastGroundNormal = character.GetLastGroundNormal();
			return component.GetGroundMaterial(lastGroundNormal, point);
		}
		if (((Component)lastGroundCollider).gameObject.layer != m_pieceLayer)
		{
			return GroundMaterial.Default;
		}
		WearNTear componentInParent = ((Component)lastGroundCollider).GetComponentInParent<WearNTear>();
		if (!Object.op_Implicit((Object)(object)componentInParent))
		{
			return GroundMaterial.Default;
		}
		switch (componentInParent.m_materialType)
		{
		case WearNTear.MaterialType.Wood:
			return GroundMaterial.Wood;
		case WearNTear.MaterialType.Stone:
		case WearNTear.MaterialType.Marble:
			return GroundMaterial.Stone;
		case WearNTear.MaterialType.HardWood:
			return GroundMaterial.Wood;
		case WearNTear.MaterialType.Iron:
			return GroundMaterial.Metal;
		default:
			return GroundMaterial.Default;
		}
	}

	public void FindJoints()
	{
		ZLog.Log((object)"Finding joints");
		Transform val = Utils.FindChild(((Component)this).transform, "LeftFootFront", (IterativeSearchType)0);
		Transform val2 = Utils.FindChild(((Component)this).transform, "RightFootFront", (IterativeSearchType)0);
		Transform val3 = Utils.FindChild(((Component)this).transform, "LeftFoot", (IterativeSearchType)0);
		if ((Object)(object)val3 == (Object)null)
		{
			val3 = Utils.FindChild(((Component)this).transform, "LeftFootBack", (IterativeSearchType)0);
		}
		if ((Object)(object)val3 == (Object)null)
		{
			val3 = Utils.FindChild(((Component)this).transform, "l_foot", (IterativeSearchType)0);
		}
		if ((Object)(object)val3 == (Object)null)
		{
			val3 = Utils.FindChild(((Component)this).transform, "Foot.l", (IterativeSearchType)0);
		}
		if ((Object)(object)val3 == (Object)null)
		{
			val3 = Utils.FindChild(((Component)this).transform, "foot.l", (IterativeSearchType)0);
		}
		Transform val4 = Utils.FindChild(((Component)this).transform, "RightFoot", (IterativeSearchType)0);
		if ((Object)(object)val4 == (Object)null)
		{
			val4 = Utils.FindChild(((Component)this).transform, "RightFootBack", (IterativeSearchType)0);
		}
		if ((Object)(object)val4 == (Object)null)
		{
			val4 = Utils.FindChild(((Component)this).transform, "r_foot", (IterativeSearchType)0);
		}
		if ((Object)(object)val4 == (Object)null)
		{
			val4 = Utils.FindChild(((Component)this).transform, "Foot.r", (IterativeSearchType)0);
		}
		if ((Object)(object)val4 == (Object)null)
		{
			val4 = Utils.FindChild(((Component)this).transform, "foot.r", (IterativeSearchType)0);
		}
		List<Transform> list = new List<Transform>();
		if (Object.op_Implicit((Object)(object)val))
		{
			list.Add(val);
		}
		if (Object.op_Implicit((Object)(object)val2))
		{
			list.Add(val2);
		}
		if (Object.op_Implicit((Object)(object)val3))
		{
			list.Add(val3);
		}
		if (Object.op_Implicit((Object)(object)val4))
		{
			list.Add(val4);
		}
		m_feet = list.ToArray();
	}

	private int FindBestStepEffect(GroundMaterial material, MotionType motion)
	{
		StepEffect stepEffect = null;
		int result = -1;
		for (int i = 0; i < m_effects.Count; i++)
		{
			StepEffect stepEffect2 = m_effects[i];
			if (((stepEffect2.m_material & material) != 0 || (stepEffect == null && (stepEffect2.m_material & GroundMaterial.Default) != 0)) && (stepEffect2.m_motionType & motion) != 0)
			{
				stepEffect = stepEffect2;
				result = i;
			}
		}
		return result;
	}
}
