using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ParticleDecal : MonoBehaviour
{
	public ParticleSystem m_decalSystem;

	[Range(0f, 100f)]
	public float m_chance = 100f;

	private ParticleSystem part;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private void Awake()
	{
		part = ((Component)this).GetComponent<ParticleSystem>();
		collisionEvents = new List<ParticleCollisionEvent>();
	}

	private void OnParticleCollision(GameObject other)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_chance < 100f) || !(Random.Range(0f, 100f) > m_chance))
		{
			int num = ParticlePhysicsExtensions.GetCollisionEvents(part, other, collisionEvents);
			for (int i = 0; i < num; i++)
			{
				ParticleCollisionEvent val = collisionEvents[i];
				Quaternion val2 = Quaternion.LookRotation(((ParticleCollisionEvent)(ref val)).normal);
				Vector3 eulerAngles = ((Quaternion)(ref val2)).eulerAngles;
				eulerAngles.x = 0f - eulerAngles.x + 180f;
				eulerAngles.y = 0f - eulerAngles.y;
				eulerAngles.z = Random.Range(0, 360);
				EmitParams val3 = default(EmitParams);
				((EmitParams)(ref val3)).position = ((ParticleCollisionEvent)(ref val)).intersection;
				((EmitParams)(ref val3)).rotation3D = eulerAngles;
				((EmitParams)(ref val3)).velocity = -((ParticleCollisionEvent)(ref val)).normal * 0.001f;
				m_decalSystem.Emit(val3, 1);
			}
		}
	}
}
