using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ship : MonoBehaviour, IMonoUpdater
{
	public enum Speed
	{
		Stop,
		Back,
		Slow,
		Half,
		Full
	}

	private bool m_forwardPressed;

	private bool m_backwardPressed;

	private float m_sendRudderTime;

	private float m_ashDamageMsgTimer;

	public float m_ashDamageMsgTime = 10f;

	[Header("Objects")]
	public GameObject m_sailObject;

	public GameObject m_mastObject;

	public GameObject m_rudderObject;

	public ShipControlls m_shipControlls;

	public Transform m_controlGuiPos;

	public GameObject m_ashdamageEffects;

	[Header("Misc")]
	public BoxCollider m_floatCollider;

	public float m_waterLevelOffset;

	public float m_forceDistance = 1f;

	public float m_force = 0.5f;

	public float m_damping = 0.05f;

	public float m_dampingSideway = 0.05f;

	public float m_dampingForward = 0.01f;

	public float m_angularDamping = 0.01f;

	public float m_disableLevel = -0.5f;

	public float m_sailForceOffset;

	public float m_sailForceFactor = 0.1f;

	public float m_rudderSpeed = 0.5f;

	public float m_stearForceOffset = -10f;

	public float m_stearForce = 0.5f;

	public float m_stearVelForceFactor = 0.1f;

	public float m_backwardForce = 50f;

	public float m_rudderRotationMax = 30f;

	public float m_minWaterImpactForce = 2.5f;

	public float m_minWaterImpactInterval = 2f;

	public float m_waterImpactDamage = 10f;

	public float m_upsideDownDmgInterval = 1f;

	public float m_upsideDownDmg = 20f;

	public EffectList m_waterImpactEffect = new EffectList();

	public bool m_ashlandsReady;

	private bool m_sailWasInPosition;

	private Vector3 m_windChangeVelocity = Vector3.zero;

	private Speed m_speed;

	private float m_rudder;

	private float m_rudderValue;

	private Vector3 m_sailForce = Vector3.zero;

	private readonly List<Player> m_players = new List<Player>();

	private List<AudioSource> m_ashlandsFxAudio;

	private WaterVolume m_previousCenter;

	private WaterVolume m_previousLeft;

	private WaterVolume m_previousRight;

	private WaterVolume m_previousForward;

	private WaterVolume m_previousBack;

	private static readonly List<Ship> s_currentShips = new List<Ship>();

	private GlobalWind m_globalWind;

	private Rigidbody m_body;

	private ZNetView m_nview;

	private IDestructible m_destructible;

	private Cloth m_sailCloth;

	private float m_lastDepth = -9999f;

	private float m_lastWaterImpactTime;

	private float m_upsideDownDmgTimer;

	private float m_ashlandsDmgTimer;

	private float m_rudderPaddleTimer;

	private float m_lastUpdateWaterForceTime;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_destructible = ((Component)this).GetComponent<IDestructible>();
		WearNTear component = ((Component)this).GetComponent<WearNTear>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.m_onDestroyed = (Action)Delegate.Combine(component.m_onDestroyed, new Action(OnDestroyed));
		}
		if (m_nview.GetZDO() == null)
		{
			((Behaviour)this).enabled = false;
		}
		m_body.maxDepenetrationVelocity = 2f;
		Heightmap.ForceGenerateAll();
		m_sailCloth = m_sailObject.GetComponentInChildren<Cloth>();
		if (Object.op_Implicit((Object)(object)m_sailCloth))
		{
			m_globalWind = ((Component)m_sailCloth).gameObject.GetComponent<GlobalWind>();
		}
		if (Object.op_Implicit((Object)(object)m_ashdamageEffects))
		{
			m_ashdamageEffects.SetActive(false);
			m_ashlandsFxAudio = m_ashdamageEffects.GetComponentsInChildren<AudioSource>().ToList();
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

	public bool CanBeRemoved()
	{
		return m_players.Count == 0;
	}

	private void Start()
	{
		m_nview.Register("Stop", RPC_Stop);
		m_nview.Register("Forward", RPC_Forward);
		m_nview.Register("Backward", RPC_Backward);
		m_nview.Register<float>("Rudder", RPC_Rudder);
		((MonoBehaviour)this).InvokeRepeating("UpdateOwner", 2f, 2f);
	}

	private void PrintStats()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (m_players.Count != 0)
		{
			Vector3 linearVelocity = m_body.linearVelocity;
			ZLog.Log((object)("Vel:" + ((Vector3)(ref linearVelocity)).magnitude.ToString("0.0")));
		}
	}

	public void ApplyControlls(Vector3 dir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (double)dir.z > 0.5;
		bool flag2 = (double)dir.z < -0.5;
		if (flag && !m_forwardPressed)
		{
			Forward();
		}
		if (flag2 && !m_backwardPressed)
		{
			Backward();
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		float num = Mathf.Lerp(0.5f, 1f, Mathf.Abs(m_rudderValue));
		m_rudder = dir.x * num;
		m_rudderValue += m_rudder * m_rudderSpeed * fixedDeltaTime;
		m_rudderValue = Utils.Clamp(m_rudderValue, -1f, 1f);
		if (Time.time - m_sendRudderTime > 0.2f)
		{
			m_sendRudderTime = Time.time;
			m_nview.InvokeRPC("Rudder", m_rudderValue);
		}
		m_forwardPressed = flag;
		m_backwardPressed = flag2;
	}

	public void Forward()
	{
		m_nview.InvokeRPC("Forward");
	}

	public void Backward()
	{
		m_nview.InvokeRPC("Backward");
	}

	public void Rudder(float rudder)
	{
		((MonoBehaviour)m_nview).Invoke("Rudder", rudder);
	}

	private void RPC_Rudder(long sender, float value)
	{
		m_rudderValue = value;
	}

	public void Stop()
	{
		m_nview.InvokeRPC("Stop");
	}

	private void RPC_Stop(long sender)
	{
		m_speed = Speed.Stop;
	}

	private void RPC_Forward(long sender)
	{
		switch (m_speed)
		{
		case Speed.Stop:
			m_speed = Speed.Slow;
			break;
		case Speed.Slow:
			m_speed = Speed.Half;
			break;
		case Speed.Half:
			m_speed = Speed.Full;
			break;
		case Speed.Back:
			m_speed = Speed.Stop;
			break;
		case Speed.Full:
			break;
		}
	}

	private void RPC_Backward(long sender)
	{
		switch (m_speed)
		{
		case Speed.Stop:
			m_speed = Speed.Back;
			break;
		case Speed.Slow:
			m_speed = Speed.Stop;
			break;
		case Speed.Half:
			m_speed = Speed.Slow;
			break;
		case Speed.Full:
			m_speed = Speed.Half;
			break;
		case Speed.Back:
			break;
		}
	}

	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_062b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0665: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0680: Unknown result type (might be due to invalid IL or missing references)
		//IL_069a: Unknown result type (might be due to invalid IL or missing references)
		//IL_069f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0727: Unknown result type (might be due to invalid IL or missing references)
		//IL_0736: Unknown result type (might be due to invalid IL or missing references)
		//IL_073b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = HaveControllingPlayer();
		UpdateControlls(fixedDeltaTime);
		UpdateSail(fixedDeltaTime);
		UpdateRudder(fixedDeltaTime, flag);
		if (Object.op_Implicit((Object)(object)m_nview) && !m_nview.IsOwner())
		{
			return;
		}
		UpdateUpsideDmg(fixedDeltaTime);
		TakeAshlandsDamage(fixedDeltaTime);
		if (m_players.Count == 0)
		{
			m_speed = Speed.Stop;
			m_rudderValue = 0f;
		}
		if (!flag && (m_speed == Speed.Slow || m_speed == Speed.Back))
		{
			m_speed = Speed.Stop;
		}
		Vector3 worldCenterOfMass = m_body.worldCenterOfMass;
		Transform transform = ((Component)m_floatCollider).transform;
		Vector3 size = m_floatCollider.size;
		Vector3 position = transform.position;
		Vector3 forward = transform.forward;
		Vector3 right = transform.right;
		Vector3 val = position + forward * size.z / 2f;
		Vector3 val2 = position - forward * size.z / 2f;
		Vector3 val3 = position - right * size.x / 2f;
		Vector3 val4 = position + right * size.x / 2f;
		Transform transform2 = ((Component)this).transform;
		Vector3 forward2 = transform2.forward;
		Vector3 right2 = transform2.right;
		float waterLevel = Floating.GetWaterLevel(worldCenterOfMass, ref m_previousCenter);
		float waterLevel2 = Floating.GetWaterLevel(val3, ref m_previousLeft);
		float waterLevel3 = Floating.GetWaterLevel(val4, ref m_previousRight);
		float waterLevel4 = Floating.GetWaterLevel(val, ref m_previousForward);
		float waterLevel5 = Floating.GetWaterLevel(val2, ref m_previousBack);
		float num = (waterLevel + waterLevel2 + waterLevel3 + waterLevel4 + waterLevel5) / 5f;
		float num2 = worldCenterOfMass.y - num - m_waterLevelOffset;
		if (!(num2 > m_disableLevel))
		{
			m_body.WakeUp();
			UpdateWaterForce(num2, Time.time);
			Vector3 val5 = new Vector3(val3.x, waterLevel2, val3.z);
			Vector3 val6 = default(Vector3);
			((Vector3)(ref val6))._002Ector(val4.x, waterLevel3, val4.z);
			Vector3 val7 = new Vector3(val.x, waterLevel4, val.z);
			Vector3 val8 = default(Vector3);
			((Vector3)(ref val8))._002Ector(val2.x, waterLevel5, val2.z);
			float num3 = fixedDeltaTime * 50f;
			Vector3 val9 = m_body.linearVelocity;
			_ = ((Vector3)(ref val9)).magnitude;
			float num4 = Utils.Clamp01(Utils.Abs(num2) / m_forceDistance);
			Vector3 val10 = m_force * num4 * Vector3.up;
			m_body.AddForceAtPosition(val10 * (m_body.mass * num3), worldCenterOfMass, (ForceMode)1);
			float num5 = Vector3.Dot(val9, forward2);
			float num6 = Vector3.Dot(val9, right2);
			float num7 = val9.y * val9.y * Utils.Sign(val9.y) * m_damping * num4;
			float num8 = num5 * num5 * Utils.Sign(num5) * m_dampingForward * num4;
			float num9 = num6 * num6 * Utils.Sign(num6) * m_dampingSideway * num4;
			val9.y -= Utils.Clamp(num7, -1f, 1f);
			val9 -= ((Component)this).transform.forward * Utils.Clamp(num8, -1f, 1f);
			val9 -= ((Component)this).transform.right * Utils.Clamp(num9, -1f, 1f);
			float magnitude = ((Vector3)(ref val9)).magnitude;
			Vector3 linearVelocity = m_body.linearVelocity;
			if (magnitude > ((Vector3)(ref linearVelocity)).magnitude)
			{
				Vector3 normalized = ((Vector3)(ref val9)).normalized;
				linearVelocity = m_body.linearVelocity;
				val9 = normalized * ((Vector3)(ref linearVelocity)).magnitude;
			}
			if (m_players.Count == 0)
			{
				val9.x *= 0.1f;
				val9.z *= 0.1f;
			}
			m_body.linearVelocity = val9;
			Rigidbody body = m_body;
			body.angularVelocity -= m_body.angularVelocity * (m_angularDamping * num4);
			float num10 = 0.15f;
			float num11 = 0.5f;
			float num12 = Utils.Clamp((val7.y - val.y) * num10, 0f - num11, num11);
			float num13 = Utils.Clamp((val8.y - val2.y) * num10, 0f - num11, num11);
			float num14 = Utils.Clamp((val5.y - val3.y) * num10, 0f - num11, num11);
			float num15 = Utils.Clamp((val6.y - val4.y) * num10, 0f - num11, num11);
			num12 = Utils.Sign(num12) * Utils.Abs(num12 * num12);
			num13 = Utils.Sign(num13) * Utils.Abs(num13 * num13);
			num14 = Utils.Sign(num14) * Utils.Abs(num14 * num14);
			num15 = Utils.Sign(num15) * Utils.Abs(num15 * num15);
			m_body.AddForceAtPosition(m_body.mass * num12 * num3 * Vector3.up, val, (ForceMode)1);
			m_body.AddForceAtPosition(m_body.mass * num13 * num3 * Vector3.up, val2, (ForceMode)1);
			m_body.AddForceAtPosition(m_body.mass * num14 * num3 * Vector3.up, val3, (ForceMode)1);
			m_body.AddForceAtPosition(m_body.mass * num15 * num3 * Vector3.up, val4, (ForceMode)1);
			float sailSize = 0f;
			if (m_speed == Speed.Full)
			{
				sailSize = 1f;
			}
			else if (m_speed == Speed.Half)
			{
				sailSize = 0.5f;
			}
			Vector3 sailForce = GetSailForce(sailSize, fixedDeltaTime);
			Vector3 val11 = worldCenterOfMass + ((Component)this).transform.up * m_sailForceOffset;
			m_body.AddForceAtPosition(sailForce * m_body.mass, val11, (ForceMode)1);
			Vector3 val12 = ((Component)this).transform.position + forward2 * m_stearForceOffset;
			float num16 = num5 * m_stearVelForceFactor;
			m_body.AddForceAtPosition(m_body.mass * num16 * (0f - m_rudderValue) * fixedDeltaTime * right2, val12, (ForceMode)1);
			Vector3 val13 = Vector3.zero;
			switch (m_speed)
			{
			case Speed.Slow:
				val13 += forward2 * (m_backwardForce * (1f - Utils.Abs(m_rudderValue)));
				break;
			case Speed.Back:
				val13 += -forward2 * (m_backwardForce * (1f - Utils.Abs(m_rudderValue)));
				break;
			}
			if (m_speed == Speed.Back || m_speed == Speed.Slow)
			{
				float num17 = ((m_speed != Speed.Back) ? 1 : (-1));
				val13 += ((Component)this).transform.right * (m_stearForce * (0f - m_rudderValue) * num17);
			}
			m_body.AddForceAtPosition(val13 * (m_body.mass * fixedDeltaTime), val12, (ForceMode)1);
			ApplyEdgeForce(fixedDeltaTime);
		}
	}

	private void UpdateUpsideDmg(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (!(((Component)this).transform.up.y >= 0f))
		{
			m_upsideDownDmgTimer += dt;
			if (!(m_upsideDownDmgTimer <= m_upsideDownDmgInterval))
			{
				m_upsideDownDmgTimer = 0f;
				HitData hitData = new HitData();
				hitData.m_damage.m_blunt = m_upsideDownDmg;
				hitData.m_point = ((Component)this).transform.position;
				hitData.m_dir = Vector3.up;
				m_destructible.Damage(hitData);
			}
		}
	}

	private void TakeAshlandsDamage(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (m_ashlandsReady)
		{
			return;
		}
		float ashlandsOceanGradient = WorldGenerator.GetAshlandsOceanGradient(((Component)this).transform.position);
		if (Object.op_Implicit((Object)(object)m_ashdamageEffects))
		{
			if (ashlandsOceanGradient < 0f)
			{
				m_ashdamageEffects.SetActive(false);
				{
					foreach (AudioSource item in m_ashlandsFxAudio)
					{
						item.Stop();
					}
					return;
				}
			}
			m_ashdamageEffects.SetActive(true);
		}
		if (m_ashDamageMsgTimer <= 0f && Object.op_Implicit((Object)(object)ZoneSystem.instance) && Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			ZoneSystem.instance.SetGlobalKey(GlobalKeys.AshlandsOcean);
			m_ashDamageMsgTimer = m_ashDamageMsgTime;
		}
		else
		{
			m_ashDamageMsgTimer -= Time.fixedDeltaTime;
		}
		m_ashlandsDmgTimer += dt;
		if (!((double)m_ashlandsDmgTimer <= 1.0))
		{
			m_ashlandsDmgTimer = 0f;
			ashlandsOceanGradient = Utils.Clamp(ashlandsOceanGradient, 0f, 3f);
			HitData hitData = new HitData();
			hitData.m_damage.m_blunt = Mathf.Floor(Mathf.Lerp(1f, 30f, ashlandsOceanGradient));
			hitData.m_hitType = HitData.HitType.AshlandsOcean;
			hitData.m_point = ((Component)this).transform.position;
			hitData.m_dir = Vector3.up;
			m_destructible.Damage(hitData);
		}
	}

	private Vector3 GetSailForce(float sailSize, float dt)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vector3 windDir = EnvMan.instance.GetWindDir();
		float windIntensity = EnvMan.instance.GetWindIntensity();
		float num = Mathf.Lerp(0.25f, 1f, windIntensity);
		float windAngleFactor = GetWindAngleFactor();
		windAngleFactor *= num;
		Vector3 val = Vector3.Normalize(windDir + ((Component)this).transform.forward) * (windAngleFactor * m_sailForceFactor * sailSize);
		m_sailForce = Vector3.SmoothDamp(m_sailForce, val, ref m_windChangeVelocity, 1f, 99f);
		return m_sailForce;
	}

	public float GetWindAngleFactor()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(EnvMan.instance.GetWindDir(), -((Component)this).transform.forward);
		float num2 = Mathf.Lerp(0.7f, 1f, 1f - Utils.Abs(num));
		float num3 = 1f - Utils.LerpStep(0.75f, 0.8f, num);
		return num2 * num3;
	}

	private void UpdateWaterForce(float depth, float time)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		float num = depth - m_lastDepth;
		float num2 = time - m_lastUpdateWaterForceTime;
		m_lastDepth = depth;
		m_lastUpdateWaterForceTime = time;
		float num3 = num / num2;
		if (!(num3 > 0f) && Utils.Abs(num3) > m_minWaterImpactForce && time - m_lastWaterImpactTime > m_minWaterImpactInterval)
		{
			m_lastWaterImpactTime = time;
			m_waterImpactEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			if (m_players.Count > 0)
			{
				HitData hitData = new HitData();
				hitData.m_damage.m_blunt = m_waterImpactDamage;
				hitData.m_point = ((Component)this).transform.position;
				hitData.m_dir = Vector3.up;
				m_destructible.Damage(hitData);
			}
		}
	}

	private void ApplyEdgeForce(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		float magnitude = ((Vector3)(ref position)).magnitude;
		float num = 10420f;
		if (magnitude > num)
		{
			Vector3 val = Vector3.Normalize(((Component)this).transform.position);
			float num2 = Utils.LerpStep(num, 10500f, magnitude) * 8f;
			Vector3 val2 = val * num2;
			m_body.AddForce(val2 * dt, (ForceMode)2);
		}
	}

	private void FixTilt()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Asin(((Component)this).transform.right.y);
		float num2 = Mathf.Asin(((Component)this).transform.forward.y);
		if (Utils.Abs(num) > (float)Math.PI / 6f)
		{
			if (num > 0f)
			{
				((Component)this).transform.RotateAround(((Component)this).transform.position, ((Component)this).transform.forward, (0f - Time.fixedDeltaTime) * 20f);
			}
			else
			{
				((Component)this).transform.RotateAround(((Component)this).transform.position, ((Component)this).transform.forward, Time.fixedDeltaTime * 20f);
			}
		}
		if (Utils.Abs(num2) > (float)Math.PI / 6f)
		{
			if (num2 > 0f)
			{
				((Component)this).transform.RotateAround(((Component)this).transform.position, ((Component)this).transform.right, (0f - Time.fixedDeltaTime) * 20f);
			}
			else
			{
				((Component)this).transform.RotateAround(((Component)this).transform.position, ((Component)this).transform.right, Time.fixedDeltaTime * 20f);
			}
		}
	}

	private void UpdateControlls(float dt)
	{
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_forward, (int)m_speed);
			m_nview.GetZDO().Set(ZDOVars.s_rudder, m_rudderValue);
			return;
		}
		m_speed = (Speed)m_nview.GetZDO().GetInt(ZDOVars.s_forward);
		if (Time.time - m_sendRudderTime > 1f)
		{
			m_rudderValue = m_nview.GetZDO().GetFloat(ZDOVars.s_rudder);
		}
	}

	public bool IsSailUp()
	{
		if (m_speed != Speed.Half)
		{
			return m_speed == Speed.Full;
		}
		return true;
	}

	private void UpdateSail(float dt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		UpdateSailSize(dt);
		Vector3 windDir = EnvMan.instance.GetWindDir();
		windDir = Vector3.Cross(Vector3.Cross(windDir, ((Component)this).transform.up), ((Component)this).transform.up);
		if (m_speed == Speed.Full || m_speed == Speed.Half)
		{
			float num = 0.5f + Vector3.Dot(((Component)this).transform.forward, windDir) * 0.5f;
			Quaternion val = Quaternion.LookRotation(-Vector3.Lerp(windDir, Vector3.Normalize(windDir - ((Component)this).transform.forward), num), ((Component)this).transform.up);
			m_mastObject.transform.rotation = Quaternion.RotateTowards(m_mastObject.transform.rotation, val, 30f * dt);
		}
		else if (m_speed == Speed.Back)
		{
			Quaternion val2 = Quaternion.LookRotation(-((Component)this).transform.forward, ((Component)this).transform.up);
			Quaternion val3 = Quaternion.LookRotation(-windDir, ((Component)this).transform.up);
			val3 = Quaternion.RotateTowards(val2, val3, 80f);
			m_mastObject.transform.rotation = Quaternion.RotateTowards(m_mastObject.transform.rotation, val3, 30f * dt);
		}
	}

	private void UpdateRudder(float dt, bool haveControllingPlayer)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_rudderObject))
		{
			return;
		}
		Quaternion val = Quaternion.Euler(0f, m_rudderRotationMax * (0f - m_rudderValue), 0f);
		if (haveControllingPlayer)
		{
			if (m_speed == Speed.Slow)
			{
				m_rudderPaddleTimer += dt;
				val *= Quaternion.Euler(0f, Mathf.Sin(m_rudderPaddleTimer * 6f) * 20f, 0f);
			}
			else if (m_speed == Speed.Back)
			{
				m_rudderPaddleTimer += dt;
				val *= Quaternion.Euler(0f, Mathf.Sin(m_rudderPaddleTimer * -3f) * 40f, 0f);
			}
		}
		m_rudderObject.transform.localRotation = Quaternion.Slerp(m_rudderObject.transform.localRotation, val, 0.5f);
	}

	private void UpdateSailSize(float dt)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		switch (m_speed)
		{
		case Speed.Back:
			num = 0.1f;
			break;
		case Speed.Half:
			num = 0.5f;
			break;
		case Speed.Full:
			num = 1f;
			break;
		case Speed.Slow:
			num = 0.1f;
			break;
		case Speed.Stop:
			num = 0.1f;
			break;
		}
		Vector3 localScale = m_sailObject.transform.localScale;
		bool flag = Utils.Abs(localScale.y - num) < 0.01f;
		if (!flag)
		{
			localScale.y = Mathf.MoveTowards(localScale.y, num, dt);
			m_sailObject.transform.localScale = localScale;
		}
		if (Object.op_Implicit((Object)(object)m_sailCloth))
		{
			if (m_speed == Speed.Stop || m_speed == Speed.Slow || m_speed == Speed.Back)
			{
				if (flag && m_sailCloth.enabled)
				{
					m_sailCloth.enabled = false;
				}
			}
			else if (flag)
			{
				if (!m_sailWasInPosition)
				{
					Utils.RecreateComponent(ref m_sailCloth);
					if (Object.op_Implicit((Object)(object)m_globalWind))
					{
						m_globalWind.UpdateClothReference(m_sailCloth);
					}
				}
			}
			else
			{
				m_sailCloth.enabled = true;
			}
		}
		m_sailWasInPosition = flag;
	}

	private void UpdateOwner()
	{
		if (m_nview.IsValid() && m_nview.IsOwner() && !((Object)(object)Player.m_localPlayer == (Object)null) && m_players.Count > 0 && !IsPlayerInBoat(Player.m_localPlayer))
		{
			RefreshPlayerList();
			long newOwnerID = GetNewOwnerID();
			m_nview.GetZDO().SetOwner(newOwnerID);
			ZLog.Log((object)("Changing ship owner to " + newOwnerID));
		}
	}

	private long GetNewOwnerID()
	{
		long num = 0L;
		for (int i = 0; i < m_players.Count; i++)
		{
			num = m_players[i].GetOwner();
			if (num != 0L)
			{
				break;
			}
		}
		if (num == 0L)
		{
			num = ZDOMan.GetSessionID();
		}
		return num;
	}

	private void RefreshPlayerList()
	{
		for (int i = 0; i < m_players.Count; i++)
		{
			if (m_players[i].GetOwner() == 0L)
			{
				m_players.RemoveAt(i);
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		Player component = ((Component)collider).GetComponent<Player>();
		if (Object.op_Implicit((Object)(object)component))
		{
			m_players.Add(component);
			ZLog.Log((object)("Player onboard, total onboard " + m_players.Count));
			if ((Object)(object)component == (Object)(object)Player.m_localPlayer)
			{
				s_currentShips.Add(this);
			}
		}
		Character component2 = ((Component)collider).GetComponent<Character>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.InNumShipVolumes++;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		Player component = ((Component)collider).GetComponent<Player>();
		if (Object.op_Implicit((Object)(object)component))
		{
			m_players.Remove(component);
			ZLog.Log((object)("Player over board, players left " + m_players.Count));
			if ((Object)(object)component == (Object)(object)Player.m_localPlayer)
			{
				s_currentShips.Remove(this);
			}
		}
		Character component2 = ((Component)collider).GetComponent<Character>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.InNumShipVolumes--;
		}
	}

	public bool IsPlayerInBoat(ZDOID zdoid)
	{
		foreach (Player player in m_players)
		{
			if (player.GetZDOID() == zdoid)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPlayerInBoat(Player player)
	{
		return m_players.Contains(player);
	}

	public bool IsPlayerInBoat(long playerID)
	{
		foreach (Player player in m_players)
		{
			if (player.GetPlayerID() == playerID)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPlayerOnboard()
	{
		return m_players.Count > 0;
	}

	private void OnDestroyed()
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			Gogan.LogEvent("Game", "ShipDestroyed", ((Object)((Component)this).gameObject).name, 0L);
		}
		s_currentShips.Remove(this);
	}

	public bool IsWindControllActive()
	{
		foreach (Player player in m_players)
		{
			if (player.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.SailingPower))
			{
				return true;
			}
		}
		return false;
	}

	public static Ship GetLocalShip()
	{
		if (s_currentShips.Count != 0)
		{
			return s_currentShips[s_currentShips.Count - 1];
		}
		return null;
	}

	private bool HaveControllingPlayer()
	{
		if (m_players.Count != 0)
		{
			return m_shipControlls.HaveValidUser();
		}
		return false;
	}

	public bool IsOwner()
	{
		if (m_nview.IsValid())
		{
			return m_nview.IsOwner();
		}
		return false;
	}

	public float GetSpeed()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Dot(m_body.linearVelocity, ((Component)this).transform.forward);
	}

	public Speed GetSpeedSetting()
	{
		return m_speed;
	}

	public float GetRudder()
	{
		return m_rudder;
	}

	public float GetRudderValue()
	{
		return m_rudderValue;
	}

	public float GetShipYawAngle()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if ((Object)(object)mainCamera == (Object)null)
		{
			return 0f;
		}
		return 0f - Utils.YawFromDirection(((Component)mainCamera).transform.InverseTransformDirection(((Component)this).transform.forward));
	}

	public float GetWindAngle()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Vector3 windDir = EnvMan.instance.GetWindDir();
		return 0f - Utils.YawFromDirection(((Component)this).transform.InverseTransformDirection(windDir));
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(((Component)this).transform.position + ((Component)this).transform.forward * m_stearForceOffset, 0.25f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(((Component)this).transform.position + ((Component)this).transform.up * m_sailForceOffset, 0.25f);
	}
}
