using System;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour, IMonoUpdater
{
	private class TailSegment
	{
		public Transform transform;

		public Vector3 pos;

		public Quaternion rot;

		public float distance;
	}

	public List<Transform> m_tailJoints = new List<Transform>();

	public float m_maxAngle = 80f;

	public float m_gravity = 2f;

	public float m_gravityInWater = 0.1f;

	public bool m_waterSurfaceCheck;

	public bool m_groundCheck;

	public float m_smoothness = 0.1f;

	public float m_tailRadius;

	public Character m_character;

	public Rigidbody m_characterBody;

	public Rigidbody m_tailBody;

	private readonly List<TailSegment> m_positions = new List<TailSegment>();

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform tailJoint in m_tailJoints)
		{
			float distance = Vector3.Distance(tailJoint.parent.position, tailJoint.position);
			Vector3 position = tailJoint.position;
			TailSegment tailSegment = new TailSegment();
			tailSegment.transform = tailJoint;
			tailSegment.pos = position;
			tailSegment.rot = tailJoint.rotation;
			tailSegment.distance = distance;
			m_positions.Add(tailSegment);
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

	public void CustomLateUpdate(float dt)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_positions.Count; i++)
		{
			TailSegment tailSegment = m_positions[i];
			if (m_waterSurfaceCheck)
			{
				float liquidLevel = Floating.GetLiquidLevel(tailSegment.pos);
				if (tailSegment.pos.y + m_tailRadius > liquidLevel)
				{
					tailSegment.pos.y -= m_gravity * dt;
				}
				else
				{
					tailSegment.pos.y -= m_gravityInWater * dt;
				}
			}
			else
			{
				tailSegment.pos.y -= m_gravity * dt;
			}
			Vector3 val = tailSegment.transform.parent.position + tailSegment.transform.parent.up * tailSegment.distance * 0.5f;
			Vector3 val2 = Vector3.Normalize(val - tailSegment.pos);
			val2 = Vector3.RotateTowards(-tailSegment.transform.parent.up, val2, (float)Math.PI / 180f * m_maxAngle, 1f);
			Vector3 val3 = val - val2 * tailSegment.distance * 0.5f;
			if (m_groundCheck)
			{
				float groundHeight = ZoneSystem.instance.GetGroundHeight(val3);
				if (val3.y - m_tailRadius < groundHeight)
				{
					val3.y = groundHeight + m_tailRadius;
				}
			}
			val3 = Vector3.Lerp(tailSegment.pos, val3, m_smoothness);
			if (val == val3)
			{
				break;
			}
			Vector3 val4 = val - val3;
			Vector3 normalized = ((Vector3)(ref val4)).normalized;
			Vector3 val5 = Vector3.Cross(Vector3.up, -normalized);
			Quaternion val6 = Quaternion.LookRotation(Vector3.Cross(-normalized, val5), -normalized);
			val6 = Quaternion.Slerp(tailSegment.rot, val6, m_smoothness);
			tailSegment.transform.position = val3;
			tailSegment.transform.rotation = val6;
			tailSegment.pos = val3;
			tailSegment.rot = val6;
		}
	}
}
