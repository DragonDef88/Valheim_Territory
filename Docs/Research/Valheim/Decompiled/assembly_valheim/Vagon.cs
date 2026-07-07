using System;
using System.Collections.Generic;
using UnityEngine;

public class Vagon : MonoBehaviour, Hoverable, Interactable
{
	[Serializable]
	public class LoadData
	{
		public GameObject m_gameobject;

		public float m_minPercentage;
	}

	private static List<Vagon> m_instances = new List<Vagon>();

	public Transform m_attachPoint;

	public string m_name = "Wagon";

	public float m_detachDistance = 2f;

	public Vector3 m_attachOffset = new Vector3(0f, 0.8f, 0f);

	public Container m_container;

	public Transform m_lineAttachPoints0;

	public Transform m_lineAttachPoints1;

	public Vector3 m_lineAttachOffset = new Vector3(0f, 1f, 0f);

	public float m_breakForce = 10000f;

	public float m_spring = 5000f;

	public float m_springDamping = 1000f;

	public float m_baseMass = 20f;

	public float m_itemWeightMassFactor = 1f;

	public float m_playerExtraPullMass;

	public ZSFX[] m_wheelLoops;

	public float m_minPitch = 1f;

	public float m_maxPitch = 1.5f;

	public float m_maxPitchVel = 10f;

	public float m_maxVol = 1f;

	public float m_maxVolVel = 10f;

	public float m_audioChangeSpeed = 2f;

	public Rigidbody[] m_wheels = (Rigidbody[])(object)new Rigidbody[0];

	public List<LoadData> m_loadVis = new List<LoadData>();

	private ZNetView m_nview;

	private ConfigurableJoint m_attachJoin;

	private GameObject m_attachedObject;

	private Rigidbody m_body;

	private LineRenderer m_lineRenderer;

	private Rigidbody[] m_bodies;

	private Humanoid m_useRequester;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.GetZDO() == null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		m_instances.Add(this);
		Heightmap.ForceGenerateAll();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_bodies = ((Component)this).GetComponentsInChildren<Rigidbody>();
		m_lineRenderer = ((Component)this).GetComponent<LineRenderer>();
		Rigidbody[] bodies = m_bodies;
		for (int i = 0; i < bodies.Length; i++)
		{
			bodies[i].maxDepenetrationVelocity = 2f;
		}
		m_nview.Register("RPC_RequestOwn", RPC_RequestOwn);
		m_nview.Register("RPC_RequestDenied", RPC_RequestDenied);
		((MonoBehaviour)this).InvokeRepeating("UpdateMass", 0f, 5f);
		((MonoBehaviour)this).InvokeRepeating("UpdateLoadVisualization", 0f, 3f);
	}

	private void OnDestroy()
	{
		m_instances.Remove(this);
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public string GetHoverText()
	{
		return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		m_useRequester = character;
		if (!m_nview.IsOwner())
		{
			m_nview.InvokeRPC("RPC_RequestOwn");
		}
		return false;
	}

	public void RPC_RequestOwn(long sender)
	{
		if (m_nview.IsOwner())
		{
			if (InUse())
			{
				ZLog.Log((object)"Requested use, but is already in use");
				m_nview.InvokeRPC(sender, "RPC_RequestDenied");
			}
			else
			{
				m_nview.GetZDO().SetOwner(sender);
			}
		}
	}

	private void RPC_RequestDenied(long sender)
	{
		ZLog.Log((object)"Got request denied");
		if (Object.op_Implicit((Object)(object)m_useRequester))
		{
			m_useRequester.Message(MessageHud.MessageType.Center, m_name + " is in use by someone else");
			m_useRequester = null;
		}
	}

	private void FixedUpdate()
	{
		if (!m_nview.IsValid())
		{
			return;
		}
		UpdateAudio(Time.fixedDeltaTime);
		if (m_nview.IsOwner())
		{
			if (Object.op_Implicit((Object)(object)m_useRequester))
			{
				if (IsAttached())
				{
					Detach();
				}
				else if (CanAttach(((Component)m_useRequester).gameObject))
				{
					AttachTo(((Component)m_useRequester).gameObject);
				}
				else
				{
					m_useRequester.Message(MessageHud.MessageType.Center, "$msg_cart_incorrectposition");
				}
				m_useRequester = null;
			}
			if (IsAttached() && (!Object.op_Implicit((Object)(object)m_attachJoin) || !CanAttach(((Component)((Joint)m_attachJoin).connectedBody).gameObject)))
			{
				Detach();
			}
		}
		else if (IsAttached())
		{
			Detach();
		}
	}

	private void LateUpdate()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (IsAttached() && (Object)(object)m_attachJoin != (Object)null)
		{
			((Renderer)m_lineRenderer).enabled = true;
			m_lineRenderer.SetPosition(0, m_lineAttachPoints0.position);
			m_lineRenderer.SetPosition(1, ((Component)((Joint)m_attachJoin).connectedBody).transform.position + m_lineAttachOffset);
			m_lineRenderer.SetPosition(2, m_lineAttachPoints1.position);
		}
		else
		{
			((Renderer)m_lineRenderer).enabled = false;
		}
	}

	public bool IsAttached(Character character)
	{
		if (Object.op_Implicit((Object)(object)m_attachJoin) && (Object)(object)((Component)((Joint)m_attachJoin).connectedBody).gameObject == (Object)(object)((Component)character).gameObject)
		{
			return true;
		}
		return false;
	}

	public bool InUse()
	{
		if (Object.op_Implicit((Object)(object)m_container) && m_container.IsInUse())
		{
			return true;
		}
		return IsAttached();
	}

	public bool IsAttached()
	{
		if (!((Object)(object)m_attachJoin != (Object)null))
		{
			if (m_nview.IsValid())
			{
				return m_nview.GetZDO().GetBool(ZDOVars.s_attachJointHash);
			}
			return false;
		}
		return true;
	}

	private bool CanAttach(GameObject go)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.up.y < 0.1f)
		{
			return false;
		}
		Humanoid component = go.GetComponent<Humanoid>();
		if (Object.op_Implicit((Object)(object)component) && (component.InDodge() || component.IsTeleporting()))
		{
			return false;
		}
		return Vector3.Distance(go.transform.position + m_attachOffset, m_attachPoint.position) < m_detachDistance;
	}

	private void AttachTo(GameObject go)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		DetachAll();
		m_attachJoin = ((Component)this).gameObject.AddComponent<ConfigurableJoint>();
		((Joint)m_attachJoin).autoConfigureConnectedAnchor = false;
		((Joint)m_attachJoin).anchor = m_attachPoint.localPosition;
		((Joint)m_attachJoin).connectedAnchor = m_attachOffset;
		((Joint)m_attachJoin).breakForce = m_breakForce;
		m_attachJoin.xMotion = (ConfigurableJointMotion)1;
		m_attachJoin.yMotion = (ConfigurableJointMotion)1;
		m_attachJoin.zMotion = (ConfigurableJointMotion)1;
		SoftJointLimit linearLimit = default(SoftJointLimit);
		((SoftJointLimit)(ref linearLimit)).limit = 0.001f;
		m_attachJoin.linearLimit = linearLimit;
		SoftJointLimitSpring linearLimitSpring = default(SoftJointLimitSpring);
		((SoftJointLimitSpring)(ref linearLimitSpring)).spring = m_spring;
		((SoftJointLimitSpring)(ref linearLimitSpring)).damper = m_springDamping;
		m_attachJoin.linearLimitSpring = linearLimitSpring;
		m_attachJoin.zMotion = (ConfigurableJointMotion)0;
		((Joint)m_attachJoin).connectedBody = go.GetComponent<Rigidbody>();
		m_attachedObject = go;
		if (m_nview.IsValid())
		{
			m_nview.GetZDO().Set(ZDOVars.s_attachJointHash, value: true);
		}
		if (m_playerExtraPullMass != 0f)
		{
			m_attachedObject.GetComponent<Character>()?.SetExtraMass(m_playerExtraPullMass);
		}
	}

	private static void DetachAll()
	{
		foreach (Vagon instance in m_instances)
		{
			instance.Detach();
		}
	}

	private void Detach()
	{
		if (Object.op_Implicit((Object)(object)m_attachJoin))
		{
			Object.Destroy((Object)(object)m_attachJoin);
		}
		m_attachJoin = null;
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_attachJointHash, value: false);
		}
		m_body.WakeUp();
		m_body.AddForce(0f, 1f, 0f);
		if (m_playerExtraPullMass != 0f && Object.op_Implicit((Object)(object)m_attachedObject))
		{
			m_attachedObject.GetComponent<Character>()?.SetExtraMass(0f);
		}
		m_attachedObject = null;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void UpdateMass()
	{
		if (m_nview.IsOwner() && !((Object)(object)m_container == (Object)null))
		{
			float totalWeight = m_container.GetInventory().GetTotalWeight();
			float mass = m_baseMass + totalWeight * m_itemWeightMassFactor;
			SetMass(mass);
		}
	}

	private void SetMass(float mass)
	{
		float mass2 = mass / (float)m_bodies.Length;
		Rigidbody[] bodies = m_bodies;
		for (int i = 0; i < bodies.Length; i++)
		{
			bodies[i].mass = mass2;
		}
	}

	private void UpdateLoadVisualization()
	{
		if ((Object)(object)m_container == (Object)null)
		{
			return;
		}
		float num = m_container.GetInventory().SlotsUsedPercentage();
		foreach (LoadData loadVi in m_loadVis)
		{
			loadVi.m_gameobject.SetActive(num >= loadVi.m_minPercentage);
		}
	}

	private void UpdateAudio(float dt)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Rigidbody[] wheels = m_wheels;
		foreach (Rigidbody val in wheels)
		{
			float num2 = num;
			Vector3 angularVelocity = val.angularVelocity;
			num = num2 + ((Vector3)(ref angularVelocity)).magnitude;
		}
		num /= (float)m_wheels.Length;
		float num3 = Mathf.Lerp(m_minPitch, m_maxPitch, Mathf.Clamp01(num / m_maxPitchVel));
		float num4 = m_maxVol * Mathf.Clamp01(num / m_maxVolVel);
		ZSFX[] wheelLoops = m_wheelLoops;
		foreach (ZSFX obj in wheelLoops)
		{
			obj.SetVolumeModifier(Mathf.MoveTowards(obj.GetVolumeModifier(), num4, m_audioChangeSpeed * dt));
			obj.SetPitchModifier(Mathf.MoveTowards(obj.GetPitchModifier(), num3, m_audioChangeSpeed * dt));
		}
	}
}
