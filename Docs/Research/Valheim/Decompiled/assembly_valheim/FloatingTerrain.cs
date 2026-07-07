using System;
using UnityEngine;

public class FloatingTerrain : MonoBehaviour
{
	public float m_padding;

	public float m_waveMinOffset;

	public float m_waveFreq;

	public float m_waveAmp;

	public FloatingTerrainDummy m_dummy;

	public float m_maxCorrectionSpeed = 0.025f;

	public bool m_copyLayer = true;

	private Rigidbody m_body;

	[NonSerialized]
	public Rigidbody m_dummyBody;

	private BoxCollider m_collider;

	private BoxCollider m_dummyCollider;

	private Heightmap m_lastHeightmap;

	private Vector3 m_lastGroundNormal;

	private float m_targetOffset;

	private float m_currentOffset;

	private float m_lastHeightmapTime;

	private float m_waveTime;

	private void Start()
	{
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_collider = ((Component)this).GetComponentInChildren<BoxCollider>();
		((MonoBehaviour)this).InvokeRepeating("UpdateTerrain", Random.Range(0.1f, 0.4f), 0.24f);
		UpdateTerrain();
	}

	private void UpdateTerrain()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_lastHeightmap))
		{
			m_targetOffset = 0f;
			return;
		}
		m_targetOffset = m_lastHeightmap.GetHeightOffset(((Component)this).transform.position) + m_padding;
		if (!Object.op_Implicit((Object)(object)m_dummy))
		{
			GameObject val = new GameObject();
			if (m_copyLayer)
			{
				val.layer = ((Component)this).gameObject.layer;
			}
			m_dummy = val.AddComponent<FloatingTerrainDummy>();
			m_dummy.m_parent = this;
			m_dummyBody = val.AddComponent<Rigidbody>();
			m_dummyBody.mass = m_body.mass;
			m_dummyBody.linearDamping = m_body.linearDamping;
			m_dummyBody.angularDamping = m_body.angularDamping;
			m_dummyBody.constraints = m_body.constraints;
			m_dummyCollider = val.AddComponent<BoxCollider>();
			m_dummyCollider.center = m_collider.center;
			m_dummyCollider.size = m_collider.size;
			if ((Object)(object)((Component)m_collider).gameObject != (Object)(object)this)
			{
				m_dummyCollider.size = Vector3.Scale(m_collider.size, ((Component)m_collider).transform.localScale);
				m_dummyCollider.center = Vector3.Scale(m_collider.center, ((Component)m_collider).transform.localScale);
				BoxCollider dummyCollider = m_dummyCollider;
				dummyCollider.center -= ((Component)m_collider).transform.localPosition;
			}
			val.transform.parent = ((Component)this).transform.parent;
			val.transform.position = ((Component)this).transform.position;
			((Collider)m_collider).isTrigger = true;
			Object.Destroy((Object)(object)m_body);
		}
	}

	private void FixedUpdate()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_dummy))
		{
			float maxCorrectionSpeed = m_maxCorrectionSpeed;
			float num = m_targetOffset - m_currentOffset;
			m_currentOffset += Mathf.Clamp(num, 0f - maxCorrectionSpeed, maxCorrectionSpeed);
			float num2 = m_currentOffset;
			if (m_waveFreq > 0f && num2 > m_waveMinOffset)
			{
				m_waveTime += Time.fixedDeltaTime;
				num2 += Mathf.Cos(m_waveTime * m_waveFreq) * m_waveAmp;
			}
			((Component)this).transform.position = ((Component)m_dummy).transform.position + new Vector3(0f, num2, 0f);
			((Component)this).transform.rotation = ((Component)m_dummy).transform.rotation;
		}
	}

	public void OnDummyCollision(Collision collision)
	{
		OnCollisionStay(collision);
	}

	private void OnCollisionStay(Collision collision)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Heightmap component = collision.gameObject.GetComponent<Heightmap>();
		if (component != null)
		{
			m_lastGroundNormal = ((ContactPoint)(ref collision.contacts[0])).normal;
			m_lastHeightmapTime = Time.time;
			if ((Object)(object)m_lastHeightmap != (Object)(object)component)
			{
				m_lastHeightmap = component;
				UpdateTerrain();
			}
		}
		else if (m_lastHeightmapTime > 0.2f)
		{
			m_lastHeightmap = null;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_dummyCollider) && ((Collider)m_dummyCollider).enabled)
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = Matrix4x4.TRS(((Component)m_dummyCollider).transform.position, ((Component)m_dummyCollider).transform.rotation, ((Component)m_dummyCollider).transform.lossyScale);
			Gizmos.DrawWireCube(m_dummyCollider.center, m_dummyCollider.size);
		}
		if ((Object)(object)m_dummy != (Object)null)
		{
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + new Vector3(0f, m_currentOffset, 0f));
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)m_dummy))
		{
			Object.Destroy((Object)(object)((Component)m_dummy).gameObject);
		}
	}

	public static Rigidbody GetBody(GameObject obj)
	{
		FloatingTerrain component = obj.GetComponent<FloatingTerrain>();
		if (component != null && Object.op_Implicit((Object)(object)component.m_dummy) && Object.op_Implicit((Object)(object)component.m_dummyBody))
		{
			return component.m_dummyBody;
		}
		return null;
	}
}
