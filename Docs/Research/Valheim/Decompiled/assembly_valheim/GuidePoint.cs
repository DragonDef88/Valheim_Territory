using UnityEngine;

public class GuidePoint : MonoBehaviour
{
	public Raven.RavenText m_text = new Raven.RavenText();

	public GameObject m_ravenPrefab;

	private void Start()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!Raven.IsInstantiated())
		{
			Object.Instantiate<GameObject>(m_ravenPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		}
		m_text.m_static = true;
		m_text.m_guidePoint = this;
		Raven.RegisterStaticText(m_text);
	}

	private void OnDestroy()
	{
		Raven.UnregisterStaticText(m_text);
	}

	private void OnDrawGizmos()
	{
	}
}
