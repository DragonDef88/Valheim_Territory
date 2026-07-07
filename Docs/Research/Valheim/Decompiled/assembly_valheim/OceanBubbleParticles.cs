using UnityEngine;

public class OceanBubbleParticles : MonoBehaviour
{
	private ParticleSystem m_particleSystem;

	private Particle[] m_particles;

	private void Start()
	{
		m_particleSystem = ((Component)this).GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (m_particles == null)
		{
			MainModule main = m_particleSystem.main;
			m_particles = (Particle[])(object)new Particle[((MainModule)(ref main)).maxParticles];
		}
		int particles = m_particleSystem.GetParticles(m_particles);
		for (int i = 0; i < particles; i++)
		{
			float liquidLevel = Floating.GetLiquidLevel(((Particle)(ref m_particles[i])).position);
			Vector3 position = ((Particle)(ref m_particles[i])).position;
			position.y = liquidLevel;
			((Particle)(ref m_particles[i])).position = position;
		}
		m_particleSystem.SetParticles(m_particles);
	}
}
