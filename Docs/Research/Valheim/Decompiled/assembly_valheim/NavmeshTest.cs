using System.Collections.Generic;
using UnityEngine;

public class NavmeshTest : MonoBehaviour
{
	public Transform m_target;

	public Pathfinding.AgentType m_agentType = Pathfinding.AgentType.Humanoid;

	public bool m_cleanPath = true;

	private List<Vector3> m_path = new List<Vector3>();

	private bool m_havePath;

	private void Awake()
	{
	}

	private void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Pathfinding.instance.GetPath(((Component)this).transform.position, m_target.position, m_path, m_agentType, requireFullPath: false, m_cleanPath))
		{
			m_havePath = true;
		}
		else
		{
			m_havePath = false;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_target == (Object)null)
		{
			return;
		}
		if (m_havePath)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < m_path.Count - 1; i++)
			{
				Vector3 val = m_path[i];
				Vector3 val2 = m_path[i + 1];
				Gizmos.DrawLine(val + Vector3.up * 0.2f, val2 + Vector3.up * 0.2f);
			}
			foreach (Vector3 item in m_path)
			{
				Gizmos.DrawSphere(item + Vector3.up * 0.2f, 0.1f);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(((Component)this).transform.position, 0.3f);
			Gizmos.DrawSphere(m_target.position, 0.3f);
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(((Component)this).transform.position + Vector3.up * 0.2f, m_target.position + Vector3.up * 0.2f);
			Gizmos.DrawSphere(((Component)this).transform.position, 0.3f);
			Gizmos.DrawSphere(m_target.position, 0.3f);
		}
	}
}
