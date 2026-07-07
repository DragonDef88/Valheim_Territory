using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineAttach : MonoBehaviour, IMonoUpdater
{
	public List<Transform> m_attachments = new List<Transform>();

	private LineRenderer m_lineRenderer;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Start()
	{
		m_lineRenderer = ((Component)this).GetComponent<LineRenderer>();
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void CustomLateUpdate(float deltaTime)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_attachments.Count; i++)
		{
			Transform val = m_attachments[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				m_lineRenderer.SetPosition(i, ((Component)this).transform.InverseTransformPoint(val.position));
			}
		}
	}
}
