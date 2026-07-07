using System.Collections.Generic;
using UnityEngine;

public class ZSyncTransform : MonoBehaviour, IMonoUpdater
{
	public bool m_syncPosition = true;

	public bool m_syncRotation = true;

	public bool m_syncScale;

	public bool m_syncBodyVelocity;

	public bool m_characterParentSync;

	private const float m_smoothnessPos = 0.2f;

	private const float m_smoothnessRot = 0.5f;

	private bool m_isKinematicBody;

	private bool m_useGravity = true;

	private Vector3 m_tempRelPos;

	private bool m_haveTempRelPos;

	private float m_targetPosTimer;

	private uint m_posRevision = uint.MaxValue;

	private int m_lastUpdateFrame = -1;

	private bool m_wasOwner;

	private ZNetView m_nview;

	private Rigidbody m_body;

	private Projectile m_projectile;

	private Character m_character;

	private ZDOID m_tempParent = ZDOID.None;

	private ZDOID m_tempParentCached;

	private string m_tempAttachJoint;

	private Vector3 m_tempRelativePos;

	private Quaternion m_tempRelativeRot;

	private Vector3 m_tempRelativeVel;

	private Vector3 m_tempRelativePosCached;

	private Quaternion m_tempRelativeRotCached;

	private Vector3 m_tempRelativeVelCached;

	private Vector3 m_positionCached = Vector3.negativeInfinity;

	private Vector3 m_velocityCached = Vector3.negativeInfinity;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_projectile = ((Component)this).GetComponent<Projectile>();
		m_character = ((Component)this).GetComponent<Character>();
		if (m_nview.GetZDO() == null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		if (Object.op_Implicit((Object)(object)m_body))
		{
			m_isKinematicBody = m_body.isKinematic;
			m_useGravity = m_body.useGravity;
		}
		m_wasOwner = m_nview.GetZDO().IsOwner();
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	private Vector3 GetVelocity()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_body != (Object)null)
		{
			return m_body.linearVelocity;
		}
		if ((Object)(object)m_projectile != (Object)null)
		{
			return m_projectile.GetVelocity();
		}
		return Vector3.zero;
	}

	private Vector3 GetPosition()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_body))
		{
			return ((Component)this).transform.position;
		}
		return m_body.position;
	}

	private void OwnerSync()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		ZDO zDO = m_nview.GetZDO();
		bool flag = zDO.IsOwner();
		bool flag2 = !m_wasOwner && flag;
		m_wasOwner = flag;
		if (!flag)
		{
			return;
		}
		if (flag2)
		{
			bool flag3 = false;
			if (m_syncPosition)
			{
				((Component)this).transform.position = zDO.GetPosition();
				flag3 = true;
			}
			if (m_syncRotation)
			{
				((Component)this).transform.rotation = zDO.GetRotation();
				flag3 = true;
			}
			if (m_syncBodyVelocity && Object.op_Implicit((Object)(object)m_body))
			{
				m_body.linearVelocity = zDO.GetVec3(ZDOVars.s_bodyVelHash, Vector3.zero);
				m_body.angularVelocity = zDO.GetVec3(ZDOVars.s_bodyAVelHash, Vector3.zero);
			}
			if (flag3 && Object.op_Implicit((Object)(object)m_body))
			{
				Physics.SyncTransforms();
			}
		}
		if (((Component)this).transform.position.y < -5000f)
		{
			if (Object.op_Implicit((Object)(object)m_body))
			{
				m_body.linearVelocity = Vector3.zero;
			}
			ZLog.Log((object)("Object fell out of world:" + ((Object)((Component)this).gameObject).name));
			float groundHeight = ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position);
			Vector3 position = ((Component)this).transform.position;
			position.y = groundHeight + 1f;
			((Component)this).transform.position = position;
			if (Object.op_Implicit((Object)(object)m_body))
			{
				Physics.SyncTransforms();
			}
			return;
		}
		if (m_syncPosition)
		{
			Vector3 position2 = GetPosition();
			if (!((Vector3)(ref m_positionCached)).Equals(position2))
			{
				zDO.SetPosition(position2);
			}
			Vector3 velocity = GetVelocity();
			if (!((Vector3)(ref m_velocityCached)).Equals(velocity))
			{
				zDO.Set(ZDOVars.s_velHash, velocity);
			}
			m_positionCached = position2;
			m_velocityCached = velocity;
			if (m_characterParentSync)
			{
				if (GetRelativePosition(zDO, out m_tempParent, out m_tempAttachJoint, out m_tempRelativePos, out m_tempRelativeRot, out m_tempRelativeVel))
				{
					if (m_tempParent != m_tempParentCached)
					{
						zDO.SetConnection(ZDOExtraData.ConnectionType.SyncTransform, m_tempParent);
						zDO.Set(ZDOVars.s_attachJointHash, m_tempAttachJoint);
					}
					if (!((Vector3)(ref m_tempRelativePos)).Equals(m_tempRelativePosCached))
					{
						zDO.Set(ZDOVars.s_relPosHash, m_tempRelativePos);
					}
					if (!((Quaternion)(ref m_tempRelativeRot)).Equals(m_tempRelativeRotCached))
					{
						zDO.Set(ZDOVars.s_relRotHash, m_tempRelativeRot);
					}
					if (!((Vector3)(ref m_tempRelativeVel)).Equals(m_tempRelativeVelCached))
					{
						zDO.Set(ZDOVars.s_velHash, m_tempRelativeVel);
					}
					m_tempRelativePosCached = m_tempRelativePos;
					m_tempRelativeRotCached = m_tempRelativeRot;
					m_tempRelativeVelCached = m_tempRelativeVel;
				}
				else if (m_tempParent != m_tempParentCached)
				{
					zDO.UpdateConnection(ZDOExtraData.ConnectionType.SyncTransform, ZDOID.None);
					zDO.Set(ZDOVars.s_attachJointHash, "");
				}
				m_tempParentCached = m_tempParent;
			}
		}
		if (m_syncRotation && ((Component)this).transform.hasChanged)
		{
			Quaternion rotation = (Object.op_Implicit((Object)(object)m_body) ? m_body.rotation : ((Component)this).transform.rotation);
			zDO.SetRotation(rotation);
		}
		if (m_syncScale && ((Component)this).transform.hasChanged)
		{
			if (Mathf.Approximately(((Component)this).transform.localScale.x, ((Component)this).transform.localScale.y) && Mathf.Approximately(((Component)this).transform.localScale.x, ((Component)this).transform.localScale.z))
			{
				zDO.RemoveVec3(ZDOVars.s_scaleHash);
				zDO.Set(ZDOVars.s_scaleScalarHash, ((Component)this).transform.localScale.x);
			}
			else
			{
				zDO.RemoveFloat(ZDOVars.s_scaleScalarHash);
				zDO.Set(ZDOVars.s_scaleHash, ((Component)this).transform.localScale);
			}
		}
		if (Object.op_Implicit((Object)(object)m_body))
		{
			if (m_syncBodyVelocity)
			{
				m_nview.GetZDO().Set(ZDOVars.s_bodyVelHash, m_body.linearVelocity);
				m_nview.GetZDO().Set(ZDOVars.s_bodyAVelHash, m_body.angularVelocity);
			}
			m_body.useGravity = m_useGravity;
		}
		((Component)this).transform.hasChanged = false;
	}

	private bool GetRelativePosition(ZDO zdo, out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_character))
		{
			return m_character.GetRelativePosition(out parent, out attachJoint, out relativePos, out relativeRot, out relativeVel);
		}
		if (Object.op_Implicit((Object)(object)((Component)this).transform.parent))
		{
			ZNetView zNetView = (Object.op_Implicit((Object)(object)((Component)this).transform.parent) ? ((Component)((Component)this).transform.parent).GetComponent<ZNetView>() : null);
			if (Object.op_Implicit((Object)(object)zNetView) && zNetView.IsValid())
			{
				parent = zNetView.GetZDO().m_uid;
				attachJoint = "";
				relativePos = ((Component)this).transform.localPosition;
				relativeRot = ((Component)this).transform.localRotation;
				relativeVel = Vector3.zero;
				return true;
			}
		}
		parent = ZDOID.None;
		attachJoint = "";
		relativePos = Vector3.zero;
		relativeRot = Quaternion.identity;
		relativeVel = Vector3.zero;
		return false;
	}

	private void SyncPosition(ZDO zdo, float dt, out bool usedLocalRotation)
	{
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		usedLocalRotation = false;
		if (m_characterParentSync && zdo.HasOwner())
		{
			ZDOID connectionZDOID = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.SyncTransform);
			if (!connectionZDOID.IsNone())
			{
				GameObject val = ZNetScene.instance.FindInstance(connectionZDOID);
				if (Object.op_Implicit((Object)(object)val))
				{
					ZSyncTransform component = val.GetComponent<ZSyncTransform>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.ClientSync(dt);
					}
					string @string = zdo.GetString(ZDOVars.s_attachJointHash);
					Vector3 vec = zdo.GetVec3(ZDOVars.s_relPosHash, Vector3.zero);
					Quaternion quaternion = zdo.GetQuaternion(ZDOVars.s_relRotHash, Quaternion.identity);
					Vector3 vec2 = zdo.GetVec3(ZDOVars.s_velHash, Vector3.zero);
					bool flag = false;
					if (zdo.DataRevision != m_posRevision)
					{
						m_posRevision = zdo.DataRevision;
						m_targetPosTimer = 0f;
					}
					if (@string.Length > 0)
					{
						Transform val2 = Utils.FindChild(val.transform, @string, (IterativeSearchType)0);
						if (Object.op_Implicit((Object)(object)val2))
						{
							((Component)this).transform.position = val2.position;
							flag = true;
						}
					}
					else
					{
						m_targetPosTimer += dt;
						m_targetPosTimer = Mathf.Min(m_targetPosTimer, 2f);
						vec += vec2 * m_targetPosTimer;
						if (!m_haveTempRelPos)
						{
							m_haveTempRelPos = true;
							m_tempRelPos = vec;
						}
						if (Vector3.Distance(m_tempRelPos, vec) > 0.001f)
						{
							m_tempRelPos = Vector3.Lerp(m_tempRelPos, vec, 0.2f);
							vec = m_tempRelPos;
						}
						Vector3 val3 = val.transform.TransformPoint(vec);
						if (Vector3.Distance(((Component)this).transform.position, val3) > 0.001f)
						{
							((Component)this).transform.position = val3;
							flag = true;
						}
					}
					Quaternion val4 = Quaternion.Inverse(val.transform.rotation) * ((Component)this).transform.rotation;
					if (Quaternion.Angle(val4, quaternion) > 0.001f)
					{
						Quaternion val5 = Quaternion.Slerp(val4, quaternion, 0.5f);
						((Component)this).transform.rotation = val.transform.rotation * val5;
						flag = true;
					}
					usedLocalRotation = true;
					if (flag && Object.op_Implicit((Object)(object)m_body))
					{
						Physics.SyncTransforms();
					}
					return;
				}
			}
		}
		m_haveTempRelPos = false;
		Vector3 val6 = zdo.GetPosition();
		if (zdo.DataRevision != m_posRevision)
		{
			m_posRevision = zdo.DataRevision;
			m_targetPosTimer = 0f;
		}
		if (zdo.HasOwner())
		{
			m_targetPosTimer += dt;
			m_targetPosTimer = Mathf.Min(m_targetPosTimer, 2f);
			Vector3 vec3 = zdo.GetVec3(ZDOVars.s_velHash, Vector3.zero);
			val6 += vec3 * m_targetPosTimer;
		}
		float num = Vector3.Distance(((Component)this).transform.position, val6);
		if (num > 0.001f)
		{
			((Component)this).transform.position = ((num < 5f) ? Vector3.Lerp(((Component)this).transform.position, val6, 0.2f) : val6);
			if (Object.op_Implicit((Object)(object)m_body))
			{
				Physics.SyncTransforms();
			}
		}
	}

	private void ClientSync(float dt)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		ZDO zDO = m_nview.GetZDO();
		if (zDO.IsOwner())
		{
			return;
		}
		int frameCount = Time.frameCount;
		if (m_lastUpdateFrame == frameCount)
		{
			return;
		}
		m_lastUpdateFrame = frameCount;
		if (m_isKinematicBody)
		{
			if (m_syncPosition)
			{
				Vector3 val = zDO.GetPosition();
				if (Vector3.Distance(m_body.position, val) > 5f)
				{
					m_body.position = val;
				}
				else
				{
					if (Vector3.Distance(m_body.position, val) > 0.01f)
					{
						val = Vector3.Lerp(m_body.position, val, 0.2f);
					}
					m_body.MovePosition(val);
				}
			}
			if (m_syncRotation)
			{
				Quaternion rotation = zDO.GetRotation();
				if (Quaternion.Angle(m_body.rotation, rotation) > 45f)
				{
					m_body.rotation = rotation;
				}
				else
				{
					m_body.MoveRotation(rotation);
				}
			}
		}
		else
		{
			bool usedLocalRotation = false;
			if (m_syncPosition)
			{
				SyncPosition(zDO, dt, out usedLocalRotation);
			}
			if (m_syncRotation && !usedLocalRotation)
			{
				Quaternion rotation2 = zDO.GetRotation();
				if (Quaternion.Angle(((Component)this).transform.rotation, rotation2) > 0.001f)
				{
					((Component)this).transform.rotation = Quaternion.Slerp(((Component)this).transform.rotation, rotation2, 0.5f);
				}
			}
			if (Object.op_Implicit((Object)(object)m_body))
			{
				m_body.useGravity = false;
				if (m_syncBodyVelocity && m_nview.HasOwner())
				{
					Vector3 vec = zDO.GetVec3(ZDOVars.s_bodyVelHash, Vector3.zero);
					Vector3 vec2 = zDO.GetVec3(ZDOVars.s_bodyAVelHash, Vector3.zero);
					if (((Vector3)(ref vec)).magnitude > 0.01f || ((Vector3)(ref vec2)).magnitude > 0.01f)
					{
						m_body.linearVelocity = vec;
						m_body.angularVelocity = vec2;
					}
					else
					{
						m_body.Sleep();
					}
				}
				else if (!m_body.IsSleeping())
				{
					m_body.linearVelocity = Vector3.zero;
					m_body.angularVelocity = Vector3.zero;
					m_body.Sleep();
				}
			}
		}
		if (!m_syncScale)
		{
			return;
		}
		Vector3 vec3 = zDO.GetVec3(ZDOVars.s_scaleHash, Vector3.zero);
		if (vec3 != Vector3.zero)
		{
			((Component)this).transform.localScale = vec3;
			return;
		}
		float @float = zDO.GetFloat(ZDOVars.s_scaleScalarHash, ((Component)this).transform.localScale.x);
		if (!((Component)this).transform.localScale.x.Equals(@float))
		{
			((Component)this).transform.localScale = new Vector3(@float, @float, @float);
		}
	}

	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		if (m_nview.IsValid())
		{
			ClientSync(fixedDeltaTime);
		}
	}

	public void CustomLateUpdate(float deltaTime)
	{
		if (m_nview.IsValid())
		{
			OwnerSync();
		}
	}

	public void SyncNow()
	{
		if (m_nview.IsValid())
		{
			OwnerSync();
		}
	}
}
