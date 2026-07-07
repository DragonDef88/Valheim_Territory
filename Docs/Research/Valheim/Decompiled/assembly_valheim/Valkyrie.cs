using System;
using UnityEngine;

public class Valkyrie : MonoBehaviour
{
	public static Valkyrie m_instance;

	public float m_speed = 10f;

	public float m_turnRate = 5f;

	public float m_dropHeight = 10f;

	public float m_startAltitude = 500f;

	public float m_descentAltitude = 100f;

	public float m_startDistance = 500f;

	public float m_startDescentDistance = 200f;

	public Vector3 m_attachOffset = new Vector3(0f, 0f, 1f);

	public float m_textDuration = 5f;

	public Transform m_attachPoint;

	private Vector3 m_targetPoint;

	private Vector3 m_descentStart;

	private Vector3 m_flyAwayPoint;

	private bool m_descent;

	private bool m_droppedPlayer;

	private Animator m_animator;

	private ZNetView m_nview;

	private float m_timer;

	private void Awake()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		m_instance = this;
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_animator = ((Component)this).GetComponentInChildren<Animator>();
		if (!m_nview.IsOwner())
		{
			((Behaviour)this).enabled = false;
			return;
		}
		ZLog.Log((object)"Setting up valkyrie ");
		float num = Random.value * (float)Math.PI * 2f;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Sin(num), 0f, Mathf.Cos(num));
		Vector3 val2 = Vector3.Cross(val, Vector3.up);
		m_targetPoint = ((Component)Player.m_localPlayer).transform.position + new Vector3(0f, m_dropHeight, 0f);
		Vector3 position = m_targetPoint + val * m_startDistance;
		position.y = m_startAltitude;
		((Component)this).transform.position = position;
		m_descentStart = m_targetPoint + val * m_startDescentDistance + val2 * 200f;
		m_descentStart.y = m_descentAltitude;
		Vector3 val3 = m_targetPoint - m_descentStart;
		val3.y = 0f;
		((Vector3)(ref val3)).Normalize();
		m_flyAwayPoint = m_targetPoint + val3 * m_startDescentDistance;
		m_flyAwayPoint.y = m_startAltitude;
		SyncPlayer(doNetworkSync: true);
		Vector3 val4 = ((Component)this).transform.position;
		string? text = ((object)(Vector3)(ref val4)).ToString();
		val4 = ZNet.instance.GetReferencePosition();
		ZLog.Log((object)("World pos " + text + "   " + ((object)(Vector3)(ref val4)).ToString()));
	}

	private void HideText()
	{
	}

	private void OnDestroy()
	{
		ZLog.Log((object)"Destroying valkyrie");
	}

	private void FixedUpdate()
	{
		UpdateValkyrie(Time.fixedDeltaTime);
		if (!m_droppedPlayer)
		{
			SyncPlayer(doNetworkSync: true);
		}
	}

	private void LateUpdate()
	{
		if (!m_droppedPlayer)
		{
			SyncPlayer(doNetworkSync: false);
		}
	}

	private void UpdateValkyrie(float dt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		m_timer += dt;
		if (TextViewer.IsShowingIntro())
		{
			return;
		}
		Vector3 val = (m_droppedPlayer ? m_flyAwayPoint : ((!m_descent) ? m_descentStart : m_targetPoint));
		if (Utils.DistanceXZ(val, ((Component)this).transform.position) < 0.5f)
		{
			if (!m_descent)
			{
				m_descent = true;
				ZLog.Log((object)"Starting descent");
			}
			else if (!m_droppedPlayer)
			{
				ZLog.Log((object)"We are here");
				DropPlayer();
			}
			else
			{
				m_nview.Destroy();
			}
		}
		Vector3 val2 = val - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		Vector3 val3 = ((Component)this).transform.position + normalized * 25f;
		if (ZoneSystem.instance.GetGroundHeight(val3, out var height))
		{
			val3.y = Mathf.Max(val3.y, height + m_dropHeight);
		}
		val2 = val3 - ((Component)this).transform.position;
		Vector3 normalized2 = ((Vector3)(ref val2)).normalized;
		Quaternion val4 = Quaternion.LookRotation(normalized2);
		Vector3 val5 = normalized2;
		val5.y = 0f;
		((Vector3)(ref val5)).Normalize();
		Vector3 forward = ((Component)this).transform.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		float num = Mathf.Clamp(Vector3.SignedAngle(forward, val5, Vector3.up), -30f, 30f) / 30f;
		val4 = Quaternion.Euler(0f, 0f, num * 45f) * val4;
		float num2 = (m_droppedPlayer ? (m_turnRate * 4f) : m_turnRate);
		((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val4, num2 * dt);
		Vector3 val6 = ((Component)this).transform.forward * m_speed;
		Vector3 val7 = ((Component)this).transform.position + val6 * dt;
		if (ZoneSystem.instance.GetGroundHeight(val7, out var height2))
		{
			val7.y = Mathf.Max(val7.y, height2 + m_dropHeight);
		}
		((Component)this).transform.position = val7;
	}

	public void DropPlayer(bool destroy = false)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)"We are here");
		m_droppedPlayer = true;
		Vector3 forward = ((Component)this).transform.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		((Component)Player.m_localPlayer).transform.rotation = Quaternion.LookRotation(forward);
		Player.m_localPlayer.SetIntro(intro: false);
		m_animator.SetBool("dropped", true);
		if (destroy)
		{
			m_nview.Destroy();
		}
	}

	private void SyncPlayer(bool doNetworkSync)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			ZLog.LogWarning((object)"No local player");
			return;
		}
		((Component)localPlayer).transform.rotation = m_attachPoint.rotation;
		((Component)localPlayer).transform.position = m_attachPoint.position - ((Component)localPlayer).transform.TransformVector(m_attachOffset);
		((Component)localPlayer).GetComponent<Rigidbody>().position = ((Component)localPlayer).transform.position;
		if (doNetworkSync)
		{
			ZNet.instance.SetReferencePosition(((Component)localPlayer).transform.position);
			((Component)localPlayer).GetComponent<ZSyncTransform>().SyncNow();
			((Component)this).GetComponent<ZSyncTransform>().SyncNow();
		}
	}
}
