using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

[ExecuteAlways]
public class VortexParticles : MonoBehaviour
{
	private struct VortexParticlesJob : IJobParticleSystemParallelFor
	{
		[ReadOnly]
		public Vector3 vortexCenter;

		[ReadOnly]
		public float pullStrength;

		[ReadOnly]
		public Vector3 upDir;

		[ReadOnly]
		public float vortexStrength;

		[ReadOnly]
		public bool lineAttraction;

		[ReadOnly]
		public bool useCustomData;

		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public bool distanceStrengthFalloff;

		public void Execute(ParticleSystemJobData particles, int i)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = new Vector3(((ParticleSystemJobData)(ref particles)).velocities.x[i], ((ParticleSystemJobData)(ref particles)).velocities.y[i], ((ParticleSystemJobData)(ref particles)).velocities.z[i]);
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(((ParticleSystemJobData)(ref particles)).positions.x[i], ((ParticleSystemJobData)(ref particles)).positions.y[i], ((ParticleSystemJobData)(ref particles)).positions.z[i]);
			Vector3 val3 = vortexCenter;
			float num = (useCustomData ? ((ParticleSystemJobData)(ref particles)).customData1.x[i] : vortexStrength);
			if (lineAttraction)
			{
				val3.y = val2.y;
			}
			Vector3 val4 = val3 - val2;
			if (distanceStrengthFalloff)
			{
				float num2 = Vector3.Magnitude(val4);
				num *= (0f - num2) / Mathf.Sqrt(num2);
			}
			val4 = Vector3.Normalize(val4);
			Vector3 val5 = Vector3.Cross(Vector3.Normalize(val4), upDir);
			Vector3 val6 = val + val4 * pullStrength * deltaTime;
			val6 += val5 * num * deltaTime;
			NativeArray<float> x = ((ParticleSystemJobData)(ref particles)).velocities.x;
			NativeArray<float> y = ((ParticleSystemJobData)(ref particles)).velocities.y;
			NativeArray<float> z = ((ParticleSystemJobData)(ref particles)).velocities.z;
			x[i] = val6.x;
			y[i] = val6.y;
			z[i] = val6.z;
		}
	}

	private ParticleSystem ps;

	private VortexParticlesJob job;

	[SerializeField]
	private bool effectOn = true;

	[SerializeField]
	private Vector3 centerOffset;

	[SerializeField]
	private float pullStrength;

	[SerializeField]
	private float vortexStrength;

	[SerializeField]
	private bool lineAttraction;

	[SerializeField]
	private bool useCustomData;

	[SerializeField]
	private bool distanceStrengthFalloff;

	private void Start()
	{
		ps = ((Component)this).GetComponent<ParticleSystem>();
		if ((Object)(object)ps == (Object)null)
		{
			ZLog.LogWarning((object)("VortexParticles object '" + ((Object)((Component)this).gameObject).name + "' is missing a particle system and disabled!"));
			effectOn = false;
		}
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		MainModule main = ps.main;
		if ((int)((MainModule)(ref main)).simulationSpace == 0)
		{
			job.vortexCenter = centerOffset;
			job.upDir = new Vector3(0f, 1f, 0f);
		}
		else
		{
			job.vortexCenter = ((Component)this).transform.position + centerOffset;
			job.upDir = ((Component)this).transform.up;
		}
		job.pullStrength = pullStrength;
		job.vortexStrength = vortexStrength;
		job.lineAttraction = lineAttraction;
		job.useCustomData = useCustomData;
		job.deltaTime = Time.deltaTime;
		job.distanceStrengthFalloff = distanceStrengthFalloff;
	}

	private void OnParticleUpdateJobScheduled()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ps == (Object)null)
		{
			ps = ((Component)this).GetComponent<ParticleSystem>();
			if ((Object)(object)ps == (Object)null)
			{
				ZLog.LogWarning((object)("VortexParticles object '" + ((Object)((Component)this).gameObject).name + "' is missing a particle system and disabled!"));
				effectOn = false;
			}
		}
		if (effectOn)
		{
			IParticleSystemJobExtensions.Schedule<VortexParticlesJob>(job, ps, 1024, default(JobHandle));
		}
	}
}
