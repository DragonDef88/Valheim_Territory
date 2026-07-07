using UnityEngine;

public class Billboard : MonoBehaviour
{
	public bool m_vertical = true;

	public bool m_invert;

	private Vector3 m_normal;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_normal = ((Component)this).transform.up;
	}

	private void LateUpdate()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (!((Object)(object)mainCamera == (Object)null))
		{
			Vector3 val = ((Component)mainCamera).transform.position;
			if (m_invert)
			{
				val = ((Component)this).transform.position - (val - ((Component)this).transform.position);
			}
			if (m_vertical)
			{
				val.y = ((Component)this).transform.position.y;
				((Component)this).transform.LookAt(val, m_normal);
			}
			else
			{
				((Component)this).transform.LookAt(val);
			}
		}
	}
}
