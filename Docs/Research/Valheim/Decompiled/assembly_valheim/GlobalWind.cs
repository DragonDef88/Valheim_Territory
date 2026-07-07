using UnityEngine;

public class GlobalWind : MonoBehaviour
{
	public float m_multiplier = 1f;

	public bool m_smoothUpdate;

	public bool m_alignToWindDirection;

	[Header("Particles")]
	public bool m_particleVelocity = true;

	public bool m_particleForce;

	public bool m_particleEmission;

	public int m_particleEmissionMin;

	public int m_particleEmissionMax = 1;

	[Header("Cloth")]
	public float m_clothRandomAccelerationFactor = 0.5f;

	public bool m_checkPlayerShelter;

	private ParticleSystem m_ps;

	private Cloth m_cloth;

	private Player m_player;

	private void Start()
	{
		if (!((Object)(object)EnvMan.instance == (Object)null))
		{
			m_ps = ((Component)this).GetComponent<ParticleSystem>();
			m_cloth = ((Component)this).GetComponent<Cloth>();
			if (m_checkPlayerShelter)
			{
				m_player = ((Component)this).GetComponentInParent<Player>();
			}
			if (m_smoothUpdate)
			{
				((MonoBehaviour)this).InvokeRepeating("UpdateWind", 0f, 0.01f);
				return;
			}
			((MonoBehaviour)this).InvokeRepeating("UpdateWind", Random.Range(1.5f, 2.5f), 2f);
			UpdateWind();
		}
	}

	private void UpdateWind()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (m_alignToWindDirection)
		{
			Vector3 windDir = EnvMan.instance.GetWindDir();
			((Component)this).transform.rotation = Quaternion.LookRotation(windDir, Vector3.up);
		}
		if (Object.op_Implicit((Object)(object)m_ps))
		{
			EmissionModule emission = m_ps.emission;
			if (!((EmissionModule)(ref emission)).enabled)
			{
				return;
			}
			Vector3 windForce = EnvMan.instance.GetWindForce();
			if (m_particleVelocity)
			{
				VelocityOverLifetimeModule velocityOverLifetime = m_ps.velocityOverLifetime;
				((VelocityOverLifetimeModule)(ref velocityOverLifetime)).space = (ParticleSystemSimulationSpace)1;
				((VelocityOverLifetimeModule)(ref velocityOverLifetime)).x = MinMaxCurve.op_Implicit(windForce.x * m_multiplier);
				((VelocityOverLifetimeModule)(ref velocityOverLifetime)).z = MinMaxCurve.op_Implicit(windForce.z * m_multiplier);
			}
			if (m_particleForce)
			{
				ForceOverLifetimeModule forceOverLifetime = m_ps.forceOverLifetime;
				((ForceOverLifetimeModule)(ref forceOverLifetime)).space = (ParticleSystemSimulationSpace)1;
				((ForceOverLifetimeModule)(ref forceOverLifetime)).x = MinMaxCurve.op_Implicit(windForce.x * m_multiplier);
				((ForceOverLifetimeModule)(ref forceOverLifetime)).z = MinMaxCurve.op_Implicit(windForce.z * m_multiplier);
			}
			if (m_particleEmission)
			{
				EmissionModule emission2 = m_ps.emission;
				((EmissionModule)(ref emission2)).rateOverTimeMultiplier = Mathf.Lerp((float)m_particleEmissionMin, (float)m_particleEmissionMax, EnvMan.instance.GetWindIntensity());
			}
		}
		if (Object.op_Implicit((Object)(object)m_cloth))
		{
			Vector3 val = EnvMan.instance.GetWindForce();
			if (m_checkPlayerShelter && (Object)(object)m_player != (Object)null && m_player.InShelter())
			{
				val = Vector3.zero;
			}
			m_cloth.externalAcceleration = val * m_multiplier;
			m_cloth.randomAcceleration = val * m_multiplier * m_clothRandomAccelerationFactor;
		}
	}

	public void UpdateClothReference(Cloth cloth)
	{
		m_cloth = cloth;
	}
}
