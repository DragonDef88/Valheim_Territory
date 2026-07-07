using System;
using UnityEngine;

public class ShieldDomeParticleColor : MonoBehaviour
{
	[Serializable]
	public enum ColorMode
	{
		ClosestShieldWall,
		ClosestShieldGenerator
	}

	public ColorMode m_colorMode;

	public ParticleSystem[] m_particleSystems;

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Color domeColor = ShieldDomeImageEffect.GetDomeColor(ShieldGenerator.GetClosestShieldGenerator(((Component)this).transform.position, m_colorMode == ColorMode.ClosestShieldGenerator).GetFuelRatio());
		ParticleSystem[] particleSystems = m_particleSystems;
		foreach (ParticleSystem obj in particleSystems)
		{
			MainModule main = obj.main;
			MainModule main2 = obj.main;
			MinMaxGradient startColor = ((MainModule)(ref main2)).startColor;
			Color color = ((MinMaxGradient)(ref startColor)).color;
			domeColor.a = color.a;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(domeColor);
		}
	}
}
