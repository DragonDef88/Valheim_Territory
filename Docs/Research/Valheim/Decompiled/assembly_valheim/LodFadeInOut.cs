using UnityEngine;

public class LodFadeInOut : MonoBehaviour
{
	private Vector3 m_originalLocalRef;

	private LODGroup m_lodGroup;

	private const float m_minTriggerDistance = 20f;

	private void Awake()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (!((Object)(object)mainCamera == (Object)null) && Vector3.Distance(((Component)mainCamera).transform.position, ((Component)this).transform.position) > 20f)
		{
			m_lodGroup = ((Component)this).GetComponent<LODGroup>();
			if (Object.op_Implicit((Object)(object)m_lodGroup))
			{
				m_originalLocalRef = m_lodGroup.localReferencePoint;
				m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
				((MonoBehaviour)this).Invoke("FadeIn", Random.Range(0.1f, 0.3f));
			}
		}
	}

	private void FadeIn()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		m_lodGroup.localReferencePoint = m_originalLocalRef;
	}
}
