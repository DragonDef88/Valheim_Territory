using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InstanceRenderer : MonoBehaviour, IMonoUpdater
{
	public Mesh m_mesh;

	public Material m_material;

	public Vector3 m_scale = Vector3.one;

	public bool m_frustumCull = true;

	public bool m_useLod;

	public bool m_useXZLodDistance = true;

	public float m_lodMinDistance = 5f;

	public float m_lodMaxDistance = 20f;

	public ShadowCastingMode m_shadowCasting;

	private bool m_dirtyBounds = true;

	private BoundingSphere m_bounds;

	private float m_lodCount;

	private Matrix4x4[] m_instances = (Matrix4x4[])(object)new Matrix4x4[1024];

	private int m_instanceCount;

	private bool m_firstFrame = true;

	private static int s_layer = -1;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void OnEnable()
	{
		if (s_layer == -1)
		{
			s_layer = LayerMask.NameToLayer("InstanceRenderer");
		}
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void CustomUpdate(float deltaTime, float time)
	{
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (m_instanceCount == 0 || (Object)(object)mainCamera == (Object)null)
		{
			return;
		}
		if (m_frustumCull)
		{
			if (m_dirtyBounds)
			{
				UpdateBounds();
			}
			if (!Utils.InsideMainCamera(m_bounds))
			{
				return;
			}
		}
		if (m_useLod)
		{
			float num = (m_useXZLodDistance ? Utils.DistanceXZ(((Component)mainCamera).transform.position, ((Component)this).transform.position) : Vector3.Distance(((Component)mainCamera).transform.position, ((Component)this).transform.position));
			int num2 = (int)((1f - Utils.LerpStep(m_lodMinDistance, m_lodMaxDistance, num)) * (float)m_instanceCount);
			float num3 = deltaTime * (float)m_instanceCount;
			m_lodCount = Mathf.MoveTowards(m_lodCount, (float)num2, num3);
			if (m_firstFrame)
			{
				if (num < m_lodMinDistance)
				{
					m_lodCount = num2;
				}
				m_firstFrame = false;
			}
			m_lodCount = Mathf.Min(m_lodCount, (float)m_instanceCount);
			int num4 = (int)m_lodCount;
			if (num4 > 0)
			{
				Graphics.DrawMeshInstanced(m_mesh, 0, m_material, m_instances, num4, (MaterialPropertyBlock)null, m_shadowCasting, true, s_layer);
			}
		}
		else
		{
			Graphics.DrawMeshInstanced(m_mesh, 0, m_material, m_instances, m_instanceCount, (MaterialPropertyBlock)null, m_shadowCasting, true, s_layer);
		}
	}

	private void UpdateBounds()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		m_dirtyBounds = false;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(9999999f, 9999999f, 9999999f);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(-9999999f, -9999999f, -9999999f);
		Bounds bounds = m_mesh.bounds;
		Vector3 extents = ((Bounds)(ref bounds)).extents;
		float magnitude = ((Vector3)(ref extents)).magnitude;
		Vector3 val4 = default(Vector3);
		Vector3 val5 = default(Vector3);
		for (int i = 0; i < m_instanceCount; i++)
		{
			Matrix4x4 val3 = m_instances[i];
			((Vector3)(ref val4))._002Ector(((Matrix4x4)(ref val3))[0, 3], ((Matrix4x4)(ref val3))[1, 3], ((Matrix4x4)(ref val3))[2, 3]);
			Vector3 lossyScale = ((Matrix4x4)(ref val3)).lossyScale;
			float num = Mathf.Max(Mathf.Max(lossyScale.x, lossyScale.y), lossyScale.z);
			((Vector3)(ref val5))._002Ector(num * magnitude, num * magnitude, num * magnitude);
			val2 = Vector3.Max(val2, val4 + val5);
			val = Vector3.Min(val, val4 - val5);
		}
		m_bounds.position = (val2 + val) * 0.5f;
		m_bounds.radius = Vector3.Distance(val2, m_bounds.position);
	}

	public void AddInstance(Vector3 pos, Quaternion rot, float scale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 m = Matrix4x4.TRS(pos, rot, m_scale * scale);
		AddInstance(m);
	}

	public void AddInstance(Vector3 pos, Quaternion rot)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 m = Matrix4x4.TRS(pos, rot, m_scale);
		AddInstance(m);
	}

	public void AddInstance(Matrix4x4 m)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (m_instanceCount < 1023)
		{
			m_instances[m_instanceCount] = m;
			m_instanceCount++;
			m_dirtyBounds = true;
		}
	}

	public void Clear()
	{
		m_instanceCount = 0;
		m_dirtyBounds = true;
	}

	public void SetInstance(int index, Vector3 pos, Quaternion rot, float scale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 val = Matrix4x4.TRS(pos, rot, m_scale * scale);
		m_instances[index] = val;
		m_dirtyBounds = true;
	}

	private void Resize(int instances)
	{
		m_instanceCount = instances;
		m_dirtyBounds = true;
	}

	public void SetInstances(List<Transform> transforms, bool faceCamera = false)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Resize(transforms.Count);
		for (int i = 0; i < transforms.Count; i++)
		{
			Transform val = transforms[i];
			m_instances[i] = Matrix4x4.TRS(val.position, val.rotation, val.lossyScale);
		}
		m_dirtyBounds = true;
	}

	public void SetInstancesBillboard(List<Vector4> points)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (!((Object)(object)mainCamera == (Object)null))
		{
			Vector3 val = -((Component)mainCamera).transform.forward;
			Resize(points.Count);
			Vector3 val3 = default(Vector3);
			for (int i = 0; i < points.Count; i++)
			{
				Vector4 val2 = points[i];
				((Vector3)(ref val3))._002Ector(val2.x, val2.y, val2.z);
				float w = val2.w;
				Quaternion val4 = Quaternion.LookRotation(val);
				m_instances[i] = Matrix4x4.TRS(val3, val4, w * m_scale);
			}
			m_dirtyBounds = true;
		}
	}

	private void OnDrawGizmosSelected()
	{
	}
}
