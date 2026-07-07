using UnityEngine;

public class StaticRotation : MonoBehaviour
{
	public bool m_NotIfBuildGhost = true;

	private ZNetView m_nview;

	private float m_rotation;

	private bool m_disabled;

	private void Start()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		int disabled;
		if (m_NotIfBuildGhost)
		{
			Piece componentInParent = ((Component)this).GetComponentInParent<Piece>();
			if (componentInParent != null)
			{
				disabled = (Player.IsPlacementGhost(((Component)componentInParent).gameObject) ? 1 : 0);
				goto IL_0021;
			}
		}
		disabled = 0;
		goto IL_0021;
		IL_0021:
		m_disabled = (byte)disabled != 0;
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		if (!Object.op_Implicit((Object)(object)m_nview))
		{
			return;
		}
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null && zDO.IsValid())
		{
			m_rotation = zDO.GetFloat(ZDOVars.s_tiltrot);
			if (m_rotation == 0f)
			{
				Quaternion rotation = ((Component)this).transform.rotation;
				m_rotation = ((Quaternion)(ref rotation)).eulerAngles.y;
				zDO.Set(ZDOVars.s_tiltrot, m_rotation);
			}
		}
	}

	private void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!m_disabled)
		{
			Quaternion rotation = ((Component)this).transform.rotation;
			Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
			((Component)this).transform.rotation = Quaternion.Euler(eulerAngles.x, m_rotation, eulerAngles.z);
		}
	}
}
