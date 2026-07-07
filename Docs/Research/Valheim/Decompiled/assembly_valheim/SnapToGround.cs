using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapToGround : MonoBehaviour
{
	public float m_offset;

	private static List<SnapToGround> m_allSnappers = new List<SnapToGround>();

	private bool m_inList;

	private void Awake()
	{
		m_allSnappers.Add(this);
		m_inList = true;
	}

	private void OnDestroy()
	{
		if (m_inList)
		{
			m_allSnappers.Remove(this);
			m_inList = false;
		}
	}

	public void Snap()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ZoneSystem.instance == (Object)null))
		{
			float groundHeight = ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position);
			Vector3 position = ((Component)this).transform.position;
			position.y = groundHeight + m_offset;
			((Component)this).transform.position = position;
			ZNetView component = ((Component)this).GetComponent<ZNetView>();
			if ((Object)(object)component != (Object)null && component.IsOwner())
			{
				component.GetZDO().SetPosition(position);
			}
		}
	}

	public bool HaveUnsnapped()
	{
		return m_allSnappers.Count > 0;
	}

	public static void SnappAll()
	{
		if (m_allSnappers.Count == 0)
		{
			return;
		}
		Heightmap.ForceGenerateAll();
		foreach (SnapToGround allSnapper in m_allSnappers)
		{
			allSnapper.Snap();
			allSnapper.m_inList = false;
		}
		m_allSnappers.Clear();
	}
}
