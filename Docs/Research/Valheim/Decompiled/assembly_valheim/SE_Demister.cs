using UnityEngine;

public class SE_Demister : StatusEffect
{
	[Header("SE_Demister")]
	public GameObject m_ballPrefab;

	public Vector3 m_offset = new Vector3(0f, 2f, 0f);

	public Vector3 m_offsetInterior = new Vector3(0.5f, 1.8f, 0f);

	public float m_maxDistance = 50f;

	public float m_ballAcceleration = 4f;

	public float m_ballMaxSpeed = 10f;

	public float m_ballFriction = 0.1f;

	public float m_noiseDistance = 1f;

	public float m_noiseDistanceInterior = 0.2f;

	public float m_noiseDistanceYScale = 1f;

	public float m_noiseSpeed = 1f;

	public float m_characterVelocityFactor = 1f;

	public float m_rotationSpeed = 1f;

	private int m_coverRayMask;

	private GameObject m_ballInstance;

	private Vector3 m_ballVel = new Vector3(0f, 0f, 0f);

	public override void Setup(Character character)
	{
		base.Setup(character);
		if (m_coverRayMask == 0)
		{
			m_coverRayMask = LayerMask.GetMask(new string[5] { "Default", "static_solid", "Default_small", "piece", "terrain" });
		}
	}

	private bool IsUnderRoof()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(m_character.GetCenterPoint(), Vector3.up, ref val, 4f, m_coverRayMask))
		{
			return true;
		}
		return false;
	}

	public override void UpdateStatusEffect(float dt)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateStatusEffect(dt);
		if (!Object.op_Implicit((Object)(object)m_ballInstance))
		{
			Vector3 val = m_character.GetCenterPoint() + ((Component)m_character).transform.forward * 0.5f;
			m_ballInstance = Object.Instantiate<GameObject>(m_ballPrefab, val, Quaternion.identity);
			return;
		}
		_ = m_character;
		bool num = IsUnderRoof();
		Vector3 position = ((Component)m_character).transform.position;
		Vector3 val2 = m_ballInstance.transform.position;
		Vector3 val3 = (num ? m_offsetInterior : m_offset);
		float num2 = (num ? m_noiseDistanceInterior : m_noiseDistance);
		Vector3 val4 = position + ((Component)m_character).transform.TransformVector(val3);
		float num3 = Time.time * m_noiseSpeed;
		val4 += new Vector3(Mathf.Sin(num3 * 4f), Mathf.Sin(num3 * 2f) * m_noiseDistanceYScale, Mathf.Cos(num3 * 5f)) * num2;
		float num4 = Vector3.Distance(val4, val2);
		Vector3 val5;
		if (num4 > m_maxDistance * 2f)
		{
			val2 = val4;
		}
		else if (num4 > m_maxDistance)
		{
			val5 = val2 - val4;
			Vector3 normalized = ((Vector3)(ref val5)).normalized;
			val2 = val4 + normalized * m_maxDistance;
		}
		val5 = val4 - val2;
		Vector3 normalized2 = ((Vector3)(ref val5)).normalized;
		m_ballVel += normalized2 * m_ballAcceleration * dt;
		if (((Vector3)(ref m_ballVel)).magnitude > m_ballMaxSpeed)
		{
			m_ballVel = ((Vector3)(ref m_ballVel)).normalized * m_ballMaxSpeed;
		}
		if (!num)
		{
			Vector3 velocity = m_character.GetVelocity();
			m_ballVel += velocity * m_characterVelocityFactor * dt;
		}
		m_ballVel -= m_ballVel * m_ballFriction;
		Vector3 position2 = val2 + m_ballVel * dt;
		m_ballInstance.transform.position = position2;
		Quaternion rotation = m_ballInstance.transform.rotation;
		rotation *= Quaternion.Euler(m_rotationSpeed, 0f, m_rotationSpeed * 0.5321f);
		m_ballInstance.transform.rotation = rotation;
	}

	private void RemoveEffects()
	{
		if ((Object)(object)m_ballInstance != (Object)null)
		{
			ZNetView component = m_ballInstance.GetComponent<ZNetView>();
			if (component.IsValid())
			{
				component.ClaimOwnership();
				component.Destroy();
			}
		}
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
		m_ballInstance = null;
	}

	public override void Stop()
	{
		base.Stop();
		RemoveEffects();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		RemoveEffects();
	}
}
