using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour, IMonoUpdater
{
	public Vector3 m_vel = Vector3.up;

	public float m_randomVel = 0.1f;

	public float m_force = 0.1f;

	public float m_ttl = 10f;

	public float m_fadetime = 3f;

	private Rigidbody m_body;

	private float m_time;

	private float m_fadeTimer = -1f;

	private bool m_added;

	private Particle m_renderParticle;

	private static readonly List<Smoke> s_smoke = new List<Smoke>();

	public Vector3Int RenderChunk { get; set; } = Vector3Int.zero;


	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		s_smoke.Add(this);
		m_added = true;
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_body.maxDepenetrationVelocity = 1f;
		m_vel = Vector3.up + Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward * m_randomVel;
		SetupParticle();
	}

	private void SetupParticle()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(0f, 360f);
		Particle renderParticle = default(Particle);
		((Particle)(ref renderParticle)).angularVelocity = 0f;
		((Particle)(ref renderParticle)).angularVelocity3D = Vector3.zero;
		((Particle)(ref renderParticle)).axisOfRotation = new Vector3(0f, 0f, 1f);
		((Particle)(ref renderParticle)).position = ((Component)this).transform.position;
		((Particle)(ref renderParticle)).randomSeed = (uint)Random.Range(int.MinValue, int.MaxValue);
		((Particle)(ref renderParticle)).remainingLifetime = m_ttl + m_fadetime;
		((Particle)(ref renderParticle)).startLifetime = m_ttl;
		((Particle)(ref renderParticle)).rotation = num;
		((Particle)(ref renderParticle)).rotation3D = new Vector3(0f, 0f, num);
		((Particle)(ref renderParticle)).velocity = Vector3.zero;
		m_renderParticle = renderParticle;
	}

	public Particle GetParticleValues()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (m_fadeTimer < 0f)
		{
			((Particle)(ref m_renderParticle)).remainingLifetime = m_ttl - m_time;
		}
		else
		{
			((Particle)(ref m_renderParticle)).remainingLifetime = m_fadetime - m_fadeTimer;
		}
		((Particle)(ref m_renderParticle)).position = ((Component)this).transform.position;
		return m_renderParticle;
	}

	public float GetAlpha()
	{
		float num = Utils.SmoothStep(0f, 1f, Mathf.Clamp01(m_time / 2f));
		float num2 = Utils.SmoothStep(0f, 1f, 1f - Mathf.Clamp01(m_fadeTimer / m_fadetime));
		return Mathf.Min(num, num2);
	}

	private void OnEnable()
	{
		Instances.Add(this);
		SmokeRenderer.Instance.RegisterSmoke(this);
	}

	private void OnDisable()
	{
		SmokeRenderer.Instance.UnregisterSmoke(this);
		Instances.Remove(this);
	}

	private void OnDestroy()
	{
		if (m_added)
		{
			s_smoke.Remove(this);
			m_added = false;
		}
	}

	public void StartFadeOut()
	{
		if (!(m_fadeTimer >= 0f))
		{
			if (m_added)
			{
				s_smoke.Remove(this);
				m_added = false;
			}
			((Particle)(ref m_renderParticle)).startLifetime = m_time + m_fadetime;
			m_fadeTimer = 0f;
		}
	}

	public static int GetTotalSmoke()
	{
		return s_smoke.Count;
	}

	public static void FadeOldest()
	{
		if (s_smoke.Count != 0)
		{
			s_smoke[0].StartFadeOut();
		}
	}

	public static void FadeMostDistant()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (s_smoke.Count == 0)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if ((Object)(object)mainCamera == (Object)null)
		{
			return;
		}
		Vector3 position = ((Component)mainCamera).transform.position;
		int num = -1;
		float num2 = 0f;
		for (int i = 0; i < s_smoke.Count; i++)
		{
			float num3 = Vector3.Distance(((Component)s_smoke[i]).transform.position, position);
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		if (num != -1)
		{
			s_smoke[num].StartFadeOut();
		}
	}

	public void CustomUpdate(float deltaTime, float time)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		m_time += deltaTime;
		if (m_time > m_ttl && m_fadeTimer < 0f)
		{
			StartFadeOut();
		}
		float num = 1f - Mathf.Clamp01(m_time / m_ttl);
		m_body.mass = num * num;
		Vector3 linearVelocity = m_body.linearVelocity;
		Vector3 vel = m_vel;
		vel.y *= num;
		Vector3 val = vel - linearVelocity;
		m_body.AddForce(val * (m_force * deltaTime), (ForceMode)2);
		if (m_fadeTimer >= 0f)
		{
			m_fadeTimer += deltaTime;
			Mathf.Clamp01(m_fadeTimer / m_fadetime);
			if (m_fadeTimer >= m_fadetime)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}
}
