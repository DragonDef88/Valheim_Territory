using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
	public enum Type
	{
		Player,
		Camera,
		Average
	}

	public Type m_follow = Type.Camera;

	public bool m_lockYPos;

	public float m_maxYPos = 1000000f;

	private void LateUpdate()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (!((Object)(object)Player.m_localPlayer == (Object)null) && !((Object)(object)mainCamera == (Object)null))
		{
			Vector3 zero = Vector3.zero;
			zero = ((m_follow == Type.Camera || GameCamera.InFreeFly()) ? ((Component)mainCamera).transform.position : ((m_follow != Type.Average) ? ((Component)Player.m_localPlayer).transform.position : ((!GameCamera.InFreeFly()) ? ((((Component)mainCamera).transform.position + ((Component)Player.m_localPlayer).transform.position) * 0.5f) : ((Component)mainCamera).transform.position)));
			if (m_lockYPos)
			{
				zero.y = ((Component)this).transform.position.y;
			}
			if (zero.y > m_maxYPos)
			{
				zero.y = m_maxYPos;
			}
			((Component)this).transform.position = zero;
		}
	}
}
