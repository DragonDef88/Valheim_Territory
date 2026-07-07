using System.Collections.Generic;
using UnityEngine;

public class LineConnect : MonoBehaviour
{
	public bool m_centerOfCharacter;

	public string m_childObject = "";

	public bool m_hideIfNoConnection = true;

	public Vector3 m_noConnectionWorldOffset = new Vector3(0f, -1f, 0f);

	[Header("Dynamic slack")]
	public bool m_dynamicSlack;

	public float m_slack = 0.5f;

	[Header("Thickness")]
	public bool m_dynamicThickness = true;

	public float m_minDistance = 6f;

	public float m_maxDistance = 30f;

	public float m_minThickness = 0.2f;

	public float m_maxThickness = 0.8f;

	public float m_thicknessPower = 0.2f;

	public string m_netViewPrefix = "";

	private LineRenderer m_lineRenderer;

	private ZNetView m_nview;

	private KeyValuePair<int, int> m_linePeerID;

	private int m_slackHash;

	private void Awake()
	{
		m_lineRenderer = ((Component)this).GetComponent<LineRenderer>();
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		m_linePeerID = ZDO.GetHashZDOID(m_netViewPrefix + "line_peer");
		m_slackHash = StringExtensionMethods.GetStableHashCode(m_netViewPrefix + "line_slack");
	}

	private void LateUpdate()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			((Renderer)m_lineRenderer).enabled = false;
			return;
		}
		ZDOID zDOID = m_nview.GetZDO().GetZDOID(m_linePeerID);
		GameObject val = ZNetScene.instance.FindInstance(zDOID);
		if (Object.op_Implicit((Object)(object)val) && !string.IsNullOrEmpty(m_childObject))
		{
			Transform val2 = Utils.FindChild(val.transform, m_childObject, (IterativeSearchType)0);
			if (Object.op_Implicit((Object)(object)val2))
			{
				val = ((Component)val2).gameObject;
			}
		}
		if ((Object)(object)val != (Object)null)
		{
			Vector3 endpoint = val.transform.position;
			if (m_centerOfCharacter)
			{
				Character component = val.GetComponent<Character>();
				if (Object.op_Implicit((Object)(object)component))
				{
					endpoint = component.GetCenterPoint();
				}
			}
			SetEndpoint(endpoint);
			((Renderer)m_lineRenderer).enabled = true;
		}
		else if (m_hideIfNoConnection)
		{
			((Renderer)m_lineRenderer).enabled = false;
		}
		else
		{
			((Renderer)m_lineRenderer).enabled = true;
			SetEndpoint(((Component)this).transform.position + m_noConnectionWorldOffset);
		}
	}

	private void SetEndpoint(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(pos);
		Vector3 val2 = ((Component)this).transform.InverseTransformDirection(Vector3.down);
		if (m_dynamicSlack)
		{
			float @float = m_nview.GetZDO().GetFloat(m_slackHash, m_slack);
			Vector3 position = m_lineRenderer.GetPosition(0);
			Vector3 val3 = val;
			float num = Vector3.Distance(position, val3) / 2f;
			for (int i = 1; i < m_lineRenderer.positionCount; i++)
			{
				float num2 = (float)i / (float)(m_lineRenderer.positionCount - 1);
				float num3 = Mathf.Abs(0.5f - num2) * 2f;
				num3 *= num3;
				num3 = 1f - num3;
				Vector3 val4 = Vector3.Lerp(position, val3, num2);
				val4 += val2 * num * @float * num3;
				m_lineRenderer.SetPosition(i, val4);
			}
		}
		else
		{
			m_lineRenderer.SetPosition(1, val);
		}
		if (m_dynamicThickness)
		{
			float num4 = Vector3.Distance(((Component)this).transform.position, pos);
			float num5 = Utils.LerpStep(m_minDistance, m_maxDistance, num4);
			num5 = Mathf.Pow(num5, m_thicknessPower);
			m_lineRenderer.widthMultiplier = Mathf.Lerp(m_maxThickness, m_minThickness, num5);
		}
	}

	public void SetPeer(ZNetView other)
	{
		if (Object.op_Implicit((Object)(object)other))
		{
			SetPeer(other.GetZDO().m_uid);
		}
		else
		{
			SetPeer(ZDOID.None);
		}
	}

	public void SetPeer(ZDOID zdoid)
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(m_linePeerID, zdoid);
		}
	}

	public void SetSlack(float slack)
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(m_slackHash, slack);
		}
	}
}
