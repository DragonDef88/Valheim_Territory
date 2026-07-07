using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimEvent : MonoBehaviour, IMonoUpdater
{
	[Serializable]
	public class Foot
	{
		public Transform m_transform;

		public AvatarIKGoal m_ikHandle;

		public float m_footDownMax = 0.4f;

		public float m_footOffset = 0.1f;

		public float m_footStepHeight = 1f;

		public float m_stabalizeDistance;

		[NonSerialized]
		public float m_ikWeight;

		[NonSerialized]
		public Vector3 m_plantPosition = Vector3.zero;

		[NonSerialized]
		public Vector3 m_plantNormal = Vector3.up;

		[NonSerialized]
		public bool m_isPlanted;

		public Foot(Transform t, AvatarIKGoal handle)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			m_transform = t;
			m_ikHandle = handle;
			m_ikWeight = 0f;
		}
	}

	[Header("Foot IK")]
	public bool m_footIK;

	public float m_footDownMax = 0.4f;

	public float m_footOffset = 0.1f;

	public float m_footStepHeight = 1f;

	public float m_stabalizeDistance;

	public bool m_useFeetValues;

	public Foot[] m_feets = Array.Empty<Foot>();

	[Header("Head/eye rotation")]
	public bool m_headRotation = true;

	public Transform[] m_eyes;

	public float m_lookWeight = 0.5f;

	public float m_bodyLookWeight = 0.1f;

	public float m_headLookWeight = 1f;

	public float m_eyeLookWeight;

	public float m_lookClamp = 0.5f;

	private const float m_headRotationSmoothness = 0.1f;

	public Transform m_lookAt;

	[Header("Player Female hack")]
	public bool m_femaleHack;

	public Transform m_leftShoulder;

	public Transform m_rightShoulder;

	public float m_femaleOffset = 0.0004f;

	public float m_maleOffset = 0.0007651657f;

	private Character m_character;

	private Animator m_animator;

	private ZNetView m_nview;

	private MonsterAI m_monsterAI;

	private VisEquipment m_visEquipment;

	private FootStep m_footStep;

	private float m_pauseTimer;

	private float m_pauseSpeed = 1f;

	private float m_sendTimer;

	private Vector3 m_headLookDir;

	private float m_lookAtWeight;

	private Transform m_head;

	private bool m_chain;

	private Vector3 m_lookTargetCached = Vector3.negativeInfinity;

	private static int s_ikGroundMask = 0;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		m_character = ((Component)this).GetComponentInParent<Character>();
		m_nview = ((Component)m_character).GetComponent<ZNetView>();
		m_animator = ((Component)this).GetComponent<Animator>();
		m_monsterAI = ((Component)m_character).GetComponent<MonsterAI>();
		m_visEquipment = ((Component)m_character).GetComponent<VisEquipment>();
		m_footStep = ((Component)m_character).GetComponent<FootStep>();
		m_head = Utils.GetBoneTransform(m_animator, (HumanBodyBones)10);
		m_headLookDir = ((Component)m_character).transform.forward;
		if (s_ikGroundMask == 0)
		{
			s_ikGroundMask = LayerMask.GetMask(new string[6] { "Default", "static_solid", "Default_small", "piece", "terrain", "vehicle" });
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

	private void OnAnimatorMove()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			m_character.AddRootMotion(m_animator.deltaPosition);
		}
	}

	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		if (!((Object)(object)m_character == (Object)null) && m_nview.IsValid())
		{
			if (!m_character.InAttack() && !m_character.InMinorAction() && !m_character.InEmote() && m_character.CanMove())
			{
				m_animator.speed = 1f;
			}
			UpdateFreezeFrame(fixedDeltaTime);
		}
	}

	public bool CanChain()
	{
		return m_chain;
	}

	public void FreezeFrame(float delay)
	{
		if (delay <= 0f)
		{
			return;
		}
		if (m_pauseTimer > 0f)
		{
			m_pauseTimer = delay;
			return;
		}
		m_pauseTimer = delay;
		m_pauseSpeed = m_animator.speed;
		m_animator.speed = 0.0001f;
		if (m_pauseSpeed <= 0.01f)
		{
			m_pauseSpeed = 1f;
		}
	}

	private void UpdateFreezeFrame(float dt)
	{
		if (m_pauseTimer > 0f)
		{
			m_pauseTimer -= dt;
			if (m_pauseTimer <= 0f)
			{
				m_animator.speed = m_pauseSpeed;
			}
		}
		if (m_animator.speed < 0.01f && m_pauseTimer <= 0f)
		{
			m_animator.speed = 1f;
		}
	}

	public void Speed(float speedScale)
	{
		m_animator.speed = speedScale;
	}

	public void Chain()
	{
		m_chain = true;
	}

	public void ResetChain()
	{
		m_chain = false;
	}

	public void FootStep(AnimationEvent e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AnimatorClipInfo animatorClipInfo = e.animatorClipInfo;
		if (!((double)((AnimatorClipInfo)(ref animatorClipInfo)).weight < 0.33) && Object.op_Implicit((Object)(object)m_footStep))
		{
			if (e.stringParameter.Length > 0)
			{
				m_footStep.OnFoot(e.stringParameter);
			}
			else
			{
				m_footStep.OnFoot();
			}
		}
	}

	public void Hit()
	{
		m_character.OnAttackTrigger();
	}

	public void OnAttackTrigger()
	{
		m_character.OnAttackTrigger();
	}

	public void Jump()
	{
		m_character.Jump(force: true);
	}

	public void Land()
	{
		if (m_character.IsFlying())
		{
			m_character.Land();
		}
	}

	public void TakeOff()
	{
		if (!m_character.IsFlying())
		{
			m_character.TakeOff();
		}
	}

	public void Stop(AnimationEvent e)
	{
		m_character.OnStopMoving();
	}

	public void DodgeMortal()
	{
		Player player = m_character as Player;
		if (Object.op_Implicit((Object)(object)player))
		{
			player.OnDodgeMortal();
		}
	}

	public void TrailOn()
	{
		if (Object.op_Implicit((Object)(object)m_visEquipment))
		{
			m_visEquipment.SetWeaponTrails(enabled: true);
		}
		m_character.OnWeaponTrailStart();
	}

	public void TrailOff()
	{
		if (Object.op_Implicit((Object)(object)m_visEquipment))
		{
			m_visEquipment.SetWeaponTrails(enabled: false);
		}
	}

	public void GPower()
	{
		Player player = m_character as Player;
		if (Object.op_Implicit((Object)(object)player))
		{
			player.ActivateGuardianPower();
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (m_nview.IsValid())
		{
			UpdateLookat();
			UpdateFootIK();
		}
	}

	public void CustomLateUpdate(float deltaTime)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		UpdateHeadRotation(deltaTime);
		if (m_femaleHack)
		{
			_ = m_character;
			float num = ((m_visEquipment.GetModelIndex() == 1) ? m_femaleOffset : m_maleOffset);
			Vector3 localPosition = m_leftShoulder.localPosition;
			localPosition.x = 0f - num;
			m_leftShoulder.localPosition = localPosition;
			Vector3 localPosition2 = m_rightShoulder.localPosition;
			localPosition2.x = num;
			m_rightShoulder.localPosition = localPosition2;
		}
	}

	private void UpdateLookat()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (m_headRotation && Object.op_Implicit((Object)(object)m_head))
		{
			float num = m_lookWeight;
			if (m_headLookDir != Vector3.zero)
			{
				m_animator.SetLookAtPosition(m_head.position + m_headLookDir * 10f);
			}
			if (m_character.InAttack() || (!m_character.IsPlayer() && !m_character.CanMove()))
			{
				num = 0f;
			}
			m_lookAtWeight = Mathf.MoveTowards(m_lookAtWeight, num, Time.deltaTime);
			float num2 = (m_character.IsAttached() ? 0f : m_bodyLookWeight);
			m_animator.SetLookAtWeight(m_lookAtWeight, num2, m_headLookWeight, m_eyeLookWeight, m_lookClamp);
		}
	}

	private void UpdateFootIK()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		if (!m_footIK)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if ((Object)(object)mainCamera == (Object)null || Vector3.Distance(((Component)this).transform.position, ((Component)mainCamera).transform.position) > 64f)
		{
			return;
		}
		if ((m_character.IsFlying() && !m_character.IsOnGround()) || (m_character.IsSwimming() && !m_character.IsOnGround()) || m_character.IsSitting())
		{
			for (int i = 0; i < m_feets.Length; i++)
			{
				Foot foot = m_feets[i];
				m_animator.SetIKPositionWeight(foot.m_ikHandle, 0f);
				m_animator.SetIKRotationWeight(foot.m_ikHandle, 0f);
			}
			return;
		}
		bool flag = m_character.IsSitting();
		float deltaTime = Time.deltaTime;
		RaycastHit val2 = default(RaycastHit);
		for (int j = 0; j < m_feets.Length; j++)
		{
			Foot foot2 = m_feets[j];
			Vector3 position = foot2.m_transform.position;
			AvatarIKGoal ikHandle = foot2.m_ikHandle;
			float num = (m_useFeetValues ? foot2.m_footDownMax : m_footDownMax);
			float num2 = (m_useFeetValues ? foot2.m_footOffset : m_footOffset);
			float num3 = (m_useFeetValues ? foot2.m_footStepHeight : m_footStepHeight);
			float num4 = (m_useFeetValues ? foot2.m_stabalizeDistance : m_stabalizeDistance);
			if (flag)
			{
				num3 /= 4f;
			}
			Vector3 val = ((Component)this).transform.InverseTransformPoint(position - ((Component)this).transform.up * num2);
			float num5 = 1f - Mathf.Clamp01(val.y / num);
			foot2.m_ikWeight = Mathf.MoveTowards(foot2.m_ikWeight, num5, deltaTime * 10f);
			m_animator.SetIKPositionWeight(ikHandle, foot2.m_ikWeight);
			m_animator.SetIKRotationWeight(ikHandle, foot2.m_ikWeight * 0.5f);
			if (!(foot2.m_ikWeight > 0f))
			{
				continue;
			}
			if (Physics.Raycast(position + Vector3.up * num3, Vector3.down, ref val2, num3 * 4f, s_ikGroundMask))
			{
				Vector3 val3 = ((RaycastHit)(ref val2)).point + Vector3.up * num2;
				Vector3 normal = ((RaycastHit)(ref val2)).normal;
				if (num4 > 0f)
				{
					if (foot2.m_ikWeight >= 1f)
					{
						if (!foot2.m_isPlanted)
						{
							foot2.m_plantPosition = val3;
							foot2.m_plantNormal = normal;
							foot2.m_isPlanted = true;
						}
						else if (Vector3.Distance(foot2.m_plantPosition, val3) > num4)
						{
							foot2.m_isPlanted = false;
						}
						else
						{
							val3 = foot2.m_plantPosition;
							normal = foot2.m_plantNormal;
						}
					}
					else
					{
						foot2.m_isPlanted = false;
					}
				}
				m_animator.SetIKPosition(ikHandle, val3);
				Quaternion val4 = Quaternion.LookRotation(Vector3.Cross(m_animator.GetIKRotation(ikHandle) * Vector3.right, ((RaycastHit)(ref val2)).normal), ((RaycastHit)(ref val2)).normal);
				m_animator.SetIKRotation(ikHandle, val4);
			}
			else
			{
				foot2.m_ikWeight = Mathf.MoveTowards(foot2.m_ikWeight, 0f, deltaTime * 4f);
				m_animator.SetIKPositionWeight(ikHandle, foot2.m_ikWeight);
				m_animator.SetIKRotationWeight(ikHandle, foot2.m_ikWeight * 0.5f);
			}
		}
	}

	private void UpdateHeadRotation(float dt)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview == (Object)null || !m_nview.IsValid() || !m_headRotation || !Object.op_Implicit((Object)(object)m_head))
		{
			return;
		}
		Vector3 lookFromPos = GetLookFromPos();
		Vector3 val = Vector3.zero;
		if (m_nview.IsOwner())
		{
			if ((Object)(object)m_monsterAI != (Object)null)
			{
				Character targetCreature = m_monsterAI.GetTargetCreature();
				if ((Object)(object)targetCreature != (Object)null)
				{
					val = targetCreature.GetEyePoint();
				}
			}
			else
			{
				val = lookFromPos + m_character.GetLookDir() * 100f;
			}
			if ((Object)(object)m_lookAt != (Object)null)
			{
				val = m_lookAt.position;
			}
			m_sendTimer += Time.deltaTime;
			if (m_sendTimer > 0.2f)
			{
				m_sendTimer = 0f;
				if (!((Vector3)(ref val)).Equals(m_lookTargetCached))
				{
					m_nview.GetZDO().Set(ZDOVars.s_lookTarget, val);
				}
				m_lookTargetCached = val;
			}
		}
		else
		{
			val = m_nview.GetZDO().GetVec3(ZDOVars.s_lookTarget, Vector3.zero);
		}
		if (val != Vector3.zero)
		{
			Vector3 val2 = Vector3.Normalize(val - lookFromPos);
			m_headLookDir = Vector3.Lerp(m_headLookDir, val2, 0.1f);
		}
		else
		{
			m_headLookDir = ((Component)m_character).transform.forward;
		}
	}

	private Vector3 GetLookFromPos()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (m_eyes != null && m_eyes.Length != 0)
		{
			Vector3 val = Vector3.zero;
			Transform[] eyes = m_eyes;
			foreach (Transform val2 in eyes)
			{
				val += val2.position;
			}
			return val / (float)m_eyes.Length;
		}
		return m_head.position;
	}

	public void FindJoints()
	{
		ZLog.Log((object)"Finding joints");
		List<Transform> list = new List<Transform>();
		Transform val = Utils.FindChild(((Component)this).transform, "LeftEye", (IterativeSearchType)0);
		Transform val2 = Utils.FindChild(((Component)this).transform, "RightEye", (IterativeSearchType)0);
		if (Object.op_Implicit((Object)(object)val))
		{
			list.Add(val);
		}
		if (Object.op_Implicit((Object)(object)val2))
		{
			list.Add(val2);
		}
		m_eyes = list.ToArray();
		Transform val3 = Utils.FindChild(((Component)this).transform, "LeftFootFront", (IterativeSearchType)0);
		Transform val4 = Utils.FindChild(((Component)this).transform, "RightFootFront", (IterativeSearchType)0);
		Transform val5 = Utils.FindChild(((Component)this).transform, "LeftFoot", (IterativeSearchType)0);
		if ((Object)(object)val5 == (Object)null)
		{
			val5 = Utils.FindChild(((Component)this).transform, "LeftFootBack", (IterativeSearchType)0);
		}
		if ((Object)(object)val5 == (Object)null)
		{
			val5 = Utils.FindChild(((Component)this).transform, "l_foot", (IterativeSearchType)0);
		}
		if ((Object)(object)val5 == (Object)null)
		{
			val5 = Utils.FindChild(((Component)this).transform, "Foot.l", (IterativeSearchType)0);
		}
		if ((Object)(object)val5 == (Object)null)
		{
			val5 = Utils.FindChild(((Component)this).transform, "foot.l", (IterativeSearchType)0);
		}
		Transform val6 = Utils.FindChild(((Component)this).transform, "RightFoot", (IterativeSearchType)0);
		if ((Object)(object)val6 == (Object)null)
		{
			val6 = Utils.FindChild(((Component)this).transform, "RightFootBack", (IterativeSearchType)0);
		}
		if ((Object)(object)val6 == (Object)null)
		{
			val6 = Utils.FindChild(((Component)this).transform, "r_foot", (IterativeSearchType)0);
		}
		if ((Object)(object)val6 == (Object)null)
		{
			val6 = Utils.FindChild(((Component)this).transform, "Foot.r", (IterativeSearchType)0);
		}
		if ((Object)(object)val6 == (Object)null)
		{
			val6 = Utils.FindChild(((Component)this).transform, "foot.r", (IterativeSearchType)0);
		}
		List<Foot> list2 = new List<Foot>();
		if (Object.op_Implicit((Object)(object)val3))
		{
			list2.Add(new Foot(val3, (AvatarIKGoal)2));
		}
		if (Object.op_Implicit((Object)(object)val4))
		{
			list2.Add(new Foot(val4, (AvatarIKGoal)3));
		}
		if (Object.op_Implicit((Object)(object)val5))
		{
			list2.Add(new Foot(val5, (AvatarIKGoal)0));
		}
		if (Object.op_Implicit((Object)(object)val6))
		{
			list2.Add(new Foot(val6, (AvatarIKGoal)1));
		}
		m_feets = list2.ToArray();
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		if (!m_footIK)
		{
			return;
		}
		Foot[] feets = m_feets;
		foreach (Foot foot in feets)
		{
			float num = (m_useFeetValues ? foot.m_footDownMax : m_footDownMax);
			float num2 = (m_useFeetValues ? foot.m_footOffset : m_footOffset);
			float num3 = (m_useFeetValues ? foot.m_footStepHeight : m_footStepHeight);
			float num4 = (m_useFeetValues ? foot.m_stabalizeDistance : m_stabalizeDistance);
			Vector3 val = foot.m_transform.position - ((Component)this).transform.up * num2;
			Gizmos.color = ((val.y > ((Component)this).transform.position.y) ? Color.red : Color.white);
			Gizmos.DrawWireSphere(val, 0.1f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(new Vector3(val.x, ((Component)this).transform.position.y, val.z) + Vector3.up * num, new Vector3(1f, 0.01f, 1f));
			Gizmos.color = Color.red;
			Gizmos.DrawLine(val, val + Vector3.up * num3);
			if (num4 > 0f)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(val, num4);
				Gizmos.matrix = Matrix4x4.identity;
			}
			if (foot.m_isPlanted)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(val, new Vector3(0.4f, 0.3f, 0.4f));
			}
		}
	}
}
